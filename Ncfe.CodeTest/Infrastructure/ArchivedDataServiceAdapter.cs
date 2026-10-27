namespace Ncfe.CodeTest
{
    public sealed class ArchivedDataServiceAdapter : IArchivedDataService
    {
        public Learner GetArchivedLearner(int learnerId)
        {
            return new ArchivedDataService().GetArchivedLearner(learnerId);
        }
    }
}
