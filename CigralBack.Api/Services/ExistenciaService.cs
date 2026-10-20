using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database;
using CigralBackend.Infraestructure.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ExistenciaService(IRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserName()
        {
            // Busca el claim configurado en AuthService.GenerateJwtToken
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistema";
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
                producto.CodigoInterno ?? "Sin Código Interno",
                e.DepositoId,
                deposito.Nombre,
                e.LoteId ?? 0,
                lote?.CodigoLote.ToUpper() ?? "Sin Código de Lote",
                e.NumSerie ?? "Sin Número de Serie",
                e.FechaVencimiento,
                e.Cantidad,
                e.InformacionAdicional
            );
        }

        /// <summary>
        /// Registra un movimiento de stock en la tabla de auditoría.
        /// </summary>
        private async Task RegistrarMovimiento(
            TipoMovimiento tipo,
            int productoId,
            int depositoId,
            int? loteId,
            string? numeroSerie,
            int cantidad,
            int stockAnterior,
            int stockNuevo,
            int? remitoIngresoId = null,
            int? remitoEgresoId = null,
            string? observaciones = null,
            string? usuario = null)
        {
            usuario = GetCurrentUserName();

            var movimiento = new MovimientoStock
            {
                Tipo = tipo,
                FechaMovimiento = DateTime.Now,
                ProductoId = productoId,
                DepositoId = depositoId,
                LoteId = loteId,
                NumeroSerie = numeroSerie,
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockNuevo = stockNuevo,
                RemitoIngresoId = remitoIngresoId,
                RemitoEgresoId = remitoEgresoId,
                Observaciones = observaciones,
                Usuario = usuario
            };

            await _repository.Add(movimiento);
        }

        /// <summary>
        /// Aumenta el stock de un producto. Si la existencia no existe, la crea. Si existe, suma la cantidad.
        /// Registra el movimiento en la auditoría.
        /// </summary>
        /// <param name="r">Datos del movimiento de stock</param>
        /// <param name="remitoIngresoId">ID del remito de ingreso (opcional)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <returns>La existencia actualizada o creada</returns>
        /// <exception cref="NotFoundException">Si el producto, deposito o lote no existen</exception>
        /// <exception cref="DomainException">Si las validaciones de negocio fallan</exception>
        public async Task<ExistenciaModelResponse> AumentarStock(
    ExistenciaModelRequest r,
    int? remitoIngresoId = null,
    string? observaciones = null)
        {
            var loteUpper = r.CodigoLote?.ToUpper();

            if (r.EsDevolucion) observaciones = "[DEVOLUCIÓN]";
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

            if (string.IsNullOrEmpty(r.NumSerie) && string.IsNullOrEmpty(loteUpper) && string.IsNullOrEmpty(producto.CodigoGenerico))
            {
                throw new DomainException(
                    DomainErrorCode.SerieYCodigoLoteNoEspecificados,
                    "Debe especificar al menos un número de serie, un código de lote o un código genérico para aumentar el stock."
                );
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
            if (!string.IsNullOrEmpty(loteUpper))
            {
                lote = await _repository.First<Lote>(l => l.CodigoLote == loteUpper);
                if (lote == null)
                {
                    // El lote es solo el código; cantidad y vencimiento viven en la existencia.
                    lote = new Lote { CodigoLote = loteUpper };

                    lote = await _repository.Add(lote);
                }
            }

            // Buscar existencia existente (Protegido contra Lote nulo)
            int? loteIdBusqueda = lote?.Id;
            var existencia = await _repository.First<Existencia>(
                e => e.ProductoId == r.ProductoId &&
                     e.DepositoId == r.DepositoId &&
                     e.LoteId == loteIdBusqueda &&
                     (string.IsNullOrEmpty(r.NumSerie) || e.NumSerie == r.NumSerie)
            );

            int stockAnterior = existencia?.Cantidad ?? 0;
            int stockNuevo;

            if (existencia != null)
            {
                // Aumentar cantidad de la existencia encontrada
                existencia.Cantidad += r.Cantidad;
                stockNuevo = existencia.Cantidad;
                await _repository.Update(existencia);
            }
            else
            {
                // Crear nueva existencia
                existencia = new Existencia
                {
                    ProductoId = r.ProductoId,
                    DepositoId = r.DepositoId,
                    Cantidad = r.Cantidad,
                    LoteId = lote?.Id,
                    FechaVencimiento = r.FechaVencimiento
                };

                if (!string.IsNullOrEmpty(r.NumSerie))
                {
                    existencia.NumSerie = r.NumSerie;
                }
                if (!string.IsNullOrEmpty(r.InformacionAdicional))
                {
                    existencia.InformacionAdicional = r.InformacionAdicional;
                }

                existencia = await _repository.Add(existencia);
                stockNuevo = existencia.Cantidad;
            }

            // Registrar movimiento en auditoría
            await RegistrarMovimiento(
                tipo: remitoIngresoId.HasValue ? TipoMovimiento.Ingreso : TipoMovimiento.AjustePositivo,
                productoId: r.ProductoId,
                depositoId: r.DepositoId,
                loteId: lote?.Id,
                numeroSerie: r.NumSerie,
                cantidad: r.Cantidad,
                stockAnterior: stockAnterior,
                stockNuevo: stockNuevo,
                remitoIngresoId: remitoIngresoId,
                observaciones: observaciones
            );

            return ResponseGenerator(existencia, producto, deposito, lote);
        }

        /// <summary>
        /// Disminuye el stock de un producto. Si la existencia queda en 0, se mantiene el registro.
        /// Registra el movimiento en la auditoría.
        /// </summary>
        /// <param name="r">Datos del movimiento de stock</param>
        /// <param name="remitoEgresoId">ID del remito de egreso (opcional)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <returns>La existencia actualizada</returns>
        /// <exception cref="NotFoundException">Si el producto, deposito, lote o existencia no existen</exception>
        /// <exception cref="DomainException">Si las validaciones de negocio fallan</exception>
        public async Task<ExistenciaModelResponse> DisminuirStock(
            ExistenciaModelRequest r,
            int? remitoEgresoId = null,
            string? observaciones = null)
        {

            var loteUpper = r.CodigoLote?.ToUpper();
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
            if (!string.IsNullOrEmpty(r.CodigoLote))
            {
                lote = await _repository.First<Lote>(l => l.CodigoLote == loteUpper)
                    ?? throw new NotFoundException(nameof(Lote), loteUpper);
                
            }

            // Buscar las existencias a descontar.
            // A propósito NO se filtra por depósito: así trabajan en Cigral (se descuenta del stock
            // del producto/lote/serie esté en el depósito que esté), a pedido del cliente.
            var existenciasADescontar = await _repository.GetFiltered<Existencia>(
                e => e.ProductoId == r.ProductoId &&
                     (string.IsNullOrEmpty(loteUpper) || e.LoteId == lote.Id) &&
                     (string.IsNullOrEmpty(r.NumSerie) || e.NumSerie == r.NumSerie),
                pageNumber: 1,
                pageSize: int.MaxValue // Todas: antes solo se consideraban las primeras 10
            );

            if (existenciasADescontar == null || !existenciasADescontar.Items.Any())
            {
                throw new NotFoundException(
                    nameof(Existencia),
                    $"Producto {r.ProductoId} en Depósito {r.DepositoId}"
                );
            }

            // --- NUEVA LÓGICA DE CONSIGNACIONES AQUÍ ---
            var idsExistencias = existenciasADescontar.Items.Select(e => e.Id).ToList();

            var consignacionesVinculadas = await _repository.GetFiltered<Consignacion>(
                c => idsExistencias.Contains(c.ExistenciaId),
                pageNumber: 1,
                pageSize: int.MaxValue
            );

            int stockFisico = existenciasADescontar.Items.Sum(e => e.Cantidad);
            int stockConsignado = consignacionesVinculadas.Items.Sum(c => c.Cantidad);
            int stockDisponibleReal = stockFisico - stockConsignado;

            // Validar stock real disponible (Físico - Consignado)
            if (stockDisponibleReal < r.Cantidad)
            {
                // NOTA: Asegúrate de tener StockReservadoEnConcesion en tu DomainErrorCode, 
                // o usa StockInsuficiente si prefieres.
                throw new DomainException(
                    DomainErrorCode.StockInsuficiente,
                    $"Stock insuficiente debido a consignaciones. Físico: {stockFisico}, En Consignación: {stockConsignado}. Disponible Real: {stockDisponibleReal}, Solicitado: {r.Cantidad}"
                );
            }

            int stockAnterior = stockFisico; // Para el registro de auditoría usamos el físico
            int cantidadACubrir = r.Cantidad;

            bool manejaTransaccionPropia = !_repository.HasActiveTransaction();
            var transaction = manejaTransaccionPropia ? await _repository.BeginTransaction() : null;

            try
            {
                foreach(var existencia in existenciasADescontar.Items) {

                    if (cantidadACubrir == 0) break;
                    if (existencia.Cantidad == 0) continue;

                    // Disminuir cantidad
                
                    if(existencia.Cantidad >= cantidadACubrir)
                    {
                        existencia.Cantidad -= cantidadACubrir;
                        cantidadACubrir = 0;
                    }
                    else
                    {
                        cantidadACubrir -= existencia.Cantidad;
                        existencia.Cantidad = 0;
                    }

                    if (existencia.Cantidad == 0)
                    {
                        await _repository.Delete(existencia);
                    }
                    else
                    {
                        await _repository.Update(existencia);
                    }

                }
                // Solo hacemos commit si NOSOTROS abrimos esta transacción
                if (manejaTransaccionPropia && transaction != null)
                {
                    await transaction.CommitAsync();
                }

            }
            catch
            {
                if (manejaTransaccionPropia && transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw new DomainException(
                    DomainErrorCode.StockInsuficiente,
                    $"Stock insuficiente. Disponible: {stockAnterior}, Solicitado: {r.Cantidad}"
                );
            }
            finally
            {
                // Limpiamos la memoria de la transacción si la creamos
                if (manejaTransaccionPropia && transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }

            var stockNuevo = stockAnterior - r.Cantidad;

            // Registrar movimiento en auditoría
            await RegistrarMovimiento(
                tipo: remitoEgresoId.HasValue ? TipoMovimiento.Egreso : TipoMovimiento.AjusteNegativo,
                productoId: r.ProductoId,
                depositoId: r.DepositoId,
                loteId: lote?.Id,
                numeroSerie: r.NumSerie,
                cantidad: -r.Cantidad, // Negativo para egreso
                stockAnterior: stockAnterior,
                stockNuevo: stockNuevo,
                remitoEgresoId: remitoEgresoId,
                observaciones: observaciones
            );

            var existenciaBase = existenciasADescontar.Items.First();

            return ResponseGenerator(existenciaBase, producto, deposito, lote);
        }

        /// <summary>
        /// Aplica varios movimientos sin remito en una sola transacción: se guardan todos o ninguno.
        /// Si una línea falla, devuelve su índice (0 = primera, igual que el remito de egreso) y el motivo.
        /// </summary>
        public async Task<ResultadoOperacion<List<ExistenciaModelResponse>>> AplicarMovimientos(
            List<ExistenciaModelRequest> movimientos,
            bool esAumento)
        {
            var resultados = new List<ExistenciaModelResponse>();
            using var transaction = await _repository.BeginTransaction();

            for (int i = 0; i < movimientos.Count; i++)
            {
                try
                {
                    resultados.Add(esAumento
                        ? await AumentarStock(movimientos[i])
                        : await DisminuirStock(movimientos[i]));
                }
                catch (Exception ex) when (ex is DomainException || ex is NotFoundException)
                {
                    await transaction.RollbackAsync();
                    return ResultadoOperacion<List<ExistenciaModelResponse>>.Fallo(
                        "No se guardó ningún movimiento porque una línea tiene un problema.",
                        new List<ErrorDetalleDto> { new ErrorDetalleDto(i, ex.Message) }
                    );
                }
            }

            await transaction.CommitAsync();
            return ResultadoOperacion<List<ExistenciaModelResponse>>.Ok(resultados);
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
                existencia.Producto?.CodigoInterno ?? "Sin Código Interno",
                existencia.DepositoId,
                existencia.Deposito?.Nombre ?? "Sin Depósito",
                existencia.LoteId ?? 0,
                existencia.Lote?.CodigoLote.ToUpper() ?? "Sin Código de Lote",
                existencia.NumSerie ?? "Sin Número de Serie",
                existencia.FechaVencimiento,
                existencia.Cantidad,
                existencia.InformacionAdicional
            );
        }

        /// <summary>
        /// Obtiene existencias filtradas con paginación.
        /// Ahora incluye filtros por fecha de vencimiento y días para vencer.
        /// </summary>
        /// <param name="filters">Filtros a aplicar</param>
        /// <returns>Resultado paginado de existencias</returns>
        public async Task<PagedResult<ExistenciaModelResponse>> GetExistencias(ExistenciaFilters filters)
        {
            var hoy = DateTime.Now.Date;
            var fechaLimiteVencimiento = filters.DiasParaVencer.HasValue
                ? hoy.AddDays(filters.DiasParaVencer.Value)
                : (DateTime?)null;

            Func<IQueryable<Existencia>, IOrderedQueryable<Existencia>> orderByLogic = q =>
            {
                return filters.OrdenarPor switch
                {
                    OrdenExistencia.NombreProducto => filters.EsDescendente
                        ? q.OrderByDescending(e => e.Producto.Nombre)
                        : q.OrderBy(e => e.Producto.Nombre),

                    OrdenExistencia.FechaVencimiento => filters.EsDescendente
                        ? q.OrderByDescending(e => e.FechaVencimiento)
                        : q.OrderBy(e => e.FechaVencimiento),

                    OrdenExistencia.Cantidad => filters.EsDescendente
                        ? q.OrderByDescending(e => e.Cantidad)
                        : q.OrderBy(e => e.Cantidad),

                    // Por defecto ordenamos por Id
                    _ => filters.EsDescendente
                        ? q.OrderByDescending(e => e.Id)
                        : q.OrderBy(e => e.Id)
                };
            };

            // Calculamos la fecha tope para los "Días para vencer" acá afuera
            DateTime? limiteDiasParaVencer = filters.DiasParaVencer.HasValue
                ? hoy.AddDays(filters.DiasParaVencer.Value)
                : null;

            var resultadoEntidad = await _repository.GetFiltered<Existencia>(
                predicate: FiltroExistencias(filters),
                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize,
                orderBy: orderByLogic,
                include: new[] { "Producto", "Deposito", "Lote" }
            );

            var itemsDto = resultadoEntidad.Items.Select(e => new ExistenciaModelResponse(
                e.Id,
                e.ProductoId,
                e.Producto?.Nombre ?? "Sin Nombre",
                e.Producto?.GTIN ?? "Sin GTIN",
                e.Producto?.CodigoInterno ?? "Sin Código Genérico",
                e.DepositoId,
                e.Deposito?.Nombre ?? "Sin Depósito",
                e.LoteId ?? 0,
                e.Lote?.CodigoLote.ToUpper() ?? "Sin Código de Lote",
                e.NumSerie ?? "Sin Número de Serie",
                e.FechaVencimiento,
                e.Cantidad,
                e.InformacionAdicional
            )).ToList();

            return new PagedResult<ExistenciaModelResponse>
            {
                Items = itemsDto,
                TotalCount = resultadoEntidad.TotalCount,
                PageNumber = resultadoEntidad.PageNumber,
                PageSize = resultadoEntidad.PageSize
            };
        }

        public async Task<PagedResult<ExistenciaAgrupadaResponse>> GetExistenciasAgrupadas(ExistenciaFilters filters)
        {
            var filtradas = _repository.Query<Existencia>().Where(FiltroExistencias(filters));

            // Un grupo = mismo producto, lote, serie y vencimiento; se suman las cantidades de todos los depósitos.
            var grupos = filtradas
                .GroupBy(e => new { e.ProductoId, NombreProducto = e.Producto.Nombre, e.LoteId, e.NumSerie, e.FechaVencimiento })
                .Select(g => new
                {
                    g.Key.ProductoId,
                    g.Key.NombreProducto,
                    g.Key.LoteId,
                    g.Key.NumSerie,
                    g.Key.FechaVencimiento,
                    Total = g.Sum(e => e.Cantidad)
                });

            var ordenados = filters.OrdenarPor switch
            {
                OrdenExistencia.NombreProducto => filters.EsDescendente
                    ? grupos.OrderByDescending(g => g.NombreProducto).ThenBy(g => g.LoteId)
                    : grupos.OrderBy(g => g.NombreProducto).ThenBy(g => g.LoteId),
                OrdenExistencia.FechaVencimiento => filters.EsDescendente
                    ? grupos.OrderByDescending(g => g.FechaVencimiento).ThenBy(g => g.NombreProducto)
                    : grupos.OrderBy(g => g.FechaVencimiento).ThenBy(g => g.NombreProducto),
                OrdenExistencia.Cantidad => filters.EsDescendente
                    ? grupos.OrderByDescending(g => g.Total).ThenBy(g => g.NombreProducto)
                    : grupos.OrderBy(g => g.Total).ThenBy(g => g.NombreProducto),
                _ => filters.EsDescendente
                    ? grupos.OrderByDescending(g => g.ProductoId).ThenByDescending(g => g.LoteId)
                    : grupos.OrderBy(g => g.ProductoId).ThenBy(g => g.LoteId)
            };

            var total = await grupos.CountAsync();
            var pagina = await ordenados
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            // Detalle por depósito de los grupos de esta página.
            var productoIds = pagina.Select(g => g.ProductoId).Distinct().ToList();
            var detalle = await filtradas
                .Where(e => productoIds.Contains(e.ProductoId))
                .Select(e => new
                {
                    e.Id,
                    e.ProductoId,
                    e.LoteId,
                    e.NumSerie,
                    e.FechaVencimiento,
                    e.DepositoId,
                    DepositoNombre = e.Deposito.Nombre,
                    e.Cantidad,
                    Gtin = e.Producto.GTIN,
                    CodigoLote = e.Lote != null ? e.Lote.CodigoLote : null
                })
                .ToListAsync();

            var items = pagina.Select(g =>
            {
                var filas = detalle
                    .Where(d => d.ProductoId == g.ProductoId
                        && d.LoteId == g.LoteId
                        && string.Equals(d.NumSerie, g.NumSerie, StringComparison.OrdinalIgnoreCase)
                        && d.FechaVencimiento == g.FechaVencimiento)
                    .OrderBy(d => d.DepositoNombre)
                    .ToList();
                var primera = filas.FirstOrDefault();
                return new ExistenciaAgrupadaResponse(
                    g.ProductoId,
                    g.NombreProducto ?? "Sin Nombre",
                    primera?.Gtin ?? "Sin GTIN",
                    g.LoteId ?? 0,
                    primera?.CodigoLote?.ToUpper() ?? "Sin Código de Lote",
                    g.NumSerie ?? "Sin Número de Serie",
                    g.FechaVencimiento,
                    g.Total,
                    filas.Select(d => new ExistenciaPorDepositoResponse(d.Id, d.DepositoId, d.DepositoNombre ?? "Sin Depósito", d.Cantidad)).ToList());
            }).ToList();

            return new PagedResult<ExistenciaAgrupadaResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = filters.PageNumber,
                PageSize = filters.PageSize
            };
        }

        /// <summary>
        /// Filtros de existencias (texto, depósito, lote, vencimiento). Lo usan el listado y el agrupado.
        /// </summary>
        private static Expression<Func<Existencia, bool>> FiltroExistencias(ExistenciaFilters filters)
        {
            var hoy = DateTime.Now.Date;
            var fechaLimiteVencimiento = filters.DiasParaVencer.HasValue
                ? hoy.AddDays(filters.DiasParaVencer.Value)
                : (DateTime?)null;
            DateTime? limiteDiasParaVencer = fechaLimiteVencimiento;

            return e =>
                    // --- FILTROS EXACTOS Y DE TEXTO ---
                    (!filters.ProductoId.HasValue || e.ProductoId == filters.ProductoId.Value) &&
                    (!filters.DepositoId.HasValue || e.DepositoId == filters.DepositoId.Value) &&
                    (!filters.LoteId.HasValue || e.LoteId == filters.LoteId.Value) &&

                    (string.IsNullOrEmpty(filters.NumSerie) ||
                        (e.NumSerie != null && Busqueda.Coincide(e.NumSerie, filters.NumSerie))) &&

                    (string.IsNullOrEmpty(filters.CodigoLote) ||
                        (e.Lote != null && Busqueda.Coincide(e.Lote.CodigoLote, filters.CodigoLote))) &&

                    (string.IsNullOrEmpty(filters.NombreProducto) ||
                        (e.Producto != null && Busqueda.Coincide(e.Producto.Nombre, filters.NombreProducto))) &&

                    // --- FILTROS DE FECHAS (Agrupados correctamente) ---

                    // Vencimiento DESDE
                    (!filters.FechaVencimientoDesde.HasValue ||
                        (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value >= filters.FechaVencimientoDesde.Value)
                    ) &&

                    // Vencimiento HASTA
                    (!filters.FechaVencimientoHasta.HasValue ||
                        (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value <= filters.FechaVencimientoHasta.Value)
                    ) &&

                    // Límite Vencimiento (Extra)
                    (!fechaLimiteVencimiento.HasValue ||
                        (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value <= fechaLimiteVencimiento.Value)
                    ) &&

                    // DÍAS PARA VENCER: no vencido hoy y vence antes del límite
                    (!limiteDiasParaVencer.HasValue ||
                        (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value.Date >= hoy && e.FechaVencimiento.Value.Date <= limiteDiasParaVencer.Value)
                    ) &&

                    // SOLO CON VENCIMIENTO
                    (!filters.SoloConVencimiento.HasValue ||
                        e.FechaVencimiento.HasValue == filters.SoloConVencimiento.Value
                    );
        }

        /// <summary>
        /// Obtiene productos próximos a vencer según filtros específicos.
        /// </summary>
        public async Task<List<ProductoProximoVencerDto>> GetProductosProximosVencer(VencimientoFilters filters)
        {
            var hoy = DateTime.Now.Date;
            var fechaMinima = filters.DiasDesde.HasValue ? hoy.AddDays(filters.DiasDesde.Value) : hoy;
            var fechaMaxima = filters.DiasHasta.HasValue ? hoy.AddDays(filters.DiasHasta.Value) : hoy.AddDays(90);

            // Obtener todas las existencias con vencimiento
            var existencias = await _repository.GetFiltered<Existencia>(
                predicate: e =>
                    // Con fecha de vencimiento dentro del rango
                    e.FechaVencimiento.HasValue && e.FechaVencimiento.Value >= fechaMinima && e.FechaVencimiento.Value <= fechaMaxima &&

                    // Filtros opcionales
                    (!filters.DepositoId.HasValue || e.DepositoId == filters.DepositoId.Value) &&
                    (!filters.ProductoId.HasValue || e.ProductoId == filters.ProductoId.Value) &&

                    // Incluir vencidos solo si se solicita
                    (filters.IncluirVencidos || e.FechaVencimiento.Value >= hoy),

                pageNumber: 1,
                pageSize: int.MaxValue, // Sin paginación
                include: new[] { "Producto", "Deposito", "Lote" }
            );

            var resultado = existencias.Items
                .Select(e =>
                {
                    var fechaVenc = e.FechaVencimiento ?? hoy;
                    var diasParaVencer = (int)(fechaVenc.Date - hoy).TotalDays;

                    return new ProductoProximoVencerDto(
                        ExistenciaId: e.Id,
                        ProductoId: e.ProductoId,
                        ProductoNombre: e.Producto?.Nombre ?? "Sin Nombre",
                        ProductoGtin: e.Producto?.GTIN ?? "Sin GTIN",
                        ProductoCodigoGenerico: e.Producto?.CodigoGenerico ?? "Sin Código Genérico",
                        DepositoId: e.DepositoId,
                        DepositoNombre: e.Deposito?.Nombre ?? "Sin Depósito",
                        LoteId: e.LoteId,
                        CodigoLote: e.Lote?.CodigoLote.ToUpper(),
                        NumeroSerie: e.NumSerie,
                        FechaVencimiento: fechaVenc,
                        DiasParaVencer: diasParaVencer,
                        Cantidad: e.Cantidad
                    );
                })
                .OrderBy(p => p.FechaVencimiento)
                .ToList();

            return resultado;
        }

        /// <summary>
        /// Obtiene dashboard de productos próximos a vencer agrupados por rangos.
        /// </summary>
        public async Task<DashboardVencimientosResponse> GetDashboardVencimientos()
        {
            var hoy = DateTime.Now.Date;

            // Obtener productos que vencen en los próximos 6 meses
            var productosProximosVencer = await GetProductosProximosVencer(new VencimientoFilters(
                DiasDesde: 0,
                DiasHasta: 180,
                DepositoId: null,
                ProductoId: null,
                IncluirVencidos: false
            ));

            // Definir rangos
            var rangos = new[]
            {
                new { Nombre = "Vencidos", Min = int.MinValue, Max = -1 },
                new { Nombre = "0-30 días", Min = 0, Max = 30 },
                new { Nombre = "31-60 días", Min = 31, Max = 60 },
                new { Nombre = "61-90 días", Min = 61, Max = 90 },
                new { Nombre = "91-120 días", Min = 91, Max = 120 },
                new { Nombre = "121-180 días", Min = 121, Max = 180 }
            };

            var estadisticas = rangos.Select(rango =>
            {
                var itemsEnRango = productosProximosVencer
                    .Where(p => p.DiasParaVencer >= rango.Min && p.DiasParaVencer <= rango.Max)
                    .ToList();

                return new VencimientoStats(
                    Rango: rango.Nombre,
                    DiasMinimo: rango.Min,
                    DiasMaximo: rango.Max,
                    TotalProductos: itemsEnRango.Select(i => i.ProductoId).Distinct().Count(),
                    TotalLotes: itemsEnRango.Select(i => i.LoteId).Distinct().Count(),
                    CantidadTotal: itemsEnRango.Sum(i => i.Cantidad),
                    Items: itemsEnRango
                );
            }).ToList();

            return new DashboardVencimientosResponse(
                FechaConsulta: hoy,
                TotalProductosProximosVencer: productosProximosVencer.Select(p => p.ProductoId).Distinct().Count(),
                TotalLotesProximosVencer: productosProximosVencer.Where(p => p.LoteId.HasValue).Select(p => p.LoteId).Distinct().Count(),
                CantidadTotalProximaVencer: productosProximosVencer.Sum(p => p.Cantidad),
                Rangos: estadisticas
            );
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

        public async Task<ExistenciaModelResponse> CorregirExistencia(int id, ExistenciaCorreccionRequest r)
        {
            var motivo = CorreccionSoporte.Motivo(r.TicketId, r.Motivo);

            // Sin includes: Update() marca todo el grafo como modificado y ensuciaría la auditoría.
            var original = await _repository.GetById<Existencia>(id)
                ?? throw new NotFoundException(nameof(Existencia), id);
            var producto = await _repository.GetById<Producto>(original.ProductoId)
                ?? throw new NotFoundException(nameof(Producto), original.ProductoId);

            var ticket = await CorreccionSoporte.Ticket(_repository, r.TicketId);

            var deposito = await _repository.GetById<Deposito>(r.DepositoId)
                ?? throw new NotFoundException(nameof(Deposito), r.DepositoId);

            var loteNuevo = string.IsNullOrWhiteSpace(r.CodigoLote) ? null : r.CodigoLote.Trim().ToUpper();
            var serieNueva = string.IsNullOrWhiteSpace(r.NumSerie) ? null : r.NumSerie.Trim();
            var loteAnterior = original.LoteId.HasValue
                ? (await _repository.GetById<Lote>(original.LoteId.Value))?.CodigoLote?.ToUpper()
                : null;
            var vencNuevo = r.FechaVencimiento?.Date;

            if (serieNueva == null && loteNuevo == null && string.IsNullOrEmpty(producto.CodigoGenerico))
                throw new DomainException(DomainErrorCode.SerieYCodigoLoteNoEspecificados,
                    "La existencia tiene que tener número de serie, código de lote o el producto un código genérico.");

            var cambiaLote = loteNuevo != loteAnterior;
            var cambiaSerie = serieNueva != original.NumSerie;
            var cambiaDeposito = r.DepositoId != original.DepositoId;
            var cambiaVenc = vencNuevo != original.FechaVencimiento?.Date;
            var cambiaCantidad = r.Cantidad != original.Cantidad;
            var cambiaIdentidad = cambiaLote || cambiaSerie || cambiaDeposito;
            if (!cambiaIdentidad && !cambiaVenc && !cambiaCantidad)
                throw new DomainException(DomainErrorCode.CorreccionSinCambios, "No hay cambios para guardar.");

            using var transaction = await _repository.BeginTransaction();

            Lote? lote = null;
            if (loteNuevo != null)
                lote = await _repository.First<Lote>(l => l.CodigoLote == loteNuevo)
                    ?? await _repository.Add(new Lote { CodigoLote = loteNuevo });

            // ¿La nueva combinación ya es otra existencia? Entonces se suman.
            Existencia? destino = null;
            if (cambiaIdentidad)
            {
                int? loteId = lote?.Id;
                destino = await _repository.First<Existencia>(e =>
                    e.Id != original.Id &&
                    e.ProductoId == original.ProductoId &&
                    e.DepositoId == r.DepositoId &&
                    e.LoteId == loteId &&
                    (serieNueva == null ? e.NumSerie == null : e.NumSerie == serieNueva));
            }

            var cantidadFinal = r.Cantidad + (destino?.Cantidad ?? 0);
            if (producto.EsUnitario && cantidadFinal > 1)
                throw new DomainException(DomainErrorCode.ProductoUnitarioCantidadInvalida,
                    destino != null
                        ? $"Ya existe la existencia #{destino.Id} con esa serie/lote en ese depósito y el producto es unitario."
                        : "Un producto unitario no puede tener más de 1 unidad por existencia.");

            var consignaciones = (await _repository.GetFiltered<Consignacion>(c => c.ExistenciaId == original.Id, 1, int.MaxValue)).Items;
            var consignado = consignaciones.Sum(c => c.Cantidad);
            if (r.Cantidad < consignado)
                throw new DomainException(DomainErrorCode.StockEnConsignacion,
                    $"Hay {consignado} unidad(es) de esta existencia en consignación; la cantidad no puede quedar por debajo.");

            var observaciones = CorreccionSoporte.Observaciones(ticket, motivo);

            var cantidadAnterior = original.Cantidad;
            var depositoAnteriorId = original.DepositoId;
            var loteAnteriorId = original.LoteId;
            var serieAnterior = original.NumSerie;
            var vencAnterior = original.FechaVencimiento;
            Existencia resultado;

            if (cambiaIdentidad)
            {
                // Sale todo de la combinación vieja y entra la cantidad corregida en la nueva.
                if (cantidadAnterior > 0)
                    await RegistrarMovimiento(TipoMovimiento.AjusteNegativo, original.ProductoId, depositoAnteriorId,
                        loteAnteriorId, serieAnterior, -cantidadAnterior, cantidadAnterior, 0, observaciones: observaciones);

                if (destino != null)
                {
                    var stockDestino = destino.Cantidad;
                    destino.Cantidad = cantidadFinal;
                    destino.FechaVencimiento = r.FechaVencimiento;
                    await _repository.Update(destino);
                    foreach (var c in consignaciones)
                    {
                        c.ExistenciaId = destino.Id;
                        await _repository.Update(c);
                    }
                    await _repository.Delete(original);
                    if (r.Cantidad > 0)
                        await RegistrarMovimiento(TipoMovimiento.AjustePositivo, destino.ProductoId, destino.DepositoId,
                            destino.LoteId, destino.NumSerie, r.Cantidad, stockDestino, cantidadFinal, observaciones: observaciones);
                    resultado = destino;
                }
                else
                {
                    original.LoteId = lote?.Id;
                    original.NumSerie = serieNueva;
                    original.DepositoId = r.DepositoId;
                    original.FechaVencimiento = r.FechaVencimiento;
                    original.Cantidad = r.Cantidad;
                    await _repository.Update(original);
                    if (r.Cantidad > 0)
                        await RegistrarMovimiento(TipoMovimiento.AjustePositivo, original.ProductoId, original.DepositoId,
                            original.LoteId, original.NumSerie, r.Cantidad, 0, r.Cantidad, observaciones: observaciones);
                    resultado = original;
                }
            }
            else
            {
                original.FechaVencimiento = r.FechaVencimiento;
                original.Cantidad = r.Cantidad;
                await _repository.Update(original);
                if (cambiaCantidad)
                {
                    var diferencia = r.Cantidad - cantidadAnterior;
                    await RegistrarMovimiento(diferencia > 0 ? TipoMovimiento.AjustePositivo : TipoMovimiento.AjusteNegativo,
                        original.ProductoId, original.DepositoId, original.LoteId, original.NumSerie,
                        diferencia, cantidadAnterior, r.Cantidad, observaciones: observaciones);
                }
                resultado = original;
            }

            if (ticket != null)
            {
                var depositoAnterior = cambiaDeposito ? (await _repository.GetById<Deposito>(depositoAnteriorId))?.Nombre : null;
                var V = CorreccionSoporte.Valor;
                var cambios = new List<string>();
                if (cambiaLote) cambios.Add($"lote {V(loteAnterior)} → {V(loteNuevo)}");
                if (cambiaSerie) cambios.Add($"serie {V(serieAnterior)} → {V(serieNueva)}");
                if (cambiaVenc) cambios.Add($"vencimiento {V(vencAnterior?.ToString("dd/MM/yyyy"))} → {V(r.FechaVencimiento?.ToString("dd/MM/yyyy"))}");
                if (cambiaDeposito) cambios.Add($"depósito {V(depositoAnterior)} → {deposito.Nombre}");
                if (cambiaCantidad) cambios.Add($"cantidad {cantidadAnterior} → {r.Cantidad}");
                var texto = $"Existencia #{id} de {producto.Nombre} corregida: {string.Join(", ", cambios)}."
                    + (destino != null ? $" Se sumó a la existencia #{destino.Id}, que quedó con {cantidadFinal}." : "");
                await CorreccionSoporte.Comentar(_repository, _httpContextAccessor, ticket, texto, motivo);
            }

            await transaction.CommitAsync();
            return await GetExistenciaById(resultado.Id);
        }

        public async Task AjustarExistenciaExacta(int depositoId, int productoId, int? loteId, string? numSerie, DateTime? fechaVencimiento,
            int delta, string observaciones, int? remitoIngresoId = null, int? remitoEgresoId = null, bool actualizarVencimiento = false)
        {
            var existencia = await _repository.First<Existencia>(e =>
                e.ProductoId == productoId &&
                e.DepositoId == depositoId &&
                e.LoteId == loteId &&
                (numSerie == null ? e.NumSerie == null : e.NumSerie == numSerie));
            var anterior = existencia?.Cantidad ?? 0;

            if (delta < 0)
            {
                var consignado = existencia == null ? 0
                    : (await _repository.GetFiltered<Consignacion>(c => c.ExistenciaId == existencia.Id, 1, int.MaxValue)).Items.Sum(c => c.Cantidad);
                var disponible = anterior - consignado;
                if (existencia == null || disponible < -delta)
                {
                    var deposito = (await _repository.GetById<Deposito>(depositoId))?.Nombre ?? $"el depósito #{depositoId}";
                    throw new DomainException(DomainErrorCode.CorreccionStockYaMovido,
                        $"En {deposito} quedan {Math.Max(disponible, 0)} u. disponibles y la corrección necesita sacar {-delta}" +
                        (consignado > 0 ? $" ({consignado} están en consignación)." : "."));
                }
                existencia.Cantidad += delta;
                if (actualizarVencimiento) existencia.FechaVencimiento = fechaVencimiento;
                if (existencia.Cantidad == 0 && consignado == 0)
                    await _repository.Delete(existencia);
                else
                    await _repository.Update(existencia);
            }
            else if (delta > 0)
            {
                var producto = await _repository.GetById<Producto>(productoId)
                    ?? throw new NotFoundException(nameof(Producto), productoId);
                if (producto.EsUnitario && anterior + delta > 1)
                    throw new DomainException(DomainErrorCode.ProductoUnitarioCantidadInvalida,
                        "El producto es unitario y ya hay una unidad con esa serie/lote en el depósito.");
                if (existencia == null)
                {
                    await _repository.Add(new Existencia
                    {
                        ProductoId = productoId,
                        DepositoId = depositoId,
                        LoteId = loteId,
                        NumSerie = numSerie,
                        FechaVencimiento = fechaVencimiento,
                        Cantidad = delta,
                    });
                }
                else
                {
                    existencia.Cantidad += delta;
                    if (actualizarVencimiento) existencia.FechaVencimiento = fechaVencimiento;
                    await _repository.Update(existencia);
                }
            }
            else if (actualizarVencimiento && existencia != null && existencia.FechaVencimiento?.Date != fechaVencimiento?.Date)
            {
                existencia.FechaVencimiento = fechaVencimiento;
                await _repository.Update(existencia);
            }

            if (delta != 0)
                await RegistrarMovimiento(delta > 0 ? TipoMovimiento.AjustePositivo : TipoMovimiento.AjusteNegativo,
                    productoId, depositoId, loteId, numSerie, delta, anterior, anterior + delta,
                    remitoIngresoId: remitoIngresoId, remitoEgresoId: remitoEgresoId, observaciones: observaciones);
        }

        public async Task<int> GetStockDisponible(int productoId, string? codigoLote = null, string? numSerie = null)
        {
            var existencia = await _repository.GetFiltered<Existencia>(
                e => e.ProductoId == productoId &&
                     (string.IsNullOrEmpty(codigoLote) || e.Lote.CodigoLote == codigoLote.ToUpper()) &&
                     (string.IsNullOrEmpty(numSerie) || e.NumSerie == numSerie)
            );

            var existenciasASumar = existencia.Items;

            if (!existenciasASumar.Any()) return 0;

            var idsExistencias = existenciasASumar.Select(e => e.Id).ToList();

            // Buscar cuánto de ese stock está retenido en consignaciones
            var consignaciones = await _repository.GetFiltered<Consignacion>(
                c => idsExistencias.Contains(c.ExistenciaId),
                pageNumber: 1,
                pageSize: int.MaxValue
            );

            var cantidadFisica = existenciasASumar.Sum(e => e.Cantidad);
            var cantidadConsignada = consignaciones.Items.Sum(c => c.Cantidad);

            // Devolvemos solo lo que realmente se puede tocar
            return cantidadFisica - cantidadConsignada;
        }

        public async Task<StockDisponibleResponse> GetStockDisponible(int existenciaId)
        {
            var existencia = await _repository.GetById<Existencia>(existenciaId);

            if (existencia == null) return new StockDisponibleResponse(0);

            // Buscar cuánto de ese stock está retenido en consignaciones
            var consignaciones = await _repository.GetFiltered<Consignacion>(
                c => c.ExistenciaId == existenciaId,
                pageNumber: 1,
                pageSize: int.MaxValue
            );

            var cantidadFisica = existencia.Cantidad;
            var cantidadConsignada = consignaciones.Items.Sum(c => c.Cantidad);

            // Devolvemos solo lo que realmente se puede tocar
            return new StockDisponibleResponse(Cantidad: (cantidadFisica - cantidadConsignada));
        }
    }
}