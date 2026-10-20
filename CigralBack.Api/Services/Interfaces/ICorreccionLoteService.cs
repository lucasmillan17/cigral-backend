using CigralBackend.Application.Dtos;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>Corrección de soporte de un lote de un producto (código y/o vencimiento).</summary>
    public interface ICorreccionLoteService
    {
        /// <summary>Qué pasaría al renombrar el lote, sin cambiar nada.</summary>
        Task<CorreccionLotePrevia> VistaPrevia(int productoId, string codigoLote, string? codigoNuevo);

        /// <summary>
        /// Renombra el lote del producto (fusionando si el código nuevo ya existe) y/o cambia su vencimiento
        /// en todas sus existencias y líneas de remito.
        /// </summary>
        Task<CorreccionLoteResponse> Corregir(CorreccionLoteRequest r);
    }
}
