using AssignmentService.Application.Interface;
using AssignmentService.Application.Service;
using AssignmentService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<AssignmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AssignmentDb")));


builder.Services.AddHttpClient<TestCaseFunc>();

builder.Services.AddScoped<ICrudAssignment, CrudAssignment>();
builder.Services.AddScoped<ITestCaseFunc, TestCaseFunc>();
builder.Services.AddScoped<AssignmentControl>();
builder.Services.AddScoped<TestCaseControl>();



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
