using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Infraestructure.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Piezas comunes de las correcciones de soporte (existencias, remitos): motivo obligatorio,
    /// texto de los movimientos y comentario automático en el ticket.
    /// </summary>
    internal static class CorreccionSoporte
    {
        /// <summary>Devuelve el motivo limpio; exige ticket o motivo.</summary>
        public static string? Motivo(int? ticketId, string? motivo)
        {
            var limpio = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
            if (ticketId == null && limpio == null)
                throw new DomainException(DomainErrorCode.CorreccionSinMotivo, "Indicá el ticket o el motivo de la corrección.");
            return limpio;
        }

        public static async Task<Ticket?> Ticket(IRepository repository, int? ticketId)
            => ticketId.HasValue
                ? await repository.GetById<Ticket>(ticketId.Value) ?? throw new NotFoundException(nameof(Ticket), ticketId.Value)
                : null;

        /// <summary>Observaciones de los movimientos de stock: "Corrección de soporte · Remito R-1 · Ticket #3 · motivo".</summary>
        public static string Observaciones(Ticket? ticket, string? motivo, string? referencia = null)
        {
            var texto = "Corrección de soporte"
                + (referencia != null ? $" · {referencia}" : "")
                + (ticket != null ? $" · Ticket #{ticket.Id}" : "")
                + (motivo != null ? $" · {motivo}" : "");
            return texto.Length > 500 ? texto[..500] : texto;
        }

        public static string Valor(string? v) => string.IsNullOrEmpty(v) ? "(vacío)" : v;

        public static string Fecha(DateTime? f) => Valor(f?.ToString("dd/MM/yyyy"));

        /// <summary>Deja el resumen de la corrección como comentario de soporte en el ticket.</summary>
        public static async Task Comentar(IRepository repository, IHttpContextAccessor http, Ticket ticket, string texto, string? motivo)
        {
            var usuario = http.HttpContext?.User;
            await repository.Add(new TicketComentario
            {
                TicketId = ticket.Id,
                UsuarioNombre = usuario?.FindFirst("nombreCompleto")?.Value ?? usuario?.Identity?.Name ?? "Sistema",
                Fecha = DateTime.Now,
                Texto = texto + (motivo != null ? $"\nMotivo: {motivo}" : ""),
                EsDeSoporte = true,
            });
            ticket.FechaActualizacion = DateTime.Now;
            await repository.Update(ticket);
        }
    }
}
