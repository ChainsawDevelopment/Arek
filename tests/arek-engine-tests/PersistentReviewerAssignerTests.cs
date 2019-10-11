using System;
using System.Collections.Generic;
using System.Linq;
using Arek.Contracts;
using Moq;
using NUnit.Framework;

namespace Arek.Engine.Tests
{
    [TestFixture]
    public class PersistentReviewerAssignerTests
    {
        private class FakeMergeRequest : IMergeRequest
        {
            public string PrefferedAssignment { get; set; }
            public int Id { get; set; }
            public string Iid { get; set; }
            public string Title { get; set; }
            public string Project { get; set; }
            public IUser Author { get; set; }
            public IEnumerable<string> Reviewers { get; set; }
            public DateTime CreatedAt { get; set; }
            public int Downvotes { get; set; }
            public int Upvotes { get; set; }
            public string Url { get; set; }
            public string TicketID { get; set; }
            public string GetPrefferedAssignment(List<string> except)
            {
                return new[]{ PrefferedAssignment }.Except(except).FirstOrDefault();
            }

            public TicketDetails TicketDetails { get; set; }
            public Dictionary<string, string[]> CommentAuthors { get; set; }
            public string HeadHash { get; set; }
            public bool IsOpened { get; set; }
            public ProjectDetails ProjectDetails { get; set; }
            public List<IMessageRule> Rules { get; set; }
            public string SourceBranchName { get; set; }
        }

        [Test]
        public void TestSimpleAssignment()
        {
            List<Tuple<int, string>> LastAssignments() => new List<Tuple<int, string>>();
            var confDevs = new[] {"dev1", "dev2", "dev3", "dev4", "dev5" };
            var persistentReviewerAssigner = new PersistentReviewerAssigner(confDevs, 2, LastAssignments);

            var user1 = new Mock<IUser>();
            var user2 = new Mock<IUser>();

            user1.SetupGet(x => x.Username).Returns("dev1");
            user2.SetupGet(x => x.Username).Returns("dev3");

            var mergeRequest1 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 1,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user1.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev5" }, SecondaryReviewers = new[] { "dev2", "dev3", "dev4" } }
            };

            var mergeRequest2 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 2,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user2.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev2", "dev4", "dev5" }, SecondaryReviewers = new[] { "dev3" } }
            };

            var mergeRequests = new IMergeRequest[] {mergeRequest1, mergeRequest2};
            persistentReviewerAssigner.AssignReviewers(mergeRequests);

            Assert.That(mergeRequest1.Reviewers, Is.EquivalentTo(new[] {"dev2", "dev3"}));
            Assert.That(mergeRequest2.Reviewers, Is.EquivalentTo(new[] {"dev1", "dev4"}));
        }

        [Test]
        public void TestAlreadyAssigned()
        {
            List<Tuple<int, string>> LastAssignments() => new List<Tuple<int, string>>
            {
                new Tuple<int, string>(1, "dev4"), new Tuple<int, string>(1, "dev5"),
                new Tuple<int, string>(2, "dev5")
            };
            var confDevs = new[] { "dev1", "dev2", "dev3", "dev4", "dev5" };
            var persistentReviewerAssigner = new PersistentReviewerAssigner(confDevs, 2, LastAssignments);

            var user1 = new Mock<IUser>();
            var user2 = new Mock<IUser>();

            user1.SetupGet(x => x.Username).Returns("dev1");
            user2.SetupGet(x => x.Username).Returns("dev3");

            var mergeRequest1 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 1,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user1.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev5" }, SecondaryReviewers = new[] { "dev2", "dev3", "dev4" } }
            };

            var mergeRequest2 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 2,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user2.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev2", "dev4", "dev5" }, SecondaryReviewers = new[] { "dev3" } }
            };

            var mergeRequests = new IMergeRequest[] { mergeRequest1, mergeRequest2 };
            persistentReviewerAssigner.AssignReviewers(mergeRequests);

            Assert.That(mergeRequest1.Reviewers, Is.EquivalentTo(new[] { "dev4", "dev5" }));
            Assert.That(mergeRequest2.Reviewers, Is.EquivalentTo(new[] { "dev1", "dev5" }));
        }

        [Test]
        public void TestPreferredAssignment()
        {
            List<Tuple<int, string>> LastAssignments() => new List<Tuple<int, string>>();
            var confDevs = new[] { "dev1", "dev2", "dev3", "dev4", "dev5" };
            var persistentReviewerAssigner = new PersistentReviewerAssigner(confDevs, 2, LastAssignments);

            var user1 = new Mock<IUser>();
            var user2 = new Mock<IUser>();

            user1.SetupGet(x => x.Username).Returns("dev1");
            user2.SetupGet(x => x.Username).Returns("dev3");

            var mergeRequest1 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 1,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user1.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev5" }, SecondaryReviewers = new[] { "dev2", "dev3", "dev4" } },
                PrefferedAssignment = "dev1"
            };

            var mergeRequest2 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 2,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user2.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev2", "dev4", "dev5" }, SecondaryReviewers = new[] { "dev3" } },
                PrefferedAssignment = "dev4"
            };

            var mergeRequests = new IMergeRequest[] { mergeRequest1, mergeRequest2 };
            persistentReviewerAssigner.AssignReviewers(mergeRequests);

            Assert.That(mergeRequest1.Reviewers, Is.EquivalentTo(new[] { "dev2", "dev3" }));
            Assert.That(mergeRequest2.Reviewers, Is.EquivalentTo(new[] { "dev1", "dev4" }));
        }

        [Test]
        public void TestUserNotInConfiguration()
        {
            List<Tuple<int, string>> LastAssignments() => new List<Tuple<int, string>>();
            var confDevs = new[] { "dev1", "dev3", "dev4", "dev5" };
            var persistentReviewerAssigner = new PersistentReviewerAssigner(confDevs, 2, LastAssignments);

            var user1 = new Mock<IUser>();
            var user2 = new Mock<IUser>();

            user1.SetupGet(x => x.Username).Returns("dev1");
            user2.SetupGet(x => x.Username).Returns("dev3");

            var mergeRequest1 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 1,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user1.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev5" }, SecondaryReviewers = new[] { "dev2", "dev3", "dev4" } }
            };

            var mergeRequest2 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 2,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user2.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev2", "dev4", "dev5" }, SecondaryReviewers = new[] { "dev3" } }
            };

            var mergeRequests = new IMergeRequest[] { mergeRequest1, mergeRequest2 };
            persistentReviewerAssigner.AssignReviewers(mergeRequests);

            Assert.That(mergeRequest1.Reviewers, Is.EquivalentTo(new[] { "dev2", "dev3" }));
            Assert.That(mergeRequest2.Reviewers, Is.EquivalentTo(new[] { "dev1", "dev4" }));
        }

        [Test]
        public void TestUserWithAssignmentsNotInConfiguration()
        {
            List<Tuple<int, string>> LastAssignments() => new List<Tuple<int, string>>
            {
                new Tuple<int, string>(3, "dev2")
            };
            var confDevs = new[] { "dev1", "dev3", "dev4", "dev5" };
            var persistentReviewerAssigner = new PersistentReviewerAssigner(confDevs, 2, LastAssignments);

            var user1 = new Mock<IUser>();
            var user2 = new Mock<IUser>();

            user1.SetupGet(x => x.Username).Returns("dev1");
            user2.SetupGet(x => x.Username).Returns("dev3");

            var mergeRequest1 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 1,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user1.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev5" }, SecondaryReviewers = new[] { "dev2", "dev3", "dev4" } }
            };

            var mergeRequest2 = new FakeMergeRequest
            {
                Project = "Project",
                Id = 2,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user2.Object,
                ProjectDetails = new ProjectDetails
                    { PrimaryReviewers = new[] { "dev1", "dev2", "dev4", "dev5" }, SecondaryReviewers = new[] { "dev3" } }
            };

            var mergeRequests = new IMergeRequest[] { mergeRequest1, mergeRequest2 };
            persistentReviewerAssigner.AssignReviewers(mergeRequests);

            Assert.That(mergeRequest1.Reviewers, Is.EquivalentTo(new[] { "dev3", "dev4" }));
            Assert.That(mergeRequest2.Reviewers, Is.EquivalentTo(new[] { "dev1", "dev5" }));
        }

        [Test]
        public void TestAssignedUserNotInConfiguration()
        {
            List<Tuple<int, string>> LastAssignments() => new List<Tuple<int, string>>
            {
                new Tuple<int, string>(1, "dev2")
            };
            var confDevs = new[] { "dev1", "dev3", "dev4", "dev5" };
            var persistentReviewerAssigner = new PersistentReviewerAssigner(confDevs, 2, LastAssignments);

            var user = new Mock<IUser>();

            user.SetupGet(x => x.Username).Returns("dev1");

            var mergeRequest = new FakeMergeRequest
            {
                Project = "Project",
                Id = 1,
                CommentAuthors = new Dictionary<string, string[]>
                {
                    {"devs", new string[] { }}
                },
                Author = user.Object,
                ProjectDetails = new ProjectDetails
                { PrimaryReviewers = new[] { "dev1", "dev5" }, SecondaryReviewers = new[] { "dev2", "dev3", "dev4" } }
            };

            var mergeRequests = new IMergeRequest[] { mergeRequest };
            persistentReviewerAssigner.AssignReviewers(mergeRequests);

            Assert.That(mergeRequest.Reviewers, Is.EquivalentTo(new[] { "dev2", "dev3" }));
        }
    }
}
