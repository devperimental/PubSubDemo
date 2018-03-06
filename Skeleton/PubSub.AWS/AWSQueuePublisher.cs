using CommonTypes.Behaviours;
using CommonTypes.Settings;
using PubSub.Behaviours;
using System;
using System.Threading.Tasks;

namespace PubSub.AWS
{
    public class AWSQueuePublisher<T> : IQueuePublisher<T>
    {
        private IAppLogger _appLogger;
        private AWSSettings _awsSettings;

        public AWSQueuePublisher(IAppLogger appLogger, AWSSettings awsSettings)
        {
            _appLogger = appLogger;
            _awsSettings = awsSettings;
        }

        public Task SendMessage(T message)
        {
            throw new NotImplementedException();
        }
    }
}
