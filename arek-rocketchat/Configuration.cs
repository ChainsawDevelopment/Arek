using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Arek.RocketChat
{
    class Configuration
    {
        public static Lazy<Configuration> Instance = new Lazy<Configuration>(Read);

        public static Configuration Read()
        {
            var configReader = ReadConfig();

            var configuration = new Configuration();
            
            configuration.RocketChatWebhookUrl = configReader["RocketChatWebhookUrl"];
            configuration.RocketLoginsMap = configReader["RocketLoginsMap"].Split(',')
                .Select(map => map.Split('='))
                .Where(map => map.Length == 2)
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

        public string RocketChatWebhookUrl { get; private set; }
        public Dictionary<string, string> RocketLoginsMap { get; private set; }
    }
}