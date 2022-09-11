using PlayCEA.RLClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.RequestManagement
{
    public static class BracketRoundCache
    {
        private static readonly Dictionary<string, BracketRound> cache = new Dictionary<string, BracketRound>();

        public static BracketRound GetBracketRound(string id)
        {
            BracketRound round;
            if (BracketRoundCache.cache.ContainsKey(id))
            {
                round = BracketRoundCache.cache[id];
            }
            else
            {
                Dictionary<string, BracketRound> cache = BracketRoundCache.cache;
                lock (cache)
                {
                    if (BracketRoundCache.cache.ContainsKey(id))
                    {
                        round = BracketRoundCache.cache[id];
                    }
                    else
                    {
                        BracketRound round2 = new BracketRound(id);
                        BracketRoundCache.cache[id] = round2;
                        round = round2;
                    }
                }
            }
            return round;
        }
    }
}
