using CigralBackend.Domain;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CigralBackend.Infraestructure.Services
{
    /// <summary>Avisos por mail al equipo de soporte.</summary>
    public interface IEmailService
    {
        /// <summary>
        /// Manda el aviso de un ticket nuevo. Nunca lanza excepción: si no hay SMTP configurado
        /// o el envío falla, lo registra en el log (el ticket ya quedó guardado).
        /// </summary>
        Task EnviarTicketNuevoAsync(Ticket ticket, IReadOnlyList<TicketAdjunto> fotos);
    }

    /// <summary>
    /// Envío por SMTP con MailKit. Configuración (variables de entorno en Easypanel):
    /// Smtp__Host, Smtp__Port (587 por defecto), Smtp__Usuario, Smtp__Password, Smtp__Remitente
    /// y Soporte__EmailDestino (una o varias direcciones separadas por coma).
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task EnviarTicketNuevoAsync(Ticket ticket, IReadOnlyList<TicketAdjunto> fotos)
        {
            var host = _config["Smtp:Host"];
            var destino = _config["Soporte:EmailDestino"];
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(destino))
            {
                _logger.LogInformation("Ticket #{Id}: no se envía mail porque falta configurar Smtp:Host o Soporte:EmailDestino.", ticket.Id);
                return;
            }

            try
            {
                var usuario = _config["Smtp:Usuario"];
                var remitente = _config["Smtp:Remitente"] ?? usuario ?? "soporte@cigral.com.ar";

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress("Cigral WMS", remitente));
                foreach (var direccion in destino.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    mensaje.To.Add(MailboxAddress.Parse(direccion));
                var etiqueta = ticket.Tipo == TipoTicket.Interfaz ? "Soporte #" + ticket.Id + " · Interfaz" : "Soporte #" + ticket.Id;
                mensaje.Subject = $"[{etiqueta}] {ticket.Titulo}";

                var cuerpo = new BodyBuilder { HtmlBody = ArmarHtml(ticket, fotos.Count) };
                foreach (var foto in fotos)
                    cuerpo.Attachments.Add(foto.NombreArchivo, foto.Datos, ContentType.Parse(foto.ContentType));
                mensaje.Body = cuerpo.ToMessageBody();

                using var smtp = new SmtpClient();
                var puerto = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
                await smtp.ConnectAsync(host, puerto, puerto == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);
                if (!string.IsNullOrWhiteSpace(usuario))
                    await smtp.AuthenticateAsync(usuario, _config["Smtp:Password"] ?? "");
                await smtp.SendAsync(mensaje);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Ticket #{Id}: aviso enviado a soporte.", ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket #{Id}: no se pudo enviar el mail a soporte.", ticket.Id);
            }
        }

        private static string ArmarHtml(Ticket t, int cantidadFotos)
        {
            return t.Tipo == TipoTicket.Interfaz ? ArmarHtmlInterfaz(t, cantidadFotos) : ArmarHtmlCodigo(t, cantidadFotos);
        }

        /// <summary>Aviso de un problema de la interfaz: descripción, contexto técnico y errores recientes.</summary>
        private static string ArmarHtmlInterfaz(Ticket t, int cantidadFotos)
        {
            static string E(string? v) => WebUtility.HtmlEncode(v ?? "");

            var sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#1b2230\">");
            sb.Append($"<h2 style=\"margin:0 0 4px\">Ticket #{t.Id} · Interfaz: {E(t.Titulo)}</h2>");
            sb.Append($"<p style=\"margin:0 0 16px;color:#5b6474\">{E(t.CreadoPorNombre)} · {t.FechaCreacion:dd/MM/yyyy HH:mm} · {E(t.Pantalla)} · app {E(t.VersionApp)}</p>");

            if (!string.IsNullOrWhiteSpace(t.Descripcion))
                sb.Append($"<p style=\"margin:0 0 4px\"><b>Descripción</b></p><p style=\"margin:0 0 16px;white-space:pre-wrap\">{E(t.Descripcion)}</p>");

            var ctx = LeerContexto(t.ContextoJson);
            if (ctx.Filas.Count > 0)
            {
                sb.Append("<p style=\"margin:0 0 4px\"><b>Contexto técnico</b></p>");
                sb.Append("<table cellpadding=\"4\" style=\"border-collapse:collapse;margin-bottom:16px\">");
                foreach (var (nombre, valor) in ctx.Filas)
                    sb.Append($"<tr><td style=\"color:#5b6474\">{E(nombre)}</td><td>{E(valor)}</td></tr>");
                sb.Append("</table>");
            }

            if (ctx.Errores.Count > 0)
            {
                sb.Append("<p style=\"margin:0 0 4px\"><b>Últimos errores</b></p>");
                sb.Append("<table cellpadding=\"4\" style=\"border-collapse:collapse;margin-bottom:16px\">");
                sb.Append("<tr style=\"background:#f0f2f6\"><th align=\"left\">Hora</th><th align=\"left\">Tipo</th><th align=\"left\">Método</th><th align=\"left\">Ruta</th><th align=\"left\">Estado</th><th align=\"left\">Mensaje</th></tr>");
                foreach (var e in ctx.Errores)
                    sb.Append($"<tr><td>{E(e.Hora)}</td><td>{E(e.Tipo)}</td><td>{E(e.Metodo)}</td><td>{E(e.Ruta)}</td><td>{E(e.Estado)}</td><td>{E(e.Mensaje)}</td></tr>");
                sb.Append("</table>");
            }

            sb.Append(cantidadFotos > 0
                ? "<p style=\"color:#5b6474\">Captura adjunta.</p>"
                : "<p style=\"color:#5b6474\">Sin captura.</p>");
            sb.Append("<p style=\"color:#5b6474\">Revisalo en la app: menú Soporte.</p></div>");
            return sb.ToString();
        }

        private record ErrorReciente(string? Hora, string? Tipo, string? Metodo, string? Ruta, string? Estado, string? Mensaje);
        private record DatosContexto(List<(string Nombre, string Valor)> Filas, List<ErrorReciente> Errores);

        /// <summary>
        /// Saca del JSON de contexto lo que se muestra en el mail. Todo es opcional y un JSON mal formado
        /// o con otra forma no hace fallar el envío: devuelve lo que pudo leer (o nada).
        /// </summary>
        private static DatosContexto LeerContexto(string? json)
        {
            var filas = new List<(string, string)>();
            var errores = new List<ErrorReciente>();
            if (string.IsNullOrWhiteSpace(json)) return new(filas, errores);
            try
            {
                using var doc = JsonDocument.Parse(json);
                var raiz = doc.RootElement;
                if (raiz.ValueKind != JsonValueKind.Object) return new(filas, errores);

                static string? Texto(JsonElement el, string nombre) =>
                    el.ValueKind == JsonValueKind.Object && el.TryGetProperty(nombre, out var v) && v.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Array)
                        ? v.ToString() : null;
                static string? Dimension(JsonElement raiz, string nombre)
                {
                    if (!raiz.TryGetProperty(nombre, out var d)) return null;
                    var ancho = Texto(d, "ancho");
                    var alto = Texto(d, "alto");
                    return ancho != null && alto != null ? $"{ancho} × {alto}" : null;
                }
                void Agregar(string nombre, string? valor)
                {
                    if (!string.IsNullOrWhiteSpace(valor)) filas.Add((nombre, valor));
                }

                Agregar("Resolución de la ventana", Dimension(raiz, "ventana"));
                Agregar("Resolución del monitor", Dimension(raiz, "monitor"));
                var zoom = Texto(raiz, "zoom");
                Agregar("Zoom", zoom);
                Agregar("Tema", Texto(raiz, "tema"));
                if (raiz.TryGetProperty("pantalla", out var pantalla))
                    Agregar("Pantalla", Texto(pantalla, "ruta"));
                Agregar("Error reportado", Texto(raiz, "errorReportado"));

                if (raiz.TryGetProperty("eventos", out var eventos) && eventos.ValueKind == JsonValueKind.Array)
                {
                    var lista = new List<ErrorReciente>();
                    foreach (var e in eventos.EnumerateArray())
                    {
                        var tipo = Texto(e, "tipo");
                        if (tipo == null || tipo == "navegacion") continue;
                        var detalle = e.TryGetProperty("detalle", out var d) ? d : default;
                        lista.Add(new ErrorReciente(Texto(e, "hora"), tipo, Texto(detalle, "metodo"), Texto(detalle, "ruta"), Texto(detalle, "estado"), Texto(detalle, "mensaje")));
                    }
                    errores.AddRange(lista.Skip(Math.Max(0, lista.Count - 5)));
                }
            }
            catch (Exception)
            {
                // JSON inválido o con otra forma: se manda el mail igual con lo que haya.
            }
            return new(filas, errores);
        }

        private static string ArmarHtmlCodigo(Ticket t, int cantidadFotos)
        {
            static string E(string? v) => WebUtility.HtmlEncode(v ?? "");
            // Los separadores GS (0x1D) no se ven en un mail: se muestran como ⟨GS⟩.
            static string CodigoVisible(string? v) => E(v).Replace("\u001d", "<b style=\"color:#b42318\">⟨GS⟩</b>");

            var parser = LeerParser(t.ResultadoParserJson);
            var sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#1b2230\">");
            sb.Append($"<h2 style=\"margin:0 0 4px\">Ticket #{t.Id}: {E(t.Titulo)}</h2>");
            sb.Append($"<p style=\"margin:0 0 16px;color:#5b6474\">{E(t.CreadoPorNombre)} · {t.FechaCreacion:dd/MM/yyyy HH:mm} · {E(t.Pantalla)} · app {E(t.VersionApp)}</p>");

            if (!string.IsNullOrWhiteSpace(t.CodigoCrudo))
            {
                sb.Append("<p style=\"margin:0 0 4px\"><b>Código leído</b></p>");
                sb.Append($"<pre style=\"background:#f0f2f6;padding:8px;border-radius:6px;white-space:pre-wrap;word-break:break-all\">{CodigoVisible(t.CodigoCrudo)}</pre>");
                sb.Append($"<p style=\"margin:4px 0 16px;color:#5b6474\">Para probarlo: <code>{E(t.CodigoCrudo.Replace("\u001d", "$"))}</code></p>");
            }

            sb.Append("<table cellpadding=\"6\" style=\"border-collapse:collapse;margin-bottom:16px\">");
            sb.Append("<tr style=\"background:#f0f2f6\"><th align=\"left\"></th><th align=\"left\">Entendió el sistema</th><th align=\"left\">Dice la etiqueta</th></tr>");
            void Fila(string nombre, string? sistema, string? etiqueta)
            {
                var distinto = !string.IsNullOrWhiteSpace(etiqueta) && !string.Equals((sistema ?? "").Trim(), etiqueta.Trim(), StringComparison.OrdinalIgnoreCase);
                var estilo = distinto ? " style=\"color:#b42318;font-weight:600\"" : "";
                sb.Append($"<tr><td><b>{nombre}</b></td><td>{E(sistema ?? "-")}</td><td{estilo}>{E(string.IsNullOrWhiteSpace(etiqueta) ? "-" : etiqueta)}</td></tr>");
            }
            Fila("Producto", parser.Producto, null);
            Fila("GTIN", parser.Gtin, null);
            Fila("Lote", parser.Lote, t.LoteEtiqueta);
            Fila("Serie", parser.Serie, t.SerieEtiqueta);
            Fila("Vencimiento", parser.Vencimiento, t.VencimientoEtiqueta);
            sb.Append("</table>");

            if (!string.IsNullOrWhiteSpace(t.Descripcion))
                sb.Append($"<p style=\"margin:0 0 4px\"><b>Comentario</b></p><p style=\"margin:0 0 16px;white-space:pre-wrap\">{E(t.Descripcion)}</p>");
            if (cantidadFotos > 0)
                sb.Append($"<p style=\"color:#5b6474\">{cantidadFotos} foto(s) adjunta(s).</p>");
            sb.Append("<p style=\"color:#5b6474\">Revisalo en la app: menú Soporte.</p></div>");
            return sb.ToString();
        }

        private record DatosParser(string? Producto, string? Gtin, string? Lote, string? Serie, string? Vencimiento);

        /// <summary>Lee los campos útiles del JSON del parser; si no se puede, devuelve todo vacío.</summary>
        private static DatosParser LeerParser(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new(null, null, null, null, null);
            try
            {
                using var doc = JsonDocument.Parse(json);
                string? Campo(string nombre) =>
                    doc.RootElement.TryGetProperty(nombre, out var v) && v.ValueKind != JsonValueKind.Null ? v.ToString() : null;
                var venc = Campo("fechaVencimiento");
                if (venc is { Length: >= 10 } && DateTime.TryParse(venc[..10], out var f)) venc = f.ToString("dd/MM/yyyy");
                return new(Campo("nombreProducto"), Campo("gtin"), Campo("lote"), Campo("numeroSerie"), venc);
            }
            catch (JsonException)
            {
                return new(null, null, null, null, null);
            }
        }
    }
}
