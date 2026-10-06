using DotNetEnv;

namespace Firmeza.API.Configuration;

/// <summary>
/// Carga las variables de entorno desde el archivo .env si existe.
/// Facilita el desarrollo local sin obligar a configurar variables del sistema operativo.
/// </summary>
public static class EnvironmentConfiguration
{
    public static void LoadDotEnv()
    {
        // Busca el .env en la raíz de la solución o en el directorio de ejecución
        var currentDir = Directory.GetCurrentDirectory();
        var rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
        var envPath = Path.Combine(rootDir, ".env");

        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }
        else if (File.Exists(".env"))
        {
            Env.Load(".env");
        }
    }
}
