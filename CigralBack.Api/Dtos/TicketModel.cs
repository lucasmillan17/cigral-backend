using CigralBackend.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    /// <summary>
    /// Datos de un ticket nuevo (las fotos llegan aparte, en el mismo formulario multipart).
    /// Los atributos van a las propiedades porque el controller valida el JSON a mano (Validator).
    /// </summary>
    public record TicketCreateRequest
    (
        TipoTicket Tipo,
        [property: Required(ErrorMessage = "El título es obligatorio")]
        [property: MaxLength(200, ErrorMessage = "El título no puede superar los 200 caracteres")]
        string Titulo,
        [property: MaxLength(4000)] string? Descripcion,
        [property: MaxLength(2000)] string? CodigoCrudo,
        [property: MaxLength(8000)] string? ResultadoParserJson,
        [property: MaxLength(50)] string? Pantalla,
        int? DepositoId,
        int? ProductoId,
        [property: MaxLength(100)] string? LoteEtiqueta,
        [property: MaxLength(100)] string? SerieEtiqueta,
        [property: MaxLength(30)] string? VencimientoEtiqueta,
        [property: MaxLength(30)] string? VersionApp,
        [property: MaxLength(20000, ErrorMessage = "El contexto técnico es demasiado largo")] string? ContextoJson = null
    );

    public record TicketComentarioRequest(
        [Required(ErrorMessage = "El comentario no puede estar vacío")]
        [MaxLength(4000)]
        string Texto
    );

    public record TicketEstadoRequest(EstadoTicket Estado);

    public record TicketFilters
    (
        EstadoTicket? Estado,
        TipoTicket? Tipo,
        string? Texto,
        [Range(1, int.MaxValue)] int PageNumber = 1,
        [Range(1, 100)] int PageSize = 25
    );

    /// <summary>Fila de la bandeja.</summary>
    public record TicketResumenResponse(
        int Id,
        TipoTicket Tipo,
        EstadoTicket Estado,
        string Titulo,
        string? Pantalla,
        string CreadoPorNombre,
        DateTime FechaCreacion,
        DateTime FechaActualizacion,
        int CantidadAdjuntos,
        int CantidadComentarios
    );

    public record TicketComentarioResponse(int Id, string UsuarioNombre, DateTime Fecha, string Texto, bool EsDeSoporte);

    public record TicketAdjuntoResponse(int Id, string NombreArchivo, string ContentType, int Tamano);

    public record TicketDetalleResponse(
        int Id,
        TipoTicket Tipo,
        EstadoTicket Estado,
        string Titulo,
        string? Descripcion,
        string? CodigoCrudo,
        string? ResultadoParserJson,
        string? Pantalla,
        int? DepositoId,
        int? ProductoId,
        string? LoteEtiqueta,
        string? SerieEtiqueta,
        string? VencimientoEtiqueta,
        int CreadoPorId,
        string CreadoPorNombre,
        DateTime FechaCreacion,
        DateTime FechaActualizacion,
        string? VersionApp,
        List<TicketComentarioResponse> Comentarios,
        List<TicketAdjuntoResponse> Adjuntos,
        string? ContextoJson
    );

    public record TicketPendientesResponse(int Abiertos, int EnRevision);

    /// <summary>Foto recibida en el formulario, ya leída en memoria.</summary>
    public record TicketArchivo(string NombreArchivo, string ContentType, byte[] Datos);

    /// <summary>Quién hace la operación (lo arma el controller a partir del token).</summary>
    public record UsuarioActual(int Id, string Nombre, bool EsSoporte);

    public record RolesUsuarioRequest(
        [Required] List<string> Roles
    );
}
