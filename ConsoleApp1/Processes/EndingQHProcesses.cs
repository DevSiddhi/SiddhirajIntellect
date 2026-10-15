using JIO_QH.Helpers;
using JIO_QH.QH;
using Newtonsoft.Json;
using System;

namespace JIO_QH.Processes
{
    public class EndingQHProcesses
    {
        public static void CreateMastersDateJson()
        {
            try
            {
                CommonFunctions.ExportToCsv("MasterDate", "MasterDate.csv");
                var masterDateConfig = ConfigHelper.GetMasterDateConfig();
                if (masterDateConfig == null)
                {
                    Console.WriteLine("MasterDate configuration not found.");
                    return;
                }

                string json = JsonConvert.SerializeObject(new
                {
                    data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.000000"),
                    FA = masterDateConfig.FA,
                    OA = masterDateConfig.OA,
                    FI = masterDateConfig.FI,
                    OI = masterDateConfig.OI,
                    TU = masterDateConfig.TU,
                    BU = masterDateConfig.BU,
                    MU = masterDateConfig.MU
                });

                File.WriteAllText(Path.Combine(CommonFunctions.MasterCSV, "MastersDate.json"), json);
                Console.WriteLine("MastersDate.json created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating MastersDate.json: " + ex.Message);
            }
        }
    }
}