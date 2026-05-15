using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Web.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;
using QuestPDF.Infrastructure;
using SistemaCotizaciones.Web.Components;
using SistemaCotizaciones.Web.Data.Entities;
using SistemaCotizaciones.Web.Services;

var builder = WebApplication.CreateBuilder(args); // Nota: Es CreateBuilder, no CreateVariables
QuestPDF.Settings.License = LicenseType.Community;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("SistemaCotizaciones");

// 1. Configurar Conexión a MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var mySqlVersion = new MySqlServerVersion(
    Version.Parse(builder.Configuration["Database:MySqlVersion"] ?? "8.0.36"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, mySqlVersion));
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, mySqlVersion), ServiceLifetime.Scoped);

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/login";
    options.ReturnUrlParameter = "returnUrl";
});
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<CotizacionService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PdfCotizacionService>();
builder.Services.AddScoped<OpcionCotizacionService>();
builder.Services.AddSingleton<ChileGeoService>();
builder.Services.AddScoped<UsuarioActualService>();
builder.Services.AddScoped<UsuarioAdminService>();

// 2. Agregar Servicios de MudBlazor
builder.Services.AddMudServices();

// 3. Agregar componentes de Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

await IdentitySeeder.SeedAsync(app.Services);

// Configuración del Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseForwardedHeaders();

var useHttpsRedirection = builder.Configuration.GetValue("Hosting:UseHttpsRedirection", !app.Environment.IsDevelopment());
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/login", async (HttpContext httpContext, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var usuario = form["usuario"].ToString().Trim();
    var password = form["password"].ToString();
    var recordar = form["recordar"].ToString().Equals("on", StringComparison.OrdinalIgnoreCase);
    var returnUrl = form["returnUrl"].ToString();

    if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/'))
    {
        returnUrl = "/";
    }

    var user = await userManager.FindByEmailAsync(usuario) ?? await userManager.FindByNameAsync(usuario);
    if (user is null || !user.Activo)
    {
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    var result = await signInManager.PasswordSignInAsync(user, password, recordar, lockoutOnFailure: true);
    return result.Succeeded
        ? Results.Redirect(returnUrl)
        : Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}");
});

app.MapPost("/auth/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
