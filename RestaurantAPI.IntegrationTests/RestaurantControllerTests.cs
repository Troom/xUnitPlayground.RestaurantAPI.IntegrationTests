using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using RestaurantAPI.Entities;
using RestaurantAPI.IntegrationTests.Auth;
using RestaurantAPI.Models;
using System.Text;
using System.Text.Json;

namespace RestaurantAPI.IntegrationTests
{
    public class RestaurantControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private HttpClient _client;
        
        public RestaurantControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory
                .WithWebHostBuilder(builder=>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContextOptions = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
                        services.Remove(dbContextOptions); //Find and remove default DbContextOptions

                        services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb"));

                        services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                        services.AddMvc(opt => opt.Filters.Add<FakeUserFilter>());
                    });

                })
                .CreateClient();
        }

        [Theory]
        [InlineData("pageSize=5&pageNumber=1")]
        [InlineData("pageSize=15&pageNumber=2")]
        [InlineData("pageSize=10&pageNumber=4")]

        public async Task GetAll_WithQueryParameters_ReturnsOkResult(string queryParams)
        { 
        //Act
        var response = await _client.GetAsync("/api/restaurant?" + queryParams);
            //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        [Theory]
        [InlineData("pageSize=100&pageNumber=3")]
        [InlineData("pageSize=11&pageNumber=2")]
        [InlineData("null")]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetAll_WithInvalidQueryParameters_ReturnsBadResult(string queryParams)
        {
            //Arrange
            var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();
            //Act
            var response = await _client.GetAsync("/api/restaurant?" + queryParams);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
        [Fact]

        public async Task CreateRestaurant_WithWalidModel_ReturnsCreatedStatus()
        {
            //Arrange
            var model = new CreateRestaurantDto() { Name = "A", City ="B", Street="C"};
            var httpContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            
            //Act
            var response = await _client.PostAsync("/api/restaurant", httpContent);
            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }


    }
}
