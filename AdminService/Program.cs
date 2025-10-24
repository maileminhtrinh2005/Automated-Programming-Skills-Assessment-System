using AdminService.Application.Interface;
<<<<<<< HEAD
using AdminService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
=======
using AdminService.Infrastructure.Hubs; // ? Thêm dòng này
using AdminService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using ShareLibrary;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddSingleton<RabbitMQEventBus>();
builder.Services.AddScoped<ChatMessageHandler>();
builder.Services.AddHostedService<ChatBackgroundService>();

builder.Services.AddSignalR();
>>>>>>> vu

// Add services to the container.

builder.Services.AddControllers();
<<<<<<< HEAD
=======
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
>>>>>>> vu
builder.Services.AddDbContext<AdminAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAPIConfigService, APIConfigService>();
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

<<<<<<< HEAD
=======

>>>>>>> vu
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

<<<<<<< HEAD
=======
app.MapHub<ChatHub>("/chathub");
>>>>>>> vu
app.Run();
