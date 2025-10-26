using AdminService.Application.Interface;
using AdminService.Infrastructure.Hubs; // ? Th�m d�ng n�y
using AdminService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using ShareLibrary;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Jwt;
using System.Text;

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

// Add services to the container.

builder.Services.AddControllers();

// L?y c?u h�nh JWT t? appsettings.json
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtOptions"));

// ??ng k� JwtService ?? Inject v�o controller
builder.Services.AddSingleton<IJwtService, JwtService>();

// ? C?u h�nh Authentication d�ng JWT
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




builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
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




//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



app.MapHub<ChatHub>("/chathub");
app.Run();
