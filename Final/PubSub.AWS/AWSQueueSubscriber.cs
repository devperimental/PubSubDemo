using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using CommonTypes.Behaviours;
using CommonTypes.Settings;
using Polly;
using PubSub.Behaviours;
using System;
using System.Threading.Tasks;

namespace PubSub.AWS
{
    public class AWSQueueSubscriber<T> : IQueueSubscriber<T>
    {
        private IAppLogger _appLogger;
        private AWSSettings _awsSettings;

        private Policy _retryPolicy;

        private AmazonSQSClient _client;
        private Action<string> _messageHandler;

        public AWSQueueSubscriber(IAppLogger appLogger, AWSSettings awsSettings, Action<string> messageHandler)
        {
            _appLogger = appLogger;
            _awsSettings = awsSettings;
            _messageHandler = messageHandler;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"AWSQueueSubscriber Retry - Count:{retryCount}, Exception:{exception.Message}";
                        _appLogger?.LogWarning(msg);
                    }
                );
        }

        private void InitializeQueue()
        {
            if (_client == null)
            {
                _client = new AmazonSQSClient(_awsSettings.PubSub.AccessKey, _awsSettings.PubSub.SecretKey, new AmazonSQSConfig
                {
                    ServiceURL = _awsSettings.PubSub.QueueBasePath,
                    SignatureMethod = SigningAlgorithm.HmacSHA256
                });
            }
        }

        public async Task Process()
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await process();
                }
                catch (Exception ex)
                {
                    _appLogger?.LogError(ex);
                    throw;
                }
            });
        }

        private async Task process()
        {
            InitializeQueue();

            var queueName = _awsSettings.PubSub.QueueMappings[typeof(T).FullName];
            var queueUrl = $"{_awsSettings.PubSub.QueueBasePath}/{_awsSettings.PubSub.QueueIdentifier}/{queueName}";

            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 1
            };

            var receiveMessageResponse = await _client.ReceiveMessageAsync(receiveMessageRequest);

            foreach (var message in receiveMessageResponse.Messages)
            {
                _appLogger.LogMessage($"Message Type: {typeof(T).FullName} dequeued with MessageId: {message.MessageId}");

                _messageHandler(message.Body);

                var deleteMessageRequest = new DeleteMessageRequest(queueUrl, message.ReceiptHandle);
                await _client.DeleteMessageAsync(deleteMessageRequest);
            }
        }
    }
}

