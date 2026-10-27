namespace Ncfe.CodeTest
{
    public sealed class FailoverLearnerDataAccessAdapter : IFailoverLearnerDataAccess
    {
        public LearnerResponse GetLearnerById(int learnerId)
        {
            return FailoverLearnerDataAccess.GetLearnerById(learnerId);
        }
    }
}
