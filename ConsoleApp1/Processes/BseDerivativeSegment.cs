using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using ConsoleApp1.QH;
using Master;
using ConsoleApp1.Proto;
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
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1.Processes
{
    public class BseDerivativeSegment
    {
        public static List<string> IndexSrcip = new List<string>();
        public static List<string> DBPendingList = new List<string>();

        public  StatusMessage ExecuteBSEDeriv(bool CheckFile = false)
        {
            StatusMessage res = new StatusMessage();
            SQLiteHelper.OpenSqlite();
            res = ExecuteBSEDeriv_New(CheckFile);

            if (res.status) 
            {
                SQLiteHelper.CopySQLiteTabletoChild("BseDerivMaster", "Exch, ScripCode,ScripCodeDaily,ScripCodeIntraday, Name ,ULToken,OFISType,CPType,ExpiryDate,StrikePrice,CALevel,MinimumLotQuantity, Desc,TradingSymbol,UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose,PrevOI,FreezeQty");
                CommonFunctions.ExportToCsv("child.BseDerivMaster", "BseDerivMaster.csv");
            } 
            return res;
        }
        public static StatusMessage ExecuteBSEDeriv_New(bool CheckFile = false)
        {

            string outputMessage = "";
            var dt = Convert.ToDateTime(MainQH.LastTradingDate);
            string contractFileName = Path.Combine(MainQH.MasterFolder, $"BSE_EQD_CONTRACT_{dt:ddMMyyyy}.csv");
            string BseFNOBhavCopyFileName = Path.Combine(MainQH.MasterFolder, $"BhavCopy_BSE_FO_0_0_0_{dt:yyyyMMdd}_F_0000.csv"); 
                                                                                               
            string DPFile = Path.Combine(MainQH.MasterFolder, $"EQD_DP{dt:ddMMyy}");

            if (CheckFile)
            {
                if (!File.Exists(contractFileName)) outputMessage += $"{contractFileName},";
                if (!File.Exists(BseFNOBhavCopyFileName)) outputMessage += $"{BseFNOBhavCopyFileName},";
                return new StatusMessage { message = outputMessage };
            }

            try
            {
                CommonFunctions.WriteToLogFile("BSE Derivative master making process started");
                Console.WriteLine("BSE derivative master creation processing.....");
                List<string> contents = new List<string>();
                List<DerivativeMasterEntry> protoRecords = new List<DerivativeMasterEntry>();
                string format = "dd MMM yyyy";
                char[] chArray = new char[1] { ',' };
                if (!File.Exists(contractFileName))
                {
                    string str = $"{contractFileName} File is not available in current folder !!!";
                    CommonFunctions.WriteToLogFile(str);
                    return new StatusMessage { message = $"{contractFileName} File is not available in current folder !!!", status = false };
                }
                else
                {
                    if (File.Exists(Path.Combine(MainQH.MasterFolder, "BseDerivMaster.DAT")))
                        File.Delete(Path.Combine(MainQH.MasterFolder, "BseDerivMaster.DAT"));

                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand("drop table if exists BseDerivMaster", SQLiteHelper.Master_dbConnection))
                    {
                        if (SQLiteHelper.Master_dbConnection.State == ConnectionState.Closed)
                        {
                            ((DbConnection)SQLiteHelper.Master_dbConnection).Open();
                        }
                        ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                    }

                    ScripData_Dict scripData = new ScripData_Dict();
                    CommonFunctNSE.readBhavCopy_FO(scripData, BseFNOBhavCopyFileName);      
                    
                    
                    Dictionary<int, bseFilesData> bseDP = new Dictionary<int, bseFilesData>();
                    var DPFileData = File.ReadAllLines(DPFile);
                    for (int i = 1; i < DPFileData.Length; i++)
                    {
                        try
                        {
                            var Line = DPFileData[i].Replace('%', ' ').Trim();
                            var Parts = Line.Split(',');
                            int ExchCode = Convert.ToInt32(Parts[0]);
                            bseFilesData bseDPData = new bseFilesData()
                            {
                                LowerCkt = Convert.ToDecimal(Parts[5]) / 100,
                                UpperCkt = Convert.ToDecimal(Parts[6]) / 100,
                                PClose = Convert.ToDecimal(Parts[2]) / 100
                            };

                            if (!bseDP.ContainsKey(ExchCode))
                                bseDP.Add(ExchCode, bseDPData);
                        }
                        catch (Exception) { }
                    }


                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand("create table BseDerivMaster ( Exch char, ScripCode int, ScripCodeDaily int, ScripCodeIntraday int, Name varchar(12),ULToken int,OFISType byte,CPType byte,ExpiryDate int,StrikePrice single,CALevel uint16,MinimumLotQuantity int64, Desc varchar(50), TradingSymbol varchar(30)," +
                        "UpperCkt money,LowerCkt money,PrevOpen money,PrevHigh money,PrevLow money,PrevClose money,PrevDayClose money,PrevOI int,FreezeQty int)", SQLiteHelper.Master_dbConnection))
                        ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                    FileStream output = new FileStream(Path.Combine(MainQH.MasterFolder, "BseDerivMaster.DAT"), FileMode.Create, FileAccess.Write);
                    FileStream fileStream = new FileStream(contractFileName, FileMode.OpenOrCreate, FileAccess.Read);
                    File.OpenText(contractFileName);
                    BinaryWriter binaryWriter = new BinaryWriter((Stream)output);
                    StreamReader streamReader = new StreamReader((Stream)fileStream);
                    var length1 = File.ReadLines(contractFileName).Count();
                    char ch1 = 'N';
                    contents.Add("ULToken,Symbol,InstrType,OptType,Expiry,Strike,CALevel,LotQty,TickSize,LowerCkt,UpperCkt,LongName,IssuedCapital,CreditRating,BoardLotQuantity,BookClosureStartDate,BookClosureEndDate,DividendInterest,RightsBonus,ExDate");
                    using (SQLiteTransaction sqLiteTransaction = SQLiteHelper.Master_dbConnection.BeginTransaction())
                    {
                        using (SQLiteCommand sqLiteCommand = new SQLiteCommand(SQLiteHelper.Master_dbConnection))
                        {
                            SQLiteParameter sqLiteParameter1 = new SQLiteParameter("@Exchange", DbType.String);
                            SQLiteParameter sqLiteParameter2 = new SQLiteParameter("@ExchangeCode", DbType.Int32);
                            SQLiteParameter sqLiteParameter3 = new SQLiteParameter("@ScripCodeDaily", DbType.Int32);
                            SQLiteParameter sqLiteParameter4 = new SQLiteParameter("@ScripCodeIntraday", DbType.Int32);
                            SQLiteParameter sqLiteParameter5 = new SQLiteParameter("@ScripName", DbType.String);
                            SQLiteParameter sqLiteParameter6 = new SQLiteParameter("@ULToken", DbType.Int32);
                            SQLiteParameter sqLiteParameter7 = new SQLiteParameter("@OFISType", DbType.Byte);
                            SQLiteParameter sqLiteParameter8 = new SQLiteParameter("@CPType", DbType.Byte);
                            SQLiteParameter sqLiteParameter9 = new SQLiteParameter("@ExpiryDate", DbType.Int32);
                            SQLiteParameter sqLiteParameter10 = new SQLiteParameter("@StrikePrice", DbType.Single);
                            SQLiteParameter sqLiteParameter11 = new SQLiteParameter("@CALevel", DbType.UInt16);
                            SQLiteParameter sqLiteParameter12 = new SQLiteParameter("@MinimumLotQty", DbType.Int64);
                            SQLiteParameter sqLiteParameter13 = new SQLiteParameter("@LongName", DbType.String);
                            SQLiteParameter sqLiteParameter14 = new SQLiteParameter("@TradingSymbol", DbType.String);
                            SQLiteParameter sqLiteParameterUpperCkt = new SQLiteParameter("@UpperCkt", DbType.Currency);
                            SQLiteParameter sqLiteParameterLowerCkt = new SQLiteParameter("@LowerCkt", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevOpen = new SQLiteParameter("@PrevOpen", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevHigh = new SQLiteParameter("@PrevHigh", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevLow = new SQLiteParameter("@PrevLow", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevClose = new SQLiteParameter("@PrevClose", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevDayClose = new SQLiteParameter("@PrevDayClose", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevOI = new SQLiteParameter("@PrevOI", DbType.Int64);
                            SQLiteParameter sqLiteParameterFreezeQty = new SQLiteParameter("@FreezeQty", DbType.Currency);
                            ((DbCommand)sqLiteCommand).CommandText = "INSERT INTO BseDerivMaster (Exch, ScripCode,ScripCodeDaily,ScripCodeIntraday, Name ,ULToken,OFISType,CPType,ExpiryDate,StrikePrice,CALevel,MinimumLotQuantity, Desc,TradingSymbol,UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose,PrevDayClose,PrevOI,FreezeQty) VALUES (@Exchange,@ExchangeCode,@ScripCodeDaily,@ScripCodeIntraday,@ScripName,@ULToken,@OFISType,@CPType,@ExpiryDate,@StrikePrice,@CALevel,@MinimumLotQty,@LongName,@TradingSymbol," +
                                "@UpperCkt ,@LowerCkt ,@PrevOpen ,@PrevHigh ,@PrevLow ,@PrevClose,@PrevDayClose,@PrevOI,@FreezeQty)";
                            sqLiteCommand.Parameters.Add(sqLiteParameter1);
                            sqLiteCommand.Parameters.Add(sqLiteParameter2);
                            sqLiteCommand.Parameters.Add(sqLiteParameter3);
                            sqLiteCommand.Parameters.Add(sqLiteParameter4);
                            sqLiteCommand.Parameters.Add(sqLiteParameter5);
                            sqLiteCommand.Parameters.Add(sqLiteParameter6);
                            sqLiteCommand.Parameters.Add(sqLiteParameter7);
                            sqLiteCommand.Parameters.Add(sqLiteParameter8);
                            sqLiteCommand.Parameters.Add(sqLiteParameter9);
                            sqLiteCommand.Parameters.Add(sqLiteParameter10);
                            sqLiteCommand.Parameters.Add(sqLiteParameter11);
                            sqLiteCommand.Parameters.Add(sqLiteParameter12);
                            sqLiteCommand.Parameters.Add(sqLiteParameter13);
                            sqLiteCommand.Parameters.Add(sqLiteParameter14);
                            sqLiteCommand.Parameters.Add(sqLiteParameterUpperCkt);
                            sqLiteCommand.Parameters.Add(sqLiteParameterLowerCkt);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevOpen);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevHigh);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevLow);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevClose);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevDayClose);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevOI);
                            sqLiteCommand.Parameters.Add(sqLiteParameterFreezeQty);
                            for (int index = 0; (long)index < length1; ++index)
                            {
                                if (streamReader.Peek() > 0)
                                {
                                    string[] strArrayFO = streamReader.ReadLine().Split(chArray);
                                    int length2 = strArrayFO.Length;
                                    Structure.Deriv_Master_Record anything = new Structure.Deriv_Master_Record();
                                    if (strArrayFO.Length > 5 && !(strArrayFO[2] == "") && int.TryParse(strArrayFO[0], out _))
                                    {
                                        if (strArrayFO[73] == "8") continue;     
                                        if (strArrayFO[21] == "E") continue;     

                                        decimal UpperCkt = 0;
                                        decimal LowerCkt = 0;
                                        strArrayFO[25] = strArrayFO[25].Trim();

                                        if ((ConfigHelper.BseDeriveIndexOnly == true) && (strArrayFO[2] != "IO" && strArrayFO[2] != "IF"))
                                            continue;
                                       
                                        //Get CKT from DP File
                                        if (strArrayFO[25].Length > 0)
                                            LowerCkt = Convert.ToDecimal(strArrayFO[25]) / 100;
                                        if (strArrayFO[26].Length > 0)
                                            UpperCkt = Convert.ToDecimal(strArrayFO[26]) / 100;


                                        string[] strArray2 = new string[10]
                                              {
                                                strArrayFO[0],
                                                strArrayFO[1],
                                                strArrayFO[2],
                                                strArrayFO[3],
                                                strArrayFO[4],
                                                strArrayFO[5],
                                                strArrayFO[6],
                                                "0",
                                                strArrayFO[8],
                                                strArrayFO[18]
                                              };
                                        Convert.ToInt32(strArray2[1]);
                                        anything.Exch = 'F';
                                        anything.Code = Convert.ToInt32(strArray2[0]);
                                        anything.ULToken = Convert.ToInt32(strArray2[1]);
                                        if ((strArrayFO[2] == "IO" || strArrayFO[2] == "IF"))
                                        {
                                            anything.ULToken += 999900;
                                        }
                                        anything.OFISType = Convert.ToByte(Array.IndexOf<string>(MainQH.OFISTypeBSEFO, strArray2[2]));
                                        if (anything.OFISType != (byte)0)
                                        {
                                            anything.NameLength = Convert.ToByte(strArray2[3].Length);
                                            anything.Name = strArray2[3];
                                            DateTime.TryParse(strArray2[4], out DateTime dateTime);

                                            anything.ExpiryDate = (int)((dateTime - MainQH.ExchStartDate).TotalSeconds);
                                            anything.StrikePrice = Convert.ToSingle(strArray2[5]) / 100f;
                                            if ((double)anything.StrikePrice < 0.0)
                                                anything.StrikePrice = 0.0f;
                                            if (anything.OFISType == 1 || anything.OFISType == 3)
                                                anything.CPType = 0;

                                            else
                                                anything.CPType = Convert.ToByte(Array.IndexOf<string>(MainQH.CPType, strArray2[6]));
                                            anything.MinimumLotQuantity = Convert.ToInt64(strArray2[8]);
                                            string str1 = anything.Name + " " + dateTime.ToString(format);
                                            if (anything.CPType > (byte)0)
                                                str1 = anything.Name + " " + dateTime.ToString(format) + " " + strArray2[6] + " " + string.Format("{0:0.00}", (object)anything.StrikePrice);
                                            else
                                                str1 = anything.Name + " " + dateTime.ToString(format) /*+ " " + "XX" + " " + string.Format("{0:0.00}", (object)anything.StrikePrice)*/;
                                            string str2 = "0";

                                            anything.Desc = str1;
                                            anything.DescLength = Convert.ToByte(anything.Desc.Length);
                                            IndexSrcip.Add(strArray2[2] + "," + strArray2[3]);

                                            char ch2;
                                            char ch3;
                                            ch2 = 'Y';
                                            ch3 = 'N';
                                            char ch4;
                                            char ch5;
                                            ch4 = 'Y';
                                            ch5 = 'Y';
                                            string StockType = MainQH.OFISType[anything.OFISType];
                                            if (strArrayFO[2] == "SF" || strArrayFO[2] == "IF")
                                            {
                                                DBPendingList.Add("Exec Proc_ScripInfo3 " + StockType + ",F,D," + (object)anything.Code + ",'" + anything.Desc + "','" + "XX" + "'," + (object)anything.ExpiryDate + ",Y,'" + "XX" + "'," + (object)anything.StrikePrice + ",N,'" + strArrayFO[10] + "'," + (object)anything.MinimumLotQuantity + "," + ch1.ToString() + ",0," + str2 + ",'" + strArrayFO[18] + "'," + 0 + ",'" + 1 + "'," + strArrayFO[10] + ",0,0," + 0 + "," + 0 + ",'" + ch2.ToString() + "','" + ch5.ToString() + "','" + ch3.ToString() + "','" + ch4.ToString() + "'," + 0 + ",'Y'," + (object)anything.ULToken + ",''," + (object)1.0 + "," + (object)1.0 + "," + (object)0 + ",'" + anything.Name + "'," + (object)0 + "," + (object)0 + "," + (object)100 + "," + (object)100 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + ",'','" + strArrayFO[19] + "'" + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + strArrayFO[31] + "," + strArrayFO[7]);        //Rutuja 22.04.2024
                                            }
                                            else
                                            {
                                                DBPendingList.Add("Exec Proc_ScripInfo3 " + StockType + ",F,D," + (object)anything.Code + ",'" + anything.Desc + "','" + strArray2[6] + "'," + (object)anything.ExpiryDate + ",Y,'" + strArray2[6] + "'," + (object)anything.StrikePrice + ",N,'" + strArrayFO[10] + "'," + (object)anything.MinimumLotQuantity + "," + ch1.ToString() + ",0," + str2 + ",'" + strArrayFO[18] + "'," + 0 + ",'" + 1 + "'," + strArrayFO[10] + ",0,0," + 0 + "," + 0 + ",'" + ch2.ToString() + "','" + ch5.ToString() + "','" + ch3.ToString() + "','" + ch4.ToString() + "'," + 0 + ",'Y'," + (object)anything.ULToken + ",''," + (object)1.0 + "," + (object)1.0 + "," + (object)0 + ",'" + anything.Name + "'," + (object)0 + "," + (object)0 + "," + (object)100 + "," + (object)100 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + ",'','" + strArrayFO[19] + "'" + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + strArrayFO[31] + "," + strArrayFO[7]);        //Rutuja 22.04.2024
                                            }
                                            byte[] buffer = MainQH.RawSerialize((object)anything);
                                            binaryWriter.Write(buffer);
                                            if (strArrayFO[2] == "SF" || strArrayFO[2] == "IF")
                                            {
                                                contents.Add(anything.Code.ToString() + "," + (object)anything.ULToken + "," + anything.Name + "," + strArray2[2] + "," + "XX" + "," + (object)anything.ExpiryDate + "," + strArray2[5] + "," + (object)anything.CALevel + "," + (object)anything.MinimumLotQuantity + "," + strArrayFO[11] + "," + string.Format("{0:0}", (object)(float)(LowerCkt * 100)) + "," + string.Format("{0:0}", (object)(float)(UpperCkt * 100)) + "," + strArrayFO[18] + "," + strArrayFO[24] + "," + strArrayFO[54] + "," + strArrayFO[10] + "," + strArrayFO[49] + "," + strArrayFO[50] + "," + strArrayFO[56] + "," + strArrayFO[57] + "," + strArrayFO[48]);

                                            }
                                            else
                                            {
                                                contents.Add(anything.Code.ToString() + "," + (object)anything.ULToken + "," + anything.Name + "," + strArray2[2] + "," + strArray2[6] + "," + (object)anything.ExpiryDate + "," + strArray2[5] + "," + (object)anything.CALevel + "," + (object)anything.MinimumLotQuantity + "," + strArrayFO[11] + "," + string.Format("{0:0}", (object)(float)(LowerCkt * 100)) + "," + string.Format("{0:0}", (object)(float)(UpperCkt * 100)) + "," + strArrayFO[18] + "," + strArrayFO[24] + "," + strArrayFO[54] + "," + strArrayFO[10] + "," + strArrayFO[49] + "," + strArrayFO[50] + "," + strArrayFO[56] + "," + strArrayFO[57] + "," + strArrayFO[48]);
                                            }
                                            ((DbParameter)sqLiteParameter1).Value = (object)anything.Exch;
                                            ((DbParameter)sqLiteParameter2).Value = (object)anything.Code;
                                            ((DbParameter)sqLiteParameter3).Value = (object)anything.Code;
                                            ((DbParameter)sqLiteParameter4).Value = (object)anything.Code;
                                            ((DbParameter)sqLiteParameter5).Value = (object)anything.Name;
                                            ((DbParameter)sqLiteParameter6).Value = (object)anything.ULToken;
                                            ((DbParameter)sqLiteParameter7).Value = (object)anything.OFISType;
                                            ((DbParameter)sqLiteParameter8).Value = (object)anything.CPType;
                                            ((DbParameter)sqLiteParameter9).Value = (object)anything.ExpiryDate;
                                            ((DbParameter)sqLiteParameter10).Value = (object)anything.StrikePrice;
                                            ((DbParameter)sqLiteParameter11).Value = (object)anything.CALevel;
                                            ((DbParameter)sqLiteParameter12).Value = (object)anything.MinimumLotQuantity;
                                            ((DbParameter)sqLiteParameter13).Value = (object)anything.Desc;
                                            ((DbParameter)sqLiteParameter14).Value = strArray2[9];
                                            string Symbol = anything.Desc;
                                            var scripRec = scripData.GetRecord(anything.Desc);//Trading Symbol in case of BSE
                                            if (bseDP.TryGetValue(anything.Code, out var bseFilesData))
                                            {
                                                UpperCkt = bseFilesData.UpperCkt;
                                                LowerCkt = bseFilesData.LowerCkt;
                                            }
                                            sqLiteParameterUpperCkt.Value = UpperCkt;
                                            sqLiteParameterLowerCkt.Value = LowerCkt;
                                            if (scripRec != null)
                                            {
                                                sqLiteParameterPrevOpen.Value = scripRec.PrevOpen;
                                                sqLiteParameterPrevHigh.Value = scripRec.PrevHigh;
                                                sqLiteParameterPrevLow.Value = scripRec.PrevLow;
                                                sqLiteParameterPrevClose.Value = scripRec.PrevClose;
                                                sqLiteParameterPrevDayClose.Value = scripRec.PrevDayClose;
                                                sqLiteParameterPrevOI.Value = scripRec.PrevOI;
                                            }
                                            else
                                            {
                                                sqLiteParameterPrevOpen.Value = 0;
                                                sqLiteParameterPrevHigh.Value = 0;
                                                sqLiteParameterPrevLow.Value = 0;
                                                sqLiteParameterPrevClose.Value = 0;
                                                sqLiteParameterPrevDayClose.Value = 0;
                                                sqLiteParameterPrevOI.Value = 0;
                                            }
                                            int FreezeQty = (int)(Convert.ToDouble(strArrayFO[40]));
                                            int Excess = (int)(FreezeQty % anything.MinimumLotQuantity);
                                            sqLiteParameterFreezeQty.Value = FreezeQty - Excess;//Skip Anything Over Exact Divisible Value, Freeze Qty has to be in Exact Multiples of Lot Size
                                            var protoEntry = new DerivativeMasterEntry
                                            {
                                                ScripCode = anything.Code,
                                                UlToken = anything.ULToken,
                                                Name = anything.Name,
                                                InstrumentName = strArray2[2],
                                                ExpiryDate = anything.ExpiryDate,
                                                StrikePrice = Convert.ToInt32(anything.StrikePrice * 100),
                                                CpType = anything.CPType,
                                                MinimumLotQty = Convert.ToInt32(anything.MinimumLotQuantity),
                                                Desc = anything.Desc,
                                                TradingSymbol = strArray2[9],
                                                TickSize = Convert.ToInt32(strArrayFO[10]),
                                                UpperCkt = Convert.ToInt32(Convert.ToDecimal(sqLiteParameterUpperCkt.Value) * 100),
                                                LowerCkt = Convert.ToInt32(Convert.ToDecimal(sqLiteParameterLowerCkt.Value) * 100),
                                                PrevClose = Convert.ToInt32(Convert.ToDecimal(sqLiteParameterPrevClose.Value) * 100),
                                                PrevOpenInterest = Convert.ToInt32(sqLiteParameterPrevOI.Value),
                                                FreezeQty = Convert.ToInt32(sqLiteParameterFreezeQty.Value)
                                            };

                                            protoRecords.Add(protoEntry);

                                            ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                        ((DbTransaction)sqLiteTransaction).Commit();
                    }
                    streamReader.Close();
                    binaryWriter.Close();
                    output.Close();

                    ProtoMasterWriter.CreateProtoMasterFile(protoRecords, Path.Combine(MainQH.protoFolder, "BseDerivativeMaster.bin"));
                    //var records = ProtoMasterReader.Read(Path.Combine(MainQH.protoFolder, "BseDerivativeMaster.bin"), DerivativeMasterEntry.Parser);
                    //Console.WriteLine(records.Count());


                    ((DbConnection)SQLiteHelper.Master_dbConnection).Close();
                    CommonFunctions.WriteToLogFile("BseDerivativeMaster file created.");
                    return new StatusMessage { message = "BSE Derivatives : " + contents.Count + " Records Processed", status = true };
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
