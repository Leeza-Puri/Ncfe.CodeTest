namespace Ncfe.CodeTest
{
    public sealed class LearnerDataAccessAdapter : ILearnerDataAccess
    {
        public LearnerResponse LoadLearner(int learnerId)
        {
            return new LearnerDataAccess().LoadLearner(learnerId);
        }
    }
}
