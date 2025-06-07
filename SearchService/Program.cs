using Nest;
using SearchService.Models;
using SearchService.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Elasticsearch Configuration
var elasticsearchUrl = builder.Configuration.GetConnectionString("Elasticsearch") ?? "http://localhost:9200";
var settings = new ConnectionSettings(new Uri(elasticsearchUrl))
    .DefaultIndex("events")
    .DefaultMappingFor<EventSearchDocument>(m => m
        .IndexName("events")
        .IdProperty(p => p.Id)
    );

builder.Services.AddSingleton<IElasticClient>(new ElasticClient(settings));
builder.Services.AddScoped<IEventSearchService, EventSearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();
app.MapControllers();

app.Run();