using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registra automáticamente todos los validadores de FluentValidation del ensamblado Application
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
