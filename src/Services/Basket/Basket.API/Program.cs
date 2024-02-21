using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Redis Configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("CacheSettings:ConnectionString").Value;
});

//MassTransit-RabbitMQ configuration
builder.Services.AddMassTransit(config => {
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetSection("EventBusSettings:HostAddress").Value); 
    });
});
builder.Services.AddMassTransitHostedService();

//General Configuration
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddAutoMapper(typeof(Program));

//Grps Configuration
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
        options.Address = new Uri(builder.Configuration.GetSection("GrpcSettings:DiscountUrl").Value));
builder.Services.AddScoped<DiscountGrpcServices>();


builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
