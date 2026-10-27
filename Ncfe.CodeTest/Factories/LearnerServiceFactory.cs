namespace Ncfe.CodeTest
{
    public static class LearnerServiceFactory
    {
        public static LearnerService Create()
        {
            return new LearnerService(new ArchivedDataServiceAdapter(),
                new LearnerDataAccessAdapter(), new FailoverLearnerDataAccessAdapter(),
                new FailoverRepository(),
                new FailoverPolicy(new AppSettingsFailoverSettings()));
        }
    }
}
