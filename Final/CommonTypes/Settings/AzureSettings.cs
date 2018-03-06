using System.Collections.Generic;

namespace CommonTypes.Settings
{
    public class AzureSettings
    {
        public PubSubSettings PubSub { get; set; }

        public class PubSubSettings
        {
            public string ConnectionString { get; set; }
            public Dictionary<string, string> QueueMappings { get; set; }

        }
    }
}
