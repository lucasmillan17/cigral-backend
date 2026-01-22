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
        // Caracter separador invisible (Group Separator - ASCII 29)
        private const char GS = (char)29;

        public BarCodeParsed Parse(string scannedCode)
        {
            // 1. Limpieza inicial: quitamos paréntesis decorativos si el escáner los pone
            string raw = scannedCode.Replace("(", "").Replace(")", "");

            var result = new BarCodeParsed();
            int currentIndex = 0;

            try
            {
                // Bucle "Pac-Man": Comemos el string de izquierda a derecha
                while (currentIndex < raw.Length - 1) // -1 para asegurar que podemos leer al menos 2 chars de ID
                {
                    // Leemos el Identificador de Aplicación (AI) actual (2 dígitos)
                    string currentAi = raw.Substring(currentIndex, 2);
                    currentIndex += 2; // Avanzamos el puntero sobre el AI

                    if (currentAi == "01")
                    {
                        // GTIN: Longitud Fija 14
                        if (currentIndex + 14 <= raw.Length)
                        {
                            result.Gtin = raw.Substring(currentIndex, 14);
                            currentIndex += 14;
                        }
                        else break; // String roto/incompleto
                    }
                    else if (currentAi == "17")
                    {
                        // VENCIMIENTO: Longitud Fija 6 (AAMMDD)
                        if (currentIndex + 6 <= raw.Length)
                        {
                            string fechaStr = raw.Substring(currentIndex, 6);
                            if (DateTime.TryParseExact(fechaStr, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                            {
                                result.FechaVencimiento = fecha;
                            }
                            currentIndex += 6;
                        }
                        else break;
                    }
                    else if (currentAi == "10")
                    {
                        // LOTE: Variable hasta 20
                        int endIndex = FindEndOfField(raw, currentIndex);
                        int length = endIndex - currentIndex;

                        // Guardamos y quitamos el separador GS si existe
                        result.Lote = raw.Substring(currentIndex, length).Replace(GS.ToString(), "");

                        currentIndex = endIndex;
                    }
                    else if (currentAi == "21")
                    {
                        // SERIE: Variable hasta 20
                        int endIndex = FindEndOfField(raw, currentIndex);
                        int length = endIndex - currentIndex;

                        result.NumeroSerie = raw.Substring(currentIndex, length).Replace(GS.ToString(), "");

                        currentIndex = endIndex;
                    }
                    else if (currentAi == "30")
                    {
                        // CANTIDAD: Variable hasta 8
                        int endIndex = FindEndOfField(raw, currentIndex);
                        int length = endIndex - currentIndex;

                        string cantStr = raw.Substring(currentIndex, length).Replace(GS.ToString(), "");
                        if (int.TryParse(cantStr, out int qty))
                        {
                            result.Cantidad = qty;
                        }

                        currentIndex = endIndex;
                    }
                    else
                    {
                        // AI DESCONOCIDO:
                        // Si nos encontramos un AI que no soportamos, intentamos saltarlo.
                        // Como no sabemos si es fijo o variable, buscamos el siguiente GS o abortamos para seguridad.
                        int nextGs = raw.IndexOf(GS, currentIndex);
                        if (nextGs != -1)
                        {
                            currentIndex = nextGs + 1; // Saltamos hasta después del GS
                        }
                        else
                        {
                            // Si no hay separador, asumimos que es basura y terminamos el parseo
                            break;
                        }
                    }

                    // Si el campo terminaba en GS, debemos saltar ese caracter extra
                    if (currentIndex < raw.Length && raw[currentIndex] == GS)
                    {
                        currentIndex++;
                    }
                }
            }
            catch (Exception)
            {
                // En caso de error crítico (IndexOutOfRange), devolvemos lo que hayamos logrado rescatar hasta ahora.
            }

            return result;
        }

        /// <summary>
        /// Encuentra dónde termina un campo variable.
        /// Prioridad 1: Busca el caracter <GS>.
        /// Prioridad 2: Busca heurísticamente el siguiente AI conocido.
        /// </summary>
        private int FindEndOfField(string raw, int startIndex)
        {
            // 1. Buscamos el separador oficial <GS>
            int gsIndex = raw.IndexOf(GS, startIndex);
            if (gsIndex != -1) return gsIndex;

            // 2. Si no hay GS, usamos heurística buscando los AIs más comunes (10, 17, 21, 30)
            // Validamos que lo que sigue al AI tenga sentido para evitar falsos positivos dentro del dato.

            int minIndex = raw.Length;
            var candidates = new List<string> { "10", "17", "21", "30", "01" };

            foreach (var ai in candidates)
            {
                int idx = raw.IndexOf(ai, startIndex);

                // Mientras encontremos el AI en el string...
                while (idx != -1 && idx < minIndex)
                {
                    bool isValidCandidate = false;

                    // Validaciones rápidas para confirmar que es un AI real y no parte del Lote
                    if (ai == "17") // El 17 debe ser seguido de 6 dígitos numéricos
                    {
                        if (idx + 2 + 6 <= raw.Length)
                        {
                            string checkDigits = raw.Substring(idx + 2, 6);
                            if (long.TryParse(checkDigits, out _)) isValidCandidate = true;
                        }
                    }
                    else if (ai == "01") // El 01 debe ser seguido de 14 dígitos
                    {
                        if (idx + 2 + 14 <= raw.Length) isValidCandidate = true;
                    }
                    else
                    {
                        // Para 10, 21, 30 asumimos que es válido si lo encontramos
                        isValidCandidate = true;
                    }

                    if (isValidCandidate)
                    {
                        minIndex = idx;
                        break; // Encontramos el corte más cercano, salimos del while interno
                    }

                    // Falso positivo, buscamos la siguiente ocurrencia del mismo AI
                    idx = raw.IndexOf(ai, idx + 1);
                }
            }

            return minIndex;
        }
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

