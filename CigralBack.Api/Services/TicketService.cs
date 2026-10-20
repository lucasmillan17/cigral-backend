using CigralBackend.Application.Dtos;
using CigralBackend.Domain;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database;
using CigralBackend.Infraestructure.Database.Interfaces;
using CigralBackend.Infraestructure.Services;
using CigralBackend.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Tickets de soporte. Quien tiene el rol Soporte ve y gestiona todos; el resto ve y comenta
    /// solo los suyos (los ajenos se responden como "no existe" para no revelar nada).
    /// </summary>
    public class TicketService : ITicketService
    {
        public const int MaxFotos = 5;
        public const int MaxBytesFoto = 3 * 1024 * 1024;

        private readonly IRepository _repository;
        private readonly IEmailService _email;

        public TicketService(IRepository repository, IEmailService email)
        {
            _repository = repository;
            _email = email;
        }

        public async Task<TicketDetalleResponse> Crear(TicketCreateRequest r, IReadOnlyList<TicketArchivo> fotos, UsuarioActual usuario)
        {
            if (fotos.Count > MaxFotos)
                throw new DomainException(Domain.Enums.DomainErrorCode.UnknownError, $"Se pueden adjuntar hasta {MaxFotos} fotos.");
            foreach (var f in fotos)
            {
                if (!f.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    throw new DomainException(Domain.Enums.DomainErrorCode.UnknownError, $"\"{f.NombreArchivo}\" no es una imagen.");
                if (f.Datos.Length > MaxBytesFoto)
                    throw new DomainException(Domain.Enums.DomainErrorCode.UnknownError, $"\"{f.NombreArchivo}\" supera los 3 MB.");
            }

            var ahora = DateTime.Now;
            var ticket = await _repository.Add(new Ticket
            {
                Tipo = r.Tipo,
                Estado = EstadoTicket.Abierto,
                Titulo = r.Titulo.Trim(),
                Descripcion = Limpio(r.Descripcion),
                CodigoCrudo = r.CodigoCrudo, // sin recortar: los espacios o separadores pueden ser el problema
                ResultadoParserJson = Limpio(r.ResultadoParserJson),
                Pantalla = Limpio(r.Pantalla),
                DepositoId = r.DepositoId,
                ProductoId = r.ProductoId,
                LoteEtiqueta = Limpio(r.LoteEtiqueta),
                SerieEtiqueta = Limpio(r.SerieEtiqueta),
                VencimientoEtiqueta = Limpio(r.VencimientoEtiqueta),
                CreadoPorId = usuario.Id,
                CreadoPorNombre = usuario.Nombre,
                FechaCreacion = ahora,
                FechaActualizacion = ahora,
                VersionApp = Limpio(r.VersionApp),
                ContextoJson = Limpio(r.ContextoJson),
            });

            var adjuntos = new List<TicketAdjunto>();
            foreach (var f in fotos)
            {
                adjuntos.Add(await _repository.Add(new TicketAdjunto
                {
                    TicketId = ticket.Id,
                    NombreArchivo = f.NombreArchivo,
                    ContentType = f.ContentType,
                    Datos = f.Datos,
                    Tamano = f.Datos.Length,
                    Fecha = ahora,
                }));
            }

            // El mail no demora la respuesta: si el SMTP tarda o falla, el ticket ya quedó guardado.
            _ = Task.Run(() => _email.EnviarTicketNuevoAsync(ticket, adjuntos));

            return await Obtener(ticket.Id, usuario);
        }

        public async Task<PagedResult<TicketResumenResponse>> Listar(TicketFilters f, UsuarioActual usuario)
        {
            var texto = f.Texto?.Trim();
            var consulta = _repository.Query<Ticket>()
                .Where(t => usuario.EsSoporte || t.CreadoPorId == usuario.Id)
                .Where(t => !f.Estado.HasValue || t.Estado == f.Estado.Value)
                .Where(t => !f.Tipo.HasValue || t.Tipo == f.Tipo.Value)
                .Where(t => string.IsNullOrEmpty(texto) ||
                    Busqueda.Coincide(t.Titulo + " " + (t.Descripcion ?? "") + " " + t.CreadoPorNombre + " " + (t.CodigoCrudo ?? ""), texto));

            var total = await consulta.CountAsync();
            var items = await consulta
                .OrderByDescending(t => t.FechaActualizacion)
                .Skip((f.PageNumber - 1) * f.PageSize)
                .Take(f.PageSize)
                .Select(t => new TicketResumenResponse(
                    t.Id, t.Tipo, t.Estado, t.Titulo, t.Pantalla, t.CreadoPorNombre, t.FechaCreacion, t.FechaActualizacion,
                    t.Adjuntos.Count, t.Comentarios.Count))
                .ToListAsync();

            return new PagedResult<TicketResumenResponse> { Items = items, TotalCount = total, PageNumber = f.PageNumber, PageSize = f.PageSize };
        }

        public async Task<TicketDetalleResponse> Obtener(int id, UsuarioActual usuario)
        {
            var t = await _repository.Query<Ticket>()
                .Where(x => x.Id == id && (usuario.EsSoporte || x.CreadoPorId == usuario.Id))
                .Select(x => new TicketDetalleResponse(
                    x.Id, x.Tipo, x.Estado, x.Titulo, x.Descripcion, x.CodigoCrudo, x.ResultadoParserJson, x.Pantalla,
                    x.DepositoId, x.ProductoId, x.LoteEtiqueta, x.SerieEtiqueta, x.VencimientoEtiqueta,
                    x.CreadoPorId, x.CreadoPorNombre, x.FechaCreacion, x.FechaActualizacion, x.VersionApp,
                    x.Comentarios.OrderBy(c => c.Fecha).Select(c => new TicketComentarioResponse(c.Id, c.UsuarioNombre, c.Fecha, c.Texto, c.EsDeSoporte)).ToList(),
                    x.Adjuntos.OrderBy(a => a.Id).Select(a => new TicketAdjuntoResponse(a.Id, a.NombreArchivo, a.ContentType, a.Tamano)).ToList(),
                    x.ContextoJson))
                .FirstOrDefaultAsync();

            return t ?? throw new NotFoundException("Ticket", id);
        }

        public async Task<TicketAdjunto> ObtenerAdjunto(int ticketId, int adjuntoId, UsuarioActual usuario)
        {
            await Obtener(ticketId, usuario); // valida que el usuario pueda ver el ticket
            var adjunto = await _repository.First<TicketAdjunto>(a => a.Id == adjuntoId && a.TicketId == ticketId);
            return adjunto ?? throw new NotFoundException("Adjunto", adjuntoId);
        }

        public async Task<TicketDetalleResponse> Comentar(int id, string texto, UsuarioActual usuario)
        {
            var ticket = await TicketEditable(id, usuario);
            await _repository.Add(new TicketComentario
            {
                TicketId = id,
                UsuarioNombre = usuario.Nombre,
                Fecha = DateTime.Now,
                Texto = texto.Trim(),
                EsDeSoporte = usuario.EsSoporte,
            });
            ticket.FechaActualizacion = DateTime.Now;
            await _repository.Update(ticket);
            return await Obtener(id, usuario);
        }

        public async Task<TicketDetalleResponse> CambiarEstado(int id, EstadoTicket estado, UsuarioActual usuario)
        {
            var ticket = await TicketEditable(id, usuario);
            if (ticket.Estado != estado)
            {
                ticket.Estado = estado;
                ticket.FechaActualizacion = DateTime.Now;
                await _repository.Update(ticket);
            }
            return await Obtener(id, usuario);
        }

        public async Task<TicketPendientesResponse> Pendientes(UsuarioActual usuario)
        {
            var visibles = _repository.Query<Ticket>().Where(t => usuario.EsSoporte || t.CreadoPorId == usuario.Id);
            return new TicketPendientesResponse(
                await visibles.CountAsync(t => t.Estado == EstadoTicket.Abierto),
                await visibles.CountAsync(t => t.Estado == EstadoTicket.EnRevision));
        }

        private async Task<Ticket> TicketEditable(int id, UsuarioActual usuario)
        {
            var ticket = await _repository.GetById<Ticket>(id);
            if (ticket == null || (!usuario.EsSoporte && ticket.CreadoPorId != usuario.Id))
                throw new NotFoundException("Ticket", id);
            return ticket;
        }

        private static string? Limpio(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }
}
