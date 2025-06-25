using ApiGateway.Application.Grpc.Clients;
using GrpcContracts;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Register Ocelot
builder.Services.AddOcelot();

builder.Services.AddRazorPages();

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

builder.Services.AddScoped<TicketGrpcClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
