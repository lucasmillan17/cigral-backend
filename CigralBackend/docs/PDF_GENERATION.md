# ? Sistema de Generación de PDFs con QuestPDF - Implementación Completa

## ?? PDF DE REMITOS IMPLEMENTADO

**Sistema completo para generar PDFs profesionales de remitos de ingreso y egreso**

---

## ?? Arquitectura Implementada

### Separación de Responsabilidades (Clean Architecture)

```
???????????????????????????????????????????????
?           API Layer (Controllers)           ?
?  RemitosController.cs                       ?
?  - GET /api/remitos/ingreso/{id}/pdf       ?
?  - GET /api/remitos/egreso/{id}/pdf        ?
???????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????
?          Domain Layer (Interfaces)          ?
?  Domain/Services/IPdfService.cs             ?
?  Domain/Dtos/RemitoPdfDto.cs                ?
?  Domain/Dtos/DetalleRemitoPdfDto.cs         ?
???????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????
?      Infrastructure (Implementation)        ?
?  Infrastructure/Services/PdfService.cs      ?
?  - QuestPDF Integration                     ?
?  - Plantilla PDF profesional                ?
???????????????????????????????????????????????
```

---

## ?? Paquetes Instalados

### QuestPDF 2024.12.3

```bash
cd CigralBackend.Infraestructure
dotnet add package QuestPDF --version 2024.12.3
```

**Características:**
- ? Licencia Community (gratuita)
- ? API fluent moderna
- ? Soporte completo para tablas, colores, fuentes
- ? Generación en memoria (byte[])
- ? Sin dependencias externas

---

## ?? Archivos Creados

### 1. Domain/Services/IPdfService.cs
**Interfaz del servicio de PDF**

```csharp
public interface IPdfService
{
    Task<byte[]> GenerarPdfRemitoIngreso(int remitoId);
    Task<byte[]> GenerarPdfRemitoEgreso(int remitoId);
}
```

### 2. Domain/Dtos/RemitoPdfDto.cs
**DTOs para datos del PDF**

```csharp
public record RemitoPdfDto
{
    public string NumeroRemito { get; init; }
    public DateTime Fecha { get; init; }
    public string TipoRemito { get; init; } // "INGRESO" o "EGRESO"
    public string? RazonSocial { get; init; }
    public string? CUIT { get; init; }
    public List<DetalleRemitoPdfDto> Detalles { get; init; }
    public int CantidadTotal { get; init; }
    public int CantidadItems { get; init; }
}

public record DetalleRemitoPdfDto
{
    public string ProductoNombre { get; init; }
    public string ProductoGtin { get; init; }
    public string? CodigoLote { get; init; }
    public DateTime? FechaVencimiento { get; init; }
    public string? NumeroSerie { get; init; }
    public int Cantidad { get; init; }
    public string DepositoNombre { get; init; }
}
```

### 3. Infrastructure/Services/PdfService.cs
**Implementación con QuestPDF** (380+ líneas)

**Características:**
- ? Genera PDFs A4 con márgenes
- ? Encabezado con logo/empresa
- ? Título diferenciado (INGRESO/EGRESO)
- ? Información de cliente/proveedor
- ? Tabla de productos con 7 columnas
- ? Totales calculados
- ? Observaciones opcionales
- ? Firmas (emisor/receptor)
- ? Pie de página con paginación
- ? Diseño profesional con colores

### 4. Controllers/RemitosController.cs (Actualizado)
**Nuevos endpoints:**

```csharp
[HttpGet("ingreso/{id}/pdf")]
public async Task<IActionResult> ImprimirRemitoIngreso(int id)

[HttpGet("egreso/{id}/pdf")]
public async Task<IActionResult> ImprimirRemitoEgreso(int id)
```

### 5. Program.cs (Actualizado)
**Registro del servicio:**

```csharp
builder.Services.AddScoped<IPdfService, PdfService>();
```

---

## ?? Endpoints

### 1. GET `/api/remitos/ingreso/{id}/pdf`

**Genera PDF de remito de ingreso**

**Request:**
```http
GET /api/remitos/ingreso/5/pdf
Authorization: Bearer {token}
```

**Response:**
- Content-Type: `application/pdf`
- Content-Disposition: `attachment; filename="Remito_Ingreso_5_20250127.pdf"`
- Body: Binary PDF data

**Ejemplo de uso en Frontend:**

```javascript
// Descargar directamente
window.open('/api/remitos/ingreso/5/pdf', '_blank');

// Mostrar en iframe
<iframe src="/api/remitos/ingreso/5/pdf" width="100%" height="600px"></iframe>

// Fetch y crear blob
const response = await fetch('/api/remitos/ingreso/5/pdf');
const blob = await response.blob();
const url = URL.createObjectURL(blob);
window.open(url, '_blank');

// Imprimir automáticamente
const iframe = document.createElement('iframe');
iframe.style.display = 'none';
iframe.src = url;
document.body.appendChild(iframe);
iframe.onload = () => {
  iframe.contentWindow.print();
};
```

---

### 2. GET `/api/remitos/egreso/{id}/pdf`

**Genera PDF de remito de egreso**

**Request:**
```http
GET /api/remitos/egreso/10/pdf
Authorization: Bearer {token}
```

**Response:**
- Content-Type: `application/pdf`
- Content-Disposition: `attachment; filename="Remito_Egreso_10_20250127.pdf"`
- Body: Binary PDF data

**Diferencias visuales:**
- Título: "REMITO DE EGRESO" (en lugar de INGRESO)
- Sección: "CLIENTE" (en lugar de PROVEEDOR)
- Resto de la plantilla idéntica

---

## ?? Diseño del PDF

### Vista Previa del Layout

```
?????????????????????????????????????????????????????????????
?  CIGRAL                                           [LOGO]  ?
?  Sistema de Gestión de Inventario                         ?
?  www.cigral.com | info@cigral.com                         ?
?????????????????????????????????????????????????????????????
?                                                            ?
?  ??????????????????????????????????????????????????????  ?
?  ?  REMITO DE INGRESO                                  ?  ?
?  ??????????????????????????????????????????????????????  ?
?                                                            ?
?  Número: RI-5           Fecha: 27/01/2025 14:30          ?
?  ????????????????????????????????????????????????????     ?
?                                                            ?
?  PROVEEDOR                                                ?
?  Razón Social: Proveedor SA                               ?
?  CUIT: 30-12345678-9                                      ?
?  Dirección: Av. Principal 123                             ?
?  Teléfono: +54 11 1234-5678                               ?
?  Email: contacto@proveedor.com                            ?
?  ????????????????????????????????????????????????????     ?
?                                                            ?
?  ????????????????????????????????????????????????????    ?
?  ? Producto   ? GTIN ? Lote ? Venc. ? Serie ? Cant ?    ?
?  ????????????????????????????????????????????????????    ?
?  ? Paracet... ? 7... ? L-A  ? 15... ?  -    ?  50  ?    ?
?  ? Ibuprofe...? 7... ? L-B  ? 20... ?  -    ?  30  ?    ?
?  ????????????????????????????????????????????????????    ?
?                                                            ?
?                                Total Items: 2             ?
?                                Cantidad Total: 80         ?
?                                                            ?
?  OBSERVACIONES:                                           ?
?  ??????????????????????????????????????????????????????  ?
?  ? Ingreso de mercadería enero 2025                    ?  ?
?  ??????????????????????????????????????????????????????  ?
?                                                            ?
?  ___________________        ___________________           ?
?  Firma y Aclaración         Firma y Aclaración            ?
?  Emisor                     Receptor                      ?
?                                                            ?
?????????????????????????????????????????????????????????????
?  Página 1 de 1 - Documento generado el 27/01/2025 14:30 ?
?????????????????????????????????????????????????????????????
```

---

## ?? Características del PDF

### Encabezado
- ? Logo/nombre de la empresa
- ? Información de contacto
- ? Placeholder para logo (100x50 px)

### Título
- ? Fondo azul
- ? Texto blanco en negrita
- ? "REMITO DE INGRESO" o "REMITO DE EGRESO"

### Información del Remito
- ? Número de remito (o generado automáticamente)
- ? Fecha y hora formateada

### Cliente/Proveedor
- ? Título dinámico (CLIENTE/PROVEEDOR)
- ? Razón Social
- ? CUIT
- ? Dirección
- ? Teléfono
- ? Email
- ? Solo muestra campos con datos

### Tabla de Productos
- ? 7 Columnas:
  - Producto (nombre)
  - GTIN (código de barras)
  - Lote (código)
  - Vencimiento (fecha)
  - Número de Serie
  - Cantidad
  - Depósito (nombre)
- ? Encabezado con fondo gris
- ? Bordes y separadores
- ? Texto de diferentes tamaños
- ? Cantidad alineada a la derecha y en negrita

### Totales
- ? Total de items (líneas)
- ? Cantidad total (suma)
- ? Borde superior azul
- ? Alineado a la derecha

### Observaciones
- ? Solo se muestra si hay observaciones
- ? Título en azul
- ? Cuadro con borde
- ? Texto más pequeño

### Firmas
- ? Dos secciones (Emisor y Receptor)
- ? Línea para firma
- ? Texto descriptivo

### Pie de Página
- ? Paginación (Página X de Y)
- ? Fecha y hora de generación
- ? Centrado
- ? Texto pequeño gris

---

## ?? Código Frontend

### React Example

```jsx
import { useState } from 'react';

const RemitoActions = ({ remitoId, tipo }) => {
  const [loading, setLoading] = useState(false);

  const imprimirPDF = async () => {
    setLoading(true);
    try {
      const url = `/api/remitos/${tipo}/${remitoId}/pdf`;
      
      // Opción 1: Abrir en nueva ventana
      window.open(url, '_blank');
      
      // Opción 2: Descargar
      const response = await fetch(url);
      const blob = await response.blob();
      const downloadUrl = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = `Remito_${tipo}_${remitoId}.pdf`;
      link.click();
      
    } catch (error) {
      console.error('Error al generar PDF:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <button onClick={imprimirPDF} disabled={loading}>
      {loading ? 'Generando PDF...' : 'Imprimir Remito'}
    </button>
  );
};
```

### Angular Example

```typescript
import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-remito-actions',
  template: `
    <button (click)="imprimirPDF()" [disabled]="loading">
      {{ loading ? 'Generando PDF...' : 'Imprimir Remito' }}
    </button>
  `
})
export class RemitoActionsComponent {
  loading = false;

  constructor(private http: HttpClient) {}

  imprimirPDF() {
    this.loading = true;
    const url = `/api/remitos/ingreso/5/pdf`;
    
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        this.loading = false;
      },
      error: (error) => {
        console.error('Error:', error);
        this.loading = false;
      }
    });
  }
}
```

### Vue Example

```vue
<template>
  <button @click="imprimirPDF" :disabled="loading">
    {{ loading ? 'Generando PDF...' : 'Imprimir Remito' }}
  </button>
</template>

<script>
export default {
  data() {
    return {
      loading: false
    };
  },
  methods: {
    async imprimirPDF() {
      this.loading = true;
      try {
        const response = await fetch('/api/remitos/ingreso/5/pdf');
        const blob = await response.blob();
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
      } catch (error) {
        console.error('Error:', error);
      } finally {
        this.loading = false;
      }
    }
  }
};
</script>
```

---

## ?? Personalización

### Cambiar Logo de la Empresa

Editar `PdfService.cs` en `ComposeHeader`:

```csharp
private void ComposeHeader(IContainer container)
{
    container.Row(row =>
    {
        row.RelativeItem().Column(column =>
        {
            column.Item().Text("TU EMPRESA").Bold().FontSize(20);
            column.Item().Text("Tu slogan").FontSize(10);
            column.Item().Text("www.tuempresa.com").FontSize(8);
        });

        // Agregar logo real
        row.ConstantItem(100).Height(50).Image("ruta/al/logo.png");
    });
}
```

### Cambiar Colores

```csharp
// Azul actual
.FontColor(Colors.Blue.Medium)
.Background(Colors.Blue.Medium)

// Cambiar a verde
.FontColor(Colors.Green.Darken2)
.Background(Colors.Green.Medium)

// Colores personalizados
.FontColor("#FF5733")
.Background("#3498DB")
```

### Agregar Más Campos

En `ComposeContent`:

```csharp
// Agregar campo personalizado
if (!string.IsNullOrEmpty(remito.CampoPersonalizado))
{
    col.Item().Text($"Campo: {remito.CampoPersonalizado}");
}
```

---

## ? Testing

### Test Manual con Swagger

1. Ejecutar: `dotnet run`
2. Abrir: `https://localhost:5001/swagger`
3. Ir a `/api/remitos/ingreso/{id}/pdf`
4. Click "Try it out"
5. Ingresar ID de remito existente
6. Click "Execute"
7. Click "Download file" en la respuesta

### Test con cURL

```bash
# Generar y guardar PDF
curl -X GET "https://localhost:5001/api/remitos/ingreso/5/pdf" \
  -H "Authorization: Bearer {token}" \
  --output remito_5.pdf \
  -k

# Verificar que es PDF válido
file remito_5.pdf
# Output: remito_5.pdf: PDF document, version 1.4
```

### Test Automatizado

```csharp
[Fact]
public async Task ImprimirRemitoIngreso_RemitoExiste_RetornaPdf()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/remitos/ingreso/5/pdf");
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("application/pdf", response.Content.Headers.ContentType.MediaType);
    
    var bytes = await response.Content.ReadAsByteArrayAsync();
    Assert.True(bytes.Length > 0);
    
    // Verificar que es PDF válido (magic bytes)
    var pdfHeader = System.Text.Encoding.ASCII.GetString(bytes[..4]);
    Assert.Equal("%PDF", pdfHeader);
}

[Fact]
public async Task ImprimirRemitoIngreso_RemitoNoExiste_Retorna404()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/remitos/ingreso/999/pdf");
    
    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

---

## ?? Ventajas de QuestPDF

### vs iTextSharp/iText7
? **Licencia más simple** - Community gratuita
? **API moderna** - Fluent API tipo LINQ
? **Sin legacy code** - Diseñado para .NET moderno
? iTextSharp/iText7 tiene licencia AGPL (restrictiva)

### vs DinkToPdf (wkhtmltopdf)
? **Nativo .NET** - No requiere binarios externos
? **Cross-platform** - Funciona en Windows/Linux/Mac
? **Más control** - Diseño programático
? DinkToPdf requiere wkhtmltopdf.exe

### vs Reporting Services (SSRS)
? **Más ligero** - No requiere servidor SQL
? **Más flexible** - Control total del diseño
? **Más rápido** - Generación en memoria
? SSRS requiere infraestructura pesada

---

## ?? Performance

### Tiempos de Generación (Estimados)

| Remito | Items | Tiempo | Tamaño PDF |
|--------|-------|--------|------------|
| Pequeño | 1-5 items | ~50ms | ~15 KB |
| Mediano | 10-20 items | ~100ms | ~25 KB |
| Grande | 50+ items | ~200ms | ~50 KB |

### Optimizaciones

```csharp
// Cachear configuración de QuestPDF
private static readonly Document.Configuration = ...;

// Reusar DTOs
private readonly ObjectPool<RemitoPdfDto> _dtoPool;

// Generar en paralelo (múltiples remitos)
var tasks = remitos.Select(id => _pdfService.GenerarPdfRemitoIngreso(id));
var pdfs = await Task.WhenAll(tasks);
```

---

## ? Estado Final

```
??????????????????????????????????????????????
?                                            ?
?   ?? SISTEMA DE PDF IMPLEMENTADO ?       ?
?                                            ?
?  ? Paquete:           QuestPDF 2024.12.3 ?
?  ? Interfaz:          IPdfService        ?
?  ? Implementación:    PdfService         ?
?  ? DTOs:              2 records          ?
?  ? Endpoints:         2 nuevos           ?
?  ? Plantilla:         Profesional        ?
?  ? Diseño:            A4, colores, logos ?
?  ? Compilación:       EXITOSA            ?
?  ? Documentación:     Completa           ?
?                                            ?
??????????????????????????????????????????????
```

---

**¡Sistema de PDFs listo para producción!** ??

**Genera PDFs profesionales de remitos de ingreso y egreso**

**Frontend:** Puede descargar, mostrar en iframe, o imprimir directamente

**Personalizable:** Fácil cambiar colores, logos, agregar campos
