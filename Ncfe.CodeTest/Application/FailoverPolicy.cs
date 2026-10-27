using System;
using System.Collections.Generic;
using System.Linq;

namespace Ncfe.CodeTest
{
    public sealed class FailoverPolicy
    {
        private const int FailureThreshold = 100;
        private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(10);
   
        private readonly IFailoverSettings settings;

        public FailoverPolicy(IFailoverSettings settings)
        {
         
            if (settings == null) throw new ArgumentNullException(nameof(settings));
           this.settings = settings;
        }

        public bool ShouldUseFailover(IEnumerable<FailoverEntry> entries)
        {
            var cutoff = DateTime.Now.Subtract(FailureWindow);
            return entries.Count(entry => entry.DateTime > cutoff) > FailureThreshold
                && settings.IsEnabled;
        }
    }
}
