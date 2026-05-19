using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection.Emit;

namespace ConsoleApp1.Helpers
{
    public static class ConfigHelper
    {
        private static IConfigurationRoot configuration;

        public static bool CoCode { get; private set; }
        public static bool BseDeriveIndexOnly { get; private set; }
        public static string CompanyMasterUrl { get; private set; }
        public static string CompanyMasterToken { get; private  set; }

        static ConfigHelper()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/appsettings.json")
                .Build();

            LoadConfigData();
        }

        public static segmentSettings GetSegment() 
        { 
            return configuration.GetSection("Segments").Get<segmentSettings>();
        }

        public static List<string> GetEnabledSegments() 
        {
            var segments = GetSegment();
            var enabledSegments = new List<string>();
            if (segments.GroupsMembers) enabledSegments.Add("GroupsMembers");
            if (segments.NseEquity) enabledSegments.Add("NseEquity");
            if (segments.NseDerivative) enabledSegments.Add("NseDerivative");
            if (segments.BseEquity) enabledSegments.Add("BseEquity");
            if (segments.BseDerivative) enabledSegments.Add("BseDerivative");
            return enabledSegments;
        }
        public static void LoadConfigData()
        {
            CoCode = configuration.GetValue<bool>("CoCode");
            BseDeriveIndexOnly = configuration.GetValue<bool>("BseDeriveIndexOnly");
            CompanyMasterUrl = configuration.GetValue<string>("CompanyMasterUrl");
            CompanyMasterToken = configuration.GetValue<string>("CompanyMasterToken");
        }
    }

    public class  segmentSettings
    {
        public bool GroupsMembers { get; set; }
        public bool NseEquity { get; set; }
        public bool NseDerivative { get; set; }
        public bool BseEquity { get; set; }
        public bool BseDerivative { get; set; }
    }
}