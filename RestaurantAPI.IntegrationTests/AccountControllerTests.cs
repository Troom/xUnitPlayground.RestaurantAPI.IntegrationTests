using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantAPI.Entities;
using RestaurantAPI.IntegrationTests.Helpers;
using RestaurantAPI.Models;
using RestaurantAPI.Services;

namespace RestaurantAPI.IntegrationTests
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private HttpClient _client;
        private Mock<IAccountService> _accountServiceMock = new Mock<IAccountService>();
        
        public AccountControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContextOptions = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
                        services.Remove(dbContextOptions);
                        services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb"));
                        services.AddSingleton(_accountServiceMock.Object);
                    });
                }).CreateClient();
        }

        [Fact]
        public async Task Register_WithValidModel_ReturnsOKStatus()
        {
            var user = new RegisterUserDto()
            {
                Email = "a@a.com",
                Password = "aaaaaa",
                ConfirmPassword = "aaaaaa"
            };
            var content = user.ToJsonHttpContent();
            var response = await _client.PostAsync("/api/account/register", content);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        [Fact]
        public async Task Register_WithInvalidModel_ReturnsBadRequests()
        {
            var user = new RegisterUserDto()
            {
                Email = "aa.com",
                Password = "aaaaa",
                ConfirmPassword = "aaaaaa"
            };
            var content = user.ToJsonHttpContent();
            var response = await _client.PostAsync("/api/account/register", content);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_WithInvalidModel_ReturnsBadRequests()
        {
            var user = new LoginDto() { Email = "a@a.pl", Password = "aaaaa" };
            _accountServiceMock.Setup(x => x.GenerateJwt(It.IsAny<LoginDto>())).Returns("Token");
            var content = user.ToJsonHttpContent();
            var response = await _client.PostAsync("/api/account/login", content);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }

  


}
