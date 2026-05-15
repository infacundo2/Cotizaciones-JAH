namespace SistemaCotizaciones.Web.Data.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Rut { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Comuna { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = "Empresa";
    public string Sucursal { get; set; } = string.Empty;
    public string Vendedor { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? UsuarioVendedorId { get; set; }
    public ApplicationUser? UsuarioVendedor { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public List<Cotizacion> Cotizaciones { get; set; } = new();
}
