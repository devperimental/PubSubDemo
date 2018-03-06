using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using CommonTypes.Behaviours;
using CommonTypes.Settings;
using Newtonsoft.Json;
using Polly;
using PubSub.Behaviours;
using System;
using System.Threading.Tasks;

namespace PubSub.AWS
{
    public class AWSQueuePublisher<T> : IQueuePublisher<T>
    {
        private IAppLogger _appLogger;
        private AWSSettings _awsSettings;

        private Policy _retryPolicy;
        private AmazonSQSClient _client;

        public AWSQueuePublisher(IAppLogger appLogger, AWSSettings awsSettings)
        {
            _appLogger = appLogger;
            _awsSettings = awsSettings;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"AWSQueuePublisher Retry - Count:{retryCount}, Exception:{exception.Message}";
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

        private async Task sendMessage(T messageObject)
        {
            InitializeQueue();

            var queueName = _awsSettings.PubSub.QueueMappings[messageObject.GetType().FullName];
            var queueUrl = $"{_awsSettings.PubSub.QueueBasePath}/{_awsSettings.PubSub.QueueIdentifier}/{queueName}";

            var message = JsonConvert.SerializeObject(messageObject);

            var sendRequest = new SendMessageRequest()
            {
                QueueUrl = queueUrl,
                MessageBody = message
            };

            sendRequest.MessageAttributes = new System.Collections.Generic.Dictionary<string, MessageAttributeValue>
            {
                ["Message.Type.FullName"] = new MessageAttributeValue()
                {
                    StringValue = messageObject.GetType().FullName,
                    DataType = "String",
                }
            };

            var messageResponse = await _client.SendMessageAsync(sendRequest);
            
            _appLogger.LogMessage($"Message Type: {messageObject.GetType().FullName} queued with MessageId: {messageResponse.MessageId} and HttpStatusCode: {messageResponse.HttpStatusCode}");
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
