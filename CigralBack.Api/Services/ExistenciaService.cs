using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                lote?.FechaVencimiento ?? e.FechaVencimiento,
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
                    // Creamos un lote nuevo inicializado en 0 (la cantidad se sumará más abajo de forma global)
                    lote = new Lote
                    {
                        CodigoLote = loteUpper,
                        FechaVencimiento = r.FechaVencimiento,
                        CantidadDisponible = 0,
                        ProductoId = r.ProductoId
                    };

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
                    Cantidad = r.Cantidad
                };

                if (lote != null)
                {
                    existencia.LoteId = lote.Id;
                    existencia.FechaVencimiento = lote.FechaVencimiento;
                }
                else
                {
                    existencia.FechaVencimiento = r.FechaVencimiento;
                }

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

            // === ACTUALIZACIÓN DEL LOTE GLOBAL ===
            // Sumamos la cantidad entrante de forma independiente a la existencia
            if (lote != null)
            {
                lote.CantidadDisponible += r.Cantidad;
                await _repository.Update(lote);
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

            // Buscar existencia existente
            // Buscar existencia existente
            var existenciasADescontar = await _repository.GetFiltered<Existencia>(
                e => e.ProductoId == r.ProductoId &&
                     (string.IsNullOrEmpty(loteUpper) || e.LoteId == lote.Id) &&
                     (string.IsNullOrEmpty(r.NumSerie) || e.NumSerie == r.NumSerie)
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
                existencia.Lote?.FechaVencimiento ?? existencia.FechaVencimiento,
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
                        // EF Core es inteligente, si la fecha es nula, ordenará usando el lote
                        ? q.OrderByDescending(e => e.FechaVencimiento ?? e.Lote.FechaVencimiento)
                        : q.OrderBy(e => e.FechaVencimiento ?? e.Lote.FechaVencimiento),

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
                predicate: e =>
                    // --- FILTROS EXACTOS Y DE TEXTO ---
                    (!filters.ProductoId.HasValue || e.ProductoId == filters.ProductoId.Value) &&
                    (!filters.DepositoId.HasValue || e.DepositoId == filters.DepositoId.Value) &&
                    (!filters.LoteId.HasValue || e.LoteId == filters.LoteId.Value) &&

                    (string.IsNullOrEmpty(filters.NumSerie) ||
                        (e.NumSerie != null && e.NumSerie.Contains(filters.NumSerie))) &&

                    (string.IsNullOrEmpty(filters.CodigoLote) ||
                        (e.Lote != null && e.Lote.CodigoLote.Contains(filters.CodigoLote))) &&

                    (string.IsNullOrEmpty(filters.NombreProducto) ||
                        (e.Producto != null && e.Producto.Nombre.Contains(filters.NombreProducto))) &&

                    // --- FILTROS DE FECHAS (Agrupados correctamente) ---

                    // Vencimiento DESDE
                    (!filters.FechaVencimientoDesde.HasValue ||
                        ( // <-- Paréntesis clave para agrupar el OR
                            (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value >= filters.FechaVencimientoDesde.Value) ||
                            (e.Lote != null && e.Lote.FechaVencimiento >= filters.FechaVencimientoDesde.Value)
                        )
                    ) &&

                    // Vencimiento HASTA
                    (!filters.FechaVencimientoHasta.HasValue ||
                        (
                            (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value <= filters.FechaVencimientoHasta.Value) ||
                            (e.Lote != null && e.Lote.FechaVencimiento <= filters.FechaVencimientoHasta.Value)
                        )
                    ) &&

                    // Límite Vencimiento (Extra)
                    (!fechaLimiteVencimiento.HasValue ||
                        (
                            (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value <= fechaLimiteVencimiento.Value) ||
                            (e.Lote != null && e.Lote.FechaVencimiento <= fechaLimiteVencimiento.Value)
                        )
                    ) &&

                    // DÍAS PARA VENCER (Arreglado)
                    (!limiteDiasParaVencer.HasValue ||
                        (
                            // Opción A: Tiene fecha propia, NO está vencido hoy, y vence antes del límite
                            (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value.Date >= hoy && e.FechaVencimiento.Value.Date <= limiteDiasParaVencer.Value)
                            ||
                            // Opción B: Su lote tiene fecha, NO está vencido hoy, y vence antes del límite
                            (e.Lote != null && e.Lote.FechaVencimiento.Date >= hoy && e.Lote.FechaVencimiento.Date <= limiteDiasParaVencer.Value)
                        )
                    ) &&

                    // SOLO CON VENCIMIENTO
                    (!filters.SoloConVencimiento.HasValue ||
                        (
                            (filters.SoloConVencimiento.Value && (e.FechaVencimiento.HasValue || e.Lote != null))
                            ||
                            (!filters.SoloConVencimiento.Value && !e.FechaVencimiento.HasValue && e.Lote == null)
                        )
                    ),

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
                e.FechaVencimiento ?? e.Lote?.FechaVencimiento,
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
                    // Solo con fecha de vencimiento
                    (e.FechaVencimiento.HasValue || e.Lote != null) &&

                    // Dentro del rango de fechas
                    ((e.FechaVencimiento.HasValue && e.FechaVencimiento.Value >= fechaMinima && e.FechaVencimiento.Value <= fechaMaxima) ||
                     (e.Lote != null && e.Lote.FechaVencimiento >= fechaMinima && e.Lote.FechaVencimiento <= fechaMaxima)) &&

                    // Filtros opcionales
                    (!filters.DepositoId.HasValue || e.DepositoId == filters.DepositoId.Value) &&
                    (!filters.ProductoId.HasValue || e.ProductoId == filters.ProductoId.Value) &&

                    // Incluir vencidos solo si se solicita
                    (filters.IncluirVencidos ||
                        (e.FechaVencimiento.HasValue && e.FechaVencimiento.Value >= hoy) ||
                        (e.Lote != null && e.Lote.FechaVencimiento >= hoy)),

                pageNumber: 1,
                pageSize: int.MaxValue, // Sin paginación
                include: new[] { "Producto", "Deposito", "Lote" }
            );

            var resultado = existencias.Items
                .Select(e =>
                {
                    var fechaVenc = e.Lote?.FechaVencimiento ?? e.FechaVencimiento ?? hoy;
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