using CigralBackend.Domain.Bases;
using System;

namespace CigralBackend.Domain
{
    public class RegistroAuditoria : EntityBase
    {

        /// <summary>Ejemplo: "Consignacion", "MovimientoStock", "Producto"</summary>
        public string Entidad { get; set; }

        /// <summary>El ID del registro que fue modificado</summary>
        public string EntidadId { get; set; }

        /// <summary>Ejemplo: "Update", "Insert", "Delete"</summary>
        public string Accion { get; set; }

        /// <summary>El nombre del campo que cambió. Ej: "Estado", "Cantidad"</summary>
        public string Campo { get; set; }

        public string? ValorAnterior { get; set; }
        public string ValorActual { get; set; }

        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
    }
}