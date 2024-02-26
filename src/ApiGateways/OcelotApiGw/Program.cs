using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;

var builder = WebApplication.CreateBuilder(args);

//Adding configuration to inform app to start with ocelot.json file
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true);

//Adding this to provide login configuration
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//Ocelot Configuration
builder.Services.AddOcelot()
                .AddCacheManager(x => x.WithDictionaryHandle()); //For Ocelot Cache Manaher


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//Add Ocelot Middleware
await app.UseOcelot();

app.Run();
