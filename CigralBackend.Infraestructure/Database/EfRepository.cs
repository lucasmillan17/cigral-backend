using CigralBackend.Domain.Bases;
using CigralBackend.Infraestructure.Database.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CigralBackend.Domain.Wrappers;
using Microsoft.EntityFrameworkCore.Storage;

namespace CigralBackend.Infraestructure.Database
{
    /// <summary>
    /// Implementación del patrón Repository usando Entity Framework Core.
    /// Proporciona operaciones CRUD genéricas con soporte de paginación.
    /// </summary>
    public class EfRepository : IRepository
    {
        private readonly CigralBackendContext _context;

        /// <summary>
        /// Constructor que inicializa el repositorio con el contexto de base de datos.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework Core</param>
        public EfRepository(CigralBackendContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<T> Add<T>(T entity) where T : EntityBase
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<T> Delete<T>(T entity) where T : EntityBase
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
        {
            return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(predicate);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<T>> GetAll<T>(int pageNumber = 1, int pageSize = 10, params string[] include) where T : EntityBase
        {
            var query = Include(_context.Set<T>(), include);
            
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                
            };

        }

        /// <inheritdoc/>
        public async Task<T?> GetById<T>(int id, params string[] include) where T : EntityBase
        {
            return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<T>> GetFiltered<T>(Expression<Func<T, bool>> predicate, int pageNumber = 1, int pageSize = 10, params string[] include) where T : EntityBase
        {
            var query = Include(_context.Set<T>(), include).Where(predicate);
            
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<T> Update<T>(T entity) where T : EntityBase
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Inicia una transacción en la base de datos y devuelve el objeto de transacción.
        /// </summary>
        public async Task<IDbContextTransaction> BeginTransaction()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Aplica eager loading de propiedades de navegación especificadas.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <param name="query">Query base</param>
        /// <param name="includes">Array de nombres de propiedades a incluir</param>
        /// <returns>Query con includes aplicados</returns>
        private static IQueryable<T> Include<T>(IQueryable<T> query, string[] includes) where T : EntityBase
        {
            var includedQuery = query;

            foreach (var include in includes)
            {
                includedQuery = includedQuery.Include(include);
            }
            return includedQuery;
        }
    }
}
