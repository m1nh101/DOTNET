using Microsoft.AspNetCore.Mvc;

namespace ExternalAuthentication.Google;

public static class GoogleApiSetup
{
    public static WebApplication SetupGoogleApi(this WebApplication app)
    {
        app.MapGet("/google/auth", () => Results.Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = "https://localhost:5055/swagger",
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(6),
        },
            authenticationSchemes: new List<string>() { "google" }));

        app.MapGet("/google/email", async ([FromServices] IHttpClientFactory factory) =>
        {
            var client = factory.CreateClient("google-mail");

            var response = await client.GetAsync("/gmail/v1/users/me/messages");

            return await response.Content.ReadAsStringAsync();
        }).RequireAuthorization();

        return app;
    }
}
