using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Đọc file cấu hình ocelot.json
builder.Configuration.AddJsonFile("Feedbackocelot.json", optional: false, reloadOnChange: true);



// 2. Thêm Ocelot vào DI container
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// 3. Sử dụng Ocelot middleware
app.UseDefaultFiles();
app.UseStaticFiles();
await app.UseOcelot();

app.Run();