using System.Collections.Generic;

namespace GitLabNotifier
{
    public interface IRuleFactory<out TRule>
    {
        TRule Create(IDictionary<string, string> options);
    }
}