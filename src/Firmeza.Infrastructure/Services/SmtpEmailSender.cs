using System.IO;
using System.Net;
using System.Net.Mail;
using Firmeza.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Implementación de IEmailSender usando SMTP estándar de .NET (System.Net.Mail).
/// Compatible con Gmail, Outlook o cualquier servidor SMTP configurado por variables de entorno.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _logger = logger;
        _settings = new SmtpSettings
        {
            Host = configuration["SMTP_HOST"] ?? configuration["Smtp:Host"] ?? "smtp.gmail.com",
            Port = int.TryParse(configuration["SMTP_PORT"] ?? configuration["Smtp:Port"], out var port) ? port : 587,
            Username = configuration["SMTP_USERNAME"] ?? configuration["Smtp:Username"] ?? string.Empty,
            Password = configuration["SMTP_PASSWORD"] ?? configuration["Smtp:Password"] ?? string.Empty,
            FromEmail = configuration["SMTP_FROM_EMAIL"] ?? configuration["Smtp:FromEmail"] ?? configuration["SMTP_USERNAME"] ?? "noreply@firmeza.com",
            FromName = configuration["SMTP_FROM_NAME"] ?? configuration["Smtp:FromName"] ?? "Firmeza Tienda",
            EnableSsl = true
        };
    }

    /// <summary>
    /// Envía un correo electrónico simple en formato HTML.
    /// </summary>
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        await SendEmailInternalAsync(toEmail, subject, htmlBody, null, null, cancellationToken);
    }

    /// <summary>
    /// Envía un correo electrónico con un archivo adjunto binario (por ejemplo, el recibo PDF).
    /// </summary>
    public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachmentBytes, string attachmentFileName, CancellationToken cancellationToken = default)
    {
        await SendEmailInternalAsync(toEmail, subject, htmlBody, attachmentBytes, attachmentFileName, cancellationToken);
    }

    /// <summary>
    /// Método privado que centraliza la creación y envío del mensaje SMTP con manejo seguro de excepciones.
    /// </summary>
    private async Task SendEmailInternalAsync(string toEmail, string subject, string htmlBody, byte[]? attachmentBytes, string? attachmentFileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Si las credenciales SMTP no están configuradas, registramos en log y no rompemos la app académica
        if (string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password) || _settings.Password == "tu_app_password_aqui")
        {
            _logger.LogWarning("El correo hacia '{ToEmail}' no se envió porque las credenciales SMTP no están configuradas en .env / variables de entorno.", toEmail);
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            if (attachmentBytes != null && !string.IsNullOrWhiteSpace(attachmentFileName))
            {
                var stream = new MemoryStream(attachmentBytes);
                var attachment = new Attachment(stream, attachmentFileName, "application/pdf");
                message.Attachments.Add(attachment);
            }

            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Correo enviado exitosamente a {ToEmail} con asunto '{Subject}'", toEmail, subject);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Envío de correo a {ToEmail} cancelado.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo SMTP a {ToEmail}: {Message}", toEmail, ex.Message);
        }
    }
}
