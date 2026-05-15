using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaCotizaciones.Web.Data;

namespace SistemaCotizaciones.Web.Services;

public class PdfCotizacionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public PdfCotizacionService(IDbContextFactory<ApplicationDbContext> contextFactory, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<string> GenerarPdf(int versionId)
    {
        using var context = _contextFactory.CreateDbContext();
        var version = await context.CotizacionVersiones
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Cliente)
            .Include(x => x.Cotizacion)
            .ThenInclude(x => x.Vendedor)
            .Include(x => x.Lineas.OrderBy(l => l.NumeroLinea))
            .FirstAsync(x => x.Id == versionId);

        var pdfDirectory = Path.Combine(_environment.WebRootPath, "pdfs");
        Directory.CreateDirectory(pdfDirectory);

        var fileName = $"{version.NumeroCotizacion}.pdf";
        var absolutePath = Path.Combine(pdfDirectory, fileName);
        var relativePath = $"/pdfs/{fileName}";
        var mostrarDescuento = version.Lineas.Any(x => x.DescuentoPorcentaje > 0);
        var empresa = ObtenerEmpresa();
        var logoPath = Path.Combine(_environment.WebRootPath, empresa.LogoPath);
        var clienteCotizacion = version.Cotizacion.Cliente;
        var vendedor = version.Cotizacion.Vendedor;
        var nombreVendedor = vendedor?.NombreCompleto ?? clienteCotizacion?.Vendedor;
        var telefonoVendedor = vendedor?.PhoneNumber ?? string.Empty;
        var emailVendedor = vendedor?.Email ?? string.Empty;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(34);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(88).Height(56).Element(box =>
                        {
                            if (File.Exists(logoPath))
                            {
                                box.Image(logoPath).FitArea();
                            }
                            else
                            {
                                box.Border(1)
                                    .BorderColor(Colors.BlueGrey.Lighten2)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text("JAH")
                                    .Bold()
                                    .FontSize(18)
                                    .FontColor(Colors.BlueGrey.Darken3);
                            }
                        });

                        row.RelativeItem().PaddingLeft(12).Column(left =>
                        {
                            left.Item().Text(empresa.Nombre).Bold().FontSize(18).FontColor(Colors.BlueGrey.Darken3);
                            left.Item().Text($"RUT: {empresa.Rut}");
                            left.Item().Text(empresa.Direccion);
                            left.Item().Text($"{empresa.Telefono} | {empresa.Email}");
                            left.Item().Text(empresa.Web).FontColor(Colors.BlueGrey.Darken1);
                        });

                        row.ConstantItem(190).AlignRight().Column(right =>
                        {
                            right.Item().Background(Colors.BlueGrey.Darken3).Padding(8).Column(card =>
                            {
                                card.Item().Text("COTIZACION").Bold().FontSize(13).FontColor(Colors.White);
                                card.Item().Text(version.NumeroCotizacion).FontSize(12).FontColor(Colors.White);
                            });
                            right.Item().PaddingTop(6).Text($"Fecha emision: {version.FechaEmision:dd-MM-yyyy}");
                            right.Item().Text($"Estado: {TextoEstado(version.Estado)}");
                        });
                    });

                    column.Item().PaddingTop(18).LineHorizontal(1).LineColor(Colors.BlueGrey.Lighten3);
                });

                page.Content().PaddingVertical(18).Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Text("Presentamos el siguiente presupuesto").FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken3);

                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(cliente =>
                    {
                        cliente.Item().Text("Datos del cliente").Bold();
                        cliente.Item().Text($"{Valor(clienteCotizacion?.RazonSocial, "Cliente eliminado")} | RUT: {Valor(clienteCotizacion?.Rut)}");
                        cliente.Item().Text($"{Valor(clienteCotizacion?.Direccion)}, {Valor(clienteCotizacion?.Comuna)}, {Valor(clienteCotizacion?.Ciudad)}, {Valor(clienteCotizacion?.Region)}");
                        cliente.Item().Text($"Contacto: {Valor(clienteCotizacion?.Telefono)} | {Valor(clienteCotizacion?.Email)}");
                    });

                    column.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(24);
                            columns.ConstantColumn(48);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.8f);
                            columns.ConstantColumn(72);
                            if (mostrarDescuento)
                            {
                                columns.ConstantColumn(58);
                            }
                            columns.ConstantColumn(78);
                        });

                        table.Header(header =>
                        {
                            HeaderCell(header.Cell(), "N");
                            HeaderCell(header.Cell(), "Cantidad");
                            HeaderCell(header.Cell(), "Articulo");
                            HeaderCell(header.Cell(), "Descripcion");
                            HeaderCell(header.Cell(), "P. unitario");
                            if (mostrarDescuento)
                            {
                                HeaderCell(header.Cell(), "Desc.");
                            }
                            HeaderCell(header.Cell(), "Total");
                        });

                        foreach (var linea in version.Lineas)
                        {
                            BodyCell(table.Cell(), linea.NumeroLinea.ToString());
                            BodyCell(table.Cell(), linea.Cantidad.ToString("N0"));
                            BodyCell(table.Cell(), linea.NombreArticulo);
                            BodyCell(table.Cell(), linea.Descripcion);
                            BodyCell(table.Cell(), FormatoMoneda(linea.PrecioUnitario, version.Moneda));
                            if (mostrarDescuento)
                            {
                                BodyCell(table.Cell(), $"{linea.DescuentoPorcentaje:N0}%");
                            }
                            BodyCell(table.Cell(), FormatoMoneda(linea.TotalLinea, version.Moneda));
                        }
                    });

                    column.Item().AlignRight().Width(230).Column(totales =>
                    {
                        Totales(totales, "Subtotal", version.Subtotal, version.Moneda);
                        Totales(totales, "Descuento", version.DescuentoLineas + version.DescuentoGlobal, version.Moneda);
                        Totales(totales, "Neto", version.Neto, version.Moneda);
                        Totales(totales, "IVA", version.Iva, version.Moneda);
                        Totales(totales, "Total", version.Total, version.Moneda, true);
                    });

                    column.Item().PaddingTop(8).BorderTop(1).BorderColor(Colors.Grey.Lighten3).PaddingTop(10).Column(info =>
                    {
                        info.Spacing(3);
                        info.Item().Text($"Forma de pago: {Valor(version.FormaPago)}");
                        info.Item().Text($"Cuenta corriente: {Valor(version.CuentaCorriente)}");
                        info.Item().Text($"Plazo de entrega: {Valor(version.PlazoEntrega)}");
                        info.Item().Text($"Moneda: {version.Moneda}");
                        info.Item().Text($"Validez de oferta: {Valor(version.ValidezOferta)}");
                        info.Item().Text($"Comentarios: {Valor(version.Comentarios)}");
                    });

                    column.Item().PaddingTop(16).Row(row =>
                    {
                        row.RelativeItem().Text("Quedo atento a su aprobacion").Bold();
                        row.ConstantItem(230).Column(firma =>
                        {
                            firma.Item().LineHorizontal(1).LineColor(Colors.BlueGrey.Lighten2);
                            firma.Item().PaddingTop(5).Text(Valor(nombreVendedor, "Vendedor")).Bold();
                            firma.Item().Text(Valor(telefonoVendedor));
                            firma.Item().Text(Valor(emailVendedor));
                        });
                    });
                });

                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(Colors.BlueGrey.Lighten3);
                    footer.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text(empresa.Nombre);
                        row.ConstantItem(120).AlignRight().Text(text =>
                        {
                            text.Span("Pagina ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                    });
                });
            });
        }).GeneratePdf(absolutePath);

        return relativePath;
    }

    public void EliminarPdf(string? pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            return;
        }

        var relativePath = pdfPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    private static void HeaderCell(IContainer container, string text)
    {
        container.Background(Colors.BlueGrey.Darken3)
            .Padding(5)
            .Text(text)
            .FontColor(Colors.White)
            .Bold();
    }

    private static void BodyCell(IContainer container, string text)
    {
        container.BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .Padding(5)
            .Text(text);
    }

    private static void Totales(ColumnDescriptor column, string label, decimal value, string moneda, bool destacado = false)
    {
        column.Item().Row(row =>
        {
            var labelText = row.RelativeItem().Text(label);
            var valueText = row.ConstantItem(110).AlignRight().Text(FormatoMoneda(value, moneda));

            if (destacado)
            {
                labelText.Bold();
                valueText.Bold();
            }
        });
    }

    private static string FormatoMoneda(decimal value, string moneda)
    {
        return $"{moneda} {value:N0}";
    }

    private EmpresaPdf ObtenerEmpresa()
    {
        var section = _configuration.GetSection("Empresa");
        return new EmpresaPdf(
            section["Nombre"] ?? "JAH Mantención",
            section["Rut"] ?? "76.420.672-K",
            section["Direccion"] ?? "Los eucaliptus 89, Mostazal",
            section["Telefono"] ?? "+56 9 9196 5507",
            section["Email"] ?? "darvin64@gmail.com",
            section["Web"] ?? "www.jahmantencion.cl",
            section["LogoPath"] ?? "img/logo-jah.png");
    }

    private static string Valor(string? value, string fallback = "-")
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string TextoEstado(SistemaCotizaciones.Web.Data.Entities.EstadoCotizacion estado)
    {
        return estado switch
        {
            SistemaCotizaciones.Web.Data.Entities.EstadoCotizacion.Aprobada => "Aprobada",
            SistemaCotizaciones.Web.Data.Entities.EstadoCotizacion.Rechazada => "Rechazada",
            SistemaCotizaciones.Web.Data.Entities.EstadoCotizacion.Borrador => "Borrador",
            _ => "Pendiente"
        };
    }

    private record EmpresaPdf(string Nombre, string Rut, string Direccion, string Telefono, string Email, string Web, string LogoPath);
}
