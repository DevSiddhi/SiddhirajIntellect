using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using ConsoleApp1.Processes;
using ConsoleApp1.QH;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1.Helpers
{
    public static class CommonFunctNSE
    {
        public static List<string> RL = new List<string>();
        public static List<string> SL = new List<string>();
        public static readonly DateTime ExchStartDate = new DateTime(1980, 1, 1);

        public static async Task Download_CoCode()
        {
            if (string.IsNullOrEmpty(ConfigHelper.CompanyMasterUrl)) return;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    if (!string.IsNullOrEmpty(ConfigHelper.CompanyMasterToken))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigHelper.CompanyMasterToken);
                    HttpResponseMessage response = await client.GetAsync(ConfigHelper.CompanyMasterUrl);
                    if (!response.IsSuccessStatusCode) return;
                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (responseBody == null || responseBody == "") return;
                    CompanyMasterReply? companyMasterReply = JsonConvert.DeserializeObject<CompanyMasterReply>(responseBody);
                    DateTime dt = DateTime.Now;
                    string coCodeFileName = $"co_code.csv";

                    List<string> csvData = new List<string>
                    {
                        "co_code,bsecode,nsesymbol,companyname,companyshortname,categoryname,isin,bsegroup,mcaptype,sectorcode,sectorname,industrycode,industryname,bselistedflag,nselistedflag,displaytype"
                    };

                    if (companyMasterReply != null && companyMasterReply.success) NseSecuritySegmentProcess.coCodeRecords = companyMasterReply.data;
                    foreach (var item in NseSecuritySegmentProcess.coCodeRecords)
                    {
                        csvData.Add($"{item.co_code},{item.bsecode},{item.nsesymbol},{item.companyname},{item.companyshortname},{item.categoryname},{item.isin},{item.bsegroup},{item.mcaptype},{item.sectorcode},{item.sectorname},{item.industrycode},{item.industryname},{item.bselistedflag},{item.nselistedflag},{item.DisplayType}");
                    }
                    File.WriteAllLines(coCodeFileName, csvData);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        public static void readBhavCopy_NseIndices(ScripData_Dict scripDataEQ_Dict)
        {
            string FName = null, ExposureFile = null, callput = null, Str;


            var Date = Convert.ToDateTime(MainQH.LastTradingDate); //LastTradingDate.Value.Date;
            var Format = "ddMMyy";
            FName = "NSE" + Date.ToString(Format) + ".Zip";
            SL.Add("ScripCode,Closing");

            if (!File.Exists(FName))
            {
                Str = FName + " is not available in current folder";
                Console.WriteLine(Str);
                CommonFunctions.WriteToLogFile(Str);
                return;
            }
            else
            {
                try { EODZIPClass.UnZipFiles(FName, AppDomain.CurrentDomain.BaseDirectory); }
                catch (Exception ex)
                {
                    Str = "Error Extracting " + FName + ".Exception : " + ex.Message;
                    CommonFunctions.WriteToLogFile(Str);
                    return;
                }
                // EODZIPClass.UnZipFiles(FName, "BhavCopyImport"); //This is use to UnZip the Zip file
                // 27.02.17 This is use to UnZip the Zip file
            }
            var FormatPd = "ddMMyyyy";
            FName = "PD" + Date.ToString(FormatPd) + ".Csv";
            var PDPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + FName;
            if (!File.Exists(PDPath))
            {
                Str = FName + " is not available in current folder";
                CommonFunctions.WriteToLogFile(Str);
            }
            else
            {
                try
                {
                    var Data = File.ReadAllLines(PDPath);
                    //if (numMinNSEPD.Value > 0 && Data.Length < numMinNSEPD.Value)
                    //{
                    //    Str = FName + $" falls below Expected Range. \r\nExpected Minimum value : {numMinNSEPD.Value}, Current : {Data.Length}. Verify Before Proceeding.";
                    //    MessageBox.Show(Str);
                    //    WriteToLogFile(Str);
                    //}

                    for (int i = 1; i < Data.Length; i++)
                    {
                        var Line = Data[i].Split(',');
                        if (Line[0] != "Y") break;//Read Only Indices. Skip other Data
                        var Symbol = Line[3];
                        var Close = Line[8];
                        var Open = Line[5];
                        var High = Line[6];
                        var Low = Line[7];
                        var YHigh = Line[14];
                        var YLow = Line[15];
                        var PrevClose = Line[4];
                        var scripRec = scripDataEQ_Dict.GetRecord(Symbol);
                        if (scripRec == null)
                        {
                            scripRec = new ScripData
                            {
                                Name = Symbol,
                                Series = "EQ"
                            };
                            scripDataEQ_Dict.addEntry(scripRec);
                        }
                        scripRec.PrevOpen = Convert.ToDecimal(Open);
                        scripRec.PrevHigh = Convert.ToDecimal(High);
                        scripRec.PrevLow = Convert.ToDecimal(Low);
                        scripRec.YearlyHigh = Convert.ToDecimal(YHigh);
                        scripRec.YearlyLow = Convert.ToDecimal(YLow);
                        scripRec.PrevClose = Convert.ToDecimal(Close);
                        scripRec.PrevDayClose = Convert.ToDecimal(PrevClose);
                    }

                }
                catch (Exception ex)
                {
                    Str = "Error Processing " + FName + ".\r\nException : " + ex.Message;
                    CommonFunctions.WriteToLogFile(Str);
                }
            }

        }

        public static void readBhavCopy_EQ(ScripData_Dict scripDataEQ_Dict, string FileName)
        {
            string[] fileData = File.ReadAllLines(FileName);

            //if (minCount > 0 && fileData.Length < minCount)
            //{
            //    Str = FileName + $" falls below Expected Range. \r\nExpected Minimum value : {minCount}, Current : {fileData.Length}. Verify Before Proceeding.";
            //    WriteToLogFile(Str);
            //}

            for (int i = 1; i < fileData.Length; i++)//Skip First Line - Header
            {
                try
                {
                    string[] lineData = fileData[i].Split(',');
                    string Symbol = lineData[7];
                    string Series = lineData[8];

                    var scripRec = scripDataEQ_Dict.GetRecord(Symbol, Series);
                    if (scripRec == null)
                    {
                        scripRec = new ScripData
                        {
                            Series = Series,
                            Name = Symbol
                        };

                        scripDataEQ_Dict.addEntry(scripRec);
                    }
                    scripRec.PrevOpen = Convert.ToDecimal(lineData[14]);
                    scripRec.PrevHigh = Convert.ToDecimal(lineData[15]);
                    scripRec.PrevLow = Convert.ToDecimal(lineData[16]);
                    scripRec.PrevClose = Convert.ToDecimal(lineData[17]);
                    scripRec.PrevDayClose = Convert.ToDecimal(lineData[19]);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public  static void readBhavCopy_FO(ScripData_Dict scripDataEQ_Dict, string FileName)
        {
            try
            {
                string format = "dd MMM yyyy";
                string[] fileData = File.ReadAllLines(FileName);
                for (int i = 1; i < fileData.Length; i++)//Skip First Line - Header
                {
                    try
                    {
                        string[] lineData = fileData[i].Split(',');

                        string OFISType = lineData[4];
                        int index = Array.IndexOf(MainQH.OFISType, OFISType);
                        string Symbol = lineData[7];
                        DateTime ExpiryDt = DateTime.ParseExact(lineData[9], "yyyy-MM-dd", null);
                        float.TryParse(lineData[11], out var StrikePrice);
                        string OptTp = lineData[12];
                        int index2 = Array.IndexOf<string>(MainQH.CPType, OptTp);
                        if (index2 > 0)
                            Symbol = Symbol + " " + ExpiryDt.ToString(format) + " " + OptTp + " " + string.Format("{0:0.00}", StrikePrice);
                        else
                            Symbol = Symbol + " " + ExpiryDt.ToString(format);

                        var scripRec = scripDataEQ_Dict.GetRecord(Symbol);
                        if (scripRec == null)
                        {
                            scripRec = new ScripData
                            {
                                Name = Symbol
                            };
                            scripDataEQ_Dict.addEntry(scripRec);
                        }

                        scripRec.PrevOpen = Convert.ToDecimal(lineData[14]);
                        scripRec.PrevHigh = Convert.ToDecimal(lineData[15]);
                        scripRec.PrevLow = Convert.ToDecimal(lineData[16]);
                        scripRec.PrevClose = Convert.ToDecimal(lineData[17]);
                        scripRec.PrevOI = (int)Math.Min(int.MaxValue, Convert.ToDecimal(lineData[22]));
                        scripRec.PrevDayClose = Convert.ToDecimal(lineData[19]);
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
            catch (Exception)
            {

            }
        }


        public static void readBhavCopy_sme(ScripData_Dict scripDataEQ_Dict, string smeFileName) 
        {
            string[] fileData = File.ReadAllLines(smeFileName);

            //if (numMinNSESME.Value > 0 && fileData.Length < numMinNSESME.Value)
            //{
            //    Str = smeFileName + $" falls below Expected Range. \r\nExpected Minimum value : {numMinNSESME.Value}, Current : {fileData.Length}. Verify Before Proceeding.";
            //    MessageBox.Show(Str);
            //    WriteToLogFile(Str);
            //}
            for (int i = 1; i < fileData.Length; i++)//Skip First Line - Header
            {
                try
                {
                    string[] lineData = fileData[i].Split(',');
                    string Symbol = lineData[2];
                    string Series = lineData[1];

                    var scripRec = scripDataEQ_Dict.GetRecord(Symbol, Series);
                    if (scripRec == null)
                    {
                        scripRec = new ScripData
                        {
                            Series = Series,
                            Name = Symbol
                        };

                        scripDataEQ_Dict.addEntry(scripRec);
                    }
                    scripRec.PrevOpen = Convert.ToDecimal(lineData[5]);
                    scripRec.PrevHigh = Convert.ToDecimal(lineData[6]);
                    scripRec.PrevLow = Convert.ToDecimal(lineData[7]);
                    scripRec.PrevClose = Convert.ToDecimal(lineData[8]);
                    scripRec.PrevDayClose = Convert.ToDecimal(lineData[4]);
                    scripRec.YearlyHigh = Convert.ToDecimal(lineData[12]);
                    scripRec.YearlyLow = Convert.ToDecimal(lineData[13]);


                }
                catch (Exception ex)
                {

                }
            }
        }

        public static StatusMessage ReadNse52HLAdjusted(ScripData_Dict scripDataEQ_Dict, bool CheckFile)
        {

            #region Variables
            string Symbol, Series, NseSymbol = "", NseSeries, FName, FName2, Format;
            string[] Record = null, Data = null;
            char[] Split = new char[1] { ',' };
            int TickSize = 0;
            ScripData scripRec = null;
            decimal Hi_52, Low_52;
            DateTime Date;
            string LatestFileName = "";
            DateTime LatestDate = DateTime.MinValue;
            #endregion
            try
            {

                Date = Convert.ToDateTime(MainQH.LastTradingDate); //this.LastTradingDate.Value;
                Format = "ddMMyyyy";
                FName2 = "CM_52_wk_High_low.Csv";
                if (File.Exists(FName2))
                {
                    var enumerable = File.ReadLines(FName2);
                    string data = enumerable.ElementAt(1);
                    string[] dataFiltered = data.Split(' ');
                    DateTime date = DateTime.Parse(dataFiltered[2].Substring(0, dataFiltered[2].Length - 1));
                    if (date > LatestDate)
                    {
                        LatestDate = date;
                        LatestFileName = FName2;
                    }

                }
                FName2 = "CM_52_wk_High_low_" + DateTime.Today.ToString(Format) + ".Csv";
                if (File.Exists(FName2))
                {
                    var enumerable = File.ReadLines(FName2);
                    string data = enumerable.ElementAt(1);
                    string[] dataFiltered = data.Split(' ');
                    DateTime date = DateTime.Parse(dataFiltered[2].Substring(0, dataFiltered[2].Length - 1));
                    if (date > LatestDate)
                    {
                        LatestDate = date;
                        LatestFileName = FName2;
                    }
                }
                FName2 = "CM_52_wk_High_low_" + Date.ToString(Format) + ".Csv";
                if (File.Exists(FName2))
                {
                    var enumerable = File.ReadLines(FName2);
                    string data = enumerable.ElementAt(1);
                    string[] dataFiltered = data.Split(' ');
                    DateTime date = DateTime.Parse(dataFiltered[2].Substring(0, dataFiltered[2].Length - 1));
                    if (date > LatestDate)
                    {
                        LatestDate = date;
                        LatestFileName = FName2;
                    }
                }

                if (CheckFile)
                {
                    if (LatestFileName == "")
                        return new StatusMessage { message = "CM_52_wk_High_low.csv" };
                    else
                        return new StatusMessage
                        {
                            message = "",
                            status = true
                        };
                }
                if (LatestFileName == "")
                {
                    return new StatusMessage
                    {
                        message = "CM_52_wk_High_low File is not available. Please Update",
                        status = false
                    };
                }
                {
                    Data = File.ReadAllLines(LatestFileName);
                    //if (numMinNSECM52.Value > 0 && Data.Length < numMinNSECM52.Value)
                    //{
                    //    Str = LatestFileName + $" falls below Expected Range. \r\nExpected Minimum value : {numMinNSECM52.Value}, Current : {Data.Length}. Verify Before Proceeding.";
                    //    MessageBox.Show(Str);
                    //    WriteToLogFile(Str);
                    //}
                    for (int i = 0; i < Data.Length; i++)
                    {
                        try
                        {
                            Record = Data[i].Split(Split);
                            if (Record.Length < 5) continue;
                            if (Record[0][0] == '\"')
                            {
                                for (int k = 0; k < Record.Length; k++)
                                {
                                    Record[k] = Record[k].Substring(1, Record[k].Length - 2);
                                }
                            }
                            if (!Decimal.TryParse(Record[2], out Hi_52)) continue;
                            if (!Decimal.TryParse(Record[4], out Low_52)) continue;
                            Series = Record[1];
                            Symbol = Record[0];

                            scripRec = scripDataEQ_Dict.GetRecord(Symbol, Series);
                            if (scripRec == null)
                            {
                                scripRec = new ScripData
                                {
                                    Series = Series,
                                    Name = Symbol
                                };
                                scripDataEQ_Dict.addEntry(scripRec);
                            }
                            if (DateTime.TryParse(Record[3], out DateTime YearHDate))
                            {
                                scripRec.YearlyHighDate = (YearHDate - ExchStartDate).Days;
                            }
                            if (DateTime.TryParse(Record[5], out DateTime YearLDate))
                            {
                                scripRec.YearlyLowDate = (YearLDate - ExchStartDate).Days;
                            }

                            NseSymbol = Symbol;


                            //if (NseAllowedIntradayTokenList.Contains(Token)) ScripAllowedInIntraday = 'Y'; //22.12.15                       

                            if (Hi_52 < scripRec.PrevHigh)
                                Hi_52 = scripRec.PrevHigh;
                            if (Low_52 > scripRec.PrevLow && scripRec.PrevLow > 0)
                                Low_52 = scripRec.PrevLow;
                            scripRec.YearlyHigh = Hi_52;
                            scripRec.YearlyLow = Low_52;
                        }
                        catch (Exception EE)
                        {
                            CommonFunctions.ErrorLog(EE);
                        }
                    }
                }
                return new StatusMessage
                {
                    message = "52HL Processed",
                    status = true
                };
            }
            catch (Exception ex)
            {
                return new StatusMessage
                {
                    message = ex.Message,
                    status = false
                };
            }
        }

        public static string GetREGString(string[] Parts, bool FoSecban = false)
        {
            int StartIndex = 5;
            int EndIndex = 47;

            string FOSecBanStr = "4.0.0.1";
            string[] IndexMapping = { "", "", "", "", "",
                                "4.1.1" ,"4.1.2" ,"4.1.5" ,"4.1.6","4.1.3" ,"4.1.8","4.1.7","","","4.1.9","4.1.12"  ,"4.1.10","4.1.11","4.1.4" ,
                                "4.2.1" ,"4.2.2" ,"4.2.3" ,"4.2.4" ,"","4.2.5" ,
                                "4.3.1" ,"4.3.2" ,"4.3.3" ,
                                "4.4.1" ,"4.4.2" ,
                                "4.5.1" ,"4.5.2" ,"4.5.3" ,"4.5.4" ,"4.5.5" ,"4.5.6" ,"4.5.7" ,"4.5.8" ,"4.5.9" ,"4.5.10" ,"4.5.11" ,"4.5.12" ,"4.5.13" ,"4.5.14" ,"4.5.15" ,"4.5.16" ,"4.5.17" ,"4.5.18"
                            };

            string SurvillanceMsg = "";

            if (FoSecban)
            {
                SurvillanceMsg = FOSecBanStr + ",";
            }
            for (int index = StartIndex; index <= EndIndex; index++)
            {
                if (int.TryParse(Parts[index], out int Val))
                {
                    if (Val < 100)
                        SurvillanceMsg += $"{IndexMapping[index]}.{Val},";
                }
            }
            if (SurvillanceMsg != "")
            {
                SurvillanceMsg = SurvillanceMsg.Substring(0, SurvillanceMsg.Length - 1);
            }
            return SurvillanceMsg;
        }

        public static string[] SplitCSV(string s, char Delim)
        {
            List<string> list = new List<string>();
            StringBuilder work = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"')
                {
                    int p2;
                    while (true)
                    {
                        p2 = s.IndexOf('"', i + 1);
                        work.Append(s.Substring(i + 1, p2 - i - 1));
                        i = p2;

                        // If this is a double quote, keep going!
                        if (((p2 + 1) < s.Length) && (s[p2 + 1] == '"'))
                        {
                            work.Append('"');
                            i++;

                            // otherwise, this is a single quote, we're done
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (c == Delim)
                {
                    list.Add(work.ToString());
                    work.Length = 0;
                }
                else
                {
                    work.Append(c);
                }
            }
            list.Add(work.ToString());
            return list.ToArray();
        }

        public static Dictionary<int, bseIndexData> readBhavCopy_BseIndices()
        {
            string FName = null, ExposureFile = null, callput = null, Str;
            var Date = Convert.ToDateTime(MainQH.LastTradingDate);
            var Format = "ddMMyy";
            FName = "QE" + Date.ToString(Format) + ".Zip";
            SL.Add("ScripCode,Closing");

            if (!File.Exists(FName))
            {
                Str = FName + " is not available in current folder";
                Console.WriteLine(Str);
                CommonFunctions.WriteToLogFile(Str);
            }
            else
            {
                EODZIPClass.UnZipFiles(FName, AppDomain.CurrentDomain.BaseDirectory); 
            }

            FName = "BI" + Date.ToString(Format) + ".Dbf";
            var BIPath = AppDomain.CurrentDomain.BaseDirectory  + "\\" + FName;
            Dictionary<int, bseIndexData> index = new Dictionary<int, bseIndexData>();
            DataTable dt_BI = ParseDBF.ReadDBF(BIPath);
            for (int k = 0; k < dt_BI.Rows.Count; k++)
            {
                int IndexCode = Convert.ToInt32(dt_BI.Rows[k]["INDEX_CODE"]);

                if (IndexCode > 100)
                {
                    continue;
                }
                int ScripCode = 999900 + Convert.ToInt32(dt_BI.Rows[k]["INDEX_CODE"]);


                var Symbol = Convert.ToString(dt_BI.Rows[k]["INDEX_NAME"]);
                float Close = Convert.ToSingle(dt_BI.Rows[k]["CLOSING"]);
                var Open = Convert.ToSingle(dt_BI.Rows[k]["OPENING"]);
                var High = Convert.ToSingle(dt_BI.Rows[k]["HIGH"]);
                var Low = Convert.ToSingle(dt_BI.Rows[k]["LOW"]);
                var PrevClose = Convert.ToSingle(dt_BI.Rows[k]["LAST_DAY"]);

                if (!index.ContainsKey(ScripCode))
                {
                    index.Add(ScripCode, new bseIndexData
                    {
                        PrevOpen = Convert.ToDecimal(Open),
                        PrevHigh = Convert.ToDecimal(High),
                        PrevLow = Convert.ToDecimal(Low),
                        PrevClose = Convert.ToDecimal(Close),
                        PrevDayClose = Convert.ToDecimal(PrevClose)
                    });
                }

                SL.Add(ScripCode + "," + Close);
            }
            return index;
        }

    }
}
