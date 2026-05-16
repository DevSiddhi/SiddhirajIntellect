using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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

        public static List<string> GetSegments()
        {
            return configuration.GetSection("Segments").Get<List<string>>();
        }

        public static void LoadConfigData()
        {
            CoCode = configuration.GetValue<bool>("CoCode");
            BseDeriveIndexOnly = configuration.GetValue<bool>("BseDeriveIndexOnly");
            CompanyMasterUrl = configuration.GetValue<string>("CompanyMasterUrl");
            CompanyMasterToken = configuration.GetValue<string>("CompanyMasterToken");
        }
    }
}