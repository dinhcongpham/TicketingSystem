using BookingService.Application.Grpc.Clients;
using EventService.Application.Implementation;
using EventService.Application.Interfaces;
using EventService.Application.Kafka;
using EventService.Application.Kafka.Interfaces;
using EventService.Application.Kafka.Producers;
using EventService.Application.SearchService;
using EventService.Data;
using Grpc.Core;
using GrpcContracts;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Net;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEventActions, EventActions>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configSection = builder.Configuration.GetRequiredSection("Redis");
    var connectionString = configSection.GetValue<string>("ConnectionString");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Redis connection string is not configured.");
    }

    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// Register HTTP client for SearchService
builder.Services.AddHttpClient<ISearchServiceClient, SearchServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:SearchService:BaseUrl"] ?? "http://localhost:5091");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddGrpc();

// Call Grpc Client to BookingService
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.Services.AddGrpcClient<TicketService.TicketServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Services:BookingService:GrpcUrl"] ?? "http://localhost:5202");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new SocketsHttpHandler
    {
        AllowAutoRedirect = true,
        AutomaticDecompression = DecompressionMethods.All,
        EnableMultipleHttp2Connections = true
    };
});
builder.Services.AddScoped<BookingGrpcClient>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}


app.UseAuthorization();

app.MapControllers();

app.Run();
