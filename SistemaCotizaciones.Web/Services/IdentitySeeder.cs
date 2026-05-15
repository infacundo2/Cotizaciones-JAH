using Microsoft.AspNetCore.Identity;
using SistemaCotizaciones.Web.Data.Entities;

namespace SistemaCotizaciones.Web.Services;

public static class IdentitySeeder
{
    public const string RolAdmin = "Admin";
    public const string RolVendedor = "Vendedor";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await CrearRol(roleManager, RolAdmin);
        await CrearRol(roleManager, RolVendedor);

        var adminEmail = configuration["SeedAdmin:Email"] ?? "admin@jahmantencion.cl";
        var adminPassword = configuration["SeedAdmin:Password"] ?? "Admin12345";
        var adminName = configuration["SeedAdmin:Nombre"] ?? "Administrador JAH";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NombreCompleto = adminName,
                Activo = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                var errores = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"No se pudo crear el usuario admin inicial: {errores}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, RolAdmin))
        {
            await userManager.AddToRoleAsync(admin, RolAdmin);
        }
    }

    private static async Task CrearRol(RoleManager<IdentityRole> roleManager, string nombre)
    {
        if (!await roleManager.RoleExistsAsync(nombre))
        {
            await roleManager.CreateAsync(new IdentityRole(nombre));
        }
    }
}
