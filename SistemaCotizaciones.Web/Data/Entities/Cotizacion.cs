namespace SistemaCotizaciones.Web.Data.Entities;

public class Cotizacion
{
    public int Id { get; set; }
    public int Correlativo { get; set; }
    public string NumeroBase { get; set; } = string.Empty;
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string? VendedorId { get; set; }
    public ApplicationUser? Vendedor { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public List<CotizacionVersion> Versiones { get; set; } = new();
}
