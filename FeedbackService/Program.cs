
using FeedbackService.Application.Interfaces;
using FeedbackService.Application.Services;
using FeedbackService.Infrastructure;
using FeedbackService.Infrastructure.Handlers;
using FeedbackService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using ShareLibrary;
using ShareLibrary.Event;



var builder = WebApplication.CreateBuilder(args);

// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Options + HttpClient cho AI
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<GeminiFeedbackGenerator>();
builder.Services.AddScoped<IFeedbackGenerator, GeminiFeedbackGenerator>();
builder.Services.AddScoped<IFeedbackAppService, FeedbackAppService>();

builder.Services.AddSingleton<IConnectionFactory>(sp =>
       new ConnectionFactory()
       {
           HostName = "localhost", // 
           Port = 5672,            // 
           UserName = "guest",     //
           Password = "guest"      // 
       }
);
builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();
builder.Services.AddScoped<GenerateFeedbackHandler>();
//builder.Services.AddHostedService<RabbitMqSubscriberService>();

// Manual feedback service
builder.Services.AddScoped<IManualFeedbackService, ManualFeedbackService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();
app.Run();
