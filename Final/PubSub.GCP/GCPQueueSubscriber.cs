using CommonTypes.Behaviours;
using CommonTypes.Settings;
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using Grpc.Auth;
using Grpc.Core;
using Polly;
using PubSub.Behaviours;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubSub.GCP
{
    public class GCPQueueSubscriber<T> : IQueueSubscriber<T>
    {
        private IAppLogger _appLogger;
        private GCPSettings _gcpSettings;
        private Action<string> _messageHandler;

        private Policy _retryPolicy;
        private SubscriberServiceApiClient _sub;

        private TopicName _topicName;
        private SubscriptionName _subscriptionName;
        
        public GCPQueueSubscriber(IAppLogger appLogger, GCPSettings gcpSettings, Action<string> messageHandler)
        {
            _appLogger = appLogger;
            _gcpSettings = gcpSettings;
            _messageHandler = messageHandler;


            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"GCPQueueSubscriber Retry - Count:{retryCount}, Exception:{exception.Message}";
                        _appLogger?.LogWarning(msg);
                    }
                );
        }

        private void InitializeQueue()
        {
            if (_sub == null)
            {
                var googleCredential = GoogleCredential.FromFile(_gcpSettings.PubSub.JsonAuthPath)
                    .CreateScoped(SubscriberServiceApiClient.DefaultScopes);

                var channel = new Channel(SubscriberServiceApiClient.DefaultEndpoint.ToString(), googleCredential.ToChannelCredentials());
                _sub = SubscriberServiceApiClient.Create(channel);
            }

            var queueName = _gcpSettings.PubSub.QueueMappings[typeof(T).FullName];

            if (_topicName == null)
            {
                _topicName = new TopicName(_gcpSettings.PubSub.ProjectId, queueName);
            }

            if (_subscriptionName == null)
            {
                _subscriptionName = new SubscriptionName(_gcpSettings.PubSub.ProjectId, queueName);
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

            var response = await _sub.PullAsync(_subscriptionName, false, 1,
                CallSettings.FromCallTiming(
                    CallTiming.FromExpiration(
                        Expiration.FromTimeout(
                            TimeSpan.FromSeconds(90)))));

            if (response.ReceivedMessages == null || response.ReceivedMessages.Count == 0)
            {
                return;
            }

            var message = response.ReceivedMessages[0];
            var jsonBytes = message.Message.Data.ToByteArray();
            var payload = Encoding.UTF8.GetString(jsonBytes);

            _appLogger.LogMessage($"Message Type: {typeof(T).FullName} dequeued with MessageId: {message.Message.MessageId}");

            _messageHandler(payload);

            await _sub.AcknowledgeAsync(_subscriptionName, new string[] { message.AckId });
        }
    }
}
