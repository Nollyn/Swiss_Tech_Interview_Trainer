using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
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

// Configure ASP.NET Core I18N Localization
builder.Services.AddLocalization();

// Add Application and Infrastructure DI layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Razor Components & Interactive Server Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure Request Localization middleware
var supportedCultures = new[] { "en", "de", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Ensure database is initialized & seed data applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseInitializer.InitializeAsync(context);
    await DatabaseSeeder.SeedAsync(context);
}

// Culture switching endpoint that sets .AspNetCore.Culture cookie
app.MapGet("/api/culture/set", (string culture, string? redirectUri, HttpContext httpContext) =>
{
    if (!string.IsNullOrWhiteSpace(culture))
    {
        httpContext.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            }
        );
    }

    string targetUrl = GetSafeLocalRedirect(redirectUri);
    return Results.LocalRedirect(targetUrl);
});

static string GetSafeLocalRedirect(string? redirectUri)
{
    if (string.IsNullOrWhiteSpace(redirectUri))
    {
        return "/";
    }

    if (redirectUri.StartsWith("//") || redirectUri.StartsWith("/\\") || redirectUri.StartsWith("\\"))
    {
        return "/";
    }

    if (Uri.TryCreate(redirectUri, UriKind.Absolute, out var absoluteUri))
    {
        var localPath = absoluteUri.PathAndQuery;
        if (IsSafeLocalPath(localPath))
        {
            return localPath;
        }
    }
    else if (IsSafeLocalPath(redirectUri))
    {
        return redirectUri;
    }
    else if (IsSafeLocalPath("/" + redirectUri.TrimStart('/')))
    {
        return "/" + redirectUri.TrimStart('/');
    }

    return "/";
}

static bool IsSafeLocalPath(string? path)
{
    if (string.IsNullOrEmpty(path))
    {
        return false;
    }

    if (!path.StartsWith('/'))
    {
        return false;
    }

    if (path.Length > 1 && (path[1] == '/' || path[1] == '\\'))
    {
        return false;
    }

    return true;
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
