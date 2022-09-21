using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// A collection of brackets which represent different groups at the same point in time.
    /// </summary>
    public class BracketSet
    {
        /// <summary>
        /// Creates a BracketSet from the given list of brackets.
        /// Ordering should be from lowest quantity seed to highest.
        /// (Best teams first).
        /// </summary>
        /// <param name="brackets">The collection of brackets to keep as a bracket set.</param>
        internal BracketSet(List<Bracket> brackets)
        {
            this.Brackets = brackets;
        }

        /// <summary>
        /// Gets all of the rounds for the set of brackets.
        /// </summary>
        /// <returns>List of List of BracketRounds.</returns>
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

        /// <summary>
        /// Returns the list of brackets this set represents.
        /// </summary>
        public List<Bracket> Brackets { get; private set; }

        /// <summary>
        /// Gets all of the rounds represented by every bracket.
        /// </summary>
        public List<List<BracketRound>> Rounds =>
            this.GetRounds();

        /// <summary>
        /// Gets all of the teams which are in any of the brackets.
        /// </summary>
        public List<Team> Teams
        {
            get
            {
                return this.Brackets.SelectMany(b => b.Teams).Distinct().ToList();
            }
        }
    }
}
