using CommonTypes.Behaviours;
using CommonTypes.Settings;
using PubSub.Behaviours;
using System;

namespace PubSub.GCP
{
    public class GCPQueuePublisher<T> : IQueuePublisher<T>
    {
        private IAppLogger _appLogger;
        private GCPSettings _gcpSettings;

        public GCPQueuePublisher(IAppLogger appLogger, GCPSettings gcpSettings)
        {
            _appLogger = appLogger;
            _gcpSettings = gcpSettings;
        }

        public void SendMessage(T message)
        {
            throw new NotImplementedException();
        }
    }
}
