using JIO_QH.Helpers;
using JIO_QH.Models;
using JIO_QH.Processes;
using JIO_QH.Proto;
using JIO_QH.QH;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JIO_QH.BhavCopy
{
    public class NSECashBhavCopy
    {
        public static Structure.TNseMasterInfoRecord[] ArrayNseInfoEquity { get; set; } = Array.Empty<Structure.TNseMasterInfoRecord>();
        public StatusMessage NseEquityBhavCopyExecution()
        {
			try
			{
                CommonFunctions.WriteToLogFile("NSE Cash Bhav Copy button clicked");
                string Symbol, Series, NseSymbol, NseSeries;
                string[] Record = null, Data = null;
                char[] Split = new char[1] { ',' };
                int NseMstCnt, Token;
                List<string> SL = new List<string>();

                SL.Add("ScripCode,Closing");
                var Date  = MainQH.LastTradingDate;

                string securityFileName = $"NSE_CM_security_{Date:ddMMyyyy}.csv";
                string NseCMBhavCopyFileName = $"BhavCopy_NSE_CM_0_0_0_{Date:yyyyMMdd}_F_0000.csv";

                if (!File.Exists(securityFileName))
                {
                    throw new FileNotFoundException(securityFileName + " File is not available in current folder");

                }
                if (!File.Exists(NseCMBhavCopyFileName))
                {
                    throw new FileNotFoundException(NseCMBhavCopyFileName + " is not available in current folder");
                }
                if (!MakeNseMasterEquity()) 
                { 
                    Console.WriteLine("Error in making NSE Master Equity Array");
                    throw new Exception("Error in making NSE Master Equity Array");
                }
                Data = File.ReadAllLines(NseCMBhavCopyFileName);
                NseMstCnt = ArrayNseInfoEquity.Length;

                for (int i = 1; i < Data.Length; i++)
                {
                    Record = Data[i].Split(',');

                    Series = Record[8];
                    Symbol = Record[7];

                    Token = -1;
                    for (int j = 0; j < NseMstCnt; j++)
                    {
                        NseSymbol = new string(ArrayNseInfoEquity[j].Symbol);
                        NseSeries = new string(ArrayNseInfoEquity[j].Series);
                        if (NseSymbol != Symbol)
                            continue;
                        if (NseSeries != Series)
                            continue;
                        Token = ArrayNseInfoEquity[j].Token;
                        break;
                    }

                    if (Token == -1)
                        continue;
                   
                    SL.Add(Token + "," + Record[17]);
                }
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\Reports\\NSE CASH BhavCopy_" + DateTime.Today.ToString("yyyyMMdd") + ".Csv", SL);
                CommonFunctions.WriteToLogFile("NSE Cash Bhav copy updated successfully. Count=" + SL.Count.ToString());
                return new StatusMessage { message = "NSE Cash Bhav copy updated successfully. Count=" + SL.Count.ToString(), status = true };
            }
            catch (Exception ex)
			{
                CommonFunctions.ErrorLog(ex);
                Console.WriteLine(ex.Message);
                return new StatusMessage { message = "Error in processing NSE Cash Bhav Copy: " + ex.Message, status = false };
            }
        }

        public static bool MakeNseMasterEquity()
        {
            try
            {
                string[] Index = null, Security = null, Record = null;
                char[] Split = new char[1] { '|' };
                string Indname = null, SecName = null, Str;
                int position = 0;
                bool Result = false;

                if (!File.Exists("Security.Txt"))
                {
                    throw new FileNotFoundException("Security.Txt is not available for conversion");
                }
                if (!File.Exists("NseIndices.Txt"))
                {
                    throw new FileNotFoundException("NseIndices.Txt is not available for conversion");
                }
                if (File.Exists("Security.Txt") && File.Exists("NseIndices.Txt"))
                {
                    SecName = "Security.Txt";
                    Indname = "NseIndices.Txt";

                    Index = File.ReadAllLines(Indname);
                    Security = File.ReadAllLines(SecName);

                    ArrayNseInfoEquity = new Structure.TNseMasterInfoRecord[Index.Length + Security.Length];
                    for (int i = 0; i < Index.Count(); i++)
                    {
                        Record = Index[i].Split(Split);
                        if (Record.Count() < 3)
                        {
                            continue;
                        }
                        ArrayNseInfoEquity[i].Token = Convert.ToInt32(Record[0]);
                        ArrayNseInfoEquity[i].Symbol = (Record[1]).ToCharArray();
                        ArrayNseInfoEquity[i].Series = ("").ToCharArray();

                        position = i;
                    }
                    for (int i = 0; i < Security.Count(); i++)
                    {
                        Record = Security[i].Split(Split);
                        if (Record.Count() < 5)
                        {
                            continue;
                        }
                        ArrayNseInfoEquity[position + i].Token = Convert.ToInt32(Record[0]);
                        ArrayNseInfoEquity[position + i].Symbol = Record[1].ToCharArray();
                        ArrayNseInfoEquity[position + i].Series = Record[2].ToCharArray();
                    }
                }
                Result = true;
                return Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                CommonFunctions.ErrorLog(ex);
                return false;
            }
            
        }


    }
}
