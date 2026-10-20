using CigralBackend.Application.Dtos;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>Ajuste de stock por conteo físico (solo soporte).</summary>
    public interface IAjusteStockService
    {
        /// <summary>Todo o nada: si una línea falla, no se guarda nada y se informa su orden (0 = primera).</summary>
        Task<ResultadoOperacion<AjusteStockResponse>> Ajustar(AjusteStockRequest request);
    }
}
