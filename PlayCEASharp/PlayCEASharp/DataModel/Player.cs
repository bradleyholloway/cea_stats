using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// Represents a play in playCEA.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Creates a player given the playerId.
        /// </summary>
        /// <param name="playerId">The id for the player. This is also the discord id.</param>
        public Player(string playerId)
        {
            this.PlayerId = playerId;
        }

        /// <summary>
        /// The unique id for the player.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// The display name for the player.
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// The Discord discriminator for the player.
        /// </summary>
        public string DiscordId { get; internal set; }

        /// <summary>
        /// The url for the players avatar picture.
        /// </summary>
        public string PictureURL { get; internal set; }

        /// <summary>
        /// The id for the player.
        /// </summary>
        public string DiscordUID { get; internal set; }

        /// <summary>
        /// If this player is a captain for their team.
        /// </summary>
        public bool Captain { get; internal set; }
    }
}
