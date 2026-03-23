using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Servicio para consultar la auditoría de movimientos de stock.
    /// </summary>
    public class MovimientoStockService : IMovimientoStockService
    {
        private readonly IRepository _repository;

        public MovimientoStockService(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Obtiene movimientos de stock filtrados con paginación.
        /// </summary>
        public async Task<PagedResult<MovimientoStockResponse>> GetMovimientos(MovimientoStockFilters filters)
        {
            var resultadoEntidad = await _repository.GetFiltered<MovimientoStock>(
                predicate: m =>
                    (string.IsNullOrEmpty(filters.NombreProducto) || m.Producto.Nombre.Contains(filters.NombreProducto)) &&
                    (!filters.DepositoId.HasValue || m.DepositoId == filters.DepositoId.Value) &&
                    (string.IsNullOrEmpty(filters.CodigoLote) || m.Lote.CodigoLote.Contains(filters.CodigoLote)) &&
                    (string.IsNullOrEmpty(filters.NumeroSerie) || m.NumeroSerie.Contains(filters.NumeroSerie)) &&
                    (!filters.Tipo.HasValue || m.Tipo == filters.Tipo.Value) &&
                    (string.IsNullOrEmpty(filters.NroRemito) ||
                    m.RemitoEgreso.NumeroRemito.Contains(filters.NroRemito) ||
                    m.RemitoIngreso.NumeroRemito.Contains(filters.NroRemito)) &&
                    (string.IsNullOrEmpty(filters.ComprobanteAsociado) ||
                    m.RemitoEgreso.ComprobanteAsociado.Contains(filters.ComprobanteAsociado) ||
                    m.RemitoIngreso.ComprobanteAsociado.Contains(filters.ComprobanteAsociado)) &&
                    (!filters.FechaDesde.HasValue || m.FechaMovimiento >= filters.FechaDesde.Value) &&
                    (!filters.FechaHasta.HasValue || m.FechaMovimiento <= filters.FechaHasta.Value),
                orderBy: q => q.OrderByDescending(m => m.FechaMovimiento),
                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize,
                include: new[] { "Producto", "Deposito", "Lote", "RemitoIngreso", "RemitoEgreso" }
            );

            var itemsDto = resultadoEntidad.Items.Select(m => new MovimientoStockResponse(
                Id: m.Id,
                Tipo: m.Tipo.ToString(),
                FechaMovimiento: m.FechaMovimiento,
                ProductoId: m.ProductoId,
                ProductoNombre: m.Producto?.Nombre ?? "Sin Nombre",
                DepositoId: m.DepositoId,
                DepositoNombre: m.Deposito?.Nombre ?? "Sin Depósito",
                LoteId: m.LoteId,
                CodigoLote: m.Lote?.CodigoLote,
                NumeroSerie: m.NumeroSerie,
                CodigoGenerico: m.Producto?.CodigoGenerico ?? "Sin Código Genérico",
                Cantidad: m.Cantidad,
                StockAnterior: m.StockAnterior,
                StockNuevo: m.StockNuevo,
                RemitoIngresoId: m.RemitoIngresoId,
                RemitoEgresoId: m.RemitoEgresoId,
                NroRemito: m.RemitoEgreso?.NumeroRemito ?? m.RemitoIngreso?.NumeroRemito,
                ComprobanteAsociado: m.RemitoEgreso?.ComprobanteAsociado ?? m.RemitoIngreso?.ComprobanteAsociado,
                Usuario: m.Usuario,
                Observaciones: m.Observaciones
            )).ToList();

            return new PagedResult<MovimientoStockResponse>
            {
                Items = itemsDto,
                TotalCount = resultadoEntidad.TotalCount,
                PageNumber = resultadoEntidad.PageNumber,
                PageSize = resultadoEntidad.PageSize
            };
        }

        /// <summary>
        /// Obtiene un movimiento de stock por su ID.
        /// </summary>
        public async Task<MovimientoStockResponse> GetMovimientoById(int id)
        {
            var movimiento = await _repository.GetById<MovimientoStock>(id, "Producto", "Deposito", "Lote", "RemitoIngreso", "RemitoEgreso");

            if (movimiento == null)
            {
                throw new NotFoundException(nameof(MovimientoStock), id);
            }

            return new MovimientoStockResponse(
                Id: movimiento.Id,
                Tipo: movimiento.Tipo.ToString(),
                FechaMovimiento: movimiento.FechaMovimiento,
                ProductoId: movimiento.ProductoId,
                ProductoNombre: movimiento.Producto?.Nombre ?? "Sin Nombre",
                DepositoId: movimiento.DepositoId,
                DepositoNombre: movimiento.Deposito?.Nombre ?? "Sin Depósito",
                LoteId: movimiento.LoteId,
                CodigoLote: movimiento.Lote?.CodigoLote,
                NumeroSerie: movimiento.NumeroSerie,
                CodigoGenerico: movimiento.Producto?.CodigoGenerico ?? "Sin Código Genérico",
                Cantidad: movimiento.Cantidad,
                StockAnterior: movimiento.StockAnterior,
                StockNuevo: movimiento.StockNuevo,
                RemitoIngresoId: movimiento.RemitoIngresoId,
                RemitoEgresoId: movimiento.RemitoEgresoId,
                NroRemito: movimiento.RemitoEgreso?.NumeroRemito ?? movimiento.RemitoIngreso?.NumeroRemito,
                ComprobanteAsociado: movimiento.RemitoEgreso?.ComprobanteAsociado ?? movimiento.RemitoIngreso?.ComprobanteAsociado,
                Usuario: movimiento.Usuario,
                Observaciones: movimiento.Observaciones
            );
        }
    }
}
