### Configuration

#### 1. Add xUnit Project
* Create xUnitProject
* Add reference to your main project
#### 2. IntegrationTests Config
```csharp
<Project Sdk="Microsoft.NET.Sdk.Web">
```
#### 3. Intall Nuget Packages
* Moq
* FluentAssertions
* Microsoft.AspNetCore.Mvc.Testing

### Testing public method

#### Good to know
* WebApplicationFactory create inMemory httpClient to invoke some endpoints from  inMemory API. It set base url by default as imMemory webAPI. You can't call any enpoint from this API manually (e.g. from browser or postman), it's only avalible for testing.
* If you want to check data from API response you can invoke ` response.Content.ReadAsStringAsync()
* You can configure database connections for these types of tests, but it is usually better and faster to use mocks. Remember, in certain test cases, database configuration is required.

### Example
```csharp 
[Theory]
[InlineData("pageSize=5&pageNumber=1")]
[InlineData("pageSize=15&pageNumber=2")]
public async Task GetAll_WithQueryParameters_ReturnsOkResult(string queryParams)
{ 
//Arrange
var factory = new WebApplicationFactory<Program>();
var client = factory.CreateClient();
//Act
var response = await client.GetAsync("/api/restaurant?" + queryParams);
//Assert
response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
}```

#### Note  WebApplicationFactory required args parameter in WebApplication.CreateBuilder
In your program.cs your webApplication.Create builder should contain args parameter
```csharp
var builder = WebApplication.CreateBuilder(args);
```

### Shared test context
You can refactor the code by moving shared code into the constructor and then use it in all test cases. It's important because code is cleaner and tests are completed faster.
To share an object between all tests, specify shared object by `IClassFixture`, as in the example below

```csharp
public class RestaurantControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
private HttpClient _client;
public RestaurantControllerTests(WebApplicationFactory<Program> factory){
	_client = factory.CreateClient();
}
//Rest of implementation
}
```

### InMemory Database for tests
For avoiding connection to real database you can use InMemoryDbConext. This solution has some limitations, but is sufficient in many scenarios.

#### Requirements 
* Install nuget packages:
	* `Microsoft.EntityFrameworkCore.InMemory` 
	* `Microsoft.Extensions.DependecyInjection`
* Configure builder
```csharp
_client = factory
.WithWebHostBuilder(builder=>
{
 builder.ConfigureServices(services =>
{
	 var dbContextOptions = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
	 services.Remove(dbContextOptions); //Find and remove default DbContextOptions
	 services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb")); //Register inMemoryDbContext
 });
})
.CreateClient(); 
```

WithWebHostBuidler we can overwrite Configure services, and in this case you replace dbContext to InMemoryDbContext.


#### Warning
In .NET we can implement an automatic migration mechanism for the SQL database, which causes a conflict for the InMemory database:
`Relational-specific methods can only be used when the context is using a relational database provider.`
To fix this, just add one check whether the database is relational:
```csharp
if (_dbContext.Database.IsRelational()) {
	var pendingMigrations = _dbContext.Database.GetPendingMigrations();
	if (pendingMigrations != null && pendingMigrations.Any())
	{
		_dbContext.Database.Migrate();
	}
}
```

#### Remember
Don't use InMemory database when you want to:
* Test more dificult queries (e.g. with transactions)
* Use raw SQL queries
* Measure time of queries.

But in many simple scenarios, this is good enough solution.

### Testing a query that requires authorization

You can do it in three ways
* Create user in every unit test, log in (for JWT token),  and put that token for every request.
* Create own (fake) startup class without authentification, and in proper way inject user context into `HttpContext`
* Workaround authorize by using a fake policy evaluator. So you can create your own class in which we indicate that we want to authorize each request to the application and then inject it into the service configuration.
---


Workaround authorize by using a fake policy evaluator
```csharp
public class FakePolicyEvaluator : IPolicyEvaluator
{
public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
{
var ticket = new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(), "testScheme");
var result =AuthenticateResult.Success(ticket);
return Task.FromResult(result);
}

public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
{
var result = PolicyAuthorizationResult.Success();
return Task.FromResult(result);
}
}
```

Add fake service to test WebHostBuilder (.NET dependency injection override service with the same interface name)
```csharp
services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
```

It's enought to workaround Authorize attribute, but we also need to implement some claims (e.g. NameIdentifier) To do that we should implement reusable class for our tests

```csharp
public static class TestingClaimsPrincipal
{
	public static ClaimsPrincipal GetClaimsPrincipal()
	{
		var claimsPrincipal = new ClaimsPrincipal();
		claimsPrincipal.AddIdentity(new ClaimsIdentity(
			new[] {
			new Claim(ClaimTypes.NameIdentifier, "1"),
			new Claim(ClaimTypes.Role, "Admin")}
			));
		return claimsPrincipal;
	}
}
```

and use it in our FakePolicyEvaliator
```csharp 
public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
{
	var claimsPrincipal = TestingClaimsPrincipal.GetClaimsPrincipal();
	var ticket = new AuthenticationTicket(claimsPrincipal, "testScheme");
	var result = AuthenticateResult.Success(ticket);
	return Task.FromResult(result);
}
```

You should then create a filter that will inject our claims into the request  pipeline
```csharp 
public class FakeUserFilter : IAsyncActionFilter
{
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
	  var claimsPrincipal = TestingClaimsPrincipal.GetClaimsPrincipal();
	  context.HttpContext.User = claimsPrincipal;
	  await next();
  }
}
```

and add it to WebHostBuilder
```csharp
services.AddMvc(opt => opt.Filters.Add<FakeUserFilter>());
```

#### Summary
1. To test method with Authorize attribute you have to implement workaround e.g `FakePolicyEvaluator`
2. If you need mock of claims of any user you should use `FakeUserFilter` and register it properly in WebHostBuilder

---


Feel free to make some refactors. For example we have to convert objects into json, so we can create extension method, like this:
```csharp
public static class HttpContentHelper
{
	public static  HttpContent ToJsonHttpContent(this object obj)
	{
		var json = JsonConvert.SerializeObject(obj);
		return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
	}
}
```

and example usage
```csharp
   public async Task CreateRestaurant_WithWalidModel_ReturnsCreatedStatus()
   {
       //Arrange
       var model = new CreateRestaurantDto() { Name = "A", City ="B", Street="C"};
       var httpContent = model.ToJsonHttpContent();
       //Act
       var response = await _client.PostAsync("/api/restaurant", httpContent);
       //Assert
       response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
       response.Headers.Location.Should().NotBeNull();
   }
```
### Seed data tests
For some test cases we will need to prepare a test database. A great example is the DELETE method -> you cannot delete data that did not exist before.
#### Seed database in test project

In the current test project, we use EntityFramework, so we need to obtain the database context during test execution. For this purpose, we can assign the `WebApplicationFactory<Program>` (in this project it is injected into the constructor) to a private variable so that it can be used and reused many times. With the help of this factory, you will be able to refer to the collection of services and then use the `GetService()` method to extract any service.

To assign the factory correctly, we assign the RESULT of the WithWebHostBuilder call and then reassign the client. This is very important because directly assigning the factory will mean that the factory will not know the WebHostBuilder configuration. Below is an example in the code

```csharp
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
```

#### Usage
Due to the fact that we use EntityFramework to extract the DbContext, we need to create a scope within which the DbContext will be created. For example:

```csharp
public void SeedRestaurant(Restaurant restaurant) { //SeedRestaurant can be reused
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
     var restaurant = new Restaurant(){/*SomeDetails*/};
     SeedRestaurant(restaurant);
     //act
     var response = await _client.DeleteAsync($"/api/restaurant/{restaurant.Id}");
     //assert
     response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
 }
```


### Other
#### SourceCode RestaurantAPI

RestaurantAPI was implemeneted by J.Kozera in version .NET5. This code was updated to .NET8 and the main goal of this repo are integration tests.
