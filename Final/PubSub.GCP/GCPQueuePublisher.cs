using CommonTypes.Behaviours;
using CommonTypes.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Grpc.Auth;
using Polly;
using PubSub.Behaviours;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PubSub.GCP
{
    public class GCPQueuePublisher<T> : IQueuePublisher<T>
    {
        private IAppLogger _appLogger;
        private GCPSettings _gcpSettings;

        private Policy _retryPolicy;
        private PublisherServiceApiClient _pub;

        public GCPQueuePublisher(IAppLogger appLogger, GCPSettings gcpSettings)
        {
            _appLogger = appLogger;
            _gcpSettings = gcpSettings;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"GCPQueuePublisher Retry - Count:{retryCount}, Exception:{exception.Message}";
                        _appLogger?.LogWarning(msg);
                    }
                );
        }

        private void InitializeQueue()
        {
            // https://github.com/GoogleCloudPlatform/google-cloud-dotnet/issues/1576
            if (_pub == null)
            {
                var googleCredential = GoogleCredential.FromFile(_gcpSettings.PubSub.JsonAuthPath).CreateScoped(PublisherServiceApiClient.DefaultScopes);
          
                var channel = new Channel(PublisherServiceApiClient.DefaultEndpoint.ToString(), googleCredential.ToChannelCredentials());

                _pub = PublisherServiceApiClient.Create(channel);
            }
        }

        private async Task sendMessage(T messageObject)
        {
            InitializeQueue();

            var queueName = _gcpSettings.PubSub.QueueMappings[messageObject.GetType().FullName];
            var _topicName = new TopicName(_gcpSettings.PubSub.ProjectId, queueName);
            var json = JsonConvert.SerializeObject(messageObject);

            var attributes = new Dictionary<string, string>
            {
                { "Message.Type.FullName", messageObject.GetType().FullName }
            };

            var message = new PubsubMessage()
            {
                Data = Google.Protobuf.ByteString.CopyFromUtf8(json)
            };

            message.Attributes.Add(attributes);
            var messageResponse = await _pub.PublishAsync(_topicName, new[] { message });

            _appLogger.LogMessage($"Message Type: {messageObject.GetType().FullName} queued with MessageId: {messageResponse.MessageIds[0]}");

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
