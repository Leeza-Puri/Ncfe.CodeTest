using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Ncfe.CodeTest.Tests
{
    [TestFixture]
    public class LearnerServiceTests
    {
        private sealed class Dependencies : IArchivedDataService, ILearnerDataAccess,
            IFailoverLearnerDataAccess, IFailoverRepository, IFailoverSettings
        {
            public DateTime Now { get; set; } = new DateTime(2024, 3, 1, 12, 0, 0);
            public bool IsEnabled { get; set; } = true;
            public List<FailoverEntry> Entries = new List<FailoverEntry>();
            public Learner Archived = new Learner { Id = 42, Name = "Archived" };
            public LearnerResponse Response = new LearnerResponse { Learner = new Learner { Id = 42 } };
            public int ArchiveCalls, PrimaryCalls, FailoverCalls, RepositoryCalls;
            public int? RequestedId;
            public Exception PrimaryException;
            public Learner GetArchivedLearner(int id) { ArchiveCalls++; RequestedId = id; return Archived; }
            public LearnerResponse LoadLearner(int id)
            {
                PrimaryCalls++; RequestedId = id;
                if (PrimaryException != null) throw PrimaryException;
                return Response;
            }
            public LearnerResponse GetLearnerById(int id) { FailoverCalls++; RequestedId = id; return Response; }
            public List<FailoverEntry> GetFailOverEntries() { RepositoryCalls++; return Entries; }
            public LearnerService Create() { return new LearnerService(this, this, this, this, new FailoverPolicy(this, this)); }
            public void AddFailures(int count, DateTime time)
            {
                for (var i = 0; i < count; i++) Entries.Add(new FailoverEntry { DateTime = time });
            }
        }

        [Test]
        public void KnownArchivedLearnerBypassesAllOtherSources()
        {
            var d = new Dependencies();
            Assert.That(d.Create().GetLearner(42, true), Is.SameAs(d.Archived));
            Assert.That(d.RequestedId, Is.EqualTo(42));
            Assert.That(d.ArchiveCalls, Is.EqualTo(1));
            Assert.That(d.RepositoryCalls + d.PrimaryCalls + d.FailoverCalls, Is.Zero);
        }

        [TestCase(0, true, false)]
        [TestCase(100, true, false)]
        [TestCase(101, true, true)]
        [TestCase(101, false, false)]
        public void SelectsExpectedStore(int failures, bool enabled, bool useFailover)
        {
            var d = new Dependencies { IsEnabled = enabled };
            d.AddFailures(failures, d.Now.AddMinutes(-1));
            Assert.That(d.Create().GetLearner(42, false), Is.SameAs(d.Response.Learner));
            Assert.That(d.RequestedId, Is.EqualTo(42));
            Assert.That(d.FailoverCalls, Is.EqualTo(useFailover ? 1 : 0));
            Assert.That(d.PrimaryCalls, Is.EqualTo(useFailover ? 0 : 1));
            Assert.That(d.RepositoryCalls, Is.EqualTo(1));
            Assert.That(d.ArchiveCalls, Is.Zero);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void AppliesStrictTimeCutoff(int ticksAfterCutoff, bool expected)
        {
            var d = new Dependencies();
            d.AddFailures(100, d.Now.AddMinutes(-1));
            d.AddFailures(1, d.Now.AddMinutes(-10).AddTicks(ticksAfterCutoff));
            Assert.That(new FailoverPolicy(d).ShouldUseFailover(d.Entries), Is.EqualTo(expected));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ArchivedResponseFromEitherStoreUsesArchive(bool useFailover)
        {
            var d = new Dependencies();
            if (useFailover) d.AddFailures(101, d.Now);
            d.Response.IsArchived = true;
            Assert.That(d.Create().GetLearner(42, false), Is.SameAs(d.Archived));
            Assert.That(d.ArchiveCalls, Is.EqualTo(1));
            Assert.That(d.RequestedId, Is.EqualTo(42));
            Assert.That(d.FailoverCalls, Is.EqualTo(useFailover ? 1 : 0));
            Assert.That(d.PrimaryCalls, Is.EqualTo(useFailover ? 0 : 1));
        }

        [Test]
        public void PrimaryExceptionPropagatesWithoutAutomaticRetry()
        {
            var d = new Dependencies { PrimaryException = new InvalidOperationException("Unavailable") };
            Assert.That(() => d.Create().GetLearner(42, false), Throws.Exception.SameAs(d.PrimaryException));
            Assert.That(d.FailoverCalls + d.ArchiveCalls, Is.Zero);
        }
    }
}
