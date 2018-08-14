﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitLabNotifier.VCS;
using Newtonsoft.Json;

namespace GitLabNotifier
{
    public class PersistentReviewerAssigner : IReviewerAssignStrategy
    {
        private readonly string[] _devs;
        private readonly int _requiredVotesCount;

        public static PersistentReviewerAssigner ConfiguredWith(Configuration configuration)
        {
            return new PersistentReviewerAssigner(configuration.Devs, configuration.RequiredVotesCount);
        }

        public PersistentReviewerAssigner(string[] configurationDevs, int requiredVotesCount)
        {
            _devs = configurationDevs;
            _requiredVotesCount = requiredVotesCount;
        }

        public void AssignReviewers(IMergeRequest[] mergeRequests)
        {
            var lastAssignmentsStore = LoadLastAssignments();

            var devAssignments = _devs.Distinct().ToDictionary(d => d, d => 0);
            var devLastAssignmentsMap = _devs.ToDictionary(
                dev => dev,
                dev => lastAssignmentsStore.Any(a => a.Item2 == dev)
                    ? lastAssignmentsStore.Where(a => a.Item2 == dev).Select(a => a.Item1).Max()
                    : 0);

            foreach (var request in mergeRequests)
            {
                AssignReviewers(request, request.CommentAuthors["devs"], devLastAssignmentsMap,
                    lastAssignmentsStore, devAssignments, _requiredVotesCount);
            }

            SaveLastAssignments(lastAssignmentsStore);
        }

        private void AssignReviewers(IMergeRequest mr, IReadOnlyCollection<string> commentAuthors, Dictionary<string, int> lastRequests, List<Tuple<int, string>> lastAssignments, Dictionary<string, int> assignments, int requiredVotes)
        {
            var usersToTake = Math.Max(requiredVotes - commentAuthors.Count, 0);
            var except = commentAuthors.Concat(new[] { mr.Author.Username.ToLower() }).ToList();

            var applicableUsers = new List<string>();

            while (usersToTake-- > 0)
            {
                var reviewer = GetExistingAssignment(mr, lastAssignments, applicableUsers, except);

                // If last assignment is no longer on dev list - remove it and reassign
                if (reviewer != null && !assignments.ContainsKey(reviewer))
                {
                    lastAssignments.Remove(new Tuple<int, string>(mr.Id, reviewer));
                    reviewer = null;
                }

                if (reviewer == null)
                {
                    reviewer = mr.GetPrefferedAssignment(except.Concat(applicableUsers).ToList());
                }

                if (reviewer == null)
                {
                    reviewer = GetNewAssignment(usersToTake == requiredVotes, mr, mr.Author.Username.ToLower(), lastRequests, assignments, except.Concat(applicableUsers).ToList());

                    lastAssignments.Add(new Tuple<int, string>(mr.Id, reviewer));
                }

                assignments.AddOrUpdate(reviewer, () => 1, x => x + 1);

                applicableUsers.Add(reviewer);
            }

            mr.Reviewers = applicableUsers.ToArray();
        }

        private void SaveLastAssignments(List<Tuple<int, string>> lastAssignments)
        {
            var lastAssignmentsFilePath = GetLastAssignmentsFilePath();

            File.WriteAllText(lastAssignmentsFilePath, JsonConvert.SerializeObject(lastAssignments));
        }

        private List<Tuple<int, string>> LoadLastAssignments()
        {
            var lastAssignmentsFilePath = GetLastAssignmentsFilePath();

            if (!File.Exists(lastAssignmentsFilePath))
                return new List<Tuple<int, string>>();

            return JsonConvert.DeserializeObject<List<Tuple<int, string>>>(File.ReadAllText(lastAssignmentsFilePath));
        }

        private string GetLastAssignmentsFilePath()
        {
            return Path.Combine(
                Path.GetDirectoryName(GetType().Assembly.Location),
                "lastAssignments.json");
        }

        private string GetNewAssignment(bool primaryAssignment, IMergeRequest mr, string author,
            Dictionary<string, int> devLastRequests,
            Dictionary<string, int> devAssignments, List<string> except)
        {
            var requestTeam = mr.ProjectDetails;

            var reviewers = (primaryAssignment
                    ? requestTeam.PrimaryReviewers
                    : requestTeam.PrimaryReviewers.Concat(requestTeam.SecondaryReviewers))
                .Except(new List<string> {author})
                .Except(except);

            if (!reviewers.Any())
            {
                if (primaryAssignment)
                {
                    reviewers = requestTeam.PrimaryReviewers.Concat(requestTeam.SecondaryReviewers);
                }
                else
                {
                    reviewers = _devs;
                }

                reviewers = reviewers
                    .Except(new List<string> {author})
                    .Except(except);
            }

            var reviewersHash = new HashSet<string>(reviewers);

            return devAssignments.Where(d => reviewersHash.Contains(d.Key))
                .OrderBy(d => d.Value)
                .ThenBy(d => devLastRequests.ContainsKey(d.Key) ? devLastRequests[d.Key] : 0)
                .ThenBy(d => d.Key)
                .Select(d => d.Key)
                .FirstOrDefault();
        }

        private static string GetExistingAssignment(IMergeRequest mr, List<Tuple<int, string>> lastAssignments, List<string> applicableDevs, List<string> except)
        {
            return lastAssignments
                .Where(a => a.Item1 == mr.Id)
                .Where(a => !applicableDevs.Contains(a.Item2))
                .Where(a => !except.Contains(a.Item2))
                .Select(a => a.Item2)
                .FirstOrDefault();
        }
    }
}