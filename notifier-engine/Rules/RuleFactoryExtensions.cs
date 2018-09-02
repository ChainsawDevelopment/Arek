using System.Collections.Generic;

namespace GitLabNotifier
{
    public static class RuleFactoryExtensions
    {
        public static TRule Create<TRule>(this IRuleFactory<TRule> factory, IDictionary<string, IDictionary<string, string>> rulesConfig)
        {
            return factory.Create(rulesConfig[typeof(TRule).Name]);
        }
    }
}