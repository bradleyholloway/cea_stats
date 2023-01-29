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
                string configString = File.ReadAllText(configurationPath);
                TournamentConfigurations = JsonSerializer.Deserialize<TournamentConfigurations>(configString);
            }
        }

        internal static TournamentConfigurations TournamentConfigurations { get; private set; }
    }
}
