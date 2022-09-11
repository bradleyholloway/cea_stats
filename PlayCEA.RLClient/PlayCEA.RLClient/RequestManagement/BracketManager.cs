using PlayCEA.RLClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.RequestManagement
{
    internal class BracketManager : IDisposable
    {
        private static readonly RequestManager rm = new RequestManager();
        private string bracketId;
        private bool keepAlive = true;
        private Bracket bracket = null;

        internal BracketManager(string bracketId)
        {
            this.bracketId = bracketId;
            Task.Run(new Action(this.RefreshThread));
        }

        public void Dispose()
        {
            this.keepAlive = false;
        }

        internal void ForceUpdate()
        {
            try
            {
                Bracket result = rm.GetBracket(this.bracketId).Result;
                rm.UpdateAllTeams(result).Wait();
                this.bracket = result;
            }
            catch (Exception)
            {
            }
        }

        private void RefreshThread()
        {
            while (true)
            {
                if (this.keepAlive)
                {
                    Thread.Sleep(TimeSpan.FromMinutes(5.0));
                    if (this.keepAlive)
                    {
                        try
                        {
                            Bracket result = rm.GetBracket(this.bracketId).Result;
                            rm.UpdateAllTeams(result).Wait();
                            this.bracket = result;
                        }
                        catch (Exception)
                        {
                        }
                        continue;
                    }
                }
                return;
            }
        }

        internal Bracket Bracket
        {
            get
            {
                Bracket bracket;
                if (this.bracket != null)
                {
                    bracket = this.bracket;
                }
                else
                {
                    this.bracket = rm.GetBracket(this.bracketId).Result;
                    try
                    {
                        rm.UpdateAllTeams(this.bracket).Wait();
                    }
                    catch (Exception)
                    {
                    }
                    bracket = this.bracket;
                }
                return bracket;
            }
        }
    }
}
