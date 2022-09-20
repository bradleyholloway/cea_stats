using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlayCEASharp.Configuration
{
    public class ConfigurationManager
    {
        private const string configurationPath = "Resources/configuration.json";
        private static object fileLock = new object();

        static ConfigurationManager()
        {
            ReadConfiguration();
        }

        public static void ReadConfiguration()
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

        public static void UpdateInMemoryConfiguration(BracketConfiguration config)
        {
            Configuration = config;
        }

        public static BracketConfiguration Configuration { get; private set; }

        public static MatchingConfiguration MatchingConfiguration { get; private set; }

        public static NamingConfiguration NamingConfiguration { get; private set; }
    }
}
