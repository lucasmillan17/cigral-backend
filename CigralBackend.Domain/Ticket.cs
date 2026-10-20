using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;

namespace CigralBackend.Domain
{
    /// <summary>
    /// Reporte de un usuario para el equipo de soporte. El primer tipo es "código que no carga bien":
    /// guarda el texto exacto que mandó el escáner y lo que entendió el parser, para poder reproducirlo.
    /// </summary>
    public class Ticket : EntityBase
    {
        public TipoTicket Tipo { get; set; }
        public EstadoTicket Estado { get; set; }

        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }

        /// <summary>Texto exacto recibido del escáner (incluye los separadores GS).</summary>
        public string? CodigoCrudo { get; set; }

        /// <summary>Respuesta del parser para ese código, tal como la recibió la app (JSON).</summary>
        public string? ResultadoParserJson { get; set; }

        /// <summary>Pantalla desde la que se reportó (Ingresos, Egresos...).</summary>
        public string? Pantalla { get; set; }

        public int? DepositoId { get; set; }
        public int? ProductoId { get; set; }

        // Lo que dice la etiqueta impresa, según el usuario (para comparar con el parser).
        public string? LoteEtiqueta { get; set; }
        public string? SerieEtiqueta { get; set; }
        public string? VencimientoEtiqueta { get; set; }

        public int CreadoPorId { get; set; }
        public string CreadoPorNombre { get; set; } = "";
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }

        public string? VersionApp { get; set; }

        /// <summary>Contexto técnico de un reporte de interfaz (JSON): resolución, zoom, errores recientes, recorrido.</summary>
        public string? ContextoJson { get; set; }

        public List<TicketComentario> Comentarios { get; set; } = new();
        public List<TicketAdjunto> Adjuntos { get; set; } = new();
    }

    public class TicketComentario : EntityBase
    {
        public int TicketId { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Texto { get; set; } = "";

        /// <summary>true si lo escribió alguien con rol Soporte (se muestra distinto en la app).</summary>
        public bool EsDeSoporte { get; set; }
    }

    /// <summary>Foto adjunta a un ticket (se guarda en la base: no hace falta un volumen en el servidor).</summary>
    public class TicketAdjunto : EntityBase
    {
        public int TicketId { get; set; }
        public string NombreArchivo { get; set; } = "";
        public string ContentType { get; set; } = "";
        public byte[] Datos { get; set; } = Array.Empty<byte>();
        public int Tamano { get; set; }
        public DateTime Fecha { get; set; }
    }

    public enum TipoTicket
    {
        /// <summary>Un código de barras o QR que no carga bien producto, lote, serie o vencimiento.</summary>
        Codigo = 1,

        /// <summary>Un problema de la interfaz de la app, reportado con captura y contexto técnico.</summary>
        Interfaz = 2,

        /// <summary>Reservado: datos a corregir (stock, lotes, series, remitos).</summary>
        Datos = 3
    }

    public enum EstadoTicket
    {
        Abierto = 1,
        EnRevision = 2,
        Resuelto = 3,
        Descartado = 4
    }

    /// <summary>Roles de usuario (ASP.NET Identity). Se agregarán más a futuro (depósito, caja...).</summary>
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Soporte = "Soporte";

        public static readonly string[] Todos = { Administrador, Soporte };
    }
}
