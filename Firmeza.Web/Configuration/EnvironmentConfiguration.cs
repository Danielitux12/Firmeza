using DotNetEnv;

namespace Firmeza.Configuration;

/// <summary>
/// Carga variables desde un archivo .env (solo desarrollo local).
/// Vive en la capa Web (composition root): las demás capas reciben
/// la configuración por IConfiguration y no conocen DotNetEnv.
/// </summary>
public static class EnvironmentConfiguration
{
    public static void LoadDotEnv()
    {
        Env.TraversePath().Load();
    }
}