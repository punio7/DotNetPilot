using Microsoft.Extensions.DependencyInjection;
using Pilot.Logic.Managers;

namespace Pilot.Logic.DependencyInjection
{
    public static class DependencyRegister
    {
        public static void RegisterDependencies(IServiceCollection services)
        {
            services.AddSingleton<KeyEventsManager>();
            services.AddScoped<QrCodeManager>();
        }
    }
}
