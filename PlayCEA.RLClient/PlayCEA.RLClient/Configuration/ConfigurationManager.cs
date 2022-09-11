using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.Configuration
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
                Configuration = JsonSerializer.Deserialize<BracketConfiguration>(configString);
            }
        }

        public static void UpdateInMemoryConfiguration(BracketConfiguration config)
        {
            Configuration = config;
        }

        public static void WriteConfiguration(BracketConfiguration config)
        {
            object fileLock = ConfigurationManager.fileLock;
            lock (fileLock)
            {
                string str = JsonSerializer.Serialize(config);
                UpdateInMemoryConfiguration(config);
                File.WriteAllText("Resources/configuration.json", str);
            }
        }

        public static BracketConfiguration Configuration { get; private set; }
    }
}
