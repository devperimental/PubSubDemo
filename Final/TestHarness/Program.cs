using CommonTypes.Behaviours;
using CommonTypes.Messages;
using CommonTypes.Settings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PubSub.AWS;
using PubSub.Azure;
using PubSub.Behaviours;
using PubSub.GCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestHarness
{
    class Program
    {
        private static AWSSettings _awsSettings;
        private static AzureSettings _azureSettings;
        private static GCPSettings _gcpSettings;
        private static IAppLogger _consoleLogger;

        static void Main(string[] args)
        {
            _consoleLogger = new ConsoleLogger();

            _consoleLogger.LogMessage("Starting Test Harness!");

            try
            {
                InitConfiguration();
                TestMessagePublish();
                TestMessageSubscription();
            }
            catch (Exception ex)
            {
                _consoleLogger.LogError(ex);
            }
            finally
            {
                _consoleLogger.LogMessage("End Test Harness!");
            }
            Console.ReadLine();
        }

        static void InitConfiguration()
        {
            _consoleLogger.LogMessage("Start Init Config");

            // Used to build key/value based configuration settings for use in an application
            // Note: AddJsonFile is an extension methods for adding JsonConfigurationProvider.
            var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appSettings.json");

            // Builds an IConfiguration with keys and values from the set of sources
            var configuration = builder.Build();

            // Bind the respective section to the respective settings class 
            _awsSettings = configuration.GetSection("aws").Get<AWSSettings>();
            _azureSettings = configuration.GetSection("azure").Get<AzureSettings>();
            _gcpSettings = configuration.GetSection("gcp").Get<GCPSettings>();

            _consoleLogger.LogMessage("End Init Config");
        }

        static void TestMessagePublish()
        { 
            var publishers = new List<IQueuePublisher<GameState>>
            {
                new AWSQueuePublisher<GameState>(_consoleLogger, _awsSettings),
                new GCPQueuePublisher<GameState>(_consoleLogger, _gcpSettings),
                new AzureQueuePublisher<GameState>(_consoleLogger, _azureSettings)
            };

            try
            {
                publishers.ForEach(c => {
                    for (var i = 0; i < 1; i++)
                    {
                        var item = new GameState()
                        {
                            PlayerId = Guid.NewGuid().ToString(),
                            Health = 100,
                            CurrentLevel = 1,
                            GameId = Guid.NewGuid().ToString(),
                            Inventory = new Dictionary<string, string>() { { "Item", "Amulet" } },
                            Provider = c.GetType().ToString()
                        };

                        c.SendMessage(item).Wait();
                    }
                });
            }
            catch (Exception ex)
            {
                _consoleLogger.LogError(ex);
            }
        }

        static void TestMessageSubscription()
        {
            var subscribers = new List<IQueueSubscriber<GameState>>
            {
                new AWSQueueSubscriber<GameState>(_consoleLogger, _awsSettings, MessageHandler),
                new GCPQueueSubscriber<GameState>(_consoleLogger, _gcpSettings, MessageHandler),
                new AzureQueueSubscriber<GameState>(_consoleLogger, _azureSettings, MessageHandler)
            };

            while (true)
            {
                try
                {
                    subscribers.ForEach(x => x.Process().Wait());
                }
                catch (Exception ex)
                {
                    _consoleLogger.LogError(ex);
                }
            }
        }

        static void MessageHandler(string message)
        {
            var gameState = JsonConvert.DeserializeObject<GameState>(message);

            var messageContents = new StringBuilder();

            messageContents.AppendLine($"PlayerId: {gameState.PlayerId}");
            messageContents.AppendLine($"Health: {gameState.Health}");
            messageContents.AppendLine($"CurrentLevel: {gameState.CurrentLevel}");
            messageContents.AppendLine($"GameId: {gameState.GameId}");
            messageContents.AppendLine($"Provider: {gameState.Provider}");
            messageContents.AppendLine($"Inventory:");

            foreach (var item in gameState.Inventory)
            {
                messageContents.AppendLine($"-->{item.Key}|{item.Value}");

            }

            _consoleLogger.LogMessage(messageContents.ToString());
        }
    }
}
