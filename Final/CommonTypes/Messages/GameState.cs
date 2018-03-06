using System;
using System.Collections.Generic;
using System.Text;

namespace CommonTypes.Messages
{
    public class GameState
    {
        public string PlayerId { get; set; }
        public int Health { get; set; }
        public int CurrentLevel { get; set; }
        public Dictionary<string, string> Inventory { get; set; }
        public string GameId { get; set; }
        public string Provider { get; set; }
    }
}
