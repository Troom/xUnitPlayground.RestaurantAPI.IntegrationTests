using FluentAssertions;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.IntegrationTests.Auth;
using RestaurantAPI.IntegrationTests.Helpers;
using RestaurantAPI.Models;

namespace RestaurantAPI.IntegrationTests
{
    public class RestaurantControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public RestaurantControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContextOptions = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
                        services.Remove(dbContextOptions); //Find and remove default DbContextOptions

                        services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb"));

                        services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                        services.AddMvc(opt => opt.Filters.Add<FakeUserFilter>());
                    });

                });
            _client = _factory.CreateClient();
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
            var model = new CreateRestaurantDto() { Name = "A", City = "B", Street = "C" };

            var httpContent = model.ToJsonHttpContent();

            //Act
            var response = await _client.PostAsync("/api/restaurant", httpContent);
            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }


        [Fact]
        public async Task CreateRestaurant_WithInValidModel_ReturnsBadRequest()
        {
            //Arrange
            var model = new CreateRestaurantDto() { ContactEmail = "Email", City = "B", Street = "C" };

            var httpContent = model.ToJsonHttpContent();
            //Act
            var response = await _client.PostAsync("/api/restaurant", httpContent);
            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_ForNonExistingRestaurant_ReturnsNotFound()
        { 
        //act
        var response = await _client.DeleteAsync("/api/restaurant/32110");
        //assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        public void SeedRestaurant(Restaurant restaurant) {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<RestaurantDbContext>();

            _dbContext.Restaurants.Add(restaurant);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task Delete_ForRestaurantOwner_ReturnsNoContent()
        {
            //arrange
            var restaurant = new Restaurant
            {
                CreatedById = 1,
                Name = "Test",
                ContactNumber = "Test",
                Category = "Test",
                Address = new Address
                {
                    City = "Test",
                    Street = "Test",
                }
            };
            SeedRestaurant(restaurant);
            //act
            var response = await _client.DeleteAsync($"/api/restaurant/{restaurant.Id}");
            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }
    }
}
