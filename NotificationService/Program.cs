using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
//using NotificationService.Background;
using NotificationService.Hubs;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Handlers;
using NotificationService.Infrastructure.Persistence;
using RabbitMQ.Client;
using SharedLibrary.Jwt;
using ShareLibrary;
using ShareLibrary.Event;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<INotificationAppService, NotificationAppService>();
builder.Services.AddScoped<INotificationGenerator, NotificationGenerator>();
builder.Services.AddScoped<NotificationCreatedHandler>();
builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();
builder.Services.AddHostedService<NotificationCreatedSubscriberService>();
builder.Services.AddTransient<NotificationEventHandler>();
builder.Services.AddSignalR();
//builder.Services.AddTransient<IEventHandler<DeadlineNotification>, DeadlineNotificationHandler>();
//builder.Services.AddHostedService<NotificationBackgroundWorker>();



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


var rabbitHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";

builder.Services.AddSingleton<IConnectionFactory>(sp =>
    new ConnectionFactory()
    {
        HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost",
        UserName = builder.Configuration["RabbitMQ:UserName"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Password"] ?? "guest",
        Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672")
    });
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowGateway", policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                "http://localhost:5261" // Cổng Gateway
               
            ));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.EnsureCreated();
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseWebSockets();
//app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowGateway");

app.UseAuthorization();
app.MapHub<NotificationHub>("/notificationhub");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // 
}

app.Run();
