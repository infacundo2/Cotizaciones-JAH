namespace SistemaCotizaciones.Web.Data.Entities;

public class CategoriaCotizacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class FormaPagoCotizacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CuentaCorrienteCotizacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
