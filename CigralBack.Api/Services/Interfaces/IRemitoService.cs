using CigralBackend.Application.Dtos;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de remitos.
    /// </summary>
    public interface IRemitoService
    {
        /// <summary>
        /// Registra un remito de ingreso (entrada de mercadería de proveedor).
        /// </summary>
        /// <param name="request">Datos del remito de ingreso</param>
        /// <returns>Información del remito creado</returns>
        Task<RemitoResponse> RegistrarIngreso(RemitoRequest request);

        /// <summary>
        /// Registra un remito de egreso (salida de mercadería a cliente).
        /// </summary>
        /// <param name="request">Datos del remito de egreso</param>
        /// <returns>Información del remito creado</returns>
        Task<RemitoResponse> RegistrarEgreso(RemitoRequest request);

        /// <summary>
        /// Actualiza un remito existente (solo datos que no afectan stock).
        /// </summary>
        /// <param name="id">ID del remito</param>
        /// <param name="request">Datos a actualizar</param>
        /// <param name="esIngreso">True si es remito de ingreso, False si es de egreso</param>
        /// <returns>Información del remito actualizado</returns>
        Task<RemitoResponse> UpdateRemito(int id, UpdateRemitoRequest request, bool esIngreso);
    }
}
