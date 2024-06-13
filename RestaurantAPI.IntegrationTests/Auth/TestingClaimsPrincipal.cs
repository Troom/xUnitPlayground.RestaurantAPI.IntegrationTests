using System.Security.Claims;

namespace RestaurantAPI.IntegrationTests.Auth
{
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
}
