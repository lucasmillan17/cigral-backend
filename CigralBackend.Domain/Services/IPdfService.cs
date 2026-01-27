using System.Threading.Tasks;

namespace CigralBackend.Domain.Services
{
    /// <summary>
    /// Interfaz para servicios de generación de PDFs.
    /// La implementación está en Infrastructure con QuestPDF.
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Genera un PDF de remito de ingreso.
        /// </summary>
        /// <param name="remitoId">ID del remito de ingreso</param>
        /// <returns>Bytes del PDF generado</returns>
        Task<byte[]> GenerarPdfRemitoIngreso(int remitoId);

        /// <summary>
        /// Genera un PDF de remito de egreso.
        /// </summary>
        /// <param name="remitoId">ID del remito de egreso</param>
        /// <returns>Bytes del PDF generado</returns>
        Task<byte[]> GenerarPdfRemitoEgreso(int remitoId);
    }
}
