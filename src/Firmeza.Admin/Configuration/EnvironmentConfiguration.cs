using DotNetEnv;

namespace Firmeza.Admin.Configuration;

/// <summary>
/// Carga variables desde un archivo .env (solo desarrollo local).
/// Vive en la capa Admin (composition root): las demás capas reciben
/// la configuración por IConfiguration y no conocen DotNetEnv.
/// </summary>
public static class EnvironmentConfiguration
{
    // Carga las variables de entorno desde el archivo .env si existe.
    public static void LoadDotEnv()
    {
        Env.TraversePath().Load();
    }
}