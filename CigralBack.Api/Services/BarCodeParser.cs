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

        private static readonly Dictionary<string, string> NombresAI = new Dictionary<string, string>
{
    // --- Identificadores de 2 y 3 dígitos principales ---
    { "00", "SSCC (Código Seriado Contenedor de Embarque)" },
    { "01", "GTIN (Número Global de Artículo Comercial)" },
    { "02", "GTIN de Artículos Contenidos" },
    { "10", "Número de Lote" },
    { "11", "Fecha de Producción" },
    { "12", "Fecha de Vencimiento" },
    { "13", "Fecha de Empaquetado" },
    { "15", "Fecha de Consumo Preferente" },
    { "17", "Fecha de Vencimiento Máxima" },
    { "20", "Número Variante" },
    { "21", "Número de Serie" },
    { "22", "Campos de Datos Secundarios" },
    { "240", "Identificación de Artículo Adicional" },
    { "241", "Número de Parte del Cliente" },
    { "242", "Número de Variación a Medida" },
    { "250", "Número de Serie Secundario" },
    { "251", "Referencia a Entidad de Fuente" },
    { "253", "GDTI (Identificador de Documento)" },
    { "254", "Componente de Extensión GLN" },
    { "30", "Cantidad de Artículos" },
    { "37", "Cantidad de Artículos Comerciales" },

    // --- Pesos y Medidas (Métricas) ---
    { "310", "Peso Neto (kg)" },
    { "311", "Longitud (m)" },
    { "312", "Ancho/Diámetro (m)" },
    { "313", "Profundidad/Altura (m)" },
    { "314", "Área (m²)" },
    { "315", "Volumen Neto (Litros)" },
    { "316", "Volumen Neto (m³)" },
    { "330", "Peso Logístico (kg)" },
    { "331", "Longitud Logística (m)" },
    { "332", "Ancho Logístico (m)" },
    { "333", "Profundidad Logística (m)" },
    { "334", "Área Logística (m²)" },
    { "335", "Volumen Logístico (Litros)" },
    { "336", "Volumen Logístico (cm³)" },
    { "337", "Kilogramos por m²" },

    // --- Pesos y Medidas (Imperiales) ---
    { "320", "Peso Neto (lb)" },
    { "321", "Longitud (pulgadas)" },
    { "322", "Longitud (pies)" },
    { "323", "Longitud (yardas)" },
    { "324", "Ancho (pulgadas)" },
    { "325", "Ancho (pies)" },
    { "326", "Ancho (yardas)" },
    { "327", "Profundidad (pulgadas)" },
    { "328", "Profundidad (pies)" },
    { "329", "Profundidad (yardas)" },
    { "340", "Peso Logístico (lb)" },
    { "341", "Longitud Logística (pulgadas)" },
    { "342", "Longitud Logística (pies)" },
    { "343", "Longitud Logística (yardas)" },
    { "344", "Ancho Logístico (pulgadas)" },
    { "345", "Ancho Logístico (pies)" },
    { "346", "Ancho Logístico (yardas)" },
    { "347", "Profundidad Logística (pulgadas)" },
    { "348", "Profundidad Logística (pies)" },
    { "349", "Profundidad Logística (yardas)" },
    { "350", "Área (pulgadas²)" },
    { "351", "Área (pies²)" },
    { "352", "Área (yardas²)" },
    { "353", "Área Logística (pulgadas²)" },
    { "354", "Área Logística (pies²)" },
    { "355", "Área Logística (yardas²)" },
    { "356", "Peso Neto (Libras Troy)" },
    { "357", "Peso Neto/Volumen (lb)" },
    { "360", "Volumen Neto (Cuarto de galón)" },
    { "361", "Volumen Neto (Galón USA)" },
    { "362", "Volumen Logístico (Cuarto de galón)" },
    { "363", "Volumen Logístico (Galón USA)" },
    { "364", "Volumen Neto (pulgadas³)" },
    { "365", "Volumen Neto (pies³)" },
    { "366", "Volumen Neto (yardas³)" },
    { "367", "Volumen Logístico (pulgadas³)" },
    { "368", "Volumen Logístico (pies³)" },
    { "369", "Volumen Logístico (yardas³)" },

    // --- Moneda, Referencias y Localizaciones ---
    { "390", "Monto Pagable (Moneda Local)" },
    { "391", "Monto Pagable (Moneda ISO)" },
    { "392", "Monto Pagable (Área única)" },
    { "393", "Monto Pagable Variable (Moneda ISO)" },
    { "400", "Número de Orden de Compra" },
    { "401", "Número de Consignación" },
    { "402", "Número de Envío" },
    { "403", "Código de Enrutamiento" },
    { "410", "GLN - Entregar A" },
    { "411", "GLN - Facturar A" },
    { "412", "GLN - Comprado De" },
    { "413", "GLN - Enviar Para" },
    { "414", "GLN - Localización Física" },
    { "415", "GLN - Parte que Factura" },
    { "420", "Código Postal Destino" },
    { "421", "Código Postal y País ISO" },
    { "422", "País de Origen" },
    { "423", "País de Procesamiento Inicial" },
    { "424", "País de Procesamiento" },
    { "425", "País de Desensamblado" },
    { "426", "País de Proceso Completo" },

    // --- Identificadores Especiales (7000+ y 8000+) ---
    { "7001", "Número de Stock OTAN (NSN)" },
    { "7002", "Clasificación Carnes UN/ECE" },
    { "7003", "Fecha y Hora de Vencimiento" },
    { "7004", "Potencia Activa" },
    { "703",  "Aprobación de Procesador" },
    { "8001", "Productos Redondos (Dimensiones)" },
    { "8002", "Identificador Teléfono Celular" },
    { "8003", "Identificador Bienes Retornables (GRAI)" },
    { "8004", "Identificador Bienes Individuales (GIAI)" },
    { "8005", "Precio por Unidad" },
    { "8006", "Identificación de Componentes" },
    { "8007", "Cuenta Bancaria (IBAN)" },
    { "8008", "Fecha y Hora de Producción" },
    { "8018", "Relación de Servicio (GSRN)" },
    { "8020", "Referencia de Talón de Pago" },
    { "8100", "Código Cupón GS1-128" },
    { "8101", "Código Cupón Extendido" },
    { "8102", "Código Cupón Extendido" },
    { "8110", "Código Cupón EEUU" },
    { "90",   "Información Acordada (Socios)" },
    
    // Agregamos el bloque 91-99 de Uso Interno de la Compañía
    { "91", "Información Interna Compañía (91)" },
    { "92", "Información Interna Compañía (92)" },
    { "93", "Información Interna Compañía (93)" },
    { "94", "Información Interna Compañía (94)" },
    { "95", "Información Interna Compañía (95)" },
    { "96", "Información Interna Compañía (96)" },
    { "97", "Información Interna Compañía (97)" },
    { "98", "Información Interna Compañía (98)" },
    { "99", "Información Interna Compañía (99)" }
};

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

        private void AsignarValor(string ai, string valor, BarCodeParsed result, Dictionary<string, string> adicionales)
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
                    // LA MAGIA DE LA TRADUCCIÓN AQUÍ
                    string nombreClave = NombresAI.ContainsKey(ai) ? NombresAI[ai] : $"AI {ai}";

                    // Un pequeño truco para los pesos (que miden 4 dígitos y empiezan con 31, 32, etc.)
                    if (ai.Length == 4 && ai.StartsWith("310")) nombreClave = "Peso Neto (kg)";
                    if (ai.Length == 4 && ai.StartsWith("320")) nombreClave = "Peso Neto (lb)";

                    // Guardamos usando el nombre traducido en lugar del número
                    adicionales[nombreClave] = valor;
                    break;
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
            var camposAdicionales = new Dictionary<string, string>();
            bool esValido = false;

            if (scannedCode.Contains("$")) scannedCode = scannedCode.Replace('$', (char)29 ); // Normalizar separadores

            var cadenaRestante = scannedCode;

            while (cadenaRestante.Length > 0)
            {

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

                if (EsLongitudFija(ai, out int longitud))
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
            } 

            if (result.EsValido && result.Cantidad <= 0)
            {
                result.Cantidad = 1; // Por defecto, si se detecta un GTIN válido pero no se especifica cantidad, se asume 1
            }

            if (camposAdicionales.Any())
            {
                var opcionesJson = new System.Text.Json.JsonSerializerOptions
                {
                    // Esto le dice a .NET: "No escapes las tildes ni las eñes, déjalas en paz"
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                result.InformacionAdicional = System.Text.Json.JsonSerializer.Serialize(camposAdicionales, opcionesJson);
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



