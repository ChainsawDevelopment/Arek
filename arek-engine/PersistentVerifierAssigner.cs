using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arek.Contracts;
using Newtonsoft.Json;

namespace Arek.Engine
{
    public class PersistentVerifierAssigner : IReviewerAssignStrategy
    {
        private Random _r = new Random(DateTime.Now.Millisecond);

        public void AssignReviewers(IMergeRequest[] mergeRequests)
        {
            var lastAssignmentsStore = LoadLastAssignments()
                .GroupBy(a => a.Item1)
                .ToDictionary(g => g.Key, g => g.LastOrDefault()?.Item2);

             var mostRecentAssignments = lastAssignmentsStore
                .GroupBy(a => a.Value, a => a.Key)
                .ToDictionary(a => a.Key, a => a.Max());
             
            foreach (var request in mergeRequests)
            {
                string verifier = GetReviewVerfier(request, lastAssignmentsStore, mostRecentAssignments);

                if (string.IsNullOrEmpty(verifier))
                {
                    continue;
                }

                
                request.CommentAuthors["verifier"] = new string[] {verifier};
                lastAssignmentsStore[request.Id] = verifier;
            }

            SaveLastAssignments(lastAssignmentsStore.Select(kvp => new Tuple<int, string>(kvp.Key, kvp.Value)).ToList());
        }

        private string GetReviewVerfier(IMergeRequest request, Dictionary<int, string> lastAssignmentsStore, Dictionary<string, int> mostRecentAssignments)
        {
            
            if (lastAssignmentsStore.ContainsKey(request.Id) && !string.IsNullOrEmpty(lastAssignmentsStore[request.Id]))
            {
                return lastAssignmentsStore[request.Id];
            } 
            else  if (request.Reviewers.Any())
            {                
                FillMissingMostRecentAssignments(request.Reviewers, mostRecentAssignments);
                return request.Reviewers.OrderBy(reviewer => mostRecentAssignments[reviewer]).ThenBy(_ => _r.Next()).FirstOrDefault();
            }
            else 
            {
                FillMissingMostRecentAssignments(request.CommentAuthors["devs"], mostRecentAssignments);
                return request.CommentAuthors["devs"]
                    .Except(new [] {request.Author.Username.ToLower()})
                    .OrderBy(user => mostRecentAssignments[user])
                    .ThenBy(_ => _r.Next())
                    .FirstOrDefault();
            }
        }

        private void FillMissingMostRecentAssignments(IEnumerable<string> users, Dictionary<string, int> mostRecentAssignments)
        {
            foreach(var user in users)
            {
                if (mostRecentAssignments.ContainsKey(user))
                {
                    continue;
                }

                mostRecentAssignments[user] = 0;
            }
        }

        private static List<Tuple<int, string>> LoadLastAssignments()
        {
            var lastAssignmentsFilePath = GetLastAssignmentsFilePath();

            if (!File.Exists(lastAssignmentsFilePath))
                return new List<Tuple<int, string>>();

            return JsonConvert.DeserializeObject<List<Tuple<int, string>>>(File.ReadAllText(lastAssignmentsFilePath));
        }

        
        private void SaveLastAssignments(List<Tuple<int, string>> lastAssignments)
        {
            var lastAssignmentsFilePath = GetLastAssignmentsFilePath();

            File.WriteAllText(lastAssignmentsFilePath, JsonConvert.SerializeObject(lastAssignments));
        }

        private static string GetLastAssignmentsFilePath()
        {
            return Path.Combine(
                Path.GetDirectoryName(typeof(PersistentVerifierAssigner).Assembly.Location),
                "lastVerifiers.json");
        }
    }
}