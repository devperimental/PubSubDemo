using CommonTypes.Behaviours;
using CommonTypes.Settings;
using Microsoft.Azure.ServiceBus;
using Polly;
using PubSub.Behaviours;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubSub.Azure
{
    public class AzureQueueSubscriber<T> : IQueueSubscriber<T>
    {
        private IAppLogger _appLogger;
        private AzureSettings _azureSettings;
        private Policy _retryPolicy;

        private Action<string> _messageHandler;
        private QueueClient _queueClient;
        private bool _initialised = false;

        public AzureQueueSubscriber(IAppLogger appLogger, AzureSettings azureSettings, Action<string> messageHandler)
        {
            _appLogger = appLogger;
            _azureSettings = azureSettings;

            _messageHandler = messageHandler;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"AzureQueueSubscriber Retry - Count:{retryCount}, Exception:{exception.Message}";
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


        public async Task Process()
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    if (!_initialised)
                    {
                        await process();
                        _initialised = true;
                    }
                    
                }
                catch (Exception ex)
                {
                    _appLogger?.LogError(ex);
                    _initialised = false;
                    throw;
                }
            });
        }

        private async Task process()
        {
            InitializeQueue();

            var messageHandlerOptions = new MessageHandlerOptions(exceptionHandler)
            {
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(HandleMessagesAsync, messageHandlerOptions);

        }

        private Task exceptionHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _appLogger.LogError(exceptionReceivedEventArgs.Exception);

            return Task.CompletedTask;
        }

        private async Task HandleMessagesAsync(Message message, CancellationToken token)
        {
            _appLogger.LogMessage($"Message Type: {typeof(T).FullName} dequeued with MessageId: {message.MessageId}");

            _messageHandler(Encoding.UTF8.GetString(message.Body));
         
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }
    }
}