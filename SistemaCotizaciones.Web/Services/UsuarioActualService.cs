using Microsoft.AspNetCore.Components.Authorization;
using SistemaCotizaciones.Web.Data.Entities;
using System.Security.Claims;

namespace SistemaCotizaciones.Web.Services;

public class UsuarioActualService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public UsuarioActualService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<UsuarioActual> Obtener()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            return UsuarioActual.Anonimo;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var nombre = principal.FindFirstValue("NombreCompleto")
            ?? principal.Identity.Name
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? string.Empty;
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.Identity.Name ?? string.Empty;

        return new UsuarioActual(
            userId,
            nombre,
            email,
            principal.IsInRole(IdentitySeeder.RolAdmin),
            principal.IsInRole(IdentitySeeder.RolVendedor));
    }
}

public record UsuarioActual(string Id, string Nombre, string Email, bool EsAdmin, bool EsVendedor)
{
    public static UsuarioActual Anonimo { get; } = new(string.Empty, string.Empty, string.Empty, false, false);
    public bool EstaAutenticado => !string.IsNullOrWhiteSpace(Id);
}
