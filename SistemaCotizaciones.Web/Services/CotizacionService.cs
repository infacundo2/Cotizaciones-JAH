using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Web.Data;
using SistemaCotizaciones.Web.Data.Entities;

namespace SistemaCotizaciones.Web.Services;

public class CotizacionService
{
    private const string Prefijo = "JAH";
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly PdfCotizacionService _pdfService;
    private readonly UsuarioActualService _usuarioActualService;

    public CotizacionService(IDbContextFactory<ApplicationDbContext> contextFactory, PdfCotizacionService pdfService, UsuarioActualService usuarioActualService)
    {
        _contextFactory = contextFactory;
        _pdfService = pdfService;
        _usuarioActualService = usuarioActualService;
    }

    public async Task<List<CotizacionVersion>> ObtenerEmitidas(string? busqueda = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var query = context.CotizacionVersiones
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Cliente)
            .Include(x => x.Lineas)
            .AsQueryable();

        query = query.Where(x => x.Cotizacion.VendedorId == usuario.Id);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();
            query = query.Where(x =>
                x.NumeroCotizacion.Contains(termino) ||
                (x.Cotizacion.Cliente != null && (
                    x.Cotizacion.Cliente.RazonSocial.Contains(termino) ||
                    x.Cotizacion.Cliente.Rut.Contains(termino))));
        }

        var versiones = await query
            .OrderByDescending(x => x.FechaEmision)
            .Take(100)
            .ToListAsync();

        if (decimal.TryParse(busqueda, out var monto))
        {
            versiones = versiones
                .Where(x => Math.Abs(x.Total - monto) <= Math.Max(1000, monto * 0.1m))
                .ToList();
        }

        return versiones;
    }

    public async Task<ResultadoPaginado<Cotizacion>> ObtenerAgrupadas(string? busqueda = null, int pagina = 1, int tamanioPagina = 20)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        pagina = Math.Max(1, pagina);
        tamanioPagina = Math.Clamp(tamanioPagina, 5, 100);

        var query = context.Cotizaciones
            .Include(x => x.Cliente)
            .Include(x => x.Versiones)
            .ThenInclude(x => x.Lineas)
            .AsQueryable();

        query = query.Where(x => x.VendedorId == usuario.Id);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();
            query = query.Where(x =>
                x.NumeroBase.Contains(termino) ||
                x.Versiones.Any(v => v.NumeroCotizacion.Contains(termino)) ||
                (x.Cliente != null && (
                    x.Cliente.RazonSocial.Contains(termino) ||
                    x.Cliente.Rut.Contains(termino))));
        }

        var totalRegistros = await query.CountAsync();

        var cotizaciones = await query
            .OrderByDescending(x => x.FechaCreacion)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

        foreach (var cotizacion in cotizaciones)
        {
            cotizacion.Versiones = cotizacion.Versiones
                .OrderByDescending(x => x.Version)
                .ToList();
        }

        if (decimal.TryParse(busqueda, out var monto))
        {
            cotizaciones = cotizaciones
                .Where(x => x.Versiones.Any(v => Math.Abs(v.Total - monto) <= Math.Max(1000, monto * 0.1m)))
                .ToList();
            totalRegistros = cotizaciones.Count;
        }

        return new ResultadoPaginado<Cotizacion>(cotizaciones, totalRegistros, pagina, tamanioPagina);
    }

    public async Task<CotizacionVersion?> ObtenerVersion(int versionId)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var query = context.CotizacionVersiones
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Cliente)
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Versiones.OrderBy(v => v.Version))
            .Include(x => x.Lineas.OrderBy(l => l.NumeroLinea))
            .AsQueryable();

        query = query.Where(x => x.Cotizacion.VendedorId == usuario.Id);

        return await query.FirstOrDefaultAsync(x => x.Id == versionId);
    }

    public async Task<CotizacionVersion> CrearNuevaVersion(Cliente cliente, CotizacionVersion version, EstadoCotizacion estado = EstadoCotizacion.Pendiente)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();

        var clientePersistido = await ResolverCliente(context, cliente, usuario);
        var correlativo = await ObtenerSiguienteCorrelativo(context);
        var cotizacion = new Cotizacion
        {
            ClienteId = clientePersistido.Id,
            Correlativo = correlativo,
            NumeroBase = $"{Prefijo}_{correlativo}",
            VendedorId = usuario.EstaAutenticado ? usuario.Id : null,
            FechaCreacion = DateTime.UtcNow
        };

        version.Cotizacion = cotizacion;
        version.Version = 1;
        version.NumeroCotizacion = $"{cotizacion.NumeroBase}_V1";
        version.Estado = estado;
        version.EsAprobada = false;
        NormalizarLineas(version);

        context.CotizacionVersiones.Add(version);
        await context.SaveChangesAsync();

        version.PdfPath = await _pdfService.GenerarPdf(version.Id);
        await context.SaveChangesAsync();

        return version;
    }

    public async Task<CotizacionVersion> GuardarComoNuevaVersion(int cotizacionId, CotizacionVersion origen)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var cotizacion = await context.Cotizaciones
            .Include(x => x.Versiones)
            .FirstAsync(x => x.Id == cotizacionId && x.VendedorId == usuario.Id);

        var siguienteVersion = cotizacion.Versiones.Any() ? cotizacion.Versiones.Max(x => x.Version) + 1 : 1;
        var nuevaVersion = ClonarVersion(origen);
        nuevaVersion.CotizacionId = cotizacion.Id;
        nuevaVersion.Version = siguienteVersion;
        nuevaVersion.NumeroCotizacion = $"{cotizacion.NumeroBase}_V{siguienteVersion}";
        NormalizarLineas(nuevaVersion);

        context.CotizacionVersiones.Add(nuevaVersion);
        await context.SaveChangesAsync();

        nuevaVersion.PdfPath = await _pdfService.GenerarPdf(nuevaVersion.Id);
        await context.SaveChangesAsync();

        return nuevaVersion;
    }

    public async Task<ResultadoGuardadoCotizacion> GuardarEdicionVersion(int versionId, Cliente clienteEditado, CotizacionVersion versionEditada, EstadoCotizacion estado = EstadoCotizacion.Pendiente)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var versionActual = await context.CotizacionVersiones
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Cliente)
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Versiones)
            .Include(x => x.Lineas)
            .FirstAsync(x => x.Id == versionId && x.Cotizacion.VendedorId == usuario.Id);

        if (versionActual.Cotizacion.Cliente is not null)
        {
            ActualizarCliente(versionActual.Cotizacion.Cliente, clienteEditado);
        }

        var debeCrearNuevaVersion = versionActual.Estado != EstadoCotizacion.Borrador;

        if (debeCrearNuevaVersion)
        {
            var siguienteVersion = versionActual.Cotizacion.Versiones.Any()
                ? versionActual.Cotizacion.Versiones.Max(x => x.Version) + 1
                : 1;

            var nuevaVersion = ClonarVersion(versionEditada);
            nuevaVersion.CotizacionId = versionActual.CotizacionId;
            nuevaVersion.Version = siguienteVersion;
            nuevaVersion.NumeroCotizacion = $"{versionActual.Cotizacion.NumeroBase}_V{siguienteVersion}";
            nuevaVersion.Estado = estado;
            nuevaVersion.EsAprobada = false;
            nuevaVersion.FechaEmision = DateTime.UtcNow;
            nuevaVersion.FechaCreacion = DateTime.UtcNow;
            NormalizarLineas(nuevaVersion);

            context.CotizacionVersiones.Add(nuevaVersion);
            await context.SaveChangesAsync();

            nuevaVersion.PdfPath = await _pdfService.GenerarPdf(nuevaVersion.Id);
            await context.SaveChangesAsync();

            return new ResultadoGuardadoCotizacion(nuevaVersion, true);
        }

        versionActual.TipoVenta = versionEditada.TipoVenta;
        versionActual.Estado = estado;
        versionActual.EsAprobada = false;
        versionActual.FechaAprobacion = null;
        versionActual.UsuarioAprobadorId = null;
        versionActual.DescuentoGlobalPorcentaje = versionEditada.DescuentoGlobalPorcentaje;
        versionActual.FormaPago = versionEditada.FormaPago;
        versionActual.CuentaCorriente = versionEditada.CuentaCorriente;
        versionActual.PlazoEntrega = versionEditada.PlazoEntrega;
        versionActual.Moneda = versionEditada.Moneda;
        versionActual.ValidezOferta = versionEditada.ValidezOferta;
        versionActual.Comentarios = versionEditada.Comentarios;
        versionActual.FechaActualizacion = DateTime.UtcNow;

        context.CotizacionLineas.RemoveRange(versionActual.Lineas);
        NormalizarLineas(versionEditada);
        versionActual.Lineas = versionEditada.Lineas.Select(x => new CotizacionLinea
        {
            NumeroLinea = x.NumeroLinea,
            NombreArticulo = x.NombreArticulo,
            Descripcion = x.Descripcion,
            Categoria = x.Categoria,
            Cantidad = x.Cantidad,
            PrecioUnitario = x.PrecioUnitario,
            DescuentoPorcentaje = x.DescuentoPorcentaje
        }).ToList();

        await context.SaveChangesAsync();

        versionActual.PdfPath = await _pdfService.GenerarPdf(versionActual.Id);
        await context.SaveChangesAsync();

        return new ResultadoGuardadoCotizacion(versionActual, false);
    }

    public async Task Aprobar(int versionId, string? usuarioId = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var version = await context.CotizacionVersiones
            .Include(x => x.Cotizacion)
            .FirstAsync(x => x.Id == versionId && x.Cotizacion.VendedorId == usuario.Id);
        var existeAprobada = await context.CotizacionVersiones
            .AnyAsync(x => x.CotizacionId == version.CotizacionId && x.Id != versionId && x.EsAprobada);

        if (existeAprobada)
        {
            throw new InvalidOperationException("Ya existe una version aprobada para esta cotizacion. Primero quite la aprobacion actual.");
        }

        version.EsAprobada = true;
        version.Estado = EstadoCotizacion.Aprobada;
        version.FechaAprobacion = DateTime.UtcNow;
        version.UsuarioAprobadorId = usuarioId;
        await context.SaveChangesAsync();
    }

    public async Task QuitarAprobacion(int versionId)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var version = await context.CotizacionVersiones
            .Include(x => x.Cotizacion)
            .FirstAsync(x => x.Id == versionId && x.Cotizacion.VendedorId == usuario.Id);
        version.EsAprobada = false;
        version.Estado = EstadoCotizacion.Pendiente;
        version.FechaAprobacion = null;
        version.UsuarioAprobadorId = null;
        await context.SaveChangesAsync();
    }

    public async Task EliminarCotizacion(int cotizacionId)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var cotizacion = await context.Cotizaciones
            .Include(x => x.Versiones)
            .FirstOrDefaultAsync(x => x.Id == cotizacionId && x.VendedorId == usuario.Id);

        if (cotizacion is null)
        {
            throw new InvalidOperationException("Cotizacion no encontrada o sin permisos.");
        }

        foreach (var version in cotizacion.Versiones)
        {
            _pdfService.EliminarPdf(version.PdfPath);
        }

        context.Cotizaciones.Remove(cotizacion);
        await context.SaveChangesAsync();
    }

    public async Task EliminarVersion(int versionId)
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var version = await context.CotizacionVersiones
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Versiones)
            .FirstOrDefaultAsync(x => x.Id == versionId && x.Cotizacion.VendedorId == usuario.Id);

        if (version is null)
        {
            throw new InvalidOperationException("Version no encontrada o sin permisos.");
        }

        _pdfService.EliminarPdf(version.PdfPath);

        if (version.Cotizacion.Versiones.Count <= 1)
        {
            context.Cotizaciones.Remove(version.Cotizacion);
        }
        else
        {
            context.CotizacionVersiones.Remove(version);
        }

        await context.SaveChangesAsync();
    }

    private static CotizacionVersion ClonarVersion(CotizacionVersion origen)
    {
        return new CotizacionVersion
        {
            TipoVenta = origen.TipoVenta,
            DescuentoGlobalPorcentaje = origen.DescuentoGlobalPorcentaje,
            FormaPago = origen.FormaPago,
            CuentaCorriente = origen.CuentaCorriente,
            PlazoEntrega = origen.PlazoEntrega,
            Moneda = origen.Moneda,
            ValidezOferta = origen.ValidezOferta,
            Comentarios = origen.Comentarios,
            Lineas = origen.Lineas.Select(x => new CotizacionLinea
            {
                NumeroLinea = x.NumeroLinea,
                NombreArticulo = x.NombreArticulo,
                Descripcion = x.Descripcion,
                Categoria = x.Categoria,
                Cantidad = x.Cantidad,
                PrecioUnitario = x.PrecioUnitario,
                DescuentoPorcentaje = x.DescuentoPorcentaje
            }).ToList()
        };
    }

    private static async Task<Cliente> ResolverCliente(ApplicationDbContext context, Cliente cliente, UsuarioActual usuario)
    {
        if (cliente.Id > 0)
        {
            return await context.Clientes.FirstAsync(x => x.Id == cliente.Id && x.UsuarioVendedorId == usuario.Id);
        }

        var rut = cliente.Rut.Trim();
        var email = cliente.Email.Trim();
        var razonSocial = cliente.RazonSocial.Trim();
        var duplicadoQuery = context.Clientes.AsQueryable();
        duplicadoQuery = duplicadoQuery.Where(x => x.UsuarioVendedorId == usuario.Id);

        var duplicado = await duplicadoQuery.FirstOrDefaultAsync(x =>
            (!string.IsNullOrWhiteSpace(rut) && x.Rut == rut) ||
            (!string.IsNullOrWhiteSpace(email) && x.Email == email) ||
            (!string.IsNullOrWhiteSpace(razonSocial) && x.RazonSocial == razonSocial));

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
        await context.SaveChangesAsync();
        return cliente;
    }

    private static async Task<int> ObtenerSiguienteCorrelativo(ApplicationDbContext context)
    {
        var ultimo = await context.Cotizaciones.MaxAsync(x => (int?)x.Correlativo);
        return (ultimo ?? 0) + 1;
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

    private static void NormalizarLineas(CotizacionVersion version)
    {
        version.Lineas = version.Lineas
            .Where(x => !string.IsNullOrWhiteSpace(x.NombreArticulo) || !string.IsNullOrWhiteSpace(x.Descripcion))
            .ToList();

        for (var i = 0; i < version.Lineas.Count; i++)
        {
            version.Lineas[i].NumeroLinea = i + 1;
        }
    }
}

public record ResultadoGuardadoCotizacion(CotizacionVersion Version, bool CreoNuevaVersion);

public record ResultadoPaginado<T>(List<T> Items, int TotalRegistros, int Pagina, int TamanioPagina)
{
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(TotalRegistros / (double)TamanioPagina));
}

internal static class CotizacionExtensions
{
    public static Cliente ClienteOrThrow(this Cotizacion cotizacion)
    {
        return cotizacion.Cliente ?? throw new InvalidOperationException("La cotizacion no tiene cliente asociado.");
    }
}
