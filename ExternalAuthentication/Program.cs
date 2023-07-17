using ExternalAuthentication.Google;
using ExternalAuthentication.Store;
using Microsoft.Extensions.Options;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOptions<GoogleOption>().Bind(builder.Configuration.GetSection("Oauth:Google"));

builder.Services.ConfigureUserTokenStore(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(GoogleOption.Google)
    .AddCookie("cookie")
    .AddGoogle(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddGoogleMailHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/users/info", (HttpContext context) =>
{
    var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var name = context.User.FindFirst(ClaimTypes.GivenName)?.Value;

    return new { id, name };
}).RequireAuthorization();

app.SetupGoogleApi();

app.Run();