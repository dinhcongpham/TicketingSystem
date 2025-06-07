using Front_end.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Configure HTTP client with HTTP/2 support
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.DefaultRequestVersion = new Version(2, 0);
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

    // If you have base URL in configuration
    var baseUrl = builder.Configuration["ApiGateway:BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
    EnableMultipleHttp2Connections = true,
    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
    {
        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
    }
});
builder.Services.AddScoped<IApiService, ApiService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "eventDetails",
    pattern: "event/{id}",
    defaults: new { controller = "Event", action = "Details" });

app.MapControllerRoute(
    name: "eventPayment",
    pattern: "event/payment/{id}",
    defaults: new { controller = "Event", action = "Payment" });

app.Run();
