using System;
using System.Collections.Generic;
using System.Text;

namespace CommonTypes.Settings
{
    public class AWSSettings
    {
        public PubSubSettings PubSub { get; set; }

        public class PubSubSettings
        {
            public string AccessKey { get; set; }
            public string SecretKey { get; set; }
            public string QueueBasePath { get; set; }
            public string QueueIdentifier { get; set; }
            public Dictionary<string, string> QueueMappings { get; set; }
        }
    }
}
