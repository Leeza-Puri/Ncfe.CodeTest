namespace Ncfe.CodeTest
{
    public sealed class AppSettingsFailoverSettings : IFailoverSettings
    {
        public bool IsEnabled
        {
            get
            {
                var value = System.Configuration.ConfigurationManager.AppSettings["IsFailoverModeEnabled"];
                return value == "true" || value == "True";
            }
        }
    }
}
