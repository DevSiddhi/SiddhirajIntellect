using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using ConsoleApp1.QH;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Processes
{
    public class NseSecuritySegmentProcess
    {
        public static List<CoCodeMaster> coCodeRecords = new List<CoCodeMaster>();
        public static List<string> DBPendingList = new List<string>();
        public static string T2TFile = "T2TRpt" + DateTime.Today.ToString("yyyyMMdd") + ".Csv";
        public static void AccessControl(string message)
        {
            Console.WriteLine(message);
            //CommonFunctions.WriteToLogFile(message);
        }
        public StatusMessage ExecuteNseSecurity(bool CheckFile = false)
        {
            bool newFormat = true;
            StatusMessage res = new StatusMessage();
            SQLiteHelper.OpenSqlite();
            if (newFormat)
                res = ExecuteNseSecurity_new(CheckFile);
            //else
            //    res = ExecuteNseSecurity_old(progressBar, CheckFile);


            //string fields = checkCoCode.Checked
            //    ? "Exch, ScripCode, Name, Desc, TradingSymbol, TickSize, Series, MinimumLotQty, ISIN, CoCode, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose ,YearlyHigh ,YearlyLow ,SurveillanceFlag"
            //    : "Exch, ScripCode, Name, Desc, TradingSymbol, TickSize, Series, MinimumLotQty, ISIN, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose ,YearlyHigh ,YearlyLow ,SurveillanceFlag";

            string fields = "Exch, ScripCode, Name, Desc, TradingSymbol, TickSize, Series, MinimumLotQty, ISIN, CoCode, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose ,YearlyHigh ,YearlyLow ,SurveillanceFlag";
            if (res.status) SQLiteHelper.CopySQLiteTabletoChild("NseEquityMaster", fields);
            CommonFunctions.ExportToCsv("child.NseEquityMaster", "NseEquityMaster.csv");   //Rutuja 14.10.2024
            return res;

        }
        public static StatusMessage ExecuteNseSecurity_new(bool CheckFile = false)
        {

            int RecCount = 0;
            string outputMessage = "";
            DateTime Date = new DateTime(2026, 05, 12);

            string securityFileName = $"NSE_CM_security_{Date:ddMMyyyy}.csv";
            string NseCMBhavCopyFileName = $"BhavCopy_NSE_CM_0_0_0_{Date:yyyyMMdd}_F_0000.csv";
            string REGFile = $"NSE_REG1_IND{Date:ddMMyy}.csv";
            string FoSecBan = $"fo_secban_{DateTime.Today:ddMMyyyy}.csv";
            string smeFileName = $"sme{Date:ddMMyyyy}.csv";

            string coCodeFileName = $"co_code.csv";

            if (CheckFile)
            {
                if (!File.Exists(securityFileName)) outputMessage += $"{securityFileName},";
                if (!File.Exists(NseCMBhavCopyFileName)) outputMessage += $"{NseCMBhavCopyFileName},";
                if (!File.Exists(REGFile)) outputMessage += $"{REGFile},";
                if (!File.Exists(FoSecBan)) outputMessage += $"{FoSecBan},";
                if (!File.Exists("Nse_DoNotAllowTrading.txt")) outputMessage += "Nse_DoNotAllowTrading.txt,";
                if (!File.Exists("Nse_AllowTradingExtra.txt")) outputMessage += "Nse_AllowTradingExtra.txt,";
                if (!File.Exists("NseIndices.txt")) outputMessage += "NseIndices.txt,";
                if (!File.Exists("cm_contract_stream_info.csv")) outputMessage += "cm_contract_stream_info.csv,";
                return new StatusMessage { message = outputMessage };
            }
            if (true) //checkCoCode.Checked
            {
                try
                {
                    Task.Run(() => CommonFunctNSE.Download_CoCode()).Wait();
                }
                catch (Exception ex)
                {
                    ex.ToString();
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
                try
                {
                    CommonFunctions.WriteToLogFile("Conv Nse Security.txt button clicked (standardized Format)");
                    //if (this.ProcessStatus())
                    //    return new StatusMessage { message = "Startup Pending", status = false };
                    List<string> contents1 = new List<string>();
                    List<string> contents2 = new List<string>();
                    List<string> contents3 = new List<string>();
                    string str1, str2;
                    string[] source1 = new string[3] { "1", "2", "3" };
                    int index1 = 1, SecurityStatusNormalMkt, EligibilityNormalMkt, SecurityStatusAdditionalMkt, EligibilityAdditionalMkt; //Added by Rhythm 09.02.2024 For New Listing Scrips.
                    int T1Settlement;                                                                                                    //Added by Rutuja 12.07.2024 For New Listing Scrips in Standardized Format.      
                    Structure.Scrip_Master_New_Series structure = new Structure.Scrip_Master_New_Series();
                    int count = Marshal.SizeOf((object)structure);
                    Hashtable hashtable = new Hashtable();
                    if (!File.Exists(securityFileName))
                    {
                        string str3 = $"{securityFileName} File is not available in current folder !!!";
                        CommonFunctions.WriteToLogFile(str3);
                        return new StatusMessage { message = $"{securityFileName} File is not available in current folder !!!", status = false };
                    }
                    else if (!File.Exists("Nse_DoNotAllowTrading.txt"))
                    {
                        string str4 = "Nse_DoNotAllowTrading.txt File is not available in current folder !!!";
                        CommonFunctions.WriteToLogFile(str4);
                        return new StatusMessage { message = "Nse_DoNotAllowTrading.txt File is not available in current folder !!!", status = false };
                    }
                    else if (!File.Exists("Nse_AllowTradingExtra.txt"))
                    {
                        string str5 = "Nse_AllowTradingExtra.txt File is not available in current folder !!!";
                        CommonFunctions.WriteToLogFile(str5);
                        return new StatusMessage { message = "Nse_AllowTradingExtra.txt File is not available in current folder !!!", status = false };
                    }
                    else if (!File.Exists("NseIndices.txt"))
                    {
                        string str6 = "NseIndices.txt File is not available in current folder !!!";
                        CommonFunctions.WriteToLogFile(str6);
                        return new StatusMessage { message = "NseIndices.txt File is not available in current folder !!!", status = false };
                    }
                    else
                    {
                        bool flag1;
                        if (!File.Exists("cm_contract_stream_info.csv"))
                        {
                            string str7 = "cm_contract_stream_info.csv File is not available in current folder !!!";
                            CommonFunctions.WriteToLogFile(str7);
                            flag1 = false;
                            return new StatusMessage { message = "cm_contract_stream_info.csv File is not available in current folder !!!", status = false };
                        }
                        else
                            flag1 = true;
                        if (File.Exists("NseEquityMasterNew.dat"))
                            File.Delete("NseEquityMasterNew.dat");
                        //if (MorningProcess == true)
                        //{
                        //    ((DbConnection)SQLiteHelper.Master_dbConnection).Open();
                        //}
                        using (SQLiteCommand sqLiteCommand = new SQLiteCommand("drop table if exists NseEquityMaster", SQLiteHelper.Master_dbConnection))
                        {
                            if (SQLiteHelper.Master_dbConnection.State == ConnectionState.Closed)
                            {
                                ((DbConnection)SQLiteHelper.Master_dbConnection).Open();
                            }
                            ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                        }

                        ScripData_Dict scripData = new ScripData_Dict();
                        CommonFunctNSE.readBhavCopy_NseIndices(scripData);
                        CommonFunctNSE.readBhavCopy_EQ(scripData, NseCMBhavCopyFileName);
                        CommonFunctNSE.readBhavCopy_sme(scripData, smeFileName);
                        CommonFunctNSE.ReadNse52HLAdjusted(scripData, false);

                        string[] FOSecBanList;

                        if (File.Exists(FoSecBan))
                            FOSecBanList = File.ReadAllLines(FoSecBan).Skip(1).Select(n => n.Split(',')[1]).ToArray();
                        else
                            FOSecBanList = new string[0];

                        Dictionary<ScripDataForDictionary, int> bseDP = new Dictionary<ScripDataForDictionary, int>();
                        var REGFileData = File.ReadAllLines(REGFile);
                        //if (numMinNSEREG.Value > 0 && REGFileData.Length < numMinNSEREG.Value)
                        //{
                        //    Str = REGFile + $" falls below Expected Range. \r\nExpected Minimum value : {numMinNSEREG.Value}, Current : {REGFileData.Length}. Verify Before Proceeding.";
                        //    MessageBox.Show(Str);
                        //    WriteToLogFile(Str);
                        //}
                        ScripData_Dict scripDataNew = new ScripData_Dict();
                        for (int i = 1; i < REGFileData.Length; i++)
                        {
                            try
                            {
                                var Line = REGFileData[i].Replace('%', ' ').Trim();
                                var Parts = Line.Split(',');


                                string Symbol = Parts[1];
                                string Series = Parts[4];

                                var scripRec = scripData.GetRecord(Symbol, Series);
                                if (scripRec == null)
                                {
                                    scripRec = new ScripData
                                    {
                                        Series = Series,
                                        Name = Symbol
                                    };

                                    scripData.addEntry(scripRec);
                                }

                                var SurvillanceMsg = CommonFunctNSE.GetREGString(Parts, FOSecBanList.Contains(Symbol));
                                if (SurvillanceMsg != "")
                                {
                                    scripRec.SurveillanceMsg = SurvillanceMsg;
                                    scripRec.SurveillanceFlag = 'Y';

                                }
                            }
                            catch (Exception) { }
                        }
                        //string createNseEquityMaster = checkCoCode.Checked ? "create table NseEquityMaster (Exch char, ScripCode int, Name varchar(12), Desc varchar(30), TradingSymbol varchar(30), TickSize int, Series varchar(2), MinimumLotQty int,ISIN varchar(20), CoCode money, SurveillanceFlag char , SurveillanceMsg varchar(120) ," +
                        //"UpperCkt money,LowerCkt money,PrevOpen money,PrevHigh money,PrevLow money,PrevClose money,PrevDayClose money,YearlyHigh money,YearlyLow money,YearlyHighDate int,YearlyLowDate int)" : "create table NseEquityMaster (Exch char, ScripCode int, Name varchar(12), Desc varchar(30), TradingSymbol varchar(30), TickSize int, Series varchar(2), MinimumLotQty int,ISIN varchar(20), GSMFlag int, SurveillanceFlag char , SurveillanceMsg varchar(120)," +
                        //"UpperCkt money,LowerCkt money,PrevOpen money,PrevHigh money,PrevLow money,PrevClose money,PrevDayClose money,YearlyHigh money,YearlyLow money,YearlyHighDate int,YearlyLowDate int)";

                        string createNseEquityMaster =  "create table NseEquityMaster (Exch char, ScripCode int, Name varchar(12), Desc varchar(30), TradingSymbol varchar(30), TickSize int, Series varchar(2), MinimumLotQty int,ISIN varchar(20), CoCode money, SurveillanceFlag char , SurveillanceMsg varchar(120) ," + "UpperCkt money,LowerCkt money,PrevOpen money,PrevHigh money,PrevLow money,PrevClose money,PrevDayClose money,YearlyHigh money,YearlyLow money,YearlyHighDate int,YearlyLowDate int)" ;

                        using (SQLiteCommand sqLiteCommand = new SQLiteCommand(createNseEquityMaster, SQLiteHelper.Master_dbConnection)) ((DbCommand)sqLiteCommand).ExecuteNonQuery();

                        // this.Button_NseSecurity.Enabled = false;

                        FileStream fileStream = new FileStream("NseEquityMasterNew.dat", FileMode.Create, FileAccess.Write);
                        string[] source2 = File.ReadAllLines("Nse_DoNotAllowTrading.txt");
                        string[] source3 = File.ReadAllLines("Nse_AllowTradingExtra.txt");
                        string[] strArray1;
                        if (flag1)
                            strArray1 = File.ReadAllLines("cm_contract_stream_info.csv");
                        else
                            strArray1 = new string[2] { "0", "1" };
                        string[] source4;
                        if (File.Exists("Nse_AllowTradingExtraByExchange.txt"))
                        {
                            source4 = File.ReadAllLines("Nse_AllowTradingExtraByExchange.txt");
                            CommonFunctions.WriteToLogFile("Nse_AllowTradingExtraByExchange.txt file reading completed");
                        }
                        else
                            source4 = (string[])null;
                        char ch1 = 'N';
                        contents1.Add("Token,Symbol,Series,ScName,TickSize,LowerCkt,UpperCkt,Allowed,InPreOpen,FreezeQty,ISIN,Name,Issued Capital,Credit Rating,BoardLotQnty,FaceValue,FreezePercent,BookClosureStartDate,BookCloserEndDate,Dividend,Rights,Bonus,Interest,ExDate,LongName");
                        contents3.Add("Token,ISIN");
                        string[] strArray2 = File.ReadAllLines("NseIndices.txt");

                        //if (numMinNSEIndices.Value > 0 && strArray2.Length < numMinNSEIndices.Value)
                        //{
                        //    Str = $"NseIndices.txt falls below Expected Range. \r\nExpected Minimum value : {numMinNSEIndices.Value}, Current : {strArray2.Length}. Verify Before Proceeding.";
                        //    MessageBox.Show(Str);
                        //    WriteToLogFile(Str);
                        //}
                        Structure.Scrip_Master_New_Series[] array = new Structure.Scrip_Master_New_Series[strArray2.Length];
                        using (SQLiteTransaction sqLiteTransaction = SQLiteHelper.Master_dbConnection.BeginTransaction())
                        {
                            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(SQLiteHelper.Master_dbConnection))
                            {
                                SQLiteParameter sqLiteParameter1 = new SQLiteParameter("@Exchange", DbType.String);
                                SQLiteParameter sqLiteParameter2 = new SQLiteParameter("@ExchangeCode", DbType.Int32);
                                SQLiteParameter sqLiteParameter3 = new SQLiteParameter("@ScripName", DbType.String);
                                SQLiteParameter sqLiteParameter4 = new SQLiteParameter("@LongName", DbType.String);
                                SQLiteParameter sqLiteParameter5 = new SQLiteParameter("@TickSize", DbType.Int16);
                                SQLiteParameter sqLiteParameter6 = new SQLiteParameter("@Series", DbType.String);
                                SQLiteParameter sqLiteParameter7 = new SQLiteParameter("@MinimumLotQty", DbType.Int32);
                                SQLiteParameter sqLiteParameter8 = new SQLiteParameter("@TradingSymbol", DbType.String);
                                SQLiteParameter sqLiteParameterISIN = new SQLiteParameter("@ISIN", DbType.String);
                                SQLiteParameter sqLiteParameterCoCode = null;
                                //if (checkCoCode.Checked) sqLiteParameterCoCode = new SQLiteParameter("@CoCode", DbType.Currency);
                                sqLiteParameterCoCode = new SQLiteParameter("@CoCode", DbType.Currency);
                                SQLiteParameter sqLiteParameterSurveillanceFlag = new SQLiteParameter("@SurveillanceFlag", DbType.String);
                                SQLiteParameter sqLiteParameterSurveillanceMsg = new SQLiteParameter("@SurveillanceMsg", DbType.String);
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
                                //((DbCommand)sqLiteCommand).CommandText =
                                //    checkCoCode.Checked ?
                                //    "INSERT INTO NseEquityMaster (Exch, ScripCode, Name, Desc, TradingSymbol, TickSize, Series, MinimumLotQty, ISIN, CoCode,  SurveillanceFlag, SurveillanceMsg, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose ,PrevDayClose,YearlyHigh ,YearlyLow ,YearlyHighDate,YearlyLowDate) VALUES (@Exchange,@ExchangeCode,@ScripName,@LongName,@TradingSymbol,@TickSize,@Series,@MinimumLotQty,@ISIN,@CoCode, @SurveillanceFlag ,@SurveillanceMsg ,@UpperCkt ,@LowerCkt ,@PrevOpen ,@PrevHigh ,@PrevLow ,@PrevClose ,@PrevDayClose,@YearlyHigh ,@YearlyLow ,@YearlyHighDate ,@YearlyLowDate )"
                                //    :
                                //    "INSERT INTO NseEquityMaster (Exch, ScripCode, Name, Desc, TradingSymbol, TickSize, Series, MinimumLotQty, ISIN,   SurveillanceFlag, SurveillanceMsg, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose ,PrevDayClose,YearlyHigh ,YearlyLow ,YearlyHighDate,YearlyLowDate) VALUES (@Exchange,@ExchangeCode,@ScripName,@LongName,@TradingSymbol,@TickSize,@Series,@MinimumLotQty,@ISIN, @SurveillanceFlag ,@SurveillanceMsg ,@UpperCkt ,@LowerCkt ,@PrevOpen ,@PrevHigh ,@PrevLow ,@PrevClose ,@PrevDayClose,@YearlyHigh ,@YearlyLow ,@YearlyHighDate ,@YearlyLowDate )";

                                ((DbCommand)sqLiteCommand).CommandText = "INSERT INTO NseEquityMaster (Exch, ScripCode, Name, Desc, TradingSymbol, TickSize, Series, MinimumLotQty, ISIN, CoCode,  SurveillanceFlag, SurveillanceMsg, UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose ,PrevDayClose,YearlyHigh ,YearlyLow ,YearlyHighDate,YearlyLowDate) VALUES (@Exchange,@ExchangeCode,@ScripName,@LongName,@TradingSymbol,@TickSize,@Series,@MinimumLotQty,@ISIN,@CoCode, @SurveillanceFlag ,@SurveillanceMsg ,@UpperCkt ,@LowerCkt ,@PrevOpen ,@PrevHigh ,@PrevLow ,@PrevClose ,@PrevDayClose,@YearlyHigh ,@YearlyLow ,@YearlyHighDate ,@YearlyLowDate )";
                                sqLiteCommand.Parameters.Add(sqLiteParameter1);
                                sqLiteCommand.Parameters.Add(sqLiteParameter2);
                                sqLiteCommand.Parameters.Add(sqLiteParameter3);
                                sqLiteCommand.Parameters.Add(sqLiteParameter4);
                                sqLiteCommand.Parameters.Add(sqLiteParameter5);
                                sqLiteCommand.Parameters.Add(sqLiteParameter6);
                                sqLiteCommand.Parameters.Add(sqLiteParameter7);
                                sqLiteCommand.Parameters.Add(sqLiteParameter8);
                                sqLiteCommand.Parameters.Add(sqLiteParameter8);
                                sqLiteCommand.Parameters.Add(sqLiteParameterSurveillanceFlag);
                                sqLiteCommand.Parameters.Add(sqLiteParameterSurveillanceMsg);
                                sqLiteCommand.Parameters.Add(sqLiteParameterISIN);
                                //if (checkCoCode.Checked) sqLiteCommand.Parameters.Add(sqLiteParameterCoCode);
                                sqLiteCommand.Parameters.Add(sqLiteParameterCoCode);
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

                                for (int index2 = 0; index2 < strArray2.Length; ++index2)
                                {
                                    string[] strArray3 = strArray2[index2].Split('|');
                                    structure.Exchange = 'N';
                                    structure.ExchangeCode = Convert.ToInt32(strArray3[0]);
                                    structure.NameLength = Convert.ToByte(strArray3[2].Length);
                                    string str8 = strArray3[2].PadRight(12, ' ');
                                    structure.ScripName = str8.ToCharArray();
                                    if (structure.NameLength > (byte)12)
                                    {
                                        structure.NameLength = (byte)12;
                                        structure.ScripName = str8.Substring(0, 12).ToCharArray();
                                    }

                                    structure.LongLength = Convert.ToByte(strArray3[1].Length);
                                    string str9 = strArray3[1].PadRight(30, ' ');
                                    if (str9.ToCharArray() != null)
                                    {
                                        if (Convert.ToByte(strArray3[1].Length) > (byte)0 && Convert.ToByte(strArray3[1].Length) < (byte)30)
                                        {
                                            structure.LongName = str9.ToCharArray();
                                            structure.LongLength = Convert.ToByte(strArray3[1].Length);
                                            structure.LongName = str9.Substring(0, 30).ToCharArray();
                                        }
                                        else
                                        {
                                            structure.LongLength = Convert.ToByte(strArray3[2].Length);
                                            structure.LongName = str8.Substring(0, 12).ToCharArray();
                                        }
                                    }
                                    else
                                    {
                                        structure.LongLength = Convert.ToByte(strArray3[2].Length);
                                        structure.LongName = str8.Substring(0, 12).ToCharArray();
                                    }
                                    structure.TickSize = (byte)1;
                                    structure.SeriesLength = (byte)2;
                                    structure.Series = "EQ".ToCharArray();
                                    structure.MinimumLotQty = 1L;
                                    structure.Reserved1 = 0;
                                    contents1.Add(structure.ExchangeCode.ToString() + "," + strArray3[1] + ",EQ," + strArray3[2] + "," + (object)1 + "," + (object)0 + "," + (object)0 + ",Y," + ch1.ToString() + "," + (object)0 + "," + (object)0 + "," + str9);
                                    DBPendingList.Add("Exec Proc_ScripInfo3 EQ," + structure.Exchange.ToString() + ",C," + (object)structure.ExchangeCode + ",'" + str8 + "',EQ," + (object)0 + ",Y,EQ," + (object)0.0 + ",N," + (object)1 + "," + (object)1 + "," + ch1.ToString() + "," + (object)0 + ",'" + (object)0 + "',''," + (object)0 + ",N," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + ",N,N,N,N," + (object)0 + ",Y," + (object)0 + ",''," + (object)1.0 + "," + (object)1.0 + "," + (object)0 + ",'" + str8 + "'," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0);
                                    fileStream.Write(structure.ToByteArray(), 0, count);
                                    array[index2] = structure;
                                    ++index1;
                                    contents3.Add(structure.ExchangeCode.ToString() + "," + (object)0);

                                    string Symbol = new string(structure.ScripName).Substring(0, (int)structure.NameLength);
                                    string Series = "EQ";
                                    sqLiteParameterISIN.Value = "";
                                    var scripRec = scripData.GetRecord(Symbol, Series);
                                    //if (true) sqLiteParameterCoCode.Value = 0;
                                    if (scripRec != null)
                                    {
                                        sqLiteParameterUpperCkt.Value = 0;
                                        sqLiteParameterLowerCkt.Value = 0;
                                        sqLiteParameterPrevOpen.Value = scripRec.PrevOpen;
                                        sqLiteParameterPrevHigh.Value = scripRec.PrevHigh;
                                        sqLiteParameterPrevLow.Value = scripRec.PrevLow;
                                        sqLiteParameterPrevClose.Value = scripRec.PrevClose;
                                        sqLiteParameterPrevDayClose.Value = scripRec.PrevDayClose;
                                        sqLiteParameterYearlyHigh.Value = scripRec.YearlyHigh;
                                        sqLiteParameterYearlyLow.Value = scripRec.YearlyLow;
                                        sqLiteParameterYearlyHighDate.Value = scripRec.YearlyHighDate;
                                        sqLiteParameterYearlyLowDate.Value = scripRec.YearlyLowDate;
                                        sqLiteParameterSurveillanceFlag.Value = scripRec.SurveillanceFlag;
                                        sqLiteParameterSurveillanceMsg.Value = scripRec.SurveillanceMsg;
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
                                    ((DbParameter)sqLiteParameter1).Value = (object)structure.Exchange;
                                    ((DbParameter)sqLiteParameter2).Value = (object)structure.ExchangeCode;
                                    ((DbParameter)sqLiteParameter3).Value = (object)new string(structure.ScripName).Substring(0, (int)structure.NameLength);
                                    ((DbParameter)sqLiteParameter4).Value = (object)new string(structure.LongName).Substring(0, (int)structure.LongLength);
                                    ((DbParameter)sqLiteParameter5).Value = (object)structure.TickSize;
                                    ((DbParameter)sqLiteParameter6).Value = (object)new string(structure.Series).Substring(0, (int)structure.SeriesLength);
                                    ((DbParameter)sqLiteParameter7).Value = (object)structure.MinimumLotQty;
                                    ((DbParameter)sqLiteParameter8).Value = "";
                                    ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                                }

                                if (flag1)
                                {
                                    for (int index3 = 0; index3 < strArray1.Length; ++index3)
                                    {
                                        string[] strArray4 = strArray1[index3].Split(',');
                                        if (!(strArray4[0] != "C"))
                                            hashtable[(object)strArray4[2]] = (object)strArray4[1];
                                    }
                                }

                                var SecurityArr = File.ReadAllLines(securityFileName);
                                //if (numMinNSESecurity.Value > 0 && SecurityArr.Length < numMinNSESecurity.Value)
                                //{
                                //    Str = securityFileName + $" falls below Expected Range. \r\nExpected Minimum value : {numMinNSESecurity.Value}, Current : {SecurityArr.Length}. Verify Before Proceeding.";
                                //    MessageBox.Show(Str);
                                //    WriteToLogFile(Str);
                                //}
                                foreach (string readAllLine in SecurityArr)
                                {
                                    RecCount++;
                                    char[] chArray = new char[1] { ',' };
                                    string[] strArrayNew = readAllLine.Split(chArray);
                                    if (!int.TryParse(strArrayNew[0], out int ScripCode)) continue;//Skip First Line or Lines with string Data start

                                    if (strArrayNew.Length >= 25)
                                    {
                                        if (strArrayNew[16] == "2") continue;     //Rutuja and Jeffin 27.03.2025
                                        bool flag2 = false;
                                        if (strArrayNew[2] != "EQ" && strArrayNew[2] != "RR" && strArrayNew[2] != "BE" && strArrayNew[2] != "DR" && strArrayNew[2] != "DE" && strArrayNew[2] != "BZ" && strArrayNew[2] != "SM" && strArrayNew[2] != "IV" && strArrayNew[2] != "ID" && strArrayNew[2] != "E1" && strArrayNew[2] != "GS" && strArrayNew[2] != "RE" && strArrayNew[2] != "ST" && !((IEnumerable<string>)source1).Contains<string>(strArrayNew[7]))
                                        {
                                            if (((IEnumerable<string>)source3).Contains<string>(strArrayNew[0]))
                                                flag2 = true;
                                            if (!flag2)
                                                continue;
                                        }
                                        int int32_1 = Convert.ToInt32(strArrayNew[18]);
                                        int int32_2 = Convert.ToInt32(strArrayNew[19]);

                                        SecurityStatusNormalMkt = Convert.ToInt32(strArrayNew[18]);
                                        EligibilityNormalMkt = Convert.ToInt32(strArrayNew[19]);
                                        SecurityStatusAdditionalMkt = Convert.ToInt32(strArrayNew[28]);
                                        EligibilityAdditionalMkt = Convert.ToInt32(strArrayNew[29]);      

                                        T1Settlement = Convert.ToInt32(strArrayNew[41]);

                                        if (source4 != null && ((IEnumerable<string>)source4).Contains<string>(strArrayNew[1]))
                                            strArrayNew[19] = "1";
                                        char ch2 = !(strArrayNew[18].Trim() == "6") ? 'N' : 'Y';
                                        structure.ExchangeCode = Convert.ToInt32(strArrayNew[0]);

                                        if (structure.ExchangeCode == 759587)
                                        {

                                        }
                                        if (structure.ExchangeCode > 0)
                                        {
                                            structure.NameLength = (byte)strArrayNew[1].Length;
                                            string str10 = strArrayNew[1];
                                            if (str10.Contains("NSETEST")) continue;
                                            string s = strArrayNew[17].Trim();
                                            //if (s.Length < 5 && (strArrayNew[2] == "EQ" || strArrayNew[2] == "DR" || strArrayNew[2] == "SM" || strArrayNew[2] == "IV" || strArrayNew[2] == "E1") && strArrayNew[1] == str1)
                                            //    s = str2;
                                            decimal UpperCkt = 0;
                                            decimal LowerCkt = 0;
                                            if (s.Length > 3)
                                            {
                                                string[] strArray6 = CommonFunctNSE.SplitCSV(s, '-');
                                                if (strArray6.Length == 2)
                                                {
                                                    strArray6[0] = strArray6[0].Trim();
                                                    if (strArray6[0].Length > 0)
                                                        LowerCkt = Convert.ToDecimal(strArray6[0]);
                                                    strArray6[1] = strArray6[1].Trim();
                                                    if (strArray6[1].Length > 0)
                                                        UpperCkt = Convert.ToDecimal(strArray6[1]);
                                                }
                                                else
                                                    continue;
                                            }
                                            long int64 = Convert.ToInt64(strArrayNew[6]);
                                            if (int64 != 0L)
                                                int64 /= 100L;
                                            structure.Exchange = 'N';
                                            structure.ExchangeCode = Convert.ToInt32(strArrayNew[0]);
                                            string str11 = str10.PadRight(12, ' ');
                                            structure.ScripName = str11.ToCharArray();
                                            structure.LongLength = (byte)strArrayNew[3].Length;
                                            string str12 = strArrayNew[3].PadRight(30, ' ');
                                            if (str12.ToCharArray() != null)
                                            {
                                                if (Convert.ToByte(strArrayNew[3].Length) > (byte)0 && Convert.ToByte(strArrayNew[3].Length) < (byte)30)
                                                {
                                                    structure.LongName = str12.ToCharArray();
                                                    structure.LongLength = Convert.ToByte(strArrayNew[3].Length);
                                                    structure.LongName = str12.Substring(0, 30).ToCharArray();
                                                }
                                                else
                                                {
                                                    structure.LongLength = Convert.ToByte(strArrayNew[23].Length);
                                                    structure.LongName = str11.Substring(0, 12).ToCharArray();
                                                }
                                            }
                                            else
                                            {
                                                structure.LongLength = Convert.ToByte(strArrayNew[23].Length);
                                                structure.LongName = str11.Substring(0, 12).ToCharArray();
                                            }
                                            structure.TickSize = (byte)Math.Min((ushort)100, Convert.ToUInt16(strArrayNew[8]));
                                            ushort TickSize = Convert.ToUInt16(strArrayNew[8]);
                                            string str13 = strArrayNew[2];
                                            structure.SeriesLength = (byte)str13.Length;
                                            structure.Series = str13.PadRight(2, ' ').ToCharArray();
                                            structure.MinimumLotQty = Convert.ToInt64(strArrayNew[5]);
                                            structure.Reserved1 = 0;
                                            char ch3 = 'Y';
                                            if (((IEnumerable<string>)source2).Contains<string>(strArrayNew[0]))
                                                ch3 = 'N';
                                            char ch4 = !(strArrayNew[2] != "BE") || !(strArrayNew[2] != "BZ") || !(strArrayNew[2] != "ID") || !(strArrayNew[2] != "ST") ? 'Y' : 'N';
                                            if (flag2)
                                                ch4 = 'N';
                                            string str14 = strArrayNew[3];
                                            if (str14.Contains("'"))
                                                str14 = str14.Replace("'", "''");
                                            if (str14.Contains("%"))
                                                str14 = str14.Replace("%", "-");
                                            double num3 = Convert.ToDouble(strArrayNew[15]);
                                            double uint16 = (double)Convert.ToUInt16(strArrayNew[33]);
                                            long num4 = Convert.ToInt64(uint16 / 10000.0 / 100.0 * num3);
                                            if (num4 > (long)int.MaxValue)
                                                num4 = (long)int.MaxValue;
                                            string str15 = strArrayNew[4].Trim();
                                            if (str15 == "")
                                                str15 = "0";
                                            int SSEC = Convert.ToInt32(strArrayNew[10]);
                                            int GSMFlag = Convert.ToInt32(strArrayNew[9]);
                                            int num5 = !flag1 ? 0 : (!hashtable.ContainsKey((object)strArrayNew[0]) ? 0 : Convert.ToInt32(hashtable[(object)strArrayNew[0]].ToString()));
                                            if ((strArrayNew[19] == "0" && strArrayNew[29] == "0") /*|| (strArrayNew[19] == "1" && strArrayNew[29] == "0")*/ /*|| (strArrayNew[19] == "0" && strArrayNew[29] == "1")*/)
                                            {

                                                str1 = strArrayNew[1];
                                                str2 = strArrayNew[17];
                                                ch3 = 'N';


                                                DBPendingList.Add("Exec Proc_ScripInfo3 EQ," + structure.Exchange.ToString() + ",C," + (object)structure.ExchangeCode + ",'" + strArrayNew[1] + "','" + str13 + "'," + (object)0 + "," + ch3.ToString() + ",XX," + (object)0.0 + "," + ch4.ToString() + ",'" + strArrayNew[8] + "'," + (object)1 + "," + ch2.ToString() + "," + (object)num4 + "," + str15 + ",'" + str14 + "','" + (object)num3 + "','" + strArrayNew[17] + "'," + strArrayNew[5] + "," + (object)int64 + "," + (object)uint16 + "," + strArrayNew[11] + "," + strArrayNew[12] + ",'" + strArrayNew[42] + "','" + strArrayNew[43] + "','" + strArrayNew[44] + "','" + strArrayNew[45] + "','" + strArrayNew[53] + "','N'," + (object)structure.ExchangeCode + ",''," + (object)1.0 + "," + (object)1.0 + "," + (object)0 + ",'" + strArrayNew[1] + "'," + (object)SSEC + "," + (object)GSMFlag + "," + (object)int32_1 + "," + (object)int32_2 + "," + (object)num5 + ",0,0,0,'','C'," + SecurityStatusNormalMkt + "," + EligibilityNormalMkt + "," + SecurityStatusAdditionalMkt + "," + EligibilityAdditionalMkt + "," + T1Settlement);
                                            }
                                            else
                                            {

                                                DBPendingList.Add("Exec Proc_ScripInfo3 EQ," + structure.Exchange.ToString() + ",C," + (object)structure.ExchangeCode + ",'" + strArrayNew[1] + "','" + str13 + "'," + (object)0 + "," + ch3.ToString() + ",XX," + (object)0.0 + "," + ch4.ToString() + ",'" + strArrayNew[8] + "'," + (object)1 + "," + ch2.ToString() + "," + (object)num4 + "," + str15 + ",'" + str14 + "','" + (object)num3 + "','" + strArrayNew[17] + "'," + strArrayNew[5] + "," + (object)int64 + "," + (object)uint16 + "," + strArrayNew[11] + "," + strArrayNew[12] + ",'" + strArrayNew[42] + "','" + strArrayNew[43] + "','" + strArrayNew[44] + "','" + strArrayNew[45] + "','" + strArrayNew[53] + "','Y'," + (object)structure.ExchangeCode + ",''," + (object)1.0 + "," + (object)1.0 + "," + (object)0 + ",'" + strArrayNew[1] + "'," + (object)SSEC + "," + (object)GSMFlag + "," + (object)int32_1 + "," + (object)int32_2 + "," + (object)num5 + ",0,0,0,'','C'," + SecurityStatusNormalMkt + "," + EligibilityNormalMkt + "," + SecurityStatusAdditionalMkt + "," + EligibilityAdditionalMkt + "," + T1Settlement);
                                                contents1.Add(structure.ExchangeCode.ToString() + "," + strArrayNew[1] + "," + str13 + "," + str11 + "," + strArrayNew[8] + "," + (object)LowerCkt + "," + (object)UpperCkt + "," + ch3.ToString() + "," + ch2.ToString() + "," + (object)num4 + "," + str15 + "," + str14 + "," + (object)num3 + "," + strArrayNew[17] + "," + strArrayNew[5] + "," + (object)int64 + "," + (object)uint16 + "," + strArrayNew[11] + "," + strArrayNew[12] + "," + strArrayNew[42] + "," + strArrayNew[43] + "," + strArrayNew[44] + "," + strArrayNew[45] + "," + strArrayNew[53]);
                                                fileStream.Write(structure.ToByteArray(), 0, count);
                                                contents3.Add(structure.ExchangeCode.ToString() + "," + str15);
                                                if (ch4 == 'Y')
                                                    contents2.Add("N," + (object)structure.ExchangeCode);
                                                Array.Resize<Structure.Scrip_Master_New_Series>(ref array, index1 + 1);
                                                array[index1] = structure;
                                                ++index1;
                                                sqLiteParameterISIN.Value = strArrayNew[4].Trim();

                                                if (true)//checkCoCode.Checked
                                                {
                                                    CoCodeMaster CoCodeRecord = coCodeRecords.SingleOrDefault(x => x.isin == strArrayNew[4].Trim());
                                                    if (CoCodeRecord != null)
                                                    {
                                                        sqLiteParameterCoCode.Value = CoCodeRecord.co_code;
                                                    }
                                                    else
                                                    {
                                                        sqLiteParameterCoCode.Value = 0;
                                                    }
                                                }
                                                ((DbParameter)sqLiteParameter1).Value = (object)structure.Exchange;
                                                ((DbParameter)sqLiteParameter2).Value = (object)structure.ExchangeCode;
                                                ((DbParameter)sqLiteParameter3).Value = (object)new string(structure.ScripName).Substring(0, (int)structure.NameLength);
                                                ((DbParameter)sqLiteParameter4).Value = (object)new string(structure.LongName).Substring(0, (int)structure.LongLength);
                                                ((DbParameter)sqLiteParameter5).Value = (object)TickSize;
                                                ((DbParameter)sqLiteParameter6).Value = (object)new string(structure.Series).Substring(0, (int)structure.SeriesLength);
                                                ((DbParameter)sqLiteParameter7).Value = (object)structure.MinimumLotQty;
                                                ((DbParameter)sqLiteParameter8).Value = $"{str10}-{str13}";
                                                string Symbol = new string(structure.ScripName).Substring(0, (int)structure.NameLength);
                                                var scripRec = scripData.GetRecord(Symbol, new string(structure.Series).Substring(0, (int)structure.SeriesLength));
                                                ScripDataForDictionary dict = new ScripDataForDictionary()
                                                {
                                                    Series = scripRec == null ? "" : scripRec.Series,
                                                    Name = scripRec == null ? "" : scripRec.Name
                                                };

                                                if (scripRec != null)
                                                {
                                                    sqLiteParameterUpperCkt.Value = UpperCkt;
                                                    sqLiteParameterLowerCkt.Value = LowerCkt;
                                                    sqLiteParameterPrevOpen.Value = scripRec.PrevOpen;
                                                    sqLiteParameterPrevHigh.Value = scripRec.PrevHigh;
                                                    sqLiteParameterPrevLow.Value = scripRec.PrevLow;
                                                    sqLiteParameterPrevClose.Value = scripRec.PrevClose;
                                                    sqLiteParameterPrevDayClose.Value = scripRec.PrevDayClose;
                                                    sqLiteParameterYearlyHigh.Value = scripRec.YearlyHigh;
                                                    sqLiteParameterYearlyLow.Value = scripRec.YearlyLow;
                                                    sqLiteParameterYearlyHighDate.Value = scripRec.YearlyHighDate;
                                                    sqLiteParameterYearlyLowDate.Value = scripRec.YearlyLowDate;
                                                    sqLiteParameterSurveillanceFlag.Value = scripRec.SurveillanceFlag;
                                                    sqLiteParameterSurveillanceMsg.Value = scripRec.SurveillanceMsg;
                                                    //sqLiteParameterSurveillanceFlag.Value = scripRec.SurveillanceFlag;

                                                }
                                                else
                                                {
                                                    sqLiteParameterUpperCkt.Value = UpperCkt;
                                                    sqLiteParameterLowerCkt.Value = LowerCkt;
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
                                                ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        ((DbTransaction)sqLiteTransaction).Commit();
                        }
                        ((DbConnection)SQLiteHelper.Master_dbConnection).Close();
                        fileStream.Close();
                        File.AppendAllLines(T2TFile, (IEnumerable<string>)contents2);
                        //File.WriteAllLines("NseEquityMasterNew.csv", (IEnumerable<string>)contents1);
                        File.WriteAllLines("NseISIN.csv", (IEnumerable<string>)contents3);
                        //Array.Clear((Array)MainQH.ExchData[0].ScripData, 0, MainQH.ExchData[0].ScripData.Length);
                        //MainQH.ExchData[0].Create('N', "EQ", 'C', "NseEquityMasterNew.Dat", new AccessCtlDelegate(this.AccessControl));
                       // MainQH.ExchData[0].Create('N', "EQ", 'C', "NseEquityMasterNew.Dat", new AccessCtlDelegate(AccessControl));

                        // TQHThread.ShowMsgBox = true;
                        //this.Button_NseSecurity.Enabled = true;
                        //this.NseCashMasterFileFlag = true;
                        // this.sendNextRequestFromList();
                        CommonFunctions.WriteToLogFile("NseEquityMasterNew file created");
                        return new StatusMessage { message = "Nse Security : " + RecCount + " Records Processed", status = true };
                    }

                }

                catch (Exception ex)
                {
                    CommonFunctions.WriteToLogFile(ex.ToString());
                    return new StatusMessage { message = ex.Message, status = false };
                }

            }
        }
    }
}

