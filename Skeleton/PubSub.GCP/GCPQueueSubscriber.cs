using CommonTypes.Behaviours;
using CommonTypes.Settings;
using PubSub.Behaviours;
using System;
using System.Threading;

namespace PubSub.GCP
{
    public class GCPQueueSubscriber<T> : IQueueSubscriber<T>
    {
        private IAppLogger _appLogger;
        private GCPSettings _gcpSettings;

        public GCPQueueSubscriber(IAppLogger appLogger, GCPSettings gcpSettings)
        {
            _appLogger = appLogger;
            _gcpSettings = gcpSettings;
        }

        public void Process(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
