using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Web.Data;
using SistemaCotizaciones.Web.Data.Entities;

namespace SistemaCotizaciones.Web.Services;

public class OpcionCotizacionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public OpcionCotizacionService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<string>> ObtenerCategorias()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.CategoriasCotizacion
            .OrderBy(x => x.Nombre)
            .Select(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<List<string>> ObtenerFormasPago()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.FormasPagoCotizacion
            .OrderBy(x => x.Nombre)
            .Select(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<List<string>> ObtenerCuentasCorrientes()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.CuentasCorrientesCotizacion
            .OrderBy(x => x.Nombre)
            .Select(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<string> AgregarCategoria(string nombre)
    {
        using var context = _contextFactory.CreateDbContext();
        nombre = Normalizar(nombre);
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("Ingrese una categoria.");
        }

        var existe = await context.CategoriasCotizacion.AnyAsync(x => x.Nombre == nombre);
        if (!existe)
        {
            context.CategoriasCotizacion.Add(new CategoriaCotizacion { Nombre = nombre });
            await context.SaveChangesAsync();
        }

        return nombre;
    }

    public async Task<string> AgregarFormaPago(string nombre)
    {
        using var context = _contextFactory.CreateDbContext();
        nombre = Normalizar(nombre);
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("Ingrese una forma de pago.");
        }

        var existe = await context.FormasPagoCotizacion.AnyAsync(x => x.Nombre == nombre);
        if (!existe)
        {
            context.FormasPagoCotizacion.Add(new FormaPagoCotizacion { Nombre = nombre });
            await context.SaveChangesAsync();
        }

        return nombre;
    }

    public async Task<string> AgregarCuentaCorriente(string nombre)
    {
        using var context = _contextFactory.CreateDbContext();
        nombre = Normalizar(nombre);
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("Ingrese una cuenta corriente.");
        }

        var existe = await context.CuentasCorrientesCotizacion.AnyAsync(x => x.Nombre == nombre);
        if (!existe)
        {
            context.CuentasCorrientesCotizacion.Add(new CuentaCorrienteCotizacion { Nombre = nombre });
            await context.SaveChangesAsync();
        }

        return nombre;
    }

    private static string Normalizar(string value)
    {
        return value.Trim();
    }
}
