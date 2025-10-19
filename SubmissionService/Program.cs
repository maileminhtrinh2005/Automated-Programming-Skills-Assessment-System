using Microsoft.EntityFrameworkCore;
using SubmissionService.Application.Interface;
using SubmissionService.Application;
using SubmissionService.Infrastructure;
using ShareLibrary;
using RabbitMQ.Client;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<Submit>();


builder.Services.AddScoped<ICompareTestCase, CompareTestCase>();
builder.Services.AddScoped<ISubmit, Submit>();
builder.Services.AddScoped<IGetResult, GetResult>();
builder.Services.AddScoped<Sub>();
builder.Services.AddScoped<RunCodeHandle>();


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
builder.Services.AddHostedService<RabbitMqSubscriberService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
