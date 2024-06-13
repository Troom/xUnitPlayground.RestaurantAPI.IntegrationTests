using Microsoft.AspNetCore.Mvc.Filters;

namespace RestaurantAPI.IntegrationTests.Auth
{
    public class FakeUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var claimsPrincipal = TestingClaimsPrincipal.GetClaimsPrincipal();
            context.HttpContext.User = claimsPrincipal;
            await next();
        }
    }
}
