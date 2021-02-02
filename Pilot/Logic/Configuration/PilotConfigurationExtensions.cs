using Microsoft.Extensions.Configuration;
using Pilot.Logic.Configuration.Models;

namespace Pilot.Logic.Configuration
{
    public static class PilotConfigurationExtensions
    {
        public static PilotConfig GetPilotConfig(this IConfiguration config)
        {
            return config.GetSection("PilotConfig").Get<PilotConfig>();
        }

        public static string[] GetServerUrls(this IConfiguration config)
        {
            string value = config.GetValue<string>("server.urls");
            return value.Split(';');
        }
    }
}
