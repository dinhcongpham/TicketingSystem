using BookingService.Application.Grpc.Servers;
using BookingService.Application.Implementation;
using BookingService.Application.Interfaces;
using BookingService.Application.Kafka;
using BookingService.Application.Kafka.Consumers;
using BookingService.Application.Kafka.Producers;
using BookingService.Data;
using EventService.Application.Kafka.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddDbContext<BookingDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
});

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


builder.Services.AddHostedService<EventCreatedConsumer>();
builder.Services.AddHostedService<PaymentSuccessConsumer>();

builder.Services.AddScoped<ITicketActions, TicketActions>();
builder.Services.AddScoped<IBookingActions, BookingActions>();

builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5201, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    options.ListenAnyIP(5202, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});


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

app.MapGrpcService<TicketServiceImpl>();

app.Run();
