using System;
using System.Collections.Generic;
using System.Text;

namespace CommonTypes.Settings
{
    public class GCPSettings
    {
        public PubSubSettings PubSub { get; set; }

        public class PubSubSettings
        {
            public string ProjectId { get; set; }
            public string JsonAuthPath { get; set; }
            public Dictionary<string, string> QueueMappings { get; set; }
        }
    }
}
