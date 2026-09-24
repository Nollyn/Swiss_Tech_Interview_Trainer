using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using SwissTechTrainer.Application;
using SwissTechTrainer.Infrastructure;
using SwissTechTrainer.Infrastructure.Persistence;
using SwissTechTrainer.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection with disk key persistence to prevent SignalR keyring invalidation on container restart
var keysDirectory = new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dp-keys"));
if (!keysDirectory.Exists)
{
    keysDirectory.Create();
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDirectory)
    .SetApplicationName("SwissTechTrainer");

// Add Application and Infrastructure DI layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Razor Components & Interactive Server Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Ensure database is initialized & seed data applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Make Program public for WebApplicationFactory in integration tests if needed
public partial class Program { }
