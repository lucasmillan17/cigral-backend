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
    /// Servicio de aplicación para operaciones de proveedores.
    /// </summary>
    public class ProveedorService : IProveedorService
    {
        private readonly IRepository _repository;

        public ProveedorService(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Crea un nuevo proveedor.
        /// </summary>
        public async Task<ProveedorModelResponse> CreateProveedor(ProveedorModelRequest request)
        {
            // Validar GLN único (solo si se proporciona)
            if (!string.IsNullOrEmpty(request.GLN))
            {
                var existeGLN = await _repository.First<Proveedor>(p => p.GLN == request.GLN);
                if (existeGLN != null)
                {
                    throw new DomainException(
                        DomainErrorCode.GlnProveedorDuplicado,
                        $"El GLN '{request.GLN}' ya existe."
                    );
                }
            }

            // Validar CUIT único (si se proporciona)
            if (!string.IsNullOrEmpty(request.Cuit))
            {
                var existeCuit = await _repository.First<Proveedor>(p => p.Cuit == request.Cuit);
                if (existeCuit != null)
                {
                    throw new DomainException(
                        DomainErrorCode.CuitProveedorDuplicado,
                        $"El CUIT '{request.Cuit}' ya existe."
                    );
                }
            }

            var proveedor = new Proveedor
            {
                RazonSocial = request.RazonSocial,
                GLN = request.GLN,
                Email = request.Email,
                Cuit = request.Cuit,
                Telefono = request.Telefono,
                Direccion = request.Direccion
            };

            proveedor = await _repository.Add(proveedor);

            return new ProveedorModelResponse(
                Id: proveedor.Id,
                RazonSocial: proveedor.RazonSocial,
                GLN: proveedor.GLN,
                Email: proveedor.Email,
                Cuit: proveedor.Cuit,
                Telefono: proveedor.Telefono,
                Direccion: proveedor.Direccion
            );
        }

        /// <summary>
        /// Obtiene un proveedor por su ID.
        /// </summary>
        public async Task<ProveedorModelResponse> GetProveedorById(int id)
        {
            var proveedor = await _repository.GetById<Proveedor>(id);

            if (proveedor == null)
            {
                throw new NotFoundException(nameof(Proveedor), id);
            }

            return new ProveedorModelResponse(
                Id: proveedor.Id,
                RazonSocial: proveedor.RazonSocial,
                GLN: proveedor.GLN,
                Email: proveedor.Email,
                Cuit: proveedor.Cuit,
                Telefono: proveedor.Telefono,
                Direccion: proveedor.Direccion
            );
        }

        /// <summary>
        /// Obtiene proveedores filtrados con paginación.
        /// </summary>
        public async Task<PagedResult<ProveedorModelResponse>> GetProveedores(ProveedorFilters filters)
        {
            var resultado = await _repository.GetFiltered<Proveedor>(
                predicate: p =>
                    (string.IsNullOrEmpty(filters.RazonSocial) || p.RazonSocial.Contains(filters.RazonSocial)) &&
                    (string.IsNullOrEmpty(filters.GLN) || p.GLN.Contains(filters.GLN)) &&
                    (string.IsNullOrEmpty(filters.Cuit) || p.Cuit.Contains(filters.Cuit)),
                pageNumber: filters.PageNumber,
                pageSize: filters.PageSize
            );

            var itemsDto = resultado.Items.Select(p => new ProveedorModelResponse(
                Id: p.Id,
                RazonSocial: p.RazonSocial,
                GLN: p.GLN,
                Email: p.Email,
                Cuit: p.Cuit,
                Telefono: p.Telefono,
                Direccion: p.Direccion
            )).ToList();

            return new PagedResult<ProveedorModelResponse>
            {
                Items = itemsDto,
                TotalCount = resultado.TotalCount,
                PageNumber = resultado.PageNumber,
                PageSize = resultado.PageSize
            };
        }

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        public async Task<ProveedorModelResponse> UpdateProveedor(int id, ProveedorModelRequest request)
        {
            var proveedor = await _repository.GetById<Proveedor>(id);
            if (proveedor == null)
            {
                throw new NotFoundException(nameof(Proveedor), id);
            }

            // Validar GLN único (si se proporciona y si cambió)
            if (!string.IsNullOrEmpty(request.GLN) && request.GLN != proveedor.GLN)
            {
                var existeGLN = await _repository.First<Proveedor>(p => p.GLN == request.GLN && p.Id != id);
                if (existeGLN != null)
                {
                    throw new DomainException(
                        DomainErrorCode.GlnProveedorDuplicado,
                        $"El GLN '{request.GLN}' ya existe en otro proveedor."
                    );
                }
            }

            // Validar CUIT único (si cambió y se proporciona)
            if (!string.IsNullOrEmpty(request.Cuit) && request.Cuit != proveedor.Cuit)
            {
                var existeCuit = await _repository.First<Proveedor>(p => p.Cuit == request.Cuit && p.Id != id);
                if (existeCuit != null)
                {
                    throw new DomainException(
                        DomainErrorCode.CuitProveedorDuplicado,
                        $"El CUIT '{request.Cuit}' ya existe en otro proveedor."
                    );
                }
            }

            proveedor.RazonSocial = request.RazonSocial;
            proveedor.GLN = request.GLN;
            proveedor.Email = request.Email;
            proveedor.Cuit = request.Cuit;
            proveedor.Telefono = request.Telefono;
            proveedor.Direccion = request.Direccion;

            await _repository.Update(proveedor);

            return new ProveedorModelResponse(
                Id: proveedor.Id,
                RazonSocial: proveedor.RazonSocial,
                GLN: proveedor.GLN,
                Email: proveedor.Email,
                Cuit: proveedor.Cuit,
                Telefono: proveedor.Telefono,
                Direccion: proveedor.Direccion
            );
        }

        /// <summary>
        /// Elimina un proveedor.
        /// </summary>
        public async Task DeleteProveedor(int id)
        {
            var proveedor = await _repository.GetById<Proveedor>(id);
            if (proveedor == null)
            {
                throw new NotFoundException(nameof(Proveedor), id);
            }

            await _repository.Delete(proveedor);
        }
    }
}
