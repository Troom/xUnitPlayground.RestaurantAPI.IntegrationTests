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
}
```

#### Note  WebApplicationFactory required args parameter in WebApplication.CreateBuilder
In your program.cs your webApplication.Create builder should contain args parameter

```csharp
var builder = WebApplication.CreateBuilder(args);
```

### Other
#### SourceCode RestaurantAPI

RestaurantAPI was implemeneted by J.Kozera in version .NET5. This code was updated to .NET8 and the main goal of this repo are integration tests.