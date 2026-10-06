namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Opciones de configuración para el servidor SMTP.
/// Se cargan desde las variables de entorno o appsettings (sección "Smtp").
/// </summary>
public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Firmeza Tienda";
    public bool EnableSsl { get; set; } = true;
}
