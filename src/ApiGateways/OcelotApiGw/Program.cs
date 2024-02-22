using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

//Adding this to provide login configuration
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//Ocelot Configuration
builder.Services.AddOcelot();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//Add Ocelot Middleware
await app.UseOcelot();

app.Run();
