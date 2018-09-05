using System.Collections.Generic;

namespace Arek.Contracts
{
    public interface IRuleFactory<out TRule>
    {
        TRule Create(IDictionary<string, string> options);
    }
}