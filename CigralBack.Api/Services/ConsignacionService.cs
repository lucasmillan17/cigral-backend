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
    public class ConsignacionService : IConsignacionService
    {
        private readonly IRepository _repository;
        public ConsignacionService(IRepository repository) 
        {
            _repository = repository;
        }

        public async Task<ConsignacionResponse> AumentarConsignacion(ConsignacionRequest request)
        {
            // 1. Validar Entidades Principales (Una sola llamada a la BD por entidad)
            var existenciaActual = await _repository.GetById<Existencia>(request.ExistenciaId);
            if (existenciaActual == null)
            {
                throw new NotFoundException(nameof(Existencia), request.ExistenciaId);
            }

            var cliente = await _repository.GetById<Cliente>(request.ClienteId);
            if (cliente == null)
            {
                throw new NotFoundException(nameof(Cliente), request.ClienteId);
            }

            // 2. Traer TODAS las consignaciones de esta existencia
            // Esto es crucial para saber cuánto stock está bloqueado por OTROS clientes
            var consignacionesExistencia = await _repository.GetFiltered<Consignacion>(
                c => c.ExistenciaId == request.ExistenciaId,
                pageNumber: 1,
                pageSize: int.MaxValue
            );

            // 3. Calcular el Stock Real Disponible
            int totalYaConsignado = consignacionesExistencia.Items.Sum(c => c.Cantidad);
            int stockDisponibleReal = existenciaActual.Cantidad - totalYaConsignado;

            // 4. Validar que la cantidad extra que se pide no supere lo que queda libre
            if (request.Cantidad > stockDisponibleReal)
            {
                throw new DomainException(
                    DomainErrorCode.StockInsuficiente,
                    $"Stock insuficiente para hacer la consignación.\n" +
                    $" Stock Físico Total: {existenciaActual.Cantidad}\n" +
                    $" Reservado por todos los clientes: {totalYaConsignado}\n" +
                    $" Disponible Real: {stockDisponibleReal}\n" +
                    $" Solicitado: {request.Cantidad}"
                );
            }

            // 5. Buscar en la lista que ya trajimos a memoria si este cliente específico ya tiene una fila
            var consignacionDelCliente = consignacionesExistencia.Items
                .FirstOrDefault(c => c.ClienteId == request.ClienteId);

            if (consignacionDelCliente != null)
            {
                // ACTUALIZAR LA EXISTENTE
                consignacionDelCliente.Cantidad += request.Cantidad;
                consignacionDelCliente.FechaModificacion = DateTime.UtcNow;

                await _repository.Update(consignacionDelCliente);
            }
            else
            {
                // CREAR UNA NUEVA
                consignacionDelCliente = new Consignacion
                {
                    ExistenciaId = request.ExistenciaId,
                    ClienteId = request.ClienteId,
                    Cantidad = request.Cantidad,
                    FechaModificacion = DateTime.UtcNow
                };

                await _repository.Add(consignacionDelCliente);
            }

            // 6. Retornar el Response
            return new ConsignacionResponse(
                consignacionDelCliente.Id,
                consignacionDelCliente.ExistenciaId,
                cliente.RazonSocial,
                consignacionDelCliente.Cantidad,
                consignacionDelCliente.FechaModificacion
            );
        }

        public async Task<ConsignacionResponse?> DisminuirConsignacion(int consignacionId, int cantidadADisminuir)
        {
            // 1. Validar que la cantidad ingresada tenga sentido lógico
            if (cantidadADisminuir <= 0)
            {
                throw new DomainException(
                    DomainErrorCode.CantidadInvalida,
                    "La cantidad a disminuir de la consignación debe ser mayor a 0."
                );
            }

            // 2. Buscar la consignación existente
            var consignacion = await _repository.GetById<Consignacion>(consignacionId);
            if (consignacion == null)
            {
                throw new NotFoundException(nameof(Consignacion), consignacionId);
            }

            // 3. Validar que no se intente restar más stock del que está en consignación
            if (cantidadADisminuir > consignacion.Cantidad)
            {
                throw new DomainException(
                    DomainErrorCode.CantidadInvalida, // O podrías crear uno como ConsignacionInsuficiente
                    $"No se puede quitar {cantidadADisminuir} unidades. La consignación actual solo tiene {consignacion.Cantidad}."
                );
            }

            // 4. Aplicar la resta
            consignacion.Cantidad -= cantidadADisminuir;
            consignacion.FechaModificacion = DateTime.UtcNow;

            // 5. Lógica de eliminación o actualización
            if (consignacion.Cantidad == 0)
            {
                // Si el cliente ya retiró todo lo que tenía reservado, eliminamos el registro
                // para mantener la base de datos limpia.
                await _repository.Delete(consignacion);

                return null; // Retornamos null para indicarle al controlador que fue borrada
            }
            else
            {
                // Si aún queda saldo, actualizamos la fila
                await _repository.Update(consignacion);

                // Traemos el cliente solo para armar el Response de forma prolija
                var cliente = await _repository.GetById<Cliente>(consignacion.ClienteId);

                return new ConsignacionResponse(
                    consignacion.Id,
                    consignacion.ExistenciaId,
                    cliente?.RazonSocial ?? "Desconocido",
                    consignacion.Cantidad,
                    consignacion.FechaModificacion
                );
            }
        }

        public async Task<PagedResult<GetConsignacionResponse>> GetConsignaciones(ConsignacionFilters filters)
        {
            // 1. Lógica de ordenamiento
            Func<IQueryable<Consignacion>, IOrderedQueryable<Consignacion>> orderByLogic = q =>
                filters.EsDescendente
                    ? q.OrderByDescending(c => c.FechaModificacion)
                    : q.OrderBy(c => c.FechaModificacion);

            // 2. Ejecutar la consulta en la base de datos con los filtros de texto
            var resultadoEntidad = await _repository.GetFiltered<Consignacion>(
                predicate: c =>
                    (string.IsNullOrEmpty(filters.ClienteNombre) ||
                        (c.Cliente != null && c.Cliente.RazonSocial.Contains(filters.ClienteNombre))) &&

                    (string.IsNullOrEmpty(filters.ProductoNombre) ||
                        (c.Existencia.Producto != null && c.Existencia.Producto.Nombre.Contains(filters.ProductoNombre))) &&

                    (string.IsNullOrEmpty(filters.CodigoLote) ||
                        (c.Existencia.Lote != null && c.Existencia.Lote.CodigoLote.Contains(filters.CodigoLote))) &&

                    (string.IsNullOrEmpty(filters.NumSerie) ||
                        (c.Existencia.NumSerie != null && c.Existencia.NumSerie.Contains(filters.NumSerie))),

                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize,
                orderBy: orderByLogic,

                // Mantenemos "Existencia.Deposito" en los includes para poblar el DTO de respuesta
                include: new[] { "Cliente", "Existencia", "Existencia.Producto", "Existencia.Lote", "Existencia.Deposito" }
            );

            // 3. Proyectar la entidad al DTO
            var itemsDto = resultadoEntidad.Items.Select(c => new GetConsignacionResponse(
                Id: c.Id,
                ExistenciaId: c.ExistenciaId,
                ProductoNombre: c.Existencia?.Producto?.Nombre ?? "Sin Nombre",
                CodigoLote: c.Existencia?.Lote?.CodigoLote.ToUpper() ?? "Sin Lote",
                NumSerie: c.Existencia?.NumSerie ?? "Sin Serie",
                DepositoNombre: c.Existencia?.Deposito?.Nombre ?? "Sin Depósito",
                ClienteId: c.ClienteId,
                ClienteRazonSocial: c.Cliente?.RazonSocial ?? "Desconocido",
                Cantidad: c.Cantidad,
                FechaModificacion: c.FechaModificacion
            )).ToList();

            // 4. Retornar utilizando tu wrapper PagedResult
            return new PagedResult<GetConsignacionResponse>
            {
                Items = itemsDto,
                TotalCount = resultadoEntidad.TotalCount,
                PageNumber = resultadoEntidad.PageNumber,
                PageSize = resultadoEntidad.PageSize
            };
        }
    }
}
