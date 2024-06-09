using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RestaurantAPI.IntegrationTests
{
    public class RestaurantControllerTests
    {
        [Theory]
        [InlineData("pageSize=5&pageNumber=1")]
        [InlineData("pageSize=15&pageNumber=2")]
        [InlineData("pageSize=10&pageNumber=4")]

        public async Task GetAll_WithQueryParameters_ReturnsOkResult(string queryParams)
        { 
        //Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        //Act
        var response = await client.GetAsync("/api/restaurant?" + queryParams);
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
            var response = await client.GetAsync("/api/restaurant?" + queryParams);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
