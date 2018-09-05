using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arek.Contracts;
using Microsoft.Extensions.Configuration;

namespace Arek.Engine
{
    public class Configuration
    {
        public int RequiredVotesCount { get; private set; }

        public static Lazy<Configuration> Instance = new Lazy<Configuration>(Read);

        public void ReloadDevs()
        {
            var configReader = ReadConfig();
            var teamsConfig = configReader.GetSection("Teams");

            ProjectTeams = teamsConfig.GetChildren().Select(team => team.LoadTeam()).ToArray();
            GitProjects = ProjectTeams.Select(project => project.Name).ToArray();

            Random randomNumberGenerator = new Random((int)DateTime.Now.Ticks);
            Devs = ProjectTeams
                .SelectMany(project => project.PrimaryReviewers.Concat(project.SecondaryReviewers))
                .Distinct()
                .Shuffle(randomNumberGenerator)
                .ToArray();
        }

        public static Configuration Read()
        {
            var configReader = ReadConfig();

            var configuration = new Configuration();
            
            configuration.Qas = configReader["Qas"].Split(',');
            configuration.GitlabApiToken = configReader["GitLabApiToken"];

            configuration.BitbucketTeamUuid = configReader["BitbucketTeamUuid"];
            configuration.RequiredVotesCount = int.Parse(configReader["RequiredVotesCount"]);

            configuration.GitLabUrl = configReader["GitLabUrl"];
            configuration.JiraBaseUrl = configReader["JiraUrl"];
            configuration.JiraToken = configReader["JiraToken"];

            configuration.ReloadDevs();

            configuration.Rules = configReader.GetSection("rules").GetChildren().ToDictionary(c => c.Key, c => c.LoadRule());

            return configuration;
        }

        public Dictionary<string, IDictionary<string, string>> Rules { get; set; }

        public string BitbucketTeamUuid { get; private set; }

        private static IConfigurationRoot ReadConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configReader = builder.Build();
            return configReader;
        }

        public int[] OldRequestThresholdsDays { get; set; }

        public string JiraToken { get; private set; }

        public string JiraBaseUrl { get; private set; }

        public string GitLabUrl { get; private set; }

        public string GitlabApiToken { get; private set; }

        public string[] Qas { get; private set; }

        public string[] Devs { get; private set; }

        public string[] GitProjects { get; private set; }

        public ProjectDetails[] ProjectTeams { get; private set; }
    }

    public static class ConfigurationExtensions
    {
        public static ProjectDetails LoadTeam(this IConfigurationSection configSection)
        {
            return new ProjectDetails
            {
                Name = configSection.Key,
                PrimaryReviewers = configSection["primary"]?.Split(',') ?? new string[] { },
                SecondaryReviewers = configSection["secondary"]?.Split(new char [] {','}, StringSplitOptions.RemoveEmptyEntries ) ?? new string[] { },
                ShortName = configSection["shortName"] ?? configSection.Key
            };
        }

        public static IDictionary<string, string> LoadRule(this IConfigurationSection configSection)
        {
            return configSection.GetChildren().ToDictionary(c => c.Key, c => c.Value);

        }
    }
}