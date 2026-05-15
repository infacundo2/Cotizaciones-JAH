namespace SistemaCotizaciones.Web.Data.Entities;

public class CotizacionLinea
{
    public int Id { get; set; }
    public int CotizacionVersionId { get; set; }
    public CotizacionVersion CotizacionVersion { get; set; } = default!;
    public int NumeroLinea { get; set; }
    public string NombreArticulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoPorcentaje { get; set; }

    public decimal TotalLinea =>
        Math.Round(Cantidad * PrecioUnitario * (1 - (DescuentoPorcentaje / 100m)), 0, MidpointRounding.AwayFromZero);
}
