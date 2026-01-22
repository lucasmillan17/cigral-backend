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
                            // Usar ParseExact con ajuste de año
                            if (DateTime.TryParseExact(fechaStr, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                            {
                                // Ajustar el año: si el año es menor a 50, asumimos 2000+, sino 1900+
                                // Por ejemplo: 30 -> 2030, 80 -> 1980
                                if (fecha.Year < 1950)
                                {
                                    fecha = fecha.AddYears(100);
                                }
                                result.FechaVencimiento = fecha;
                            }
                            currentIndex += 6;
                        }
                        else break;
                    }
                    else if (currentAi == "10")
                    {
                        // LOTE: Variable hasta 20
                        int endIndex = FindEndOfField(raw, currentIndex, "10");
                        int length = endIndex - currentIndex;

                        // Guardamos y quitamos el separador GS si existe
                        result.Lote = raw.Substring(currentIndex, length).Replace(GS.ToString(), "");

                        currentIndex = endIndex;
                    }
                    else if (currentAi == "21")
                    {
                        // SERIE: Variable hasta 20
                        int endIndex = FindEndOfField(raw, currentIndex, "21");
                        int length = endIndex - currentIndex;

                        result.NumeroSerie = raw.Substring(currentIndex, length).Replace(GS.ToString(), "");

                        currentIndex = endIndex;
                    }
                    else if (currentAi == "30")
                    {
                        // CANTIDAD: Variable hasta 8
                        int endIndex = FindEndOfField(raw, currentIndex, "30");
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
        /// Prioridad 2: Busca el siguiente AI válido (sin GS, el parser continúa hasta el final).
        /// </summary>
        private int FindEndOfField(string raw, int startIndex, string currentAi)
        {
            // 1. Buscamos el separador oficial <GS>
            int gsIndex = raw.IndexOf(GS, startIndex);
            if (gsIndex != -1) return gsIndex;

            // 2. Si no hay GS, intentamos encontrar el siguiente AI
            // Esto es complicado porque los AIs pueden aparecer dentro del contenido
            // Estrategia: buscar posiciones donde TODOS los AIs son candidatos y elegir el más cercano
            
            int minIndex = raw.Length;
            
            // Para cada AI que buscamos, intentamos encontrar la primera ocurrencia válida
            var aiCandidates = new[] { "01", "17", "10", "21", "30" };
            
            foreach (var ai in aiCandidates)
            {
                // No buscar el mismo AI que estamos parseando
                if (ai == currentAi)
                    continue;
                
                int candidate = FindNextValidAi(raw, startIndex, ai);
                if (candidate != -1 && candidate < minIndex)
                {
                    minIndex = candidate;
                }
            }

            return minIndex;
        }

        /// <summary>
        /// Encuentra la próxima ocurrencia válida de un AI específico.
        /// </summary>
        private int FindNextValidAi(string raw, int startIndex, string ai)
        {
            int position = startIndex;
            
            while (position < raw.Length - 1)
            {
                int idx = raw.IndexOf(ai, position);
                
                if (idx == -1)
                    return -1;
                
                // Verificar si esta posición es un AI válido
                if (idx + 2 <= raw.Length)
                {
                    // Para AI "01" (GTIN): debe tener exactamente 14 dígitos después
                    if (ai == "01" && idx + 16 <= raw.Length)
                    {
                        string content = raw.Substring(idx + 2, 14);
                        if (content.All(char.IsDigit))
                            return idx;
                    }
                    // Para AI "17" (Fecha): debe tener exactly 6 dígitos y ser fecha válida
                    else if (ai == "17" && idx + 8 <= raw.Length)
                    {
                        string content = raw.Substring(idx + 2, 6);
                        if (content.All(char.IsDigit) && 
                            DateTime.TryParseExact(content, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            return idx;
                    }
                    // Para AI "30" (Cantidad): debe venir seguido de dígitos
                    else if (ai == "30" && idx + 3 <= raw.Length)
                    {
                        char nextChar = raw[idx + 2];
                        if (char.IsDigit(nextChar))
                            return idx;
                    }
                    // Para AI "10" y "21": aceptamos si tiene contenido después
                    else if ((ai == "10" || ai == "21") && idx + 3 <= raw.Length)
                    {
                        return idx;
                    }
                }
                
                // No fue válido, continuar buscando
                position = idx + 1;
            }
            
            return -1;
        }

        /// <summary>
        /// Verifica si un string de 2 caracteres es un AI conocido.
        /// </summary>
        private bool IsKnownAi(string ai)
        {
            return ai == "01" || ai == "10" || ai == "17" || ai == "21" || ai == "30";
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

