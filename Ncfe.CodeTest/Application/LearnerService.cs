using System;

namespace Ncfe.CodeTest
{
    public class LearnerService
    {
        private readonly IArchivedDataService archive;
        private readonly ILearnerDataAccess primary;
        private readonly IFailoverLearnerDataAccess failover;
        private readonly IFailoverRepository repository;
        private readonly FailoverPolicy policy;

        public LearnerService(IArchivedDataService archive, ILearnerDataAccess primary,
            IFailoverLearnerDataAccess failover, IFailoverRepository repository,
            FailoverPolicy policy)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (primary == null) throw new ArgumentNullException(nameof(primary));
            if (failover == null) throw new ArgumentNullException(nameof(failover));
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            this.archive = archive;
            this.primary = primary;
            this.failover = failover;
            this.repository = repository;
            this.policy = policy;
        }

        public Learner GetLearner(int learnerId, bool isLearnerArchived)
        {
            if (isLearnerArchived)
                return archive.GetArchivedLearner(learnerId);

            var response = policy.ShouldUseFailover(repository.GetFailOverEntries())
                ? failover.GetLearnerById(learnerId)
                : primary.LoadLearner(learnerId);

            return response.IsArchived
                ? archive.GetArchivedLearner(learnerId)
                : response.Learner;
        }
    }
}
