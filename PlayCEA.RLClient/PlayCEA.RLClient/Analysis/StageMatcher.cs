﻿using PlayCEA.RLClient.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.Analysis
{
    public static class StageMatcher
    {
        public static string Lookup(string stageName)
        {
            string str;
            return (ConfigurationManager.Configuration.stageConfiguration.TryGetValue(stageName, out str) ? str : "default_stage");
        }
    }
}
