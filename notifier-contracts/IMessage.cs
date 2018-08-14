using System.Collections.Generic;

namespace GitLabNotifier
{
    public interface IMessage
    {
        string LinkUrl { get;  }
        string LinkText { get;  }
        string Message { get; }
        IEnumerable<string> Receipents { get; }
    }
}