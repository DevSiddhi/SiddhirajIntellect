using JIO_QH.Models;
using JIO_QH.Helpers;
using JIO_QH.QH;
using JIO_QH.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace JIO_QH.BhavCopy
{
    public class NSEDerivBhavCopy
    {
        static int exchcount = 9;
        public static ExchdataForQH[] ExchData = new ExchdataForQH[exchcount];
        public static Structure.TNseMasterInfoRecord[] ArrayNseInfoEquity { get; set; } = Array.Empty<Structure.TNseMasterInfoRecord>();

        public StatusMessage NseDerivBhavCopyExecution()
        {
            string outputMessage = "";
            try
            {
                CommonFunctions.WriteToLogFile("NSE Deriv Bhav Copy button clicked");
                string Symbol, Expiry, StrDesc;
                int Token;
                Single Close;
                List<string> SL = new List<string>();
                SL.Add("ScripCode,Closing");
                DateTime Date = MainQH.LastTradingDate;

                string NseFNOBhavCopyFileName = $"BhavCopy_NSE_FO_0_0_0_{Date:yyyyMMdd}_F_0000.csv";
                string contractFileName = $"NSE_FO_contract_{Date:ddMMyyyy}.csv";

                if (!File.Exists(contractFileName))
                {
                    CommonFunctions.WriteToLogFile($"{contractFileName} File is not available in current folder ");
                    throw new FileNotFoundException($"{contractFileName} File is not available in current folder");
                }
                if (!File.Exists(NseFNOBhavCopyFileName))
                {
                    CommonFunctions.WriteToLogFile(NseFNOBhavCopyFileName + " is not available in current folder");
                    throw new FileNotFoundException(NseFNOBhavCopyFileName + " is not available in current folder");
                }

                if (!MakeNseMasterDeriv()) 
                { 
                    CommonFunctions.WriteToLogFile("Error while creating NSE Master Deriv");
                    throw new Exception("Error while creating NSE Master Deriv");
                }


                foreach (string lines in File.ReadAllLines(NseFNOBhavCopyFileName).Skip(1))
                {
                    string[] columns = lines.Split(',');
                    Symbol = columns[7];
                    Expiry = (Convert.ToDateTime(columns[9])).ToString("dd MMM yyyy");

                    if (columns[4] == "STO" || columns[4] == "IDO")
                    {
                        Single StrikePrice = Convert.ToSingle(columns[11]);///100.0f;
                        string Series = columns[12].Trim();
                        StrDesc = Symbol.Trim() + " " + Expiry + " " + Series + " " + string.Format("{0:0.00}", StrikePrice);
                    }
                    else
                    {
                        StrDesc = Symbol.Trim() + " " + Expiry;
                    }
                    Token = ExchData[1].GetScripDerivRecByDesc(StrDesc);
                    if (Token == 0) continue;
                    Close = Convert.ToSingle(columns[17]);
                    SL.Add(Token + "," + Close);
                }
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\Reports\\NSE F&O BhavCopy_" + DateTime.Today.ToString("yyyyMMdd") + ".Csv", SL);//17.04.16   
                return new StatusMessage { message = "Success :\r\n" + outputMessage, status = true };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while importing");
                CommonFunctions.ErrorLog(ex);
                return new StatusMessage { message = "Success :\r\n" + outputMessage, status = false };
            }
        }
        public static bool MakeNseMasterDeriv()
        {
            #region Variables
            string[] Index = null, Contract = null, Record = null;
            char[] Split = new char[1] { '|' };
            string Indname = null, SecName = null, Str;
            int position = 0;
            bool Result = false;
            #endregion

            if (!File.Exists("Contract.Txt"))
            {
                throw new FileNotFoundException("Contract.Txt is not available for conversion");
            }
            if (!File.Exists("NseIndices.Txt"))
            {
                throw new FileNotFoundException("NseIndices.Txt is not available for conversion");
            }
            if (File.Exists("Contract.Txt") && File.Exists("NseIndices.Txt"))
            {
                SecName = "Contract.Txt";
                Indname = "NseIndices.Txt";

                Index = File.ReadAllLines(Indname);
                Contract = File.ReadAllLines(SecName);

                ArrayNseInfoEquity = new Structure.TNseMasterInfoRecord[Index.Length + Contract.Length];
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
                for (int i = 0; i < Contract.Count(); i++)
                {
                    Record = Contract[i].Split(Split);
                    if (Record.Count() < 5)
                    {
                        continue;
                    }
                    ArrayNseInfoEquity[position + i].Token = Convert.ToInt32(Record[0]);
                    ArrayNseInfoEquity[position + i].Symbol = Record[3].ToCharArray();
                    ArrayNseInfoEquity[position + i].Series = Record[8].ToCharArray();
                }
            }
            Result = true;
            return Result;
        }


    }
}
