using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.DataModel
{
    public class Player
    {
        public Player(string playerId)
        {
            this.PlayerId = playerId;
        }

        public string PlayerId { get; }

        public string? DisplayName { get; set; }

        public string? DiscordId { get; set; }

        public bool Captain { get; set; }
    }
}
