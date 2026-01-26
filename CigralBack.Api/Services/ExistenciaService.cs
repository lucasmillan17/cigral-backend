using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Servicio de aplicacion para operaciones de existencias.
    /// </summary>
    public class ExistenciaService : IExistenciaService
    {
        private readonly IRepository _repository;

        public ExistenciaService(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Genera un ExistenciaModelResponse desde una entidad Existencia.
        /// </summary>
        private ExistenciaModelResponse ResponseGenerator(Existencia e, Producto producto, Deposito deposito, Lote? lote)
        {
            return new ExistenciaModelResponse(
                e.Id,
                e.ProductoId,
                producto.Nombre,
                producto.GTIN,
                e.DepositoId,
                deposito.Nombre,
                e.LoteId ?? 0,
                lote?.CodigoLote ?? "Sin Código de Lote",
                e.NumSerie ?? "Sin Número de Serie",
                lote?.FechaVencimiento ?? e.FechaVencimiento,
                e.Cantidad
            );
        }

        /// <summary>
        /// Aumenta el stock de un producto. Si la existencia no existe, la crea. Si existe, suma la cantidad.
        /// </summary>
        /// <param name="r">Datos del movimiento de stock</param>
        /// <returns>La existencia actualizada o creada</returns>
        /// <exception cref="NotFoundException">Si el producto, deposito o lote no existen</exception>
        /// <exception cref="DomainException">Si las validaciones de negocio fallan</exception>
        public async Task<ExistenciaModelResponse> AumentarStock(ExistenciaModelRequest r)
        {
            // Validar cantidad
            if (r.Cantidad <= 0)
            {
                throw new DomainException(
                    DomainErrorCode.CantidadInvalida,
                    "La cantidad debe ser mayor a 0."
                );
            }

            // Validar que el producto exista
            var producto = await _repository.GetById<Producto>(r.ProductoId);
            if (producto == null)
            {
                throw new NotFoundException(nameof(Producto), r.ProductoId);
            }

            // Validar producto unitario
            if (producto.EsUnitario && r.Cantidad != 1)
            {
                throw new DomainException(
                    DomainErrorCode.ProductoUnitarioCantidadInvalida,
                    "No se puede aumentar el stock de un producto unitario con cantidad distinta de 1."
                );
            }

            // Validar que el deposito exista
            var deposito = await _repository.GetById<Deposito>(r.DepositoId);
            if (deposito == null)
            {
                throw new NotFoundException(nameof(Deposito), r.DepositoId);
            }

            // Validar que el lote exista si se especifica
            Lote? lote = null;
            if (r.LoteId.HasValue)
            {
                lote = await _repository.GetById<Lote>(r.LoteId.Value);
                if (lote == null)
                {
                    throw new NotFoundException(nameof(Lote), r.LoteId.Value);
                }

                // Validar que el lote no esté vencido
                if (lote.FechaVencimiento < DateTime.Now)
                {
                    throw new DomainException(
                        DomainErrorCode.LoteVencido,
                        $"El lote '{lote.CodigoLote}' está vencido. Fecha de vencimiento: {lote.FechaVencimiento:dd/MM/yyyy}"
                    );
                }
            }

            // Validar número de serie duplicado si se especifica
            if (!string.IsNullOrEmpty(r.NumSerie))
            {
                var existenciaConMismoNumSerie = await _repository.First<Existencia>(
                    e => e.NumSerie == r.NumSerie && e.ProductoId == r.ProductoId
                );
                if (existenciaConMismoNumSerie != null)
                {
                    throw new DomainException(
                        DomainErrorCode.SerieDuplicada,
                        $"Ya existe una existencia del producto '{producto.Nombre}' con el número de serie '{r.NumSerie}'."
                    );
                }
            }

            // Buscar existencia existente (sin número de serie para permitir sumar cantidades)
            var existencia = await _repository.First<Existencia>(
                e => e.ProductoId == r.ProductoId &&
                     e.DepositoId == r.DepositoId &&
                     e.LoteId == r.LoteId &&
                     (string.IsNullOrEmpty(r.NumSerie) || e.NumSerie == r.NumSerie)
            );

            if (existencia != null)
            {
                // Aumentar cantidad existente
                existencia.Cantidad += r.Cantidad;
                await _repository.Update(existencia);
            }
            else
            {
                // Crear nueva existencia
                existencia = new Existencia
                {
                    ProductoId = r.ProductoId,
                    DepositoId = r.DepositoId,
                    LoteId = r.LoteId,
                    NumSerie = r.NumSerie,
                    FechaVencimiento = r.FechaVencimiento,
                    Cantidad = r.Cantidad
                };
                existencia = await _repository.Add(existencia);
            }

            return ResponseGenerator(existencia, producto, deposito, lote);
        }

        /// <summary>
        /// Disminuye el stock de un producto. Si la existencia queda en 0, se mantiene el registro.
        /// </summary>
        /// <param name="r">Datos del movimiento de stock</param>
        /// <returns>La existencia actualizada</returns>
        /// <exception cref="NotFoundException">Si el producto, deposito, lote o existencia no existen</exception>
        /// <exception cref="DomainException">Si las validaciones de negocio fallan</exception>
        public async Task<ExistenciaModelResponse> DisminuirStock(ExistenciaModelRequest r)
        {
            // Validar cantidad
            if (r.Cantidad <= 0)
            {
                throw new DomainException(
                    DomainErrorCode.CantidadInvalida,
                    "La cantidad debe ser mayor a 0."
                );
            }

            // Validar que el producto exista
            var producto = await _repository.GetById<Producto>(r.ProductoId);
            if (producto == null)
            {
                throw new NotFoundException(nameof(Producto), r.ProductoId);
            }

            // Validar producto unitario
            if (producto.EsUnitario && r.Cantidad != 1)
            {
                throw new DomainException(
                    DomainErrorCode.ProductoUnitarioCantidadInvalida,
                    "No se puede disminuir el stock de un producto unitario con cantidad distinta de 1."
                );
            }

            // Validar que el deposito exista
            var deposito = await _repository.GetById<Deposito>(r.DepositoId);
            if (deposito == null)
            {
                throw new NotFoundException(nameof(Deposito), r.DepositoId);
            }

            // Validar que el lote exista si se especifica
            Lote? lote = null;
            if (r.LoteId.HasValue)
            {
                lote = await _repository.GetById<Lote>(r.LoteId.Value);
                if (lote == null)
                {
                    throw new NotFoundException(nameof(Lote), r.LoteId.Value);
                }
            }

            // Buscar existencia existente
            var existencia = await _repository.First<Existencia>(
                e => e.ProductoId == r.ProductoId &&
                     e.DepositoId == r.DepositoId &&
                     e.LoteId == r.LoteId &&
                     (string.IsNullOrEmpty(r.NumSerie) || e.NumSerie == r.NumSerie)
            );

            if (existencia == null)
            {
                throw new NotFoundException(
                    nameof(Existencia),
                    $"Producto {r.ProductoId} en Depósito {r.DepositoId}"
                );
            }

            // Validar stock suficiente
            if (existencia.Cantidad < r.Cantidad)
            {
                throw new DomainException(
                    DomainErrorCode.StockInsuficiente,
                    $"Stock insuficiente. Disponible: {existencia.Cantidad}, Solicitado: {r.Cantidad}"
                );
            }

            // Disminuir cantidad
            existencia.Cantidad -= r.Cantidad;
            await _repository.Update(existencia);

            return ResponseGenerator(existencia, producto, deposito, lote);
        }

        /// <summary>
        /// Obtiene una existencia por su ID.
        /// </summary>
        /// <param name="id">ID de la existencia</param>
        /// <returns>La existencia encontrada</returns>
        /// <exception cref="NotFoundException">Si la existencia no existe</exception>
        public async Task<ExistenciaModelResponse> GetExistenciaById(int id)
        {
            var existencia = await _repository.GetById<Existencia>(id, "Producto", "Deposito", "Lote");

            if (existencia == null)
            {
                throw new NotFoundException(nameof(Existencia), id);
            }

            return new ExistenciaModelResponse(
                existencia.Id,
                existencia.ProductoId,
                existencia.Producto?.Nombre ?? "Sin Nombre",
                existencia.Producto?.GTIN ?? "Sin GTIN",
                existencia.DepositoId,
                existencia.Deposito?.Nombre ?? "Sin Depósito",
                existencia.LoteId ?? 0,
                existencia.Lote?.CodigoLote ?? "Sin Código de Lote",
                existencia.NumSerie ?? "Sin Número de Serie",
                existencia.Lote?.FechaVencimiento ?? existencia.FechaVencimiento,
                existencia.Cantidad
            );
        }

        /// <summary>
        /// Obtiene existencias filtradas con paginacion.
        /// </summary>
        /// <param name="filters">Filtros a aplicar</param>
        /// <returns>Resultado paginado de existencias</returns>
        public async Task<PagedResult<ExistenciaModelResponse>> GetExistencias(ExistenciaFilters filters)
        {
            var resultadoEntidad = await _repository.GetFiltered<Existencia>(
                predicate: e =>
                    (!filters.ProductoId.HasValue || e.ProductoId == filters.ProductoId.Value) &&
                    (!filters.DepositoId.HasValue || e.DepositoId == filters.DepositoId.Value) &&
                    (!filters.LoteId.HasValue || e.LoteId == filters.LoteId.Value),
                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize,
                include: new[] { "Producto", "Deposito", "Lote" }
            );

            var itemsDto = resultadoEntidad.Items.Select(e => new ExistenciaModelResponse(
                e.Id,
                e.ProductoId,
                e.Producto?.Nombre ?? "Sin Nombre",
                e.Producto?.GTIN ?? "Sin GTIN",
                e.DepositoId,
                e.Deposito?.Nombre ?? "Sin Depósito",
                e.LoteId ?? 0,
                e.Lote?.CodigoLote ?? "Sin Código de Lote",
                e.NumSerie ?? "Sin Número de Serie",
                e.Lote?.FechaVencimiento ?? e.FechaVencimiento,
                e.Cantidad
            )).ToList();

            return new PagedResult<ExistenciaModelResponse>
            {
                Items = itemsDto,
                TotalCount = resultadoEntidad.TotalCount,
                PageNumber = resultadoEntidad.PageNumber,
                PageSize = resultadoEntidad.PageSize
            };
        }

        /// <summary>
        /// Elimina una existencia del sistema (solo si cantidad = 0).
        /// </summary>
        /// <param name="id">ID de la existencia a eliminar</param>
        /// <exception cref="NotFoundException">Si la existencia no existe</exception>
        /// <exception cref="DomainException">Si la existencia tiene stock</exception>
        public async Task DeleteExistencia(int id)
        {
            var existencia = await _repository.GetById<Existencia>(id);
            if (existencia == null)
            {
                throw new NotFoundException(nameof(Existencia), id);
            }

            // Solo permitir eliminar si no hay stock
            if (existencia.Cantidad > 0)
            {
                throw new DomainException(
                    DomainErrorCode.StockInsuficiente,
                    $"No se puede eliminar una existencia con stock. Cantidad actual: {existencia.Cantidad}"
                );
            }

            await _repository.Delete(existencia);
        }
    }
}