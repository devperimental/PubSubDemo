using CommonTypes.Behaviours;
using CommonTypes.Settings;
using PubSub.Behaviours;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubSub.Azure
{
    public class AzureQueueSubscriber<T> : IQueueSubscriber<T>
    {
        private IAppLogger _appLogger;
        private AzureSettings _azureSettings;

        public AzureQueueSubscriber(IAppLogger appLogger, AzureSettings azureSettings)
        {
            _appLogger = appLogger;
            _azureSettings = azureSettings;
        }

        public Task Process()
        {
            throw new NotImplementedException();
        }
    }
}
