using FeedbackService.Application.Interfaces;
using FeedbackService.Application.Services;
using FeedbackService.Infrastructure;
using FeedbackService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Options + HttpClient cho AI
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<GeminiFeedbackGenerator>();
builder.Services.AddScoped<IFeedbackGenerator, GeminiFeedbackGenerator>();
builder.Services.AddScoped<IFeedbackAppService, FeedbackAppService>();

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
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.Run();
