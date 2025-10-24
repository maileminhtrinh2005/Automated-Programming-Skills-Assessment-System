using Microsoft.EntityFrameworkCore;
using SubmissionService.Application.Interface;
using SubmissionService.Infrastructure;
using ShareLibrary;
using RabbitMQ.Client;
using SubmissionService.Infrastructure.Persistence;
using SubmissionService.Application.Service;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<SendToJudge0>();


builder.Services.AddScoped<ICompareTestCase, CompareTestCase>();
builder.Services.AddScoped<ISendToJudge0, SendToJudge0>();
builder.Services.AddScoped<IResultHandle, ResultHandle>();
builder.Services.AddScoped<ISubmissionHandle, SubmissionHandle>();
builder.Services.AddScoped<SubmissionControl>();
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
