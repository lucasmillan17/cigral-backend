using CigralBackend.Domain;
using CigralBackend.Domain.Dtos;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Services;
using CigralBackend.Infraestructure.Database.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CigralBackend.Infraestructure.Services
{
    /// <summary>
    /// Servicio de generación de PDFs usando QuestPDF.
    /// </summary>
    public class PdfService : IPdfService
    {
        private readonly IRepository _repository;

        public PdfService(IRepository repository)
        {
            _repository = repository;
            
            // Configurar licencia de QuestPDF (Community)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Genera un PDF de remito de ingreso.
        /// </summary>
        public async Task<byte[]> GenerarPdfRemitoIngreso(int remitoId)
        {
            // Obtener remito con todos los datos necesarios
            var remito = await _repository.GetById<RemitoIngreso>(
                remitoId, 
                "Proveedor", 
                "Detalles.Producto", 
                "Detalles.Lote",
                "Deposito"
            );

            if (remito == null)
            {
                throw new NotFoundException(nameof(RemitoIngreso), remitoId);
            }

            // Obtener el depósito
            var deposito = await _repository.GetById<Deposito>(remito.DepositoId);

            // Mapear a DTO
            var remitoDto = new RemitoPdfDto
            {
                NumeroRemito = remito.NumeroRemito ?? $"RI-{remito.Id}",
                Fecha = remito.Fecha,
                Observaciones = remito.Observaciones,
                TipoRemito = "INGRESO",
                RazonSocial = remito.Proveedor?.RazonSocial,
                CUIT = remito.Proveedor?.Cuit,
                Direccion = remito.Proveedor?.Direccion,
                Telefono = remito.Proveedor?.Telefono,
                Email = remito.Proveedor?.Email,
                Detalles = remito.Detalles?.Select(d => new DetalleRemitoPdfDto
                {
                    ProductoNombre = d.Producto?.Nombre ?? "Sin Nombre",
                    ProductoGtin = d.Producto?.GTIN ?? "Sin GTIN",
                    CodigoLote = d.Lote?.CodigoLote,
                    FechaVencimiento = d.Lote?.FechaVencimiento,
                    NumeroSerie = d.NumeroSerie,
                    Cantidad = d.Cantidad,
                    DepositoNombre = deposito?.Nombre ?? "Sin Depósito"
                }).ToList() ?? new(),
                CantidadTotal = remito.Detalles?.Sum(d => d.Cantidad) ?? 0,
                CantidadItems = remito.Detalles?.Count ?? 0
            };

            // Generar PDF
            return GenerarPdf(remitoDto);
        }

        /// <summary>
        /// Genera un PDF de remito de egreso.
        /// </summary>
        public async Task<byte[]> GenerarPdfRemitoEgreso(int remitoId)
        {
            // Obtener remito con todos los datos necesarios
            var remito = await _repository.GetById<RemitoEgreso>(
                remitoId,
                "Cliente",
                "Detalles.Producto",
                "Detalles.Lote",
                "Deposito"
            );

            if (remito == null)
            {
                throw new NotFoundException(nameof(RemitoEgreso), remitoId);
            }

            // Obtener el depósito
            var deposito = await _repository.GetById<Deposito>(remito.DepositoId);

            // Mapear a DTO
            var remitoDto = new RemitoPdfDto
            {
                NumeroRemito = remito.NumeroRemito ?? $"RE-{remito.Id}",
                Fecha = remito.Fecha,
                Observaciones = remito.Observaciones,
                TipoRemito = "EGRESO",
                RazonSocial = remito.Cliente?.RazonSocial,
                CUIT = remito.Cliente?.Cuit,
                Direccion = remito.Cliente?.Direccion,
                Telefono = remito.Cliente?.Telefono,
                Email = remito.Cliente?.Email,
                Detalles = remito.Detalles?.Select(d => new DetalleRemitoPdfDto
                {
                    ProductoNombre = d.Producto?.Nombre ?? "Sin Nombre",
                    ProductoGtin = d.Producto?.GTIN ?? "Sin GTIN",
                    CodigoLote = d.Lote?.CodigoLote,
                    FechaVencimiento = d.Lote?.FechaVencimiento,
                    NumeroSerie = d.NumeroSerie,
                    Cantidad = d.Cantidad,
                    DepositoNombre = deposito?.Nombre ?? "Sin Depósito"
                }).ToList() ?? new(),
                CantidadTotal = remito.Detalles?.Sum(d => d.Cantidad) ?? 0,
                CantidadItems = remito.Detalles?.Count ?? 0
            };

            // Generar PDF
            return GenerarPdf(remitoDto);
        }

        /// <summary>
        /// Genera el PDF usando QuestPDF.
        /// </summary>
        private byte[] GenerarPdf(RemitoPdfDto remito)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeContent(c, remito));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return documento.GeneratePdf();
        }

        /// <summary>
        /// Compone el encabezado del PDF.
        /// </summary>
        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("CIGRAL").Bold().FontSize(20).FontColor(Colors.Blue.Medium);
                    column.Item().Text("Sistema de Gestión de Inventario").FontSize(10);
                    column.Item().Text("www.cigral.com | info@cigral.com").FontSize(8).FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(100).Height(50).Placeholder();
            });
        }

        /// <summary>
        /// Compone el contenido principal del PDF.
        /// </summary>
        private void ComposeContent(IContainer container, RemitoPdfDto remito)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(10);

                // Título del remito
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Background(Colors.Blue.Medium).Padding(10).Text($"REMITO DE {remito.TipoRemito}")
                            .Bold().FontSize(16).FontColor(Colors.White);
                    });
                });

                // Información del remito
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5)
                            .Text($"Número: {remito.NumeroRemito}").Bold();
                        col.Item().PaddingTop(5).Text($"Fecha: {remito.Fecha:dd/MM/yyyy HH:mm}");
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Información del cliente/proveedor
                column.Item().PaddingVertical(10).Column(col =>
                {
                    var titulo = remito.TipoRemito == "INGRESO" ? "PROVEEDOR" : "CLIENTE";
                    col.Item().Text(titulo).Bold().FontSize(12).FontColor(Colors.Blue.Medium);
                    
                    if (!string.IsNullOrEmpty(remito.RazonSocial))
                        col.Item().Text($"Razón Social: {remito.RazonSocial}");
                    
                    if (!string.IsNullOrEmpty(remito.CUIT))
                        col.Item().Text($"CUIT: {remito.CUIT}");
                    
                    if (!string.IsNullOrEmpty(remito.Direccion))
                        col.Item().Text($"Dirección: {remito.Direccion}");
                    
                    if (!string.IsNullOrEmpty(remito.Telefono))
                        col.Item().Text($"Teléfono: {remito.Telefono}");
                    
                    if (!string.IsNullOrEmpty(remito.Email))
                        col.Item().Text($"Email: {remito.Email}");
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Tabla de detalles
                column.Item().PaddingTop(10).Table(table =>
                {
                    // Definir columnas
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Producto
                        columns.RelativeColumn(2); // GTIN
                        columns.RelativeColumn(1.5f); // Lote
                        columns.RelativeColumn(1.5f); // Vencimiento
                        columns.RelativeColumn(2); // Serie
                        columns.RelativeColumn(1); // Cantidad
                        columns.RelativeColumn(2); // Depósito
                    });

                    // Encabezado de tabla
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Producto").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("GTIN").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Lote").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Vencimiento").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("N° Serie").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Cant.").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Depósito").Bold();
                    });

                    // Filas de datos
                    foreach (var detalle in remito.Detalles)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(detalle.ProductoNombre).FontSize(9);
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(detalle.ProductoGtin).FontSize(8);
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(detalle.CodigoLote ?? "-").FontSize(8);
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(detalle.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(detalle.NumeroSerie ?? "-").FontSize(8);
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text(detalle.Cantidad.ToString()).Bold();
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(detalle.DepositoNombre).FontSize(8);
                    }
                });

                // Totales
                column.Item().PaddingTop(10).AlignRight().Row(row =>
                {
                    row.ConstantItem(200).Column(col =>
                    {
                        col.Item().BorderTop(2).BorderColor(Colors.Blue.Medium).PaddingTop(5)
                            .Row(r =>
                            {
                                r.RelativeItem().Text("Total Items:").Bold();
                                r.ConstantItem(50).AlignRight().Text(remito.CantidadItems.ToString()).Bold();
                            });
                        
                        col.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Text("Cantidad Total:").Bold().FontSize(12);
                            r.ConstantItem(50).AlignRight().Text(remito.CantidadTotal.ToString()).Bold().FontSize(12);
                        });
                    });
                });

                // Observaciones
                if (!string.IsNullOrEmpty(remito.Observaciones))
                {
                    column.Item().PaddingTop(15).Column(col =>
                    {
                        col.Item().Text("OBSERVACIONES:").Bold().FontColor(Colors.Blue.Medium);
                        col.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(10).Text(remito.Observaciones).FontSize(9);
                    });
                }

                // Firmas
                column.Item().PaddingTop(30).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).AlignCenter().Text("Firma y Aclaración").FontSize(8);
                        col.Item().AlignCenter().Text("Emisor").FontSize(8).FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(50);

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).AlignCenter().Text("Firma y Aclaración").FontSize(8);
                        col.Item().AlignCenter().Text("Receptor").FontSize(8).FontColor(Colors.Grey.Darken2);
                    });
                });
            });
        }

        /// <summary>
        /// Compone el pie de página del PDF.
        /// </summary>
        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
                text.Span(" - Documento generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            });
        }
    }
}
