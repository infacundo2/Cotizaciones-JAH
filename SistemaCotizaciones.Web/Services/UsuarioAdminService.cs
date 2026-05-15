using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Web.Data.Entities;

namespace SistemaCotizaciones.Web.Services;

public class UsuarioAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsuarioAdminService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<List<UsuarioListado>> ObtenerTodos()
    {
        var usuarios = await _userManager.Users
            .OrderBy(x => x.NombreCompleto)
            .ToListAsync();

        var resultado = new List<UsuarioListado>();
        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            resultado.Add(new UsuarioListado
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                UserName = usuario.UserName ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                Telefono = usuario.PhoneNumber ?? string.Empty,
                Activo = usuario.Activo,
                Roles = roles.ToList()
            });
        }

        return resultado;
    }

    public async Task<UsuarioEditor?> ObtenerParaEditar(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        return new UsuarioEditor
        {
            Id = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            UserName = usuario.UserName ?? usuario.Email ?? string.Empty,
            Email = usuario.Email ?? string.Empty,
            Telefono = usuario.PhoneNumber ?? string.Empty,
            Rol = roles.Contains(IdentitySeeder.RolAdmin) ? IdentitySeeder.RolAdmin : IdentitySeeder.RolVendedor,
            Activo = usuario.Activo
        };
    }

    public async Task Guardar(UsuarioEditor editor)
    {
        await AsegurarRoles();
        editor.Email = editor.Email.Trim();
        editor.UserName = NormalizarUserName(editor.UserName, editor.Email);

        if (string.IsNullOrWhiteSpace(editor.Id))
        {
            await CrearUsuario(editor);
            return;
        }

        await ActualizarUsuario(editor);
    }

    public async Task CambiarEstado(string id, bool activo)
    {
        var usuario = await _userManager.FindByIdAsync(id)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        usuario.Activo = activo;
        var result = await _userManager.UpdateAsync(usuario);
        Validar(result);
    }

    public async Task ResetearClave(string id, string nuevaClave)
    {
        var usuario = await _userManager.FindByIdAsync(id)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var result = await _userManager.ResetPasswordAsync(usuario, token, nuevaClave);
        Validar(result);
    }

    private async Task CrearUsuario(UsuarioEditor editor)
    {
        if (string.IsNullOrWhiteSpace(editor.Password))
        {
            throw new InvalidOperationException("La clave es obligatoria para un usuario nuevo.");
        }

        var usuario = new ApplicationUser
        {
            UserName = editor.UserName,
            Email = editor.Email,
            EmailConfirmed = true,
            NombreCompleto = editor.NombreCompleto,
            PhoneNumber = editor.Telefono,
            Activo = editor.Activo
        };

        var result = await _userManager.CreateAsync(usuario, editor.Password);
        Validar(result);

        result = await _userManager.AddToRoleAsync(usuario, editor.Rol);
        Validar(result);
    }

    private async Task ActualizarUsuario(UsuarioEditor editor)
    {
        var usuario = await _userManager.FindByIdAsync(editor.Id)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        usuario.NombreCompleto = editor.NombreCompleto;
        usuario.Email = editor.Email;
        usuario.UserName = editor.UserName;
        usuario.PhoneNumber = editor.Telefono;
        usuario.Activo = editor.Activo;

        var result = await _userManager.UpdateAsync(usuario);
        Validar(result);

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (rolesActuales.Any())
        {
            result = await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            Validar(result);
        }

        result = await _userManager.AddToRoleAsync(usuario, editor.Rol);
        Validar(result);

        if (!string.IsNullOrWhiteSpace(editor.Password))
        {
            await ResetearClave(usuario.Id, editor.Password);
        }
    }

    private async Task AsegurarRoles()
    {
        foreach (var rol in new[] { IdentitySeeder.RolAdmin, IdentitySeeder.RolVendedor })
        {
            if (!await _roleManager.RoleExistsAsync(rol))
            {
                await _roleManager.CreateAsync(new IdentityRole(rol));
            }
        }
    }

    private static void Validar(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errores = string.Join(" ", result.Errors.Select(FormatearError));
        throw new InvalidOperationException(errores);
    }

    private static string FormatearError(IdentityError error)
    {
        if (error.Code == "InvalidEmail")
        {
            return "El email ingresado no es valido. Revise que tenga formato usuario@dominio.cl.";
        }

        if (error.Code == "InvalidUserName")
        {
            return "El usuario ingresado no es valido. Use letras, numeros, punto, guion o guion bajo.";
        }

        if (error.Code == "DuplicateUserName")
        {
            return "Ya existe un usuario con ese nombre de usuario.";
        }

        if (error.Code == "DuplicateEmail")
        {
            return "Ya existe un usuario con ese email.";
        }

        return error.Description;
    }

    private static string NormalizarUserName(string userName, string email)
    {
        userName = userName.Trim();
        return string.IsNullOrWhiteSpace(userName) ? email : userName;
    }
}

public class UsuarioListado
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public List<string> Roles { get; set; } = new();
    public string RolesTexto => Roles.Count == 0 ? "Sin rol" : string.Join(", ", Roles);
}

public class UsuarioEditor
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = IdentitySeeder.RolVendedor;
    public bool Activo { get; set; } = true;
    public bool EsNuevo => string.IsNullOrWhiteSpace(Id);
}
