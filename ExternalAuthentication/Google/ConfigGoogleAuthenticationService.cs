using ExternalAuthentication.Store;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace ExternalAuthentication.Google;

public static class ConfigGoogleAuthenticationService
{
    public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder)
    {
        builder.AddOAuth("google", opt =>
        {
            opt.SignInScheme = "cookie";
            opt.ClientId = "745405021313-h6sj59gf0jh0u8gmsoaer0p2ie11ahpe.apps.googleusercontent.com";
            opt.ClientSecret = "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";
            opt.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
            opt.TokenEndpoint = "https://oauth2.googleapis.com/token";
            opt.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
            opt.CallbackPath = "/oauth2-callback";

            opt.Scope.Add("https://www.googleapis.com/auth/gmail.readonly");
            opt.Scope.Add("profile");
            opt.Scope.Add("email");

            opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            opt.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name");

            opt.Events.OnCreatingTicket = async context => 
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

                using var response = await context.Backchannel.SendAsync(request);

                var user = await response.Content.ReadFromJsonAsync<JsonElement>();

                context.RunClaimActions(user);

                var store = context.HttpContext.RequestServices.GetRequiredService<TokenStore>();

                var userId = user.GetString("id") ?? throw new Exception();

                await store.SaveToken(userId, context.AccessToken!);
            };
        });

        return builder;
    }
}
