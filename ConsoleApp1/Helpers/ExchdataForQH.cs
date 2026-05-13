using ConsoleApp1.Models;
using ConsoleApp1.QH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Helpers
{
    public delegate void AccessCtlDelegate(string str);

    public class ExchdataForQH
    {
        #region value declaration
        public long Master_FileCount;
        public double Master_FileCount_Check;
        int Master_RecSize, ScripCode = 0;
        char Exch, ExchTypeShort;
        string ExchType, ExchLong, ExchTypeLong, MasterFileName;
        //public static byte[] Sh1Rec = new byte[30];
        public int LastDataRecTime, DerivULCnt, ScripCnt, ExpiryDateCnt;
        byte[] Master_Record;
        public long CountNseFo;// jinal Spread // Added on 24-Aug-2016 - Rohan - Pinnacle
        int[] SortedExchCodes;
        public int[] ExpiryDates;
        AccessCtlDelegate AccessCtl;
        #endregion

        #region Structure Declaration
        public Structure.Equity_Master_Record EQMR;
        public Structure.SH1_Record SH1;
        Structure.Sh1ARecord SH1ARec;
        Structure.Sh1WRecord SH1WRec;
        Structure.Sh1HRecord SH1HRec;
        Structure.Sh1BRecord SH1BRec;
        Structure.Sh1GRecord SH1GRec;   //Added on 24-Oct-2016 - Rohan - Merged from Indira QH

        public Structure.Deriv_Master_Record DERIV_Mast;
        public Structure.ScripData_Record[] ScripData;
        public Structure.Deriv_Master_Record[] DerivScripDataTemp;
        public Structure.McxDeriv_Master_Record[] DerivMcxScripDataTemp;
        public Structure.Spread_Master_Record[] DerivSpreadDataTemp; // jinal Spread    // Added on 24-Aug-2016 - Rohan - Pinnacle
        public Structure.Equity_Master_Record[] EquityScripDataTemp;
        public Structure.Equity_Master_Record_New[] EquityScripDataTempNew; //Added on 08-Nov-2017 - Rohan - Series
        public Structure.TDerivUnderlyingRecord[] DerivUnderLying;  //Added on 11-May-2017 - Rohan - To Add ULToken to the SpdContracts.
        public Structure.BseMFCodeMasterDetails[] BseMFScripDataTemp;//bseMf Krutika
        public Structure.TCommodityExpiry[] ExpiryDatesList;
        #endregion

        public void Create(char Iexch, string IExchType, char IExchTypeShort, string IMasterFileName, AccessCtlDelegate temp)
        {
            Exch = Iexch;
            ExchType = IExchType;
            ExchTypeShort = IExchTypeShort;
            MasterFileName = IMasterFileName;
            AccessCtl = temp;

            if (Exch == 'N' || Exch == 'C')
                ExchLong = "NSE";
            else if (Exch == 'B')
                ExchLong = "BSE";
            else
                ExchLong = "MCX"; //jinal mcx


            if (ExchType == "EQ")
                ExchTypeLong = "Equity";
            else if (ExchType == "MF") //BseMf Krutika
                ExchTypeLong = "MutualFund";
            else
                ExchTypeLong = "Derivative";
            ExpiryDatesList = new Structure.TCommodityExpiry[0];
            LoadScripMaster();

            //changedone
            if (((Exch == 'N') && (ExchType == "FO")) && (MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_PINNACLE || MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_INDIRA || MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_EMKAY))//07.10.2016 || MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_FAIRWEALTH)) //jinal Spread
            {
                Deriv_Master_Reader("SpreadMaster.Dat");
            }
            if (((Exch == 'C') && (ExchType == "FO")) && (MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_PINNACLE || MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_INDIRA || MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_EMKAY))//07.10.2016 || MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_FAIRWEALTH)) //jinal Spread
            {
                Deriv_Master_Reader("CurrSpreadMaster.Dat");
            }
        }

        //jinal Spread
        void Deriv_Master_Reader(string MasterFileName) // Added on 24-Aug-2016 - Rohan - Pinnacle
        {
            Master_Record = new byte[Marshal.SizeOf(new Structure.Spread_Master_Record())];
            FileStream Master_Stream = new FileStream(MasterFileName, FileMode.OpenOrCreate, FileAccess.Read);
            Master_RecSize = Marshal.SizeOf(new Structure.Spread_Master_Record());
            Master_FileCount = Master_Stream.Length / Master_RecSize;
            Master_FileCount = Master_FileCount + CountNseFo;
            Array.Resize<Structure.ScripData_Record>(ref ScripData, Convert.ToInt32(Master_FileCount));
            //ScripData = new Structure.ScripData_Record[Master_FileCount];
            Array.Resize<int>(ref SortedExchCodes, Convert.ToInt32(Master_FileCount));
            //SortedExchCodes = new int[Master_FileCount];
            Array.Resize<Structure.Spread_Master_Record>(ref DerivSpreadDataTemp, Convert.ToInt32(Master_FileCount));
            //DerivSpreadDataTemp = new Structure.Spread_Master_Record[Master_FileCount];
            int k;
            ScripCnt = (int)Master_Stream.Length / Master_RecSize;
            for (long i = CountNseFo; i < Master_FileCount; i++)
            {
                Master_Stream.Read(Master_Record, 0, Master_RecSize);
                DerivSpreadDataTemp[i] = MainQH.ReadUsingMarshalUnsafe<Structure.Spread_Master_Record>(Master_Record);//, DerivScripDataTemp[i], 0);
                SortedExchCodes[i] = DerivSpreadDataTemp[i].Code;
            }
            Master_Stream.Close();
            if (SortedExchCodes.Length != 0)
            {
                Array.Sort(SortedExchCodes);
                //SearchSort.QuickSort(ref SortedExchCodes);
                for (long i = CountNseFo; i < Master_FileCount; i++)
                {
                    //k = SearchSort.BinSearch(ref SortedExchCodes, DerivScripDataTemp[i].code);
                    k = Array.BinarySearch(SortedExchCodes, DerivSpreadDataTemp[i].Code);
                    ScripData[k].Exch = DerivSpreadDataTemp[i].Exch;
                    ScripData[k].ScripCode = DerivSpreadDataTemp[i].Code;
                    ScripData[k].ScNameLen = DerivSpreadDataTemp[i].NameLength; //ask
                    ScripData[k].ScripName = DerivSpreadDataTemp[i].Name;
                    ScripData[k].ScDescLen = DerivSpreadDataTemp[i].DescLength;
                    ScripData[k].ScripDesc = DerivSpreadDataTemp[i].Desc;
                    ScripData[k].ULToken = DerivSpreadDataTemp[i].ULToken;
                    ScripData[k].LastTradeRate = 0.00f;
                    ScripData[k].LastTradeTime = 0;
                    ScripData[k].UpperCktLimit = 0;
                    ScripData[k].LowerCktLimit = 0;
                    ScripData[k].CumQty = 0;
                    ScripData[k].HasNewTrade = false;
                    ScripData[k].MBPArray = new Structure.Sh1BRecord[1];
                }
            }
        }

        void LoadScripMaster()
        {
            if (ExchType == "EQ")
            {
                //Added code on 08-Nov-2017 - Rohan - QH loading with old and new Equity masters - Starts here.
                FileStream Master_Stream = new FileStream(MasterFileName, FileMode.OpenOrCreate, FileAccess.Read);
                Master_RecSize = Marshal.SizeOf(new Structure.Equity_Master_Record_New());
                Master_FileCount_Check = (double)Master_Stream.Length / Master_RecSize;
                Master_Stream.Close();
                if (Master_FileCount_Check % 1 == 0)
                    Equity_Master_Reader_New();
                else
                    Equity_Master_Reader();
                //Added code on 08-Nov-2017 - Rohan - QH loading with old and new Equity masters - Ends here.
            }
            else if (Exch == 'M')
                LoadScripMasterMcxDeriv();
            else if (ExchType == "MF")//BseMf Krutika
                LoadMutualFundMaster();
            else
                Deriv_Master_Reader();


        }

        public int LocateScripByName(string Name)
        {
            int num = -1;
            for (int index = 0; (long)index < this.Master_FileCount; ++index)
            {
                if (!(this.ScripData[index].ScripName.Trim() != Name.TrimEnd()))
                {
                    num = index;
                    break;
                }
            }
            return num;
        }

        public void LoadMutualFundMaster()//BseMf Krutika
        {
            string[] SortedExchCodes;
            Master_Record = new byte[Marshal.SizeOf(new Structure.BseMFCodeMasterDetails())];
            FileStream Master_Stream = new FileStream(MasterFileName, FileMode.OpenOrCreate, FileAccess.Read);
            Master_RecSize = Marshal.SizeOf(new Structure.BseMFCodeMasterDetails());
            Master_FileCount = Master_Stream.Length / Master_RecSize;
            ScripData = new Structure.ScripData_Record[Master_FileCount];
            SortedExchCodes = new string[Master_FileCount];
            BseMFScripDataTemp = new Structure.BseMFCodeMasterDetails[Master_FileCount];

            try
            {
                int k;
                for (int i = 0; i < Master_FileCount; i++)
                {
                    Master_Stream.Read(Master_Record, 0, Master_RecSize);
                    BseMFScripDataTemp[i] = MainQH.ReadUsingMarshalUnsafe<Structure.BseMFCodeMasterDetails>(Master_Record);//, EquityScripDataTemp[i], 0);
                    SortedExchCodes[i] = new string(BseMFScripDataTemp[i].UniqueNo).PadRight(5);
                }
                Master_Stream.Close();
                if (SortedExchCodes.Length != 0)
                {
                    Array.Sort(SortedExchCodes);
                    for (int i = 0; i < Master_FileCount; i++)
                    {
                        k = Array.BinarySearch(SortedExchCodes, new string(BseMFScripDataTemp[i].UniqueNo));

                        ScripData[k].Exch = BseMFScripDataTemp[i].Exch;
                        ScripData[k].UniqueNoLength = BseMFScripDataTemp[i].UniqueNoLength;
                        ScripData[k].UniqueNo = BseMFScripDataTemp[i].UniqueNo;
                        ScripData[k].SchemeCodeLength = BseMFScripDataTemp[i].SchemeCodeLength;
                        ScripData[k].SchemeCode = BseMFScripDataTemp[i].SchemeCode;
                        ScripData[k].RTASchemeCodeLength = BseMFScripDataTemp[i].RTASchemeCodeLength;
                        ScripData[k].RTASchemeCode = BseMFScripDataTemp[i].RTASchemeCode;
                        ScripData[k].AMCSchemeCodeLength = BseMFScripDataTemp[i].AMCSchemeCodeLength;
                        ScripData[k].AMCSchemeCode = BseMFScripDataTemp[i].AMCSchemeCode;
                        ScripData[k].ISINLength = BseMFScripDataTemp[i].ISINLength;
                        ScripData[k].ISIN = BseMFScripDataTemp[i].ISIN;
                        ScripData[k].AMCCodeLength = BseMFScripDataTemp[i].AMCCodeLength;
                        ScripData[k].AMCCode = BseMFScripDataTemp[i].AMCCode;
                        ScripData[k].SchemeTypeLength = BseMFScripDataTemp[i].SchemeTypeLength;
                        ScripData[k].SchemeType = BseMFScripDataTemp[i].SchemeType;
                        ScripData[k].SchemePlanLength = BseMFScripDataTemp[i].SchemePlanLength;
                        ScripData[k].SchemePlan = BseMFScripDataTemp[i].SchemePlan;
                        ScripData[k].SchemeNameLength = BseMFScripDataTemp[i].SchemeNameLength;
                        ScripData[k].SchemeName = BseMFScripDataTemp[i].SchemeName;
                        ScripData[k].PurchaseAllowed = BseMFScripDataTemp[i].PurchaseAllowed;
                        ScripData[k].MinimumPurchaseAmt = BseMFScripDataTemp[i].MinimumPurchaseAmt;
                        ScripData[k].MaximumPurchaseAmt = BseMFScripDataTemp[i].MaximumPurchaseAmt;
                        ScripData[k].PurchaseAmountMultiplier = BseMFScripDataTemp[i].PurchaseAmountMultiplier;
                        ScripData[k].PurchaseCutoffTimeLength = BseMFScripDataTemp[i].PurchaseCutoffTimeLength;
                        ScripData[k].PurchaseCutoffTime = BseMFScripDataTemp[i].PurchaseCutoffTime;
                        ScripData[k].RedemptionAllowed = BseMFScripDataTemp[i].RedemptionAllowed;
                        ScripData[k].MinimumRedemptionQty = BseMFScripDataTemp[i].MinimumRedemptionQty;
                        ScripData[k].MaximumRedemptionQty = BseMFScripDataTemp[i].MaximumRedemptionQty;
                        ScripData[k].RedemptionAmtMin = BseMFScripDataTemp[i].RedemptionAmtMin;
                        ScripData[k].RedemptionAmtMax = BseMFScripDataTemp[i].RedemptionAmtMax;
                        ScripData[k].DividendReinvestmentFlag = BseMFScripDataTemp[i].DividendReinvestmentFlag;
                        ScripData[k].SIPFlag = BseMFScripDataTemp[i].SIPFlag;
                        ScripData[k].STPFlag = BseMFScripDataTemp[i].STPFlag;
                        ScripData[k].SWPFlag = BseMFScripDataTemp[i].SWPFlag;
                        ScripData[k].SwitchFlag = BseMFScripDataTemp[i].SwitchFlag;
                        ScripData[k].NavValue = BseMFScripDataTemp[i].NavValue;
                    }
                }
            }
            catch (Exception ex)
            {

                //File.Delete(MasterFileName); // If File is Junked, Delete the same.
                //Master_Stream = new FileStream(MasterFileName, FileMode.Create, FileAccess.ReadWrite);
                //Master_Stream.Close();
                //frmMainForm.ErrorLog(ex);
                //return;

            }
        }

        void Equity_Master_Reader()
        {
            //List<string> writeToFileList = new List<string>();
            Master_Record = new byte[Marshal.SizeOf(new Structure.Equity_Master_Record())];
            FileStream Master_Stream = new FileStream(MasterFileName, FileMode.OpenOrCreate, FileAccess.Read);

            Master_RecSize = Marshal.SizeOf(new Structure.Equity_Master_Record());
            Master_FileCount = Master_Stream.Length / Master_RecSize;
            ScripData = new Structure.ScripData_Record[Master_FileCount];
            SortedExchCodes = new int[Master_FileCount];
            EquityScripDataTemp = new Structure.Equity_Master_Record[Master_FileCount];
            int k;

            for (int i = 0; i < Master_FileCount; i++)
            {
                Master_Stream.Read(Master_Record, 0, Master_RecSize);
                EquityScripDataTemp[i] = MainQH.ReadUsingMarshalUnsafe<Structure.Equity_Master_Record>(Master_Record);//, EquityScripDataTemp[i], 0);
                SortedExchCodes[i] = EquityScripDataTemp[i].ExchCode; ;
            }
            Master_Stream.Close();

            if (SortedExchCodes.Length != 0)
            {
                //SearchSort.QuickSort(ref SortedExchCodes);
                Array.Sort(SortedExchCodes);

                for (int i = 0; i < Master_FileCount; i++)
                {
                    // k = SearchSort.BinSearch(ref SortedExchCodes, EquityScripDataTemp[i].ExchCode);
                    k = Array.BinarySearch(SortedExchCodes, EquityScripDataTemp[i].ExchCode);
                    ScripData[k].Exch = EquityScripDataTemp[i].Exch;
                    ScripData[k].ScripCode = EquityScripDataTemp[i].ExchCode;
                    ScripData[k].ScNameLen = EquityScripDataTemp[i].Length;
                    ScripData[k].ScripName = EquityScripDataTemp[i].Name;
                    ScripData[k].ScDescLen = EquityScripDataTemp[i].Length;
                    ScripData[k].ScripDesc = EquityScripDataTemp[i].Name;

                    ScripData[k].LastTradeRate = 0.00f;
                    ScripData[k].LastTradeTime = 0;
                    ScripData[k].UpperCktLimit = 0;
                    ScripData[k].LowerCktLimit = 0;
                    ScripData[k].CumQty = 0;
                    ScripData[k].MBPArray = new Structure.Sh1BRecord[1];
                    ScripData[k].HasNewTrade = false;
                }
            }
        }

        void Equity_Master_Reader_New() //Added on 08-Nov-2017 - Rohan - Series New Master
        {
            try
            {
                Master_Record = new byte[Marshal.SizeOf(new Structure.Equity_Master_Record_New())];
                FileStream Master_Stream = new FileStream(MasterFileName, FileMode.OpenOrCreate, FileAccess.Read);

                Master_RecSize = Marshal.SizeOf(new Structure.Equity_Master_Record_New());
                Master_FileCount = Master_Stream.Length / Master_RecSize;
                ScripData = new Structure.ScripData_Record[Master_FileCount];
                SortedExchCodes = new int[Master_FileCount];
                ScripCnt = (int)Master_Stream.Length / Master_RecSize;
                EquityScripDataTempNew = new Structure.Equity_Master_Record_New[Master_FileCount];
                int k;

                for (int i = 0; i < Master_FileCount; i++)
                {
                    Master_Stream.Read(Master_Record, 0, Master_RecSize);
                    EquityScripDataTempNew[i] = MainQH.ReadUsingMarshalUnsafe<Structure.Equity_Master_Record_New>(Master_Record);
                    SortedExchCodes[i] = EquityScripDataTempNew[i].ExchangeCode;
                }
                Master_Stream.Close();

                if (SortedExchCodes.Length != 0)
                {
                    //SearchSort.QuickSort(ref SortedExchCodes);
                    Array.Sort(SortedExchCodes);

                    for (int i = 0; i < Master_FileCount; i++)
                    {
                        k = Array.BinarySearch(SortedExchCodes, EquityScripDataTempNew[i].ExchangeCode);
                        ScripData[k].Exch = EquityScripDataTempNew[i].Exchange;
                        ScripData[k].ScripCode = EquityScripDataTempNew[i].ExchangeCode;
                        ScripData[k].ScNameLen = EquityScripDataTempNew[i].NameLength;
                        ScripData[k].ScripName = new string(EquityScripDataTempNew[i].ScripName).Substring(0, EquityScripDataTempNew[i].NameLength);
                        ScripData[k].ScDescLen = EquityScripDataTempNew[i].LongLength;
                        ScripData[k].ScripDesc = new string(EquityScripDataTempNew[i].LongName).Substring(0, EquityScripDataTempNew[i].LongLength);
                        ScripData[k].TickSize = EquityScripDataTempNew[i].TickSize;
                        ScripData[k].Series = new string(EquityScripDataTempNew[i].Series).Substring(0, EquityScripDataTempNew[i].SeriesLength);
                        ScripData[k].MinimumLotQuantity = (ushort)EquityScripDataTempNew[i].MinimumLotQty;
                        ScripData[k].LastTradeRate = 0.00f;
                        ScripData[k].LastTradeTime = 0;
                        ScripData[k].UpperCktLimit = 0;
                        ScripData[k].LowerCktLimit = 0;
                        ScripData[k].CumQty = 0;
                        ScripData[k].MBPArray = new Structure.Sh1BRecord[1];
                        ScripData[k].HasNewTrade = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Index and length must refer to a location within the string.\r\nParameter name: length")
                {
                    Equity_Master_Reader();
                }
            }
        }

        public void GetUnderlyingTokens() //Added on 11-May-2017 - Rohan - To Add ULToken to the SpdContracts.
        {
            int ULToken, i, j;
            bool Exists;
            DerivULCnt = 0;

            Array.Resize(ref DerivUnderLying, (int)Master_FileCount);
            try
            {
                for (i = 0; i < Master_FileCount; i++)
                {
                    ULToken = ScripData[i].ULToken;

                    if (ULToken <= 0) continue;
                    Exists = false;
                    for (j = 0; j < DerivULCnt; j++)
                    {
                        if (ULToken != DerivUnderLying[j].ULToken) continue;
                        Exists = true;
                        break;
                    }

                    DerivUnderLying[DerivULCnt].ULToken = ScripData[i].ULToken;
                    DerivUnderLying[DerivULCnt].Name = ScripData[i].ScripName.ToCharArray();
                    DerivUnderLying[DerivULCnt].NameLength = ScripData[i].ScNameLen;
                    DerivULCnt++;
                }
                Array.Resize(ref DerivUnderLying, DerivULCnt);
            }
            catch (Exception ee)
            {

            }
        }

        void Deriv_Master_Reader()
        {
            Master_Record = new byte[Marshal.SizeOf(new Structure.Deriv_Master_Record())];
            FileStream Master_Stream = new FileStream(MasterFileName, FileMode.OpenOrCreate, FileAccess.Read);
            Master_RecSize = Marshal.SizeOf(new Structure.Deriv_Master_Record());
            Master_FileCount = Master_Stream.Length / Master_RecSize;
            ScripData = new Structure.ScripData_Record[Master_FileCount];
            ScripCnt = (int)Master_Stream.Length / Master_RecSize;
            SortedExchCodes = new int[Master_FileCount];
            DerivScripDataTemp = new Structure.Deriv_Master_Record[Master_FileCount];
            if (MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_PINNACLE || MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_INDIRA || MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_EMKAY)//07.10.2010 || frmQuoteHandler.BrokerCodeQH == frmQuoteHandler.BrokerNameRecords.BROKER_FAIRWEALTH)  // Added on 24-Aug-2016 - Rohan - Pinnacle
            {
                //jinal spread count
                CountNseFo = Master_FileCount;
            }
            int k;
            for (int i = 0; i < Master_FileCount; i++)
            {
                Master_Stream.Read(Master_Record, 0, Master_RecSize);
                DerivScripDataTemp[i] = MainQH.ReadUsingMarshalUnsafe<Structure.Deriv_Master_Record>(Master_Record);//, DerivScripDataTemp[i], 0);
                SortedExchCodes[i] = DerivScripDataTemp[i].Code;
            }
            Master_Stream.Close();
            if (SortedExchCodes.Length != 0)
            {
                Array.Sort(SortedExchCodes);
                //SearchSort.QuickSort(ref SortedExchCodes);
                for (int i = 0; i < Master_FileCount; i++)
                {
                    //k = SearchSort.BinSearch(ref SortedExchCodes, DerivScripDataTemp[i].code);
                    k = Array.BinarySearch(SortedExchCodes, DerivScripDataTemp[i].Code);
                    ScripData[k].Exch = DerivScripDataTemp[i].Exch;
                    ScripData[k].ScripCode = DerivScripDataTemp[i].Code;
                    ScripData[k].ScNameLen = DerivScripDataTemp[i].NameLength;
                    ScripData[k].ScripName = DerivScripDataTemp[i].Name;
                    ScripData[k].ScDescLen = DerivScripDataTemp[i].DescLength;
                    ScripData[k].ScripDesc = DerivScripDataTemp[i].Desc;
                    ScripData[k].ULToken = DerivScripDataTemp[i].ULToken;
                    ScripData[k].Expiry = DerivScripDataTemp[i].ExpiryDate;
                    ScripData[k].OFISType = DerivScripDataTemp[i].OFISType;
                    ScripData[k].CPType = DerivScripDataTemp[i].CPType;
                    ScripData[k].LastTradeRate = 0.00f;
                    ScripData[k].LastTradeTime = 0;
                    ScripData[k].UpperCktLimit = 0;
                    ScripData[k].LowerCktLimit = 0;
                    ScripData[k].CumQty = 0;
                    ScripData[k].HasNewTrade = false;
                    ScripData[k].MBPArray = new Structure.Sh1BRecord[1];
                }
            }
            GetUnderlyingTokens();  //Added on 11-May-2017 - Rohan - To Add ULToken to the SpdContracts.
            GetExpiryDates();
            GetExpiryDateCommodity();
        }

        public void GetExpiryDates() //14.02.17 Priyanka
        {
            int i, j, Expiry;
            bool EDExists;
            ExpiryDates = null;
            ExpiryDateCnt = 0;
            string ScName; //26.05.16

            Array.Resize(ref ExpiryDates, ScripCnt);
            for (i = 0; i < ScripCnt; i++)
            {
                if (ScripData[i].ScripName == null) continue;
                if (ScripData[i].OFISType == 9) continue; //07.08.15 Handling All FUTIRC Contracts on Basis of INSTR TYPE, instead of hard coding their name.
                //30.07.15 Adding 7-10 Code for COGGBP Indexes //10.08.15 For new index 16 [CCLIUSD], handling with Expiry.
                if (ScripData[i].Exch == 'C' && ((ScripData[i].ScripCode == 1 || ScripData[i].ScripCode == 25 || ScripData[i].ScripCode == 26 || ScripData[i].ScripCode == 27 || ScripData[i].ScripCode == 7 || ScripData[i].ScripCode == 8 || ScripData[i].ScripCode == 9 || ScripData[i].ScripCode == 10) || ScripData[i].Expiry == -1))
                    continue;
                //26.05.16 For Ignoring Weekly Contracts
                if (ScripData[i].Exch == 'N' && DerivUnderLying[ScripData[i].DerivULPos].IsWeeklyContractSymbol == 1) continue;
                if (ScripData[i].Exch == 'C' && DerivUnderLying[ScripData[i].DerivULPos].IsWeeklyContractSymbol == 1) continue;
                ScName = ScripData[i].ScripName;//26.05.16  Instead of making substring everytime.
                if (ScName == "DJIA" || ScName == "S&P500" || ScName == "FTSE100" || ScName == "INDIAVIX") continue;

                Expiry = ScripData[i].Expiry;
                if (Expiry == 0) continue;
                EDExists = false;

                for (j = 0; j < ExpiryDateCnt; j++)
                {
                    if (Expiry != ExpiryDates[j]) continue;
                    EDExists = true;
                    break;
                }

                if (EDExists) continue;
                ExpiryDates[ExpiryDateCnt] = Expiry;
                ExpiryDateCnt++;

            }
            Array.Resize(ref ExpiryDates, ExpiryDateCnt);

            for (i = 0; i < ExpiryDateCnt; i++)
            {
                for (j = i + 1; j < ExpiryDateCnt; j++)
                {
                    if (ExpiryDates[i] < ExpiryDates[j]) continue;
                    Expiry = ExpiryDates[i];
                    ExpiryDates[i] = ExpiryDates[j];
                    ExpiryDates[j] = Expiry;
                }
            }
        }

        public void GetExpiryDateCommodity() //28.05.2016 Utilization
        {
            try
            {
                List<string> Scrip = new List<string>();
                List<string> DayMonth = new List<string>();
                int pos = -1, Loc = -1, ULToken, DerivePos;
                int[] DistExpiry = new int[10]; //11.06.2016
                int Position = 0;
                bool Update = false;

                List<int> Monthly = new List<int>();
                for (int i = 0; i < ScripCnt; i++)
                {
                    if (ScripData[0].Exch == 'N')   //11.06.2016
                        Update = true;

                    if (ScripData[i].Exch == 'C' && ((ScripData[i].ScripCode == 1 || ScripData[i].ScripCode == 25 || ScripData[i].ScripCode == 26 || ScripData[i].ScripCode == 27 || ScripData[i].ScripCode == 7 || ScripData[i].ScripCode == 8 || ScripData[i].ScripCode == 9 || ScripData[i].ScripCode == 10)
                                                     || ScripData[i].Expiry == -1)) //30.07.15 Adding 7-10 Code for COGGBP Indexes //10.08.15 For new index 16 [CCLIUSD], handling with Expiry.
                        continue;
                    if (Scrip.Contains(ScripData[i].ScripName))
                    {
                        Loc = Scrip.IndexOf(ScripData[i].ScripName);
                        if (ExpiryDatesList[Loc].EDateList.Contains(ScripData[i].Expiry)) continue;
                        ExpiryDatesList[Loc].EDateList.Add(ScripData[i].Expiry);

                    }
                    else
                    {
                        Scrip.Add(ScripData[i].ScripName);

                        Array.Resize(ref ExpiryDatesList, ExpiryDatesList.Length + 1);
                        pos++;

                        ExpiryDatesList[pos].Name = ScripData[i].ScripName;
                        ExpiryDatesList[pos].NameLength = ScripData[i].ScNameLen;
                        ExpiryDatesList[pos].EDateList = new List<int>();
                        ExpiryDatesList[pos].ULToken = ScripData[i].ULToken;
                        ExpiryDatesList[pos].EDateList.Add(ScripData[i].Expiry);
                    }
                }
                for (int i = 0; i < ExpiryDatesList.Length; i++)
                {
                    ULToken = ExpiryDatesList[i].ULToken;
                    DerivePos = GetULTokenPositionFromToken(ULToken);
                    if (DerivUnderLying == null) break;
                    if (DerivUnderLying[DerivePos].IsWeeklyContractSymbol == 1)
                    {
                        if (ULToken == 26017) continue;
                        ExpiryDatesList[i].WeeklyExpiryList = new List<int>();
                        for (int WeekArrPos = 0; WeekArrPos < DerivUnderLying[DerivePos].WeeklyExpiries.Length; WeekArrPos++)
                        {
                            if (Array.IndexOf(ExpiryDates, DerivUnderLying[DerivePos].WeeklyExpiries[WeekArrPos]) < 0)
                            {
                                ExpiryDatesList[i].WeeklyExpiryList.Add(DerivUnderLying[DerivePos].WeeklyExpiries[WeekArrPos]);
                                ExpiryDatesList[i].EDateList.Remove(DerivUnderLying[DerivePos].WeeklyExpiries[WeekArrPos]);
                            }
                        }
                        ExpiryDatesList[i].EDateList.Sort();
                    }
                }
            }
            catch (Exception ex)
            {
                //frmMainForm.ErrorLog(ex);
            }
        }

        public int GetULTokenPositionFromToken(int ULToken)
        {
            int i, Result = 0;
            for (i = 0; i < DerivULCnt; i++)
            {
                if (DerivUnderLying[i].ULToken != ULToken) continue;
                Result = i;
                break;
            }
            return Result;
        }

        public int GetULTokenFromDerivName(string Name)
        {
            int i, Result = 0;
            string DervName;
            for (i = 0; i < DerivULCnt; i++)
            {
                DervName = new string(DerivUnderLying[i].Name).Substring(0, DerivUnderLying[i].NameLength);
                if (DervName != Name.TrimEnd()) continue;
                Result = DerivUnderLying[i].ULToken;
                break;
            }
            return Result;
        }   //Added on 11-May-2017 - Rohan - To Add ULToken to the SpdContracts.

        public void LoadScripMasterMcxDeriv()//McxTrading 26.11.16
        {

            Master_Record = new byte[Marshal.SizeOf(new Structure.McxDeriv_Master_Record())];
            FileStream Master_Stream = new FileStream(MasterFileName, FileMode.OpenOrCreate, FileAccess.Read);
            Master_RecSize = Marshal.SizeOf(new Structure.McxDeriv_Master_Record());
            Master_FileCount = Master_Stream.Length / Master_RecSize;
            ScripData = new Structure.ScripData_Record[Master_FileCount];
            SortedExchCodes = new int[Master_FileCount];
            DerivMcxScripDataTemp = new Structure.McxDeriv_Master_Record[Master_FileCount];
            //if (frmQuoteHandler.BrokerCodeQH == frmQuoteHandler.BrokerNameRecords.BROKER_PINNACLE || frmQuoteHandler.BrokerCodeQH == frmQuoteHandler.BrokerNameRecords.BROKER_INDIRA || frmQuoteHandler.BrokerCodeQH == frmQuoteHandler.BrokerNameRecords.BROKER_EMKAY)//07.10.2010 || frmQuoteHandler.BrokerCodeQH == frmQuoteHandler.BrokerNameRecords.BROKER_FAIRWEALTH)  // Added on 24-Aug-2016 - Rohan - Pinnacle
            //{
            //    //jinal spread count
            //    CountNseFo = Master_FileCount;
            //}
            ScripCnt = (int)Master_Stream.Length / Master_RecSize;
            int k;
            for (int i = 0; i < Master_FileCount; i++)
            {
                Master_Stream.Read(Master_Record, 0, Master_RecSize);
                DerivMcxScripDataTemp[i] = MainQH.ReadUsingMarshalUnsafe<Structure.McxDeriv_Master_Record>(Master_Record);//, DerivScripDataTemp[i], 0);
                SortedExchCodes[i] = DerivMcxScripDataTemp[i].Code;
            }
            Master_Stream.Close();
            if (SortedExchCodes.Length != 0)
            {
                Array.Sort(SortedExchCodes);
                //SearchSort.QuickSort(ref SortedExchCodes);
                for (int i = 0; i < Master_FileCount; i++)
                {
                    //k = SearchSort.BinSearch(ref SortedExchCodes, DerivScripDataTemp[i].code);
                    k = Array.BinarySearch(SortedExchCodes, DerivMcxScripDataTemp[i].Code);
                    ScripData[k].Exch = DerivMcxScripDataTemp[i].Exch;
                    ScripData[k].ScripCode = DerivMcxScripDataTemp[i].Code;
                    ScripData[k].ScNameLen = DerivMcxScripDataTemp[i].NameLength;
                    ScripData[k].ScripName = DerivMcxScripDataTemp[i].Name;
                    ScripData[k].ScDescLen = DerivMcxScripDataTemp[i].DescLength;
                    ScripData[k].ScripDesc = DerivMcxScripDataTemp[i].Desc;
                    ScripData[k].ULToken = DerivMcxScripDataTemp[i].ULToken;
                    ScripData[k].LastTradeRate = 0.00f;
                    ScripData[k].LastTradeTime = 0;
                    ScripData[k].UpperCktLimit = 0;
                    ScripData[k].LowerCktLimit = 0;
                    ScripData[k].CumQty = 0;
                    ScripData[k].HasNewTrade = false;
                    ScripData[k].MBPArray = new Structure.Sh1BRecord[1];
                }
            }
            GetExpiryDates();
            GetExpiryDateCommodity();
        }


        public void ProcessSH1Rec(Structure.SH1_Record Sh1Rec)
        {
            int x = -1;
            x = Array.BinarySearch(SortedExchCodes, Sh1Rec.ExchCode);
            if (x < 0 || x > SortedExchCodes.Length)
            {
                string str = Exch + "," + ExchTypeShort + "," + Sh1Rec.ExchCode + " is not available, x = " + x;
                AccessCtl(str);
                return;
            }
            ScripData[x].LastTradeTime = Sh1Rec.RecTime;

            switch (Sh1Rec.RecType)
            {
                case 'A':
                    {
                        SH1ARec = MainQH.ReadUsingMarshalUnsafe<Structure.Sh1ARecord>(MainQH.RawSerialize(Sh1Rec));
                        ScripData[x].LastTradeRate = SH1ARec.Rate;
                        ScripData[x].CumQty = SH1ARec.CumQty;
                        ScripData[x].HasNewTrade = true;
                        ScripData[x].BidRate = SH1ARec.BidRate;
                        ScripData[x].OfferRate = SH1ARec.OfferRate;
                        if (MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_LKP) // 10.07.2015
                        {
                            ScripData[x].Today.Value = 0;   //SH1ARec.AvgTradePrize * SH1ARec.CumQty; //23.01.15    //James 25-Apr-2017
                            ScripData[x].AvgTradedPrice = SH1ARec.AvgTradePrize; //23.01.15
                            ScripData[x].OpenInt = SH1ARec.OpenInt;
                        }
                        break;
                    }

                case 'W':
                    {
                        SH1WRec = MainQH.ReadUsingMarshalUnsafe<Structure.Sh1WRecord>(MainQH.RawSerialize(Sh1Rec));
                        ScripData[x].UpperCktLimit = SH1WRec.UpperCktLimit;
                        ScripData[x].LowerCktLimit = SH1WRec.LowerCktLimit;
                        break;
                    }
                case 'B': //10.07.2015
                    {
                        if (MainQH.BrokerCodeQH == MainQH.BrokerNameRecords.BROKER_LKP)
                        {
                            SH1BRec = MainQH.ReadUsingMarshalUnsafe<Structure.Sh1BRecord>(MainQH.RawSerialize(Sh1Rec));
                            ScripData[x].MBPArray[0] = SH1BRec;
                            ScripData[x].HasNewTrade = true;
                        }
                        break;
                    }

                case 'H':
                    {
                        SH1HRec = MainQH.ReadUsingMarshalUnsafe<Structure.Sh1HRecord>(MainQH.RawSerialize(Sh1Rec));
                        ScripData[x].LastTradeRate = SH1HRec.IndexValue;
                        ScripData[x].LastTradeTime = SH1HRec.sh1Header.RecTime;
                        ScripData[x].HasNewTrade = true;
                    }
                    break;

                case 'G': //15-09-2016  //Added on 24-Oct-2016 - Rohan - Merged from Indira QH
                    {
                        SH1GRec = MainQH.ReadUsingMarshalUnsafe<Structure.Sh1GRecord>(MainQH.RawSerialize(Sh1Rec));
                        ScripData[x].Today.Open = SH1GRec.Open;
                        ScripData[x].Today.High = SH1GRec.High;
                        ScripData[x].Today.Low = SH1GRec.Low;
                        ScripData[x].HasNewTrade = true;
                    }
                    break;

            }

        }

        public int GetScripDerivRecByDesc(string StrDesc)
        {

            //     int iNDEX = Array.FindIndex(ScripData, x => x.ScripDesc.TrimEnd() == StrDesc);
            for (int i = 0; i < Master_FileCount; i++)
            {
                try
                {
                    if (StrDesc == ScripData[i].ScripDesc.Substring(0, ScripData[i].ScDescLen))
                    {
                        ScripCode = ScripData[i].ScripCode;

                        return ScripCode;
                        //break;
                    }
                }
                catch (Exception ER)
                {
                    CommonFunctions.ErrorLog(ER);
                }
            }
            return 0;
        }

        public void GetEqBeScripCodesByDesc(string StrDesc, out int ScCode1, out int ScCode2)
        {
            ScCode1 = ScCode2 = 0;
            for (int i = 0; i < Master_FileCount; i++)
            {
                if (StrDesc != ScripData[i].ScripName.Trim()) continue;

                if (ScCode1 == 0)
                    ScCode1 = ScripData[i].ScripCode;
                else
                {
                    ScCode2 = ScripData[i].ScripCode;
                    break;
                }
            }
        }

        public void AllScripReport(bool flAll)
        {
            DateTime temp = new DateTime(1980, 1, 1, 0, 0, 0);
            List<string> SL = new List<string>();
            string Str1;

            SL.Add("Exch,Code,Desc,LastTradeRate,LowerCktLimit,UpperCktLimit,CumQty,HasNewTrade,LastTradeTime");

            for (int i = 0; i < ScripData.Length; i++)
            {
                if (!flAll)
                    if (ScripData[i].CumQty <= 0) continue;

                Str1 = ScripData[i].Exch + "," + ScripData[i].ScripCode + "," + ScripData[i].ScripDesc + "," + ScripData[i].LastTradeRate + "," + ScripData[i].LowerCktLimit + "," + ScripData[i].UpperCktLimit + "," + ScripData[i].CumQty + "," + ScripData[i].HasNewTrade;
                Str1 = Str1 + "," + temp.AddSeconds(ScripData[i].LastTradeTime).ToString("dd/MM/yy:hhmmss");//FormatDateTime('yyyymmdd:hhmmss',29221 + LastRecTime/86400) ;
                SL.Add(Str1);
            }
            File.WriteAllLines(MasterFileName + ".Csv", SL);
        }
    }
}
