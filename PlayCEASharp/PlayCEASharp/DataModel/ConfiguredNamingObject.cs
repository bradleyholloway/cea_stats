using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Configuration;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// Represents a single game in PlayCEA.
    /// </summary>
    public abstract class ConfiguredNamingObject
    {
        /// <summary>
        /// The NamingConfiguration to be used.
        /// </summary>
        public NamingConfiguration NameConfiguration { get; internal set; }
    }
}
