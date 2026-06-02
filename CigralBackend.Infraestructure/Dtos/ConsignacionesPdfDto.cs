using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Infraestructure.Dtos
{
    public record ReporteConsignacionesPdfDto(
    List<ConsignacionClientePdfDto> Clientes,
    DateTime FechaReporte
    );

    public record ConsignacionClientePdfDto(
        string RazonSocial,
        List<DetalleConsignacionPdfDto> Consignaciones
    )
    {
        // Las propiedades calculadas se pueden mantener dentro del cuerpo del record
        public int CantidadItems => Consignaciones.Count;
        public decimal TotalUnidades => Consignaciones.Sum(c => c.Cantidad);
    }

    public record DetalleConsignacionPdfDto(
        string ProductoNombre,
        string GtinODetalle,
        decimal Cantidad,
        DateTime FechaModificacion
    );
}
