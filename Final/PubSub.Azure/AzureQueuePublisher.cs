using CommonTypes.Behaviours;
using CommonTypes.Settings;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Polly;
using PubSub.Behaviours;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubSub.Azure
{
    public class AzureQueuePublisher<T> : IQueuePublisher<T>
    {
        private IAppLogger _appLogger;
        private AzureSettings _azureSettings;
        private Policy _retryPolicy;

        private QueueClient _queueClient;

        public AzureQueuePublisher(IAppLogger appLogger, AzureSettings azureSettings)
        {
            _appLogger = appLogger;
            _azureSettings = azureSettings;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"AzureQueuePublisher Retry - Count:{retryCount}, Exception:{exception.Message}";
                        _appLogger?.LogWarning(msg);
                    }
                );
        }

        public void InitializeQueue()
        {
            if (_queueClient == null)
            {
                var queueName = _azureSettings.PubSub.QueueMappings[typeof(T).FullName];
                _queueClient = new QueueClient(_azureSettings.PubSub.ConnectionString, queueName);
            }
        }

        private async Task sendMessage(T messageObject)
        {
            InitializeQueue();

            var json = JsonConvert.SerializeObject(messageObject);
            var message = new Message(Encoding.UTF8.GetBytes(json))
            {
                MessageId = Guid.NewGuid().ToString()
            };

            var attributes = new Dictionary<string, string>
            {
                { "Message.Type.FullName", messageObject.GetType().FullName }
            };

            message.UserProperties.Add("Message.Type.FullName", messageObject.GetType().FullName);

            await _queueClient.SendAsync(message);

            _appLogger.LogMessage($"Message Type: {messageObject.GetType().FullName} queued with MessageId: {message.MessageId}");
        }

        public async Task SendMessage(T message)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await sendMessage(message);
                }
                catch (Exception ex)
                {
                    _appLogger?.LogError(ex);
                    throw;
                }
            });
        }
    }
}
