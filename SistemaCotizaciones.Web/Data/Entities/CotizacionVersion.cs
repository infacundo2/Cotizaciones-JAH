namespace SistemaCotizaciones.Web.Data.Entities;

public class CotizacionVersion
{
    public const decimal IvaChile = 0.19m;

    public int Id { get; set; }
    public int CotizacionId { get; set; }
    public Cotizacion Cotizacion { get; set; } = default!;
    public int Version { get; set; } = 1;
    public string NumeroCotizacion { get; set; } = string.Empty;
    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.Pendiente;
    public bool EsAprobada { get; set; }
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public DateTime? FechaAprobacion { get; set; }
    public string? UsuarioAprobadorId { get; set; }
    public ApplicationUser? UsuarioAprobador { get; set; }
    public TipoVenta TipoVenta { get; set; } = TipoVenta.Nacional;
    public decimal DescuentoGlobalPorcentaje { get; set; }
    public string FormaPago { get; set; } = string.Empty;
    public string CuentaCorriente { get; set; } = string.Empty;
    public string PlazoEntrega { get; set; } = string.Empty;
    public string Moneda { get; set; } = "CLP";
    public string ValidezOferta { get; set; } = "15 dias";
    public string Comentarios { get; set; } = string.Empty;
    public string? PdfPath { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public List<CotizacionLinea> Lineas { get; set; } = new();

    public decimal Subtotal => Math.Round(Lineas.Sum(x => x.Cantidad * x.PrecioUnitario), 0, MidpointRounding.AwayFromZero);
    public decimal DescuentoLineas => Math.Round(Lineas.Sum(x => (x.Cantidad * x.PrecioUnitario) - x.TotalLinea), 0, MidpointRounding.AwayFromZero);
    public decimal TotalLineas => Math.Round(Lineas.Sum(x => x.TotalLinea), 0, MidpointRounding.AwayFromZero);
    public decimal DescuentoGlobal => Math.Round(TotalLineas * (DescuentoGlobalPorcentaje / 100m), 0, MidpointRounding.AwayFromZero);
    public decimal Neto => Math.Max(0, TotalLineas - DescuentoGlobal);
    public decimal Iva => TipoVenta == TipoVenta.Nacional ? Math.Round(Neto * IvaChile, 0, MidpointRounding.AwayFromZero) : 0;
    public decimal Total => Neto + Iva;
}
