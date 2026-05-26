using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Net;
using CrashReporterDotNET;

namespace IntellectUtilities
{
    public static class ConstantUnits
    {
        public const int MAXEXCH = 3;
        public const int PORT_OTS = 18101;
        public const int PORT_ITS = 18002;
        public const int PORT_CTCL = 18301;
        public const int PORT_IML = 18401;

        public const int REQ_ITS_TO_OTS_CLIENT_DISCONN = 252;
        public const int REQ_ITS_TO_OTS_CLIENT_DISCONN_2FA = 247;
        public const int REQ_ITS_TO_OTS_CONN = 245;
        public const int REQ_ITS_TO_OTS_T2T_SCRIPS = 80;
        public const int REQ_ITS_TO_OTS_BSE_NSE_MAP = 81;
        public const int REQ_ITS_TO_OTS_BANNED_SCRIPS_DERIV = 82;
        public const int REQ_ITS_TO_OTS_ILLIQUID_SCRIPS = 89;

        public const int REQ_FE_TO_ITS_LOGIN = 1;
        public const int REQ_FE_TO_ITS_CHANGE_PASS = 2;
        public const int REQ_FE_TO_ITS_LOGIN_NEW = 3;
        public const int REQ_FE_TO_ITS_LOGIN_NEW_TwoFactor = 85;
        public const int REQ_FE_TO_ITS_ADD_TRADER = 5;
        public const int REQ_FE_TO_ITS_VARIFY_TRADER = 6;
        public const int REQ_FE_TO_ITS_HANDSHAKE = 253;

        public const int REP_ITS_TO_FE_LOGIN = 1;
        public const int REP_ITS_TO_FE_CHANGE_PASS = 2;
        public const int REP_ITS_TO_FE_LOGIN_NEW = 3;
        public const int REP_ITS_TO_FE_ADD_TRADER = 5;
        public const int REP_ITS_TO_FE_TRADERLIST = 5;//Atish 30.03.2019
        public const int REP_ITS_TO_FE_VARIFY_TRADER = 6;
        public const int REP_ITS_TO_FE_DUPCONN = 9;
        public const int REP_ITS_TO_FE_PLACE_ORD_EQ_CONFIRM_BROKER = 10;
        public const int REP_ITS_TO_FE_PLACE_ORD_EQ_CONFIRM_EXCH = 11;
        public const int REP_ITS_TO_FE_REJECT_AH_ORD_CONFIRM_BROKER = 16;
        public const int REP_ITS_TO_FE_MODIFY_ORD_EQ_CONFIRM_BROKER = 20;
        public const int REP_ITS_TO_FE_MODIFY_ORD_EQ_CONFIRM_EXCH = 21;
        public const int REP_ITS_TO_FE_CANCEL_ORD_EQ_CONFIRM_BROKER = 30;
        public const int REP_ITS_TO_FE_CANCEL_ORD_EQ_CONFIRM_EXCH = 31;
        public const int REP_ITS_TO_FE_KILLED_ORD_EQ_CONFIRM_EXCH = 32;
        public const int REP_ITS_TO_FE_KILLED_IOC_EQ_CONFIRM_EXCH = 33;
        public const int REP_ITS_TO_FE_SCRIP_MGN = 41;
        public const int REP_ITS_TO_FE_TRD_EQ_CONFIRM_EXCH = 50;
        public const int REP_ITS_TO_FE_SL_EQ_TRIGGER_EXCH = 51;
        public const int REP_ITS_TO_FE_RPT_ORDERS_WITH_REJECTED_ORDERS = 52;
        public const int REP_ITS_TO_FE_RPT_TRADES = 61;
        public const int REP_ITS_TO_FE_RPT_ORDERS = 62;
        public const int REP_ITS_TO_FE_RPT_ACTIVITY_LOG = 64;
        public const int REP_ITS_TO_FE_RPT_HOLDINGS = 69;
        public const int REP_ITS_TO_FE_RPT_HOLDING_MGN = 70;
        public const int REP_ITS_TO_FE_RPT_HOLDING_MGN_MTF = 218;
        public const int REP_ITS_TO_FE_RPT_TRADES_NEW = 71;
        public const int REP_ITS_TO_FE_RPT_ORDERS_NEW = 72;
        public const int REP_ITS_TO_FE_RPT_HOLDINGS_WITHCOLLATERAL = 74;
        //public const int REP_ITS_TO_FE_RPT_LEDGER                 = 79;
        public const int REP_ITS_TO_FE_RPT_T2T_SCRIPS = 80;
        public const int REP_ITS_TO_FE_RPT_BSE_NSE_MAP = 81;
        public const int REP_ITS_TO_FE_RPT_BANNED_SCRIPS_DERIV = 82;
        public const int REP_ITS_TO_FE_RPT_ADHOC = 83;
        public const int REP_ITS_TO_FE_RPT_ADHOC_MODIFY = 84;
        public const int REP_ITS_TO_FE_RPT_ILLIQUID_SCRIPS = 89;

        public const int REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL = 95;
        public const int REP_ITS_TO_FE_RPT_ACTIVITY_LOG_NEW = 104;
        public const int REP_ITS_TO_FE_PLACE_ORD_FO_CONFIRM_BROKER = 110;
        public const int REP_ITS_TO_FE_PLACE_ORD_FO_CONFIRM_EXCH = 111;
        public const int REP_ITS_TO_FE_MODIFY_ORD_FO_CONFIRM_BROKER = 120;
        public const int REP_ITS_TO_FE_MODIFY_ORD_FO_CONFIRM_EXCH = 121;
        public const int REP_ITS_TO_FE_CANCEL_ORD_FO_CONFIRM_BROKER = 130;
        public const int REP_ITS_TO_FE_CANCEL_ORD_FO_CONFIRM_EXCH = 131;
        public const int REP_ITS_TO_FE_KILLED_IOC_FO_CONFIRM_EXCH = 133;
        public const int REP_ITS_TO_FE_TRD_FO_CONFIRM_EXCH = 150;
        public const int REP_ITS_TO_FE_SL_FO_TRIGGER_EXCH = 151;
        public const int REP_ITS_TO_FE_MESSAGE = 199;
        public const int REP_ITS_TO_FE_LINK_FAIL = 251;
        public const int REP_ITS_TO_FE_HANDSHAKE = 253;
        public const int REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL_New = 105;
        #region SLBM
        public const int REP_ITS_TO_FE_PLACE_ORD_SLBM_CONFIRM_BROKER = 235;
        public const int REP_ITS_TO_FE_MODIFY_ORD_SLBM_CONFIRM_BROKER = 236;
        public const int REP_ITS_TO_FE_CANCEL_ORD_SLBM_CONFIRM_BROKER = 237;
        public const int REP_ITS_TO_FE_PLACE_ORD_SLBM_CONFIRM_EXCH = 238;
        public const int REP_ITS_TO_FE_MODIFY_ORD_SLBM_CONFIRM_EXCH = 239;
        public const int REP_ITS_TO_FE_CANCEL_ORD_SLBM_CONFIRM_EXCH = 240;
        public const int REP_ITS_TO_FE_KILLED_IOC_SLBM_CONFIRM_EXCH = 243;
        #endregion

        #region Curr Derive
        public const int REP_ITS_TO_FE_PLACE_ORD_CURR_FO_CONFIRM_BROKER = 211;
        public const int REP_ITS_TO_FE_PLACE_ORD_CURR_FO_CONFIRM_EXCH = 212;
        public const int REP_ITS_TO_FE_MODIFY_ORD_CURR_FO_CONFIRM_BROKER = 221;
        public const int REP_ITS_TO_FE_MODIFY_ORD_CURR_FO_CONFIRM_EXCH = 222;
        public const int REP_ITS_TO_FE_CANCEL_ORD_CURR_FO_CONFIRM_BROKER = 231;
        public const int REP_ITS_TO_FE_CANCEL_ORD_CURR_FO_CONFIRM_EXCH = 232;
        public const int REP_ITS_TO_FE_KILLED_IOC_CURR_FO_CONFIRM_EXCH = 233;
        #endregion

        #region Spread Order Flags By Sandeep
        public const int REP_ITS_TO_FE_PLACE_ORD_SPREAD_CONFIRM_BROKER = 191;
        public const int REP_ITS_TO_FE_PLACE_ORD_SPREAD_CONFIRM_EXCH = 241;
        public const int REP_ITS_TO_FE_MODIFY_ORD_SPREAD_CONFIRM_BROKER = 192;
        public const int REP_ITS_TO_FE_MODIFY_ORD_SPREAD_CONFIRM_EXCH = 242;
        public const int REP_ITS_TO_FE_CANCEL_ORD_SPREAD_CONFIRM_BROKER = 193;
        public const int REP_ITS_TO_FE_CANCEL_ORD_SPREAD_CONFIRM_EXCH = 243;
        public const int REP_ITS_TO_FE_KILLED_IOC_SPREAD_CONFIRM_EXCH = 249;
        #endregion

        public const int REP_ITS_TO_FE_PLACE_ORD_CONFIRM_BROKER = 17;
        public const int REP_ITS_TO_FE_PLACE_COVER_ORD_CONFIRM_BROKER = 171;// sumitp 28_jan_22
        public const int REP_ITS_TO_FE_MODIFY_ORD_CONFIRM_BROKER = 18;
        public const int REP_ITS_TO_FE_CANCEL_ORD_CONFIRM_BROKER = 19;
        public const int REP_ITS_TO_FE_Order_CONFIRM_EXCH = 27;
        public const int REP_ITS_TO_FE_MODIFY_Order_CONFIRM_EXCH = 28;
        public const int REP_ITS_TO_FE_CANCEL_Order_CONFIRM_EXCH = 29;
        public const int REP_ITS_TO_FE_Order_CONVERSION_EXCH = 26;
        public const int REP_ITS_TO_FE_Order_CONVERSION_EXCH_WITH_EXPIRY = 25;

        public const int REP_OTS_TO_ITS_LOGIN = 1;
        public const int REP_OTS_TO_ITS_LOGIN_NEW = 3;
        public const int REP_OTS_TO_ITS_LOGIN_NEW_TwoFactor = 85;
        public const int REP_OTS_TO_ITS_ADD_TRADER = 5;
        public const int REP_OTS_TO_ITS_VARIFY_TRADER = 6;
        public const int REP_OTS_TO_ITS_DUPCONN = 9;
        public const int REP_OTS_TO_ITS_MFCLIENT_LIST = 86;

        public const int REP_ITS_TO_FE_EXERCISE_PLACE_REQ = 181;
        public const int REP_OTS_TO_ITS_EXERCISE_PLACE_REPLY = 184;
        public const int REP_ITS_TO_FE_EXERCISE_MODIFY_REQ = 182;
        public const int REP_OTS_TO_ITS_EXERCISE_MODIFY_REPLY = 185;
        public const int REP_ITS_TO_FE_EXERCISE_CANCEL_REQ = 183;
        public const int REP_OTS_TO_ITS_EXERCISE_CANCEL_REPLY = 186;

        public const int REP_OTS_TO_ITS_SURVIELLENCE_ORDER_REP = 59; //21.10.13

        public const int REP_OTS_IIS = 246;//By Sandeep For IIS

        public const int BSEMF_PLACE_BROKER_REPLY = 43;//BseMf Krutika
        public const int BSEMF_MODIFY_BROKER_REPLY = 44;
        public const int BSEMF_CANCEL_BROKER_REPLY = 45;
        public const int BSEMF_PLACE_EXCHANGE_REPLY = 46;
        public const int BSEMF_MODIFY_EXCHANGE_REPLY = 47;
        public const int BSEMF_CANCEL_EXCHANGE_REPLY = 48;
        public const int REP_ITS_TO_FE_RPT_ORDERS_BSEMF = 93;
        public const int REP_ITS_TO_FE_RPT_ACTIVITY_BSEMF = 100;
        public const int BSEMF_SIPREGISTRATION_BROKER_REPLY = 34;
        public const int BSEMF_SIPCANCELLATION_BROKER_REPLY = 35;
        public const int BSEMF_SIPREGISTRATION_EXCHANGE_REPLY = 36;
        public const int BSEMF_SIPCANCELLATION_EXCHANGE_REPLY = 37;
        public const int REP_ITS_TO_FE_SIP_RPT_ORDERS_BSEMF = 38;
        public const int REP_ITS_TO_FE_MANDATE_RPT_ORDERS_BSEMF = 189;
        public const int BSEMF_XSIPREGISTRATION_BROKER_REPLY = 39;
        public const int BSEMF_MANDATE_BROKER_REPLY = 187;
        public const int BSEMF_MANDATE_EXCH_REPLY = 188;
        
        public const int OFS_WATCHLIST = 201;

        public static int[] ARRAY_REP_ITS_TO_FE_CLIENT_SEND = new int[] {
            REP_ITS_TO_FE_LOGIN,
            REP_ITS_TO_FE_CHANGE_PASS,
            REP_ITS_TO_FE_LOGIN_NEW,
            REP_ITS_TO_FE_ADD_TRADER,
            REP_ITS_TO_FE_VARIFY_TRADER,
            207,
            141,
            142,
            250
        };

        public static int[] ARRAY_REP_ITS_TO_FE_MULTIPLE_SEND = new int[] {
            REP_ITS_TO_FE_PLACE_ORD_EQ_CONFIRM_BROKER,
            REP_ITS_TO_FE_PLACE_COVER_ORD_CONFIRM_BROKER,// sumitp 28_jan_22
            REP_ITS_TO_FE_PLACE_ORD_EQ_CONFIRM_EXCH,
            REP_ITS_TO_FE_REJECT_AH_ORD_CONFIRM_BROKER,
            REP_ITS_TO_FE_MODIFY_ORD_EQ_CONFIRM_BROKER,
            REP_ITS_TO_FE_MODIFY_ORD_EQ_CONFIRM_EXCH,
            REP_ITS_TO_FE_CANCEL_ORD_EQ_CONFIRM_BROKER,
            REP_ITS_TO_FE_CANCEL_ORD_EQ_CONFIRM_EXCH,
            REP_ITS_TO_FE_KILLED_ORD_EQ_CONFIRM_EXCH,
            REP_ITS_TO_FE_KILLED_IOC_EQ_CONFIRM_EXCH,
            REP_ITS_TO_FE_TRD_EQ_CONFIRM_EXCH,
            REP_ITS_TO_FE_SL_EQ_TRIGGER_EXCH,
            REP_ITS_TO_FE_PLACE_ORD_FO_CONFIRM_BROKER,
            REP_ITS_TO_FE_PLACE_ORD_FO_CONFIRM_EXCH,
            REP_ITS_TO_FE_MODIFY_ORD_FO_CONFIRM_BROKER,
            REP_ITS_TO_FE_MODIFY_ORD_FO_CONFIRM_EXCH,
            REP_ITS_TO_FE_CANCEL_ORD_FO_CONFIRM_BROKER,
            REP_ITS_TO_FE_CANCEL_ORD_FO_CONFIRM_EXCH,
            REP_ITS_TO_FE_KILLED_IOC_FO_CONFIRM_EXCH,
            REP_ITS_TO_FE_TRD_FO_CONFIRM_EXCH,
            REP_ITS_TO_FE_SL_FO_TRIGGER_EXCH,
            REP_ITS_TO_FE_PLACE_ORD_CURR_FO_CONFIRM_BROKER,
            REP_ITS_TO_FE_PLACE_ORD_CURR_FO_CONFIRM_EXCH ,
            REP_ITS_TO_FE_MODIFY_ORD_CURR_FO_CONFIRM_BROKER,
            REP_ITS_TO_FE_MODIFY_ORD_CURR_FO_CONFIRM_EXCH ,
            REP_ITS_TO_FE_CANCEL_ORD_CURR_FO_CONFIRM_BROKER ,
            REP_ITS_TO_FE_CANCEL_ORD_CURR_FO_CONFIRM_EXCH ,
            REP_ITS_TO_FE_KILLED_IOC_CURR_FO_CONFIRM_EXCH,
            REP_ITS_TO_FE_MODIFY_Order_CONFIRM_EXCH,
            REP_ITS_TO_FE_CANCEL_Order_CONFIRM_EXCH,

            REP_ITS_TO_FE_PLACE_ORD_CONFIRM_BROKER,
            REP_ITS_TO_FE_MODIFY_ORD_CONFIRM_BROKER,
            REP_ITS_TO_FE_CANCEL_ORD_CONFIRM_BROKER,
            REP_ITS_TO_FE_Order_CONFIRM_EXCH,
            REP_ITS_TO_FE_Order_CONVERSION_EXCH,
            REP_ITS_TO_FE_Order_CONVERSION_EXCH_WITH_EXPIRY,
            REP_OTS_IIS,

            //By Sandeep For Spread
            REP_ITS_TO_FE_PLACE_ORD_SPREAD_CONFIRM_EXCH,//241
            REP_ITS_TO_FE_MODIFY_ORD_SPREAD_CONFIRM_EXCH,//242
            REP_ITS_TO_FE_CANCEL_ORD_SPREAD_CONFIRM_EXCH,//243
            REP_ITS_TO_FE_KILLED_IOC_SPREAD_CONFIRM_EXCH,//249
            REP_ITS_TO_FE_PLACE_ORD_SPREAD_CONFIRM_BROKER,//191
            REP_ITS_TO_FE_MODIFY_ORD_SPREAD_CONFIRM_BROKER, //192
            REP_ITS_TO_FE_CANCEL_ORD_SPREAD_CONFIRM_BROKER,//193

            //03.08.07
            REP_ITS_TO_FE_EXERCISE_PLACE_REQ ,
            REP_OTS_TO_ITS_EXERCISE_PLACE_REPLY ,
            REP_ITS_TO_FE_EXERCISE_MODIFY_REQ,
            REP_OTS_TO_ITS_EXERCISE_MODIFY_REPLY,
            REP_ITS_TO_FE_EXERCISE_CANCEL_REQ,
            REP_OTS_TO_ITS_EXERCISE_CANCEL_REPLY,

            //BseMf Krutika
            BSEMF_PLACE_BROKER_REPLY,
            BSEMF_MODIFY_BROKER_REPLY,
            BSEMF_CANCEL_BROKER_REPLY,
            BSEMF_PLACE_EXCHANGE_REPLY,
            BSEMF_MODIFY_EXCHANGE_REPLY,
            BSEMF_CANCEL_EXCHANGE_REPLY,
            BSEMF_SIPREGISTRATION_BROKER_REPLY,
            BSEMF_XSIPREGISTRATION_BROKER_REPLY,
            BSEMF_SIPCANCELLATION_BROKER_REPLY,
            BSEMF_SIPREGISTRATION_EXCHANGE_REPLY,
            BSEMF_SIPCANCELLATION_EXCHANGE_REPLY,
            BSEMF_MANDATE_BROKER_REPLY,
            BSEMF_MANDATE_EXCH_REPLY,
            122,123,124
        };

        public static int TIARequestHeaderRecordSize = Marshal.SizeOf(new TIARequestHeaderRecord());
        public static int TIAReplyHeaderRecordSize = Marshal.SizeOf(new TIAReplyHeaderRecord());
        public static int TIAErrorMessageRecordSize = Marshal.SizeOf(new TIAErrorMessageRecord());
        public static ushort TMessageStringRecordSize = (ushort)Marshal.SizeOf(new TMessageStringRecord());
        public static ushort TDummyRecordSize = (ushort)Marshal.SizeOf(new TDummyRecord());
        public static ushort TLoginReplyRecordSize = (ushort)Marshal.SizeOf(new TLoginReplyRecord());
        public static ushort TLogoutRequestRecordSize = (ushort)Marshal.SizeOf(new TLogoutRequestRecord());
        public static ushort TLogoutRequestRecordSize_2FA = (ushort)Marshal.SizeOf(new TLogoutRequestRecord_2FA());
        public static ushort TLinkFailedRecordSize = (ushort)Marshal.SizeOf(new TLinkFailedRecord());
        public static ushort TTradeToTradeReplyRecordSize = (ushort)Marshal.SizeOf(new TTradeToTradeReplyRecord());
        public static ushort TBseNseMapRecordRecordSize = (ushort)Marshal.SizeOf(new TBseNseMapRecord());
        public static ushort TDerivBannedScripReplyRecordSize = (ushort)Marshal.SizeOf(new TDerivBannedScripReplyRecord());
        public static ushort TIlliquidReplyRecordSize = (ushort)Marshal.SizeOf(new TIlliquidReplyRecord());
        public static ushort TISegmentReplyRecordSize = (ushort)Marshal.SizeOf(new TAllowSegmentReply());

    }

    #region Structs
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TGlobalHeader
    {
        public DateTime DateTime;
        public int Size;
        public byte SentFrom;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TIARequestHeaderRecord
    {
        public ushort SocketNum;
        public byte TraderLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public char[] TraderID;
        public byte AccountType;
        public byte FELength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] FEUserID;
        public byte ClientType;
        public byte RequestType;
        public ushort RequestLen;
        public Int64 ChannelID_NC;
        public Int64 ChannelID_ND;
        public Int64 ChannelID_BC;

        public string toJsonString()
        {
            string data = "\r\n\r\n------------REQ HEADER ------------\r\n"
                + $"TraderID : {new string(TraderID)}{Environment.NewLine}"
                + $"TraderLength : {TraderLength}{Environment.NewLine}"
                + $"FEUserID : {new string(FEUserID)}{Environment.NewLine}"
                + $"FELength : {FELength}{Environment.NewLine}"
                + $"RequestType : {RequestType}{Environment.NewLine}"
                + $"Request Len : {RequestLen}{Environment.NewLine}"
                + $"Time : {DateTime.Now.ToString("dd-MM-yy HH:mm:ss.fff")}{Environment.NewLine}";
            return data;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TIAReplyHeaderRecord
    {
        public ushort SocketNum;
        public byte TraderLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public char[] TraderID;
        public byte AccountType;
        public byte FELength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] FEUserID;
        public byte ClientType;
        public byte ReplyType;
        public int ReplyLen;
        public ushort ReplyRecSize;
        public ushort ReplyCnt;
        public ushort ErrorCode;
        public Int64 ChannelID_NC;
        public Int64 ChannelID_ND;
        public Int64 ChannelID_BC;

        public string toJsonString()
        {
            string data = "\r\n\r\n------------HEADER-REPLY-------\r\n"
                + $"TraderID : {new string(TraderID)}{Environment.NewLine}"
                + $"TraderLength : {TraderLength}{Environment.NewLine}"
                + $"FEUserID : {new string(FEUserID)}{Environment.NewLine}"
                + $"FELength : {FELength}{Environment.NewLine}"
                + $"Reply Len : {ReplyLen}{Environment.NewLine}"
                + $"Reply Type : {ReplyType}{Environment.NewLine}"
                + $"ReplyRecSize : {ReplyRecSize}{Environment.NewLine}"
                + $"ReplyCnt : {ReplyCnt}{Environment.NewLine}"
                + $"Error Code : {ErrorCode}{Environment.NewLine}"
                + $"Time : {DateTime.Now.ToString("dd-MM-yy HH:mm:ss.fff")}{Environment.NewLine}";
            return data;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TIAErrorMessageRecord
    {
        public byte Errorlength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public char[] ErrorStr;

        public string toJsonString()
        {
            string data = "\r\n\r\n------------ERROR------------\r\n"
                + $"Message : {new string(ErrorStr)}{Environment.NewLine}"
                + $"Errorlength : {Errorlength}{Environment.NewLine}"
                + $"Time : {DateTime.Now.ToString("dd-MM-yy HH:mm:ss.fff")}{Environment.NewLine}";
            return data;
        }
    }


    #region AllReply OTS->ITS

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TAllReplyNewRecord
    {
        public TOrderPlaceBrokerReplyRecord_Cover OrderPlaceBrokerReplyRecord_Cover; // sumitp 28_jan_22
        public IntellectSharedClassLibrary.ISCL_HoldingEquityReply ISCL_HoldingEquityReply; //194
        //public ISCL_HoldingEquityReply ISCL_HoldingEquityReply; //194
        public TLoginReplyRecord LoginReply; //1
        public TLoginReplyRecordNew LoginRepyNew; //3
        public TLoginTwoFactorAuthRecord LoginTwoFactorAuthRecord; //85
        public TChangePasswordReplyRecord ChgPswdReplyRec; //2
        public TLogoutUserRecord LogoutUserRec; //9    for disconnecting multiple connections
        public TTraderListReplyRecord TraderListReplyRec;  //5  For admin ,6
        public TEquityOrderPlaceBrokerReplyRecord EquityOrderPlaceBrokerReplyRec; //10
        public TEquityOrderModifyBrokerReplyRecord EquityOrderModifyBrokerReplyRec;  //20
        public TEquityOrderCancelBrokerReplyRecord EquityOrderCancelBrokerReplyRec;  //30
        public TEquityOrderExchReplyRecord EquityOrderExchReplyRec;         //11,21,31,33
        public TAHOrderRejectedRecord AHOrderRejectedRecord;  //16  AHOrderRejectedRec
        public TKillOrderExchReplyRecord KillOrderExchReplyRec;  //32  nse no reply,bse reply
        public TExchTradeConfirmationRecord ExchTradeConfirmationRec;  //50
        public TSLTriggerRecord SLTriggerRec;     //51    
        //public TMarketToLimitRecord MarketToLimitRecBSE; //52 BSE
        public TMarketToLimitRecord MarketToLimitRecNSE; //53 NSE - No Reply
        public TExchangeStatusRecord ExchangeStatusRec;  //54
        public TMCXGroupLevelHaltInfo_OTS McxGrpHalt;//McxTrading 56
        public TScripMarginReplyRecord ScripMarginReplyRec;   //41
        //for Report Requests Reply
        public TScripInfoReplyRecord ScripInfoReplyRec;   //42
        public TScripMarginReplyRecord_New NewScripMarginReplyRec;   //49
        public TTradeReportReplyRecord TradeReportReplyRec; //61
        public TOrderReportReplyRecord OrderReportReplyRec;  //62
        public TOrderReportReplyRecordCoverOrBracket OrderReportReplyRecCoverOrBracket; //78    // Himanshu // sumitp 28_jan_22
        public TTradeReportReplyRecNewCover TradeReportReplyRecNewCover; //58    // sumitp 28_jan_22
        public TExerciseReportReplyRecord ExerciseReportReplyRec; //63
        public TActivityLogReplyRecord ActivityLogReplyRec;
        public TAHOrderReportReplyRecord AHOrderReportReplyRec;
        public TLicenceInfoReplyRecord LicenceInfoReplyRec;
        public TClientDbNumReportReplyRecord ClientDbNumReplyRec; //68
        public TMoneyTransferReplyRecord MoneyTransferReplyRec;
        public THoldingsReportReplyRecord HoldingsReportReplyRec;
        public THoldingsValuationReportRecord HoldingsValuationReportReplyRec; //70
        public THoldingsValuationReportWithMemVar HoldingsValuationReportWithMemVarReplyRec; //170
        public TTradeReportReplyRecordNew TradeReportReplyRecNew;//71
        public TOrderTypeReportsReplyRec OrderReportReplyRecNew;//72

        public TTradeReportReplyReplyRecNew TradeReportReplyRecNewSpread;   //New 61 for Spread - Added on 18-Apr-2018 - Rohan
        public TOrderTypeTradeReportReplyRecNewCover OrderTypeTradeReportReplyRecNewCover;   //New 58 Cover // sumitp 28_jan_22
        public TOrderTypeReportsReplyRecNew OrderReportReplyRecNewSpread;  //New 62 for Spread - Added on 18-Apr-2018 - Rohan
        public TActivityLogReplyRecordNewForRequestorNew ActivityLogReplyRecForRequestorNewSpread;    //New 64 for Spread - Added on 18-Apr-2018 - Rohan

        public TOrderTypeReportsReplyRecNewWithRejetOrder OrderReportReplyRecNewSpreadWithRejOrder;

        public THoldingsReportReplyRecord_WithCollatral HoldingsReportWithCollateralReplyRec; //74
        public THoldingsReportReplyRecordNew AllClientsHoldingsReportReplyRec;//75
        public THoldingsEquityReportReplyRecordNew AllClientsHoldingsEquityReportReplyRec;//76 Added on 08-Nov-2016 - Rohan
        public THoldingsReportReplyRecord_WithCollatralNew HoldingsReportWithCollatralReplyRecNew;//94 McxTrading
        public THoldingsReportPreCloseReplyRecord_WithCollatralNew HoldingsReportWithCollatralPreCloseReplyRecNew;//106 jinal 
        public THoldingsReportPreCloseReplyRecord_MTF HoldingsReportWithMTF;//106 jinal 
        public THoldingsReportPreCloseReplyRecord_MTFNew HoldingsReportWithMTFNew;//106 jinal 
        public TAllClientsHoldingsReportReplyRecord AllClientsHoldingsReportReplyRecNew;//95
        public TAllClientsHoldingsReportReplyRecordWithPreClose AllClientsHoldingsReportReplyRecWithPreClose;//105
        public TDBDateReplyRecord DBDateReplyRec;
        public TClientPositionReportReplyRecord ClientPositionReplyRec;
        public TTradeToTradeReplyRecord TradeToTradeReplyRec;
        public TBseNseMapRecord BseNseMapReplyRec;
        public TDerivBannedScripReplyRecord DerivBannedScripReplyRec;
        public TAdhocReportReplyRecord AdhocReportReplyRec;
        public TAdhocModifyReplyRecord AdhocModifyReplyRec;
        public TScripCategoryReportReplyRecord ScripCategoryReportReplyRec;
        public TMaalFodoClientsReportReplyRecord MFClientsReportReplyRec;
        public TMaalFodoClientHoldingReportReplyRecord MFClientHoldingReportReplyRec;
        public TDummyRecord1 MFClientAllowToTradeReplyRec;
        public TIlliquidReplyRecord IlliquidReplyRec;
        public TPreOpenScripReplyRecord PreopenScripReplyRec;
        public TDerivOrderPlaceBrokerReplyRecord DerivOrderPlaceBrokerReplyRec;
        public TDerivOrderModifyBrokerReplyRecord DerivOrderModifyBrokerReplyRec;
        public TDerivOrderCancelBrokerReplyRecord DerivOrderCancelBrokerReplyRec;
        public TDerivOrderExchReplyRecord DerivOrderExchReplyRec; //111,121,131,133
        public TDerivExchTradeConfirmationRecord DerivExchTradeConfirmationRec;//150
        public TDerivSLTriggerRecord DerivSLTriggerRec;
        public TMessageStringRecord MsgReplyRec;
        public TMarginReportReplyRecord MarginReportReplyRec;
        public TMarginReportReplyRecord MarginReportReplyRecMCX;
        public TMarginReportReplyRecordCombined MarginReportReplyRecCombined;//208
        public TClearSentToExchRecord ClearSentToExchRec;
        //OPTION EXERCISE
        public TOptionExerciseBrokerReplyRecord OptionExerciseBrokerReplyRec;
        public TOptionExerciseExchReplyRecord OptionExerciseExchReplyRec;
        public TLinkFailedRecord LinkFailRec;
        public TDummyRecord1 HandShakeRec;   //10/1/2007
        public TOrderReportForSurviellenceReplyRecord OrderReportForSurviellenceReplyRec; //59 //21.10.13

        public TSpreadOrderExchReplyRecord SpdOrderExchReplyRec;
        public TSpreadOrderPlaceBrokerReplyRecord SpdOrderPlaceBrokerReplyRec;
        public TSpreadOrderCancelBrokerReplyRecord SpdOrderCancelBrokerReplyRec;

        #region MTM - Added on 12-Sep-2016 - Rohan
        public TAllClientMarginReportReplyRecord AllClientMarginReply;
        public ClientsForMTMSquareOff ClientsForMTMReply;
        public UpdateClientsReply ClientsMTMReply;
        public UpdateClientsReply ClientsMTMReplyMcx;
        public MTMClientsWithAllMargins MTMClientsWithAllMarginsReply;
        public MTMClientsWithAllMargins MTMClientsWithAllMarginsReplyMcx;
        public MTMClientsWithAllMarginsInstrumentWise MTMClientsWithAllMarginsReplyInstrumentWise;
        #endregion

        #region Place/Modify/Cancel BseMF Krutika
        public TBseMFAddModifyCancelBrokerResponse MFAddModifyCancelBrokerResRec;//BseMf
        public TBseMFAddModifyCancelExchangeOrderReply MFAddModifyCancelExchangeResRec;//BseMf
        public TBseMFOrderTypeReportsReplyRec BseMfOrderReportReplyRecNew;
        public TBseMFActivitylogReplyRec BseMfActivitylogReplyRecNew;
        public THoldingsReportReplyRecord_MF HoldingsReportOfMFReplyRec;
        public TBseMFSIPRegistrationMsgBrokerReply MFSIPRegistrationBrokerResRec;
        public TBseMFSIPRegistrationExchangeReplyRecord MFSIPRegistrationExchangeResRec;
        public XSIPMandateExchangeReply XSIPMandateExchangeReplyRec;
        public TBseMFSIPOrderTypeReportsReplyRec BseMfSIPOrderReportReplyRecNew;
        public MandateOrderTypeReportsReplyRec MandateOrderTypeReportsReplyRecNew;
        public XSIPMandateBrokerReply XSIPMandateBrokerReply;
        #endregion

        #region Intraday
        //public TOrderPlaceBrokerReplyRecordCoverAndBracket OrderPlaceBrokerReplyRecordCoverAndBracket;  //171
        public TEquityOrderPlaceBrokerReplyRecord_cover EquityOrderPlaceBrokerReplyRecord_cover;  //171 // sumitp 28_jan_22
        public TOrderPlaceRequestRecordCoverAndBracket OrderPlaceRequestRecordCoverAndBracket; // sumitp 28_jan_22

        public TActivityLogReplyRecordNew ActivityLogReplyRecNew;
        public TActivityLogReplyRecordNewForRequestor ActivityLogReplyRecNewForRequestor; //15.10.16 Hetal
        public TOrderPlaceBrokerReplyRecord OrderPlaceBrokerReplyRecord;
        public TOrderModifyBrokerReplyRecord OrderModifyBrokerReplyRecord;
        public TOrderCancelBrokerReplyRecord OrderCancelBrokerReplyRecord;
        public TOrderConversionRecReply OrderConversion;
        public TOrderExchReplyRecord OrderExchReplyRecord;
        #endregion

        #region Nse Slbm
        public TSLBMOrderPlaceBrokerReplyRecord SLBMOrderPlaceBrokerReplyRec;   //235
        public TSLBMOrderExchReplyRecord SLBMOrderExchReplyRec;                 //238/239/240/243
        public TSLBMOrderModifyBrokerReplyRecord SLBMOrderModifyBrokerReplyRec; //236
        public TSLBMOrderCancelBrokerReplyRecord SLBMOrderCancelBrokerReplyRec; //237
        public TSLBMHoldingsReportReplyRecord SLBMHoldingsReportReplyRec; //241
        #endregion


        #region Disclosed Qty
        public TCurrDerivOrderExchReplyRecord CurrDerivOrderExchReplyRec;
        public TCurrDerivOrderPlaceBrokerReplyRecord CurrDerivOrderPlaceBrokerReplyRec;
        public TCurrDerivOrderModifyBrokerReplyRecord CurrDerivOrderModifyBrokerReplyRec;
        public TCurrDerivOrderCancelBrokerReplyRecord CurrDerivOrderCancelBrokerReplyRec;
        #endregion

        public TLotQuantityRecord LotQtyOtsReplyRec;
        public TOrderTradeGeneral IISOrderTradeGeneral;
        public TTemplateMarginReplyRecord TemplateMarginReport;
        public TTemplateMarginReplyRecord_Peak TemplateMarginReport_Peak;
        public TMarginReportReplyRecordTemplateWithDerivMargin TemplateMarginReply; //210 20.10.22
        public PeakMarginReplyData PeakMarginReplyData;
        public TAllowSegmentReply SegmentReply;
        public TradeExchReportReply TradeReportReply;
        public TOrderTypeReportsReplyRecDealer OrdersReportDealerReply;
        public TTradeReportReplyRecDealer TradesReportDealerReply;
        public OFSWatchListReply OFSWatchListReply;
        //public TOrderPlaceRequestRecordOFS PlaceRequestRecordOFSReply;
        public TOrderPlaceModifyCancelReplyRecordOFS PlaceOFSReplyRec;
        public TOrderTypeReportsReplyRecOFS OFSOrderReportReply;
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
    public struct TOrderPlaceRequestRecordOFS
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

        //Revised on 19-11-2013

        //public byte AllowNseCurrency;
        //public long ChannelID_CD;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        //public byte[] Junk;           

        //------------------- upto here on 19-11-2013

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
        public byte AllowNseCurr;
        public byte AllowMcxDeriv;//Mcx Changes
        public byte Reserved1;
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
        public long Reserved3;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]//03/08/2012
        public char[] Junk2;
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
    public struct TLoginTwoFactorAuthRecord                                             //Request & Reply =  85 - LOGIN 
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
        public byte AllowNseCurr;
        public byte AllowMcxDeriv;//Mcx Changes
        public byte Reserved1;
        public byte AllowNseSLBM;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Reserved2;

        public ushort BranchID;
        public long ChannelID_NC;
        public long ChannelID_ND;
        public long ChannelID_BC;

        public long ChannelID_CD;
        public long ChannelID_MD;//MCX Changes
        public long Reserved3;
        public long ChannelID_SLBM;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public long[] Reserved4;

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
        public byte AllowMarginTrading; //11.09.17 Sanchit
        public byte AllowBseMf;//BseMf Krutika
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] Reserved5;
        public int Journey;


        public string toJsonString()
        {
            string data = "\r\n\r\n------------REPLY------------\r\n"
                + $"User ID : {new string(FEUserID)}{Environment.NewLine}"
                + $"FEUserIDLength : {FEUserIDLength}{Environment.NewLine}"
                + $"LastModifiedTime : {LastModifiedTime}{Environment.NewLine}"
                + $"Journey : {Journey}{Environment.NewLine}"
                + $"PopupMsg : {PopupMsg}{Environment.NewLine}"
                + $"PopupMsgLength : {PopupMsgLength}{Environment.NewLine}"
                + $"BadLoginCnt : {BadLoginCnt}{Environment.NewLine}"
                + $"FEUserType : {FEUserType}{Environment.NewLine}"
                + $"Time : {DateTime.Now.ToString("dd-MM-yy HH:mm:ss.fff")}{Environment.NewLine}";
            return data;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //15.10.16 Hetal
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //15.10.16 Hetal
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TChangePasswordReplyRecord
    {
        public byte FEUserIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] FEUserID;
        public byte Status;
        public byte StatusStrLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] StatusStr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TLogoutUserRecord
    {
        public byte FELength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public char[] FEUserID;
        public byte ITS_Num;
        public byte IPLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] IP;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TTraderListReplyRecord
    {
        public byte TDRIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public char[] TraderID;
        public byte AccountType;  // new on 19/02/2008
        public byte TdrNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 29)]
        public char[] TraderName;  //30 to 29 on 01/02/2008
        public byte NilOrders; //boolean
        public byte TraderType;
        public ushort BranchID;
        public byte AllowNseCash; //boolean
        public byte AllowNseDeriv; //boolean
        public byte AllowBseCash; //boolean
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TEquityOrderPlaceBrokerReplyRecord
    {
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public byte ScripCodeLength;
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
        public int AlgoID;        //Added by Rhythm 01-01-24
        public int Reserved1;
        public int Reserved2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     // sumitp 28_jan_22
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
        public int AlgoID;        //Added by Rhythm 01-01-24
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
        public int AlgoID;        //Added by Rhythm 01-01-24
        public int Reserved1;
        public int Reserved2;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] Status;
        public byte IOC;
    }

    //16 on 30/10/2012 for AH Rejected order
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TAHOrderRejectedRecord
    {
        public int BrokerOrderID;
        public byte ErrorMsgLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string ErrorMsg;
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
    public struct TExchTradeConfirmationRecord
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


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]//McxTrading 18.01.16
    public struct TMCXGroupLevelHaltInfo_OTS
    {
        public short GroupID;
        public short Status;
        public long TriggerTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TMarketToLimitRecord
    {
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public byte ScripNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] ScripName;
        public Single Rate;
        public long ExchangeOrderID;
        public int ExchangeTradeTime;
        public int ExchReplyCode;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TTradeReportReplyRecord
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
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TOrderReportReplyRecord
    {
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public byte ScripNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ScripName;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] Status; //'Sent To Exch','Executed','Cancelled
        public char AHStatus;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]                 // addded by Himanshu  27-Nov-2021 // sumitp 28_jan_22
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
    

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] // sumitp 28_jan_22
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

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TOrderReportForSurviellenceReplyRecord //21.10.13
    {
        public byte TraderIdLengthLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] TraderID;
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public byte ScripNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ScripName;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] Status; //'Sent To Exch','Executed','Cancelled
        public char AHStatus;
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
    public struct TActivityLogReplyRecord
    {
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public byte ScripNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ScripName;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
        public char[] Status; //'Sent To Exch','Executed','Cancelled
        public char ReqType; //changed from 20 to 19 on 9/2/5 and added ReqType
        public int OldOrderQty;
        public int TradedQty;
        public int PendingQty;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TAHOrderReportReplyRecord
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public char[] TraderID;
        public byte AccountType;
        public byte OrderRequesterIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] OrderRequesterID;
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public byte ScripNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ScripName;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TLicenceInfoReplyRecord
    {
        public byte UserType;
        public int UserCnt;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TClientDbNumReportReplyRecord
    {
        public byte ClientCodeLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ClientCode;
        public byte DBNum;
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
    public struct THoldingsReportReplyRecord
    {
        public int BseCode;
        public int NseCode;
        public byte ScripMameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ScripName;
        public int TotalQty;
        public int SoldQty;
        public int BrokerBeniQty;
        public int ExchRcvQty;
        public int ClientDPQty;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDBDateReplyRecord
    {
        public int DBDate;
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
    public struct TDerivBannedScripReplyRecord
    {
        public int ULToken;
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
    public struct TAdhocModifyReplyRecord
    {
        public byte ClientCodeLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ClientCode;
        public char Status;
        public byte MsgLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] Msg;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TScripCategoryReportReplyRecord
    {
        public char Category;
        public Single BuyMgn;
        public Single SellMgn;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ClientName;
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
    public struct TDummyRecord1
    {
        public int DummyInt;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TIlliquidReplyRecord
    {
        public char Exch;
        public int ScripCode;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TPreOpenScripReplyRecord
    {
        public char Exch;
        public int ScCode;
    }

    //Spread Start 
    #region Spread Structures Start By Sandeep Jadhav

    //Spread Place Order Structures By sandeep    
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
        public int ExpiryDate; //Seconds from 01/01/80
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
        public byte OrderTypes;

        public TLegInfoRecord SpdLeg1;
        public TLegInfoRecord SpdLeg2;
        public TLegInfoRecord SpdLeg3;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TSpreadOrderPlaceBrokerReplyRecord
    {
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv And Spread
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
        public byte IntraDelivery;//0 – Intraday & 1 – Delivery.
        public byte OrderTypes;//Reserved Field.
        public int Reserved1;
        public int Reserved2;


        public TLegInfoRecord SPREAD_LEG_1;
        public TLegInfoRecord SPREAD_LEG_2;
        public TLegInfoRecord SPREAD_LEG_3;
    }

    //Spread Cancel and Modify Order Structures By sandeep
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
        public byte OrderTypes;

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

        public byte IntraDelivery;//0 – Intraday & 1 – Delivery.
        public byte OrderTypes;//Reserved Field.
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

        public byte IntraDelivery; //0 – Intraday & 1 – Delivery.
        public byte OrderTypes;//Reserved Field.
        public int Reserved1;
        public int Reserved2;

        public TLegInfoExchReplyRec SPREAD_LEG_1;
        public TLegInfoExchReplyRec SPREAD_LEG_2;
        public TLegInfoExchReplyRec SPREAD_LEG_3;
    }

    #endregion
    //Spread Structures End

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
        public int AlgoID;        //Added by Rhythm 15-03-24
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
        public int AlgoID;        //Added by Rhythm 15-03-24
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
        public int AlgoID;        //Added by Rhythm 15-03-24
    }

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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] Status;
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
    public struct TMessageStringRecord
    {
        public byte MsgStrLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
        public char[] MsgStr;
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
    public struct TTemplateMarginReplyRecord
    {
        public double OpeningMargin;
        public double Adhoc;
        public double Payments;
        public double HoldingCFS;
        public double OptionCFS;
        public double MBPO;
        public double MarginForOpenPos;
        public double AvailableMargin;
        //public double PeakMargin;
        public double Junk1;
        public double Junk2;
        public int Junk3;
        public float Junk4;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //Jigar for template margin
    public struct TTemplateMarginReplyRecord_Peak
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TClearSentToExchRecord
    {
        public char Exch;
        public char ExchType;
    }

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
    public struct TLinkFailedRecord
    {
        public byte RequestType;
        public ushort TraderRequestID;
        public int BrokerOrderID;
        public ushort ErrorLevel;
        public byte ErrorMsgLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public char[] ErrorMsg;
    }

    #endregion

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
    public struct TUpdateClientsForMTM
    {
        public byte DummyLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] Dummy;
        public int DummyInt;
        public char MTMLimitHit;
        public char SkipFromMTM;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TTraderRecord
    {
        public byte TraderIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public char[] TraderID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TDerivInfoRecord
    {
        public byte InstrType;
        public byte SymbolLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public char[] Symbol;
        public int ExpiryDate; //Seconds from 01/01/80
        public Single StrikePrice;
        public byte CPType;
        public byte CALevel;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct THoldingsReportReplyRecordNew
    {
        public byte TraderIDLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string TraderID;
        public int BseCode;
        public int NseCode;
        public byte ScripMameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ScripName;
        public int TotalQty;
        public int SoldQty;
        public int BrokerBeniQty;
        public int ExchRcvQty;
        public int ClientDPQty;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //Added on 08-Nov-2016 - Rohan
    public struct THoldingsEquityReportReplyRecordNew
    {
        public byte TraderIDLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string TraderID;
        public int BseCode;
        public int NseCode;
        public byte ScripMameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ScripName;
        public int TotalQty;
        public int SoldQty;
        public int BrokerBeniQty;
        public int ExchRcvQty;
        public int ClientDPQty;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct THoldingsReportReplyRecord_WithCollatralNew//McxTrading
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
        public int Collatral;
        public double PrevClose;
        public int PendingQty;
        public double PurchasePrice;
    }   

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//106 jinal 
    public struct THoldingsReportPreCloseReplyRecord_MTF
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
        
        public double PurchasePrice;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//106 jinal 
    public struct THoldingsReportPreCloseReplyRecord_MTFNew
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
        public double PurchasePrice;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ISCL_HoldingEquityReply
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
        public int PendingQty;
        public int BrokerBeniQty;
        public int ExchRcvQty;
        public int ExchRcvQty2;
        public int ClientDPQty;
        public int Collatral;
        public int FundedStockQty;
        public int FSCollateralQty;
        public int NonPOA;
        public double PrevClose;
        public double PurchasePrice;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] Reserves;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//
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


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     //14
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

        public int Reserved1;
        public byte AllowBannedScrip;
        public byte Reserved2;
        public short Reserved3;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //Cover Order Structure     // sumitp 28_jan_22
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
        public int EpochTime; //  EpochTime for order identification 	

        //public Single SLRateBUY;//Rate For BO
        //public Single SLRateSELL;//Rate For BO

        public int Reserved1;
        public byte Reserved2;
        public byte AllowBannedScrip;
        public short Reserved3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     //24
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

        public int Reserved1;
        public int Reserved2;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]     //34
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

        public int Reserved1;
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

        public int Reserved1;
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


    //[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]   //171
    //public struct TOrderPlaceBrokerReplyRecordCoverAndBracket
    //{
    //    public char Exch;
    //    public char ExchType;
    //    public int ScripCode;
    //    public byte ScripNameLength;
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    //    public char[] ScripName;
    //    public byte InstrType;
    //    public byte CPType;
    //    public byte CALevel;
    //    public int ExpiryDate;
    //    public Single StrikePrice;
    //    public char BuySell1;
    //    public char BuySell2;
    //    public int Qty;
    //    public char AtMarket;
    //    public Single Rate;
    //    public char WithSL;
    //    public Single TriggerRate;
    //    public ushort TraderRequestID;
    //    public int BrokerOrderID1;
    //    public int BrokerOrderID2;
    //    public int BrokerTime1;
    //    public int BrokerTime2;
    //    public Single MarginBlocked;
    //    public byte IOC;
    //    public int DisclosedQty;
    //    public char AHStatus;
    //    public byte OrderType;
    //    public byte ProductType;
    //    public int Reserved1;
    //    public int Reserved2;

    //    public byte OrderTypeNew; // Co/Bo type new
    //    public Single SLRateCO;//Rate for CO
    //    public long EpochTime; //  EpochTime for order identification COVER
    //}

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

        public int Reserved1;
        public int Reserved2;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TActivityLogReplyRecordNew
    {
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public byte ScripNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] ScripName;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
        public char[] Status; //'Sent To Exch','Executed','Cancelled
        public char ReqType; //changed from 20 to 19 on 9/2/5 and added ReqType
        public int OldOrderQty;
        public int TradedQty;
        public int PendingQty;

        public byte OrderType;
        public byte ProductType;
        public int Reserved1;
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

        public int Reserved1;
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
    public struct TTradeReportReplyRecordNew
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
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //Added on 18-Apr-2018 - Rohan
    public struct TTradeReportReplyReplyRecNew
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

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)] //Added on 18-Apr-2018 - Rohan
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TBseMfCurrentOrderRecord
    {
        public byte TraderIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] TraderID;
        public char Exch;
        public char ExchType;
        public int ExchPos;  //For ExchData
        public int ScripPos; //For ExchData
        public int BrokerOrderID;
        public int BrokerOrderTime;
        public byte OrderNoLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] OrderNo;
        public byte UniqueNoLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public char[] UniqueNo;
        public byte SchemeCodeLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] SchemeCode;
        public byte ISINLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public char[] ISIN;
        public byte SchemeNameLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public char[] SchemeName;
        public char BuySell;
        public char DptxnType;
        public char AllUnitsFlag;
        public Single Amount;
        public long Qty;
        public byte BuySellTypeLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public char[] BuySellType;
        public byte FolioNoLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] FolioNo;
        public long ExchangeOrderId;
        public int LastExchangeTime;
        public string Status;
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
        // public int NseCode;
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
    #endregion
    #region Mandate
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
    public struct XSIPMandate
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
        public ushort TraderRequestID;

    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
    public struct XSIPMandateBrokerReply
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
    #endregion
   // [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//BseMf
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
    //    public double Brokerage;
    //    public int Mandate;
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

    #region AllRequest ITS -> OTS

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TAllRequestNewRecord
    {
        public TLoginRequestRecord LoginRequest;
        public TLoginRequestRecordTwoFactor LoginRequestTwoFactor;
        public TChangePasswordRequestRecord ChgPswdRequestRec;
        public TTraderListRequestRecord TraderListRequestRec;
        public TVerifyTraderRequestRecord VerifyTraderRequestRec;
        public TScripMarginRequestRecord ScripMarginRequestRec;
        public TScripInfoRequestRecord ScripInfoRequestRec;//42   ---26-03-2015
        public TTradeReportRequestRecord TradeReportRequestRec;
        public TOrderReportRequestRecord OrderReportRequestRec;
        public TExerciseReportRequestRecord ExerciseReportRequestRec;
        public TActivityLogRequestRecord ActivityLogRequestRec;            // Sumit 21_jan_22 For Single Activity Log
        public TActivityLogRequestRecord_new ActivityLogRequestRec_new;
        public TAHOrderReportRequestRecord AHOrderReportRequestRec;
        public TDummyRecord1 LicenceInfoRequestRec;
        public TDummyRecord1 CliDBNumRequestRec;
        public TMoneyTransferRequestRecord MoneyTransferRequestRec;
        public TTraderIDRecord HoldingsReportRequestRec;    //75
        public TTraderIDRecord HoldingsEquityReportRequestRec;  //76
        public TTraderIDRecord HoldingsValuationReportRequestRec; //70
        public TTraderIDRecord HoldingsValuationReportWithMemVarRequestRec; //170 //20.05.15
        public TTraderIDRecord HoldingsReportWithCollateralRequestRec;//74
        public TTraderIDRecord HoldingsReportWithCollatralPreCloseReplyRecNew;//106

        public TDummyRecord1 DBDateRequestRec;
        public TClientPositionReportRequestRecord ClientPositionRequestRec;
        public TDummyRecord1 TradeToTradeRequestRec;
        public TDummyRecord1 BseNseMapRequestRec;
        public TDummyRecord1 DerivBannedScripRequestRec;
        public TAdhocReportRequestRecord AdhocReportRequestRec;
        public TAdhocModifyRequestRecord AdhocModifyRequestRec;
        public TScripCategoryReportRequestRecord ScripCategoryReportRequestRec;
        public TMaalFodoClientsReportRequestRecord MFClientsReportRequestRec;
        public TMaalFodoClientHoldingReportRequestRecord MFClientHoldingReportRequestRec;
        public TMaalFodoClientAllowToTradeRequestRecord MFClientAllowToTradeRequestRec;
        public TDummyRecord1 IlliquidRequestRec;
        public TDummyRecord1 PreOpenScripRequestRec;
        public TTraderIDRecord MarginReportRequestRec;
        public TTraderIDRecord MarginReportRequestRecMCX;
        public TTraderIDRecord MarginReportRequestRecCombined;
        public TTraderIDRecord HoldingsReportWithCollateralRequestRecNew;//94 McxTrading
        public TDummyRecord1 ITS2OTSNumRec; //25/01/2008
        public TLinkFailedRecord LinkFailRec;
        public TLogoutRequestRecord LogoutRequest;
        public TLogoutRequestRecord_2FA LogoutRequest_2FA;
        public TDummyRecord1 HandShakeRec; //10/1/2007
        public TEquityOrderPlaceRequestRecord EquityOrderPlaceRequestRec;
        public TAHEquityOrderPlaceRequestRecord AHEquityOrderPlaceRequestRec; //reply as 10
        public TEquityOrderModifyRequestRecord EquityOrderModifyRequestRec;
        public TEquityOrderCancelRequestRecord EquityOrderCancelRequestRec;
        public TDerivOrderPlaceRequestRecord DerivOrderPlaceRequestRec;
        public TAHDerivOrderPlaceRequestRecord AHDerivOrderPlaceRequestRec; //reply as 110
        public TDerivOrderModifyRequestRecord DerivOrderModifyRequestRec;
        public TDerivOrderCancelRequestRecord DerivOrderCancelRequestRec;
        public TOptionExerciseRequestRecord OptionExerciseRequestRec;
        public TCashFutureAlgoOrderPlaceRequestRecord CashFutureAlgoOrderPlaceRequestRec; // 140
        public TDummyRecord MarginReq;
        public TTraderIDRecordForMTM ClientForMTMReq;
        public TTraderIDRecordForMTM ClientForMTMMcxReq;
        public TUpdateClientsForMTM ClientsForMTM;
        public TUpdateClientsForMTM ClientsForMTMMcx;
        public TTraderIDRecordForSurveillance ClientForMTMReqInstrumentWise;
        public TSpreadOrderPlaceRequestRecord AHSpreadOrderPlaceRequestRec;
        public TSpreadOrderCancelRequestRecord SpreadOrderCancelRequestRec;
        #region Added for Disclosed Qty
        public TCurrDerivOrderPlaceRequestRecord CurrDerivOrderPlaceRequestRec;
        public TCurrDerivOrderModifyRequestRecord CurrDerivOrderModifyRequestRec;
        public TCurrDerivOrderCancelRequestRecord CurrDerivOrderCancelRequestRec;
        #endregion

        #region LotQty
        public TTraderIDRecord LotQtyRequestRec;
        #endregion

        #region Nse Slbm
        public TSLBMOrderPlaceRequestRecord SLBMOrderPlaceRequestRec;   //235
        public TSLBMOrderModifyRequestRecord SLBMOrderModifyRequestRec; //236
        public TSLBMOrderCancelRequestRecord SLBMOrderCancelRequestRec; //237
        #endregion

        #region Intra Delivery
        public TActivityLogReplyRecordNew ActivityLogReplyRecNew;
        #endregion

        #region Add/Modify/Cancel BseMF Krutika
        public TBseMFAddModifyCancelRequestRecord MFAddModifyCancelReqRec;//BseMf
        public TBseMfOrderReportRequestRecord BseMfOrderReportRequestRec;
        public TBseMfActivitylogRequestRecord BseMfActivitylogRequestRec;
        public MandateOrderReportRequestRecord MandateOrderReportRequestRec;
        public TTraderIDRecord HoldingsReportOfMFRequestRec;//92
        public TBseMFSIPRegistrationMsgRequestRecord MFSIPRegistrationReqRec;//BseMf
        public XBseMFSIPRegistrationMsgRequestRecord XMFSIPRegistrationReqRec;
        public XSIPMandate XSIPMandateReqrec;
        #endregion 

        public TTemplateMarginRequestRecord TemplateMarginReqRecord;
        public TTemplateMarginRequestRecord_Peak TemplateMarginReqRecord_Peak;
        public TMarginReportReplyRecordTemplateWithDerivMargin TemplateMarginNew; //20.10.22
        public TPeakMarginData PeakMarginData;
        public TDummyRecord GetSegmentMaster;
        public TTradesReportExchFormatRequest GetTradesReport;
        public TOrderPlaceRequestRecordOFS PlaceOfsOrderRequest;
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
    public struct TCashFutureAlgoOrderPlaceRequestRecord
    {
        public TCashFutureHeaderAlgoOrderPlaceRequestRecord TCashFutureHeaderAlgoReq;
        public TDerivAlgoOrderPlaceRequestRecord DerivAlgoOrderPlaceRequestRecord;
        public TCashAlgoOrderPlaceRequestRecord CashAlgoOrderPlaceRequestRecord;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TCashFutureHeaderAlgoOrderPlaceRequestRecord
    {
        public byte BuyCashSellFut; //Flag 
        public int Qty;
        public ushort TraderRequestID;
        public byte IOC;
        public Single CreateDiff;
        public Single ReversalDiff;
        public int PendingLifeTime;
        public byte MktDepthLevel;
        public int TotalQty;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TDerivAlgoOrderPlaceRequestRecord
    {
        public char Exch1;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public TDerivInfoRecord DerivInfo;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TCashAlgoOrderPlaceRequestRecord
    {
        public char Exch;
        public char ExchType; // 'C' for Cash, 'D' for Deriv
        public int ScripCode;
        public byte ScripNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] ScripName;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TLoginRequestRecord
    {
        public byte FEIDLength;
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
        public byte ClientIPLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ClientIP;
        public byte ServerIPLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ServerIP;
        public byte ITS_IPLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ITS_IP;
        public byte MachineIDLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] MachineID;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TChangePasswordRequestRecord
    {
        public byte FEIDLength;
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
        public byte ClientIPLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ClientIP;
        public byte ServerIPLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ServerIP;
        public byte ITS_IPLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ITS_IP;
        public byte MachineIDLen;
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

        public int Reserved1;
        public int Reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public Char[] Reserved3;
        public byte Reserved4;
        public byte Reserved5;


        public string toJsonString()
        {
            string data = "\r\n\r\n------------REQUEST----------\r\n"
                + $"User ID : {new string(FEUserID)}{Environment.NewLine}"
                + $"FEUserIDLength : {FEUserIDLength}{Environment.NewLine}"
                + $"Password1 : {new string(Password1)}{Environment.NewLine}"
                + $"Password1Length : {Password1Length}{Environment.NewLine}"
                + $"PAN : {new string(PAN)}{Environment.NewLine}"
                + $"PANLength : {PANLength}{Environment.NewLine}"
                + $"DOB : {new string(DOB)}{Environment.NewLine}"
                + $"DOBLength : {DOBLength}{Environment.NewLine}"
                + $"OTP : {OTP}{Environment.NewLine}"
                + $"MPIN : {MPin}{Environment.NewLine}"
                + $"Version : {new string(Version)}{Environment.NewLine}"
                + $"VersionLength : {VersionLength}{Environment.NewLine}"
                + $"Journey : {Journey}{Environment.NewLine}"
                + $"Client IP : {new string(ClientIP)}{Environment.NewLine}"
                + $"ClientIPLength : {ClientIPLength}{Environment.NewLine}"
                + $"Server IP : {new string(ServerIP)}{Environment.NewLine}"
                + $"ServerIPLength : {ServerIPLength}{Environment.NewLine}"
                + $"ITS IP : {new string(ITS_IP)}{Environment.NewLine}"
                + $"ITS_IPLength : {ITS_IPLength}{Environment.NewLine}"
                + $"Device ID : {new string(DeviceID)}{Environment.NewLine}"
                + $"DeviceIDLength : {DeviceIDLength}{Environment.NewLine}"
                + $"Device Type : {new string(DeviceType)}{Environment.NewLine}"
                + $"DeviceTypeLength : {DeviceTypeLength}{Environment.NewLine}"
                + $"Session ID : {new string(SessionId)}{Environment.NewLine}"
                + $"Time : {DateTime.Now.ToString("dd-MM-yy HH:mm:ss.fff")}{Environment.NewLine}";
            return data;
        }
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
    public struct TScripMarginRequestRecord
    {
        public char Exch;
        public int ScCode;
    }
    //Add structure 2015.03.26
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TScripInfoRequestRecord
    {
        public char Exch;
        public int ScCode;
    }
    //End structure 2015.03.26

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TTradeReportRequestRecord
    {
        public int StartTime;
        public int EndTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TOrderReportRequestRecord
    {
        public int StartTime;
        public int EndTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TExerciseReportRequestRecord
    {
        public int StartTime, EndTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TActivityLogRequestRecord
    {
        public int StartTime;
        public int EndTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]          // Sumit 21_jan_22 For Single Activity Log
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

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TAHOrderReportRequestRecord
    {
        public int StartTime;
        public int EndTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TMoneyTransferRequestRecord
    {
        public byte AddToBse; //boolean
        public int Amount;
        public int TrxID;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TTraderIDRecord
    {
        public byte FEUserIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] FEUserID;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//Atish 25.03.2019
    public struct TTraderIDRecordForSurveillance
    {
        public byte FEUserIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public char[] FEUserID;
        public char Status;
        public byte DealerIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public char[] DealerID;
        public long LastUpdatedMarginTime;
        public long LastUpdatedClientsTime;
        public byte SegmentIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] SegmentID;
        public byte BranchIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public char[] BranchID;
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
    public struct TPeakMarginData
    {
        public byte FEUserIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public char[] FEUserID;
        public int SegmentId;
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
    public struct TClientPositionReportRequestRecord
    {
        public byte ClientCodeLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ClientCode;
        public int ScripCode;  //if 0 then all scrips, else selected scrip
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
    public struct TScripCategoryReportRequestRecord
    {
        public int Junk;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TMaalFodoClientsReportRequestRecord
    {
        public int ClientType;
        public int MinimumDebit;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TMaalFodoClientHoldingReportRequestRecord
    {
        public int ClientType;
        public int MinimumDebit;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TMaalFodoClientAllowToTradeRequestRecord
    {
        public byte TraderIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] TraderID;
        public char Category;
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
        public byte ITSIPLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ITS_IP;
        public byte MachineIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] MachineID;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TLogoutRequestRecord_2FA
    {
        public byte ClientIPLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ClientIP;
        public byte ServerIPLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ServerIP;
        public byte ITSIPLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ITS_IP;
        public byte MachineIDLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] MachineID;
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
        public long ExchangeOrderId;
        public int ExchangeOrderTime;
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
    }

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

    #region Structure for Disclosed Qty 15 Apr 2014

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
        public double Reciepts;
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
        public byte Reciepts;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //10-Sep-2016 - Rohan
    public struct MTMClientsWithAllMargins
    {
        public byte FELength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] FEUserID;
        public double AvailableMargin;
        public double MBPO;
        public double MBOP;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//30.05.2016 Pranoti
    public struct UpdateClientsReply
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public string Status;
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

    //ITS->>>>IIS By Sandeep
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TOrderTradeGeneral
    {
        public int DateOfDay;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string ActiveUSers;
    }

    #endregion

  

    public class IBuffer
    {
        int WriteCount, ReadCount;
        byte[] buffer;

        public IBuffer(int FBufSize)
        {
            buffer = new byte[FBufSize];
        }

        public int BufUsed
        {
            get { return (WriteCount - ReadCount); }
        }

        public int TotalBuffSize
        {
            get { return buffer.Length; }
        }

        /// <summary>
        /// Write Data to Socket buffer.
        /// </summary>        
        /// <param name="Data">byte[] data to write into a socket buffer.</param>
        /// <param name="Length">Length of the data to write.</param>
        public void Write(byte[] Data, int Length)
        {
            int Remaining;
            int Copied;

            Remaining = buffer.Length - WriteCount;
            if (Remaining < Length)
                JiggleABit();

            Copied = Length;

            Array.Copy(Data, 0, buffer, WriteCount, Copied);
            WriteCount = WriteCount + Copied;
        }

        /// <summary>
        /// Writes Data to Socket buffer, and dynamically increases buffer size if required.
        /// </summary>        
        /// <param name="Data">byte[] data to write into a socket buffer.</param>
        /// <param name="Length">Length of the data to write.</param>
        public void WriteAndExtend(byte[] Data, int Length)
        {
            int Remaining;
            int Copied;

            Remaining = buffer.Length - WriteCount;
            if (Remaining < Length)
                JiggleABit();

            Remaining = buffer.Length - WriteCount;            //-----------------+
            if (Remaining < Length)                            // Dynamic resizing|
                Array.Resize(ref buffer, WriteCount + Length); //-----------------+

            Copied = Length;

            Array.Copy(Data, 0, buffer, WriteCount, Copied);
            WriteCount = WriteCount + Copied;
        }

        /// <summary>
        /// Read Data from Socket buffer upto specified length.
        /// </summary>        
        /// <param name="data">byte[] data to read from a socket buffer.</param>
        /// <param name="length">Length of the data to be read.</param>
        public void Read(byte[] data, int length)
        {
            int Remaining;
            int Copied;

            Remaining = WriteCount - ReadCount;
            if (Remaining <= 0)
            {
                return;
            }
            else
            {
                if (length < Remaining)
                {
                    Copied = length;
                }
                else
                {
                    Copied = Remaining;
                }
                Array.Copy(buffer, ReadCount, data, 0, Copied);
                ReadCount = ReadCount + Copied;

                if (ReadCount == WriteCount)
                {
                    ReadCount = 0;
                    WriteCount = 0;
                }
                return;
            }
        }

        /// <summary>
        /// Read Data from Socket buffer upto specified length W/O updating Read position of socket buffer.
        /// </summary>      
        /// <param name="data">byte[] data to read from a socket buffer.</param>
        /// <param name="length">length of the data to be read.</param>
        public void Peek(byte[] data, int length)
        {
            int Remaining;
            int Copied;
            Remaining = WriteCount - ReadCount;
            if (Remaining <= 0)
            {
                return;
            }
            else
            {
                if (length < Remaining)
                {
                    Copied = length;
                }
                else
                {
                    Copied = Remaining;
                }
                Array.Copy(buffer, ReadCount, data, 0, Copied);
                return;
            }
        }

        /// <summary>
        /// Use incase buffer overflow condition,
        /// when buffer has some space available,
        /// but not enough to store a complete 
        /// record of struct size.
        /// </summary>
        public void JiggleABit()
        {
            int DataAvail;//, SpaceAvail;

            //SpaceAvail = buffer.Length - WriteCount;
            //if (SpaceAvail > 0) return;


            DataAvail = WriteCount - ReadCount;
            if (DataAvail <= 0) return;

            Array.Copy(buffer, ReadCount, buffer, 0, DataAvail); //Move((PChar(Buf) + RdCount)^, (PChar(Buf))^,WrCount-RdCount);

            WriteCount = WriteCount - ReadCount;
            ReadCount = 0;
        }

        public void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);
            ReadCount = 0;
            WriteCount = 0;
        }

        public void GetAllData(byte[] data)
        {
            Read(data, BufUsed);
        }
    }

    public class TClientConnectionToITS
    {
        public object ConnectionID;
        public ushort SockNum;
        public DateTime SockCreateTime;

        public string FEUserID;
        public string Version;
        public string ClientIP;
        public string ServerIP;
        public byte ServerIPLength;
        public string ITS_IP;
        public byte ITS_IPLength;
        public bool Is2FA;

        public DateTime ConnTime;
        public DateTime DisconnTime;
        public DateTime HandShakeRecvdTime;
        public string TraderID;
        public byte FEUserType;
        public string FEUserPassword;
        public string FEUserPassword2;
        public int ClientCnt;
        //public ushort ClientCnt;
        //public TTraderRecord[] ClientArray;
        public string[] ClientArray;
        public string[] ClientArrayForMargins;
        public string[] ClientArrayForMcxMargins;
        public string[] ClientArrayForMarginsInstrumentWise;
        public string[] ClientArrayForOR;
        public string[] ClientArrayForTR;
        public string MachineID;
        public List<byte> PendingData = new List<byte>();

        public IBuffer Buff;

        public TClientConnectionToITS(ushort SocNum, string ConId, string RemotHost, string LocalIp, int BufSize, bool Is2Fa)
        {
            SockCreateTime = DateTime.Now;
            HandShakeRecvdTime = DateTime.Now;

            Buff = new IBuffer(BufSize);
            ServerIP = RemotHost;
            ServerIPLength = (byte)ServerIP.Length;
            SockNum = SocNum;
            ConnectionID = ConId;
            ITS_IP = LocalIp;
            ITS_IPLength = (byte)LocalIp.Length;

            FEUserID = "";
            TraderID = "";
            FEUserType = 0;
            FEUserPassword = "";
            FEUserPassword2 = "";
            ClientCnt = 0;
            ClientArray = new string[0];  //for Non Trader Type only    
            ClientArrayForMargins = new string[0];  //for Margin of All 1 and 2 type clients  //Added on 28-Apr-2017 - Rohan
            ClientArrayForMcxMargins = new string[0];  //for MCX Margin of All 1 and 2 type clients  //Added on 28-Apr-2017 - Rohan
            ClientArrayForMarginsInstrumentWise = new string[0];
            ClientIP = "";
            MachineID = "";

            this.Is2FA = Is2Fa;
        }

        //Structure declared on main form in dephi code
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TTraderRecord
        {
            public byte TraderIDLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] TraderID;
        }
    }

    public static class IntellectExtensions
    {
        public static byte[] ToByteArray(this object anything)
        {
            int rawsize = Marshal.SizeOf(anything);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(anything, buffer, false);
            byte[] rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);
            Marshal.FreeHGlobal(buffer);
            return rawdatas;
        }

        public static T ToStructure<T>(this byte[] data) where T : struct
        {
            unsafe
            {
                fixed (byte* p = &data[0])
                {
                    return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
                }
            }
        }

        public static byte[] ToAllRequestsArray(this TAllRequestNewRecord AllReqst, byte ReqType)
        {
            byte[] ArrReturn = null;
            switch (ReqType)
            {
                case 1:
                case 3:
                    {
                        ArrReturn = AllReqst.LoginRequest.ToByteArray();
                    }
                    break;
                case 85:
                    {
                        ArrReturn = AllReqst.LoginRequestTwoFactor.ToByteArray();
                    }
                    break;
                case 2:
                    {
                        ArrReturn = AllReqst.ChgPswdRequestRec.ToByteArray();
                    }
                    break;
                case 6:
                    {
                        ArrReturn = AllReqst.VerifyTraderRequestRec.ToByteArray();
                    } break;
                case 5:
                    {
                        ArrReturn = AllReqst.TraderListRequestRec.ToByteArray();
                    } break;
                case 10:
                    {
                        ArrReturn = AllReqst.EquityOrderPlaceRequestRec.ToByteArray();
                    } break;
                case 20:
                    {
                        ArrReturn = AllReqst.EquityOrderModifyRequestRec.ToByteArray();
                    } break;
                case 30:
                    {
                        ArrReturn = AllReqst.EquityOrderCancelRequestRec.ToByteArray();
                    } break;
                case 35:
                case 34:
                    {
                        ArrReturn = AllReqst.MFSIPRegistrationReqRec.ToByteArray();
                    }break;
                case 53:
                    {
                        ArrReturn = AllReqst.XSIPMandateReqrec.ToByteArray();
                    } break;
                case 39:
                    {
                        ArrReturn = AllReqst.XMFSIPRegistrationReqRec.ToByteArray();
                    }
                    break;
                case 41:
                    {
                        ArrReturn = AllReqst.ScripMarginRequestRec.ToByteArray();
                    } break;
                //2015.03.26 Add  Case 42
                case 42:
                    {
                        ArrReturn = AllReqst.ScripInfoRequestRec.ToByteArray();
                    } break;
                case 44:
                case 45:
                case 43://BseMf Krutika
                    {
                        ArrReturn = AllReqst.MFAddModifyCancelReqRec.ToByteArray();
                    } break;
                case 49:
                    {
                        ArrReturn = AllReqst.ScripMarginRequestRec.ToByteArray();
                    } break;
                case 61:
                case 71:
                case 142:
                case 58:      // sumitp 28_jan_22
                    {
                        ArrReturn = AllReqst.TradeReportRequestRec.ToByteArray();
                    } break;
                case 125:
                case 7:
                case 8:
                case 52:
                case 62:
                case 78:    //HIMANSHU  // sumitp 28_jan_22
                case 72:
                case 141:
                    {
                        ArrReturn = AllReqst.OrderReportRequestRec.ToByteArray();
                    } break;
                case 63://OPTION EXERCISE
                    {
                        ArrReturn = AllReqst.ExerciseReportRequestRec.ToByteArray();
                        
                    }
                    break;
                case 64:
                case 104:
                case 204:
                    {
                        ArrReturn = AllReqst.ActivityLogRequestRec.ToByteArray();
                    }
                    break;
                case 202:           // Sumit 21_jan_22 For Single Activity Log
                    {
                        ArrReturn = AllReqst.ActivityLogRequestRec_new.ToByteArray();   
                    }
                    break;
                case 69:
                case 75:
                //case 241://SLBM
                case ConstantUnits.REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL:
                    {
                        ArrReturn = AllReqst.HoldingsReportRequestRec.ToByteArray();
                    } break;
                case ConstantUnits.REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL_New:
                    {
                        ArrReturn = AllReqst.HoldingsReportRequestRec.ToByteArray();
                    } break;
                case 76:    //Added on 08-Nov-2016 - Rohan
                    {
                        ArrReturn = AllReqst.HoldingsEquityReportRequestRec.ToByteArray();
                    } break;
                case 70:
                    {
                        ArrReturn = AllReqst.HoldingsValuationReportRequestRec.ToByteArray();
                    }
                    break;
                case 218:
                    {
                        ArrReturn = AllReqst.HoldingsValuationReportRequestRec.ToByteArray();
                    }
                    break;
                case 170:
                    {
                        ArrReturn = AllReqst.HoldingsValuationReportWithMemVarRequestRec.ToByteArray(); //20.05.15
                    }
                    break;
                case 74:
                    {
                        ArrReturn = AllReqst.HoldingsReportWithCollateralRequestRec.ToByteArray();
                    } break;
                case 79:
                    {
                        ArrReturn = AllReqst.ClientPositionRequestRec.ToByteArray();

                    } break;
                case 80:
                    {
                        ArrReturn = AllReqst.TradeToTradeRequestRec.ToByteArray();
                    }
                    break;
                case 81:
                    {
                        ArrReturn = AllReqst.BseNseMapRequestRec.ToByteArray();
                    } break;
                case 82:
                    {
                        ArrReturn = AllReqst.DerivBannedScripRequestRec.ToByteArray();
                    } break;
                case 83:
                    {
                        ArrReturn = AllReqst.AdhocReportRequestRec.ToByteArray();
                    }
                    break;
                case 84:
                    {
                        ArrReturn = AllReqst.AdhocModifyRequestRec.ToByteArray();
                    }
                    break;
                case 86:
                    {
                        ArrReturn = AllReqst.MFClientsReportRequestRec.ToByteArray();
                    } break;
                case 89:
                    {
                        ArrReturn = AllReqst.IlliquidRequestRec.ToByteArray();
                    } break;
                case 92://BseMf Krutika
                    {
                        ArrReturn = AllReqst.HoldingsReportOfMFRequestRec.ToByteArray();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        ArrReturn = AllReqst.BseMfOrderReportRequestRec.ToByteArray();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_RPT_ACTIVITY_BSEMF://BseMf Krutika
                    {
                        ArrReturn = AllReqst.BseMfActivitylogRequestRec.ToByteArray();
                    } break;
          
                case 67:
                    {
                        ArrReturn = AllReqst.HoldingsReportWithCollatralPreCloseReplyRecNew.ToByteArray();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_SIP_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        ArrReturn = AllReqst.BseMfOrderReportRequestRec.ToByteArray();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_MANDATE_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        ArrReturn = AllReqst.MandateOrderReportRequestRec.ToByteArray();
                    } break;
                case 94://Mcx Trading
                case 106: // 106 jinal
                    {
                        ArrReturn = AllReqst.HoldingsReportWithCollatralPreCloseReplyRecNew.ToByteArray();
                    } break;
                #region LotQty
                case 91:
                    {
                        ArrReturn = AllReqst.LotQtyRequestRec.ToByteArray();
                    } break;
                #endregion
                case 110:
                    {
                        ArrReturn = AllReqst.AHDerivOrderPlaceRequestRec.ToByteArray();
                    }
                    break;
                case 120:
                    {
                        ArrReturn = AllReqst.DerivOrderModifyRequestRec.ToByteArray();
                    }
                    break;
                case 130:
                    {
                        ArrReturn = AllReqst.DerivOrderCancelRequestRec.ToByteArray();
                    }
                    break;

                case 140:
                    {
                        ArrReturn = AllReqst.CashFutureAlgoOrderPlaceRequestRec.ToByteArray();
                    } break;
                case 191:
                    {
                        ArrReturn = AllReqst.AHSpreadOrderPlaceRequestRec.ToByteArray();
                    }
                    break;
                case 192:
                case 193:
                    {
                        ArrReturn = AllReqst.SpreadOrderCancelRequestRec.ToByteArray();
                    }
                    break;
                case 200:
                    {
                        ArrReturn = AllReqst.MarginReportRequestRec.ToByteArray();
                    } break;
                case 206:
                    {
                        ArrReturn = AllReqst.MarginReportRequestRecMCX.ToByteArray();
                    } break;
                case 208:
                    {
                        ArrReturn = AllReqst.MarginReportRequestRecCombined.ToByteArray();
                    } break;
                #region MTM - Added on 12-Sep-2016 - Rohan
                case 203:
                    {
                        ArrReturn = AllReqst.ClientsForMTM.ToByteArray();
                    } break;
                //case 208:   //Added on 08-Mar-2017 - Rohan
                //    {
                //        ArrReturn = AllReqst.ClientsForMTMMcx.ToByteArray();
                //    } break;
                case 205:   //Added on 10-Sep-2016 - Rohan
                    {
                        ArrReturn = AllReqst.ClientForMTMReq.ToByteArray();
                    } break;
                case 207:   //Added on 08-Mar-2017 - Rohan
                    {
                        ArrReturn = AllReqst.ClientForMTMMcxReq.ToByteArray();
                    } break;
                #endregion

                #region Disclosed Qty 15 Apr 2014
                case 211:
                    {
                        ArrReturn = AllReqst.CurrDerivOrderPlaceRequestRec.ToByteArray();
                    }
                    break;
                case 221:
                    {
                        ArrReturn = AllReqst.CurrDerivOrderModifyRequestRec.ToByteArray();
                    }
                    break;
                case 231:
                    {
                        ArrReturn = AllReqst.CurrDerivOrderCancelRequestRec.ToByteArray();
                    }
                    break;

                #endregion
                case 209:
                    {
                        ArrReturn = AllReqst.TemplateMarginReqRecord.ToByteArray();
                    } break;
                case 172:
                    {
                        ArrReturn = AllReqst.TemplateMarginReqRecord_Peak.ToByteArray();
                    }
                    break;
                case 210:
                    {
                        ArrReturn = AllReqst.TemplateMarginNew.ToByteArray();
                    }
                    break;

                case 173:
                    {
                        ArrReturn = AllReqst.PeakMarginData.ToByteArray();
                    }
                    break;
                case 217:
                    {
                        ArrReturn = AllReqst.TemplateMarginReqRecord.ToByteArray();
                    } break;
                case 248:
                    {
                        ArrReturn = AllReqst.GetSegmentMaster.ToByteArray();
                    }
                    break;
                case 250:   //Added on 10-Sep-2016 - Rohan
                    {
                        ArrReturn = AllReqst.ClientForMTMReqInstrumentWise.ToByteArray();
                    } break;
                case 226:
                    {
                        ArrReturn = AllReqst.GetTradesReport.ToByteArray();
                    }
                    break;
                case 201:
                    {
                        ArrReturn = AllReqst.OrderReportRequestRec.ToByteArray();
                    }
                    break;
                case 122:
                    {
                        ArrReturn = AllReqst.PlaceOfsOrderRequest.ToByteArray();
                    }
                    break;
                case 123:
                    {
                        ArrReturn = AllReqst.PlaceOfsOrderRequest.ToByteArray();
                    }
                    break;
                case 124:
                    {
                        ArrReturn = AllReqst.PlaceOfsOrderRequest.ToByteArray();
                    }
                    break;
                case 235: //21.11.18 SLBM
                    {
                        ArrReturn = AllReqst.SLBMOrderPlaceRequestRec.ToByteArray();
                    }
                    break;
                case 236: //21.11.18 SLBM
                    {
                        ArrReturn = AllReqst.SLBMOrderModifyRequestRec.ToByteArray();
                    }
                    break;
                case 237: //21.11.18 SLBM
                    {
                        ArrReturn = AllReqst.SLBMOrderCancelRequestRec.ToByteArray();
                    }
                    break;
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
            return ArrReturn;
        }

        public static TAllRequestNewRecord ToAllRequestsStruct(this byte[] buffToReadBody, byte ReqType)
        {
            TAllRequestNewRecord AllReq = new TAllRequestNewRecord();
            switch (ReqType)
            {
                case 1:
                case 3:
                    {
                        AllReq.LoginRequest = buffToReadBody.ToStructure<TLoginRequestRecord>();
                    }
                    break;
                case 85:
                    {
                        AllReq.LoginRequestTwoFactor = buffToReadBody.ToStructure<TLoginRequestRecordTwoFactor>();
                    }
                    break;
                case 2:
                    {
                        AllReq.ChgPswdRequestRec = buffToReadBody.ToStructure<TChangePasswordRequestRecord>();
                    }
                    break;
                case 5:
                    {
                        AllReq.TraderListRequestRec = buffToReadBody.ToStructure<TTraderListRequestRecord>();
                    } break;
                case 6:
                    {
                        AllReq.VerifyTraderRequestRec = buffToReadBody.ToStructure<TVerifyTraderRequestRecord>();
                    } break;
                case 10:
                    {
                        AllReq.EquityOrderPlaceRequestRec = buffToReadBody.ToStructure<TEquityOrderPlaceRequestRecord>();
                    } break;
                case 20:
                    {
                        AllReq.EquityOrderModifyRequestRec = buffToReadBody.ToStructure<TEquityOrderModifyRequestRecord>();
                    } break;
                case 30:
                    {
                        AllReq.EquityOrderCancelRequestRec = buffToReadBody.ToStructure<TEquityOrderCancelRequestRecord>();
                    } break;
                case 35:
                case 39:
                    {
                        AllReq.XMFSIPRegistrationReqRec=buffToReadBody.ToStructure<XBseMFSIPRegistrationMsgRequestRecord>();
                    }break;
                case 34:
                    {
                        AllReq.MFSIPRegistrationReqRec = buffToReadBody.ToStructure<TBseMFSIPRegistrationMsgRequestRecord>();
                    }
                    break;
                case 187:
                    {
                        AllReq.XSIPMandateReqrec = buffToReadBody.ToStructure<XSIPMandate>();
                    }break;
                case 41:
                    {
                        AllReq.ScripMarginRequestRec = buffToReadBody.ToStructure<TScripMarginRequestRecord>();
                    } break;
                // 2015.03.26
                case 42:
                    {
                        AllReq.ScripInfoRequestRec = buffToReadBody.ToStructure<TScripInfoRequestRecord>();
                    } break;
                // 2015.03.26
                case 44:
                case 45:
                case 43://BseMf Krutika
                    {
                        AllReq.MFAddModifyCancelReqRec = buffToReadBody.ToStructure<TBseMFAddModifyCancelRequestRecord>();
                    } break;
                case 49:
                    {
                        AllReq.ScripMarginRequestRec = buffToReadBody.ToStructure<TScripMarginRequestRecord>();
                    } break;
                case 61:
                case 71:
                case 142:
                case 58:    // sumitp 28_jan_22
                    {
                        AllReq.TradeReportRequestRec = buffToReadBody.ToStructure<TTradeReportRequestRecord>();
                    }
                    break;
                case 7:
                case 8:
                case 125:
                case 52:
                case 62:
                case 78:           // Himanshu    // sumitp 28_jan_22
                case 72:
                case 141:
                    {
                        AllReq.OrderReportRequestRec = buffToReadBody.ToStructure<TOrderReportRequestRecord>();
                    }
                    break;
                case 63:
                    {
                        AllReq.ExerciseReportRequestRec = buffToReadBody.ToStructure<TExerciseReportRequestRecord>();
                        
                    }
                    break;
                case 64:
                case 104:
                case 204:
                    {
                        AllReq.ActivityLogRequestRec = buffToReadBody.ToStructure<TActivityLogRequestRecord>();
                    }
                    break;
                case 202: // Sumit 21_jan_22 For Single Activity Log
                    {
                        AllReq.ActivityLogRequestRec_new = buffToReadBody.ToStructure<TActivityLogRequestRecord_new>();
                    }
                    break;
                case 69:
                case 75:
                case 255: //SLBM Holding
                case ConstantUnits.REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL:
                    {
                        AllReq.HoldingsReportRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case ConstantUnits.REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL_New:
                    {
                        AllReq.HoldingsReportRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case 76:    //Added on 08-Nov-2016 -Rohan
                    {
                        AllReq.HoldingsEquityReportRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case 70:
                    {
                        AllReq.HoldingsValuationReportRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case 218:
                    {
                        AllReq.HoldingsValuationReportRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case 170:
                    {
                        AllReq.HoldingsValuationReportWithMemVarRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>(); //20.05.15
                    }
                    break;
                case 74:
                    {
                        AllReq.HoldingsReportWithCollateralRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case 79:
                    {
                        AllReq.ClientPositionRequestRec = buffToReadBody.ToStructure<TClientPositionReportRequestRecord>();
                    } break;
                case 80:
                    {
                        AllReq.TradeToTradeRequestRec = buffToReadBody.ToStructure<TDummyRecord1>();

                    } break;
                case 81:
                    {
                        AllReq.BseNseMapRequestRec = buffToReadBody.ToStructure<TDummyRecord1>();

                    } break;
                case 82:
                    {
                        AllReq.DerivBannedScripRequestRec = buffToReadBody.ToStructure<TDummyRecord1>();
                    } break;
                case 83:
                    {
                        AllReq.AdhocReportRequestRec = buffToReadBody.ToStructure<TAdhocReportRequestRecord>();
                    } break;
                case 84:
                    {
                        AllReq.AdhocModifyRequestRec = buffToReadBody.ToStructure<TAdhocModifyRequestRecord>();
                    } break;
                case 86:
                    {
                        AllReq.MFClientsReportRequestRec = buffToReadBody.ToStructure<TMaalFodoClientsReportRequestRecord>();
                    } break;
                case 89:
                    {
                        AllReq.IlliquidRequestRec = buffToReadBody.ToStructure<TDummyRecord1>();
                    } break;
                case 92://BseMf Krutika
                    {
                        AllReq.HoldingsReportOfMFRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case ConstantUnits.REP_ITS_TO_FE_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        AllReq.BseMfOrderReportRequestRec = buffToReadBody.ToStructure<TBseMfOrderReportRequestRecord>();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_RPT_ACTIVITY_BSEMF://BseMf Krutika
                    {
                        AllReq.BseMfActivitylogRequestRec = buffToReadBody.ToStructure<TBseMfActivitylogRequestRecord>();
                    } break; 
               
                case 67:
                     {
                        AllReq.HoldingsReportWithCollatralPreCloseReplyRecNew = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case ConstantUnits.REP_ITS_TO_FE_SIP_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        AllReq.BseMfOrderReportRequestRec = buffToReadBody.ToStructure<TBseMfOrderReportRequestRecord>();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_MANDATE_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        AllReq.MandateOrderReportRequestRec = buffToReadBody.ToStructure<MandateOrderReportRequestRecord>();
                    } break;
                case 94://McxTrading
                case 106:
                    {
                        AllReq.HoldingsReportWithCollatralPreCloseReplyRecNew = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                #region LotQty
                case 91:
                    {
                        AllReq.LotQtyRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                #endregion

                case 110:
                    {
                        AllReq.AHDerivOrderPlaceRequestRec = buffToReadBody.ToStructure<TAHDerivOrderPlaceRequestRecord>();
                    } break;
                case 120:
                    {
                        AllReq.DerivOrderModifyRequestRec = buffToReadBody.ToStructure<TDerivOrderModifyRequestRecord>();
                    } break;
                case 130:
                    {
                        AllReq.DerivOrderCancelRequestRec = buffToReadBody.ToStructure<TDerivOrderCancelRequestRecord>();
                    }
                    break;
                case 140:
                    {
                        AllReq.CashFutureAlgoOrderPlaceRequestRec = buffToReadBody.ToStructure<TCashFutureAlgoOrderPlaceRequestRecord>();
                    } break;
                case 191:
                    {
                        AllReq.AHSpreadOrderPlaceRequestRec = buffToReadBody.ToStructure<TSpreadOrderPlaceRequestRecord>();
                    } break;
                case 192:
                case 193:
                    {
                        AllReq.SpreadOrderCancelRequestRec = buffToReadBody.ToStructure<TSpreadOrderCancelRequestRecord>();
                    } break;
                case 200:
                    {
                        AllReq.MarginReportRequestRec = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case 206:
                    {
                        AllReq.MarginReportRequestRecMCX = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                case 208:
                    {
                        AllReq.MarginReportRequestRecCombined = buffToReadBody.ToStructure<TTraderIDRecord>();
                    }
                    break;
                #region MTM - Added on 12-Sep-2016 - Rohan
                case 203:
                    {
                        AllReq.ClientsForMTM = buffToReadBody.ToStructure<TUpdateClientsForMTM>();
                    }
                    break;
                //case 208:   //Added on 08-Mar-2017 - Rohan
                //    {
                //        AllReq.ClientsForMTMMcx = buffToReadBody.ToStructure<TUpdateClientsForMTM>();
                //    }
                //    break;
                case 205:   //Added on 10-Sep-2016 - Rohan
                    {
                        AllReq.ClientForMTMReq = buffToReadBody.ToStructure<TTraderIDRecordForMTM>();
                    }
                    break;
                case 207:   //Added on 08-Mar-2017 - Rohan
                    {
                        AllReq.ClientForMTMMcxReq = buffToReadBody.ToStructure<TTraderIDRecordForMTM>();
                    }
                    break;
                #endregion
                case 253:
                    {
                        AllReq.HandShakeRec = buffToReadBody.ToStructure<TDummyRecord1>();
                    } break;
                case 245:
                    {
                        AllReq.ITS2OTSNumRec = buffToReadBody.ToStructure<TDummyRecord1>();
                    }
                    break;
                #region For Disclosed Qty
                case 211:
                    {
                        AllReq.CurrDerivOrderPlaceRequestRec = buffToReadBody.ToStructure<TCurrDerivOrderPlaceRequestRecord>();
                    } break;
                case 221:
                    {
                        AllReq.CurrDerivOrderModifyRequestRec = buffToReadBody.ToStructure<TCurrDerivOrderModifyRequestRecord>();
                    } break;
                case 231:
                    {
                        AllReq.CurrDerivOrderCancelRequestRec = buffToReadBody.ToStructure<TCurrDerivOrderCancelRequestRecord>();
                    }
                    break;
                #endregion
                case 246:
                    {
                        AllReq.LoginRequest = buffToReadBody.ToStructure<TLoginRequestRecord>();
                    } break;
                case 209:
                    {
                        AllReq.TemplateMarginReqRecord = buffToReadBody.ToStructure<TTemplateMarginRequestRecord>();
                    } break;
                case 172:
                    {
                        AllReq.TemplateMarginReqRecord_Peak = buffToReadBody.ToStructure<TTemplateMarginRequestRecord_Peak>();
                    }
                    break;
                case 210: //20.10.22
                    {
                        AllReq.TemplateMarginNew = buffToReadBody.ToStructure<TMarginReportReplyRecordTemplateWithDerivMargin>();
                    }
                    break;
                case 173:
                    {
                        AllReq.PeakMarginData = buffToReadBody.ToStructure<TPeakMarginData>();
                    }
                    break;
                case 217:
                    {
                        AllReq.TemplateMarginReqRecord = buffToReadBody.ToStructure<TTemplateMarginRequestRecord>();
                    } break;
                case 248:
                    {
                        AllReq.GetSegmentMaster = buffToReadBody.ToStructure<TDummyRecord>();

                    } break;
                case 250:   //Added on 10-Sep-2016 - Rohan
                    {
                        AllReq.ClientForMTMReqInstrumentWise = buffToReadBody.ToStructure<TTraderIDRecordForSurveillance>();
                    }
                    break;
                case 226:
                    {
                        AllReq.GetTradesReport = buffToReadBody.ToStructure<TTradesReportExchFormatRequest>();
                    }
                    break;
                case 201:
                    {
                        AllReq.OrderReportRequestRec = buffToReadBody.ToStructure<TOrderReportRequestRecord>();
                    }
                    break;
                case 122:
                    {
                        AllReq.PlaceOfsOrderRequest = buffToReadBody.ToStructure<TOrderPlaceRequestRecordOFS>();
                    }
                    break;
                case 123:
                    {
                        AllReq.PlaceOfsOrderRequest = buffToReadBody.ToStructure<TOrderPlaceRequestRecordOFS>();
                    }
                    break;
                case 124:
                    {
                        AllReq.PlaceOfsOrderRequest = buffToReadBody.ToStructure<TOrderPlaceRequestRecordOFS>();
                    }
                    break;
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
            return AllReq;
        }

        public static byte[] ToAllReplyArray(this TAllReplyNewRecord AllReply, byte ReplyType)
        {
            byte[] ArrReturn = null;

            switch (ReplyType)
            {
                case 1:
                    {
                        ArrReturn = AllReply.LoginReply.ToByteArray();
                    }
                    break;
                case 2:
                    {
                        ArrReturn = AllReply.ChgPswdReplyRec.ToByteArray();

                    } break;
                case 3:
                    {
                        ArrReturn = AllReply.LoginRepyNew.ToByteArray();
                    } break;
                case 85:
                    {
                        ArrReturn = AllReply.LoginTwoFactorAuthRecord.ToByteArray();
                    }
                    break;
                case 5:
                case 6:
                    {
                        ArrReturn = AllReply.TraderListReplyRec.ToByteArray();
                    } break;
                case 7:
                    {
                        ArrReturn = AllReply.OrdersReportDealerReply.ToByteArray();
                    } break;
                case 8:
                    {
                        ArrReturn = AllReply.TradesReportDealerReply.ToByteArray();
                    } break;
                case 9:
                    {
                        ArrReturn = AllReply.LogoutUserRec.ToByteArray();
                    } break;
                case 10:
                    {
                        ArrReturn = AllReply.EquityOrderPlaceBrokerReplyRec.ToByteArray();
                    } break;
                case 11:
                case 21:
                case 31:
                case 33:
                    {
                        ArrReturn = AllReply.EquityOrderExchReplyRec.ToByteArray();
                    } break;
                case 16:
                    {
                        ArrReturn = AllReply.AHOrderRejectedRecord.ToByteArray();
                    } break;
                case 20:
                    {
                        ArrReturn = AllReply.EquityOrderModifyBrokerReplyRec.ToByteArray();
                    } break;
                case 30:
                    {
                        ArrReturn = AllReply.EquityOrderCancelBrokerReplyRec.ToByteArray();
                    } break;
                case 32:
                    {
                        ArrReturn = AllReply.KillOrderExchReplyRec.ToByteArray();
                    } break;
                case 35:
                case 34:
                    {
                        ArrReturn = AllReply.MFSIPRegistrationBrokerResRec.ToByteArray();
                    }break;
                case 187:
                    {
                        ArrReturn = AllReply.XSIPMandateBrokerReply.ToByteArray();
                    } break;
                case 39:
                    {
                        ArrReturn = AllReply.MFSIPRegistrationBrokerResRec.ToByteArray();
                    }
                    break;
                case 37:
                case 36:
                    {
                        ArrReturn = AllReply.MFSIPRegistrationExchangeResRec.ToByteArray();
                    }break;
                case 188:
                    {
                        ArrReturn = AllReply.XSIPMandateExchangeReplyRec.ToByteArray();
                    } break;
                case 41:
                    {
                        ArrReturn = AllReply.ScripMarginReplyRec.ToByteArray();
                    } break;
                case 42:
                    {
                        ArrReturn = AllReply.ScripInfoReplyRec.ToByteArray();
                    } break;
                case 43://BseMf Krutika
                case 44:
                case 45:
                    {
                        ArrReturn = AllReply.MFAddModifyCancelBrokerResRec.ToByteArray();
                    } break;
                case 46:
                case 47:
                case 48:
                    {
                        ArrReturn = AllReply.MFAddModifyCancelExchangeResRec.ToByteArray();
                    } break;
                case 49:
                    {
                        ArrReturn = AllReply.NewScripMarginReplyRec.ToByteArray();
                    } break;
                case 50:
                    {
                        ArrReturn = AllReply.ExchTradeConfirmationRec.ToByteArray();
                    } break;
                case 51:
                    {
                        ArrReturn = AllReply.SLTriggerRec.ToByteArray();
                    } break;
                //case 52:
                //    {
                //        ArrReturn = AllReply.MarketToLimitRecBSE.ToByteArray();
                //    } break;
                case 53:
                    {
                        ArrReturn = AllReply.MarketToLimitRecNSE.ToByteArray();
                    } break;
                case 54:
                    {
                        ArrReturn = AllReply.ExchangeStatusRec.ToByteArray();
                    } break;
                case 56: //McxTrading 
                    {
                        ArrReturn = AllReply.McxGrpHalt.ToByteArray();
                    } break;
                case 59:
                    {
                        ArrReturn = AllReply.OrderReportForSurviellenceReplyRec.ToByteArray();
                    }
                    break;
                //case 61:
                //    {
                //        ArrReturn = AllReply.TradeReportReplyRec.ToByteArray();
                //    }
                //    break;
                //case 62:
                //    {
                //        ArrReturn = AllReply.OrderReportReplyRec.ToByteArray();
                //    }
                //    break;
                case 63:
                    {
                        ArrReturn = AllReply.ExerciseReportReplyRec.ToByteArray();
                    } break;
                //case 64:
                //    {
                //        ArrReturn = AllReply.ActivityLogReplyRec.ToByteArray();
                //    }
                //    break;
                case 65:
                    {
                        ArrReturn = AllReply.AHOrderReportReplyRec.ToByteArray();
                    } break;
                case 66:
                    {
                        ArrReturn = AllReply.LicenceInfoReplyRec.ToByteArray();
                    } break;
                case 68:
                    {
                        ArrReturn = AllReply.ClientDbNumReplyRec.ToByteArray();
                    } break;
                case 69:
                    {
                        ArrReturn = AllReply.HoldingsReportReplyRec.ToByteArray();
                    }
                    break;
                case 70:
                    {
                        ArrReturn = AllReply.HoldingsValuationReportReplyRec.ToByteArray();
                    }
                    break;
                case 218:
                    {
                        ArrReturn = AllReply.HoldingsValuationReportReplyRec.ToByteArray();
                    }
                    break;
                case 170:
                    {
                        ArrReturn = AllReply.HoldingsValuationReportWithMemVarReplyRec.ToByteArray();
                    }
                    break;
                case 71: //04.05.15
                case 142:
                    {
                        ArrReturn = AllReply.TradeReportReplyRecNew.ToByteArray();
                    } break;
                case 61:    //Added on 18-Apr-2018 - Rohan
                    {
                        ArrReturn = AllReply.TradeReportReplyRecNewSpread.ToByteArray();
                    } break;
                case 58:    //Cover Trade Report // sumitp 28_jan_22
                    {
                        ArrReturn = AllReply.OrderTypeTradeReportReplyRecNewCover.ToByteArray();
                    }
                    break;
                case 72:
                case 141:
                    {
                        ArrReturn = AllReply.OrderReportReplyRecNew.ToByteArray();
                    }
                    break;
                case 62:    //Added on 18-Apr-2018 - Rohan
                    {
                        ArrReturn = AllReply.OrderReportReplyRecNewSpread.ToByteArray();
                    }
                    break;
                case 78:    //Added on 27-Nov-2011 - Himanshu  // sumitp 28_jan_22
                    {
                        ArrReturn = AllReply.OrderReportReplyRecCoverOrBracket.ToByteArray();
                    }
                    break;
                case 52:    //Added on 18-Apr-2018 - Rohan
                    {
                        ArrReturn = AllReply.OrderReportReplyRecNewSpreadWithRejOrder.ToByteArray();
                    }
                    break;
                case 64:    //Added on 18-Apr-2018 - Rohan
                    {
                        ArrReturn = AllReply.ActivityLogReplyRecForRequestorNewSpread.ToByteArray();
                    }
                    break;
                case 74:
                    {
                        ArrReturn = AllReply.HoldingsReportWithCollateralReplyRec.ToByteArray();
                    }
                    break;
                case 75:
                    {
                        ArrReturn = AllReply.AllClientsHoldingsReportReplyRec.ToByteArray();
                    }
                    break;
                case 76:    //Added on 08-Nov-2016 - Rohan
                    {
                        ArrReturn = AllReply.AllClientsHoldingsEquityReportReplyRec.ToByteArray();
                    }
                    break;
                case 79:
                    {
                        ArrReturn = AllReply.ClientPositionReplyRec.ToByteArray();
                    } break;
                case 80:
                    {
                        ArrReturn = AllReply.TradeToTradeReplyRec.ToByteArray();

                    } break;
                case 81:
                    {
                        ArrReturn = AllReply.BseNseMapReplyRec.ToByteArray();

                    } break;
                case 82:
                    {
                        ArrReturn = AllReply.DerivBannedScripReplyRec.ToByteArray();

                    } break;
                case 83:
                    {
                        ArrReturn = AllReply.AdhocReportReplyRec.ToByteArray();
                    } break;
                case 84:
                    {
                        ArrReturn = AllReply.AdhocModifyReplyRec.ToByteArray();
                    } break;
                case 86:
                    {
                        ArrReturn = AllReply.MFClientsReportReplyRec.ToByteArray();
                    } break;
                case 89:
                    {
                        ArrReturn = AllReply.IlliquidReplyRec.ToByteArray();
                    } break;
                case 92://BseMf Krutika
                    {
                        ArrReturn = AllReply.HoldingsReportOfMFReplyRec.ToByteArray();
                    }
                    break;
                case ConstantUnits.REP_ITS_TO_FE_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        ArrReturn = AllReply.BseMfOrderReportReplyRecNew.ToByteArray();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_RPT_ACTIVITY_BSEMF://BseMf Krutika
                    {
                        ArrReturn = AllReply.BseMfActivitylogReplyRecNew.ToByteArray();
                    } break;
                    
                case ConstantUnits.REP_ITS_TO_FE_SIP_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        ArrReturn = AllReply.BseMfSIPOrderReportReplyRecNew.ToByteArray();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_MANDATE_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        ArrReturn = AllReply.MandateOrderTypeReportsReplyRecNew.ToByteArray();
                    } break;
                case 94://McxTrading
                    {
                        ArrReturn = AllReply.HoldingsReportWithCollatralReplyRecNew.ToByteArray();
                    }
                    break;
                case 67:
                    {
                        ArrReturn = AllReply.HoldingsReportWithMTF.ToByteArray();
                    }
                    break;
                case 90:
                    {
                        ArrReturn = AllReply.HoldingsReportWithMTFNew.ToByteArray();
                    }
                    break;
                case 194:
                    {
                        ArrReturn = AllReply.ISCL_HoldingEquityReply.ToByteArray();
                    }
                    break;
                case 106://McxTrading
                    {
                        ArrReturn = AllReply.HoldingsReportWithCollatralPreCloseReplyRecNew.ToByteArray();
                    }
                    break;
                case ConstantUnits.REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL://31.01.17 
                    {
                        ArrReturn = AllReply.AllClientsHoldingsReportReplyRecNew.ToByteArray();
                    }
                    break;
                case ConstantUnits.REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL_New://31.01.17 
                    {
                        ArrReturn = AllReply.AllClientsHoldingsReportReplyRecWithPreClose.ToByteArray();
                    }
                    break;

                #region LotQty

                case 91:
                    {
                        ArrReturn = AllReply.LotQtyOtsReplyRec.ToByteArray();
                    } break;
                #endregion
                case 110:
                    {
                        ArrReturn = AllReply.DerivOrderPlaceBrokerReplyRec.ToByteArray();
                    } break;
                case 111:
                case 121:
                case 131:
                case 133:
                    {
                        ArrReturn = AllReply.DerivOrderExchReplyRec.ToByteArray();
                    } break;
                case 120:
                    {
                        ArrReturn = AllReply.DerivOrderModifyBrokerReplyRec.ToByteArray();
                    } break;
                case 130:
                    {
                        ArrReturn = AllReply.DerivOrderCancelBrokerReplyRec.ToByteArray();
                    } break;
                case 150:
                    {
                        ArrReturn = AllReply.DerivExchTradeConfirmationRec.ToByteArray();
                    } break;
                case 151:
                    {
                        ArrReturn = AllReply.DerivSLTriggerRec.ToByteArray();
                    } break;
                case 200:
                    {
                        ArrReturn = AllReply.MarginReportReplyRec.ToByteArray();
                    }
                    break;
                case 206:
                    {
                        ArrReturn = AllReply.MarginReportReplyRecMCX.ToByteArray();
                    }
                    break;
                case 241://romil SLBM
                case 242:
                case 243://romil slbm test
                case 249://By Sandeep
                    {
                        ArrReturn = AllReply.SpdOrderExchReplyRec.ToByteArray();
                    }
                    break;
                case 191:
                    {
                        ArrReturn = AllReply.SpdOrderPlaceBrokerReplyRec.ToByteArray();
                    }
                    break;
                case 192:
                case 193:
                    {
                        ArrReturn = AllReply.SpdOrderCancelBrokerReplyRec.ToByteArray();
                    }
                    break;
                case 208:
                    {
                        ArrReturn = AllReply.MarginReportReplyRecCombined.ToByteArray();
                    }
                    break;
                #region MTM - Added on 12-Sep-2016 - Rohan
                case 203:
                    {
                        ArrReturn = AllReply.ClientsMTMReply.ToByteArray();
                    }
                    break;
                //case 208:   //Added on 08-Mar-2017 - Rohan
                //    {
                //        ArrReturn = AllReply.ClientsMTMReplyMcx.ToByteArray();
                //    }
                //    break;
                case 205:   //Added on 10-Sep-2016 - Rohan
                    {
                        ArrReturn = AllReply.MTMClientsWithAllMarginsReply.ToByteArray();
                    }
                    break;
                case 207:   //Added on 08-Mar-2017 - Rohan
                    {
                        ArrReturn = AllReply.MTMClientsWithAllMarginsReplyMcx.ToByteArray();
                    }
                    break;
                #endregion
                case 230:
                    {
                        ArrReturn = AllReply.ClearSentToExchRec.ToByteArray();
                    } break;
                case 251:
                    {
                        ArrReturn = AllReply.LinkFailRec.ToByteArray();
                    }
                    break;
                case 253:
                    {
                        ArrReturn = AllReply.HandShakeRec.ToByteArray();
                    } break;

                #region Disclosed Qty
                case 211:
                    {
                        ArrReturn = AllReply.CurrDerivOrderPlaceBrokerReplyRec.ToByteArray();
                    } break;
                case 212:
                case 222:
                case 232:
                case 233:
                    {
                        ArrReturn = AllReply.CurrDerivOrderExchReplyRec.ToByteArray();
                    } break;
                case 221:
                    {
                        ArrReturn = AllReply.CurrDerivOrderModifyBrokerReplyRec.ToByteArray();
                    } break;
                case 231:
                    {
                        ArrReturn = AllReply.CurrDerivOrderCancelBrokerReplyRec.ToByteArray();
                    } break;

                #endregion

                #region IntraDelivery
                case 104:
                    {
                        ArrReturn = AllReply.ActivityLogReplyRecNew.ToByteArray();
                    }
                    break;
                case 204: //15.10.16 Hetal
                    {
                        ArrReturn = AllReply.ActivityLogReplyRecNewForRequestor.ToByteArray();
                    }
                    break;
                case 202: // Sumit 21_jan_22 For Single Activity Log
                    {
                        ArrReturn = AllReply.ActivityLogReplyRecNewForRequestor.ToByteArray();
                    }
                    break;
                case 17:
                    {
                        ArrReturn = AllReply.OrderPlaceBrokerReplyRecord.ToByteArray();
                    } break;
                case 171:    // sumitp 28_jan_22
                    {
                        ArrReturn = AllReply.EquityOrderPlaceBrokerReplyRecord_cover.ToByteArray();
                    }
                    break;
                case 18:
                    {
                        ArrReturn = AllReply.OrderModifyBrokerReplyRecord.ToByteArray();
                    } break;
                case 19:
                    {
                        ArrReturn = AllReply.OrderCancelBrokerReplyRecord.ToByteArray();
                    } break;
                case 25:
                case 26:
                    {
                        ArrReturn = AllReply.OrderConversion.ToByteArray();
                    }
                    break;
                case 27:
                case 28:
                case 29:
                    {
                        ArrReturn = AllReply.OrderExchReplyRecord.ToByteArray();
                    } break;
                #endregion
                case 246:
                    {
                        ArrReturn = AllReply.IISOrderTradeGeneral.ToByteArray();
                    } break;
                case 209:
                    {
                        ArrReturn = AllReply.TemplateMarginReport.ToByteArray();
                    } break;
                case 172:
                    {
                        ArrReturn = AllReply.TemplateMarginReport_Peak.ToByteArray();
                    }
                    break;
                case 210:
                    {
                        ArrReturn = AllReply.TemplateMarginReply.ToByteArray();
                    }
                    break;
                case 173:
                    {
                        ArrReturn = AllReply.PeakMarginReplyData.ToByteArray();
                    }
                    break;
                case 217:
                    {
                        ArrReturn = AllReply.TemplateMarginReport.ToByteArray();
                    } break;
                case 248:
                    {
                        ArrReturn = AllReply.SegmentReply.ToByteArray();

                    } break;
                case 250:   //Added on 10-Sep-2016 - Rohan
                    {
                        ArrReturn = AllReply.MTMClientsWithAllMarginsReplyInstrumentWise.ToByteArray();
                    }
                    break;
                case ConstantUnits.REP_ITS_TO_FE_EXERCISE_PLACE_REQ:
                case ConstantUnits.REP_ITS_TO_FE_EXERCISE_MODIFY_REQ:
                case ConstantUnits.REP_ITS_TO_FE_EXERCISE_CANCEL_REQ:
                    {
                        ArrReturn = AllReply.OptionExerciseBrokerReplyRec.ToByteArray();
                    }
                    break;

                case ConstantUnits.REP_OTS_TO_ITS_EXERCISE_PLACE_REPLY:
                case ConstantUnits.REP_OTS_TO_ITS_EXERCISE_MODIFY_REPLY:
                case ConstantUnits.REP_OTS_TO_ITS_EXERCISE_CANCEL_REPLY:
                          {
                              ArrReturn = AllReply.OptionExerciseExchReplyRec.ToByteArray();
                          }
                           break;
                case 226:
                    {
                        ArrReturn = AllReply.TradeReportReply.ToByteArray();
                    }
                    break;
                case 201:
                    {
                        ArrReturn = AllReply.OFSWatchListReply.ToByteArray();
                    }
                    break;
                case 122:
                    {
                        ArrReturn = AllReply.PlaceOFSReplyRec.ToByteArray();
                    }
                    break;
                case 123:
                    {
                        ArrReturn = AllReply.PlaceOFSReplyRec.ToByteArray();
                    }
                    break;
                case 124:
                    {
                        ArrReturn = AllReply.PlaceOFSReplyRec.ToByteArray();
                    }
                    break;
                case 126:
                    {
                        ArrReturn = AllReply.PlaceOFSReplyRec.ToByteArray();
                    }
                    break;
                case 127:
                    {
                        ArrReturn = AllReply.PlaceOFSReplyRec.ToByteArray();
                    }
                    break;
                case 128:
                    {
                        ArrReturn = AllReply.PlaceOFSReplyRec.ToByteArray();
                    }
                    break;
                case 125:
                    {
                        ArrReturn = AllReply.OFSOrderReportReply.ToByteArray();
                    }
                    break;
                case 235: ArrReturn = AllReply.SLBMOrderPlaceBrokerReplyRec.ToByteArray(); break; //30.11.18 SLBM Order Confirmation Broker Reply
                case 236: ArrReturn = AllReply.SLBMOrderModifyBrokerReplyRec.ToByteArray(); break; //30.11.18 SLBM Order Confirmation Broker Reply
                case 237: ArrReturn = AllReply.SLBMOrderCancelBrokerReplyRec.ToByteArray(); break; //30.11.18 SLBM Order Confirmation Broker Reply
                case 238:
                case 239: //03.12.18 SLBM Order Confirmation Exchange Reply
                case 240:
                //case 243: ArrReturn = AllReply.SLBMOrderExchReplyRec.ToByteArray(); break;
                case 255: ArrReturn = AllReply.SLBMHoldingsReportReplyRec.ToByteArray(); break; //30.11.18 SLBM Holding Report
               
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
            return ArrReturn;
        }

        public static TAllReplyNewRecord ToAllReplyStruct(this byte[] buffToReadBody, byte ReplyType)
        {
            TAllReplyNewRecord AllReply = new TAllReplyNewRecord();

            switch (ReplyType)
            {
                case 1:
                    {
                        AllReply.LoginReply = buffToReadBody.ToStructure<TLoginReplyRecord>();
                    }
                    break;
                case 2:
                    {
                        AllReply.ChgPswdReplyRec = buffToReadBody.ToStructure<TChangePasswordReplyRecord>();

                    } break;
                case 3:
                    {
                        AllReply.LoginRepyNew = buffToReadBody.ToStructure<TLoginReplyRecordNew>();
                    } break;
                case 85:
                    {
                        AllReply.LoginTwoFactorAuthRecord = buffToReadBody.ToStructure<TLoginTwoFactorAuthRecord>();
                        if (ITS.Form1.logEnable2FA)
                            ITS.Form1.Log2FA(AllReply.LoginTwoFactorAuthRecord.toJsonString());
                    }
                    break;
                case 5:
                case 6:
                    {
                        AllReply.TraderListReplyRec = buffToReadBody.ToStructure<TTraderListReplyRecord>();
                    } break;
                case 7:
                    {
                        AllReply.OrdersReportDealerReply = buffToReadBody.ToStructure<TOrderTypeReportsReplyRecDealer>();
                    } break;
                case 8:
                    {
                        AllReply.TradesReportDealerReply = buffToReadBody.ToStructure<TTradeReportReplyRecDealer>();
                    } break;
                case 9:
                    {
                        AllReply.LogoutUserRec = buffToReadBody.ToStructure<TLogoutUserRecord>();

                    } break;
                case 10:
                    {
                        AllReply.EquityOrderPlaceBrokerReplyRec = buffToReadBody.ToStructure<TEquityOrderPlaceBrokerReplyRecord>();
                    } break;
                case 11:
                case 21:
                case 31:
                case 33:
                    {
                        AllReply.EquityOrderExchReplyRec = buffToReadBody.ToStructure<TEquityOrderExchReplyRecord>();
                    } break;
                case 16:
                    {
                        AllReply.AHOrderRejectedRecord = buffToReadBody.ToStructure<TAHOrderRejectedRecord>();
                    } break;
                case 20:
                    {
                        AllReply.EquityOrderModifyBrokerReplyRec = buffToReadBody.ToStructure<TEquityOrderModifyBrokerReplyRecord>();
                    } break;
                case 30:
                    {
                        AllReply.EquityOrderCancelBrokerReplyRec = buffToReadBody.ToStructure<TEquityOrderCancelBrokerReplyRecord>();
                    } break;
                case 32:
                    {
                        AllReply.KillOrderExchReplyRec = buffToReadBody.ToStructure<TKillOrderExchReplyRecord>();
                    } break;
                case 35:
                case 34:
                    {
                        AllReply.MFSIPRegistrationBrokerResRec = buffToReadBody.ToStructure<TBseMFSIPRegistrationMsgBrokerReply>();
                    }break;
                case 187:
                    {
                        AllReply.XSIPMandateBrokerReply = buffToReadBody.ToStructure<XSIPMandateBrokerReply>();
                    } break;
                case 39:
                    {
                        AllReply.MFSIPRegistrationBrokerResRec = buffToReadBody.ToStructure<TBseMFSIPRegistrationMsgBrokerReply>();
                    }
                    break;
                case 37:
                case 36:
                    {
                        AllReply.MFSIPRegistrationExchangeResRec = buffToReadBody.ToStructure<TBseMFSIPRegistrationExchangeReplyRecord>();
                    }break;
                case 188:
                    {
                        AllReply.XSIPMandateExchangeReplyRec = buffToReadBody.ToStructure<XSIPMandateExchangeReply>();
                
                } break;
                case 41:
                    {
                        AllReply.ScripMarginReplyRec = buffToReadBody.ToStructure<TScripMarginReplyRecord>();
                    } break;
                //2015.03.26 Add reply case 42
                case 42:
                    {
                        AllReply.ScripInfoReplyRec = buffToReadBody.ToStructure<TScripInfoReplyRecord>();
                    } break;
                //2015.03.26 end rply case 42
                case 43://BseMf Krutika
                case 44:
                case 45:
                    {
                        AllReply.MFAddModifyCancelBrokerResRec = buffToReadBody.ToStructure<TBseMFAddModifyCancelBrokerResponse>();
                    } break;
                case 46:
                case 47:
                case 48:
                    {
                        AllReply.MFAddModifyCancelExchangeResRec = buffToReadBody.ToStructure<TBseMFAddModifyCancelExchangeOrderReply>();
                    } break;
                case 49:
                    {
                        AllReply.NewScripMarginReplyRec = buffToReadBody.ToStructure<TScripMarginReplyRecord_New>();
                    } break;
                case 50:
                    {
                        AllReply.ExchTradeConfirmationRec = buffToReadBody.ToStructure<TExchTradeConfirmationRecord>();
                    } break;
                case 51:
                    {
                        AllReply.SLTriggerRec = buffToReadBody.ToStructure<TSLTriggerRecord>();
                    } break;
                //case 52:
                //    {
                //        AllReply.MarketToLimitRecBSE = buffToReadBody.ToStructure<TMarketToLimitRecord>();
                //    } break;
                case 53:
                    {
                        AllReply.MarketToLimitRecNSE = buffToReadBody.ToStructure<TMarketToLimitRecord>();
                    } break;
                case 54:
                    {
                        AllReply.ExchangeStatusRec = buffToReadBody.ToStructure<TExchangeStatusRecord>();
                    } break;
                case 56://McxTrading 
                    {
                        AllReply.McxGrpHalt = buffToReadBody.ToStructure<TMCXGroupLevelHaltInfo_OTS>();
                    }
                    break;
                case 59: //21.10.13
                    {
                        AllReply.OrderReportForSurviellenceReplyRec = buffToReadBody.ToStructure<TOrderReportForSurviellenceReplyRecord>();
                    }
                    break;
                //case 61:
                //    {
                //        AllReply.TradeReportReplyRec = buffToReadBody.ToStructure<TTradeReportReplyRecord>();
                //    }
                //    break;
                //case 62:
                //    {
                //        AllReply.OrderReportReplyRec = buffToReadBody.ToStructure<TOrderReportReplyRecord>();
                //    }
                //    break;
                case 63:
                    {
                        AllReply.ExerciseReportReplyRec = buffToReadBody.ToStructure<TExerciseReportReplyRecord>();
                    } break;
                //case 64:
                //    {
                //        AllReply.ActivityLogReplyRec = buffToReadBody.ToStructure<TActivityLogReplyRecord>();
                //    }
                //    break;
                case 65:
                    {
                        AllReply.AHOrderReportReplyRec = buffToReadBody.ToStructure<TAHOrderReportReplyRecord>();
                    } break;
                case 66:
                    {
                        AllReply.LicenceInfoReplyRec = buffToReadBody.ToStructure<TLicenceInfoReplyRecord>();
                    } break;
                case 68:
                    {
                        AllReply.ClientDbNumReplyRec = buffToReadBody.ToStructure<TClientDbNumReportReplyRecord>();
                    } break;
                case 69:
                    {
                        AllReply.HoldingsReportReplyRec = buffToReadBody.ToStructure<THoldingsReportReplyRecord>();
                    }
                    break;
                case 70:
                    {
                        AllReply.HoldingsValuationReportReplyRec = buffToReadBody.ToStructure<THoldingsValuationReportRecord>();
                    }
                    break;
                case 218:
                    {
                        AllReply.HoldingsValuationReportReplyRec = buffToReadBody.ToStructure<THoldingsValuationReportRecord>();
                    }
                    break;
                case 170:
                    {
                        AllReply.HoldingsValuationReportWithMemVarReplyRec = buffToReadBody.ToStructure<THoldingsValuationReportWithMemVar>();
                    }
                    break;
                case 71:
                case 142:
                    {
                        AllReply.TradeReportReplyRecNew = buffToReadBody.ToStructure<TTradeReportReplyRecordNew>();
                    }
                    break;
                case 58:    // sumitp 28_jan_22
                    {
                        AllReply.OrderTypeTradeReportReplyRecNewCover = buffToReadBody.ToStructure<TOrderTypeTradeReportReplyRecNewCover>();
                    }
                    break;
                case 61:    //Added on 18-Apr-2018 - Rohan
                    {
                        AllReply.TradeReportReplyRecNewSpread = buffToReadBody.ToStructure<TTradeReportReplyReplyRecNew>();
                    }
                    break;
                case 72:
                case 141:
                    {
                        AllReply.OrderReportReplyRecNew = buffToReadBody.ToStructure<TOrderTypeReportsReplyRec>();
                    }
                    break;
                case 62:    //Added on 18-Apr-2018 - Rohan
                    {
                        AllReply.OrderReportReplyRecNewSpread = buffToReadBody.ToStructure<TOrderTypeReportsReplyRecNew>();
                    }
                    break;
                case 78:    //Added on 27-Nov-2021 - Himanshu // sumitp 28_jan_22
                    {
                        AllReply.OrderReportReplyRecCoverOrBracket = buffToReadBody.ToStructure<TOrderReportReplyRecordCoverOrBracket>();
                    }
                    break;
                case 52:    //Added on 18-Apr-2018 - Rohan
                    {
                        AllReply.OrderReportReplyRecNewSpreadWithRejOrder = buffToReadBody.ToStructure<TOrderTypeReportsReplyRecNewWithRejetOrder>();
                    }
                    break;
                case 64: //Added on 18-Apr-2018 - Rohan
                    {
                        AllReply.ActivityLogReplyRecForRequestorNewSpread = buffToReadBody.ToStructure<TActivityLogReplyRecordNewForRequestorNew>();
                    }
                    break;
                case 74:
                    {
                        AllReply.HoldingsReportWithCollateralReplyRec = buffToReadBody.ToStructure<THoldingsReportReplyRecord_WithCollatral>();
                    }
                    break;
                case 75:
                    {
                        AllReply.AllClientsHoldingsReportReplyRec = buffToReadBody.ToStructure<THoldingsReportReplyRecordNew>();
                    } break;
                case 76:    //Added on 08-Nov-2016 - Rohan
                    {
                        AllReply.AllClientsHoldingsEquityReportReplyRec = buffToReadBody.ToStructure<THoldingsEquityReportReplyRecordNew>();
                    } break;
                case 79:
                    {
                        AllReply.ClientPositionReplyRec = buffToReadBody.ToStructure<TClientPositionReportReplyRecord>();
                    } break;
                case 80:
                    {
                        AllReply.TradeToTradeReplyRec = buffToReadBody.ToStructure<TTradeToTradeReplyRecord>();

                    } break;
                case 81:
                    {
                        AllReply.BseNseMapReplyRec = buffToReadBody.ToStructure<TBseNseMapRecord>();

                    } break;
                case 82:
                    {
                        AllReply.DerivBannedScripReplyRec = buffToReadBody.ToStructure<TDerivBannedScripReplyRecord>();

                    } break;
                case 83:
                    {
                        AllReply.AdhocReportReplyRec = buffToReadBody.ToStructure<TAdhocReportReplyRecord>();
                    } break;
                case 84:
                    {
                        AllReply.AdhocModifyReplyRec = buffToReadBody.ToStructure<TAdhocModifyReplyRecord>();
                    } break;
                case 86:
                    {
                        AllReply.MFClientsReportReplyRec = buffToReadBody.ToStructure<TMaalFodoClientsReportReplyRecord>();
                    } break;
                case 89:
                    {
                        AllReply.IlliquidReplyRec = buffToReadBody.ToStructure<TIlliquidReplyRecord>();

                    } break;
                case 92://BseMf Krutika
                    {
                        AllReply.HoldingsReportOfMFReplyRec = buffToReadBody.ToStructure<THoldingsReportReplyRecord_MF>();
                    }
                    break;
                case ConstantUnits.REP_ITS_TO_FE_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        AllReply.BseMfOrderReportReplyRecNew = buffToReadBody.ToStructure<TBseMFOrderTypeReportsReplyRec>();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_RPT_ACTIVITY_BSEMF://BseMf Krutika
                    {
                        AllReply.BseMfActivitylogReplyRecNew = buffToReadBody.ToStructure<TBseMFActivitylogReplyRec>();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_SIP_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        AllReply.BseMfSIPOrderReportReplyRecNew = buffToReadBody.ToStructure<TBseMFSIPOrderTypeReportsReplyRec>();
                    } break;
                case ConstantUnits.REP_ITS_TO_FE_MANDATE_RPT_ORDERS_BSEMF://BseMf Krutika
                    {
                        AllReply.MandateOrderTypeReportsReplyRecNew = buffToReadBody.ToStructure<MandateOrderTypeReportsReplyRec>();
                    } break;
                case 94://McxTrading
                    {
                        AllReply.HoldingsReportWithCollatralReplyRecNew = buffToReadBody.ToStructure<THoldingsReportReplyRecord_WithCollatralNew>();
                    } break;
                case 67: //MTF
                     {
                        AllReply.HoldingsReportWithMTF = buffToReadBody.ToStructure<THoldingsReportPreCloseReplyRecord_MTF>();
                    } break;
                case 90: //MTF
                    {
                        AllReply.HoldingsReportWithMTFNew = buffToReadBody.ToStructure<THoldingsReportPreCloseReplyRecord_MTFNew>();
                    }
                    break;
                case 194: //new holding report 
                    {
                        AllReply.ISCL_HoldingEquityReply = buffToReadBody.ToStructure<IntellectSharedClassLibrary.ISCL_HoldingEquityReply>();
                        //AllReply.ISCL_HoldingEquityReply = buffToReadBody.ToStructure<ISCL_HoldingEquityReply>();
                    }
                    break;
                case 106:// 106 jinal 
                    {
                        AllReply.HoldingsReportWithCollatralPreCloseReplyRecNew = buffToReadBody.ToStructure<THoldingsReportPreCloseReplyRecord_WithCollatralNew>();
                    } break;
                case ConstantUnits.REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL://31.01.17
                    {
                        AllReply.AllClientsHoldingsReportReplyRecNew = buffToReadBody.ToStructure<TAllClientsHoldingsReportReplyRecord>();
                    } break;


                #region LOtQty
                case 91:
                    {
                        AllReply.LotQtyOtsReplyRec = buffToReadBody.ToStructure<TLotQuantityRecord>();

                    } break;
                #endregion
                case ConstantUnits.REQ_REP_ITS_TO_FE_ALLCLIENTS_NETOBL_New://31.01.17
                    {
                        AllReply.AllClientsHoldingsReportReplyRecWithPreClose = buffToReadBody.ToStructure<TAllClientsHoldingsReportReplyRecordWithPreClose>();
                    } break;
                case 110:
                    {
                        AllReply.DerivOrderPlaceBrokerReplyRec = buffToReadBody.ToStructure<TDerivOrderPlaceBrokerReplyRecord>();

                    } break;
                case 111:
                case 121:
                case 131:
                case 133:
                    {
                        AllReply.DerivOrderExchReplyRec = buffToReadBody.ToStructure<TDerivOrderExchReplyRecord>();
                    } break;
                case 120:
                    {
                        AllReply.DerivOrderModifyBrokerReplyRec = buffToReadBody.ToStructure<TDerivOrderModifyBrokerReplyRecord>();

                    } break;
                case 130:
                    {
                        AllReply.DerivOrderCancelBrokerReplyRec = buffToReadBody.ToStructure<TDerivOrderCancelBrokerReplyRecord>();

                    } break;
                case 150:
                    {
                        AllReply.DerivExchTradeConfirmationRec = buffToReadBody.ToStructure<TDerivExchTradeConfirmationRecord>();
                    } break;
                case 151:
                    {
                        AllReply.DerivSLTriggerRec = buffToReadBody.ToStructure<TDerivSLTriggerRecord>();
                    } break;
                case 191:
                    {
                        AllReply.SpdOrderPlaceBrokerReplyRec = buffToReadBody.ToStructure<TSpreadOrderPlaceBrokerReplyRecord>();
                    } break;
                case 192:
                case 193:
                    {
                        AllReply.SpdOrderCancelBrokerReplyRec = buffToReadBody.ToStructure<TSpreadOrderCancelBrokerReplyRecord>();
                    } break;
                //case 199:
                //    {                        
                //    }break;

                case 200:
                    {
                        AllReply.MarginReportReplyRec = buffToReadBody.ToStructure<TMarginReportReplyRecord>();
                    }
                    break;
                case 206:
                    {
                        AllReply.MarginReportReplyRecMCX = buffToReadBody.ToStructure<TMarginReportReplyRecord>();
                    }
                    break;
                case 208:
                    {
                        AllReply.MarginReportReplyRecCombined = buffToReadBody.ToStructure<TMarginReportReplyRecordCombined>();
                    }
                    break;
                #region MTM - Added on 12-Sep-2016 - Rohan
                case 203:
                    {
                        AllReply.ClientsMTMReply = buffToReadBody.ToStructure<UpdateClientsReply>();
                    }
                    break;
                //case 208:   //Added on 08-Mar-2017 - Rohan
                //    {
                //        AllReply.ClientsMTMReplyMcx = buffToReadBody.ToStructure<UpdateClientsReply>();
                //    }
                //    break;
                case 205:   //Added on 10-Sep-2016 - Rohan
                    {
                        AllReply.MTMClientsWithAllMarginsReply = buffToReadBody.ToStructure<MTMClientsWithAllMargins>();
                    }
                    break;
                case 207:   //Added on 08-Mar-2017 - Rohan
                    {
                        AllReply.MTMClientsWithAllMarginsReplyMcx = buffToReadBody.ToStructure<MTMClientsWithAllMargins>();
                    }
                    break;
                #endregion
                case 230:
                    {
                        AllReply.ClearSentToExchRec = buffToReadBody.ToStructure<TClearSentToExchRecord>();
                    } break;
                case 241://SLBM
                case 242:
                case 243://ROMIL SLBM test
                case 249:
                    {
                        AllReply.SpdOrderExchReplyRec = buffToReadBody.ToStructure<TSpreadOrderExchReplyRecord>();
                    } break;
                case 251:
                    {
                        AllReply.LinkFailRec = buffToReadBody.ToStructure<TLinkFailedRecord>();
                    } break;
                case 253:
                    {
                        AllReply.HandShakeRec = buffToReadBody.ToStructure<TDummyRecord1>();
                    } break;

                #region For Disclosed Qty
                case 212:
                case 222:
                case 232:
                case 233:
                    {
                        AllReply.CurrDerivOrderExchReplyRec = buffToReadBody.ToStructure<TCurrDerivOrderExchReplyRecord>();
                    } break;
                case 211:
                    {
                        AllReply.CurrDerivOrderPlaceBrokerReplyRec = buffToReadBody.ToStructure<TCurrDerivOrderPlaceBrokerReplyRecord>();
                    }
                    break;
                case 221:
                    {
                        AllReply.CurrDerivOrderModifyBrokerReplyRec = buffToReadBody.ToStructure<TCurrDerivOrderModifyBrokerReplyRecord>();
                    }
                    break;
                case 231:
                    {
                        AllReply.CurrDerivOrderCancelBrokerReplyRec = buffToReadBody.ToStructure<TCurrDerivOrderCancelBrokerReplyRecord>();
                    }
                    break;
                #endregion

                #region OrderTypes
                case 17:
                    {
                        AllReply.OrderPlaceBrokerReplyRecord = buffToReadBody.ToStructure<TOrderPlaceBrokerReplyRecord>();
                    }
                    break;
                case 171:   // sumitp 28_jan_22
                    {
                        if (AllReply.OrderPlaceBrokerReplyRecord_Cover.ExchType == 'D')
                        {
                            AllReply.OrderPlaceBrokerReplyRecord_Cover = buffToReadBody.ToStructure<TOrderPlaceBrokerReplyRecord_Cover>();

                            //AllReply.OrderPlaceBrokerReplyRecordCoverAndBracket = buffToReadBody.ToStructure<TOrderPlaceBrokerReplyRecordCoverAndBracket>();
                        }
                        else
                        {
                            AllReply.EquityOrderPlaceBrokerReplyRecord_cover = buffToReadBody.ToStructure<TEquityOrderPlaceBrokerReplyRecord_cover>();

                            //AllReply.OrderPlaceBrokerReplyRecordCoverAndBracket = buffToReadBody.ToStructure<TOrderPlaceBrokerReplyRecordCoverAndBracket>();
                        }

                    }
                    break;
                case 18:
                    {
                        AllReply.OrderModifyBrokerReplyRecord = buffToReadBody.ToStructure<TOrderModifyBrokerReplyRecord>();
                    }
                    break;
                case 19:
                    {
                        AllReply.OrderCancelBrokerReplyRecord = buffToReadBody.ToStructure<TOrderCancelBrokerReplyRecord>();
                    }
                    break;
                case 25:
                case 26:
                    {
                        AllReply.OrderConversion = buffToReadBody.ToStructure<TOrderConversionRecReply>();
                    }
                    break;
                case 27:
                case 28:
                case 29:
                    {
                        AllReply.OrderExchReplyRecord = buffToReadBody.ToStructure<TOrderExchReplyRecord>();
                    }
                    break;
                case 104:
                    {
                        AllReply.ActivityLogReplyRecNew = buffToReadBody.ToStructure<TActivityLogReplyRecordNew>();
                    }
                    break;
                case 204: //15.10.16 Hetal
                    {
                        AllReply.ActivityLogReplyRecNewForRequestor = buffToReadBody.ToStructure<TActivityLogReplyRecordNewForRequestor>();
                    }
                    break;
                case 202: // Sumit 21_jan_22 For Single Activity Log
                    {
                        AllReply.ActivityLogReplyRecNewForRequestor = buffToReadBody.ToStructure<TActivityLogReplyRecordNewForRequestor>();
                    }
                    break;
                #endregion
                case 246:
                    {
                        AllReply.IISOrderTradeGeneral = buffToReadBody.ToStructure<TOrderTradeGeneral>();
                    } break;
                case 209:
                    {
                        AllReply.TemplateMarginReport = buffToReadBody.ToStructure<TTemplateMarginReplyRecord>();
                    } break;
                case 172:
                    {
                        AllReply.TemplateMarginReport_Peak = buffToReadBody.ToStructure<TTemplateMarginReplyRecord_Peak>();
                    }
                    break;
                case 210: //Derivative Margin 210 20.10.22
                    {
                        AllReply.TemplateMarginReply = buffToReadBody.ToStructure<TMarginReportReplyRecordTemplateWithDerivMargin>();
                    }
                    break;
                case 173:
                    {
                        AllReply.PeakMarginReplyData = buffToReadBody.ToStructure<PeakMarginReplyData>();
                    }
                    break;
                case 217:
                    {
                        AllReply.TemplateMarginReport = buffToReadBody.ToStructure<TTemplateMarginReplyRecord>();
                    } break;
                case 248:
                    {
                        AllReply.SegmentReply = buffToReadBody.ToStructure<TAllowSegmentReply>();

                    } break;
                case 250:   //Added on 10-Sep-2016 - Rohan
                    {
                        AllReply.MTMClientsWithAllMarginsReplyInstrumentWise = buffToReadBody.ToStructure<MTMClientsWithAllMarginsInstrumentWise>();
                    }
                    break;
                    //Options Exercise 03.08.17
                case ConstantUnits.REP_ITS_TO_FE_EXERCISE_PLACE_REQ:
                case ConstantUnits.REP_ITS_TO_FE_EXERCISE_MODIFY_REQ:
                case ConstantUnits.REP_ITS_TO_FE_EXERCISE_CANCEL_REQ:
                    {
                        AllReply.OptionExerciseBrokerReplyRec = buffToReadBody.ToStructure<TOptionExerciseBrokerReplyRecord>();
                    }
                    break;
                case ConstantUnits.REP_OTS_TO_ITS_EXERCISE_PLACE_REPLY:
                case ConstantUnits.REP_OTS_TO_ITS_EXERCISE_MODIFY_REPLY:
                case ConstantUnits.REP_OTS_TO_ITS_EXERCISE_CANCEL_REPLY:
                    {
                        AllReply.OptionExerciseExchReplyRec = buffToReadBody.ToStructure<TOptionExerciseExchReplyRecord>();
                    }
                    break;
                case 226:
                    {
                        AllReply.TradeReportReply = buffToReadBody.ToStructure<TradeExchReportReply>();
                    }
                    break;
                case 201:
                    {
                        AllReply.OFSWatchListReply = buffToReadBody.ToStructure<OFSWatchListReply>();
                    }
                    break;
                case 122:
                    {
                        AllReply.PlaceOFSReplyRec = buffToReadBody.ToStructure<TOrderPlaceModifyCancelReplyRecordOFS>();
                    }
                    break;
                case 123:
                    {
                        AllReply.PlaceOFSReplyRec = buffToReadBody.ToStructure<TOrderPlaceModifyCancelReplyRecordOFS>();
                    }
                    break;
                case 124:
                    {
                        AllReply.PlaceOFSReplyRec = buffToReadBody.ToStructure<TOrderPlaceModifyCancelReplyRecordOFS>();
                    }
                    break;
                case 126:
                    {
                        AllReply.PlaceOFSReplyRec = buffToReadBody.ToStructure<TOrderPlaceModifyCancelReplyRecordOFS>();
                    }
                    break;
                case 127:
                    {
                        AllReply.PlaceOFSReplyRec = buffToReadBody.ToStructure<TOrderPlaceModifyCancelReplyRecordOFS>();
                    }
                    break;
                case 128:
                    {
                        AllReply.PlaceOFSReplyRec = buffToReadBody.ToStructure<TOrderPlaceModifyCancelReplyRecordOFS>();
                    }
                    break;
                case 125:
                    {
                        AllReply.OFSOrderReportReply = buffToReadBody.ToStructure<TOrderTypeReportsReplyRecOFS>();
                    }
                    break;
                case 235: //21.11.18 SLBM Order Confirmation Broker Reply
                    {
                        AllReply.SLBMOrderPlaceBrokerReplyRec = buffToReadBody.ToStructure<TSLBMOrderPlaceBrokerReplyRecord>();

                    }
                    break;
                case 236: //21.11.18 SLBM Order Confirmation Broker Reply
                    {
                        AllReply.SLBMOrderModifyBrokerReplyRec = buffToReadBody.ToStructure<TSLBMOrderModifyBrokerReplyRecord>();

                    }
                    break;
                case 237: //21.11.18 SLBM Order Confirmation Broker Reply
                    {
                        AllReply.SLBMOrderCancelBrokerReplyRec = buffToReadBody.ToStructure<TSLBMOrderCancelBrokerReplyRecord>();

                    }
                    break;
                case 238: //03.11.18 SLBM Order Confirmation Exch Reply
                case 239:
                case 240:
                //case 243:
                //    {
                //        AllReply.SLBMOrderExchReplyRec = buffToReadBody.ToStructure<TSLBMOrderExchReplyRecord>();
                //    }
                //    break;
                case 255:
                    {
                        AllReply.SLBMHoldingsReportReplyRec = buffToReadBody.ToStructure<TSLBMHoldingsReportReplyRecord>();
                    }
                    break;
                //
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
            return AllReply;
        }



        public static T ReadStruct<T>(this FileStream fs)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return temp;
        }
    }

    /// <summary>
    /// Logger is used for creating a customized error log files or an error can be registered as
    /// a log entry in the Windows Event Log on the administrator's machine.
    /// </summary>
    public class ErrorLog
    {
        public static List<string> Error = new List<string>();
        protected static string strLogFilePath = string.Empty;
        private static StreamWriter sw = null;
        /// <summary>
        /// Setting LogFile path. If the logfile path is null then it will update error info into LogFile.txt under
        /// application directory.
        /// </summary>
        public static string LogFilePath
        {
            set
            {
                strLogFilePath = value;
            }
            get
            {
                return strLogFilePath;
            }
        }
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public ErrorLog() { }
        /// <summary>
        /// Write error log entry for window event if the bLogType is true. Otherwise, write the log entry to
        /// customized text-based text file
        /// </summary>
        /// <param name="bLogType"></param>
        /// <param name="objException"></param>
        /// <returns>false if the problem persists</returns>
        public static bool ErrorRoutine(bool bLogType, Exception objException)
        {
            try
            {
                //Check whether logging is enabled or not
                bool bLoggingEnabled = false;
                bLoggingEnabled = CheckLoggingEnabled();

                //Don't process more if the logging 
                if (false == bLoggingEnabled)
                    return true;

                //Write to Windows event log
                if (true == bLogType)
                {
                    string EventLogName = "ErrorSample";

                    if (!EventLog.SourceExists(EventLogName))
                        EventLog.CreateEventSource(objException.Message, EventLogName);

                    // Inserting into event log
                    EventLog Log = new EventLog();
                    Log.Source = EventLogName;
                    Log.WriteEntry(objException.Message, EventLogEntryType.Error);
                }
                //Custom text-based event log
                else
                {
                    if (true != CustomErrorRoutine(objException))
                        return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Check Logginstatus config file is exist. If exist read the value set the loggig status
        /// </summary>
        private static bool CheckLoggingEnabled()
        {
            string strLoggingStatusConfig = string.Empty;

            strLoggingStatusConfig = GetLoggingStatusConfigFileName();

            //If it's empty then enable the logging status 
            if (strLoggingStatusConfig.Equals(string.Empty))
            {
                return true;
            }
            else
            {

                //Read the value from xml and set the logging status
                bool bTemp = GetValueFromXml(strLoggingStatusConfig);
                return bTemp;
            }
        }
        /// <summary>
        /// Check the Logginstatus config under debug or release folder. If not exist, check under 
        /// project folder. If not exist again, return empty string
        /// </summary>
        /// <returns>empty string if file not exists</returns>
        private static string GetLoggingStatusConfigFileName()
        {
            string strCheckinBaseDirecotry = AppDomain.CurrentDomain.BaseDirectory + "LoggingStatus.Config";

            if (File.Exists(strCheckinBaseDirecotry))
                return strCheckinBaseDirecotry;
            else
            {
                string strCheckinApplicationDirecotry = GetApplicationPath() + "LoggingStatus.Config";

                if (File.Exists(strCheckinApplicationDirecotry))
                    return strCheckinApplicationDirecotry;
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Read the xml file and getthe logging status
        /// </summary>
        /// <param name="strXmlPath"></param>
        /// <returns></returns>
        private static bool GetValueFromXml(string strXmlPath)
        {
            try
            {
                //Open a FileStream on the Xml file
                FileStream docIn = new FileStream(strXmlPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                XmlDocument contactDoc = new XmlDocument();
                //Load the Xml Document
                contactDoc.Load(docIn);

                //Get a node
                XmlNodeList UserList = contactDoc.GetElementsByTagName("LoggingEnabled");

                //get the value
                string strGetValue = UserList.Item(0).InnerText.ToString();

                if (strGetValue.Equals("0"))
                    return false;
                else if (strGetValue.Equals("1"))
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// If the LogFile path is empty then, it will write the log entry to LogFile.txt under application directory.
        /// If the LogFile.txt is not availble it will create it
        /// If the Log File path is not empty but the file is not availble it will create it.
        /// </summary>
        /// <param name="objException"></param>
        /// <returns>false if the problem persists</returns>
        private static bool CustomErrorRoutine(Exception objException)
        {
            string strPathName = string.Empty;
            if (strLogFilePath.Equals(string.Empty))
            {
                //Get Default log file path "LogFile.txt"
                strPathName = GetLogFilePath();
            }
            else
            {

                //If the log file path is not empty but the file is not available it will create it
                if (false == File.Exists(strLogFilePath))
                {
                    if (false == CheckDirectory(strLogFilePath))
                        return false;

                    FileStream fs = new FileStream(strLogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }
                strPathName = strLogFilePath;

            }

            bool bReturn = true;
            // write the error log to that text file
            if (true != WriteErrorLog(strPathName, objException))
            {
                bReturn = false;
            }
            return bReturn;
        }
        /// <summary>
        /// Write Source,method,date,time,computer,error and stack trace information to the text file
        /// </summary>
        /// <param name="strPathName"></param>
        /// <param name="objException"></param>
        /// <returns>false if the problem persists</returns>
        private static bool WriteErrorLog(string strPathName, Exception objException)
        {
            bool bReturn = false;
            string strException = string.Empty;
            string Er = objException.Message.ToString();
            string method = objException.TargetSite.Name.ToString();
            string Formname = objException.StackTrace.ToString();
            int lastindex = Formname.LastIndexOf('\\');
            string frmName = Formname.Substring(lastindex + 1);
            string Insert = method + "," + frmName + "," + Er;
            if (Error.Contains(Insert)) return false;
            Error.Add(Insert);
            try
            {
                SendReport(objException);
                sw = new StreamWriter(strPathName, true);
                sw.WriteLine("Source		: " + objException.Source.ToString().Trim());
                sw.WriteLine("Method		: " + objException.TargetSite.Name.ToString());
                sw.WriteLine("Date		: " + DateTime.Now.ToLongTimeString());
                sw.WriteLine("Time		: " + DateTime.Now.ToShortDateString());
                sw.WriteLine("Computer	: " + Dns.GetHostName().ToString());
                sw.WriteLine("Error		: " + objException.Message.ToString().Trim());
                sw.WriteLine("Stack Trace	: " + objException.StackTrace.ToString().Trim());
                sw.WriteLine("^^-------------------------------------------------------------------^^");
                sw.Flush();
                sw.Close();
                bReturn = true;
            }
            catch (Exception)
            {
                bReturn = false;
            }
            return bReturn;
        }
        public static void SendReport(Exception exception)
        {
            //var reportCrash = new ReportCrash("Email where you want to receive crash reports.")

            var reportCrash = new ReportCrash("errorrpt@intellectsoftwares.com")
            {

                DeveloperMessage = "Version:"+ITS.Form1.Vname +"___"+ DateTime.Now.ToString("yyyy/MM/dd    HH:mm:ss"),
                Silent = true,
                EnableSSL = false,
                CaptureScreen = true,
                Port = 587,
                EmailRequired = true,
                Exception = exception,
                ToEmail = "errorrpt@intellectsoftwares.com",
                FromEmail = "errorrpt@intellectsoftwares.com",
                UserName = "errorrpt@intellectsoftwares.com",
                Password = "intellect1!",
                SmtpHost = "mail.intellectsoftwares.com",
                IncludeScreenshot = true,
                AnalyzeWithDoctorDump = false,

            };


            reportCrash.Send(exception);

        }


        /// <summary>
        /// Check the log file in applcation directory. If it is not available, creae it
        /// </summary>
        /// <returns>Log file path</returns>
        private static string GetLogFilePath()
        {
            try
            {
                // get the base directory
                string baseDir = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.RelativeSearchPath;

                // search the file below the current directory
                string retFilePath = baseDir + "//" + "LogFile.txt";

                // if exists, return the path
                if (File.Exists(retFilePath) == true)
                    return retFilePath;
                //create a text file
                else
                {
                    if (false == CheckDirectory(retFilePath))
                        return string.Empty;

                    FileStream fs = new FileStream(retFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }

                return retFilePath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Create a directory if not exists
        /// </summary>
        /// <param name="strLogPath"></param>
        /// <returns></returns>
        private static bool CheckDirectory(string strLogPath)
        {
            try
            {
                int nFindSlashPos = strLogPath.Trim().LastIndexOf("\\");
                string strDirectoryname = strLogPath.Trim().Substring(0, nFindSlashPos);

                if (false == Directory.Exists(strDirectoryname))
                    Directory.CreateDirectory(strDirectoryname);

                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        private static string GetApplicationPath()
        {
            try
            {
                string strBaseDirectory = AppDomain.CurrentDomain.BaseDirectory.ToString();
                int nFirstSlashPos = strBaseDirectory.LastIndexOf("\\");
                string strTemp = string.Empty;

                if (0 < nFirstSlashPos)
                    strTemp = strBaseDirectory.Substring(0, nFirstSlashPos);

                int nSecondSlashPos = strTemp.LastIndexOf("\\");
                string strTempAppPath = string.Empty;
                if (0 < nSecondSlashPos)
                    strTempAppPath = strTemp.Substring(0, nSecondSlashPos);

                string strAppPath = strTempAppPath.Replace("bin", "");
                return strAppPath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    static class MemoryHelper
    {
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        [DllImportAttribute("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet =
       CharSet.Ansi, SetLastError = true)]

        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int
        maximumWorkingSetSize);

        /// <summary>
        /// clears the unwanted memory
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }

        public static void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

    }

    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <param name="INIPath"></param>
        public IniFile(string INIPath)
        {
            path = INIPath;
        }
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <param name="Section"></param>
        /// Section name
        /// <param name="Key"></param>
        /// Key Name
        /// <param name="Value"></param>
        /// Value Name
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();

        }
    }
}
