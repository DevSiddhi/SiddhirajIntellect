using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1.Helpers
{
    public static class ConfigHelper
    {
        private static IConfigurationRoot configuration;
        public static string CompanyMasterUrl { get; set; }
        public static string CompanyMasterToken { get; set; }

        static ConfigHelper()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/appsettings.json")
                .Build();

            LoadCompanyMasterConfig();
        }

        public static List<string> GetSegments()
        {
            return configuration.GetSection("Segments").Get<List<string>>();
        }

        public static void LoadCompanyMasterConfig()
        {
            CompanyMasterUrl = configuration["CompanyMaster"];
            CompanyMasterToken = configuration["CompanyMasterToken"];
        }
    }
}