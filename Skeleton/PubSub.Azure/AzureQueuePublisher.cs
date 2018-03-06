using CommonTypes.Behaviours;
using CommonTypes.Settings;
using PubSub.Behaviours;
using System;

namespace PubSub.Azure
{
    public class AzureQueuePublisher<T> : IQueuePublisher<T>
    {
        private IAppLogger _appLogger;
        private AzureSettings _azureSettings;

        public AzureQueuePublisher(IAppLogger appLogger, AzureSettings azureSettings)
        {
            _appLogger = appLogger;
            _azureSettings = azureSettings;
        }

        public void SendMessage(T message)
        {
            throw new NotImplementedException();
        }
    }
}
