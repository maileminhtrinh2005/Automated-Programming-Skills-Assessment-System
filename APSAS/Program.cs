using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
//--------------------------------------------------------------------------------------------------------------
var gatewayFolder = Path.Combine(Directory.GetCurrentDirectory(), "Gateway");
var ocelotFiles = Directory.GetFiles(gatewayFolder, "*.json", SearchOption.TopDirectoryOnly)
                           .Where(f => !f.EndsWith("ocelot.json")); // 

var allRoutes = new JArray();

foreach (var file in ocelotFiles)
{
    var json = JObject.Parse(File.ReadAllText(file));
    if (json["Routes"] is JArray routes)
        allRoutes.Merge(routes);
}

// 
var uniqueRoutes = allRoutes
    .GroupBy(r => r["UpstreamPathTemplate"]?.ToString())
    .Select(g => g.First());

var mergedConfig = new JObject
{
    ["Routes"] = new JArray(uniqueRoutes),
    ["GlobalConfiguration"] = JObject.FromObject(new { BaseUrl = "http://localhost:5261" })
};

File.WriteAllText(Path.Combine(gatewayFolder, "ocelot.json"), mergedConfig.ToString());
//------------------------------------------------------------------------------------------------------------------

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Configuration.AddJsonFile("Gateway/SubmissionOcelot.json", optional: false, reloadOnChange: true);// submission
//builder.Configuration.AddJsonFile("Gateway/AssignmentOcelot.json", optional: false, reloadOnChange: true);// submission

builder.Configuration.AddJsonFile("Gateway/ocelot.json", optional: false, reloadOnChange: true);



builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddOcelot();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles();


app.MapControllers();
await app.UseOcelot();

app.Run();
