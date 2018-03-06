using CommonTypes.Behaviours;
using CommonTypes.Settings;
using PubSub.Behaviours;
using System;
using System.Threading.Tasks;

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

        public Task SendMessage(T message)
        {
            throw new NotImplementedException();
        }
    }
}
