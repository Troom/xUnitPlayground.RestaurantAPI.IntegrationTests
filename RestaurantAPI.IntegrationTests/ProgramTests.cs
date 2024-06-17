using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RestaurantAPI.IntegrationTests
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly IEnumerable<Type> _controllerTypes;
        private readonly WebApplicationFactory<Program> _factory;
        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _controllerTypes = typeof(Program)
                .Assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(ControllerBase)));

            _factory = factory.WithWebHostBuilder(builder => {
                builder.ConfigureServices(services =>
                {
                    foreach (var item in _controllerTypes)
                    { 
                        services.AddScoped(item);
                    }
                });
            });
        }

        [Fact]
        public void ConfigureServices_ForControllers_RegisterAllDependencies()
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            foreach (var item in _controllerTypes)
            {
                var controller = scope.ServiceProvider.GetService(item);
                controller.Should().NotBeNull();
            }
        }
    }
}