using CigralBackend.Domain;
using CigralBackend.Domain.Dtos;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Services;
using CigralBackend.Infraestructure.Database.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static System.Net.Mime.MediaTypeNames;

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
        private Document CrearDocumento(RemitoPdfDto remito)
        {
            return Document.Create(container =>
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
        }

        private byte[] GenerarPdf(RemitoPdfDto remito)
        {
            var documento = CrearDocumento(remito);
            return documento.GeneratePdf();
        }

        /// <summary>
        /// Compone el encabezado del PDF.
        /// </summary>
        private void ComposeHeader(IContainer container)
        {
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images/LogoCigral.png");

            container.Row(row =>
            {
                row.ConstantItem(150)
                  .AlignLeft()
                  .PaddingTop(-15)
                  .Width(150)
                  .Background(Colors.Grey.Medium) // 1. Pintamos el fondo de azul claro
                  .Image(logoPath); // 2. Colocamos la imagen encima

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

                // Información del remito
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Background(Colors.Blue.Medium).PaddingHorizontal(15).PaddingVertical(10).AlignMiddle().AlignCenter().Text($"ANEXO DE REMITO {remito.TipoRemito}")
                        .Bold().FontSize(16).FontColor(Colors.White);
                        col.Item().PaddingTop(10).Text($"Afecta a Remito N°: -----").FontSize(10);
                        col.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5)
                            .Text($"Número: {remito.NumeroRemito}").Bold();
                        col.Item().PaddingTop(8).Text($"Fecha: {remito.Fecha:dd/MM/yyyy HH:mm}");
                    });
                });

                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Información del cliente/proveedor
                column.Item().Column(col =>
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

                // Tabla de detalles
                column.Item().PaddingTop(8).Table(table =>
                {
                    // Definir columnas
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Producto
                        columns.RelativeColumn(2); // GTIN
                        columns.RelativeColumn(1.5f); // Lote
                        columns.ConstantColumn(70); // Vencimiento
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

        public async Task GenerarPdfRemitoDisenio()
        {
            // Obtener remito con todos los datos necesarios


            // 1. Generamos una lista masiva y variada de detalles falsos
            var detallesFalsos = new List<DetalleRemitoPdfDto>();

            for (int i = 1; i <= 45; i++) // 45 ítems aseguran que el PDF salte a una segunda hoja
            {
                // Trampa 1: Textos extremadamente largos en algunos ítems para probar márgenes
                string nombreProducto = (i == 7 || i == 23)
                    ? $"PRODUCTO {i} CON UN NOMBRE EXTREMADAMENTE LARGO PARA FORZAR EL SALTO DE LÍNEA EN LA TABLA DEL PDF Y VER SI ROMPE EL DISEÑO"
                    : $"Producto Estándar de Prueba {i}";

                // Trampa 2: Alternamos datos nulos. (Ej: Algunos productos no tienen lote ni vencimiento)
                string? codigoLote = (i % 4 == 0) ? null : $"LT-{DateTime.Now.Year}-{i * 10}";
                DateTime? fechaVencimiento = (i % 4 == 0) ? null : DateTime.Now.AddDays(i * 15);

                // Trampa 3: Solo algunos productos tienen número de serie
                string? numeroSerie = (i % 5 == 0) ? $"SN-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}" : null;

                detallesFalsos.Add(new DetalleRemitoPdfDto
                {
                    ProductoNombre = nombreProducto,
                    ProductoGtin = $"77912345678{i:D2}", // Genera 7791234567801, 7791234567802...
                    CodigoLote = codigoLote,
                    FechaVencimiento = fechaVencimiento,
                    NumeroSerie = numeroSerie,
                    Cantidad = i * 2, // Cantidades variables: 2, 4, 6...
                    DepositoNombre = "Depósito Central Prueba"
                });
            }

            // Mapear a DTO
            var remitoDto = new RemitoPdfDto
            {
                NumeroRemito = "AAA-001",
                Fecha = DateTime.Now,
                Observaciones = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                TipoRemito = "INGRESO",
                RazonSocial = "Razon Social PRUEBA",
                CUIT = "00-00000000-0",
                Direccion = "Dirección PRUEBAAA",
                Telefono = "0000-0000",
                Email = "prueba@example.com",
                Detalles = detallesFalsos,
                CantidadTotal = detallesFalsos.Sum(d => d.Cantidad),
                CantidadItems = detallesFalsos.Count
            };

            var documento = CrearDocumento(remitoDto);

            #if DEBUG
            _ = documento.ShowInCompanionAsync();
#endif

            await Task.CompletedTask;

        }
    }
}
