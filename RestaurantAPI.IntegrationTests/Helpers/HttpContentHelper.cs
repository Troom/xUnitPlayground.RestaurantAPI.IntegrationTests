using Newtonsoft.Json;

namespace RestaurantAPI.IntegrationTests.Helpers
{
    public static class HttpContentHelper
    {
        public static  HttpContent ToJsonHttpContent(this object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }
    }
}
