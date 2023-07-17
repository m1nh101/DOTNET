using ExternalAuthentication.Store;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace ExternalAuthentication.Google;

public static class ConfigGoogleAuthenticationService
{
    public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        var option = configuration.GetSection("Oauth:Google").Get<GoogleOption>() ?? throw new Exception();

        builder.AddOAuth(GoogleOption.Google, opt =>
        {
            opt.SignInScheme = "cookie";
            opt.ClientId = option.ClientId;
            opt.ClientSecret = option.ClientSecret;
            opt.AuthorizationEndpoint = option.AuthorizationEndPoint;
            opt.TokenEndpoint = option.TokenEndPoint;
            opt.UserInformationEndpoint = option.UserInfoEndPoint;
            opt.CallbackPath = option.CallbackPath;

            foreach(var scope in option.Scopes)
                opt.Scope.Add(scope);

            opt.Scope.Add("https://www.googleapis.com/auth/gmail.readonly");

            opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            opt.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name");

            //save access_token/refresh token in http cookie
            //opt.SaveTokens = true;

            opt.Events.OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

                using var response = await context.Backchannel.SendAsync(request);

                var user = await response.Content.ReadFromJsonAsync<JsonElement>();

                context.RunClaimActions(user);

                var store = context.HttpContext.RequestServices.GetRequiredService<TokenStore>();

                var userId = user.GetString("id") ?? throw new Exception();

                await store.SaveToken(userId, context.AccessToken!, context.RefreshToken);
            };
        });

        return builder;
    }
}
