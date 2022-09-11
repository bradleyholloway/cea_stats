using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.DataModel
{
    public class BracketSet
    {
        public BracketSet(List<Bracket> brackets)
        {
            this.Brackets = brackets;
        }

        private List<List<BracketRound>> GetRounds()
        {
            List<List<BracketRound>> list = new List<List<BracketRound>>();

            int num = this.Brackets.Select(b => b.Rounds.Count).Max();
            for (int i = 0; i < num; i++)
            {
                List<BracketRound> item = new List<BracketRound>();
                foreach (Bracket bracket in this.Brackets)
                {
                    if (bracket.Rounds.Count > i)
                    {
                        item.Add(bracket.Rounds[i]);
                    }
                }
                list.Add(item);
            }
            return list;
        }

        public List<Bracket> Brackets { get; private set; }

        public List<List<BracketRound>> Rounds =>
            this.GetRounds();

        public List<Team> Teams
        {
            get
            {
                return this.Brackets.SelectMany(b => b.Teams).Distinct().ToList();
            }
        }
    }
}
