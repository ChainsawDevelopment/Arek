using Arek.Contracts;
using NGitLab.Models;

namespace Arek.GitLab
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
