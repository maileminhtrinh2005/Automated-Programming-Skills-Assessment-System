using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Hubs;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Handlers;
using NotificationService.Infrastructure.Persistence;
using RabbitMQ.Client;
using ShareLibrary;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<INotificationAppService, NotificationAppService>();
builder.Services.AddScoped<INotificationGenerator, NotificationGenerator>();
builder.Services.AddScoped<NotificationCreatedHandler>();
builder.Services.AddHostedService<NotificationCreatedSubscriberService>();
builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();
builder.Services.AddTransient<NotificationEventHandler>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IConnectionFactory>(sp =>
       new ConnectionFactory()
       {
           HostName = "localhost", // 
           Port = 5672,            // 
           UserName = "guest",     //
           Password = "guest"      // 
       }
);
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseWebSockets();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowGateway");

app.UseAuthorization();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapControllers();

app.Run();
