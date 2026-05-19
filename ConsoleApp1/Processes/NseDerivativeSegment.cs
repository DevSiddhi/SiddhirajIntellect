using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using ConsoleApp1.QH;
using ConsoleApp1.Proto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Master;

namespace ConsoleApp1.Processes
{
    public class NseDerivativeSegment
    {
        public static List<string> IndexSrcip = new List<string>();
        public static List<string> DBPendingList = new List<string>();


        public StatusMessage ExecuteNseDerivative(bool CheckFile = false)
        {
            StatusMessage res = new StatusMessage();
            SQLiteHelper.OpenSqlite();
            res = ExecuteContract_new(CheckFile);
            if (res.status) 
            {
                SQLiteHelper.CopySQLiteTabletoChild("NseDerivMaster", "Exch, ScripCode,ScripCodeDaily,ScripCodeIntraday, Name ,ULToken,OFISType,CPType,ExpiryDate,StrikePrice,CALevel,MinimumLotQuantity, Desc,TradingSymbol,TickSize ,UpperCkt ,LowerCkt ,FreezeQty, PrevOI");
                CommonFunctions.ExportToCsv("child.NseDerivMaster", "NseDerivMaster.csv");
            }          
            return res;
        }
        private StatusMessage ExecuteContract_new(bool CheckFile = false)
        {
            string outputMessage = "";
            DateTime Date = Convert.ToDateTime(MainQH.LastTradingDate);
            string contractFileName = $"NSE_FO_contract_{Date:ddMMyyyy}.csv";
            string NseFNOBhavCopyFileName = $"BhavCopy_NSE_FO_0_0_0_{Date:yyyyMMdd}_F_0000.csv";
            if (CheckFile)
            {
                if (!File.Exists(contractFileName)) outputMessage += $"{contractFileName},";
                if (!File.Exists(NseFNOBhavCopyFileName)) outputMessage += $"{NseFNOBhavCopyFileName},";
                return new StatusMessage { message = outputMessage };
            }
            try
            {
                CommonFunctions.WriteToLogFile("NSE derivative master making process started");
                Console.WriteLine("NSE derivative master creation processing....");
                //if (this.ProcessStatus())
                //    return new StatusMessage { message = "Startup Pending", status = false };
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
                    if (File.Exists("NseDerivMaster.DAT"))
                        File.Delete("NseDerivMaster.DAT");
                    //if (MorningProcess == true)
                    //{
                    //    ((DbConnection)this.Master_dbConnection).Open();
                    //}
                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand("drop table if exists NseDerivMaster", SQLiteHelper.Master_dbConnection))
                    {
                        if (SQLiteHelper.Master_dbConnection.State == ConnectionState.Closed)
                        {
                            ((DbConnection)SQLiteHelper.Master_dbConnection).Open();
                        }
                        ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                    }

                    ScripData_Dict scripData = new ScripData_Dict();
                    CommonFunctNSE.readBhavCopy_FO(scripData, NseFNOBhavCopyFileName);

                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand("create table NseDerivMaster ( Exch char, ScripCode int, ScripCodeDaily int, ScripCodeIntraday int, Name varchar(12),ULToken int,OFISType byte,CPType byte,ExpiryDate int,StrikePrice single,CALevel uint16,MinimumLotQuantity int64, Desc varchar(50), TradingSymbol varchar(30), TickSize int," +
                        "UpperCkt money,LowerCkt money,PrevOpen money,PrevHigh money,PrevLow money,PrevClose money,PrevDayClose money,PrevOI int,FreezeQty int)", SQLiteHelper.Master_dbConnection))
                        ((DbCommand)sqLiteCommand).ExecuteNonQuery();
                    FileStream output = new FileStream("NseDerivMaster.DAT", FileMode.Create, FileAccess.Write);
                    FileStream fileStream = new FileStream(contractFileName, FileMode.OpenOrCreate, FileAccess.Read);
                    File.OpenText(contractFileName);
                    BinaryWriter binaryWriter = new BinaryWriter((Stream)output);
                    StreamReader streamReader = new StreamReader((Stream)fileStream);
                    var length1 = File.ReadLines(contractFileName).Count();

                    //long length1 = fileStream.Length / (long)Marshal.SizeOf((object)new Structure.Deriv_Master_Record());
                    //Structure.Deriv_Master_Record[] derivMasterRecordArray = new Structure.Deriv_Master_Record[length1];
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
                            SQLiteParameter sqLiteParameterTickSize = new SQLiteParameter("@TickSize", DbType.Int16);
                            SQLiteParameter sqLiteParameterUpperCkt = new SQLiteParameter("@UpperCkt", DbType.Currency);
                            SQLiteParameter sqLiteParameterLowerCkt = new SQLiteParameter("@LowerCkt", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevOpen = new SQLiteParameter("@PrevOpen", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevHigh = new SQLiteParameter("@PrevHigh", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevLow = new SQLiteParameter("@PrevLow", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevClose = new SQLiteParameter("@PrevClose", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevDayClose = new SQLiteParameter("@PrevDayClose", DbType.Currency);
                            SQLiteParameter sqLiteParameterPrevOI = new SQLiteParameter("@PrevOI", DbType.Int64);
                            SQLiteParameter sqLiteParameterFreezeQty = new SQLiteParameter("@FreezeQty", DbType.Currency);
                            ((DbCommand)sqLiteCommand).CommandText = "INSERT INTO NseDerivMaster (Exch, ScripCode,ScripCodeDaily,ScripCodeIntraday, Name ,ULToken,OFISType,CPType,ExpiryDate,StrikePrice,CALevel,MinimumLotQuantity, Desc ,TradingSymbol ,TickSize ,UpperCkt ,LowerCkt ,PrevOpen ,PrevHigh ,PrevLow ,PrevClose,PrevDayClose,PrevOI,FreezeQty) VALUES (@Exchange,@ExchangeCode,@ScripCodeDaily,@ScripCodeIntraday,@ScripName,@ULToken,@OFISType,@CPType,@ExpiryDate,@StrikePrice,@CALevel,@MinimumLotQty,@LongName,@TradingSymbol ," +
                                "@TickSize ,@UpperCkt ,@LowerCkt ,@PrevOpen ,@PrevHigh ,@PrevLow ,@PrevClose,@PrevDayClose,@PrevOI,@FreezeQty)";
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
                            sqLiteCommand.Parameters.Add(sqLiteParameterTickSize);        //Added by Rutuja on 06.07.2024 for ticksize
                            sqLiteCommand.Parameters.Add(sqLiteParameterUpperCkt);
                            sqLiteCommand.Parameters.Add(sqLiteParameterLowerCkt);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevOpen);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevHigh);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevLow);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevClose);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevDayClose);
                            sqLiteCommand.Parameters.Add(sqLiteParameterPrevOI);
                            sqLiteCommand.Parameters.Add(sqLiteParameterFreezeQty);

                            //progressBar.Maximum = (int)length1;
                            for (int index = 0; (long)index < length1; ++index)
                            {
                                //if (index % 1000 == 0)
                                //{
                                //    //progressBar.Value = index;
                                //    Application.DoEvents();
                                //}
                                if (streamReader.Peek() > 0)
                                {
                                    string[] strArrayFO = streamReader.ReadLine().Split(chArray);
                                    int length2 = strArrayFO.Length;
                                    Structure.Deriv_Master_Record anything = new Structure.Deriv_Master_Record();
                                    if (strArrayFO.Length > 5 && !(strArrayFO[21] != "N") && !(strArrayFO[2] == ""))
                                    {
                                        if (strArrayFO[3].Contains("NSETEST")) continue;
                                        decimal UpperCkt = 0;
                                        decimal LowerCkt = 0;
                                        strArrayFO[25] = strArrayFO[25].Trim();

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
                                        anything.Exch = 'N';
                                        anything.Code = Convert.ToInt32(strArray2[0]);
                                        anything.ULToken = Convert.ToInt32(strArray2[1]);
                                        short TickSize = Convert.ToInt16(strArrayFO[10]);  //Rutuja
                                        anything.OFISType = Convert.ToByte(Array.IndexOf<string>(MainQH.OFISType, strArray2[2]));
                                        if (anything.OFISType != (byte)0)
                                        {
                                            anything.NameLength = Convert.ToByte(strArray2[3].Length);
                                            anything.Name = strArray2[3];
                                            anything.ExpiryDate = Convert.ToInt32(strArray2[4]);
                                            anything.StrikePrice = Convert.ToSingle(strArray2[5]) / 100f;
                                            if ((double)anything.StrikePrice < 0.0)
                                                anything.StrikePrice = 0.0f;
                                            anything.CPType = Convert.ToByte(Array.IndexOf<string>(MainQH.CPType, strArray2[6]));
                                            anything.MinimumLotQuantity = Convert.ToInt64(strArray2[8]);
                                            DateTime dateTime = new DateTime(1980, 1, 1).AddSeconds((double)anything.ExpiryDate);
                                            string str1 = anything.Name + " " + dateTime.ToString(format);
                                            if (anything.CPType > (byte)0)
                                                str1 = anything.Name + " " + dateTime.ToString(format) + " " + strArray2[6] + " " + string.Format("{0:0.00}", (object)anything.StrikePrice);
                                            string str2 = "0";
                                            char ch2;
                                            char ch3;
                                            switch (Convert.ToChar(strArrayFO[56]))
                                            {
                                                case '0':
                                                    ch2 = 'N';
                                                    ch3 = 'N';
                                                    break;
                                                case '1':
                                                    ch2 = 'N';
                                                    ch3 = 'Y';
                                                    break;
                                                default:
                                                    ch2 = 'Y';
                                                    ch3 = 'N';
                                                    break;
                                            }
                                            char ch4;
                                            char ch5;
                                            switch (Convert.ToChar(strArrayFO[57]))
                                            {
                                                case char.MinValue:
                                                    ch4 = 'N';
                                                    ch5 = 'N';
                                                    break;
                                                case '1':
                                                    ch4 = 'N';
                                                    ch5 = 'Y';
                                                    break;
                                                case '2':
                                                    ch4 = 'Y';
                                                    ch5 = 'N';
                                                    break;
                                                default:
                                                    ch4 = 'Y';
                                                    ch5 = 'Y';
                                                    break;
                                            }
                                            anything.Desc = str1;
                                            anything.DescLength = Convert.ToByte(anything.Desc.Length);
                                            IndexSrcip.Add(strArray2[2] + "," + strArray2[3]);
                                            DBPendingList.Add("Exec Proc_ScripInfo3 " + strArray2[2] + ",N,D," + (object)anything.Code + ",'" + anything.Desc + "','" + strArray2[6] + "'," + (object)anything.ExpiryDate + ",Y,'" + strArray2[6] + "'," + (object)anything.StrikePrice + ",N,'" + strArrayFO[10] + "'," + (object)anything.MinimumLotQuantity + "," + ch1.ToString() + ",0," + str2 + ",'" + strArrayFO[18] + "'," + strArrayFO[24] + ",'" + strArrayFO[54] + "'," + strArrayFO[10] + ",0,0," + strArrayFO[49] + "," + strArrayFO[50] + ",'" + ch2.ToString() + "','" + ch5.ToString() + "','" + ch3.ToString() + "','" + ch4.ToString() + "'," + strArrayFO[48] + ",'Y'," + (object)anything.ULToken + ",''," + (object)1.0 + "," + (object)1.0 + "," + (object)0 + ",'" + anything.Name + "'," + (object)0 + "," + (object)0 + "," + (object)100 + "," + (object)100 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + ",'','" + strArrayFO[19] + "'" + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + (object)0 + "," + strArrayFO[31] + "," + strArrayFO[7]);
                                            byte[] buffer = MainQH.RawSerialize((object)anything);
                                            binaryWriter.Write(buffer);
                                            contents.Add(anything.Code.ToString() + "," + (object)anything.ULToken + "," + anything.Name + "," + strArray2[2] + "," + strArray2[6] + "," + (object)anything.ExpiryDate + "," + strArray2[5] + "," + (object)anything.CALevel + "," + (object)anything.MinimumLotQuantity + "," + strArrayFO[11] + "," + string.Format("{0:0}", (object)(float)(LowerCkt * 100)) + "," + string.Format("{0:0}", (object)(float)(UpperCkt * 100)) + "," + strArrayFO[18] + "," + strArrayFO[24] + "," + strArrayFO[54] + "," + strArrayFO[10] + "," + strArrayFO[49] + "," + strArrayFO[50] + "," + strArrayFO[56] + "," + strArrayFO[57] + "," + strArrayFO[48]);
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
                                            // ((DbParameter)sqLiteParameter15).Value = (object)anything.TickSize;         //Rutuja 
                                            ((DbParameter)sqLiteParameterTickSize).Value = TickSize;
                                            string Symbol = anything.Desc;
                                            var scripRec = scripData.GetRecord(Symbol);
                                            if (scripRec != null)
                                            {
                                                sqLiteParameterUpperCkt.Value = UpperCkt;
                                                sqLiteParameterLowerCkt.Value = LowerCkt;
                                                sqLiteParameterPrevOpen.Value = scripRec.PrevOpen;
                                                sqLiteParameterPrevHigh.Value = scripRec.PrevHigh;
                                                sqLiteParameterPrevLow.Value = scripRec.PrevLow;
                                                sqLiteParameterPrevClose.Value = scripRec.PrevClose;
                                                sqLiteParameterPrevDayClose.Value = scripRec.PrevDayClose;
                                                sqLiteParameterPrevOI.Value = scripRec.PrevOI;
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
                                                StrikePrice = Convert.ToInt32(Convert.ToDecimal(anything.StrikePrice) * 100),
                                                CpType = anything.CPType,
                                                MinimumLotQty = Convert.ToInt32(anything.MinimumLotQuantity),
                                                Desc = anything.Desc,
                                                TradingSymbol = strArray2[9],
                                                TickSize = TickSize,
                                                UpperCkt = Convert.ToInt32(Convert.ToDecimal(UpperCkt) * 100),
                                                LowerCkt = Convert.ToInt32(Convert.ToDecimal(LowerCkt) * 100),
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

                    ProtoMasterWriter.CreateProtoMasterFile(protoRecords, Path.Combine(MainQH.protoFolder, "NseDerivativeMaster.bin"));
                    //var records = ProtoMasterReader.Read(Path.Combine(MainQH.protoFolder, "NseDerivativeMaster.bin"), DerivativeMasterEntry.Parser);
                    //Console.WriteLine(records.Count());

                    CommonFunctions.WriteToLogFile("NseDerivativeMaster file created.");
                    return new StatusMessage { message = "NSE Derivatives : " + contents.Count + " Records Processed", status = true };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
                CommonFunctions.WriteToLogFile(ex.ToString());
                return new StatusMessage { message = ex.Message, status = false };

            }

        }
    }
}
