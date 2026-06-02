using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Infraestructure.Services;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones de remitos de ingreso y egreso.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RemitosController : ControllerBase
    {
        private readonly IRemitoService _remitoService;
        private readonly IPdfService _pdfService;

        public RemitosController(IRemitoService remitoService, IPdfService pdfService)
        {
            _remitoService = remitoService;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Registra un remito de ingreso (entrada de mercadería de proveedor).
        /// Aumenta automáticamente el stock en el depósito especificado.
        /// </summary>
        /// <param name="request">Datos del remito de ingreso</param>
        /// <returns>Información del remito creado</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/remitos/ingreso
        ///     {
        ///        "depositoId": 1,
        ///        "entidadId": 5,
        ///        "numeroRemito": "REM-001",
        ///        "observaciones": "Ingreso de mercadería",
        ///        "detalles": [
        ///          {
        ///            "productoId": 10,
        ///            "loteId": 3,
        ///            "numeroSerie": null,
        ///            "cantidad": 100
        ///          }
        ///        ]
        ///     }
        ///
        /// </remarks>
        [HttpPost("ingreso")]
        [ProducesResponseType(typeof(RemitoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarIngreso([FromBody] RemitoRequest request)
        {
            var remito = await _remitoService.RegistrarIngreso(request);
            return CreatedAtAction(
                actionName: nameof(RegistrarIngreso),
                routeValues: new { id = remito.Id },
                value: remito
            );
        }

        /// <summary>
        /// Registra un remito de egreso (salida de mercadería a cliente).
        /// Disminuye automáticamente el stock en el depósito especificado.
        /// </summary>
        /// <param name="request">Datos del remito de egreso</param>
        /// <returns>Información del remito creado</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/remitos/egreso
        ///     {
        ///        "depositoId": 1,
        ///        "entidadId": 8,
        ///        "numeroRemito": "REM-SAL-001",
        ///        "observaciones": "Venta a cliente",
        ///        "detalles": [
        ///          {
        ///            "productoId": 10,
        ///            "loteId": 3,
        ///            "numeroSerie": null,
        ///            "cantidad": 50
        ///          }
        ///        ]
        ///     }
        ///
        /// </remarks>
        [HttpPost("egreso")]
        [ProducesResponseType(typeof(RemitoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegistrarEgreso([FromBody] RemitoRequest request)
        {
            var response = await _remitoService.RegistrarEgreso(request);

            if(!response.Exito)
            {
                return UnprocessableEntity(new
                {
                    mensaje = response.MensajeGeneral,
                    errores = response.ErroresDetalle
                });
            }
            return CreatedAtAction(
                actionName: nameof(RegistrarEgreso),
                routeValues: new { id = response.Datos.Id },
                value: response.Datos
            );
        }

        /// <summary>
        /// Actualiza un remito de ingreso (solo número y observaciones, NO afecta stock).
        /// </summary>
        /// <param name="id">ID del remito de ingreso</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Información del remito actualizado</returns>
        [HttpPut("ingreso/{id}")]
        [ProducesResponseType(typeof(RemitoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateIngreso(int id, [FromBody] UpdateRemitoRequest request)
        {
            var remito = await _remitoService.UpdateRemito(id, request, esIngreso: true);
            return Ok(remito);
        }

        /// <summary>
        /// Actualiza un remito de egreso (solo número y observaciones, NO afecta stock).
        /// </summary>
        /// <param name="id">ID del remito de egreso</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Información del remito actualizado</returns>
        [HttpPut("egreso/{id}")]
        [ProducesResponseType(typeof(RemitoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEgreso(int id, [FromBody] UpdateRemitoRequest request)
        {
            var remito = await _remitoService.UpdateRemito(id, request, esIngreso: false);
            return Ok(remito);
        }

        /// <summary>
        /// Genera e imprime un PDF del remito de ingreso.
        /// </summary>
        /// <param name="id">ID del remito de ingreso</param>
        /// <returns>Archivo PDF del remito</returns>
        /// <remarks>
        /// Retorna un archivo PDF que puede ser:
        /// - Descargado directamente por el navegador
        /// - Mostrado en un visor de PDF (iframe, modal, etc.)
        /// - Impreso directamente desde el frontend
        /// 
        /// Ejemplo de uso en frontend:
        /// 
        ///     // Descargar
        ///     window.open('/api/remitos/ingreso/5/pdf', '_blank');
        ///     
        ///     // Mostrar en iframe
        ///     &lt;iframe src="/api/remitos/ingreso/5/pdf" /&gt;
        ///     
        ///     // Fetch y crear blob
        ///     const response = await fetch('/api/remitos/ingreso/5/pdf');
        ///     const blob = await response.blob();
        ///     const url = URL.createObjectURL(blob);
        /// </remarks>
        [HttpGet("ingreso/{id}/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ImprimirRemitoIngreso(int id)
        {
            var pdfBytes = await _pdfService.GenerarPdfRemitoIngreso(id);

            return File(
                fileContents: pdfBytes,
                contentType: "application/pdf",
                fileDownloadName: $"Remito_Ingreso_{id}_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }

        /// <summary>
        /// Genera e imprime un PDF del remito de egreso.
        /// </summary>
        /// <param name="id">ID del remito de egreso</param>
        /// <returns>Archivo PDF del remito</returns>
        /// <remarks>
        /// Retorna un archivo PDF que puede ser:
        /// - Descargado directamente por el navegador
        /// - Mostrado en un visor de PDF (iframe, modal, etc.)
        /// - Impreso directamente desde el frontend
        /// 
        /// Ejemplo de uso en frontend:
        /// 
        ///     // Descargar
        ///     window.open('/api/remitos/egreso/10/pdf', '_blank');
        ///     
        ///     // Mostrar en modal
        ///     const response = await fetch('/api/remitos/egreso/10/pdf');
        ///     const blob = await response.blob();
        ///     const url = URL.createObjectURL(blob);
        ///     window.open(url, '_blank');
        /// </remarks>
        [HttpGet("egreso/{id}/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ImprimirRemitoEgreso(int id)
        {
            var pdfBytes = await _pdfService.GenerarPdfRemitoEgreso(id);

            return File(
                fileContents: pdfBytes,
                contentType: "application/pdf",
                fileDownloadName: $"Remito_Egreso_{id}_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }

        [HttpGet("ingreso")]
        [ProducesResponseType(typeof(PagedResult<RemitoResponseGet>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRemitosIngreso([FromQuery] RemitoFilters filters)
        {
            var remitos = await _remitoService.GetRemitosIngreso(filters);
            return Ok(remitos);
        }

        [HttpGet("siguiente-nro")]
        [ProducesResponseType(typeof(SiguienteRemitoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSiguienteNumeroRemito([FromBody]UltimoRemitoRequest r)
        {
            var siguienteRemito = await _remitoService.GetSiguienteNumeroRemito(r);
            return Ok(siguienteRemito);
        }

        [HttpGet("egreso")]
        [ProducesResponseType(typeof(PagedResult<RemitoResponseGet>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRemitosEgreso([FromQuery] RemitoFilters filters)
        {
            var remitos = await _remitoService.GetRemitosEgreso(filters);
            return Ok(remitos);
        }

        [HttpGet("Design")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRemitoDesign()
        {
            await _pdfService.GenerarPdfRemitoDisenio(); // ID fijo para diseño

            return Ok();
        }
    }
}
