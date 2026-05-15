using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Web.Data;
using SistemaCotizaciones.Web.Data.Entities;

namespace SistemaCotizaciones.Web.Services;

public class DashboardService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly UsuarioActualService _usuarioActualService;

    public DashboardService(IDbContextFactory<ApplicationDbContext> contextFactory, UsuarioActualService usuarioActualService)
    {
        _contextFactory = contextFactory;
        _usuarioActualService = usuarioActualService;
    }

    public async Task<DashboardResumen> ObtenerResumen()
    {
        using var context = _contextFactory.CreateDbContext();
        var usuario = await _usuarioActualService.Obtener();
        var query = context.CotizacionVersiones
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Cliente)
            .Include(x => x.Lineas)
            .AsQueryable();

        query = query.Where(x => x.Cotizacion.VendedorId == usuario.Id);

        var versiones = await query.ToListAsync();

        var aprobadas = versiones.Where(x => x.EsAprobada).ToList();

        return new DashboardResumen
        {
            TotalCotizacionesEmitidas = versiones.Count,
            MontoTotalCotizado = versiones.Sum(x => x.Total),
            TotalIvaGenerado = versiones.Sum(x => x.Iva),
            TotalCotizacionesAceptadas = aprobadas.Count,
            MontoTotalAceptado = aprobadas.Sum(x => x.Total),
            IvaTotalAceptado = aprobadas.Sum(x => x.Iva),
            ClientesConMasCotizaciones = versiones
                .GroupBy(x => x.Cotizacion.Cliente?.RazonSocial ?? "Cliente eliminado")
                .Select(x => new RankingCliente(x.Key, x.Count(), x.Sum(v => v.Total)))
                .OrderByDescending(x => x.Cantidad)
                .ThenByDescending(x => x.MontoTotal)
                .Take(5)
                .ToList(),
            ClientesConMasAprobadas = aprobadas
                .GroupBy(x => x.Cotizacion.Cliente?.RazonSocial ?? "Cliente eliminado")
                .Select(x => new RankingCliente(x.Key, x.Count(), x.Sum(v => v.Total)))
                .OrderByDescending(x => x.Cantidad)
                .ThenByDescending(x => x.MontoTotal)
                .Take(5)
                .ToList()
        };
    }
}

public class DashboardResumen
{
    public int TotalCotizacionesEmitidas { get; set; }
    public decimal MontoTotalCotizado { get; set; }
    public decimal TotalIvaGenerado { get; set; }
    public int TotalCotizacionesAceptadas { get; set; }
    public decimal MontoTotalAceptado { get; set; }
    public decimal IvaTotalAceptado { get; set; }
    public List<RankingCliente> ClientesConMasCotizaciones { get; set; } = new();
    public List<RankingCliente> ClientesConMasAprobadas { get; set; } = new();
}

public record RankingCliente(string NombreCliente, int Cantidad, decimal MontoTotal);
