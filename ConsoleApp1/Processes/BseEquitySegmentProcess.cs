using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using ConsoleApp1.Proto;
using ConsoleApp1.QH;
using Google.Protobuf;
using Master;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Processes
{
    public class BseEquitySegmentProcess
    {
        public static List<CoCodeMaster> coCodeRecords = new List<CoCodeMaster>();
        string T2TFile = "T2TRpt" + DateTime.Today.ToString("yyyyMMdd") + ".Csv";
        public static List<string> DBPendingList = new List<string>();

        public StatusMessage ExecuteBseScrip(bool CheckFile = false)
        {
            StatusMessage res = new StatusMessage();
            SQLiteHelper.OpenSqlite();
            res = ExecuteBseScrip_New(CheckFile);
            
            string fields = ConfigHelper.CoCode
                ? "Exch, ScripCode, Name, Desc,TradingSymbol, TickSize, Series, MinimumLotQty, ISIN, CoCode, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose ,YearlyHigh ,YearlyLow, SurveillanceFlag"
                : "Exch, ScripCode, Name, Desc,TradingSymbol, TickSize, Series, MinimumLotQty, ISIN, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose ,YearlyHigh ,YearlyLow, SurveillanceFlag";

            if (res.status) 
            {
                SQLiteHelper.CopySQLiteTabletoChild("BseEquityMaster", fields);
                CommonFunctions.ExportToCsv("child.BseEquityMaster", "BseEquityMaster.csv");
            } 
            return res;
        }
        private StatusMessage ExecuteBseScrip_New(bool CheckFile = false)
        {
            string outputMessage = "";
            var dt = new DateTime(2026, 05, 14);
            string DPFile = $"DP{DateTime.Now:ddMMyy}";
            string REGFile = $"BSE_REG1_IND{dt:ddMMyy}.csv";
            string securityFileName = $"BSE_EQ_SCRIP_{dt:ddMMyyyy}.csv";
            string bhavCopyFileName = $"BhavCopy_BSE_CM_0_0_0_{dt:yyyyMMdd}_F_0000.csv";
            string coCodeFileName = "co_code.csv";
            List<EquityMasterEntry> protoRecordsBSE = new();



            if (CheckFile)
            {
                if (!File.Exists(securityFileName)) outputMessage += $"{securityFileName},";
                if (!File.Exists(bhavCopyFileName)) outputMessage += $"{bhavCopyFileName},";
                if (!File.Exists(DPFile)) outputMessage += $"{DPFile},";
                if (!File.Exists(REGFile)) outputMessage += $"{REGFile},";
                if (!File.Exists("BseIndices.txt")) outputMessage += "BseIndices.txt,";
                if (!File.Exists("Bse_AllowTradingExtra.txt")) outputMessage += "Bse_AllowTradingExtra.txt,";
                if (!File.Exists("Bse_DoNotAllowTrading.txt")) outputMessage += "Bse_DoNotAllowTrading.txt,";
                return new StatusMessage { message = outputMessage };
            }

            if (ConfigHelper.CoCode) //checkCoCode.Checked
            {
                if (!File.Exists(coCodeFileName))
                {
                    Task.Run(() => CommonFunctNSE.Download_CoCode()).Wait();
                }
                if (coCodeRecords.Count == 0)
                {
                    foreach (string line in File.ReadAllLines(coCodeFileName).Skip(1))
                    {
                        string[] columns = line.Split(',');
                        CoCodeMaster rec = new CoCodeMaster()
                        {
                            co_code = Convert.ToDouble(columns[0]),
                            bsecode = columns[1],
                            nsesymbol = columns[2],
                            companyname = columns[3],
                            companyshortname = columns[4],
                            categoryname = columns[5],
                            isin = columns[6],
                            bsegroup = columns[7],
                            mcaptype = columns[8],
                            sectorcode = columns[9],
                            sectorname = columns[10],
                            industrycode = columns[11],
                            industryname = columns[12],
                            bselistedflag = columns[13],
                            nselistedflag = columns[14],
                            DisplayType = columns[15],
                        };
                        coCodeRecords.Add(rec);
                    }
                }
            }
            try
            {
                CommonFunctions.WriteToLogFile("Bse equity master making process started");
                Console.WriteLine("Bse equity master creation processing....");
                List<string> contents1 = new List<string>();
                List<string> contents2 = new List<string>();
                List<string> contents3 = new List<string>();
                DateTime lastTradeDate = new DateTime(2026, 05, 14);
                //this.BSE_CIFILE = new Hashtable();
                //string str1 = lastTradeDate.ToString("ddMMyy");
                string str1 = lastTradeDate.ToString("ddMMyy");
                string path1 = securityFileName;
                //string path2;
                string Exch = "";
                if (!File.Exists("BseIndices.txt"))
                {
                    CommonFunctions.WriteToLogFile("BseIndices.txt File is not available in current folder");
                    return new StatusMessage { message = "BseIndices.txt File is not available in current folder !!!", status = false };
                }
                else if (!File.Exists("Bse_AllowTradingExtra.txt"))
                {
                    CommonFunctions.WriteToLogFile("Bse_AllowTradingExtra.txt File is not available in current folder");
                    return new StatusMessage { message = "Bse_AllowTradingExtra.txt File is not available in current folder !!!", status = false };
                }
                else if (!File.Exists("Bse_DoNotAllowTrading.txt"))
                {
                    CommonFunctions.WriteToLogFile("Bse_DoNotAllowTrading.txt File is not available in current folder");
                    return new StatusMessage { message = "Bse_DoNotAllowTrading.txt File is not available in current folder !!!", status = false };
                }
                else if (!File.Exists(path1))
                {
                    CommonFunctions.WriteToLogFile(path1 + " File is not available in current folder");
                    return new StatusMessage { message = path1 + "File is not available in current folder !!!", status = false };
                }
                else
                {
                    if (File.Exists("BseEquityMasterNew.dat"))
                        File.Delete("BseEquityMasterNew.dat");
                    
                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand("drop table if exists BseEquityMaster", SQLiteHelper.Master_dbConnection))
                    {
                        if (SQLiteHelper.Master_dbConnection.State == ConnectionState.Closed)
                        {
                            ((DbConnection)SQLiteHelper.Master_dbConnection).Open();
                        }
                        ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                    }


                    Dictionary<int, bseFilesData> bseDP = new Dictionary<int, bseFilesData>();
                    var DPFileData = File.ReadAllLines(DPFile);
                    for (int i = 1; i < DPFileData.Length; i++)
                    {
                        try
                        {
                            var Line = DPFileData[i].Replace('%', ' ').Trim();
                            var Parts = Line.Split(',');
                            int ExchCode = Convert.ToInt32(Parts[0]);
                            if ((ExchCode < 500000) || (ExchCode >= 600000) &&
                               (ExchCode < 700000 || ExchCode > 799999) &&
                               (ExchCode < 900000 || ExchCode > 999999)) continue;
                            bseFilesData bseDPData = new bseFilesData()
                            {
                                LowerCkt = Convert.ToDecimal(Parts[5]) / 100,
                                UpperCkt = Convert.ToDecimal(Parts[6]) / 100,
                                YearlyHigh = Convert.ToDecimal(Parts[7]) / 100,
                                YearlyLow = Convert.ToDecimal(Parts[8]) / 100,
                                SurveillanceMsg = "",
                                SurveillanceFlag = 'N'
                            };
                            if (DateTime.TryParseExact(Parts[9], "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var YearHDate))
                            {
                                bseDPData.YearlyHighDate = (YearHDate - MainQH.ExchStartDate).Days;
                            }
                            if (DateTime.TryParseExact(Parts[10], "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var YearLDate))
                            {
                                bseDPData.YearlyLowDate = (YearLDate - MainQH.ExchStartDate).Days;
                            }

                            if (!bseDP.ContainsKey(ExchCode))
                                bseDP.Add(ExchCode, bseDPData);
                        }
                        catch (Exception) { }
                    }
                    var REGFileData = File.ReadAllLines(REGFile);
                    for (int i = 1; i < REGFileData.Length; i++)
                    {
                        try
                        {
                            var Line = REGFileData[i].Replace('%', ' ').Trim();
                            var Parts = Line.Split(',');
                            int ExchCode = Convert.ToInt32(Parts[0]);
                            if ((ExchCode < 500000) || (ExchCode >= 600000) &&
                               (ExchCode < 700000 || ExchCode > 799999) &&
                               (ExchCode < 900000 || ExchCode > 999999)) continue;

                            var SurvillanceMsg = CommonFunctNSE.GetREGString(Parts);
                            if (SurvillanceMsg != "")
                            {

                                if (!bseDP.TryGetValue(ExchCode, out var bseDPData))
                                {
                                    bseDPData = new bseFilesData();
                                    bseDPData.SurveillanceMsg = SurvillanceMsg;
                                    bseDPData.SurveillanceFlag = 'Y';
                                    bseDP.Add(ExchCode, bseDPData);
                                }
                                else
                                {
                                    bseDPData.SurveillanceMsg = SurvillanceMsg;
                                    bseDPData.SurveillanceFlag = 'Y';
                                    bseDP[ExchCode] = bseDPData;
                                }


                            }
                        }
                        catch (Exception) { }
                    }


                    string createBseEquityMaster = ConfigHelper.CoCode
                        ? "create table BseEquityMaster ( Exch char, ScripCode int, Name varchar(12), Desc varchar(30),TradingSymbol varchar(30), TickSize byte, Series varchar(2), MinimumLotQty int,ISIN varchar(20), CoCode money, SurveillanceFlag char , SurveillanceMsg varchar(120),Partition varchar(15), UpperCkt money,LowerCkt money,PrevOpen money,PrevHigh money,PrevLow money,PrevClose money,PrevDayClose money,YearlyHigh money,YearlyLow money,YearlyHighDate int,YearlyLowDate int)"
                        : "create table BseEquityMaster ( Exch char, ScripCode int, Name varchar(12), Desc varchar(30),TradingSymbol varchar(30), TickSize byte, Series varchar(2), MinimumLotQty int,ISIN varchar(20),  SurveillanceFlag char , SurveillanceMsg varchar(120),Partition varchar(15), UpperCkt money,LowerCkt money,PrevOpen money,PrevHigh money,PrevLow money,PrevClose money,PrevDayClose money,YearlyHigh money,YearlyLow money,YearlyHighDate int,YearlyLowDate int)";

                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand(createBseEquityMaster, SQLiteHelper.Master_dbConnection))

                    ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                    FileStream fileStream = new FileStream("BseEquityMasterNew.dat", FileMode.Create, FileAccess.Write);
                    string[] strArray1 = File.ReadAllLines("BseIndices.Txt");
                    string[] source1 = File.ReadAllLines("Bse_AllowTradingExtra.Txt");
                    string[] source2 = File.ReadAllLines("Bse_DoNotAllowTrading.txt");
                    string[] strArray2 = File.ReadAllLines(path1);
                    Structure.Scrip_Master_New_Series scripMasterNewSeries = new Structure.Scrip_Master_New_Series();
                    char ch1 = 'N';
                    contents1.Add("ScripCode,ScripId,ScripName,GroupName,SuspStatus,TickSize,MktLot,Allowed,InPreOpen,ISIN,LongName,IssuedCapital,CreditRating,BoardLotQuantity,FaceValue,FreezePercent,BookClosureStartDate,BookClosureEndDate,Dividend,rights,bonus,Interest,ExDate");
                    contents3.Add("ScripCode,ISIN");
                    ScripData_Dict scripData = new ScripData_Dict();
                    var bseEquityMaster = new BseEquityMaster();
                    var IndexDict = CommonFunctNSE.readBhavCopy_BseIndices();
                    CommonFunctNSE.readBhavCopy_EQ(scripData, bhavCopyFileName);

                    using (SQLiteTransaction sqLiteTransaction = SQLiteHelper.Master_dbConnection.BeginTransaction())
                    {
                        using (SQLiteCommand sqLiteCommand = new SQLiteCommand(SQLiteHelper.Master_dbConnection))
                        {
                            SQLiteParameter sqLiteParameter1 = new SQLiteParameter("@Exchange", DbType.String);
                            SQLiteParameter sqLiteParameter2 = new SQLiteParameter("@ExchangeCode", DbType.Int32);
                            SQLiteParameter sqLiteParameter3 = new SQLiteParameter("@ScripName", DbType.String);
                            SQLiteParameter sqLiteParameter4 = new SQLiteParameter("@LongName", DbType.String);
                            SQLiteParameter sqLiteParameter5 = new SQLiteParameter("@TickSize", DbType.Int32);
                            SQLiteParameter sqLiteParameter6 = new SQLiteParameter("@Series", DbType.String);
                            SQLiteParameter sqLiteParameter7 = new SQLiteParameter("@MinimumLotQty", DbType.Int32);
                            SQLiteParameter sqLiteParameter8 = new SQLiteParameter("@TradingSymbol", DbType.String);
                            SQLiteParameter sqLiteParameterISIN = new SQLiteParameter("@ISIN", DbType.String);
                            //SQLiteParameter sqLiteParameterCoCode = new SQLiteParameter("@CoCode", DbType.String);
                            SQLiteParameter sqLiteParameterCoCode = null;
                            if (ConfigHelper.CoCode)
                            {
                                sqLiteParameterCoCode = new SQLiteParameter("@CoCode", DbType.String);
                            }
                            SQLiteParameter sqLiteParameterSurveillanceFlag = new SQLiteParameter("@SurveillanceFlag", DbType.String);
                            SQLiteParameter sqLiteParameterSurveillanceMsg = new SQLiteParameter("@SurveillanceMsg", DbType.String);
                            SQLiteParameter sqLiteParameterPartition = new SQLiteParameter("@Partition", DbType.String);
                            SQLiteParameter sqLiteParameterUpperCkt = new SQLiteParameter("@UpperCkt", DbType.Currency);
                            SQLiteParameter sqLiteParameterLowerCkt = new SQLiteParameter("@LowerCkt", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevOpen = new SQLiteParameter("@PrevOpen", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevHigh = new SQLiteParameter("@PrevHigh", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevLow = new SQLiteParameter("@PrevLow", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevClose = new SQLiteParameter("@PrevClose", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevDayClose = new SQLiteParameter("@PrevDayClose", DbType.Currency);
                            SQLiteParameter sqLiteParameterYearlyHigh = new SQLiteParameter("@YearlyHigh", DbType.Currency);
                            SQLiteParameter sqLiteParameterYearlyLow = new SQLiteParameter("@YearlyLow", DbType.Currency);
                            SQLiteParameter sqLiteParameterYearlyHighDate = new SQLiteParameter("@YearlyHighDate", DbType.Currency);
                            SQLiteParameter sqLiteParameterYearlyLowDate = new SQLiteParameter("@YearlyLowDate", DbType.Currency);

                            string insertQuery = ConfigHelper.CoCode
                                ? "INSERT INTO BseEquityMaster (Exch, ScripCode, Name, Desc,TradingSymbol, TickSize, Series, MinimumLotQty, ISIN,CoCode,  SurveillanceFlag,SurveillanceMsg, Partition, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose,PrevDayClose ,YearlyHigh ,YearlyLow ,YearlyHighDate,YearlyLowDate) VALUES (@Exchange,@ExchangeCode,@ScripName,@LongName,@TradingSymbol,@TickSize,@Series,@MinimumLotQty,@ISIN,@CoCode,@SurveillanceFlag,@SurveillanceMsg,@Partition,@UpperCkt ,@LowerCkt ,@PrevOpen ,@PrevHigh ,@PrevLow ,@PrevClose,@PrevDayClose ,@YearlyHigh ,@YearlyLow ,@YearlyHighDate ,@YearlyLowDate)"
                                : "INSERT INTO BseEquityMaster (Exch, ScripCode, Name, Desc,TradingSymbol, TickSize, Series, MinimumLotQty, ISIN, SurveillanceFlag,SurveillanceMsg, Partition, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose,PrevDayClose ,YearlyHigh ,YearlyLow ,YearlyHighDate,YearlyLowDate) VALUES (@Exchange,@ExchangeCode,@ScripName,@LongName,@TradingSymbol,@TickSize,@Series,@MinimumLotQty,@ISIN,@SurveillanceFlag,@SurveillanceMsg,@Partition,@UpperCkt ,@LowerCkt ,@PrevOpen ,@PrevHigh ,@PrevLow ,@PrevClose,@PrevDayClose ,@YearlyHigh ,@YearlyLow ,@YearlyHighDate ,@YearlyLowDate)";


                            ((DbCommand)sqLiteCommand).CommandText = insertQuery;

                            sqLiteCommand.Parameters.Add(sqLiteParameter1);
                            sqLiteCommand.Parameters.Add(sqLiteParameter2);
                            sqLiteCommand.Parameters.Add(sqLiteParameter3);
                            sqLiteCommand.Parameters.Add(sqLiteParameter4);
                            sqLiteCommand.Parameters.Add(sqLiteParameter5);
                            sqLiteCommand.Parameters.Add(sqLiteParameter6);
                            sqLiteCommand.Parameters.Add(sqLiteParameter7);
                            sqLiteCommand.Parameters.Add(sqLiteParameter8);
                            sqLiteCommand.Parameters.Add(sqLiteParameterSurveillanceFlag);
                            sqLiteCommand.Parameters.Add(sqLiteParameterSurveillanceMsg);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPartition);
                            sqLiteCommand.Parameters.Add(sqLiteParameterISIN);
                            if (ConfigHelper.CoCode) sqLiteCommand.Parameters.Add(sqLiteParameterCoCode);
                            sqLiteCommand.Parameters.Add(sqLiteParameterUpperCkt);
                            sqLiteCommand.Parameters.Add(sqLiteParameterLowerCkt);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevOpen);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevHigh);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevLow);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevClose);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevDayClose);
                            sqLiteCommand.Parameters.Add(sqLiteParameterYearlyHigh);
                            sqLiteCommand.Parameters.Add(sqLiteParameterYearlyLow);
                            sqLiteCommand.Parameters.Add(sqLiteParameterYearlyHighDate);
                            sqLiteCommand.Parameters.Add(sqLiteParameterYearlyLowDate);
                            for (int index = 0; index < strArray1.Length; ++index)
                            {
                                string[] strArray5 = strArray1[index].Split('|');
                                scripMasterNewSeries.Exchange = 'B';
                                scripMasterNewSeries.ExchangeCode = Convert.ToInt32(strArray5[0]);
                                scripMasterNewSeries.NameLength = Convert.ToByte(strArray5[1].Length);
                                string str5 = strArray5[1].PadRight(12, ' ');
                                scripMasterNewSeries.ScripName = str5.ToCharArray();
                                if (scripMasterNewSeries.NameLength > (byte)12)
                                {
                                    scripMasterNewSeries.NameLength = (byte)12;
                                    scripMasterNewSeries.ScripName = str5.Substring(0, 12).ToCharArray();
                                }
                                scripMasterNewSeries.LongLength = Convert.ToByte(strArray5[1].Length);
                                string str6 = strArray5[1].PadRight(30, ' ');
                                scripMasterNewSeries.LongName = str6.ToCharArray();
                                if (scripMasterNewSeries.LongLength < (byte)30)
                                {
                                    scripMasterNewSeries.LongLength = (byte)30;
                                    scripMasterNewSeries.LongName = str6.Substring(0, 30).ToCharArray();
                                }
                                int num1 = 1;
                                scripMasterNewSeries.TickSize = Convert.ToByte(num1);
                                scripMasterNewSeries.SeriesLength = (byte)2;
                                scripMasterNewSeries.Series = "EQ".ToCharArray();
                                scripMasterNewSeries.MinimumLotQty = 1L;
                                scripMasterNewSeries.Reserved1 = 0;
                                scripMasterNewSeries.Reserved2 = ' ';
                                scripMasterNewSeries.Reserved3 = (byte)0;
                                fileStream.Write(MainQH.RawSerialize((object)scripMasterNewSeries), 0, Marshal.SizeOf((object)scripMasterNewSeries));
                                int num2 = 1;
                                DBPendingList.Add("Exec Proc_ScripInfo3 " + "'EQ'" + "," + "'" + scripMasterNewSeries.Exchange + "'" + "," + "'C'"
                                   + "," + (object)scripMasterNewSeries.ExchangeCode + "," + "'" + strArray5[1] + "'" + "," + "'A'" + "," + (object)0 + "," + "'Y'"
                                   + "," + "'EQ'" + "," + (object)0.0 + "," + "'N'" + "," + (object)num2 + "," + (object)1 + "," + "'" + ch1.ToString() + "'" + "," + (object)0 + "," + "'" + (object)0 + "'"
                                   + ",''" + "," + (object)0 + "," + "'N'" + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0
                                   + "," + "'N'" + "," + "'N'" + "," + "'N'" + "," + "'N'" + "," + (object)0 + "," + "'Y'" + "," + (object)scripMasterNewSeries.ExchangeCode
                                   + ",''" + "," + (object)1.00 + "," + (object)1.00 + "," + (object)0 + ",'" + strArray5[1] + "'," + (object)0);
                                contents1.Add(scripMasterNewSeries.ExchangeCode.ToString() + "," + str5.TrimEnd() + "," + str5.TrimEnd() + ",F," + (object)0 + "," + (object)num2 + "," + (object)1 + ",Y," + ch1.ToString() + "," + str6);       //Rutuja 04.05.2024
                                contents3.Add(scripMasterNewSeries.ExchangeCode.ToString() + "," + (object)0);
                                ((DbParameter)sqLiteParameter1).Value = (object)scripMasterNewSeries.Exchange;
                                ((DbParameter)sqLiteParameter2).Value = (object)scripMasterNewSeries.ExchangeCode;
                                ((DbParameter)sqLiteParameter3).Value = (object)new string(scripMasterNewSeries.ScripName).Substring(0, (int)scripMasterNewSeries.NameLength);
                                ((DbParameter)sqLiteParameter4).Value = (object)new string(scripMasterNewSeries.LongName).Substring(0, (int)scripMasterNewSeries.LongLength);
                                ((DbParameter)sqLiteParameter5).Value = (object)scripMasterNewSeries.TickSize;
                                ((DbParameter)sqLiteParameter6).Value = (object)new string(scripMasterNewSeries.Series).Substring(0, (int)scripMasterNewSeries.SeriesLength);
                                ((DbParameter)sqLiteParameter7).Value = (object)scripMasterNewSeries.MinimumLotQty;
                                ((DbParameter)sqLiteParameter8).Value = strArray5[1];

                                if (ConfigHelper.CoCode) sqLiteParameterCoCode.Value = 0;
                                sqLiteParameterISIN.Value = "";
                                sqLiteParameterPartition.Value = "";
                                if (IndexDict.TryGetValue(scripMasterNewSeries.ExchangeCode, out var ScrData))
                                {
                                    sqLiteParameterUpperCkt.Value = 0;
                                    sqLiteParameterLowerCkt.Value = 0;
                                    sqLiteParameterPrevOpen.Value = ScrData.PrevOpen;
                                    sqLiteParameterPrevHigh.Value = ScrData.PrevHigh;
                                    sqLiteParameterPrevLow.Value = ScrData.PrevLow;
                                    sqLiteParameterPrevClose.Value = ScrData.PrevClose;
                                    sqLiteParameterPrevDayClose.Value = ScrData.PrevDayClose;
                                    sqLiteParameterYearlyHigh.Value = 0;
                                    sqLiteParameterYearlyLow.Value = 0;
                                    sqLiteParameterYearlyHighDate.Value = 0;
                                    sqLiteParameterYearlyLowDate.Value = 0;
                                    sqLiteParameterSurveillanceFlag.Value = 'N';
                                    sqLiteParameterSurveillanceMsg.Value = "";
                                }
                                else
                                {

                                    sqLiteParameterUpperCkt.Value = 0;
                                    sqLiteParameterLowerCkt.Value = 0;
                                    sqLiteParameterPrevOpen.Value = 0;
                                    sqLiteParameterPrevHigh.Value = 0;
                                    sqLiteParameterPrevLow.Value = 0;
                                    sqLiteParameterPrevClose.Value = 0;
                                    sqLiteParameterPrevDayClose.Value = 0;
                                    sqLiteParameterYearlyHigh.Value = 0;
                                    sqLiteParameterYearlyLow.Value = 0;
                                    sqLiteParameterYearlyHighDate.Value = 0;
                                    sqLiteParameterYearlyLowDate.Value = 0;
                                    sqLiteParameterSurveillanceFlag.Value = 'N';
                                    sqLiteParameterSurveillanceMsg.Value = "";
                                }

                                var entry = new EquityMasterEntry
                                {
                                    ScripCode = scripMasterNewSeries.ExchangeCode,
                                    Name = new string(scripMasterNewSeries.ScripName).Substring(0, (int)scripMasterNewSeries.NameLength),
                                    LongName = new string(scripMasterNewSeries.LongName).Substring(0, (int)scripMasterNewSeries.LongLength),
                                    Series = new string(scripMasterNewSeries.Series).Substring(0, (int)scripMasterNewSeries.SeriesLength),
                                    TickSize = Convert.ToInt32(scripMasterNewSeries.TickSize),
                                    MinimumLotQty = (int)scripMasterNewSeries.MinimumLotQty,
                                    Isin = "",
                                    UpperCkt = Convert.ToInt64(Convert.ToDecimal(sqLiteParameterUpperCkt.Value) * 100),
                                    LowerCkt = Convert.ToInt64(Convert.ToDecimal(sqLiteParameterLowerCkt.Value) * 100),
                                    PrevClose = Convert.ToInt64(Convert.ToDecimal(sqLiteParameterPrevClose.Value) * 100)
                                };

                                protoRecordsBSE.Add(entry);
                                ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                            }
                            char TT;
                            for (int index = 1; index < strArray2.Length; index++)
                            {
                               
                                scripMasterNewSeries.Exchange = 'B';
                                string[] strArrayBSE = strArray2[index].Split(',');
                                //
                                scripMasterNewSeries.Exchange = 'B';
                                if (strArrayBSE.Length >= 25)
                                {
                                    if (strArrayBSE[54] != "BSE") continue;

                                    int int32_1 = Convert.ToInt32(strArrayBSE[0].Trim());
                                    string str7 = strArrayBSE[3].Trim();
                                    string str8 = strArrayBSE[1].Trim();
                                    if (str8.EndsWith("#"))
                                    {
                                        continue;
                                    }

                                    string str9 = strArrayBSE[2].Trim();
                                    char ch2 = Convert.ToChar(strArrayBSE[59].Trim());
                                    int int32_2 = Convert.ToInt32(strArrayBSE[10].Trim());
                                    int int32_3 = Convert.ToInt32(strArrayBSE[5].Trim());
                                    string ISIN = strArrayBSE[4].Trim();
                                    long int64 = Convert.ToInt64(strArrayBSE[6]);

                                    if (int64 != 0L)
                                        int64 /= 100L;
                                    bool flag = false;
                                    if ((int32_1 < 500000 || int32_1 > 599999) && (int32_1 < 700000 || int32_1 > 799999) && (int32_1 < 900000 || int32_1 > 999999) && (int32_1 < 800000 || int32_1 > 899999))
                                    {
                                        if (((IEnumerable<string>)source1).Contains<string>(int32_1.ToString()))
                                            flag = true;
                                        else
                                            continue;
                                    }
                                    int int32_4 = Convert.ToInt32(Convert.ToSingle(strArrayBSE[57]) * 100f);
                                    scripMasterNewSeries.TickSize = Convert.ToByte(int32_4);
                                    char ch3 = str9[0] == 'T' || str9.Length > 1 && str9[0] == 'T' && Convert.ToChar(str9[1]) == 'S' || str9.Length > 1 && str9[0] == 'A' && Convert.ToChar(str9[1]) == 'T' || str9.Length > 1 && str9[0] == 'B' && Convert.ToChar(str9[1]) == 'T' || str9.Length > 1 && str9[0] == 'M' && Convert.ToChar(str9[1]) == 'T' || str9.Length > 1 && str9[0] == 'X' && Convert.ToChar(str9[1]) == 'T' ? 'Y' : 'N';
                                    if (str9[0] == 'T' ||
                                       (str9.Length > 1 && str9[0] == 'T' && Convert.ToChar(str9[1]) == 'S') ||
                                       (str9.Length > 1 && str9[0] == 'A' && Convert.ToChar(str9[1]) == 'T') ||
                                       (str9.Length > 1 && str9[0] == 'B' && Convert.ToChar(str9[1]) == 'T') ||
                                       (str9.Length > 1 && str9[0] == 'M' && Convert.ToChar(str9[1]) == 'T') ||
                                       (str9.Length > 1 && str9[0] == 'X' && Convert.ToChar(str9[1]) == 'T') ||//For XT group 05.11.15
                                           (str9[0] == 'Z' || str9[0] == 'P' || str9[0] == 'R')) //Rutuja 25.09.2024
                                    {
                                        TT = 'Y';
                                    }
                                    else if (str9 == "FC" && Exch == "BSE" || str9 == "F" && Exch == "BSE" || str9 == "ZP" && Exch == "BSE" ||
                                             str9 == "Z" && Exch == "BSE" || str9 == "ZP" && Exch == "BSE" || str9 == "F" && Exch == "BSE" ||
                                             str9 == "XT" && Exch == "BSE" || str9 == "BZ" && Exch == "BSE" || str9 == "BO" && Exch == "BSE" ||
                                             str9 == "TO" && Exch == "BSE" || str9 == "DS" && Exch == "BSE" || str9 == "T2" && Exch == "BSE" ||
                                             str9 == "TT" && Exch == "BSE" || str9 == "ZY" && Exch == "BSE" || str9 == "T" && Exch == "BSE" ||
                                             str9 == "MT" && Exch == "BSE" || str9 == "MS" && Exch == "BSE" || str9 == "TS" && Exch == "BSE")
                                    {
                                        TT = 'Y';
                                    }
                                    else
                                    {
                                        TT = 'N';
                                    }
                                    if (flag)
                                        ch3 = 'N';
                                    scripMasterNewSeries.ExchangeCode = int32_1;
                                    scripMasterNewSeries.ScripName = str8.PadRight(12).ToCharArray();
                                    scripMasterNewSeries.NameLength = (byte)str8.Length;
                                    string str11 = strArrayBSE[3].PadRight(30, ' ');
                                    scripMasterNewSeries.LongName = str11.PadRight(30).ToCharArray();
                                    scripMasterNewSeries.LongLength = (byte)str11.Length;
                                    if (scripMasterNewSeries.LongName != null)
                                    {
                                        if (scripMasterNewSeries.NameLength < (byte)30 && scripMasterNewSeries.NameLength > (byte)0)
                                        {
                                            scripMasterNewSeries.LongLength = (byte)30;
                                            scripMasterNewSeries.LongName = str11.Substring(0, 30).ToCharArray();
                                        }
                                    }
                                    else if (scripMasterNewSeries.LongLength < (byte)30 && scripMasterNewSeries.NameLength > (byte)0)
                                    {
                                        scripMasterNewSeries.LongLength = (byte)12;
                                        scripMasterNewSeries.LongName = scripMasterNewSeries.ScripName;
                                    }
                                    char ch4 = 'Y';
                                    if (ch2 == 'S')
                                        ch4 = 'N';
                                    if (((IEnumerable<string>)source2).Contains<string>(int32_1.ToString()))
                                        ch4 = 'N';
                                    ch1 = int32_2 != 90 ? 'N' : 'Y';
                                    string str12;
                                    Decimal num3 = 0M;
                                    str12 = str8;

                                    int GSMFlag = Convert.ToInt32(strArrayBSE[9]);
                                    //if (str12 == "")
                                    str12 = str8;
                                    if (str12.Contains("'"))
                                        str12 = str12.Replace("'", "''");
                                    long num5;
                                    long num6;
                                    if (strArrayBSE[11] != "" || strArrayBSE[12] != "")
                                    {
                                        DateTime dtStart, dt1;
                                        dtStart = Convert.ToDateTime((strArrayBSE[11]));
                                        TimeSpan timeSpan1 = Convert.ToDateTime(strArrayBSE[11]) - new DateTime(1980, 1, 1);
                                        dt1 = Convert.ToDateTime((strArrayBSE[12]));
                                        TimeSpan timeSpan2 = Convert.ToDateTime(strArrayBSE[12]) - new DateTime(1980, 1, 1);
                                        num5 = (long)timeSpan1.TotalSeconds;
                                        num6 = (long)timeSpan1.TotalSeconds;
                                    }
                                    else
                                    {
                                        num5 = 0L;
                                        num6 = 0L;
                                    }
                                    if ((str9.StartsWith("B") || str9.StartsWith("P") || str9 == "ZP" || str9 == "XC" || str9 == "XD" || str9 == "SS" || str9 == "ST") && GSMFlag == 0)
                                        GSMFlag = 101;
                                    string str13 = str9;
                                    scripMasterNewSeries.SeriesLength = (byte)str13.Length;
                                    scripMasterNewSeries.Series = str13.PadRight(2, ' ').ToCharArray();
                                    scripMasterNewSeries.MinimumLotQty = (long)int32_3;
                                    scripMasterNewSeries.Reserved1 = 0;
                                    scripMasterNewSeries.Reserved2 = ' ';
                                    scripMasterNewSeries.Reserved3 = (byte)0;
                                    DBPendingList.Add("Exec Proc_ScripInfo3 " + "'EQ'" + "," + "'" + scripMasterNewSeries.Exchange + "'" + "," + "'C'"
                                     + "," + (object)scripMasterNewSeries.ExchangeCode + "," + "'" + str8
                                     + "'" + "," + "'" + str13 + "'" + "," + (object)0 + "," + "'" + ch4.ToString() + "'" + "," + "'EQ'" + "," + (object)0.0
                                     + "," + "'" + TT + "'" + "," + (object)int32_4 + "," + (object)int32_3 + "," + "'" + ch1.ToString() + "'" + "," + (object)0
                                     + "," + "'" + ISIN + "'" + "," + "'" + str12 + "'" + "," + (object)num3
                                     + "," + "'" + "N" + "'" + "," + (object)0 + "," + (object)int64 + "," + (object)0 + "," + (object)num5
                                     + "," + (object)num6 + "," + "'" + "N" + "'" + "," + "'" + "N" + "'"
                                     + "," + "'" + "N" + "'" + "," + "'" + "N" + "'" + "," + (object)0 + "," + "'" + ch4.ToString() + "'" + "," + (object)scripMasterNewSeries.ExchangeCode
                                     + ",''" + "," + (object)1.0 + "," + (object)1.0 + "," + (object)0 + ",'" + str8 + "'," + (object)0 + "," + (object)GSMFlag);                         
                                    if (ch4 == 'N') continue;
                                    {
                                        fileStream.Write(MainQH.RawSerialize((object)scripMasterNewSeries), 0, Marshal.SizeOf((object)scripMasterNewSeries));
                                        contents1.Add(int32_1.ToString() + "," + str8 + "," + str7 + "," + str9 + "," + ch2.ToString() + "," + (object)int32_4 + "," + (object)int32_3 + "," + ch4.ToString() + "," + ch1.ToString() + "," + ISIN + "," + str11 + "," + (object)num3 + ",Null," + (object)0 + "," + (object)int64 + "," + (object)0 + "," + (object)num5 + "," + (object)num6 + ",N,N,N,N," + (object)0);
                                        contents3.Add(int32_1.ToString() + "," + ISIN);
                                        if (ch3 == 'Y')
                                            contents2.Add("B," + (object)int32_1);
                                        ((DbParameter)sqLiteParameter1).Value = (object)scripMasterNewSeries.Exchange;
                                        ((DbParameter)sqLiteParameter2).Value = (object)scripMasterNewSeries.ExchangeCode;
                                        ((DbParameter)sqLiteParameter3).Value = (object)new string(scripMasterNewSeries.ScripName).Substring(0, (int)scripMasterNewSeries.NameLength);
                                        ((DbParameter)sqLiteParameter4).Value = (object)new string(scripMasterNewSeries.LongName).Substring(0, (int)scripMasterNewSeries.LongLength);
                                        ((DbParameter)sqLiteParameter5).Value = (object)scripMasterNewSeries.TickSize;
                                        ((DbParameter)sqLiteParameter6).Value = (object)new string(scripMasterNewSeries.Series).Substring(0, (int)scripMasterNewSeries.SeriesLength);
                                        ((DbParameter)sqLiteParameter7).Value = (object)scripMasterNewSeries.MinimumLotQty;
                                        ((DbParameter)sqLiteParameter8).Value = str8;
                                        string Partition = strArrayBSE[55];
                                        sqLiteParameterPartition.Value = Partition;
  
                                        var scripRec = scripData.GetRecord(new string(scripMasterNewSeries.ScripName).Substring(0, (int)scripMasterNewSeries.NameLength), new string(scripMasterNewSeries.Series).Substring(0, (int)scripMasterNewSeries.SeriesLength));

                                        sqLiteParameterISIN.Value = ISIN;
                                        if (ConfigHelper.CoCode)//checkCoCode.Checked
                                        {
                                            CoCodeMaster coCodeRecord = coCodeRecords.SingleOrDefault(x => x.isin == ISIN);
                                            if (coCodeRecord != null)
                                            {
                                                sqLiteParameterCoCode.Value = coCodeRecord.co_code;
                                            }
                                            else
                                            {
                                                sqLiteParameterCoCode.Value = 0;
                                            }
                                        }
                                        if (scripRec != null)
                                        {
                                            sqLiteParameterPrevOpen.Value = scripRec.PrevOpen;
                                            sqLiteParameterPrevHigh.Value = scripRec.PrevHigh;
                                            sqLiteParameterPrevLow.Value = scripRec.PrevLow;
                                            sqLiteParameterPrevClose.Value = scripRec.PrevClose;
                                            sqLiteParameterPrevDayClose.Value = scripRec.PrevDayClose;
                                        }
                                        else
                                        {
                                            sqLiteParameterPrevOpen.Value = 0;
                                            sqLiteParameterPrevHigh.Value = 0;
                                            sqLiteParameterPrevLow.Value = 0;
                                            sqLiteParameterPrevClose.Value = 0;
                                            sqLiteParameterPrevDayClose.Value = 0;
                                        }
                                        if (bseDP.TryGetValue(scripMasterNewSeries.ExchangeCode, out var bseDPData))
                                        {
                                            sqLiteParameterUpperCkt.Value = bseDPData.UpperCkt;
                                            sqLiteParameterLowerCkt.Value = bseDPData.LowerCkt;
                                            sqLiteParameterYearlyHigh.Value = bseDPData.YearlyHigh;
                                            sqLiteParameterYearlyLow.Value = bseDPData.YearlyLow;
                                            sqLiteParameterYearlyHighDate.Value = bseDPData.YearlyHighDate;
                                            sqLiteParameterYearlyLowDate.Value = bseDPData.YearlyLowDate;
                                            sqLiteParameterSurveillanceFlag.Value = bseDPData.SurveillanceFlag;
                                            sqLiteParameterSurveillanceMsg.Value = bseDPData.SurveillanceMsg;
                                        }
                                        else
                                        {
                                            sqLiteParameterUpperCkt.Value = 0;
                                            sqLiteParameterLowerCkt.Value = 0;
                                            sqLiteParameterYearlyHigh.Value = 0;
                                            sqLiteParameterYearlyLow.Value = 0;
                                            sqLiteParameterYearlyHighDate.Value = 0;
                                            sqLiteParameterYearlyLowDate.Value = 0;
                                            sqLiteParameterSurveillanceFlag.Value = 'N';
                                            sqLiteParameterSurveillanceMsg.Value = "";
                                        }

                                        var entry = new EquityMasterEntry
                                        {
                                            ScripCode = scripMasterNewSeries.ExchangeCode,
                                            Name = new string(scripMasterNewSeries.ScripName).Substring(0, (int)scripMasterNewSeries.NameLength),
                                            LongName = new string(scripMasterNewSeries.LongName).Substring(0, (int)scripMasterNewSeries.LongLength),
                                            Series = new string(scripMasterNewSeries.Series).Substring(0, (int)scripMasterNewSeries.SeriesLength),
                                            TickSize = Convert.ToInt32(scripMasterNewSeries.TickSize),
                                            MinimumLotQty = (int)scripMasterNewSeries.MinimumLotQty,
                                            Isin = ISIN,
                                            UpperCkt = Convert.ToInt64(Convert.ToDecimal(sqLiteParameterUpperCkt.Value) * 100),
                                            LowerCkt = Convert.ToInt64(Convert.ToDecimal(sqLiteParameterLowerCkt.Value) * 100),
                                            PrevClose = Convert.ToInt64(Convert.ToDecimal(sqLiteParameterPrevClose.Value) * 100)
                                        };

                                        protoRecordsBSE.Add(entry);
                                        ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        ((DbTransaction)sqLiteTransaction).Commit();
                    }
                    File.AppendAllLines(this.T2TFile, (IEnumerable<string>)contents2);
                    fileStream.Close();
                    ((DbConnection)SQLiteHelper.Master_dbConnection).Close();
                    
                    ProtoMasterWriter.CreateProtoMasterFile(protoRecordsBSE, Path.Combine(MainQH.protoFolder, "BseEquityMaster.bin"));
                    var records = ProtoMasterReader.Read(Path.Combine(MainQH.protoFolder, "BseEquityMaster.bin"), EquityMasterEntry.Parser);
                    Console.WriteLine(records.Count());




                    CommonFunctions.WriteToLogFile("BseEquityMasterNew created.");
                    string outMsg = "BseEquityMasterNew.csv - " + contents1.Count + " Records \r\n";
                    return new StatusMessage { message = "BSE Equity : " + contents1.Count + " Records Processed", status = true };
                }
            }
            catch (Exception ex)
            {
                CommonFunctions.ErrorLog(ex);
                return new StatusMessage { message = ex.Message, status = false };
            }
        }

    }
}
