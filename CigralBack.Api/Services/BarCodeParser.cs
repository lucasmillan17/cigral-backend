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
            // If scanner includes parentheses, prefer parsing using those delimiters
            if (!string.IsNullOrEmpty(scannedCode) && (scannedCode.Contains('(') || scannedCode.Contains(')')))
            {
                var parsedWithParens = ParseWithParentheses(scannedCode);
                if (parsedWithParens != null && parsedWithParens.EsValido)
                    return parsedWithParens;
                // else fall back to heuristics below
            }

            // 1. Limpieza inicial: quitamos paréntesis decorativos si el escáner los pone
            string raw = scannedCode.Replace("(", "").Replace(")", "");

            // Detect if original code had parentheses — used to choose heuristics
            bool hadParentheses = scannedCode.Contains('(') || scannedCode.Contains(')');

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
                        int gsIndex = raw.IndexOf(GS, currentIndex);

                        if (gsIndex == -1)
                        {
                            // No GS - apply advanced heuristic: try multiple splits of the digit run
                            int pos = currentIndex;
                            while (pos < raw.Length && char.IsDigit(raw[pos]) && pos - currentIndex < 20) // allow larger examination
                                pos++;

                            var digitRun = raw.Substring(currentIndex, pos - currentIndex);

                            // If no digits at all, fallback
                            if (string.IsNullOrEmpty(digitRun))
                            {
                                currentIndex = FindEndOfField(raw, currentIndex, "30");
                                continue;
                            }

                            int maxQtyLen = Math.Min(8, digitRun.Length);

                            int bestQty = -1;
                            string bestLote = null;
                            int bestScore = int.MinValue;
                            int bestEndIndex = currentIndex;

                            for (int qtyLen = 1; qtyLen <= maxQtyLen; qtyLen++)
                            {
                                string qtyStr = digitRun.Substring(0, qtyLen);
                                if (!int.TryParse(qtyStr, out int candidateQty))
                                    continue;

                                int loteStart = currentIndex + qtyLen;

                                // Determine end of lote field (until next AI or GS)
                                int loteEnd = FindEndOfField(raw, loteStart, "10");
                                int loteLen = Math.Max(0, loteEnd - loteStart);
                                string loteCandidate = loteLen > 0 ? raw.Substring(loteStart, loteLen).Replace(GS.ToString(), "") : string.Empty;

                                // If the loteCandidate starts with the qty digits, consider trimmed version
                                string trimmedLoteCandidate = loteCandidate;
                                if (!string.IsNullOrEmpty(loteCandidate) && loteCandidate.StartsWith(qtyStr))
                                {
                                    trimmedLoteCandidate = loteCandidate.Substring(qtyStr.Length);
                                }

                                // Score the candidate
                                int score = 0;

                                if (!string.IsNullOrEmpty(trimmedLoteCandidate)) score += 1;
                                // prefer lote with letters (likely a real lot)
                                if (trimmedLoteCandidate.Any(c => char.IsLetter(c))) score += 5;
                                // prefer keeping leading zeros in lote (small bonus)
                                if (trimmedLoteCandidate.StartsWith("0")) score += 1;
                                // if lote starts with a known AI, penalize heavily
                                if (trimmedLoteCandidate.Length >= 2 && IsKnownAi(trimmedLoteCandidate.Substring(0, 2))) score -= 20;
                                // prefer common quantity 10
                                if (candidateQty == 10) score += 30;
                                // penalize huge quantities
                                if (candidateQty > 1000000) score -= 2;
                                // prefer shorter qtyLen (avoid consuming too many digits from lote)
                                score -= qtyLen * 2; 

                                // Bonus if trimming removed a duplicated qty prefix
                                if (!string.IsNullOrEmpty(loteCandidate) && loteCandidate.StartsWith(qtyStr) && !string.IsNullOrEmpty(trimmedLoteCandidate))
                                {
                                    score += 8;
                                }

                                // Tie-breaker: prefer non-empty lote
                                if (score > bestScore || (score == bestScore && !string.IsNullOrEmpty(trimmedLoteCandidate) && string.IsNullOrEmpty(bestLote)))
                                {
                                    bestScore = score;
                                    bestQty = candidateQty;
                                    bestLote = trimmedLoteCandidate;
                                    bestEndIndex = loteEnd;
                                }
                            }

                            if (bestQty != -1)
                            {
                                result.Cantidad = bestQty;
                                if (!string.IsNullOrEmpty(bestLote)) result.Lote = bestLote;
                                currentIndex = bestEndIndex;
                                continue;
                            }

                            // Fallback: take entire digit run as quantity
                            if (int.TryParse(digitRun, out int qtyAll)) result.Cantidad = qtyAll;
                            currentIndex = pos;
                        }
                        else
                        {
                            // If GS exists, end of field at GS
                            int endIndex = FindEndOfField(raw, currentIndex, "30");
                            int length = endIndex - currentIndex;

                            string cantStr = raw.Substring(currentIndex, length).Replace(GS.ToString(), "");
                            if (int.TryParse(cantStr, out int qty))
                            {
                                result.Cantidad = qty;
                            }

                            currentIndex = endIndex;
                        }
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
                            // Si no hay separador, assumos que es basura y terminamos el parseo
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

        private BarCodeParsed ParseWithParentheses(string scanned)
        {
            var result = new BarCodeParsed();
            int pos = 0;
            try
            {
                while (pos < scanned.Length)
                {
                    int open = scanned.IndexOf('(', pos);
                    if (open == -1) break;
                    int close = scanned.IndexOf(')', open + 1);
                    if (close == -1) break;

                    string ai = scanned.Substring(open + 1, close - open - 1);
                    if (ai.Length > 2) ai = ai.Substring(0, 2);

                    int contentStart = close + 1;
                    int nextOpen = scanned.IndexOf('(', contentStart);
                    int gsIndex = scanned.IndexOf(GS, contentStart);

                    int contentEnd = scanned.Length;
                    if (gsIndex != -1 && (nextOpen == -1 || gsIndex < nextOpen))
                        contentEnd = gsIndex;
                    else if (nextOpen != -1)
                        contentEnd = nextOpen;

                    string content = contentEnd > contentStart ? scanned.Substring(contentStart, contentEnd - contentStart) : string.Empty;

                    switch (ai)
                    {
                        case "01":
                            var cleaned = new string(content.Where(char.IsDigit).ToArray());
                            if (cleaned.Length >= 14) result.Gtin = cleaned.Substring(0, 14);
                            break;
                        case "17":
                            var fechaStr = new string(content.Where(char.IsDigit).ToArray());
                            if (fechaStr.Length >= 6 && DateTime.TryParseExact(fechaStr.Substring(0, 6), "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                            {
                                if (fecha.Year < 1950) fecha = fecha.AddYears(100);
                                result.FechaVencimiento = fecha;
                            }
                            break;
                        case "30":
                            var num = new string(content.Where(char.IsDigit).ToArray());
                            if (int.TryParse(num, out int qty)) result.Cantidad = qty;
                            break;
                        case "10":
                            result.Lote = content.Replace(GS.ToString(), "");
                            break;
                        case "21":
                            result.NumeroSerie = content.Replace(GS.ToString(), "");
                            break;
                    }

                    pos = contentEnd;
                    if (pos < scanned.Length && scanned[pos] == GS) pos++;
                }
            }
            catch
            {
                // ignore
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
            int minIndex = raw.Length;
            var aiCandidates = new[] { "01", "17", "10", "21", "30" };

            foreach (var ai in aiCandidates)
            {
                if (ai == currentAi) continue;
                int candidate = FindNextValidAi(raw, startIndex, ai);
                if (candidate != -1 && candidate < minIndex)
                {
                    // Validar que el contenido entre startIndex y candidate sea compatible con currentAi
                    var content = raw.Substring(startIndex, candidate - startIndex);
                    if (IsContentCompatibleWithAi(content, currentAi))
                    {
                        minIndex = candidate;
                    }
                }
            }

            return minIndex;
        }

        /// <summary>
        /// Determina si el contenido posible es compatible con el AI actual.
        /// Ej: AI 30 (cantidad) admite solo dígitos, AI 21/10 admiten alfanuméricos.
        /// </summary>
        private bool IsContentCompatibleWithAi(string content, string ai)
        {
            if (string.IsNullOrEmpty(content)) return false; // no aceptamos campos vacíos

            switch (ai)
            {
                case "30": // cantidad -> solo dígitos
                    return content.All(char.IsDigit) && content.Length <= 8;
                case "17": // fecha siempre tiene longitud fija (no llegamos aquí normalmente)
                case "01":
                    return false; // no variable
                case "21": // serie -> permitimos alfanuméricos y algunos símbolos
                case "10": // lote -> permitimos alfanuméricos y símbolos
                    return content.All(c => !char.IsControl(c));
                default:
                    return content.All(c => !char.IsControl(c));
            }
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

