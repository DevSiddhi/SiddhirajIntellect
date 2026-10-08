using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace OTS
{
    public class ITInteractiveStructures
    {
        public const long NSE_Internet_ID = 111111111111100;
        public const long NSE_Cash_Internet_UserInfo = 111111111111100;
        public const long NSE_Deriv_Internet_UserInfo = 111111111111100;
        public const long BSE_Cash_Internet_UserInfo = 1111111111111088;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TScripDerivRecord //New Structure
        {
            public char exch;
            public int code;
            public byte Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] name;
            public int ULToken;
            public byte OFISType;
            public byte CPType;
            public int ExpiryDate;
            public Single StrikePrice;
            public ushort CALevel;
            public long MinimumLotQuantity;
            public byte DescLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Desc;
            public byte Reserved1;
            public Single Reserved2;
            public int Reserved3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TIARequestHeaderRecord
        {
            public ushort SocketNum;
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] TraderID;
            public byte AccountType;
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte ClientType;
            public byte RequestType;
            public ushort RequestLen;
            public Int64 ChannelID_NC;
            public Int64 ChannelID_ND;
            public Int64 ChannelID_BC;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TIAReplyHeaderRecord
        {
            public ushort SocketNum;
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] TraderID;
            public byte AccountType;
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte ClientType;
            public byte ReplyType;
            public int ReplyLen;
            public ushort ReplyRecSize;
            public ushort ReplyCnt;
            public ushort ErrorCode;
            public long ChannelID_NC;
            public long ChannelID_ND;
            public long ChannelID_BC;
        }



        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TIARequestHeader2Record
        {
            //SocketNum      : Word ;
            public char[] TraderID;
            //AccountType    : Byte ;
            //FEUserID       : string[15] ;
            //ClientType     : Byte ;
            public byte RequestType;
            public ushort RequestLen;
            //ChannelID_NC   : Int64 ;
            //ChannelID_ND   : Int64 ;
            //ChannelID_BC   : Int64 ;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTemplateMarginRequestRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public int SegmentId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTemplateMarginRequestRecord_Peak
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public int SegmentId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTemplateMarginRequestRecordNew //Derivative Margin 20.10.22
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public int SegmentId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TPeakMarginData
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] FEUserID;
            public int SegmentId;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TIAReplyHeader2Record
        {

            //SocketNum      : Word ;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] TraderID;
            //AccountType    : Byte ;
            //FEUserID       : string[15] ;
            //ClientType     : Byte ;
            public byte ReplyType;
            public int ReplyLen;
            public ushort ReplyRecSize;
            public ushort ReplyCnt;
            public ushort ErrorCode;
            //ChannelID_NC   : Int64 ;
            //ChannelID_ND   : Int64 ;
            //ChannelID_BC   : Int64 ;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TIAErrorMessageRecord
        {
            public byte ErrorStrLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
            public string ErrorStr;
        }

        //changed on 2.04.2012 for PAN and DOB
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TLoginRequestRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte Password1Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] Password1;
            public byte Password2Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] Password2;
            public byte VersionLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Version;
            public int Dummy1;
            public ushort Dummy2;
            public byte ChkType1;   //1 for Pan ; 2 for Date of Birth in ChkType1Info
            public byte ChkType1InfoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] ChkType1Info;
            public byte ChkType2;
            public byte ChkType2InfoLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string ChkType2Info;
            public byte ClientIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientIP;
            public byte ServerIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ServerIP;
            public byte ITS_IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ITS_IP;
            public byte MachineIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MachineID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TLoginRequestRecordTwoFactor
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] FEUserID;


            public byte Password1Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] Password1;
            public byte PANLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] PAN;
            public byte DOBLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] DOB;


            public int OTP;
            public int MPin;

            public byte VersionLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Version;

            public int Dummy1;
            public ushort Dummy2;

            public int Journey;



            public byte ClientIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientIP;
            public byte ServerIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ServerIP;
            public byte ITS_IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ITS_IP;
            public byte DeviceIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] DeviceID;
            public byte DeviceTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public char[] DeviceType;

            public byte SessionIdLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] SessionId;


            public int NewPin;
            public int Reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public Char[] Reserved3;
            public byte Reserved4;
            public byte Reserved5;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TLogoutRequestRecord
        {
            public byte ClientIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientIP;
            public byte ServerIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ServerIP;
            public byte ITS_IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ITS_IP;
            public byte MachineIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MachineID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TLoginReplyRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] FEUserID;
            public byte AccountType;
            public byte FEUserType;
            public int SystemDateTime;
            public int BadLoginCnt;
            public int LastModifiedTime;
            public int LastAccessedTime;
            public byte AllowNseCash;
            public byte AllowNseDeriv;
            public byte AllowBseCash;
            public ushort BranchID;
            public long ChannelID_NC;
            public long ChannelID_ND;
            public long ChannelID_BC;
            public byte DbNum;
            public byte AllowTotalBuySellQty;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public int[] Junk;
            public char AllowFastFeed;
            public char AllowHoldingsMargin;//03/08/2012
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]//03/08/2012
            public char[] Junk2;
            public byte AddMsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string AddMsg;
            public byte PopupMsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
            public string PopupMsg;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TAllowSegmentReply
        {
            public char AllowNseEquity;
            public char AllowNseDeriv;
            public char AllowAllEquityDerivCombine;
            public char AllowNseCurr;
            public char AllowDerivCombine;
            public char AllowMCX;
            public char AllowNCDEX;
            public char AllowCommCombine;
            public char AllowBseEquity;
            public char AllowEquityCombine;
            public char AllowBseCurr;
            public char AllowCurrCombine;
            public char Reserved1;
            public char Reserved2;
            public char Reserved3;
        }

         [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TLoginReplyRecordNew                                             //Request & Reply =  3 - LOGIN 04/12/2013
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] FEUserID;
            public byte AccountType;
            public byte FEUserType;
            public int SystemDateTime;
            public int BadLoginCnt;
            public int LastModifiedTime;
            public int LastAccessedTime;
            public byte AllowNseCash;
            public byte AllowNseDeriv;
            public byte AllowBseCash;
            public byte AllowNseCurrency;
            public byte AllowMcxDeriv;//Mcx Changes
            public byte AllowBseCurr;
            public byte AllowNseSLBM;
            public byte AllowBseDeriv;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Reserved2;

            public ushort BranchID;
            public long ChannelID_NC;
            public long ChannelID_ND;
            public long ChannelID_BC;

            public long ChannelID_CD;
            public long ChannelID_MD;//MCX Changes
            public long ChannelID_ED;
            //public long Reserved3;//
            public long ChannelID_SLBM;
            public long ChannelID_BD;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public long[] Reserved4;

            public byte DbNum;
            public byte AllowTotalBuySellQty;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public int[] Junk;
            public char AllowFastFeed;
            public char AllowHoldingsMargin;//03/08/2012
            public char DealerType; // 15.10.2016 Hetal
            public char Junk2; // 15.10.2016 Hetal
            public byte AddMsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string AddMsg;
            public byte PopupMsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
            public string PopupMsg;
            public byte AllowMarginTrading; //11.09.17 Sanchit
            public byte AllowBseMf;//BseMf Krutika
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] Reserved5;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TTwoFactorLoginReplyRecord                                       //Request & Reply =  85 - LOGIN 04/12/2013
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] FEUserID;
            public byte AccountType;
            public byte FEUserType;
            public int SystemDateTime;
            public int BadLoginCnt;
            public int LastModifiedTime;
            public int LastAccessedTime;
            public byte AllowNseCash;
            public byte AllowNseDeriv;
            public byte AllowBseCash;
            public byte AllowNseCurrency;
            public byte AllowMcxDeriv;//Mcx Changes
            public byte AllowBseCurr;
            public byte AllowNseSLBM;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserved2;

            public ushort BranchID;
            public long ChannelID_NC;
            public long ChannelID_ND;
            public long ChannelID_BC;

            public long ChannelID_CD;
            public long ChannelID_MD;//MCX Changes
            public long ChannelID_ED;
            //public long Reserved3;//
            public long ChannelID_SLBM;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public long[] Reserved4;

            public byte DbNum;
            public byte AllowTotalBuySellQty;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public int[] Junk;
            public char AllowFastFeed;
            public char AllowHoldingsMargin;//03/08/2012
            public char DealerType; // 15.10.2016 Hetal
            public char Junk2; // 15.10.2016 Hetal
            public byte AddMsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string AddMsg;
            public byte PopupMsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
            public string PopupMsg;
            public byte AllowMarginTrading; //11.09.17 Sanchit
            public byte AllowBseMf;//BseMf Krutika
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] Reserved5;
            public int Journey;

        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TLogoutUserRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] FEUserID;
            public byte ITS_Num;
            public byte IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] IP;
        }
        //BOB Junk Changes 26.08.23
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TLogoutRequestRecord_2FA
        {
            public byte ClientIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientIP;
            public byte ServerIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ServerIP;
            public byte ITS_IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ITS_IP;
            public byte MachineIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] MachineID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TAHOrderRejectedRecord  //16 on 30/10/2012 for AH Rejected order
        {
            public int BrokerOrderID;
            public byte ErrorMsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string ErrorMsg;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TEquityOrderPlaceRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public byte IOC;
            public int DisclosedQty;
            public int AlgoID;        //Added by Rhythm 02-01-24
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TEquityOrderPlaceBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            public int AlgoID;        //Added by Rhythm 02-01-24
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]                    // cover place sumit p   // sumitp 28_jan_22
        public struct TEquityOrderPlaceBrokerReplyRecord_cover
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;

            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;

            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;

            public byte OrderType;
            public byte ProductType;

            public int Reserved1;
            public int Reserved2;

            public byte OrderTypeNew; // Co/Bo type new
            public Single SLRateCO;//Rate for CO
            public long EpochTime; //  EpochTime for order identification COVER

        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TEquityOrderModifyRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;  //bse - diff req for sl before/after trigger. diff from limit/mkt order
            public char SLTriggered;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            public int AlgoID;        //Added by Rhythm 02-01-24
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TEquityOrderModifyBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public char SLTriggered;
            public Single TriggerRate;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public ushort TraderRequestID;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            public int AlgoID;        //Added by Rhythm 02-01-24
            public int Reserved1;
            public int Reserved2;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]                       // cover modify sumit p   // sumitp 28_jan_22
        public struct TEquityOrderModifyBrokerReplyRecord_Cover
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public char SLTriggered;
            public Single TriggerRate;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public ushort TraderRequestID;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;

            public byte OrderTypeNew; // Co/Bo type new
            public Single SLRateCO;//Rate for CO
            public long EpochTime; //  EpochTime for order identification COVER

        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TEquityOrderCancelRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;  //bse - diff req for sl before/after trigger. diff from limit/mkt order
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public ushort TraderRequestID;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public char AHStatus;
            public int AlgoID;        //Added by Rhythm 02-01-24
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TEquityOrderCancelBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;  //bse - diff req for sl before/after trigger. diff from limit/mkt order
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public ushort TraderRequestID;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public char AHStatus;
            public int AlgoID;        //Added by Rhythm 02-01-24
            public int Reserved1;
            public int Reserved2;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TExchTradeConfirmationRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int OrgQty;
            public int PendingQty;
            public long ExchangeOrderID;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TSLTriggerRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public int PendingQty;
            public Single Rate;
            public long ExchangeOrderID;
            public int ExchangeTradeTime;
            public int ExchReplyCode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TMarketToLimitRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public Single Rate;
            public long ExchangeOrderID;
            public int ExchangeTradeTime;
            public int ExchReplyCode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TKillOrderExchReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public long ExchangeOrderID;
            public int KilledQty;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TEquityOrderExchReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchReasonCode;
            public int ExchErrorCode;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status;
            public byte IOC;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderReportRequestRecord
        {
            public int StartTime;
            public int EndTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderReportReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char AHStatus;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]                       // added by Himanshu 25-nov-21        // sumitp 28_jan_22
        public struct TOrderReportReplyRecordCoverOrBracket
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public byte OrderTypeNew;  // add By Himanshu
            public int BrokerOrderID1; // add By Himanshu
            public int BrokerOrderID2; // add By Himanshu
            public int EpochTime;      // add By Himanshu
            public int Reserved1;
            public int Reserved2;
            public bool IsSpread;
            public int ScripCode2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]         // 58 cover trade report    // sumitp 28_jan_22
        public struct TTradeReportReplyRecNewCover
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int OrgQty;
            public int PendingQty;
            public long ExchangeOrderID;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;

            public byte OrderType;
            public byte ProductType;


            public byte OrderTypeNew;
            public int EpochTime;

            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public int Reserved4;
            public bool IsSpread;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TTradeReportRequestRecord
        {
            public int StartTime;
            public int EndTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TTradeReportReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int OrgQty;
            public int PendingQty;
            public long ExchangeOrderID;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TAHOrderReportRequestRecord
        {
            public int StartTime;
            public int EndTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TAHOrderReportReplyRecord
        {
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string TraderID;
            public byte AccountType;
            public byte OrderRequesterIDLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string OrderRequesterID;
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public int DisclosedQty;
            //added on 25/07/2007
            public long ChannelID_NC;
            public long ChannelID_ND;
            public long ChannelID_BC;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TAHEquityOrderPlaceRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public int DisclosedQty;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TAHDerivOrderPlaceRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public int DisclosedQty;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivInfo2Record //With Series
        {
            public byte InstrType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Symbol;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Series;
            public int ExpiryDate;//Seconds from 01/01/80
            public Single StrikePrice;
            public byte CPType;
            public byte CALevel;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivOrderPlaceRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public byte IOC;
            public int AlgoID;        //Added by Rhythm 28-02-24
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivOrderPlaceBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public byte IOC;
            public char AHStatus;
            public int AlgoID;        //Added by Rhythm 28-02-24
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivOrderPlaceBrokerReplyRecordCover
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public byte IOC;
            public char AHStatus;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivOrderModifyRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public char SLTriggered;
            public int OldQty;
            public char AHStatus;
            public byte IOC;
            public int TradedQty;
            public int PendingQty;
            public Int64 ExchangeOrderId;
            public int ExchangeOrderTime;
            public int AlgoID;        //Added by Rhythm 28-02-24
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivOrderModifyBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public char SLTriggered;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public char AHStatus;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int AlgoID;        //Added by Rhythm 28-02-24
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivOrderCancelRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public char SLTriggered;
            public int OldQty;
            public char AHStatus;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int AlgoID;        //Added by Rhythm 28-02-24
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivOrderCancelBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public char SLTriggered;
            public int OldQty;
            public int TradedQty;
            public char AHStatus;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public int AlgoID;        //Added by Rhythm 28-02-24
        }

        //31.12.14 Structure took from EC - Sanchit // Previous Status Length field missing
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivOrderExchReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchReasonCode;
            public int ExchErrorCode;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status;

            //public TDerivOrderExchReplyRecord(char c)
            //{
            //    Exch = c;
            //    ExchType = c;
            //    ScripCode = 0;
            //    DerivInfo = new TDerivInfoRecord(c);
            //    BuySell = c;
            //    Qty = 0;
            //    AtMarket = c;
            //    Rate = 0;
            //    WithSL = c;
            //    TriggerRate = 0;
            //    TradedQty = 0;
            //    PendingQty = 0;
            //    BrokerOrderID = 0;
            //    BrokerOrderTime = 0;
            //    ExchangeOrderId = 0;
            //    ExchangeOrderTime = 0;
            //    ExchangeOrderId = 0;
            //    ExchReplyCode = 0;
            //    ExchReasonCode = 0;
            //    ExchErrorCode = 0;
            //    Status = new char[20];
            //    for (int i = 0; i < 20; i++)
            //        Status[i] = c;
            //}
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivExchTradeConfirmationRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int OrgQty;
            public int PendingQty;
            public long ExchangeOrderID;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TDerivSLTriggerRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public int PendingQty;
            public Single Rate;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TActivityLogRequestRecord
        {
            public int StartTime;
            public int EndTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TActivityLogRequestRecord_new
        {
            public int StartTime;
            public int EndTime;
            public char Exch;
            public int ScripCode;
            public char ExchType;
            public long ExchangeOrderID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
            public string Status;
            public int BrokerOrderID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TActivityLogReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int BrokerOrderID; //of Original Place Order
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 19)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char ReqType; //changed from 20 to 19 on 9/2/5 and added ReqType
            public int OldOrderQty;
            public int TradedQty;
            public int PendingQty;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TChangePasswordRequestRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte Password1Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] Password1;
            public byte Password2Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] Password2;
            public byte NewPassword1Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] NewPassword1;
            public byte NewPassword2Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] NewPassword2;
            public byte ClientIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientIP;
            public byte ServerIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ServerIP;
            public byte ITS_IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ITS_IP;
            public byte MachineIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MachineID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TChangePasswordReplyRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte Status;
            public byte StatusStrLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string StatusStr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTraderIDRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TFEUserIDRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDummyRecord
        {
            public byte DummyLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] Dummy;
            public int DummyInt;
            public byte DummyByte;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMarginReportReplyRecordCombined
        {
            public double OpeningMargin;
            public double AdditionalMargin;
            public double MarginForOpenPos;
            public double MarginForPendOrder;
            public double AvailableMargin;
            public double LedgerBalance;
            public double T1Short;
            public double T2Short;
            public double Withdrawable;
            public double MFdebit;
            public double FOBalance;
            public double FOMgnPayable;
            public double FOExcessCol;
            public double HoldingsMargin;//03/08/2012
            public double Junk1;//03/08/2012
            public double Junk2;//03/08/201


            public double OpeningMargin2;
            public double AdditionalMargin2;
            public double MarginForOpenPos2;
            public double MarginForPendOrder2;
            public double AvailableMargin2;
            public double LedgerBalance2;
            public double T1Short2;
            public double T2Short2;
            public double Withdrawable2;
            public double MFdebit2;
            public double FOBalance2;
            public double FOMgnPayable2;
            public double FOExcessCol2;
            public double HoldingsMargin2;
            public double Junk3;
            public double Junk4;
            public double Junk5;
            public double Junk6;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMarginReportReplyRecord
        {
            public double OpeningMargin;
            public double AdditionalMargin;
            public double MarginForOpenPos;
            public double MarginForPendOrder;
            public double AvailableMargin;
            public double LedgerBalance;
            public double T1Short;
            public double T2Short;
            public double Withdrawable;
            public double MFdebit;
            public double FOBalance;
            public double FOMgnPayable;
            public double FOExcessCol;
            public double HoldingsMargin;//03/08/2012
            public double Junk1;//03/08/2012
            public double Junk2;//03/08/2012
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //Jigar for template margin
        public struct TMarginReportReplyRecordTemplate 
        {
            public double OpeningMargin;
            public double Adhoc;
            public double Payments;
            public double HoldingCFS;
            public double OptionCFS;
            public double MBPO;
            public double MarginForOpenPos;
            public double AvailableMargin;          
            public double Junk1;
            public double Junk2;
            public int Junk3;
            public float Junk4;  
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //Jigar for template margin
        public struct TMarginReportReplyRecordTemplate_Peak
        {
            public double OpeningMargin;
            public double Adhoc;
            public double Payments;
            public double HoldingCFS;
            public double OptionCFS;
            public double MBPO;
            public double MarginForOpenPos;
            public double AvailableMargin;
            public double PeakMargin;
            public double Junk1;
            public double Junk2;
            public int Junk3;
            public float Junk4;

            public double DerivativeMargin;
            public double Reserved1;
            public double Reserved2;
            public double Reserved3;
            public double Reserved4;
            public double Reserved5;
            public float Reserved6;
            public float Reserved7;
            public int Reserved8;

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //Added for Derivative Margin 20.10.22
        public struct TMarginReportReplyRecordTemplateWithDerivMargin
        {
            public double OpeningMargin;
            public double Adhoc;
            public double Payments;
            public double HoldingCFS;
            public double OptionCFS;
            public double MBPO;
            public double MarginForOpenPos;
            public double AvailableMargin;
            public double PeakMargin;
            public double Junk1;
            public double Junk2;
            public int Junk3;
            public float Junk4;

            public double DerivativeMargin;
            public double Reserved1;
            public double Reserved2;
            public double Reserved3;
            public double Reserved4;
            public double Reserved5;
            public float Reserved6;
            public float Reserved7;
            public int Reserved8;

        }

        #region MTM - Added on 12-Sep-2016 - Rohan
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//21.06.2016 Pranoti
        public struct TAllClientMarginReportReplyRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public double OpeningMargin;
            public double AdditionalMargin;
            public double MarginForOpenPos;
            public double MarginForPendOrder;
            public double AvailableMargin;
            public double Adhoc;
            public double Receipts;
            public double Payments;
            public double LedgerBalance;
            public double HoldingMargin;
            public double Reserved;
            public double Reserved1;
            public double Reserved2;
            public double Reserved3;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//21.06.2016 Pranoti
        public struct ClientsForMTMSquareOff
        {
            public byte FELength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte Adhoc;
            public byte Receipts;
            public byte Payments;
            public byte LedgerBalance;
            public byte HoldingMargin;
            public byte SkipFromMTM;
            public byte MBPO;
            public byte MBOP;
            public byte Reserved;
            public byte Reserved1;
            public byte Reserved2;
            public byte Reserved3;
            public byte Reserved4;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //12-Sep-2016 - Rohan
        public struct MTMClientsWithAllMargins
        {
            public byte FELength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public double AvailableMargin;
            public double MBPO;
            public double MBOP;
            public double DerivMarginBlocked;
            public double HoldingMargin;
            public double LedgerBalance;
            public double AdHoc;
            public double Receipts;
            public double Payments;
            public byte SkipFromMTM;
            public double MTMLimitAmount;
            public byte MTMLimitHit;
            public long MaxLastUpdatedMarginTime;
            public long MaxLastUpdatedClientsTime;
            public long LastValMargin;
            public long LastValClients;
            public double TotalMtmPL;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //Atish 25.03.2019
        public struct MTMClientsWithAllMarginsInstrumentWise
        {
            public char Flag;
            public byte FELength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public double AvailableMargin;
            public double MBPO;
            public double MBOP;
            public double DerivMarginBlocked;
            public double HoldingMargin;
            public double LedgerBalance;
            public double AdHoc;
            //public double Receipts;
            public double Payments;
            //public byte SkipFromMTM;
            //public byte MTMLimitHit;
            public long MaxLastUpdatedMarginTime;
            //public long MaxLastUpdatedClientsTime;
            //public long LastValMargin;
            //public long LastValClients;
            //public double Bought;
            //public double Sold;
            //public double GrossTraded;
            //public double Exposure;
            public double MTMLimitSet;//Atish 18.01.2019
            public double BookedPL;
            public double MTMPL;
            //public double RealTimeMTMPL;
            //public double MTMValue;
            public double MTMLoss;
            public double MTMProfit;//Atish 28.02.2019
            public double BookedProfit;//Atish 28.02.2019
            public double BookedLoss;//Atish 28.02.2019
            //public double TotalValueForMTM;
            public ushort SegmentId;
            //public Single PopUpPercent;
            public Single SqOffModePercent;
            public double GrossExposureLimit;
            public double NetPurchaseLimit;
            public double NetSaleLimit;
            public double NetQty;
            public double NetPositionLimit;
            public double TurnOverLimit;
            public double MarginLimit;
            public double GrossExposureLimitUtilized;
            public double NetPurchaseLimitUtilized;
            public double NetSaleLimitUtilized;
            public double NetQtyUtilized;
            public double NetPositionLimitUtilized;
            public double TurnOverLimitUtilized;
            public double MTMLimitUtilized;
            public Single Reserved1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //Atish 25.03.2019
        public struct PeakMarginReplyData
        {
            public char Flag;
            public byte FELength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] FEUserID;
            public double T1PeakMgn;
            public double T2PeakMgn;
            public double IntraDayPeakMgn;
            public double PeakMargin;
            public double AvaliableMargin;
            public double DiffPeakMargin;
            public Single Reserved1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//30.05.2016 Pranoti
        public struct UpdateClientsReply
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string Status;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //Atish 25.03.2019
        public struct TTraderIDRecordForSurveillance
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] FEUserID;
            public byte DealerIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] DealerID;
            public char Status;
            public long LastUpdatedMarginTime;
            public long LastUpdatedClientsTime;
            public byte SegmentIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] SegmentID;
            public byte BranchIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] BranchID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTraderIDRecordForMTM
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public char SkipFromMTM;
            public long LastUpdatedMarginTime;
            public long LastUpdatedClientsTime;
            public int SegmentID;
        }

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        //public struct TTraderIDRecordForMTMInstrumentWise
        //{
        //    public byte FEUserIDLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        //    public char[] FEUserID;
        //    public char SkipFromMTM;
        //    public long LastUpdatedMarginTime;
        //    public long LastUpdatedClientsTime;
        //    public int SegmentID;
        //}

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TUpdateClientsForMTM
        {
            public byte DummyLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] Dummy;
            public int DummyInt;
            public char MTMLimitHit;
            public char SkipFromMTM;
        }

        #endregion

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TOptionExerciseRequestRecord
        {
            public char Exch;
            public char ExchType; // 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char Exercise; //Y ,N
            public int Qty;
            public int OldQty;
            public ushort TraderRequestID;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public char BuySell;//02.08.17
            public int Reserved1;
            public int Reserved2;

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDerivInfoRecord
        {
            public byte InstrType;
            public byte SymbolLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Symbol;
            public int ExpiryDate;  //Seconds from 01/01/80
            public Single StrikePrice;
            public byte CPType;
            public byte CALevel;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TOptionExerciseBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char Exercise; //Y ,N
            public int Qty;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public char BuySell;//02.08.17
            public int Reserved1;
            public int Reserved2;

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TOptionExerciseExchReplyRecord
        {
            public char Exch;
            public char ExchType; // 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char Exercise; //Y ,N
            public int Qty;
            public int BrokerOrderID;
            public int BrokerTime;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte statusLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] status;
            public int ExchReplyCode;
            public int ExchReasonCode;
            public int ExchErrorCode;
            public char BuySell;//02.08.17
            public ulong Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TExerciseReportRequestRecord
        {
            public int StartTime, EndTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TExerciseReportReplyRecord
        {
            public char Exch;
            public char ExchType;//'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char Exercise; //Y ,N
            public int Qty;
            public int BrokerOrderID;
            public int BrokerTime;
            public Int64 ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] Status;  //'Sent To Exch','Placed','Cancelled only
            public char BuySell;//03.08.17
            public ulong Reserved;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TExchangeStatusRecord
        {
            public char Exch;
            public char ExchType;
            public char ExchOpen;
            public char AHOpen;
            public char Preopen;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TClearSentToExchRecord
        {
            public char Exch;
            public char ExchType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TLinkFailedRecord
        {
            public byte RequestType;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public ushort ErrorLevel;
            public byte ErrorMsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string ErrorMsg;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TVerifyTraderRequestRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte FEUserType;
            public ushort BranchID;
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] TraderID;
            public byte ClientIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientIP;
            public byte ServerIPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ServerIP;
            public byte ITS_IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ITS_IP;
            public byte MachineIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MachineID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTraderListRequestRecord
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte FEUserType;
            public ushort BranchID;
            public byte ReqUserType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTraderListReplyRecord
        {
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] TraderID;
            public byte AccountType;  // new on 19/02/2008
            public byte TraderNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 29)]
            public string TraderName;  //30 to 29 on 01/02/2008
            public byte NilOrders; //boolean
            public byte TraderType;
            public ushort BranchID;
            public byte AllowNseCash; //boolean
            public byte AllowNseDeriv; //boolean
            public byte AllowBseCash; //boolean
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TLicenceInfoReplyRecord
        {
            public byte UserType;
            public int UserCnt;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMoneyTransferRequestRecord
        {
            public byte AddToBse; //boolean
            public int Amount;
            public int TrxID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMoneyTransferReplyRecord
        {
            public byte AddToBse; //boolean
            public int Amount;
            public int TrxID;
            public Single OpeningMargin;
            public Single AdditionalMargin;
            public Single MarginForOpenPos;
            public Single MarginForPendOrder;
            public Single AvailableMargin;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TAdhocReportRequestRecord
        {
            public byte AdminCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] AdminCode;
            public byte ClientCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientCode;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TAdhocReportReplyRecord
        {
            public byte ClientCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientCode;
            public Single BodLB;
            public Single Receipts;
            public Single Payments;
            public Single Adhoc;
            public Single AHVCash;
            public Single PSVCash;
            public Single MBPOCash;
            public Single AHVFnO;
            public Single PSVFnO;
            public Single MBPOFnO;
            public Single AvlbMgn;
            public Single MFDebit;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TAdhocModifyRequestRecord
        {
            public byte ClientCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientCode;
            public Single OldAdhoc;
            public Single NewAdhoc;
            public Single AvlbMgn;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TAdhocModifyReplyRecord
        {
            public byte ClientCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientCode;
            public char Status;
            public byte MsgLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string Msg;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TClientPositionReportRequestRecord
        {
            public byte ClientCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientCode;
            public int ScripCode;  //if 0 then all scrips, else selected scrip
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TClientPositionReportReplyRecord
        {
            public int ScCode;
            public int HoldingUsedQty;
            public int BoughtQty;
            public Single BuyAvgRate;
            public int SoldQty;
            public Single SellAvgRate;
            public int PendBuyQty;
            public Single PendBuyAvgRate;
            public int PendSellQty;
            public Single PendSellAvgRate;
            public Single AHV;
            public Single PSV;
            public Single MBPO;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct THoldingsReportReplyRecord
        {
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTradeToTradeReplyRecord
        {
            public char Exch;
            public int ScripCode;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TBseNseMapRecord
        {
            public int BseCode;
            public int NseCode;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TIlliquidReplyRecord
        {
            public char Exch;
            public int ScripCode;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDerivBannedScripReplyRecord
        {
            public int ULToken;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDateReplyRecord
        {
            public int DBDate;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TClientDbNumReportReplyRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ClientCode;
            public byte DBNum;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TScripCategoryReportRequestRecord
        {
            public int Junk;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TScripCategoryReportReplyRecord
        {
            public char Category;
            public Single BuyMgn;
            public Single SellMgn;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMaalFodoClientsReportRequestRecord
        {
            public int ClientType;
            public int MinimumDebit;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMaalFodoClientsReportReplyRecord
        {
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] TraderID;
            public byte AccountType;
            public byte ClientType;
            public byte ClientNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ClientName;
            public int BranchId;
            public byte BranchAddrLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] BranchAddr;
            public byte AdminIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] AdminID;
            public double MFDebit;
            public double Mgn4OpenPos;
            public double Receipts;
            public double Shortage;
            public double FOBalance;
            public double FOMgnPayable;
            public double FinalMF;
            public char AllowNseCash;
            public char AllowNseDeriv;
            public char AllowBseCash;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct THoldingsReportReplyRecord_WithCollatral
        {
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
            public int Collatral;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct THoldingsReportReplyRecordNew
        {
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string TraderID;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] // Added on 08-Nov-2016 - Rohan
        public struct THoldingsEquityReportReplyRecordNew
        {
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string TraderID;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMaalFodoClientHoldingReportRequestRecord
        {
            public int ClientType;
            public int MinimumDebit;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMaalFodoClientHoldingReportReplyRecord
        {
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] TraderID;
            public int NseCode;
            public int BseCode;
            public int BrokerBeneQty;
            public int ExchRcvQty;
            public int TodaySoldQty;
            public char Category;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMaalFodoClientAllowToTradeRequestRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] TraderID;
            public char Category;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMaalFodoClientAllowToTradeReplyRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] TraderID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMessageStringRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
            public char[] MsgStr;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TScripMarginRequestRecord
        {
            public char Exch;
            public int ScCode;
        }

        // 99 20150618 VerifyUser Request
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TVerifyUserRequestRecord
        {
            public byte UserIdLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] UserId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Pan;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Dob;
            public int Reserved1;
            public byte Reserved2;
        }

        // 99 20150618 VerifyUser Reply
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TVerifyUserReplyRecord
        {
            public byte Verified;// 0 =  false and 1 = true
            public int Reserved1;
            public byte Reserved2;
        }

        //2015.03.26
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TScripInfoRequestRecord
        {
            public char Exch;
            public int ScCode;
        }
        //end 2015.03.26

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TScripMarginReplyRecord
        {
            public char Exch;
            public int ScCode;
            public char ScCate;
            public char ExchType;
            public Single Margin;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TScripMarginReplyRecord_New
        {
            public char Exch;
            public int ScCode;
            public char ExchType;
            public char IntraScCate;
            public char DelScCate;
            public Single IntraMargin;
            public Single DelMargin;
            public byte Reserved;
            public byte Reserved1;
            public byte Reserve2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TScripInfoReplyRecord
        {
            public char Exch;
            public char ExchType;
            public int Code;
            public byte ISINLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ISIN; //
            public float SpanMargin;
            public float ExpMargin;
            public float IssuedCapital;
            public int FreezeQty;
            public int FaceValue;
            public char PreOpen;
            public int BookClosureStartDate;
            public int BookClosureEndDate;
            public int ExpDate;
            public char Category;
            public Single VarMargin;
            public char Dividend;
            public char Rights;
            public char Bonus;
            public char Interest;
            public byte CreditRatingLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public char[] CreditRating;
            public float IntraVarMgn;
            public float DelVarmgn;
            public byte Reserved1;
            public int Reserved2;
            public float Reserved3;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TPreOpenScripReplyRecord
        {
            public char Exch;
            public int ScCode;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TOTSRequestRecord
        {
            public byte IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] IP;
            public DateTime ReqTime;
            public DateTime ThreadStartTime;
            public TIARequestHeaderRecord ReqHdr;
            public TAllRequestNewRecord AllReq;
            public ushort RetryCnt;
            public int ITS_Num;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TOTSReplyRecord
        {
            public byte IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] IP;
            public DateTime ReplyTime;
            public TIAReplyHeaderRecord ReplyHdr;
            public byte ErrorStringLength;
            public string ErrorString;
            public TAllReplyNewRecord[] AllReply;
            public TAllReplyNewRecord[][] AllReplyNew;
            public TIAReplyHeaderRecord[] NewReplyHdr;
            public int ITS_Num;
            public int DB_Num;
            public int ReplyCount;
            public int ReportReplyCount;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TOTSCliDBNumReplyRecord
        {
            public int Cnt;
            public TClientDbNumReportReplyRecord AllReply;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TBSE_EC_RequestRecord
        {
            // public TIAReplyHeaderRecord ReplyHdr;
            // public TAllReplyNewRecord AllReply;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TECReplyRecord
        {
            public DateTime ReqTime;
            // public TIAReplyHeaderRecord ReplyHdr;
            public string ErrorString;
            // public TAllReplyNewRecord AllReply;
            public ushort RetryCnt;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TECReplyFileWriteRecord
        {
            public DateTime ReqTime;
            // public TIAReplyHeaderRecord ReplyHdr;
            // public TAllReplyNewRecord AllReply;
            public ushort RetryCnt;
            public int ExType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
            public char[] RqStr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
            public char[] ExStr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TCurrentOrderRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] TraderID;
            public char Exch;
            public char ExchType;// 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public TDerivInfoRecord DerivInfo;
            public int ExchPos;  //For ExchData
            public int ScripPos; //For ExchData
            public char BuySell;
            public int Qty;
            public int AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public Single AvgTradedRate;
            public Single TradedValue;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public long ExchangeOrderId;
            public int LastExchangeTime;
            public string Status;
            public char AHStatus;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TCurrentTradeRecord
        {
            public char Exch;
            public char ExchType;// 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public TDerivInfoRecord DerivInfo;
            public int ExchPos;  //For ExchData
            public int ScripPos; //For ExchData
            public char BuySell;
            public int Qty;
            public Single Rate;
            public long ExchangeOrderId;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;
            public Single CurrRate;
            public int RevQty;  //For NEt Pos calc
            public int BookedPL;   //For NEt Pos calc
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TCurrentExerciseOrderRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] TraderID;
            public char Exch;
            public char ExchType;// 'D' for Deriv
            public int ScripCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public TDerivInfoRecord DerivInfo;
            public char Exercise;
            public int Qty;
            public int BrokerOrderID;
            public int BrokerTime;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] Status;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TNetPositionRecord
        {
            public char Exch;
            public char ExchType;// 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public char TT; //Trade For Trade Scrip
            public TDerivInfoRecord DerivInfo;
            public int ExchPos;  //For ExchData
            public int ScripPos; //For ExchData
            public int BuyQty;
            public double AvgBuyRate;
            public double BuyValue;
            public int SellQty;
            public double AvgSellRate;
            public double SellValue;
            public int NetQty;
            public double AvgNetRate;
            public double NetValue;
            public double BookedPL;
            public double MtmPL;
            public double CurrRate;
            public int ExerciseQty;
            public int DExerciseQty;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TMCBrokerOrderRecord
        {
            public char OrderMC;  //M for modify / C for Cancel
            public char Exch;
            public int ScripCode;
            public Single ExchangeOrderId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TTraderPositionRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] TraderID;
            public byte AccountType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] TraderName;
            public byte AllowNseCash; //boolean
            public byte AllowNseDeriv; //boolean
            public byte AllowBseCash; //boolean
            public byte PositionRequested; //boolean
            public byte PositionReceived; //boolean
            public TCurrentOrderRecord[] ArrayCurrentOrders;
            public TCurrentTradeRecord[] ArrayCurrentTrades;
            public TNetPositionRecord[] ArrayNetPosition;
            public TCurrentExerciseOrderRecord[] ArrayCurrentExerciseOrders;
            public TCurrentOrderRecord[] ArrayModifyCancelOrders;
            public TMCBrokerOrderRecord[] ArrayMCBrokerOrders;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TThreadUserRecord
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string User;
            public char Exch;
            public int ScCode;
            public bool InUse; // boolean

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct THoldingsValuationReportRecord
        {
            public byte ScNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScName;
            public int Qty;
            public Single PrevClose;
            public double HolidingsValue;
            public double AfterHaircutValue;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //McxTrading 19.01.17
        public struct TMCXGroupLevelHaltInfo_OTS
        {
            public short GroupID;
            public short Status;
            public long TriggerTime;
         }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct THoldingsValuationReportWithMemVar
        {
            public byte ScNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScName;
            public int Qty;
            public Single LastRatePrice;
            public Single BODVarMargin;
            public double HolidingsValue;
            public double AfterHaircutValue;            
            public byte Reserved1;
            public int Reserved2;
        }




        #region Structure for Disclosed Qty 15 Apr 2014

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TCurrDerivOrderPlaceRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public byte IOC;
            public int DisclosedQty;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TCurrDerivOrderModifyRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public char SLTriggered;
            public int OldQty;
            public char AHStatus;
            public byte IOC;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int DisclosedQty;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TCurrDerivOrderCancelRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public char SLTriggered;
            public int OldQty;
            public char AHStatus;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int DisclosedQty;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TCurrDerivOrderPlaceBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public byte IOC;
            public char AHStatus;
            public int DisclosedQty;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TCurrDerivOrderModifyBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public char SLTriggered;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public char AHStatus;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TCurrDerivOrderCancelBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public char SLTriggered;
            public int OldQty;
            public int TradedQty;
            public char AHStatus;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public int DisclosedQty;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TCurrDerivOrderExchReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public TDerivInfoRecord DerivInfo;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchReasonCode;
            public int ExchErrorCode;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status;
            public int DisclosedQty;
        }

        #endregion

        #region Structure for Lot Quantity
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TLotQuantityRecord
        {
            public byte ScNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScName;
            public int Qty;
        }
        #endregion

        #region Add/Modify/Cancel BseMF Krutika
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        public struct TBseMFAddModifyCancelRequestRecord
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public char BuySell;
            public byte BuySellTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] BuySellType;
            public char DptxnType;
            public Single Amount;
            public long RedeemQty;
            public byte FolioNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public char[] FolioNo;
            public char KYCStatus;
            public long ExchOrderNo;
            public char DPCFlag;
            public ushort TraderRequestID;
            public int Reserved1;
            public int Reserved2;
            public float Reserved3;
            public float Reserved4;
            public byte Reserved5;
            public byte Reserved6;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TBseMFAddModifyCancelExchangeOrderReply
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public char BuySell;
            public byte BuySellTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] BuySellType;
            public char DptxnType;
            public Single Amount;
            public long RedeemQty;
            public byte FolioNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public char[] FolioNo;
            public char KYCStatus;
            public char DPCFlag;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchErrorCode;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
            public string Status;
            public char AllUnitsFlag;
            public int Reserved1;
            public int Reserved2;
            public float Reserved3;
            public float Reserved4;
            public byte Reserved5;
            public byte Reserved6;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TBseMFAddModifyCancelBrokerResponse
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public char BuySell;
            public byte BuySellTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] BuySellType;
            public char DptxnType;
            public Single Amount;
            public long RedeemQty;
            public byte FolioNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public char[] FolioNo;
            public char KYCStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] Remarks;
            public ushort TraderRequestID;
            public int BrokerId;
            public int BrokerTime;
            public long ExchOrderNo;
            public char AllUnitsFlag;
            public int Reserved1;
            public int Reserved2;
            public float Reserved3;
            public float Reserved4;
            public byte Reserved5;
            public byte Reserved6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TBseMfOrderReportRequestRecord
        {
            public int StartTime;
            public int EndTime;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TBseMfActivitylogRequestRecord
        {
            public int StartTime;
            public int EndTime;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct MandateOrderReportRequestRecord
        {
            public int StartTime;
            public int EndTime;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TBseMFOrderTypeReportsReplyRec
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public char BuySell;
            public byte BuySellTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] BuySellType;
            public char DptxnType;
            public Single Amount;
            public long RedeemQty;
            public char KYCStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] Status;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] Reason;
            public char DPCFlag;
            public ushort TraderRequestID;
            public int BrokerId;
            public int BrokerTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public char AllUnitsFlag;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TBseMFActivitylogReplyRec
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public char BuySell;
            public byte BuySellTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] BuySellType;
            public char DptxnType;
            public Single Amount;
            public long RedeemQty;
            public char KYCStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] Status;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] Reason;
            public char DPCFlag;
            public ushort TraderRequestID;
            public int BrokerId;
            public int BrokerTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public char AllUnitsFlag;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//Holding Report
        public struct THoldingsReportReplyRecord_MF
        {
            //public int BseCode;
            //public int NseCode;
            public int UniqueNo;
            public byte SchemeNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] SchemeName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
            public int Collatral;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        public struct TBseMFSIPRegistrationMsgRequestRecord
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte AMCCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] AMCCode;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public char TxnMode;
            public char DptxnType;
            public byte FolioNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] FolioNo;
            public byte FrequencyTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] FrequencyType;
            public byte FrequencyAllowed;
            public double Amount;
            public char Firstordertodayflag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] StartDate;
            public int Numberofinstallments;
            public char DPCFlag;
            public long SIPregistrationNum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] EndDate;
            public ushort TraderRequestID;
            public int Reserved1;
            public int Reserved2;
            public float Reserved3;
            public float Reserved4;
            public byte Reserved5;
            public byte Reserved6;
        }
        #region XSIP
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        public struct XBseMFSIPRegistrationMsgRequestRecord
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte AMCCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] AMCCode;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public char TxnMode;
            public char DptxnType;
            public byte FolioNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] FolioNo;
            public byte FrequencyTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] FrequencyType;
            public byte FrequencyAllowed;
            public double Amount;
            public double Brokerage;
            public int Mandate;
            public char Firstordertodayflag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] StartDate;
            public int Numberofinstallments;
            public char DPCFlag;
            public long SIPregistrationNum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] EndDate;
            public ushort TraderRequestID;
            public int Reserved1;
            public int Reserved2;
            public float Reserved3;
            public float Reserved4;
            public byte Reserved5;
            public byte Reserved6;
        }

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        //public struct TBseMFSIPRegistrationMsgRequestRecord
        //{
        //    public char Exch;
        //    public char ExchType;
        //    public byte UniqueNoLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        //    public char[] UniqueNo;
        //    public byte AMCCodeLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        //    public char[] AMCCode;
        //    public byte SchemeCodeLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] SchemeCode;
        //    public char TxnMode;
        //    public char DptxnType;
        //    public byte FolioNoLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] FolioNo;
        //    public byte FrequencyTypeLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] FrequencyType;
        //    public byte FrequencyAllowed;
        //    public double Amount;
        //    public char Firstordertodayflag;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        //    public char[] StartDate;
        //    public int Numberofinstallments;
        //    public char DPCFlag;
        //    public long SIPregistrationNum;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] EndDate;
        //    public ushort TraderRequestID;
        //    public int Reserved1;
        //    public int Reserved2;
        //    public float Reserved3;
        //    public float Reserved4;
        //    public byte Reserved5;
        //    public byte Reserved6;
        //}

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        public struct TBseMFSIPRegistrationExchangeReplyRecord
        {
            public char Exch;
            public char ExchType;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] RegistrationDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] StartDate;
            public int InstallmentNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] EndDate;
            public double InstallmentAmount;
            public char Firstordertodayflag;
            public byte FolioNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] FolioNo;
            public char DPCFlag;
            public long SIPregistrationNum;//ExchOrderId
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchErrorCode;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
            public string Status;
            public int Reserved1;
            public int Reserved2;
            public float Reserved3;
            public float Reserved4;
            public byte Reserved5;
            public byte Reserved6;
        }

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        //public struct TBseMFSIPRegistrationMsgBrokerReply
        //{
        //    public char Exch;
        //    public char ExchType;
        //    public byte UniqueNoLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        //    public char[] UniqueNo;
        //    public byte AMCNameLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        //    public char[] AMCName;
        //    public byte SchemeCodeLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] SchemeCode;
        //    public char TransactionMode;
        //    public char DPTransactionMode;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] FolioNo;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] FrequencyType;
        //    public byte FrequencyAllowed;
        //    public double Amount;
        //    public char Firstordertodayflag;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        //    public char[] StartDate;
        //    public int InstallmentNumb;
        //    public byte RemarksLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        //    public char[] Remarks;
        //    public long SIPregistrationNum;
        //    public ushort TraderRequestID;
        //    public int BrokerOrderID;
        //    public int BrokerOrderTime;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] EndDate;
        //    public int Reserved1;
        //    public int Reserved2;
        //    public float Reserved3;
        //    public float Reserved4;
        //    public byte Reserved5;
        //    public byte Reserved6;
        //}

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TBseMFSIPOrderTypeReportsReplyRec
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public Single Amount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] RegistrationDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] StartDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] Status;
            public int InstallmentNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] EndDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] FrequencyType;
            public char FirstOrderFlag;
            public char DptxnType;
            public char TxnMode;
            public ushort TraderRequestID;
            public int BrokerId;
            public int BrokerTime;
            public long SIPRegistrationID;
            public int ExchangeOrderTime;
        }
        
        #endregion
        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        //public struct TBseMFSIPRegistrationExchangeReplyRecord
        //{
        //    public char Exch;
        //    public char ExchType;
        //    public byte SchemeCodeLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] SchemeCode;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        //    public char[] RegistrationDate;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        //    public char[] StartDate;
        //    public int InstallmentNumber;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] EndDate;
        //    public double InstallmentAmount;
        //    public char Firstordertodayflag;
        //    public byte FolioNoLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] FolioNo;
        //    public char DPCFlag;
        //    public long SIPregistrationNum;//ExchOrderId
        //    public int ExchangeOrderTime;
        //    public int ExchReplyCode;
        //    public int ExchErrorCode;
        //    public int BrokerOrderID;
        //    public int BrokerOrderTime;
        //    public byte StatusLength;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        //    public string Status;
        //    public int Reserved1;
        //    public int Reserved2;
        //    public float Reserved3;
        //    public float Reserved4;
        //    public byte Reserved5;
        //    public byte Reserved6;
        //}

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        public struct TBseMFSIPRegistrationMsgBrokerReply
        {
            public char Exch;
            public char ExchType;
            public byte UniqueNoLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] UniqueNo;
            public byte AMCNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] AMCName;
            public byte SchemeCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] SchemeCode;
            public char TransactionMode;
            public char DPTransactionMode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] FolioNo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] FrequencyType;
            public byte FrequencyAllowed;
            public double Amount;
            public double Brokerage;
            public int Mandate;
            public char Firstordertodayflag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] StartDate;
            public int InstallmentNumb;
            public byte RemarksLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] Remarks;
            public long SIPregistrationNum;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] EndDate;
            public int Reserved1;
            public int Reserved2;
            public float Reserved3;
            public float Reserved4;
            public byte Reserved5;
            public byte Reserved6;
        }

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        //public struct TBseMFSIPOrderTypeReportsReplyRec
        //{
        //    public char Exch;
        //    public char ExchType;
        //    public byte UniqueNoLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        //    public char[] UniqueNo;
        //    public byte SchemeCodeLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] SchemeCode;
        //    public Single Amount;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        //    public char[] RegistrationDate;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        //    public char[] StartDate;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] Status;
        //    public int InstallmentNumber;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] EndDate;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public char[] FrequencyType;
        //    public char FirstOrderFlag;
        //    public char DptxnType;
        //    public char TxnMode;
        //    public ushort TraderRequestID;
        //    public int BrokerId;
        //    public int BrokerTime;
        //    public long SIPRegistrationID;
        //    public int ExchangeOrderTime;
        //}
        
        #endregion

        #region SLBM Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TSLBMOrderPlaceRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv & SLBM
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte InstrType;
            public int ExpiryDate;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public byte IOC;
            public int AllowBanScrip;
            public int DisclosedQty;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TSLBMOrderPlaceBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv & SLBM
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte InstrType;
            public int ExpiryDate;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TSLBMOrderModifyRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte InstrType;
            public int ExpiryDate;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;  //bse - diff req for sl before/after trigger. diff from limit/mkt order
            public char SLTriggered;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TSLBMOrderModifyBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv & SLBM
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte InstrType;
            public int ExpiryDate;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public char SLTriggered;
            public Single TriggerRate;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public ushort TraderRequestID;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TSLBMOrderCancelRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv & SLBM
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte InstrType;
            public int ExpiryDate;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;  //bse - diff req for sl before/after trigger. diff from limit/mkt order
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public ushort TraderRequestID;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TSLBMOrderCancelBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv & SLBM
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte InstrType;
            public int ExpiryDate;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public ushort TraderRequestID;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TSLBMOrderExchReplyRecord
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte InstrType;
            public int ExpiryDate;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchReasonCode;
            public int ExchErrorCode;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status;
            public int DisclosedQty;
            public byte IOC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TSLBMHoldingsReportReplyRecord
        {
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public char ReceivingDeliveringFlag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public char[] DeliveryDate;
            public long TradeNo;
            public char SettlementType;
            public int SettlementNo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public char[] ExpiryDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] QuantityToBedelivered;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] CustodialParticipantCode;
            public double LendingPrice;
            public double TotalAmount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] RolloverIndicator;
            public int BfQty;
            public int NetQty;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Reserved;
        }
        #endregion

        #region Mandate
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        public struct XSIPMandate
        {
            public char Exch;
            public char ExchType;
            public double Amount;
            public char MandateType;
            public double AccountNo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] ACtype;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] IFSC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MICR;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] StartDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] EndDate;
            public ushort TraderRequestID;

        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        public struct XSIPMandateBrokerReply
        {
            public char Exch;
            public char ExchType;
            public double Amount;
            public char MandateType;
            public double AccountNo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] ACtype;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] IFSC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MICR;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] StartDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] EndDate;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public ushort TraderRequestID;
            public byte RemarksLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] Remarks;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
        public struct XSIPMandateExchangeReply
        {
            public char Exch;
            public char ExchType;
            public byte MandateidLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MANDATEID;
            public int BrokerOrderID;
            public int BrokerOrderTime;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct MandateOrderTypeReportsReplyRec
        {
            public char Exch;
            public char ExchType;
            public double Amount;
            public char MandateType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string AccountNo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] ACtype;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] IFSC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MICR;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] StartDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] EndDate;
            public byte MandateidLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] MANDATEID;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public ushort TraderRequestID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
            public string Status;
            public byte RemarksLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] Remarks;
        }
        #endregion

        #region Dealer All Report

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderTypeReportsReplyRecDealer
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
            public bool IsSpread;
            public int ScripCode2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] RejectReason;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string OrderRequestorCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string ClientCode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TTradeReportReplyRecDealer
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int OrgQty;
            public int PendingQty;
            public long ExchangeOrderID;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;

            public byte OrderType;
            public byte ProductType;

            public int Reserved1;
            public int Reserved2;
            public bool IsSpread;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string OrderRequestorCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string ClientCode;
        }

        #endregion

        #region Comment
        //[StructLayout(LayoutKind.Explicit, Pack = 1)]
        //public unsafe struct TAllRequestNewRecord
        //{
        //[FieldOffset(0)]
        //public TLoginRequestRecord LoginRequest;  //1
        //[FieldOffset(0)]
        //public TChangePasswordRequestRecord ChgPswdRequestRec; //2
        //[FieldOffset(0)]
        //public TTraderListRequestRecord TraderListRequestRec; //5
        //[FieldOffset(0)]
        //public TVerifyTraderRequestRecord VerifyTraderRequestRec; //6
        //[FieldOffset(0)]
        //public TScripMarginRequestRecord ScripMarginRequestRec; //41
        //[FieldOffset(0)]
        //public TTradeReportRequestRecord TradeReportRequestRec; //61
        //[FieldOffset(0)]
        //public TOrderReportRequestRecord OrderReportRequestRec; //62
        ////[FieldOffset(0)]
        ////public TExerciseReportRequestRecord ExerciseReportRequestRec; //63
        //[FieldOffset(0)]
        //public TActivityLogRequestRecord ActivityLogRequestRec; //64
        //[FieldOffset(0)]
        //public TAHOrderReportRequestRecord AHOrderReportRequestRec; //65
        //[FieldOffset(0)]
        //public TDummyRecord LicenceInfoRequestRec; //66
        //[FieldOffset(0)]
        //public TDummyRecord CliDBNumRequestRec; //68
        //[FieldOffset(0)]
        //public TMoneyTransferRequestRecord MoneyTransferRequestRec; //67
        //[FieldOffset(0)]
        //public TTraderIDRecord HoldingsReportRequestRec; //69
        //[FieldOffset(0)]
        //public TDummyRecord DBDateRequestRec; //73
        //[FieldOffset(0)]
        //public TClientPositionReportRequestRecord ClientPositionRequestRec; //79
        //[FieldOffset(0)]
        //public TDummyRecord TradeToTradeRequestRec; //80
        //[FieldOffset(0)]
        //public TDummyRecord BseNseMapRequestRec; //81
        //[FieldOffset(0)]
        //public TDummyRecord DerivBannedScripRequestRec; //82
        //[FieldOffset(0)]
        //public TAdhocReportRequestRecord AdhocReportRequestRec; //83
        //[FieldOffset(0)]
        //public TAdhocReportRequestRecord AdhocModifyRequestRec; //84
        //[FieldOffset(0)]
        //public TScripCategoryReportRequestRecord ScripCategoryReportRequestRec; //85
        //[FieldOffset(0)]
        //public TMaalFodoClientsReportRequestRecord MFClientsReportRequestRec; //86
        //[FieldOffset(0)]
        //public TMaalFodoClientHoldingReportRequestRecord MFClientHoldingReportRequestRec; //87
        //[FieldOffset(0)]
        //public TMaalFodoClientAllowToTradeRequestRecord MFClientAllowToTradeRequestRec; //88 
        //[FieldOffset(0)]
        //public TDummyRecord IlliquidRequestRec; //89
        //[FieldOffset(0)]
        //public TDummyRecord PreOpenScripRequestRec; //90
        //[FieldOffset(0)]
        //public TTraderIDRecord MarginReportRequestRec; //200
        //[FieldOffset(0)]
        //public TDummyRecord ITS2OTSNumRec; //245 //25/01/2008
        //[FieldOffset(0)]
        //public TLinkFailedRecord LinkFailRec; //251
        //[FieldOffset(0)]
        //public TLogoutRequestRecord LogoutRequest; //252
        //[FieldOffset(0)]
        //public TDummyRecord HandShakeRec; //253
        //[FieldOffset(0)]
        //public TEquityOrderPlaceRequestRecord EquityOrderPlaceRequestRec; //10
        //[FieldOffset(0)]
        //public TAHEquityOrderPlaceRequestRecord AHEquityOrderPlaceRequestRec; //15 //reply as 10
        //[FieldOffset(0)]
        //public TEquityOrderModifyRequestRecord EquityOrderModifyRequestRec; //20
        //[FieldOffset(0)]
        //public TEquityOrderCancelRequestRecord EquityOrderCancelRequestRec; //30
        //[FieldOffset(0)]
        //public TDerivOrderPlaceRequestRecord DerivOrderPlaceRequestRec; //110
        //[FieldOffset(0)]
        //public TAHDerivOrderPlaceRequestRecord AHDerivOrderPlaceRequestRec; //115//reply as 110
        //[FieldOffset(0)]
        //public TDerivOrderModifyRequestRecord DerivOrderModifyRequestRec; //120  
        //[FieldOffset(0)]
        //public TDerivOrderCancelRequestRecord DerivOrderCancelRequestRec; //130
        //[FieldOffset(0)]
        //public TOptionExerciseRequestRecord OptionExerciseRequestRec; //181,182,183   

        //  }
        #endregion

        #region Structures used for Preventing Unauthorized Trading - Sanchit
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct OrderPlaceReqRecForApproval
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public ushort TraderRequestID;
            public char AHStatus;
            public byte IOC;
            public int DisclosedQty;
            public byte OrderType;
            public byte IntraMultiplierReq;
            public byte RequestType;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte Reserved1;
            public char Reserved2;
            public short Reserved3;
            
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct OrderPlaceReplyRecOfApproval
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte ReplyType;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] TraderID;
            public int ReferenceID;
            public int OrderReqTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            public int OTPNumber;
            public byte IsIVRProcessed;
            public int BrokerOrderID;
            public int BrokerTime;
            public char SLTriggered;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte OrderType;
            public byte ResendOTPFlag;
            public byte SendOTP;
            public byte IntraMultiplierReq;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] RecallRepayOrder;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public int Reserved2;
            public Single Reserved3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct PendingApprovalOrderReport
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte ReplyType;
            public byte InstrType;
            public byte CPType;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] TraderID;
            public int ReferenceID;
            public int OrderReqTime;
            public byte IOC;
            public int DisclosedQty;
            public int OTPNumber;
            public char SLTriggered;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte OrderType;
            public byte IntraMultiplierReq;
            public char ApprovalStatus;
            public byte Reserved1;
            public ushort Reserved4;
            public int Reserved2;
            public Single Reserved3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct UpdateOTPOrderStatus
        {
            public byte ClientCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] ClientCode;
            public int ReferenceID;
            public int ClientOTPNumber;
            public char OTPApproved;
            public byte ResendOTP;
            public byte Reserved1;
            public int Reserved2;
            public int Reserved3;
            public Single Reserved4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct UpdateOTPOrderStatusResponse
        {
            public byte ClientCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] ClientCode;
            public int ReferenceID;
            public char OTPStatusUpdated;
            public byte OTPNumberAttempt;
            public byte Reserved1;
            public int Reserved2;
            public int Reserved3;
        }


        #region Flag For IVR
        public struct IsIVRDealerFlag
        {
            public bool IsIVRDealer;
            public bool Reserved1;
            public bool Reserved2;
            public bool Reserved3;
            public bool Reserved4;
            public bool Reserved5;
            public int Reserved6;

        }


        #endregion


        #endregion

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TAllRequestNewRecord
        {
            public IntellectSharedClassLibrary.ISCL_HoldingEquityReply ISCL_HoldingEquityReply; // 199
            public ITInteractiveStructures.TActivityLogRequestRecord_new ActivityLogRequestRecord_new; //sjumit   
            public ITInteractiveStructures.TLoginRequestRecord LoginRequest;  //1      
            public ITInteractiveStructures.TLoginRequestRecordTwoFactor LoginRequestTwoFactor;  //85
            public ITInteractiveStructures.TChangePasswordRequestRecord ChgPswdRequestRec; //2
            public ITInteractiveStructures.TTraderListRequestRecord TraderListRequestRec; //5
            public ITInteractiveStructures.TVerifyTraderRequestRecord VerifyTraderRequestRec; //6                  
            public ITInteractiveStructures.TScripMarginRequestRecord ScripMarginRequestRec; //41
            public ITInteractiveStructures.TScripInfoRequestRecord ScripInfoRequestRec;//42
            public ITInteractiveStructures.TTradeReportRequestRecord TradeReportRequestRec; //61
            public ITInteractiveStructures.TTradeReportReplyRecNewCover TradeReportReplyRecNewCover; //58   // sumitp 28_jan_22
            public ITInteractiveStructures.TOrderReportRequestRecord OrderReportRequestRec; //62             
            public ITInteractiveStructures.TExerciseReportRequestRecord ExerciseReportRequestRec; //63       
            public ITInteractiveStructures.TActivityLogRequestRecord ActivityLogRequestRec; //64
            //public ITInteractiveStructures.TAHOrderReportRequestRecord AHOrderReportRequestRec; //65    
            public ITInteractiveStructures.TDummyRecord LicenceInfoRequestRec; //66   
            //public ITInteractiveStructures.TMoneyTransferRequestRecord MoneyTransferRequestRec; //67        
            public ITInteractiveStructures.TDummyRecord CliDBNumRequestRec; //68           
            public ITInteractiveStructures.TTraderIDRecord HoldingsReportRequestRec; //69    
            public ITInteractiveStructures.TTraderIDRecord HoldingsEquityReportRequestRec; //76 - Added on 08-Nov-2016 - Rohan
            public ITInteractiveStructures.TTraderIDRecord HoldingsValuationReportRequestRec; //70 
            public ITInteractiveStructures.TTraderIDRecord HoldingsValuationReportWithMemVarRequestRec; //170 //20.05.15
            public ITInteractiveStructures.TDummyRecord DBDateRequestRec; //73       
            public ITInteractiveStructures.TClientPositionReportRequestRecord ClientPositionRequestRec; //79       
            public ITInteractiveStructures.TDummyRecord TradeToTradeRequestRec; //80        
            public ITInteractiveStructures.TDummyRecord BseNseMapRequestRec; //81        
            public ITInteractiveStructures.TDummyRecord DerivBannedScripRequestRec; //82      
            public ITInteractiveStructures.TAdhocReportRequestRecord AdhocReportRequestRec; //83        
            public ITInteractiveStructures.TAdhocModifyRequestRecord AdhocModifyRequestRec; //84        
            //public ITInteractiveStructures.TScripCategoryReportRequestRecord ScripCategoryReportRequestRec; //85 pending    
            public ITInteractiveStructures.TMaalFodoClientsReportRequestRecord MFClientsReportRequestRec; //86       
            public ITInteractiveStructures.TMaalFodoClientHoldingReportRequestRecord MFClientHoldingReportRequestRec; //87      
            //public ITInteractiveStructures.TMaalFodoClientAllowToTradeRequestRecord MFClientAllowToTradeRequestRec; //88 pending   
            public ITInteractiveStructures.TDummyRecord IlliquidRequestRec; //89        
            public ITInteractiveStructures.TDummyRecord PreOpenScripRequestRec; //90 pending
            public ITInteractiveStructures.TVerifyUserRequestRecord VerifyUserRequestRec; //99 
            //Trades Records.
            public ITInteractiveStructures.TEquityOrderPlaceRequestRecord EquityOrderPlaceRequestRec; //10   
            public ITInteractiveStructures.TOrderPlaceRequestRecord OrderTypePlaceReqRec; //14
            public ITInteractiveStructures.TOrderPlaceRequestRecordCoverAndBracket PlaceCoverOrderReqRec; //171    // sumitp 28_jan_22

            public ITInteractiveStructures.TAHEquityOrderPlaceRequestRecord AHEquityOrderPlaceRequestRec; //15 //reply as 10  
            public ITInteractiveStructures.TEquityOrderModifyRequestRecord EquityOrderModifyRequestRec; //20   
            public ITInteractiveStructures.TOrderModifyRequestRecord OrderTypeModifyReqRec; //24
            public ITInteractiveStructures.TOrderConversionRec OrderTypeConversionRec; //26
            public ITInteractiveStructures.TEquityOrderCancelRequestRecord EquityOrderCancelRequestRec; //30  
            public ITInteractiveStructures.TOrderCancelRequestRecord OrderTypeCancelReqRec; //34
            public ITInteractiveStructures.TDerivOrderPlaceRequestRecord DerivOrderPlaceRequestRec; //110   
            public ITInteractiveStructures.TAHDerivOrderPlaceRequestRecord AHDerivOrderPlaceRequestRec; //115//reply as 110  
            public ITInteractiveStructures.TDerivOrderModifyRequestRecord DerivOrderModifyRequestRec; //120    
            public ITInteractiveStructures.TDerivOrderCancelRequestRecord DerivOrderCancelRequestRec; //130
            public ITInteractiveStructures.TTraderIDRecord MarginReportRequestRec; //200  
            public ITInteractiveStructures.TTraderIDRecord MarginReportRequestRecMCX; //206
            public ITInteractiveStructures.TTraderIDRecord MarginReportRequestReccombined; //206

            public ITInteractiveStructures.TDummyRecord ITS2OTSNumRec; //245 //25/01/2008       
            public ITInteractiveStructures.TLinkFailedRecord LinkFailRec; //251       
            public ITInteractiveStructures.TLogoutRequestRecord LogoutRequest; //252      
            public ITInteractiveStructures.TDummyRecord HandShakeRec; //253   
            //Option Exercise
            public ITInteractiveStructures.TOptionExerciseRequestRecord OptionExerciseRequestRec; //181,182,183 

            #region Spread
            public TSpreadOrderPlaceRequestRecord SpreadOrderPlaceRequestRec;
            public TSpreadOrderCancelRequestRecord SpreadOrderModifyCancelRequestRec;
            #endregion

            public ITInteractiveStructures.TDummyRecord AllMarginReq; //201 
            public ITInteractiveStructures.TTraderIDRecordForMTM ClientsForMTMReq;  //202
            public ITInteractiveStructures.TTraderIDRecordForMTM ClientsForMTMReqMcx;
            public ITInteractiveStructures.TUpdateClientsForMTM UpdateMTMLimitHitReq;   //203
            public ITInteractiveStructures.TUpdateClientsForMTM UpdateMTMLimitHitReqMcx;
            public ITInteractiveStructures.TTraderIDRecordForSurveillance ClientsForMTMReqInstrumentWise;
            public ITInteractiveStructures.TPeakMarginData PeakMarginData;


            #region Nse Derive Curr
            public ITInteractiveStructures.TCurrDerivOrderPlaceRequestRecord CurrDerivOrderPlaceRequestRec; //211
            public ITInteractiveStructures.TCurrDerivOrderModifyRequestRecord CurrDerivOrderModifyRequestRec; //221
            public ITInteractiveStructures.TCurrDerivOrderCancelRequestRecord CurrDerivOrderCancelRequestRec; //231
            #endregion

            #region LotQty
            public ITInteractiveStructures.TTraderIDRecord LotQtyRequestRec;
            #endregion
            public ITInteractiveStructures.TOrderTradeGeneral IISOrderTradeGenreal;
            public ITInteractiveStructures.TTemplateMarginRequestRecord TemplateMargin;
            public ITInteractiveStructures.TTemplateMarginRequestRecord_Peak TemplateMargin_Peak;
            public ITInteractiveStructures.TTemplateMarginRequestRecordNew TemplateMarginNew; //Derivative Margin 20.10.22

            #region Add/Modify/Cancel BseMF
            public ITInteractiveStructures.TBseMFAddModifyCancelRequestRecord MfAddModifyCancelReqRec;//Bsemf
            public ITInteractiveStructures.TBseMfOrderReportRequestRecord MfOrderReportRequestRec;
            public ITInteractiveStructures.TBseMfActivitylogRequestRecord MfActivitylogRequestRec;
            public ITInteractiveStructures.MandateOrderReportRequestRecord MandateOrderReportRequestRec;
            public ITInteractiveStructures.TBseMFSIPRegistrationMsgRequestRecord MfSIPRegistrationReqRec;//Bsemf
            public ITInteractiveStructures.XBseMFSIPRegistrationMsgRequestRecord XMfSIPRegistrationReqRec;
            public ITInteractiveStructures.XSIPMandate XSIPMandateRec;
            #endregion

            #region Nse Slbm
            public TSLBMOrderPlaceRequestRecord SLBMOrderPlaceRequestRec;   //235
            public TSLBMOrderModifyRequestRecord SLBMOrderModifyRequestRec; //236
            public TSLBMOrderCancelRequestRecord SLBMOrderCancelRequestRec; //237
            #endregion

            public ITInteractiveStructures.TFetchClientInfoRequest LoginRequestForName;  //4  Jigar
            public ITInteractiveStructures.TDummyRecord GetSegmentMaster; //73   
            public ITInteractiveStructures.TTradesReportExchFormatRequest GetTradesReport; //226
            public ITInteractiveStructures.TOrderPlaceModifyRequestRecordOFS PlaceOfsOrderRequest; //226

            public ITInteractiveStructures.OrderPlaceReqRecForApproval orderPlaceReqRecForApproval; //02.03.23//134
            public ITInteractiveStructures.UpdateOTPOrderStatus UpdateOTPOrderStatusRec;//135
            //public ITInteractiveStructures.ApprovalOrderStatus approvalOrderStatus; //35
            public ITInteractiveStructures.TOrderReportRequestRecord OrderInProcessReportRequestRec; //136

            public ITInteractiveStructures.TTraderIDRecord SurveillanceIndicatorReq; //160,161

            public ITInteractiveStructures.GetIVRDealerDetailsReq GetIVRDealerDetailsReq; //14
            public ITInteractiveStructures.TLogoutRequestRecord_2FA LogoutRequest_2FA; //247      
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TFetchClientInfoRequest
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TAllReplyNewRecord
        {
            public IntellectSharedClassLibrary.ISCL_HoldingEquityReply ISCL_HoldingEquityReply;
            public ITInteractiveStructures.TLoginReplyRecord LoginReply; //1
            public ITInteractiveStructures.TChangePasswordReplyRecord ChgPswdReplyRec; //2
            public ITInteractiveStructures.TLoginReplyRecordNew LoginReplyNew; //3
            public ITInteractiveStructures.TTwoFactorLoginReplyRecord twoFactorLoginReplyRecord; //85
            public ITInteractiveStructures.TTraderListReplyRecord TraderListReplyRec;//5,6  
            public ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord EquityOrderPlaceBrokerReplyRec; //10,15
            public ITInteractiveStructures.TEquityOrderExchReplyRecord EquityOrderExchReplyRec;//11
            public ITInteractiveStructures.TOrderPlaceBrokerReplyRecord OrderTypePlaceBrokerReplyRec; //16
            public ITInteractiveStructures.TOrderPlaceBrokerReplyRecord_Cover OrderTypePlaceBrokerReplyRec_Cover; //16    // sumitp 28_jan_22
            public ITInteractiveStructures.TOrderPlaceBrokerReplyRecordCoverAndBracket OrderPlaceBrokerReplyRecordCoverAndBracket;  //171 // sumitp 28_jan_22
            public ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord_cover EquityOrderPlaceBrokerReplyRecord_cover;  //171 sub structure // sumitp 28_jan_22
            public ITInteractiveStructures.TOrderModifyBrokerReplyRecord OrderTypeModifyBrokerReplyRec;//17
            public ITInteractiveStructures.TOrderCancelBrokerReplyRecord OrderTypeCancelBrokerReplyRec;//18
            public ITInteractiveStructures.TOrderExchReplyRecord OrderTypeExchReplyRecord; //19
            public ITInteractiveStructures.TEquityOrderModifyBrokerReplyRecord EquityOrderModifyBrokerReplyRec;//20
            public ITInteractiveStructures.TOrderConversionRecReply OrderTypeConversionReplyRec;//26
            public ITInteractiveStructures.TEquityOrderCancelBrokerReplyRecord EquityOrderCancelBrokerReplyRec;//30
            public ITInteractiveStructures.TScripMarginReplyRecord ScripMarginReplyRec; // 41
            public ITInteractiveStructures.TScripInfoReplyRecord ScripInfoReplyRec; //42
            public ITInteractiveStructures.TScripMarginReplyRecord_New NewScripMarginReplyRec; // 41
            public ITInteractiveStructures.TTradeReportReplyRecord TradeReportReplyRec; //61
            public ITInteractiveStructures.TOrderReportReplyRecord OrderReportReplyRec;//62
            public ITInteractiveStructures.TOrderReportReplyRecordCoverOrBracket OrderReportReplyRecCoverOrBracket;//78         // Himanshu  // sumitp 28_jan_22
            public ITInteractiveStructures.TTradeReportReplyRecNewCover TradeReportReplyRecNewCover;//58         // sumit   // sumitp 28_jan_22
            public ITInteractiveStructures.TOrderTypeTradeReportReplyRecNew OrderTypeTradeReportReplyRecNew; //New 61 for Spread - Added on 18-Apr-2018 - Rohan
            public ITInteractiveStructures.TOrderTypeTradeReportReplyRecNewCover OrderTypeTradeReportReplyRecNewCover; //New 58 Cover Trade sumit // sumitp 28_jan_22
            public ITInteractiveStructures.TOrderTypeReportsReplyRecNew OrdeTypeReportsReplyRecNew; //New 62 for Spread - Added on 18-Apr-2018 - Rohan
            public ITInteractiveStructures.TActivityLogReplyRecordNewForRequestorNew OrderTypeActivityLogReplyRecForRequestorNew;   //New 64 for Spread - Added on 18-Apr-2018 - Rohan

            public ITInteractiveStructures.TOrderTypeReportsReplyRecNewWithRejetOrder OrdeTypeReportsReplyRecNewWithRejectOrder;

            public ITInteractiveStructures.TOrderTypeTradeReportReplyRec OrderTypeTradeReportReplyRec; //71
            public ITInteractiveStructures.TOrderTypeReportsReplyRec OrdeTypeReportsReplyRec;//72
            public ITInteractiveStructures.TActivityLogReplyRecord ActivityLogReplyRec;//64
            public ITInteractiveStructures.TActivityLogReplyRecordNew OrderTypeActivityLogReplyRec;//104
            public ITInteractiveStructures.TActivityLogReplyRecordNewForRequestor OrderTypeActivityLogReplyRecForRequestor;//204
            public ITInteractiveStructures.TOrderReportReplyRecord AHOrderReportReplyRec; // 65 procedures ignored
            public ITInteractiveStructures.TLicenceInfoReplyRecord LicenceInfoReplyRec; //66
            public ITInteractiveStructures.TClientDbNumReportReplyRecord ClientDbNumReplyRec; //68
            public ITInteractiveStructures.THoldingsReportReplyRecord HoldingsReportReplyRec; //69
            public ITInteractiveStructures.THoldingsValuationReportRecord HoldingsValuationReportReplyRec; //70
            public ITInteractiveStructures.THoldingsValuationReportWithMemVar HoldingsValuationReportWithMemVarReplyRec; //170 //20.05.15
            public ITInteractiveStructures.TDBDateReplyRecord DBDateReplyRec; //73
            public ITInteractiveStructures.THoldingsReportReplyRecord_WithCollatral HoldingsReportWithCollatralReplyRec; //74
            public ITInteractiveStructures.THoldingsReportReplyRecordNew AllClientsHoldingsReportReplyRec; //75
            public ITInteractiveStructures.THoldingsEquityReportReplyRecordNew AllClientsHoldingsEquityReportReplyRec; //76 Added on 08-Nov-2016 - Rohan
            public ITInteractiveStructures.TClientPositionReportReplyRecord ClientPositionReplyRec;//79
            public ITInteractiveStructures.TTradeToTradeReplyRecord TradeToTradeReplyRec; //80
            public ITInteractiveStructures.TBseNseMapRecord BseNseMapReplyRec; //81
            public ITInteractiveStructures.TDerivBannedScripReplyRecord DerivBannedScripReplyRec; //82    
            public ITInteractiveStructures.TAdhocReportReplyRecord AdhocReportReplyRec; //83
            public ITInteractiveStructures.TAdhocModifyReplyRecord AdhocModifyReplyRec; //84
            public ITInteractiveStructures.TMaalFodoClientsReportReplyRecord MFClientsReportReplyRec;//86
            public ITInteractiveStructures.TMaalFodoClientHoldingReportReplyRecord MFClientHoldingReportReplyRec;//87
            public ITInteractiveStructures.TIlliquidReplyRecord IlliquidReplyRec; //89
            public ITInteractiveStructures.THoldingsReportReplyRecord_WithCollatralNew HoldingsReportWithCollatralReplyRecNew;//94
            public ITInteractiveStructures.THoldingsReportPreCloseReplyRecord_WithCollatralNew HoldingsReportWithCollatralPreCloseReplyRecNew;//106 jinal

            public ITInteractiveStructures.THoldingsReportPreCloseReplyRecord_WithCollatralNew_FS HoldingsReportWithCollatralPreCloseReplyRecNew_FS;//112 Himanshu
            public ITInteractiveStructures.THoldingsReportPreCloseReplyRecord_WithMTF HoldingsReportMTF;//67
            public ITInteractiveStructures.THoldingsReportPreCloseReplyRecord_WithMTFNew HoldingsReportMTFNew;//90
            public ITInteractiveStructures.TAllClientsHoldingsReportReplyRecord AllClientsHoldingsReportReplyRecNew;//95
            public ITInteractiveStructures.TAllClientsHoldingsReportReplyRecordWithPreClose AllClientsHoldingsReportReplyRecWithPreClose;//105
            public ITInteractiveStructures.TVerifyUserReplyRecord VerifyUserReplyRec; // 99
            public ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord DerivOrderPlaceBrokerReplyRec; //110,115
            public ITInteractiveStructures.TDerivOrderModifyBrokerReplyRecord DerivOrderModifyBrokerReplyRec;//120
            public ITInteractiveStructures.TDerivOrderCancelBrokerReplyRecord DerivOrderCancelBrokerReplyRec;//130
            public ITInteractiveStructures.TMarginReportReplyRecord MarginReportReplyRec;//200
            public ITInteractiveStructures.TMarginReportReplyRecord MarginReportReplyRecMCX;//206
            public ITInteractiveStructures.TMarginReportReplyRecordCombined MarginReportReplyRecCombined;//208
            public ITInteractiveStructures.TSLTriggerRecord SLTriggerRec;
            public ITInteractiveStructures.TDerivOrderExchReplyRecord DerivOrderExchReplyRec;//111 //31.12.14 Added

            public ITInteractiveStructures.TAllClientMarginReportReplyRecord AllClientMargin;
            public ITInteractiveStructures.ClientsForMTMSquareOff ClientsforMTMReply;
            public ITInteractiveStructures.UpdateClientsReply UpdateMTMLimitHitReply;
            public ITInteractiveStructures.MTMClientsWithAllMargins MTMClientsWithAllMarginsReply;
            public ITInteractiveStructures.MTMClientsWithAllMargins MTMClientsWithAllMarginsMcxReply;  //Added on 19-Sep-2016 - Rohan - MTM
            public ITInteractiveStructures.MTMClientsWithAllMarginsInstrumentWise MTMClientsWithAllMarginsReplyInstrumentWise;
            public ITInteractiveStructures.PeakMarginReplyData PeakMarginReplyData;

            public ITInteractiveStructures.TSpreadOrderPlaceBrokerReplyRecord SpdOrderPlaceBrokerReplyRec;
            public ITInteractiveStructures.TSpreadOrderCancelBrokerReplyRecord SpdOrderCancelBrokerReplyRec;
            public ITInteractiveStructures.TSpreadOrderModifyBrokerReplyRecord SpdOrderModifyBrokerReplyRec;
            public ITInteractiveStructures.TSpreadOrderExchReplyRecord SpdOrderExchReplyRec;

            #region Nse Currency Derive
            public ITInteractiveStructures.TCurrDerivOrderExchReplyRecord CurrDerivOrderExchReplyRec;//212

            public ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord CurrDerivOrderPlaceBrokerReplyRec; //211
            public ITInteractiveStructures.TCurrDerivOrderModifyBrokerReplyRecord CurrDerivOrderModifyBrokerReplyRec;//221
            public ITInteractiveStructures.TCurrDerivOrderCancelBrokerReplyRecord CurrDerivOrderCancelBrokerReplyRec;//231
            #endregion

            #region LotQty
            public ITInteractiveStructures.TLotQuantityRecord LotQtyReplyRec;
            #endregion

            public TOptionExerciseBrokerReplyRecord OptionExerciseBrokerReplyRec;//181,182,183
            public TOptionExerciseExchReplyRecord OptionExerciseExchReplyRec;//184,183,184
            public ITInteractiveStructures.TExerciseReportReplyRecord OptionExerciseReportReplyRec;//63

            public ITInteractiveStructures.TOrderTradeGeneral IISOrderTradeGeneral;
            public ITInteractiveStructures.TMarginReportReplyRecordTemplate TemplateMarginReply;
            public ITInteractiveStructures.TMarginReportReplyRecordTemplate_Peak TemplateMarginReply_Peak;
            public ITInteractiveStructures.TMarginReportReplyRecordTemplateWithDerivMargin TemplateWithDerivMargin;
            #region Nse Slbm
            public TSLBMOrderPlaceBrokerReplyRecord SLBMOrderPlaceBrokerReplyRec;   //235
            public TSLBMOrderExchReplyRecord SLBMOrderExchReplyRec;                 //238/239/240/243
            public TSLBMOrderModifyBrokerReplyRecord SLBMOrderModifyBrokerReplyRec; //236
            public TSLBMOrderCancelBrokerReplyRecord SLBMOrderCancelBrokerReplyRec; //237
            public TSLBMHoldingsReportReplyRecord SLBMHoldingsReportReplyRec; //241
            #endregion

            #region Add/Modify/Cancel BseMF Kruitka
            public ITInteractiveStructures.TBseMFAddModifyCancelBrokerResponse MFAddModifyCancelBrokerResRec;//BseMf
            public ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply MFAddModifyCancelExchangeResRec;//BseMf
            public ITInteractiveStructures.TBseMFOrderTypeReportsReplyRec BseMfOrdeTypeReportsReplyRec;
            public ITInteractiveStructures.TBseMFActivitylogReplyRec TBseMFActivitylogReplyRec;
            public ITInteractiveStructures.THoldingsReportReplyRecord_MF HoldingsReportofBseMfReplyRec; //92
            public ITInteractiveStructures.TBseMFSIPRegistrationMsgBrokerReply MFSIPRegistrationBrokerResRec;
            public ITInteractiveStructures.XSIPMandateBrokerReply XSIPMandateBrokerReplyRec;
            public ITInteractiveStructures.TBseMFSIPRegistrationExchangeReplyRecord MFSIPRegistrationExchangeResRec;
            public ITInteractiveStructures.TBseMFSIPOrderTypeReportsReplyRec BseMfSIPOrdeTypeReportsReplyRec;
            public ITInteractiveStructures.MandateOrderTypeReportsReplyRec MandateOrderTypeReportsReplyRec;
            public ITInteractiveStructures.XBseMFSIPRegistrationMsgRequestRecord XMfSIPRegistrationReqRec;
            public ITInteractiveStructures.XSIPMandateExchangeReply XSIPMandateExchangeResRec;
            #endregion
            public ITInteractiveStructures.TLoginReplyRecordWithName LoginReplyWithName; //4
            public ITInteractiveStructures.TAllowSegmentReply SegmentReply;
            public ITInteractiveStructures.TradeExchReportReply TradeReportReply; // 226 
            public ITInteractiveStructures.LastActivityRef LastActReference;
            public ITInteractiveStructures.OFSWatchListReply OFSWatchListReply; // 201 
            public ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS OfsPlaceOrderReply; // 201 
            public ITInteractiveStructures.TOrderTypeReportsReplyRecOFS OfsOrdersReportsReply;
            public ITInteractiveStructures.TOrderTypeReportsReplyRecDealer OrdersReportDealer;
            public ITInteractiveStructures.TTradeReportReplyRecDealer TradesReportDealer;

            public ITInteractiveStructures.OrderPlaceReplyRecOfApproval orderPlaceReplyRecOfApproval; //02.03.23
            public ITInteractiveStructures.UpdateOTPOrderStatus UpdateOTPOrderStatusRec;
            public ITInteractiveStructures.PendingApprovalOrderReport PendingApprovalOrderReportReplyRec;            
            public ITInteractiveStructures.UpdateOTPOrderStatusResponse UpdateOTPOrderStatusResponseRec;

            public ITInteractiveStructures.SurveillanceIndicatorMessagesNse SurveillanceIndicatorMessagesNseReplyRec; //160 20.03.23
            public ITInteractiveStructures.SurveillanceIndicatorMessagesBse SurveillanceIndicatorMessagesBseReplyRec; //161 20.03.23

            public ITInteractiveStructures.GetIVRDealerDetails GetIVRDealerDetailsReply; //14

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TLoginReplyRecordWithName                                            //Request & Reply =  3 - LOGIN 04/12/2013
        {
            public byte FEUserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] FEUserID;
            public byte FEUserNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string FEUserName;
            public int Reserved1;
            public int Reserved2;
            public long Reserved3;
            public long Reserved4;
            public long Reserved5;
            public byte Reserved6;
            public byte Reserved7;
            public byte Reserved8;
            public byte Reserved9;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public char[] Reserved10;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TOTSRequestRecordtemp
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] IP;
            public DateTime ReqTime;
            public DateTime ThreadStartTime;
            public TIARequestHeaderRecord ReqHdr;
            public object AllReq;
            public ushort RetryCnt;
            public int ITS_Num;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDbInfoRecord
        {
            public DateTime CurrTime;
            public int ThNum, PendReqCnt, ThreadsInUse, ReqType, TimeTaken, Status;
            public byte ReqStrLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string ReqStr;
            public byte ExStrLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string ExStr;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TNewRequestRecord
        {
            // public ITInteractiveStructures.TOTSRequestRecord OTSReqRec;
            //public byte[] OTSReqRecArr;
            public byte RqStrLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string RqStr;
            public byte ExStrLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string ExStr;
            public int ExType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TOTSRequestRecord1
        {
            public byte IPLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] IP;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public byte[] Dates;
            public TIARequestHeaderRecord ReqHdr;
            public TAllRequestNewRecord AllReq;
            public ushort RetryCnt;
            public int ITS_Num;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct Dates
        {
            public DateTime ReqTime;
            public DateTime ThreadStartTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct RawHeader
        {
            public int ReadSize;
            public DateTime CurrTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//19.01.17
        public struct THoldingsReportReplyRecord_WithCollatralNew
        {
            public char Exch;
            public char ExchType;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
            public int Collatral;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//106 jinal 
        public struct THoldingsReportPreCloseReplyRecord_WithMTF
        {
            public char Exch;
            public char ExchType;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
            public int FundedStockQty;
            public int Collatral;
            public double PrevClose;
            public int PendingQty;           
            //public long Reserved2;
            public double PurchasePrice; // added by Neeta 20.03.2019
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//112 Himanshnu 
        public struct THoldingsReportPreCloseReplyRecord_WithCollatralNew_FS
        {
            public char Exch;
            public char ExchType;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
            public int FundedStockQty; // Add by Himanshu - 01.10.2022
            public int Collatral;
            public double PrevClose;
            public int PendingQty;
            //public long Reserved2;
            public double PurchasePrice; // added by Neeta 20.03.2019
            public int FSCollateralQty; // Add - Himanshu - 17.06.2022
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//106 jinal 
        public struct THoldingsReportPreCloseReplyRecord_WithMTFNew
        {
            public char Exch;
            public char ExchType;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
            public int FundedStockQty;
            public int Collatral;
            public double PrevClose;
            public int PendingQty;
            public int ExchRcvQty2;
            //public long Reserved2;
            public double PurchasePrice; // added by Neeta 20.03.2019
            public int FSCollateralQty; // Add - Himanshu - 17.06.2022  // Uncommented by Rhythm for Bigul Utilized MTF on 14-08-24
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//106 jinal 
        public struct THoldingsReportPreCloseReplyRecord_WithCollatralNew
        {
            public char Exch;
            public char ExchType;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
            //public int FundedStockQty; // Add by Himanshu - 01.10.2022
            public int Collatral;
            public double PrevClose;
            public int PendingQty;
            //public long Reserved2;
            public double PurchasePrice; // added by Neeta 20.03.2019
            //public int FSCollateralQty; // Add - Himanshu - 17.06.2022
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//09.01.17
        public struct TAllClientsHoldingsReportReplyRecord
        {
            public char Exch;
            public char ExchType;
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string TraderID;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//09.01.17
        public struct TAllClientsHoldingsReportReplyRecordWithPreClose
        {
            public char Exch;
            public char ExchType;
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string TraderID;
            public int BseCode;
            public int NseCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ScripName;
            public int TotalQty;
            public int SoldQty;
            public int BrokerBeniQty;
            public int ExchRcvQty;
            public int ClientDPQty;
            public double PrevClose;
            public int Reserved1;
            public double PurchasePrice;
        }


        #region New Order Types For intraDelivery
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     //1
        public struct TOrderPlaceRequestRecord
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public byte IOC;
            public int DisclosedQty;
            public byte OrderType;
            public byte ProductType;
            public int AlgoID;
            public byte AllowBannedScrip;
            public byte Reserved2;
            public short Reserved3;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //Cover Order Structure    // sumitp 28_jan_22
        public struct TOrderPlaceRequestRecordCoverAndBracket
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;

            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;

            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public byte IOC;
            public int DisclosedQty;

            public byte OrderType;
            public byte ProductType;



            public byte OrderTypeNew; // Co/Bo type new
            public Single SLRateCO;//Rate for CO
            public int EpochTime; //  EpochTime for order identification COVER

            //public Single SLRateBUY;//Rate For BO
            //public Single SLRateSELL;//Rate For BO

            public int Reserved1;
            public byte Reserved2;
            public byte AllowBannedScrip;
            public short Reserved3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     //
        public struct TOrderModifyRequestRecord
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public char SLTriggered;
            public int OldQty;
            public char AHStatus;
            public byte IOC;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int DisclosedQty;
            public byte OrderType;
            public byte ProductType;
            public int AlgoID;
            public int Reserved2;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     //
        public struct TOrderCancelRequestRecord
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public byte IOC;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public ushort TraderRequestID;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public char AHStatus;
            public int DisclosedQty;
            public byte OrderType;
            public byte ProductType;
            public int AlgoID;
            public int Reserved2;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]   //14
        public struct TOrderPlaceBrokerReplyRecord
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public int AlgoID;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]   //14    // sumitp 28_jan_22
        public struct TOrderPlaceBrokerReplyRecord_Cover    
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;




            public byte OrderTypeNew; // Co/Bo type new
            public Single SLRateCO;//Rate for CO
            public long EpochTime; //  EpochTime for order identification COVER
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]   //171    // sumitp 28_jan_22
        public struct TOrderPlaceBrokerReplyRecordCoverAndBracket
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell1;
            public char BuySell2;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public ushort TraderRequestID;
            public int BrokerOrderID1;
            public int BrokerOrderID2;
            public int BrokerTime1;
            public int BrokerTime2;
            public Single MarginBlocked;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;

            public byte OrderTypeNew; // Co/Bo type new
            public Single SLRateCO;//Rate for CO
            public long EpochTime; //  EpochTime for order identification COVER
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]   // 24
        public struct TOrderModifyBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public char SLTriggered;
            public Single TriggerRate;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public ushort TraderRequestID;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public byte IOC;
            public int DisclosedQty;
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public int AlgoID;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     //34
        public struct TOrderCancelBrokerReplyRecord
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;
            public ushort TraderRequestID;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public char AHStatus;
            public int OldQty;
            public int DisclosedQty;
            public byte OrderType;
            public byte ProductType;
            public int AlgoID;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     //35
        public struct TOrderExchReplyRecord
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchReasonCode;
            public int ExchErrorCode;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] Status;
            public byte IOC;
            public int DisclosedQty;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
        }

        //[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        //public struct TOrderConversionRec
        //{
        //    public char Exch;
        //    public char ExchType;
        //    public int ScripCode;
        //    public byte ScripNameLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        //    public char[] ScripName;
        //    public char BuySell;
        //    public int Qty;
        //    public long ExchangeOrderID;
        //    public byte OrderType;
        //    public byte ProductType;
        //    public int Reserved1;
        //    public int Reserved2;
        //}

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderConversionRec
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public int AvailableQty;
            public Single Rate;
            public long ExchangeOrderID;
            public long ExchangeTradeID;   
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]   // 24
        public struct TOrderConversionRecReply
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            public byte InstrType;
            public byte CPType;
            public byte CALevel;
            public int ExpiryDate;
            public Single StrikePrice;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int BrokerOrderID;
            public int BrokerTime;
            public Single MarginBlocked;            
            public long ExchangeOrderId;
            public long ExchangeTradeID;       
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderTypeReportsReplyRec
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //18-Apr-2018 - Rohan - New Stucture for Spread
        public struct TOrderTypeReportsReplyRecNew
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
            public bool IsSpread;
            public int ScripCode2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //09-May-2019 - Ashish - New Stucture for RejectedReason
        public struct TOrderTypeReportsReplyRecNewWithRejetOrder
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char AHStatus;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
            public bool IsSpread;
            public int ScripCode2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] RejectReason;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string OrderRequestorCode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderTypeTradeReportReplyRec
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int OrgQty;
            public int PendingQty;
            public long ExchangeOrderID;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //18-Apr-2018 - Rohan - New Stucture for Spread
        public struct TOrderTypeTradeReportReplyRecNew
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int OrgQty;
            public int PendingQty;
            public long ExchangeOrderID;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
            public bool IsSpread;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //sumit Cover trade report   // sumitp 28_jan_22
        public struct TOrderTypeTradeReportReplyRecNewCover
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public Single Rate;
            public int OrgQty;
            public int PendingQty;
            public long ExchangeOrderID;
            public int ExchangeTradeID;
            public int ExchangeTradeTime;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
            public bool IsSpread;

            public byte OrderTypeNew;
            public int EpochTime;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TActivityLogReplyRecordNew
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int BrokerOrderID; //of Original Place Order
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 19)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char ReqType; //changed from 20 to 19 on 9/2/5 and added ReqType
            public int OldOrderQty;
            public int TradedQty;
            public int PendingQty;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //24.09.16 Hetal
        public struct TActivityLogReplyRecordNewForRequestor
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int BrokerOrderID; //of Original Place Order
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 19)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char ReqType; //changed from 20 to 19 on 9/2/5 and added ReqType
            public int OldOrderQty;
            public int TradedQty;
            public int PendingQty;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
            public byte RequestorCodeLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string RequestorCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
            public string Reserved3;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
            public string Reserved4;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //18-Apr-2018 - Rohan - New Stucture for Spread
        public struct TActivityLogReplyRecordNewForRequestorNew
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public char WithSL;
            public Single TriggerRate;
            public char SLTriggered;
            public int BrokerOrderID; //of Original Place Order
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 19)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public char ReqType; //changed from 20 to 19 on 9/2/5 and added ReqType
            public int OldOrderQty;
            public int TradedQty;
            public int PendingQty;
            public byte OrderType;
            public byte ProductType;
            public int Reserved1;
            public int Reserved2;
            public byte RequestorCodeLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string RequestorCode;
            public bool IsSpread;
            public int ScripCode2;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
            public string Reserved3;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
            public string Reserved4;
        }

        #endregion

        #region Spread Structures Start By Sandeep Jadhav

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TLegInfoRecord
        {
            public int Token;
            public char BuySell;
            public int Qty;
            public byte InstrType;
            public byte SymbolLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Symbol;
            public int ExpiryDate;
            public Single StrikePrice;
            public byte CPType;
            public byte CALevel;
            public int Reserved1;
            public byte Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TSpreadOrderPlaceRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv 'S' For Spread
            public int ScripCode;
            public float RateDiff;
            public char AtMarket;
            public ushort TraderRequestID;
            public byte DayIOC;
            public short OrderCategory;
            public int Reserved;
            public byte ProductType;

            public TLegInfoRecord SpdLeg1;
            public TLegInfoRecord SpdLeg2;
            public TLegInfoRecord SpdLeg3;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TSpreadOrderPlaceBrokerReplyRecord
        {
            public char Exch;
            public char ExchType;
            public int ULToken;
            public float RateDiff;
            public char AtMarket;
            public ushort TraderRequestID;
            public byte IOC;
            public short Reserved;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public Single MarginBlocked;
            public char AHStatus;

            public byte OrderType;//0 â€“ Intraday & 1 â€“ Delivery.
            public byte ProductType;//Reserved Field.
            public int Reserved1;
            public int Reserved2;

            public TLegInfoRecord SPREAD_LEG_1;
            public TLegInfoRecord SPREAD_LEG_2;
            public TLegInfoRecord SPREAD_LEG_3;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TSpreadOrderCancelRequestRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv And Spread
            public int ScripCode;
            public float RateDiff;
            public char AtMarket;
            public ushort TraderRequestID;
            public short OrderCategory;
            public char AHStatus;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int Reserved;
            public byte ProductType;

            public TLegInfoExchReplyRec SpdLeg1;
            public TLegInfoExchReplyRec SpdLeg2;
            public TLegInfoExchReplyRec SpdLeg3;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TSpreadOrderCancelBrokerReplyRecord
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv And Spread
            public int ULToken;
            public float RateDiff;
            public char AtMarket;
            public ushort TraderRequestID;
            public byte IOC;
            public short Reserved;


            public long ExchangeOrderId;
            public int ExchangeOrderTime;

            public int BrokerOrderID;
            public int BrokerOrderTime;
            public Single MarginBlocked;
            public char AHStatus;

            public byte OrderType;//0 â€“ Intraday & 1 â€“ Delivery.
            public byte ProductType;//Reserved Field.
            public int Reserved1;
            public int Reserved2;


            public TLegInfoExchReplyRec SPREAD_LEG_1;
            public TLegInfoExchReplyRec SPREAD_LEG_2;
            public TLegInfoExchReplyRec SPREAD_LEG_3;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TSpreadOrderModifyBrokerReplyRecord
        {
            public char Exch;
            public char ExchType;
            public int ULToken;
            public float RateDiff;
            public char AtMarket;
            public ushort TraderRequestID;
            public byte IOC;
            public short Reserved;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public Single MarginBlocked;
            public char AHStatus;

            public byte OrderType;//0 â€“ Intraday & 1 â€“ Delivery.
            public byte ProductType;//Reserved Field.
            public int Reserved1;
            public int Reserved2;

            public TLegInfoExchReplyRec SPREAD_LEG_1;
            public TLegInfoExchReplyRec SPREAD_LEG_2;
            public TLegInfoExchReplyRec SPREAD_LEG_3;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TSpreadOrderExchReplyRecord
        {
            public char Exch;
            public char ExchType;
            public int ULToken;
            public float RateDiff;
            public char AtMarket;
            public ushort TraderRequestID;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderId;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchErrorCode;
            public int ExchReasonCode;
            public byte IOC;
            public short Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status;
            public byte StatusLength;

            public byte OrderType; //0 â€“ Intraday & 1 â€“ Delivery.
            public byte ProductType;//Reserved Field.
            public int Reserved1;
            public int Reserved2;


            public TLegInfoExchReplyRec SPREAD_LEG_1;
            public TLegInfoExchReplyRec SPREAD_LEG_2;
            public TLegInfoExchReplyRec SPREAD_LEG_3;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TLegInfoExchReplyRec
        {
            public int Token;
            public char BuySell;
            public int Qty;
            public int OldQty;
            public int TradedQty;
            public int PendingQty;
            public byte InstrType;
            public byte SymbolLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public char[] Symbol;
            public int ExpiryDate; //Seconds from 01/01/80
            public Single StrikePrice;
            public byte CPType;
            public byte CALevel;
            public int Reserved1;
            public byte Reserved2;
        }

        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderTradeGeneral
        {
            public int DateOfDay;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string ActiveUSers;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TTradesReportExchFormatRequest
        {
            public char Exch;
            public char ExchType;
            public byte FEIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] FEUserID;
            public byte Type;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TradeExchReportReply
        {
            public Int16 BrokerId;
            public byte UserIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] UserID;
            public int ScripCode;
            public byte SymbolLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string Symbol;
            public Single TradedPrice;
            public int TradedQty;
            public byte OppositeMemberIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] OppositeMemberID;
            public byte OppositeTraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] OppositeTrader;
            public DateTime TradeEntryDate;
            public DateTime TradeEntryTime;
            public byte ClientIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] ClientID;
            public long OrderNumber;
            public char TransactionType;
            public char BuySell;
            public int TradeNumber;
            public byte CAClassLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] CAClass;
            public DateTime OrderTime;
            public byte TradeStatusLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] TradeStatus;
            public byte SeriesLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte SecurityNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] SecurityName;
            public byte InstrumentType;
            public byte BookType;
            public byte MarketType;
            public byte BranchIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] Branch;
            public byte ProClient;
            public byte ClientAcLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] ClientAc;
            public Int16 ParticipantCode;
            public byte AuctionPartType;
            public byte AuctionNumber;
            public char SettPeriod;
            public DateTime TradeEntryDateTime;
            public DateTime TradeModifyDateTime;
            public DateTime OrderInsertTime;
            public long CTCLUserInfoNC;
            public byte CPIdLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] CPId;
            public byte InstrumentNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public char[] InstrumentName;
            public byte ClientCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] ClientCode;
            public byte CCodeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] CCode;
            public DateTime ExpiryDate;
            public Single StrikePrice;
            public byte OptionTypeLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] OptionType;
            public byte BookTypeNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public char[] BookTypeName;
            public byte OpenCloseLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public char[] OpenClose;
            public byte CoverUncoverLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public char[] CoverUncover;
            public long CTCLUserInfoND;
            public int Reserved1;
            public int Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct LastActivityRef
        {
            public long LastActivityReference;
        }


        #region OFS Structure
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct OFSWatchListReply
        {
            public byte SymbolLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string Symbol;
            public byte SecurityNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
            public string SecurityName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string IsinCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string Series;
            public int IssueSize;
            public int FaceValue;
            public double TickSize;
            public int RegularLotSize;
            public double MinimumPercentageDay;
            public double MaximumPercentageDay;
            public double BasePrice;
            public int OpenOnDate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string MktOpenTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string MktCloseTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string MktModCxlOpenTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string MktModCxlCloseTime;
            public bool IsNewOrderAllowed;
            public bool IsModificationAllowed;
            public bool IsCancellationAllowed;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string QtyModAllowed;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string PriceModAllowed;
            public bool IsAccountNoModAllowed;
            public bool IsCpCodeModAllowed;
            public long OrderValueUpperLimit;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderPlaceModifyRequestRecordOFS
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;

            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;
            public byte InstrType;

            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public ushort TraderRequestID;

            public byte OrderType;
            public byte ProductType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string ClientType;
            public int MarginType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string ParticipantId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
            public string OrderId;

            public int Reserved1;
            public short Reserved3;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TOrderPlaceModifyCancelReplyRecordOFS
        {
            public char Exch;
            public char ExchType;
            public int ScripCode;

            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ScripName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Series;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public char[] OrderId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] TradingMemberId;
            public int BranchId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string ClientType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string ParticipantId;

            public int MarginType;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public Double Value;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] OrderStatus;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] PlacedBy;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] PlacedDate;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] ModifiedBy;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] ModifiedDate;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public char[] DeletedBy;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] DeletedDate;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] BatchId;
            public int BatchSerialNo;

            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public int ExchReplyCode;
            public int ExchReasonCode;
            public int ExchErrorCode;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] Status;

            public ushort TraderRequestID;

            public byte OrderType;
            public byte ProductType;

            public char OfsOrderType;

            public int Reserved1;
            public short Reserved3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //OFS ORDER REPORT
        public struct TOrderTypeReportsReplyRecOFS
        {
            public char Exch;
            public char ExchType; // 'C' for Cash, 'D' for Deriv
            public int ScripCode;
            public byte ScripNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string ScripName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string Series;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string ClientType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string ParticipantId;
            public int MarginType;
            public char BuySell;
            public int Qty;
            public char AtMarket;
            public Single Rate;
            public int TradedQty;
            public int PendingQty;
            public int BrokerOrderID;
            public int BrokerOrderTime;
            public long ExchangeOrderID;
            public int ExchangeOrderTime;
            public byte StatusLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string Status; //'Sent To Exch','Executed','Cancelled
            public byte OrderType;
            public byte ProductType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public char[] RejectReason;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string OrderRequestorCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string PlacedDate;
            public int PlacedBy;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string ModifiedDate;
            public int ModifiedBy;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string CancelledDate;
            public int CancelledBy;

            public int OperationUser;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string OperationTimestamp;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string OperationType;
            public byte IsCpOrder;
            public byte IsCpApproved;
            public int CpErrorCode;
        }
        #endregion

        #region  Surveillance Indicator Messages 20.03.23
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct SurveillanceIndicatorMessagesNse
        {
            public char Exch;
            public int ScripCode;
            public byte ISINLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ISIN;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
            public string SurveillanceMessages;
            public int GsmFlag;
            public char Reserved1;
            public int Reserved2;
            public int Resesrved3;
            public byte Reserved4;
            public bool Reserved5;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct SurveillanceIndicatorMessagesBse
        {
            public char Exch;
            public int ScripCode;
            public byte ISINLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public char[] ISIN;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
            public string SurveillanceMessages;
            public int GsmFlag;
            public char Reserved1;
            public int Reserved2;
            public int Resesrved3;
            public byte Reserved4;
            public bool Reserved5;

        }
        #endregion


        #region Get IVR Dealer Status Reply
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct GetIVRDealerDetailsReq
        {
            public int StartTime;
            public int EndTime;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct GetIVRDealerDetails //21.03.23
        {
            public bool IsIVRDealer;
            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public char Reserved4;
            public char Reserved5;
            public char Reserved6;
        }
        #endregion

    }


}
