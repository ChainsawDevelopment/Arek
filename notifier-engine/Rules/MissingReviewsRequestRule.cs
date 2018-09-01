using System.Collections.Generic;
using System.Linq;
using GitLabNotifier.VCS;

namespace GitLabNotifier
{
    static class StringExtensions
    {
        public static int ToInt(this string value) => int.Parse(value);
    }

    public interface IRuleFactory<out TRule>
    {
        TRule Create(IDictionary<string, string> options);
    }

    public static class RuleFactoryExtensions
    {
        public static TRule Create<TRule>(this IRuleFactory<TRule> factory, IDictionary<string, IDictionary<string, string>> rulesConfig)
        {
            return factory.Create(rulesConfig[typeof(TRule).Name]);
        }
    }

    public class MissingReviewsRequestRuleFactory : IRuleFactory<MissingReviewsRequestRule>
    {
        public MissingReviewsRequestRule Create(IDictionary<string, string> options)
        {
            return new MissingReviewsRequestRule(options["RequiredVotesCount"].ToInt());
        }
    }

    public class MissingReviewsRequestRule : IMessageRule
    {
        private readonly int _requiredVotesCount;

        public MissingReviewsRequestRule(int requiredVotesCount)
        {
            _requiredVotesCount = requiredVotesCount;
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
            if (request.Upvotes < _requiredVotesCount && _requiredVotesCount - request.CommentAuthors["devs"].Length - request.CommentAuthors["qas"].Length > 0)
            {
                return new RequestMessage(request, 
                    $"{_requiredVotesCount - request.CommentAuthors["devs"].Length} more review needed",
                    request.Reviewers);
            }

            return null;
        }

        public IEnumerable<IMessage> GetMessages(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests)
        {
            return ticketRequests.Select(GetMessage);
        }
    }
}