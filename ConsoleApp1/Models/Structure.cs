using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class Structure
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct THeaderRecord
        {
            public char MsgType;
            public ushort MsgLen;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TCustodianIds
        {
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public string CustodianId;
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public string ClientId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDataStartRecord //MsgType = 'B' or 'I'
        {
            public THeaderRecord Header;
            public int Time;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct Scrip_Master
        {
            public char Exch;
            public int EXCHCODE;
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;

            public byte[] ToByteArray() //Convert Structure To Byte
            {
                int rawsize = Marshal.SizeOf(this);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] rawdatas = new byte[rawsize];
                Marshal.Copy(buffer, rawdatas, 0, rawsize);
                Marshal.FreeHGlobal(buffer);
                return rawdatas;
            }
        }

        #region Sanchitcodeforlongname
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct Scrip_Master_New
        {
            public char Exchange;
            public int ExchangeCode;
            //Read Short Name
            public byte NameLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripNm;
            // Read Complete Name
            public byte LongLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public char[] LongName;
            public byte TickSize;
            public byte[] ToByteArray() //Convert Structure To Byte
            {
                int rawsize = Marshal.SizeOf(this);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] rawdatas = new byte[rawsize];
                Marshal.Copy(buffer, rawdatas, 0, rawsize);
                Marshal.FreeHGlobal(buffer);
                return rawdatas;
            }
        }


        #endregion

        #region New Structure for Series - Rohan - 08-Nov-2017 - With reseved fields
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct Scrip_Master_New_Series
        {
            public char Exchange;
            public int ExchangeCode;
            // public string IndexName;
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte LongLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public char[] LongName;
            public byte TickSize;
            public byte SeriesLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public long MinimumLotQty;
            public int Reserved1;
            public char Reserved2;
            public byte Reserved3;
            public byte[] ToByteArray() //Convert Structure To Byte
            {
                int rawsize = Marshal.SizeOf(this);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] rawdatas = new byte[rawsize];
                Marshal.Copy(buffer, rawdatas, 0, rawsize);
                Marshal.FreeHGlobal(buffer);
                return rawdatas;
            }
        }
        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct Indices_struct
        {
            public int ExchCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string Name;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct Bse_Scrip_Rec
        {
            public int EXCHCODE;
            public int MktLot;
            public short Filler1;
            public short InPreOpen;
            public short Filler3;
            public short InBCast;
            public short Filler4;
            public short AVMBuyMargin;
            public short AVMSellMargin;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string ScripId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public char[] GroupName;
            public char NDFlag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public char[] NDStartDt;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public char[] NDEndDt;
            public char SuspStatus;
            public char Filler5;
            public char AVMFlag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public char[] Filler6;
        }

        //jinal Spread
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Spread_Master_Record //New Structure
        {
            public char Exch;//
            public int Code;//
            public int Spd_Code1;//
            public int Spd_Code2;//
            public byte NameLength;//
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string Name;//
            public byte DescLength;//
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string Desc;//
            public int ULToken;
            public byte OFISType1;//
            public byte OFISType2;//
            public int ExpiryDate1;//
            public int ExpiryDate2;//
            public Single StrikePrice1;
            public Single StrikePrice2;
            public Int16 CALevel1;
            public Int16 CALevel2;
            public long MinimumLotQuantity1; //
            public long MinimumLotQuantity2; //
            public short Ticksize1; //leg 1 contract
            public short Ticksize2; //leg 2 contract
            public byte Reserved1;
            public Single Reserved2;
            public int Reserved3;

            public byte[] ToByteArray()
            {
                int rawsize = Marshal.SizeOf(this);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] rawdatas = new byte[rawsize];
                Marshal.Copy(buffer, rawdatas, 0, rawsize);
                Marshal.FreeHGlobal(buffer);
                return rawdatas;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]//For FPOS File
        public struct TScripDerivRecord
        {
            public char exch;
            public int code;
            public byte Length;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string name;
            public int ULToken;
            public byte OFISType;
            public byte CPType;
            public int ExpiryDate;
            public Single StrikePrice;
            public UInt16 CALevel;
            public UInt16 MinimumLotQuantity;
            public byte Desc_Length;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string Desc;   //51 bytes
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct IMLReq //for requesting BSE to send data from Current Time
        {
            public char Exch;
            public ushort Msgline;
            public int DateTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Equity_Master_Record //18 bytes From BSE
        {
            public char Exch;
            public int ExchCode;
            public byte Length;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string Name;
        }

        #region New Structure for Series - Rohan - 08-Nov-2017 - With reseved fields
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Equity_Master_Record_New
        {
            public char Exchange;
            public int ExchangeCode;
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte LongLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public char[] LongName;
            public byte TickSize;
            public byte SeriesLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public long MinimumLotQty;
            public int Reserved1;
            public char Reserved2;
            public byte Reserved3;
        }
        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //23.08.18 Priyanka for Compression
        public struct TIQSReplyHeaderRecord
        {
            public byte Compressed;
            public int ReplyLen;
            public ushort ReplyRecSize;
            public ushort ReplyCnt;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct SH1_Record_Header //30 bytes From Eqy
        {
            public char Exch;
            public int ExchCode;
            public int RecTime;
            public char RecType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct SH1_Record //30 bytes From Eqy
        {
            public char Exch;
            public int ExchCode;
            public int RecTime;
            public char RecType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Junk;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Sh1ARecord //30 bytes From Eqy
        {
            public SH1_Record_Header sh1Header;
            public Single Rate;
            public int Qty;
            public int CumQty;
            public Single AvgTradePrize;
            public int OpenInt;
            public Single BidRate;
            public Single OfferRate;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Sh1WRecord //30 bytes From Eqy
        {
            public SH1_Record_Header sh1Header;
            public Single UpperCktLimit;
            public Single LowerCktLimit;
            public int FreezQty;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            byte[] padding;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Sh1GRecord //30 bytes From Eqy    //Added on 24-Oct-2016 - Rohan Merging from Indira QH
        {
            public SH1_Record_Header sh1Header;
            public Single Open;
            public Single High;
            public Single Low;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            byte[] padding;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Sh1HRecord //30 bytes From Eqy
        {
            public SH1_Record_Header sh1Header;
            public Single IndexValue;
            public int ExchQty;
            public Single ExchValue;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] padding;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Sh1GRecord52Wk //30 bytes From Eqy    //Added on 24-Oct-2016 - Rohan Merging from Indira QH
        {
            public SH1_Record_Header sh1Header;
            public Single YearlyHigh;
            public Single YearlyLow;
            public byte TickSize;
            public byte AllowedIntraday;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            byte[] padding;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct NCDEX_Deriv_Master_Record //New Structure  08.12.17
        {
            public char Exch;
            public int ScripCode;
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string Symbol;
            public int ULToken;
            public byte OFISType;
            public byte CPType;
            public int ExpiryDate;
            public Single StrikePrice;
            public Int16 CALevel;
            public long MinimumLotQuantity; //02.09.15
            public byte UnitsLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string Units;
            public byte DescLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string Desc;
            public byte Reserved1;//17.11.17
            public byte Reserved2;//17.11.17
            public int Reserved3; //17.11.17
            public int Reserved4;//17.11.17            
            public long Reserved5;//17.11.17
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Reserved6;//17.11.17
            public byte[] ToByteArray()
            {
                int rawsize = Marshal.SizeOf(this);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] rawdatas = new byte[rawsize];
                Marshal.Copy(buffer, rawdatas, 0, rawsize);
                Marshal.FreeHGlobal(buffer);
                return rawdatas;
            }
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Deriv_Master_Record //New Structure
        {
            public char Exch;
            public int Code;
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string Name;
            public int ULToken;
            public byte OFISType;
            public byte CPType;
            public int ExpiryDate;
            public Single StrikePrice;
            public Int16 CALevel;
            public long MinimumLotQuantity; //02.09.15
            public byte DescLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string Desc;
            public byte Reserved1;
            public Single Reserved2;
            public int Reserved3;

            public byte[] ToByteArray()
            {
                int rawsize = Marshal.SizeOf(this);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] rawdatas = new byte[rawsize];
                Marshal.Copy(buffer, rawdatas, 0, rawsize);
                Marshal.FreeHGlobal(buffer);
                return rawdatas;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct ScripData_Record  //For Master Array 
        {
            public char Exch;
            public int ScripCode;
            public byte ScNameLen;
            public string ScripName;

            public byte ScDescLen;
            public string ScripDesc;

            public char Exchtype;
            public string Series;
            public int Expiry;
            public char Allowed;
            public byte CPType;
            public Single StrikeRate;
            public char TT;
            public int TickSize;

            public Single LastTradeRate;
            public int LastTradeTime;
            public Single UpperCktLimit;
            public Single LowerCktLimit;
            public int CumQty;

            public int ULToken;
            public byte OFISType;
            public UInt16 MinimumLotQuantity;
            public UInt16 CALevel;
            public bool HasNewTrade;
            public Single AvgTradedPrice; //23.01.15
            public TDailyRecord Today; //23.01.15

            public Sh1BRecord[] MBPArray;  // 10.07.2015 
            public int OpenInt;   //Added on 01-Apr-2017 - Rohan
            public int DerivULPos;

            //BseMf Krutika
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public byte RTASchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] RTASchemeCode;
            public byte AMCSchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] AMCSchemeCode;
            public byte ISINLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ISIN;
            public byte AMCCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] AMCCode;
            public byte SchemeTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeType;
            public byte SchemePlanLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] SchemePlan;
            public byte SchemeNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] SchemeName;
            public char PurchaseAllowed;
            public double MinimumPurchaseAmt;
            public double MaximumPurchaseAmt;
            public double PurchaseAmountMultiplier;
            public byte PurchaseCutoffTimeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] PurchaseCutoffTime;
            public char RedemptionAllowed;
            public double MinimumRedemptionQty;
            public double MaximumRedemptionQty;
            public double RedemptionAmtMin;
            public double RedemptionAmtMax;
            public char DividendReinvestmentFlag;
            public char SIPFlag;
            public char STPFlag;
            public char SWPFlag;
            public char SwitchFlag;
            public double NavValue;
            public double BidRate;
            public double OfferRate;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Sh1BRecord //30 bytes From Eqy  // 10.07.2015 
        {
            public SH1_Record_Header sh1Header;
            public Single BidRate;
            public int BidQty;
            public ushort BidOrder;
            public Single OfferRate;
            public int OfferQty;
            public ushort OfferOrder;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDailyRecord
        {
            public int Trades;
            public Single Value;
            public Single Open;
            public Single High;
            public ushort Date;
            public Single Low;
            public Single Close;
            public int Qty;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct BseScripDetailMasterRecord  //For Master Array 
        {
            public int ScripCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 208)]
            public char[] Filler6;
            public int TickSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TickSizeRecord  //For Master Array 
        {
            public int ScripCode;
            public int TickSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TClientBalanceRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientCode;
            public double Balance;
            public double Balance2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct EODRequestHeaderRecord
        {
            public byte IDlength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientID;
            public byte RequestType;
            public ushort RequestLen;

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct McxDeriv_Master_Record //New Structure
        {
            public char Exch;
            public int Code;
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string Name;
            public int ULToken;
            public byte OFISType;
            public byte CPType;
            public int ExpiryDate;
            public Single StrikePrice;
            public ushort CALevel;
            public long MinimumLotQuantity; //02.09.15
            public byte DescLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string Desc;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string Units;
            public byte UnitLength;
            public int Start;
            public int EndDate;
            public int DelStart;
            public int DelEnd;
            public int TenderStart;
            public int TenderEnd;
            public int MaxSingleValue;
            public long MaxSingleQty;
            public Single Factor;
            public byte T2TIndicator;
            public double BuyMargin;
            public double SellMargin;
            public byte Reserved1; // no of unit
            public short Reserved2;
            public short GroupID;   //Added on 13-Dec-2016 - Rohan
            public int Reserved3;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string Units1;
            public byte UnitLength1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string Units2;
            public byte UnitLength2;

            public byte[] ToByteArray()
            {
                int rawsize = Marshal.SizeOf(this);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] rawdatas = new byte[rawsize];
                Marshal.Copy(buffer, rawdatas, 0, rawsize);
                Marshal.FreeHGlobal(buffer);
                return rawdatas;
            }
        }

        #region Bse_Mf Krutika
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct BseMFCodeMasterDetails //New Structure
        {
            public char Exch;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public byte RTASchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] RTASchemeCode;
            public byte AMCSchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] AMCSchemeCode;
            public byte ISINLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ISIN;
            public byte AMCCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] AMCCode;
            public byte SchemeTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeType;
            public byte SchemePlanLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] SchemePlan;
            public byte SchemeNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] SchemeName;
            public char PurchaseAllowed;
            public double MinimumPurchaseAmt;
            public double MaximumPurchaseAmt;
            public double PurchaseAmountMultiplier;
            public byte PurchaseCutoffTimeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] PurchaseCutoffTime;
            public char RedemptionAllowed;
            public double MinimumRedemptionQty;
            public double MaximumRedemptionQty;
            public double RedemptionAmtMin;
            public double RedemptionAmtMax;
            public char DividendReinvestmentFlag;
            public char SIPFlag;
            public char STPFlag;
            public char SWPFlag;
            public char SwitchFlag;
            public double NavValue;

            public byte[] ToByteArray()
            {
                int rawsize = Marshal.SizeOf(this);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] rawdatas = new byte[rawsize];
                Marshal.Copy(buffer, rawdatas, 0, rawsize);
                Marshal.FreeHGlobal(buffer);
                return rawdatas;
            }
        }
        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TNseMasterInfoRecord
        {
            public int Token;
            public byte SymbolLen;
            public char[] Symbol;
            public byte SeriesLen;
            public char[] Series;
        }

        #region Structure For Span File Reading
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct FillRecord
        {
            public string Name, Symbol;
            public string Instrument, Expiry, exMonth;
            public Single Price, spanMargin, expPrice, spanPercent, SOMC, SOMCPercent, SpecialBuyMargin, SpecialSellMargin, InitialMargin, TenderMargin, AdditionalLongMargin, AdditionShortMargin, ELMLong, ELMShort, DeliveryMargin;
        }
        #endregion

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //14.02.17 Priyanka
        public struct TDerivUnderlyingRecord
        {
            public int ULToken;
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] Name;
            public byte IsWeeklyContractSymbol;//28.05.16
            public SGExpiryDetailRec[] WeeklyExpiryRecs;
            public int WeeklyExpiriesCnt;
            public int[] WeeklyExpiries;
            public int[] MonthlyExpiries;
            public int[] AllExpiries;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//14.02.17 Priyanka
        public struct SGExpiryDetailRec
        {
            public int Month;
            public int[] MonthWiseWeeklyExpiryRecs;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TGroupRecord
        {
            public char GrType;
            public byte GrCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public char[] GrCode;
            public byte GrNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public char[] GrName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMemberRecord
        {
            public char Exch;
            public char ExchType;
            public int Code;
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Name;
            public char GrType;
            public byte GrCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public char[] GrCode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct Expiry
        {
            public char Exch;
            public char InstrType;
            public int ULToken;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Symbol;
            public int expiry;
            public int Sccode;
            public byte OFISType; //10.03.17 Priyanka
            public byte CPType; //10.03.17 Priyanka
            public Single StrikePrice; //10.03.17 Priyanka
            public int DerivCode; //06.05.17 Priyanka
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//28.05.16
        public struct TCommodityExpiry
        {
            public byte NameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string Name;
            public List<int> EDateList;
            public List<int> WeeklyExpiryList;
            public int ULToken;

        }

        public class CoCodeMaster
        {
            private string _sectorname = null;
            public double co_code { get; set; }
            public string bsecode { get; set; }
            public string nsesymbol { get; set; }
            public string companyname { get; set; }
            public string companyshortname { get; set; }
            public string categoryname { get; set; }
            public string isin { get; set; }
            public string bsegroup { get; set; }
            public string mcaptype { get; set; }
            public string sectorcode { get; set; }
            public string sectorname
            {
                get => _sectorname;
                set => _sectorname = value?.Replace(',', '-');
            }
            public string industrycode { get; set; }
            public string industryname { get; set; }
            public string bselistedflag { get; set; }
            public string nselistedflag { get; set; }
            public string DisplayType { get; set; }
        }

        public class CompanyMasterReply
        {
            public bool success { get; set; }
            public List<CoCodeMaster> data { get; set; }
        }
    }
}
