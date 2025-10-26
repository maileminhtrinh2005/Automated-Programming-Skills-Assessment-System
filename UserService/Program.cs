using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using SharedLibrary.Jwt;
using ShareLibrary;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Interface;
using UserService.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// C?u hình RabbitMQ t? appsettings.json
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
builder.Services.AddHostedService<RabbitMqSubscriberService>();

// L?y c?u hình JWT t? appsettings.json
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

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UserAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICRUD, CRUD>();
builder.Services.AddScoped<ILogin,Login>();
builder.Services.AddScoped<IChat, ChatSv>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowAll");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
