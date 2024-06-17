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
    public class DishControllerDish : IClassFixture<WebApplicationFactory<Program>>
    {
        private HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public DishControllerDish(WebApplicationFactory<Program> factory)
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
        [InlineData("1")]
        public async Task GetAll_ForExistingResturant_ReturnsOK(string restaurantId)
        {
            var response = await _client.GetAsync($"/api/restaurant/{restaurantId}/dish");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        [Theory]
        [InlineData("1","1")]
        public async Task Get_ForExistingResturantAndDish_ReturnsOK(string restaurantId, string dishId)
        {
            var response = await _client.GetAsync($"/api/restaurant/{restaurantId}/dish/{dishId}");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        [Theory]
        [InlineData("1")]

        public async Task CreateDish_WithWalidModel_ReturnsCreatedStatus(string restaurantId)
        {
            //Arrange
            var model = new CreateDishDto() { Name = "Rotten meat", Description = "B", Price = 19.99m, RestaurantId = 1 };
            var httpContent = model.ToJsonHttpContent();
            //Act
            var response = await _client.PostAsync($"/api/restaurant/{restaurantId}/dish", httpContent);
            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }
        public void SeedDish(Dish dish)
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<RestaurantDbContext>();

            _dbContext.Dishes.Add(dish);
            _dbContext.SaveChanges();
        }


        [Fact]
        public async Task Delete_ForRestaurantOwner_ReturnsNoContent()
        {
            //arrange
            var dish = new Dish
            {
                Name = "Rotten meat",
                Description = "B",
                Price = 19.99m,
                RestaurantId = 1,
                
            };
            SeedDish(dish);
            //act
            var response = await _client.DeleteAsync($"/api/restaurant/1/dish/{dish.Id}");
            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

    }
}

//[Route("api/restaurant/{restaurantId}/dish")]
//[ApiController]
//public class DishController : ControllerBase
//{
//    private readonly IDishService _dishService;

//    public DishController(IDishService dishService)
//    {
//        _dishService = dishService;
//    }

//    [HttpDelete]
//    public ActionResult Delete([FromRoute] int restaurantId)
//    {
//        _dishService.RemoveAll(restaurantId);

//        return NoContent();
//    }

//    [HttpPost]
//    public ActionResult Post([FromRoute] int restaurantId, [FromBody] CreateDishDto dto)
//    {
//        var newDishId = _dishService.Create(restaurantId, dto);

//        return Created($"api/restaurant/{restaurantId}/dish/{newDishId}", null);
//    }

//    [HttpGet("{dishId}")]
//    public ActionResult<DishDto> Get([FromRoute] int restaurantId, [FromRoute] int dishId)
//    {
//        DishDto dish = _dishService.GetById(restaurantId, dishId);
//        return Ok(dish);
//    }

//    [HttpGet]
//    public ActionResult<List<DishDto>> Get([FromRoute] int restaurantId)
//    {
//        var result = _dishService.GetAll(restaurantId);
//        return Ok(result);
//    }