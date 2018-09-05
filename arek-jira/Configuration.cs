using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Arek.Jira
{
    class Configuration
    {
        public static Lazy<Configuration> Instance = new Lazy<Configuration>(Read);
       
        public static Configuration Read()
        {
            var configReader = ReadConfig();
            
            return new Configuration
            {
                JiraBaseUrl = configReader["JiraUrl"],
                JiraToken = configReader["JiraToken"]
            };
        }

        private static IConfigurationRoot ReadConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configReader = builder.Build();
            return configReader;
        }

        public string JiraToken { get; private set; }

        public string JiraBaseUrl { get; private set; }
    }
}