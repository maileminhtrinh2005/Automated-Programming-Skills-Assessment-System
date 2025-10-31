
using FeedbackService.Application.Interfaces;
using FeedbackService.Application.Services;
using FeedbackService.Infrastructure;

using FeedbackService.Infrastructure.Handlers;
using FeedbackService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using SharedLibrary.Jwt;
using ShareLibrary;
using ShareLibrary.Event;
using System.Text;




var builder = WebApplication.CreateBuilder(args);



// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Options + HttpClient cho AI
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<GeminiFeedbackGenerator>();
builder.Services.AddScoped<IFeedbackGenerator, GeminiFeedbackGenerator>();
builder.Services.AddScoped<IFeedbackAppService, FeedbackAppService>();
builder.Services.AddHttpClient<ITestcaseFeedbackGenerator, GeminiTestcaseFeedbackGenerator>();
//builder.Services.AddScoped<ITestcaseFeedbackGenerator, TestcaseFeedbackGenerator>();

builder.Services.AddScoped<IManualFeedbackService, ManualFeedbackService>();

//

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtOptions"));

// ??ng ký JwtService ?? Inject vào controller
builder.Services.AddSingleton<IJwtService, JwtService>();

// ? C?u hình Authentication dùng JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
        };
    });

// B?t Authorization middleware
builder.Services.AddAuthorization();

//

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
builder.Services.AddHostedService<RabbitMqSubscriberService>();
builder.Services.AddScoped<FeedbackPushService>();

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
