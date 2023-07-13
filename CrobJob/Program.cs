using CrobJob.Crons;
using CrobJob.Databases;
using Quartz;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddSingleton<LogStore>();

//builder.Services.AddHostedService<CrobJob.Crons.CronosTimerJob>();

builder.Services.AddQuartz(opt =>
{
    opt.UseMicrosoftDependencyInjectionScopedJobFactory();

    var jobKey = new JobKey("Log");

    opt.AddJob<QuartzJob>(opt => opt.WithIdentity(jobKey));

    opt.AddTrigger(opt => opt.ForJob(jobKey)
        .WithIdentity("Log-trigger")
        .WithCronSchedule("* * * * * ?"));
});

builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});

// Add services to the container.
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

app.Run();
