using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para operaciones de depósitos.
    /// </summary>
    public class DepositoService : IDepositoService
    {
        private readonly IRepository _repository;

        public DepositoService(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Crea un nuevo depósito.
        /// </summary>
        public async Task<DepositoModelResponse> CreateDeposito(DepositoModelRequest request)
        {
            // Validar código único
            var existeCodigo = await _repository.First<Deposito>(d => d.Codigo == request.Codigo);
            if (existeCodigo != null)
            {
                throw new DomainException(
                    DomainErrorCode.CodigoDepositoDuplicado,
                    $"El código '{request.Codigo}' ya existe."
                );
            }

            var deposito = new Deposito
            {
                Nombre = request.Nombre,
                Codigo = request.Codigo,
                Activo = request.Activo
            };

            deposito = await _repository.Add(deposito);

            return new DepositoModelResponse(
                Id: deposito.Id,
                Nombre: deposito.Nombre,
                Codigo: deposito.Codigo,
                Activo: deposito.Activo
            );
        }

        /// <summary>
        /// Obtiene un depósito por su ID.
        /// </summary>
        public async Task<DepositoModelResponse> GetDepositoById(int id)
        {
            var deposito = await _repository.GetById<Deposito>(id);

            if (deposito == null)
            {
                throw new NotFoundException(nameof(Deposito), id);
            }

            return new DepositoModelResponse(
                Id: deposito.Id,
                Nombre: deposito.Nombre,
                Codigo: deposito.Codigo,
                Activo: deposito.Activo
            );
        }

        /// <summary>
        /// Obtiene depósitos filtrados con paginación.
        /// </summary>
        public async Task<PagedResult<DepositoModelResponse>> GetDepositos(DepositoFilters filters)
        {
            var resultado = await _repository.GetFiltered<Deposito>(
                predicate: d =>
                    (string.IsNullOrEmpty(filters.Nombre) || d.Nombre.Contains(filters.Nombre)) &&
                    (string.IsNullOrEmpty(filters.Codigo) || d.Codigo.Contains(filters.Codigo)) &&
                    (!filters.Activo.HasValue || d.Activo == filters.Activo.Value),
                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize
            );

            var itemsDto = resultado.Items.Select(d => new DepositoModelResponse(
                Id: d.Id,
                Nombre: d.Nombre,
                Codigo: d.Codigo,
                Activo: d.Activo
            )).ToList();

            return new PagedResult<DepositoModelResponse>
            {
                Items = itemsDto,
                TotalCount = resultado.TotalCount,
                PageNumber = resultado.PageNumber,
                PageSize = resultado.PageSize
            };
        }

        /// <summary>
        /// Actualiza un depósito existente.
        /// </summary>
        public async Task<DepositoModelResponse> UpdateDeposito(int id, DepositoModelRequest request)
        {
            var deposito = await _repository.GetById<Deposito>(id);
            if (deposito == null)
            {
                throw new NotFoundException(nameof(Deposito), id);
            }

            // Validar código único (si cambió)
            if (request.Codigo != deposito.Codigo)
            {
                var existeCodigo = await _repository.First<Deposito>(d => d.Codigo == request.Codigo && d.Id != id);
                if (existeCodigo != null)
                {
                    throw new DomainException(
                        DomainErrorCode.CodigoDepositoDuplicado,
                        $"El código '{request.Codigo}' ya existe en otro depósito."
                    );
                }
            }

            deposito.Nombre = request.Nombre;
            deposito.Codigo = request.Codigo;
            deposito.Activo = request.Activo;

            await _repository.Update(deposito);

            return new DepositoModelResponse(
                Id: deposito.Id,
                Nombre: deposito.Nombre,
                Codigo: deposito.Codigo,
                Activo: deposito.Activo
            );
        }

        /// <summary>
        /// Elimina un depósito.
        /// </summary>
        public async Task DeleteDeposito(int id)
        {
            var deposito = await _repository.GetById<Deposito>(id);
            if (deposito == null)
            {
                throw new NotFoundException(nameof(Deposito), id);
            }

            await _repository.Delete(deposito);
        }
    }
}
