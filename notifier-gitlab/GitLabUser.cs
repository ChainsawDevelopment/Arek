using GitLabNotifier.VCS;
using NGitLab.Models;

namespace notifier_gitlab
{
    public class GitLabUser : IUser
    {
        private readonly User _user;

        public GitLabUser(User user)
        {
            _user = user;
        }

        public string Username => _user.Username;
    }
}
