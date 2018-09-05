using System.Runtime.Serialization;
using NGitLab.Models;

namespace Arek.GitLab
{
    [DataContract]
    public class ExtendedMergeRequest : MergeRequest
    {
        [DataMember(Name = "web_url")]
        public string WebUrl { get; set; }

        [DataMember(Name = "sha")]
        public string Sha { get; set; }
    }
}