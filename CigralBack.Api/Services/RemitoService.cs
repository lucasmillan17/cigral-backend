using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Bases;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Servicio para gestión de remitos de ingreso y egreso.
    /// </summary>
    public class RemitoService : IRemitoService
    {
        private readonly IRepository _repository;
        private readonly IExistenciaService _existenciaService;
        private readonly IProductoService _productoService;

        public RemitoService(IRepository repository, IExistenciaService existenciaService, IProductoService productoService)
        {
            _repository = repository;
            _existenciaService = existenciaService;
            _productoService = productoService;
        }

        /// <summary>
        /// Registra un remito de ingreso (entrada de mercadería de proveedor).
        /// Se ejecuta dentro de una transacción para evitar inconsistencias cuando hay muchas operaciones concurrentes.
        /// </summary>
        /// <param name="request">Datos del remito de ingreso</param>
        /// <returns>Información del remito creado</returns>
        /// <exception cref="NotFoundException">Si proveedor o depósito no existen</exception>
        /// <exception cref="DomainException">Si hay errores de validación</exception>
        public async Task<RemitoResponse> RegistrarIngreso(RemitoRequest request)
        {
            // Validar que los detalles no estén vacíos
            if (request.Detalles == null || !request.Detalles.Any())
            {
                throw new DomainException(
                    DomainErrorCode.RemitoSinDetalles,
                    "El remito debe tener al menos un detalle."
                );
            }

            // Validar que el proveedor exista
            var proveedor = await _repository.GetById<Proveedor>(request.EntidadId);
            if (proveedor == null)
            {
                throw new NotFoundException(nameof(Proveedor), request.EntidadId);
            }

            // Validar que el depósito exista
            var deposito = await _repository.GetById<Deposito>(request.DepositoId);
            if (deposito == null)
            {
                throw new NotFoundException(nameof(Deposito), request.DepositoId);
            }

            // Validar número de remito único (si se proporciona)
            if (string.IsNullOrEmpty(request.NumeroRemito))
            {
                /*var existeNumero = await _repository.First<RemitoIngreso>(
                    r => r.NumeroRemito == request.NumeroRemito
                );*/
                    throw new DomainException(
                        DomainErrorCode.NumeroRemitoDuplicado,
                        $"No se recibio numero de remito"
                    );
                
            }

            // Iniciar transacción para todo el proceso (cabecera, detalles y movimientos de stock)
            using var transaction = await _repository.BeginTransaction();
            try
            {
                // Crear la cabecera del remito
                var remito = new RemitoIngreso
                {
                    Fecha = DateTime.Now,
                    NumeroRemito = request.NumeroRemito,
                    ComprobanteAsociado = request.ComprobanteAsociado,
                    Observaciones = request.Observaciones,
                    ProveedorId = request.EntidadId,
                    DepositoId = request.DepositoId
                };

                remito = await _repository.Add(remito);

                int cantidadTotal = 0;

                // Procesar cada detalle
                foreach (var detalle in request.Detalles)
                {
                    Lote? lote = null;
                    if (!string.IsNullOrEmpty(detalle.CodigoLote.ToUpper()))
                    {
                            lote = await _repository.First<Lote>(d => d.CodigoLote == detalle.CodigoLote.ToUpper());
                        if (lote == null)
                        {
                            // Si el lote no existe, lo creamos
                            lote = new Lote
                            {
                                CodigoLote = detalle.CodigoLote,
                                ProductoId = detalle.ProductoId,
                                FechaVencimiento = detalle.FechaVencimiento // Podríamos agregar esta info al detalle si es necesario
                            };
                            lote = await _repository.Add(lote);
                        }
                    }

                    // Crear el detalle del remito
                    var detalleRemito = new DetalleRemito
                    {
                        RemitoIngresoId = remito.Id,
                        ProductoId = detalle.ProductoId,
                        LoteId = lote?.Id,
                        NumeroSerie = detalle.NumeroSerie,
                        Cantidad = detalle.Cantidad
                    };

                    await _repository.Add(detalleRemito);

                    // Aumentar el stock en existencias pasando el ID del remito
                    var existenciaRequest = new ExistenciaModelRequest(
                        DepositoId: request.DepositoId,
                        ProductoId: detalle.ProductoId,
                        NumSerie: detalle.NumeroSerie,
                        CodigoLote: detalle.CodigoLote.ToUpper(),
                        FechaVencimiento: detalle.FechaVencimiento,
                        Cantidad: detalle.Cantidad,
                        InformacionAdicional: detalle.InformacionAdicional,
                        EsDevolucion: request.EsDevolucion
                    );

                    // Llamamos al servicio de existencias que internamente hará validaciones y guardados.
                    // Como estamos dentro de la transacción, los cambios se aplicarán de forma atómica.
                    await _existenciaService.AumentarStock(
                        existenciaRequest,
                        remitoIngresoId: remito.Id,
                        observaciones: $"Remito de ingreso {request.NumeroRemito ?? remito.Id.ToString()}"
                    );

                    cantidadTotal += detalle.Cantidad;
                }

                await transaction.CommitAsync();

                return new RemitoResponse(
                    Id: remito.Id,
                    NumeroRemito: remito.NumeroRemito,
                    ComprobanteAsociado: remito.ComprobanteAsociado,
                    Fecha: remito.Fecha,
                    DepositoId: remito.DepositoId,
                    EntidadId: remito.ProveedorId,
                    Observaciones: remito.Observaciones,
                    CantidadDetalles: request.Detalles.Count,
                    CantidadTotal: cantidadTotal
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Registra un remito de egreso (salida de mercadería a cliente).
        /// Se ejecuta dentro de una transacción para evitar inconsistencias cuando hay muchas operaciones concurrentes.
        /// </summary>
        /// <param name="request">Datos del remito de egreso</param>
        /// <returns>Información del remito creado</returns>
        /// <exception cref="NotFoundException">Si cliente o depósito no existen</exception>
        /// <exception cref="DomainException">Si hay errores de validación o stock insuficiente</exception>
        public async Task<ResultadoOperacion<RemitoResponse>> RegistrarEgreso(RemitoRequest request)
        {
            var erroresValidacion = new List<ErrorDetalleDto>();
            // Validar que los detalles no estén vacíos
            if (request.Detalles == null || !request.Detalles.Any())
            {
                throw new DomainException(
                    DomainErrorCode.RemitoSinDetalles,
                    "El remito debe tener al menos un detalle."
                );
            }

            // Validar que el cliente exista
            var cliente = await _repository.GetById<Cliente>(request.EntidadId);
            if (cliente == null)
            {
                throw new NotFoundException(nameof(Cliente), request.EntidadId);
            }

            // Validar que el depósito exista
            var deposito = await _repository.GetById<Deposito>(request.DepositoId);
            if (deposito == null)
            {
                throw new NotFoundException(nameof(Deposito), request.DepositoId);
            }

            // Validar número de remito único (si se proporciona)
            if (string.IsNullOrEmpty(request.NumeroRemito))
            {
               
                    throw new DomainException(
                        DomainErrorCode.NumeroRemitoDuplicado,
                        $"No se recibio un numero de remito"
                    );
                
            }

            foreach (var detalle in request.Detalles)
            {
                if (detalle.Cantidad <= 0)
                {
                    var producto = await _productoService.GetProductoById(detalle.ProductoId); 
                    erroresValidacion.Add(new ErrorDetalleDto(
                        Orden: request.Detalles.IndexOf(detalle),
                        Mensaje: $"La cantidad para el producto {producto.Nombre}, Lote: {detalle.CodigoLote.ToUpper()}, debe ser mayor a cero."
                    ));
                }

                string numeroSerie = detalle.NumeroSerie == "Sin Número de Serie" ? null : detalle.NumeroSerie;

                var stockExistente = await _existenciaService.GetStockDisponible(
                    depositoId: request.DepositoId,
                    productoId: detalle.ProductoId,
                    numSerie: numeroSerie,
                    codigoLote: detalle.CodigoLote.ToUpper()
                );

                if (stockExistente < detalle.Cantidad)
                {
                    var producto = await _productoService.GetProductoById(detalle.ProductoId);
                    erroresValidacion.Add(new ErrorDetalleDto(
                        Orden: request.Detalles.IndexOf(detalle),
                        Mensaje: $"Stock insuficiente para el producto {producto.Nombre}, Lote: {detalle.CodigoLote.ToUpper()}. Disponible: {stockExistente}, Solicitado: {detalle.Cantidad}."
                    ));
                }
            }

            if(erroresValidacion.Any())
            {
                return ResultadoOperacion<RemitoResponse>.Fallo(
                    mensaje: "Errores de validación en los detalles del remito.",
                    errores: erroresValidacion
                );
            }

            using var transaction = await _repository.BeginTransaction();
            try
            {
                // Crear la cabecera del remito
                var remito = new RemitoEgreso
                {
                    Fecha = DateTime.Now,
                    NumeroRemito = request.NumeroRemito,
                    ComprobanteAsociado = request.ComprobanteAsociado,
                    Observaciones = request.Observaciones,
                    ClienteId = request.EntidadId,
                    DepositoId = request.DepositoId
                };

                remito = await _repository.Add(remito);

                int cantidadTotal = 0;

                // Procesar cada detalle
                foreach (var detalle in request.Detalles)
                {
                    var lote = await _repository.First<Lote>(d => d.CodigoLote == detalle.CodigoLote.ToUpper());
                    // Crear el detalle del remito
                    var detalleRemito = new DetalleRemito
                    {
                        RemitoEgresoId = remito.Id,
                        ProductoId = detalle.ProductoId,
                        LoteId = lote?.Id,
                        NumeroSerie = detalle.NumeroSerie,
                        Cantidad = detalle.Cantidad
                    };

                    await _repository.Add(detalleRemito);

                    // Disminuir el stock en existencias pasando el ID del remito
                    var existenciaRequest = new ExistenciaModelRequest(
                        DepositoId: request.DepositoId,
                        ProductoId: detalle.ProductoId,
                        NumSerie: detalle.NumeroSerie,
                        CodigoLote: lote?.CodigoLote.ToUpper(),
                        FechaVencimiento: detalle.FechaVencimiento,
                        Cantidad: detalle.Cantidad,
                        InformacionAdicional: detalle.InformacionAdicional
                    );

                    await _existenciaService.DisminuirStock(
                        existenciaRequest,
                        remitoEgresoId: remito.Id,
                        observaciones: $"Remito de egreso {request.NumeroRemito ?? remito.Id.ToString()}"
                    );

                    cantidadTotal += detalle.Cantidad;
                }

                await transaction.CommitAsync();

                var remitoResponse = new RemitoResponse(
                    Id: remito.Id,
                    NumeroRemito: remito.NumeroRemito,
                    ComprobanteAsociado: remito.ComprobanteAsociado,
                    Fecha: remito.Fecha,
                    DepositoId: remito.DepositoId,
                    EntidadId: remito.ClienteId,
                    Observaciones: remito.Observaciones,
                    CantidadDetalles: request.Detalles.Count,
                    CantidadTotal: cantidadTotal
                );
                return ResultadoOperacion<RemitoResponse>.Ok(remitoResponse);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Actualiza un remito existente (solo datos que no afectan stock).
        /// </summary>
        public async Task<RemitoResponse> UpdateRemito(int id, UpdateRemitoRequest request, bool esIngreso)
        {
            if (esIngreso)
            {
                var remito = await _repository.GetById<RemitoIngreso>(id);
                if (remito == null)
                {
                    throw new NotFoundException(nameof(RemitoIngreso), id);
                }

                // Validar número de remito único (si se cambia)
                if (!string.IsNullOrEmpty(request.NumeroRemito) && request.NumeroRemito != remito.NumeroRemito)
                {
                    var existeNumero = await _repository.First<RemitoIngreso>(
                        r => r.NumeroRemito == request.NumeroRemito && r.Id != id
                    );
                    
                    if (existeNumero != null)
                    {
                        throw new DomainException(
                            DomainErrorCode.NumeroRemitoDuplicado,
                            $"El número de remito '{request.NumeroRemito}' ya existe."
                        );
                    }
                }

                // Actualizar solo campos permitidos
                remito.NumeroRemito = request.NumeroRemito;
                remito.Observaciones = request.Observaciones;
                remito.ComprobanteAsociado = request.ComprobanteAsociado;

                await _repository.Update(remito);

                return new RemitoResponse(
                    Id: remito.Id,
                    NumeroRemito: remito.NumeroRemito,
                    ComprobanteAsociado: remito.ComprobanteAsociado,
                    Fecha: remito.Fecha,
                    DepositoId: remito.DepositoId,
                    EntidadId: remito.ProveedorId,
                    Observaciones: remito.Observaciones,
                    CantidadDetalles: 0, // No calculamos aquí
                    CantidadTotal: 0
                );
            }
            else
            {
                var remito = await _repository.GetById<RemitoEgreso>(id);
                if (remito == null)
                {
                    throw new NotFoundException(nameof(RemitoEgreso), id);
                }

                // Validar número de remito único (si se cambia)
                if (!string.IsNullOrEmpty(request.NumeroRemito) && request.NumeroRemito != remito.NumeroRemito)
                {
                    var existeNumero = await _repository.First<RemitoEgreso>(
                        r => r.NumeroRemito == request.NumeroRemito && r.Id != id
                    );
                    
                    if (existeNumero != null)
                    {
                        throw new DomainException(
                            DomainErrorCode.NumeroRemitoDuplicado,
                            $"El número de remito '{request.NumeroRemito}' ya existe."
                        );
                    }
                }

                // Actualizar solo campos permitidos
                remito.NumeroRemito = request.NumeroRemito;
                remito.Observaciones = request.Observaciones;

                await _repository.Update(remito);

                return new RemitoResponse(
                    Id: remito.Id,
                    NumeroRemito: remito.NumeroRemito,
                    ComprobanteAsociado: remito.ComprobanteAsociado,
                    Fecha: remito.Fecha,
                    DepositoId: remito.DepositoId,
                    EntidadId: remito.ClienteId,
                    Observaciones: remito.Observaciones,
                    CantidadDetalles: 0, // No calculamos aquí
                    CantidadTotal: 0
                );
            }
        }

        private Func<IQueryable<T>, IOrderedQueryable<T>> orderByLogic<T>(RemitoFilters filters) where T : RemitoBase => q =>
        {
            return filters.OrdenarPor switch
            {
                OrdenRemito.Fecha => filters.EsDescendente
                    ? q.OrderByDescending(e => e.Fecha)
                    : q.OrderBy(e => e.Fecha),

                // Por defecto ordenamos por Id
                _ => filters.EsDescendente
                    ? q.OrderByDescending(e => e.Id)
                    : q.OrderBy(e => e.Id)
            };
        };

        public async Task<PagedResult<RemitoResponseGet>> GetRemitosIngreso(RemitoFilters filters)
        {
            var resultadoEntidad = await _repository.GetFiltered<RemitoIngreso>(
                predicate: e =>
                    (!filters.DepositoId.HasValue || e.DepositoId == filters.DepositoId.Value) &&
                    (!filters.EntidadId.HasValue || e.ProveedorId == filters.EntidadId.Value) &&

                    // Filtros de vencimiento
                    (!filters.FechaDesde.HasValue || e.Fecha.Date >= filters.FechaDesde.Value) &&

                    (!filters.FechaHasta.HasValue || e.Fecha.Date <= filters.FechaHasta.Value) &&

                    (string.IsNullOrEmpty(filters.NumeroRemito) ||
                        (e.NumeroRemito != null && e.NumeroRemito.Contains(filters.NumeroRemito))),

                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize,
                orderBy: orderByLogic<RemitoIngreso>(filters),
                include: new[] { "Detalles", "Detalles.Lote", "Deposito", "Proveedor" }
            );

            var remitos = resultadoEntidad.Items.Select(e => new RemitoResponseGet(
                Id: e.Id,
                NumeroRemito: e.NumeroRemito,
                ComprobanteAsociado: e.ComprobanteAsociado,
                Fecha: e.Fecha,
                DepositoId: e.DepositoId,
                NombreDeposito: e.Deposito.Nombre,
                EntidadId: e.ProveedorId,
                NombreEntidad: e.Proveedor.RazonSocial,
                Observaciones: e.Observaciones,
                Detalles: e.Detalles.Select(d => new RemitoDetalleResponse(
                    ProductoId: d.ProductoId,
                    CodigoLote: d.Lote != null ? d.Lote.CodigoLote.ToUpper() : null,
                    FechaVencimiento: d.Lote != null ? d.Lote.FechaVencimiento : (DateTime?)null,
                    NumeroSerie: d.NumeroSerie,
                    Cantidad: d.Cantidad
                )).ToList()
            )).ToList();

            return new PagedResult<RemitoResponseGet> { 
                Items = remitos,
                TotalCount = resultadoEntidad.TotalCount,
                PageNumber = filters.PageNumber,
                PageSize = filters.PageSize
            };
        }

        public async Task<PagedResult<RemitoResponseGet>> GetRemitosEgreso(RemitoFilters filters)
        {
            var resultadoEntidad = await _repository.GetFiltered<RemitoEgreso>(
                predicate: e =>
                    (!filters.DepositoId.HasValue || e.DepositoId == filters.DepositoId.Value) &&
                    (!filters.EntidadId.HasValue || e.ClienteId == filters.EntidadId.Value) &&

                    // Filtros de vencimiento
                    (!filters.FechaDesde.HasValue || e.Fecha.Date >= filters.FechaDesde.Value) &&

                    (!filters.FechaHasta.HasValue || e.Fecha.Date <= filters.FechaHasta.Value) &&

                    (string.IsNullOrEmpty(filters.NumeroRemito) ||
                        (e.NumeroRemito != null && e.NumeroRemito.Contains(filters.NumeroRemito))),

                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize,
                orderBy: orderByLogic<RemitoEgreso>(filters),
                include: new[] { "Detalles", "Detalles.Lote", "Deposito", "Cliente" }
            );

            var remitos = resultadoEntidad.Items.Select(e => new RemitoResponseGet(
                Id: e.Id,
                NumeroRemito: e.NumeroRemito,
                ComprobanteAsociado: e.ComprobanteAsociado,
                Fecha: e.Fecha,
                DepositoId: e.DepositoId,
                NombreDeposito: e.Deposito.Nombre,
                EntidadId: e.ClienteId,
                NombreEntidad: e.Cliente.RazonSocial,
                Observaciones: e.Observaciones,
                Detalles: e.Detalles.Select(d => new RemitoDetalleResponse(
                    ProductoId: d.ProductoId,
                    CodigoLote: d.Lote != null ? d.Lote.CodigoLote.ToUpper() : null,
                    FechaVencimiento: d.Lote != null ? d.Lote.FechaVencimiento : (DateTime?)null,
                    NumeroSerie: d.NumeroSerie,
                    Cantidad: d.Cantidad
                )).ToList()
            )).ToList();

            return new PagedResult<RemitoResponseGet>
            {
                Items = remitos,
                TotalCount = resultadoEntidad.TotalCount,
                PageNumber = filters.PageNumber,
                PageSize = filters.PageSize
            };
        }

        public async Task<SiguienteRemitoResponse> GetSiguienteNumeroRemito(UltimoRemitoRequest request)
        {
            RemitoBase ultimoRemito = null;
            var digitoRemito = request.EsIngreso ? "ING" : "EGR";

            // 1. Buscar en la base de datos
            if (request.EsIngreso)
            {
                ultimoRemito = await _repository.Last<RemitoIngreso>(r => r.DepositoId == request.DepositoId);
            }
            else
            {
                ultimoRemito = await _repository.Last<RemitoEgreso>(r => r.DepositoId == request.DepositoId);
            }

            // 2. Variables iniciales
            var depositoFormateado = request.DepositoId.ToString("D3");
            int ultimoNumero = 0; // Por defecto empezamos en 0

            // 3. Validar PRIMERO si existe un remito anterior
            if (ultimoRemito != null && !string.IsNullOrEmpty(ultimoRemito.NumeroRemito))
            {
                // Forma segura: cortamos por el guion y tomamos la última parte (ej: de "001-ING-045" toma "045")
                var partes = ultimoRemito.NumeroRemito.Split('-');
                if (partes.Length >= 3)
                {
                    int.TryParse(partes.Last(), out ultimoNumero);
                }
            }

            // 4. LA MAGIA: Interpolación de strings (escribe las variables dentro de las llaves)
            // El :D7 adentro de la llave formatea el número a 7 ceros automáticamente
            string numeroSiguienteStr = $"{depositoFormateado}-{digitoRemito}-{(ultimoNumero + 1):D7}";

            return new SiguienteRemitoResponse(SiguienteNumeroRemito: numeroSiguienteStr);
        }

     

    }
    public class ResultadoOperacion<T>
    {
        public bool Exito { get; set; }
        public string? MensajeGeneral { get; set; }
        public List<ErrorDetalleDto> ErroresDetalle { get; set; } = new();
        public T? Datos { get; set; }

        public static ResultadoOperacion<T> Ok(T datos) =>
            new() { Exito = true, Datos = datos };

        public static ResultadoOperacion<T> Fallo(string mensaje, List<ErrorDetalleDto> errores = null) =>
            new() { Exito = false, MensajeGeneral = mensaje, ErroresDetalle = errores ?? new List<ErrorDetalleDto>() };
    }
}
