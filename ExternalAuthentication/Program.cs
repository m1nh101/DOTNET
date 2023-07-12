using ExternalAuthentication.Google;
using ExternalAuthentication.Store;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<JsonSerializerOptions>(opt =>
{
    opt.PropertyNameCaseInsensitive = true;
    opt.IncludeFields = true;
});

builder.Services.ConfigureUserTokenStore(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddGoogle();

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/api/users/info", (HttpContext context) =>
{
    var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var name = context.User.FindFirst(ClaimTypes.GivenName)?.Value;

    return new { id, name };
}).RequireAuthorization();

app.SetupGoogleApi();

app.Run();