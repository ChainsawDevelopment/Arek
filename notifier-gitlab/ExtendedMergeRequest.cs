using System.Runtime.Serialization;
using NGitLab.Models;

namespace notifier_gitlab
{
    [DataContract]
    public class ExtendedMergeRequest : MergeRequest
    {
        [DataMember(Name = "web_url")]
        public string WebUrl { get; set; }
    }
}