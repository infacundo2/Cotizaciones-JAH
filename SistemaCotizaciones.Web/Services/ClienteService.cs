using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Web.Data;
using SistemaCotizaciones.Web.Data.Entities;

namespace SistemaCotizaciones.Web.Services;

public class ClienteService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly UsuarioActualService _usuarioActualService;

    public ClienteService(IDbContextFactory<ApplicationDbContext> contextFactory, UsuarioActualService usuarioActualService)
    {
        _contextFactory = contextFactory;
        _usuarioActualService = usuarioActualService;
    }

    public async Task<List<Cliente>> ObtenerTodos()
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var query = FiltrarPorUsuario(context.Clientes.AsQueryable(), usuario);
        return await query.OrderBy(x => x.RazonSocial).ToListAsync();
    }

    public async Task<Cliente?> ObtenerPorId(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        return await context.Clientes
            .Include(x => x.Cotizaciones)
            .ThenInclude(x => x.Versiones)
            .Where(x => x.UsuarioVendedorId == usuario.Id)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Cliente>> Buscar(string termino)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        termino = termino.Trim();

        if (string.IsNullOrWhiteSpace(termino))
        {
            return await ObtenerTodos();
        }

        return await FiltrarPorUsuario(context.Clientes.AsQueryable(), usuario)
            .Where(x =>
                x.Rut.Contains(termino) ||
                x.RazonSocial.Contains(termino) ||
                x.Direccion.Contains(termino) ||
                x.Region.Contains(termino) ||
                x.Comuna.Contains(termino) ||
                x.Ciudad.Contains(termino) ||
                x.Email.Contains(termino) ||
                x.Telefono.Contains(termino))
            .OrderBy(x => x.RazonSocial)
            .Take(30)
            .ToListAsync();
    }

    public async Task<Cliente?> BuscarDuplicado(Cliente cliente)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var rut = cliente.Rut.Trim();
        var email = cliente.Email.Trim();
        var razonSocial = cliente.RazonSocial.Trim();

        return await FiltrarPorUsuario(context.Clientes.AsQueryable(), usuario)
            .Where(x => x.Id != cliente.Id)
            .FirstOrDefaultAsync(x =>
                (!string.IsNullOrWhiteSpace(rut) && x.Rut == rut) ||
                (!string.IsNullOrWhiteSpace(email) && x.Email == email) ||
                (!string.IsNullOrWhiteSpace(razonSocial) && x.RazonSocial == razonSocial));
    }

    public async Task<Cliente> Guardar(Cliente cliente)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        if (cliente.Id == 0)
        {
            var duplicado = await BuscarDuplicado(cliente);
            if (duplicado is not null)
            {
                throw new InvalidOperationException($"Ya existe el cliente '{duplicado.RazonSocial}'. Seleccionelo desde la busqueda para evitar duplicados.");
            }

            if (usuario.EstaAutenticado)
            {
                cliente.UsuarioVendedorId = usuario.Id;
                cliente.Vendedor = usuario.Nombre;
            }

            context.Clientes.Add(cliente);
        }
        else
        {
            var existente = await context.Clientes
                .FirstOrDefaultAsync(x => x.Id == cliente.Id && x.UsuarioVendedorId == usuario.Id)
                ?? throw new InvalidOperationException("Cliente no encontrado o sin permisos.");

            ActualizarCliente(existente, cliente);
        }
            
        await context.SaveChangesAsync();
        return cliente;
    }

    public async Task Eliminar(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var cliente = await context.Clientes
            .Include(x => x.Cotizaciones)
            .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioVendedorId == usuario.Id)
            ?? throw new InvalidOperationException("Cliente no encontrado o sin permisos.");

        foreach (var cotizacion in cliente.Cotizaciones)
        {
            cotizacion.ClienteId = null;
        }

        context.Clientes.Remove(cliente);
        await context.SaveChangesAsync();
    }

    private static void ActualizarCliente(Cliente destino, Cliente origen)
    {
        destino.Rut = origen.Rut;
        destino.RazonSocial = origen.RazonSocial;
        destino.Direccion = origen.Direccion;
        destino.Region = origen.Region;
        destino.Comuna = origen.Comuna;
        destino.Ciudad = origen.Ciudad;
        destino.TipoCliente = origen.TipoCliente;
        destino.Sucursal = origen.Sucursal;
        destino.Vendedor = origen.Vendedor;
        destino.Email = origen.Email;
        destino.Telefono = origen.Telefono;
        destino.FechaActualizacion = DateTime.UtcNow;
    }

    private static IQueryable<Cliente> FiltrarPorUsuario(IQueryable<Cliente> query, UsuarioActual usuario)
    {
        return query.Where(x => x.UsuarioVendedorId == usuario.Id);
    }
}
