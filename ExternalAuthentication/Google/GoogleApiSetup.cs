using ExternalAuthentication.Store;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using System.Web;
using System.Text;

namespace ExternalAuthentication.Google;

public static class GoogleApiSetup
{
    public static WebApplication SetupGoogleApi(this WebApplication app)
    {
        app.MapGet("/google/auth", () =>
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = "https://localhost:5055/swagger",
                AllowRefresh = true
            };

            return Results.Challenge(authenticationProperties,
            authenticationSchemes: new List<string>() { "google" });
        });

        app.MapPost("/google/token/refresh", async ([FromServices] TokenStore store,
            [FromServices] IHttpContextAccessor http) =>
        {
            var userId = http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Results.Unauthorized();

            var userToken = await store.GetToken(userId);

            using var client = new HttpClient()
            {
                BaseAddress = new Uri("https://oauth2.googleapis.com/token"),
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"client_id", "745405021313-h6sj59gf0jh0u8gmsoaer0p2ie11ahpe.apps.googleusercontent.com" },
                {"client_secret", "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" },
                {"refresh_token", userToken.RefreshToken! },
                {"grant_type", "refresh_token" }
            });

            //var parameters = new Dictionary<string, string?>
            //{
            //    {"client_id", "745405021313-h6sj59gf0jh0u8gmsoaer0p2ie11ahpe.apps.googleusercontent.com" },
            //    {"client_secret", "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" },
            //    {"refresh_token", userToken.RefreshToken! },
            //    {"grant_type", "refresh_token" }
            //};

            var response = await client.SendAsync(request);

            var content = await response.Content.ReadFromJsonAsync<JsonElement>();

            var access_token = content.GetString("access_token") ?? throw new Exception();

            await store.SaveToken(userId, access_token, userToken.RefreshToken!);

            return Results.NoContent();
        });

        app.MapDelete("/google/revoke", async ([FromServices] IHttpClientFactory factory,
            [FromServices] TokenStore store,
            [FromServices] IHttpContextAccessor http) =>
        {
            var client = factory.CreateClient("google");

            client.BaseAddress = new Uri("https://oauth2.googleapis.com");

            var userId = http.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier)
                ?.Replace("-", "")
                ?? throw new NullReferenceException();

            var token = await store.GetToken(userId);

            await client.PostAsync($"/revoke?token={token}", default);
        }).RequireAuthorization();

        app.MapGet("/google/email", async ([FromServices] IHttpClientFactory factory) =>
        {
            var client = factory.CreateClient("google");

            client.BaseAddress = new Uri("https://gmail.googleapis.com");

            var response = await client.GetAsync("/gmail/v1/users/me/messages");

            return await response.Content.ReadAsStringAsync();
        })
            .RequireAuthorization()
            .WithGroupName("google mail api");

        return app;
    }
}
