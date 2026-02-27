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

        private int ObtenerLongitudAI(string cadenaRestante)
        {
            if (cadenaRestante.Length < 2) return 2; // Por seguridad

            string prefijo = cadenaRestante.Substring(0, 2);

            // 1. Reglas para 2 dígitos (Empiezan con 0, 1, 9, o son los específicos 20, 21, 22, 30, 37)
            if (prefijo.StartsWith("0") || prefijo.StartsWith("1") || prefijo.StartsWith("9") ||
                prefijo == "20" || prefijo == "21" || prefijo == "22" ||
                prefijo == "30" || prefijo == "37")
            {
                return 2;
            }

            // 2. Reglas para 3 dígitos (Empiezan con 4, o van del 23 al 25)
            if (prefijo.StartsWith("4") || prefijo == "24" || prefijo == "25" || (prefijo.StartsWith("3") && prefijo != "30" && prefijo != "37"))
            {
                return 3;
            }

            // 3. Reglas para 4 dígitos (Empiezan con 7, 8, o son los específicos de peso/moneda 31 al 36, 39)
            if (prefijo.StartsWith("7") || prefijo.StartsWith("8"))
            {
                return 4;
            }

            return 2; // Fallback por defecto
        }

        private bool EsLongitudFija(string ai, out int longitud)
        {
            longitud = 0;

            // --- REGLAS PARA AIs DE 2 DÍGITOS ---
            if (ai.Length == 2)
            {
                switch (ai)
                {
                    case "00": longitud = 18; return true; // SSCC
                    case "01":
                    case "02": longitud = 14; return true; // GTINs

                    // Todas las fechas (11 a 17) miden 6 caracteres (AAMMDD)
                    case "11":
                    case "12":
                    case "13":
                    case "14":
                    case "15":
                    case "16":
                    case "17":
                        longitud = 6; return true;

                    case "20": longitud = 2; return true;  // Variante
                }
            }

            // --- REGLAS PARA AIs DE 3 DÍGITOS ---
            if (ai.Length == 3)
            {
                // Los GLN de localización (410 al 415) siempre miden 13
                if (ai.StartsWith("41")) { longitud = 13; return true; }

                // Países (422, 424, 425, 426) siempre miden 3
                if (ai == "422" || ai == "424" || ai == "425" || ai == "426") { longitud = 3; return true; }
            }

            // --- REGLAS PARA AIs DE 4 DÍGITOS ---
            if (ai.Length == 4)
            {
                // ¡El gran truco para los pesos y medidas!
                // Todos los AIs que empiezan entre 31 y 36 (ej: 3102, 3300, 3650) 
                // siempre tienen un valor de exactamente 6 dígitos.
                if (int.TryParse(ai.Substring(0, 2), out int prefijo))
                {
                    if (prefijo >= 31 && prefijo <= 36)
                    {
                        longitud = 6;
                        return true;
                    }
                }
            }

            // Si llegó hasta aquí y no está en la lista de fijos, 
            // la regla de oro de GS1 es que DEBE llevar un separador <GS>.
            return false;
        }

        private void AsignarValor(string ai, string valor, BarCodeParsed result, List<string> adicionales)
        {
            switch (ai)
            {
                case "01":
                    result.Gtin = valor;
                    break;
                case "10":
                    result.Lote = valor;
                    break;
                case "21":
                    result.NumeroSerie = valor;
                    break;
                case "30":
                    if (int.TryParse(valor, out int cant)) result.Cantidad = cant;
                    break;
                case "17":
                case "12":
                    result.FechaVencimiento = ConvertirAFecha(valor);
                    break;
                default:
                    // Lo guardamos en el JSON con el formato original "IA + VALOR"
                    adicionales.Add(ai + valor);
                    break;
            }
        }

        private DateTime? ConvertirAFecha(string valorGS1)
        {
            // 1. Verificación de seguridad: debe tener exactamente 6 caracteres
            if (string.IsNullOrWhiteSpace(valorGS1) || valorGS1.Length != 6)
            {
                return null;
            }

            // 2. El truco de GS1: Si el día es "00", lo cambiamos a "01" 
            // para que C# no lance una excepción al crear el DateTime.
            if (valorGS1.EndsWith("00"))
            {
                valorGS1 = valorGS1.Substring(0, 4) + "01";
            }

            // 3. Conversión segura usando el formato exacto de GS1 (AñoMesDía)
            if (DateTime.TryParseExact(valorGS1, "yyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime fechaParseada))
            {
                return fechaParseada;
            }

            // Si el texto venía corrupto (ej: "ABCDEF"), devolvemos null en vez de romper el sistema
            return null;
        }

        // Caracter separador de campos (comilla simple) - solo para campos variables
        private const char FIELD_SEPARATOR = (char)29;

        public BarCodeParsed Parse(string scannedCode)
        {
            if (string.IsNullOrEmpty(scannedCode))
                return new BarCodeParsed();

            var result = new BarCodeParsed();
            var camposAdicionales = new List<string>();
            bool esValido = false;

            if (scannedCode.Contains("$")) scannedCode = scannedCode.Replace('$', (char)29 ); // Normalizar separadores

            var cadenaRestante = scannedCode;

            while (cadenaRestante.Length>0)
                // 0. Limpieza: Si quedó un separador <GS> suelto al principio, lo ignoramos
                if (cadenaRestante[0] == (char)29)
                {
                    cadenaRestante = cadenaRestante.Substring(1);
                    if (cadenaRestante.Length == 0) break;
                }

            int obtenerLongitudAI = ObtenerLongitudAI(cadenaRestante);

            string ai = cadenaRestante.Substring(0, obtenerLongitudAI);
            cadenaRestante = cadenaRestante.Substring(obtenerLongitudAI);

            string valor = "";

            if(EsLongitudFija(ai, out int longitud))
            {
                valor = cadenaRestante.Substring(0, longitud);
                cadenaRestante = cadenaRestante.Substring(longitud);
            }
            else
            {
                int indiceSeparador = cadenaRestante.IndexOf(FIELD_SEPARATOR);
                if (indiceSeparador >= 0)
                {
                    valor = cadenaRestante.Substring(0, indiceSeparador);
                    cadenaRestante = cadenaRestante.Substring(indiceSeparador + 1); // +1 para saltar el separador
                }
                else
                {
                    valor = cadenaRestante;
                    cadenaRestante = "";
                }
            }

            AsignarValor(ai, valor, result, camposAdicionales);

            /*try
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
                    switch (campo)
                    {
                        case var c when c.StartsWith("10"): // Lote
                            result.Lote = c[2..];
                            break;

                        case var c when c.StartsWith("21"): // Número de serie
                            result.NumeroSerie = c[2..];
                            break;

                        case var c when c.StartsWith("30"): // Cantidad
                            if (int.TryParse(c[2..], out int cantidad))
                            {
                                result.Cantidad = cantidad;
                            }
                            break;

                        // Aquí puedes seguir agregando los que descubriste hoy
                        case var c when c.StartsWith("11"): // Fecha de Producción
                                                            // result.FechaProduccion = ...
                            break;

                        case var c when c.StartsWith("240"): // Código interno (¡Ojo, este tiene 3 dígitos!)
                                                             // result.CodigoInterno = c[3..]; 
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw new FormatException("El código escaneado no tiene un formato válido o contiene datos incompletos.");
            }
            */

            if (result.EsValido && result.Cantidad <= 0)
            {
                result.Cantidad = 1; // Por defecto, si se detecta un GTIN válido pero no se especifica cantidad, se asume 1
            }

            if (camposAdicionales.Any())
            {
                result.InformacionAdicional = System.Text.Json.JsonSerializer.Serialize(camposAdicionales);
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

        public string InformacionAdicional { get; set; } // Para guardar cualquier otro campo variable que no hayamos mapeado explícitamente

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

