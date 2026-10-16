using CigralBackend.Application.Dtos;
using CigralBackend.Domain;
using CigralBackend.Domain.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>Tickets de soporte (ver TicketService).</summary>
    public interface ITicketService
    {
        Task<TicketDetalleResponse> Crear(TicketCreateRequest request, IReadOnlyList<TicketArchivo> fotos, UsuarioActual usuario);
        Task<PagedResult<TicketResumenResponse>> Listar(TicketFilters filtros, UsuarioActual usuario);
        Task<TicketDetalleResponse> Obtener(int id, UsuarioActual usuario);
        Task<TicketAdjunto> ObtenerAdjunto(int ticketId, int adjuntoId, UsuarioActual usuario);
        Task<TicketDetalleResponse> Comentar(int id, string texto, UsuarioActual usuario);
        Task<TicketDetalleResponse> CambiarEstado(int id, EstadoTicket estado, UsuarioActual usuario);
        Task<TicketPendientesResponse> Pendientes(UsuarioActual usuario);
    }
}
