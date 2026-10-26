using Microsoft.Extensions.DependencyInjection;

namespace Nativefy.Engine.DI;

public static class NativefyEngineDI
{
    public static IServiceCollection AddNativefyEngine(this IServiceCollection services)
    {
        services.AddSingleton<SearchEngine>();
        services.AddSingleton<NativefyEngine>();

        return services;
    }
}
