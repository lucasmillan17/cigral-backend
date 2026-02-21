using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
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

        public RemitoService(IRepository repository, IExistenciaService existenciaService)
        {
            _repository = repository;
            _existenciaService = existenciaService;
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
            if (!string.IsNullOrEmpty(request.NumeroRemito))
            {
                var existeNumero = await _repository.First<RemitoIngreso>(
                    r => r.NumeroRemito == request.NumeroRemito
                );

                if (existeNumero != null)
                {
                    throw new DomainException(
                        DomainErrorCode.NumeroRemitoDuplicado,
                        $"El número de remito '{request.NumeroRemito}' ya existe."
                    );
                }
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
                    Observaciones = request.Observaciones,
                    ProveedorId = request.EntidadId,
                    DepositoId = request.DepositoId
                };

                remito = await _repository.Add(remito);

                int cantidadTotal = 0;

                // Procesar cada detalle
                foreach (var detalle in request.Detalles)
                {
                    Lote lote = null;
                    if (!string.IsNullOrEmpty(detalle.CodigoLote))
                    {
                            lote = await _repository.First<Lote>(d => d.CodigoLote == detalle.CodigoLote);
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
                        CodigoLote: detalle.CodigoLote,
                        FechaVencimiento: detalle.FechaVencimiento,
                        Cantidad: detalle.Cantidad
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
        public async Task<RemitoResponse> RegistrarEgreso(RemitoRequest request)
        {
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
            if (!string.IsNullOrEmpty(request.NumeroRemito))
            {
                var existeNumero = await _repository.First<RemitoEgreso>(
                    r => r.NumeroRemito == request.NumeroRemito
                );
                
                if (existeNumero != null)
                {
                    throw new DomainException(
                        DomainErrorCode.NumeroRemitoDuplicado,
                        $"El número de remito '{request.NumeroRemito}' ya existe."
                    );
                }
            }

            using var transaction = await _repository.BeginTransaction();
            try
            {
                // Crear la cabecera del remito
                var remito = new RemitoEgreso
                {
                    Fecha = DateTime.Now,
                    NumeroRemito = request.NumeroRemito,
                    Observaciones = request.Observaciones,
                    ClienteId = request.EntidadId,
                    DepositoId = request.DepositoId
                };

                remito = await _repository.Add(remito);

                int cantidadTotal = 0;

                // Procesar cada detalle
                foreach (var detalle in request.Detalles)
                {
                    var lote = await _repository.First<Lote>(d => d.CodigoLote == detalle.CodigoLote);
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
                        CodigoLote: lote?.CodigoLote,
                        FechaVencimiento: detalle.FechaVencimiento,
                        Cantidad: detalle.Cantidad
                    );

                    await _existenciaService.DisminuirStock(
                        existenciaRequest,
                        remitoEgresoId: remito.Id,
                        observaciones: $"Remito de egreso {request.NumeroRemito ?? remito.Id.ToString()}"
                    );

                    cantidadTotal += detalle.Cantidad;
                }

                await transaction.CommitAsync();

                return new RemitoResponse(
                    Id: remito.Id,
                    NumeroRemito: remito.NumeroRemito,
                    Fecha: remito.Fecha,
                    DepositoId: remito.DepositoId,
                    EntidadId: remito.ClienteId,
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

                await _repository.Update(remito);

                return new RemitoResponse(
                    Id: remito.Id,
                    NumeroRemito: remito.NumeroRemito,
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
                    Fecha: remito.Fecha,
                    DepositoId: remito.DepositoId,
                    EntidadId: remito.ClienteId,
                    Observaciones: remito.Observaciones,
                    CantidadDetalles: 0, // No calculamos aquí
                    CantidadTotal: 0
                );
            }
        }
    }
}
