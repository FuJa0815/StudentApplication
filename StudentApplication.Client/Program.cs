using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using StudentApplication.Client;
using StudentApplication.Client.HttpRepository;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddLogging();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var restServer = builder.Configuration.GetValue<string>("RestServer");
var hubServer = builder.Configuration.GetValue<string>("HubServer");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(restServer) });
builder.Services.AddScoped(sp => new NotificationHub(new Uri(hubServer)));
builder.Services.AddScoped(typeof(RestData<,>));
builder.Services.AddMudServices();
await builder.Build().RunAsync();