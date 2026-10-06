namespace Firmeza.Application.Interfaces;

/// <summary>
/// Interfaz para el envío de correos electrónicos.
/// Permite desacoplar el envío de correos (SMTP, SendGrid, etc.) de la lógica de negocio.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Envía un correo electrónico simple (asunto y cuerpo HTML).
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);

    /// <summary>
    /// Envía un correo electrónico con un archivo adjunto (por ejemplo un recibo PDF).
    /// </summary>
    Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachmentBytes, string attachmentFileName);
}
