using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlayCEASharp.Configuration
{
    internal class ConfigurationManager
    {
        private const string configurationPath = "Resources/configuration.json";
        private static object fileLock = new object();

        static ConfigurationManager()
        {
            ReadConfiguration();
        }

        internal static void ReadConfiguration()
        {
            object fileLock = ConfigurationManager.fileLock;
            lock (fileLock)
            {
                string configString = File.ReadAllText("Resources/configuration.json");
                NamingConfiguration = JsonSerializer.Deserialize<NamingConfiguration>(configString);
                MatchingConfiguration = JsonSerializer.Deserialize<MatchingConfiguration>(configString);
                Configuration = JsonSerializer.Deserialize<BracketConfiguration>(configString);
            }
        }

        internal static void UpdateInMemoryConfiguration(BracketConfiguration config)
        {
            Configuration = config;
        }

        internal static BracketConfiguration Configuration { get; private set; }

        internal static MatchingConfiguration MatchingConfiguration { get; private set; }

        internal static NamingConfiguration NamingConfiguration { get; private set; }
    }
}
