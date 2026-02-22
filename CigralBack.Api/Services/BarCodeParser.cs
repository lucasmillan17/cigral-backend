using CigralBackend.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    public class BarCodeParser : IBarCodeParser
    {
        // Caracter separador de campos (comilla simple) - solo para campos variables
        private const char FIELD_SEPARATOR = '\'';

        public BarCodeParsed Parse(string scannedCode)
        {
            if (string.IsNullOrEmpty(scannedCode))
                return new BarCodeParsed();

            var result = new BarCodeParsed();
            bool esValido = false;

            try
            {
                // Escaneo GTIN
                if (scannedCode.StartsWith("01"))
                {
                    scannedCode = scannedCode.Remove(0, 2); // Eliminar el AI del inicio para facilitar el parseo
                    var gtin = scannedCode.Substring(0, 14);
                    scannedCode = scannedCode.Remove(0, 14);
                    result.Gtin = gtin;
                    esValido = true;
                }
                // Escaneo fecha vencimiento
                if (scannedCode.StartsWith("17"))
                {
                    scannedCode = scannedCode.Remove(0, 2); // Eliminar el AI
                    var fechaStr = scannedCode.Substring(0, 6);
                    scannedCode = scannedCode.Remove(0, 6);
                    if (DateTime.TryParseExact(fechaStr, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaVencimiento))
                    {
                        result.FechaVencimiento = fechaVencimiento;
                    }
                }
                if(!esValido)
                {
                    // Si no se detectó un GTIN válido, no se continúa con el parseo de campos variables
                    return result;
                }
                // Separo campos variables por el separador
                var campos = scannedCode.Split(FIELD_SEPARATOR);
                foreach (var campo in campos)
                {
                    if (campo.StartsWith("10")) // Lote
                    {
                        var campoLote = campo.Remove(0, 2); // Eliminar el AI
                        result.Lote = campoLote;
                    }
                    else if (campo.StartsWith("21")) // Número de serie
                    {
                        var campoNumeroSerie = campo.Remove(0, 2); // Eliminar el AI
                        result.NumeroSerie = campoNumeroSerie;
                    }
                    else if (campo.StartsWith("30")) // Cantidad
                    {
                        var campoCantidad = campo.Remove(0, 2); // Eliminar el AI
                        if (int.TryParse(campoCantidad, out int cantidad))
                        {
                            result.Cantidad = cantidad;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new FormatException("El código escaneado no tiene un formato válido o contiene datos incompletos.");
            }

            if(result.EsValido && result.Cantidad <= 0)
            {
                result.Cantidad = 1; // Por defecto, si se detecta un GTIN válido pero no se especifica cantidad, se asume 1
            }

            return result;
        }
    }

    public class BarCodeParsed
    {
        /// <summary>
        /// El código GTIN completo de 14 dígitos.
        /// Úsalo para buscar el producto en la BD.
        /// </summary>
        public string Gtin { get; set; }

        public string Lote { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string NumeroSerie { get; set; }

        /// <summary>
        /// Cantidad detectada en el código (AI 30).
        /// Por defecto es 1 si no se especifica.
        /// </summary>
        public int Cantidad { get; set; } = 1;

        /// <summary>
        /// Indica si el parseo encontró al menos un GTIN válido.
        /// </summary>
        public bool EsValido => !string.IsNullOrEmpty(Gtin);
    }
}

