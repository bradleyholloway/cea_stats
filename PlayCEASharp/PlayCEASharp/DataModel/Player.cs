using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    public class Player
    {
        public Player(string playerId)
        {
            this.PlayerId = playerId;
        }

        public string PlayerId { get; }

        public string DisplayName { get; internal set; }

        public string DiscordId { get; internal set; }

        public string PictureURL { get; internal set; }

        public string DiscordUID { get; internal set; }

        public bool Captain { get; internal set; }
    }
}
