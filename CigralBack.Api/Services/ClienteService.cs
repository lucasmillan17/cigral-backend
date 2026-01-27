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
    /// Servicio de aplicación para operaciones de clientes.
    /// </summary>
    public class ClienteService : IClienteService
    {
        private readonly IRepository _repository;

        public ClienteService(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Crea un nuevo cliente.
        /// </summary>
        public async Task<ClienteModelResponse> CreateCliente(ClienteModelRequest request)
        {
            // Validar GLN único (solo si se proporciona)
            if (!string.IsNullOrEmpty(request.GLN))
            {
                var existeGLN = await _repository.First<Cliente>(c => c.GLN == request.GLN);
                if (existeGLN != null)
                {
                    throw new DomainException(
                        DomainErrorCode.GlnClienteDuplicado,
                        $"El GLN '{request.GLN}' ya existe."
                    );
                }
            }

            // Validar CUIT único (si se proporciona)
            if (!string.IsNullOrEmpty(request.Cuit))
            {
                var existeCuit = await _repository.First<Cliente>(c => c.Cuit == request.Cuit);
                if (existeCuit != null)
                {
                    throw new DomainException(
                        DomainErrorCode.CuitClienteDuplicado,
                        $"El CUIT '{request.Cuit}' ya existe."
                    );
                }
            }

            var cliente = new Cliente
            {
                RazonSocial = request.RazonSocial,
                GLN = request.GLN,
                Email = request.Email,
                Cuit = request.Cuit,
                Telefono = request.Telefono,
                Direccion = request.Direccion
            };

            cliente = await _repository.Add(cliente);

            return new ClienteModelResponse(
                Id: cliente.Id,
                RazonSocial: cliente.RazonSocial,
                GLN: cliente.GLN,
                Email: cliente.Email,
                Cuit: cliente.Cuit,
                Telefono: cliente.Telefono,
                Direccion: cliente.Direccion
            );
        }

        /// <summary>
        /// Obtiene un cliente por su ID.
        /// </summary>
        public async Task<ClienteModelResponse> GetClienteById(int id)
        {
            var cliente = await _repository.GetById<Cliente>(id);

            if (cliente == null)
            {
                throw new NotFoundException(nameof(Cliente), id);
            }

            return new ClienteModelResponse(
                Id: cliente.Id,
                RazonSocial: cliente.RazonSocial,
                GLN: cliente.GLN,
                Email: cliente.Email,
                Cuit: cliente.Cuit,
                Telefono: cliente.Telefono,
                Direccion: cliente.Direccion
            );
        }

        /// <summary>
        /// Obtiene clientes filtrados con paginación.
        /// </summary>
        public async Task<PagedResult<ClienteModelResponse>> GetClientes(ClienteFilters filters)
        {
            var resultado = await _repository.GetFiltered<Cliente>(
                predicate: c =>
                    (string.IsNullOrEmpty(filters.RazonSocial) || c.RazonSocial.Contains(filters.RazonSocial)) &&
                    (string.IsNullOrEmpty(filters.GLN) || c.GLN.Contains(filters.GLN)) &&
                    (string.IsNullOrEmpty(filters.Cuit) || c.Cuit.Contains(filters.Cuit)),
                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize
            );

            var itemsDto = resultado.Items.Select(c => new ClienteModelResponse(
                Id: c.Id,
                RazonSocial: c.RazonSocial,
                GLN: c.GLN,
                Email: c.Email,
                Cuit: c.Cuit,
                Telefono: c.Telefono,
                Direccion: c.Direccion
            )).ToList();

            return new PagedResult<ClienteModelResponse>
            {
                Items = itemsDto,
                TotalCount = resultado.TotalCount,
                PageNumber = resultado.PageNumber,
                PageSize = resultado.PageSize
            };
        }

        /// <summary>
        /// Actualiza un cliente existente.
        /// </summary>
        public async Task<ClienteModelResponse> UpdateCliente(int id, ClienteModelRequest request)
        {
            var cliente = await _repository.GetById<Cliente>(id);
            if (cliente == null)
            {
                throw new NotFoundException(nameof(Cliente), id);
            }

            // Validar GLN único (si se proporciona y si cambió)
            if (!string.IsNullOrEmpty(request.GLN) && request.GLN != cliente.GLN)
            {
                var existeGLN = await _repository.First<Cliente>(c => c.GLN == request.GLN && c.Id != id);
                if (existeGLN != null)
                {
                    throw new DomainException(
                        DomainErrorCode.GlnClienteDuplicado,
                        $"El GLN '{request.GLN}' ya existe en otro cliente."
                    );
                }
            }

            // Validar CUIT único (si cambió y se proporciona)
            if (!string.IsNullOrEmpty(request.Cuit) && request.Cuit != cliente.Cuit)
            {
                var existeCuit = await _repository.First<Cliente>(c => c.Cuit == request.Cuit && c.Id != id);
                if (existeCuit != null)
                {
                    throw new DomainException(
                        DomainErrorCode.CuitClienteDuplicado,
                        $"El CUIT '{request.Cuit}' ya existe en otro cliente."
                    );
                }
            }

            cliente.RazonSocial = request.RazonSocial;
            cliente.GLN = request.GLN;
            cliente.Email = request.Email;
            cliente.Cuit = request.Cuit;
            cliente.Telefono = request.Telefono;
            cliente.Direccion = request.Direccion;

            await _repository.Update(cliente);

            return new ClienteModelResponse(
                Id: cliente.Id,
                RazonSocial: cliente.RazonSocial,
                GLN: cliente.GLN,
                Email: cliente.Email,
                Cuit: cliente.Cuit,
                Telefono: cliente.Telefono,
                Direccion: cliente.Direccion
            );
        }

        /// <summary>
        /// Elimina un cliente.
        /// </summary>
        public async Task DeleteCliente(int id)
        {
            var cliente = await _repository.GetById<Cliente>(id);
            if (cliente == null)
            {
                throw new NotFoundException(nameof(Cliente), id);
            }

            await _repository.Delete(cliente);
        }
    }
}
