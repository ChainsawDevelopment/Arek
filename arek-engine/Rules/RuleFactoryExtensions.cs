using System.Collections.Generic;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    public static class RuleFactoryExtensions
    {
        public static TRule Create<TRule>(this IRuleFactory<TRule> factory, IDictionary<string, IDictionary<string, string>> rulesConfig)
        {
            return factory.Create(rulesConfig[typeof(TRule).Name]);
        }
    }
}