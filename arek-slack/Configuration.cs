using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Arek.Slack
{
    class Configuration
    {
        public static Lazy<Configuration> Instance = new Lazy<Configuration>(Read);

        public static Configuration Read()
        {
            var configReader = ReadConfig();

            var configuration = new Configuration();
            
            configuration.SlackIntegrationUrl = configReader["SlackIntegrationUrl"];
            configuration.SlackUserIds = configReader["UserIds"].Split(',')
                .Select(i => i.Split('='))
                .ToDictionary(i => i[0], i => i[1]);

            configuration.SlackLoginsMaps = configReader["SlackLoginsMap"].Split(',')
                .Select(map => map.Split('='))
                .ToDictionary(map => map[0], map => map[1]);

            return configuration;
        }

        private static IConfigurationRoot ReadConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configReader = builder.Build();
            return configReader;
        }

        public Dictionary<string, string> SlackUserIds { get; private set; }

        public string SlackIntegrationUrl { get; private set; }
        public Dictionary<string, string> SlackLoginsMaps { get; private set; }
    }
}