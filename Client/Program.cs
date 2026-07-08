using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SSTDigitalRD.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider,SSTAuthStateProvider>();
builder.Services.AddScoped<SSTAuthStateProvider>(sp =>
    (SSTAuthStateProvider)sp
        .GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<SSTDigitalRD.Client.Services.PermisosService> ();

var host = builder.Build();

// Inicializar estado de auth antes de renderizar
var authProvider = host.Services
    .GetRequiredService<AuthenticationStateProvider>()
    as SSTAuthStateProvider;

if (authProvider is not null)
    await authProvider.GetAuthenticationStateAsync();

await host.RunAsync();
//await builder.Build().RunAsync();
