using Microsoft.Extensions.DependencyInjection;
using Application.Services;

namespace Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<IGameSessionService, GameSessionService>();

            return services;
        }
    }
}