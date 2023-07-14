namespace ExternalAuthentication.Google;

public class GoogleOption
{
    public const string Google = "Google";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = string.Empty;
    public string AuthorizationEndPoint { get; set; } = string.Empty;
    public string UserInfoEndPoint { get; set; } = string.Empty;
    public string TokenEndPoint { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();

}
