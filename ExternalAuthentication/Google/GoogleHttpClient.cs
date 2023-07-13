using ExternalAuthentication.Store;
using System.Security.Claims;

namespace ExternalAuthentication.Google;

public static class GoogleHttpClient
{
    public static void AddGoogleMailHttpClient(this IServiceCollection service)
    {
        service.AddHttpClient("google", (provider, client) =>
        {
            var context = provider.GetRequiredService<IHttpContextAccessor>();
            var tokenStore = provider.GetRequiredService<TokenStore>();

            var userId = context.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)?.Replace("-", "") ?? throw new NullReferenceException();

            var token = tokenStore.GetToken(userId).Result;

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Token}");
            client.BaseAddress = new Uri("https://gmail.googleapis.com");
        });
    }
}