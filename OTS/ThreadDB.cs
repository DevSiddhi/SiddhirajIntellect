using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.Data;
using System.Windows.Forms;
using System.Windows.Threading;


namespace OTS
{
    public delegate void ThreadTerminationAction(ThreadDB db);
    public class ThreadDB
    {
        #region Variable Declaration
        public int DBNo, ThreadNumber, TimeNow, ExType;
        public long ThreadStartTick;
        string[] ConnectionString = new string[3];
        public Thread t;
        public OleDbConnection conn;
        Form1 frm;
        OleDbCommand cmd;
        public string ReplyStr, RqStr, ExStr, ExStr1;
        public ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();
        public ITInteractiveStructures.TOTSReplyRecord OTSReplyRec = new ITInteractiveStructures.TOTSReplyRecord();
        public ITInteractiveStructures.TAllRequestNewRecord AllReq = new ITInteractiveStructures.TAllRequestNewRecord();
        public ushort MaxDeadlockRetryCnt;
        string Result, ExceptionStr, RequestStr, RequestString = null;
        public char AfterHours;
        public bool DeadlockRetry;
        OleDbDataAdapter da;
        DataSet ds = new DataSet();
        DataTable dt = new DataTable();
        public char TraderChar;
        char[] TrimChars = new char[2] { ' ', '\0' };
        char[] splitchar = new char[1] { '|' };
        string[] VList;
        DateTime ExchStartDate = new DateTime(1980, 1, 1, 0, 0, 0);
        string[] ArrayOptionType = new string[5] { "XX", "CA", "PA", "CE", "PE" };
        public bool TimeOutFlag;
        int MFDebitAmt;
        string errorSting;


        #endregion

        public ThreadDB(Form1 form1)
        {
            frm = form1;
        }

        //---------------------------------

        public ThreadState Status
        {
            get
            {
                return t.ThreadState;
            }
        }

        //---------------------------------

        public void start()
        {
            t = new Thread(new ThreadStart(this.Execute));
            t.Start();

        }

        //---------------------------------

        public void Execute()
        {
            ConnectionString = File.ReadAllLines("OTSIntellectDB" + DBNo + ".udl");
            conn = new OleDbConnection(ConnectionString[2]);
            cmd = new OleDbCommand();
            cmd.Connection = conn;
            da = new OleDbDataAdapter(cmd);
            if (TimeOutFlag)
                cmd.CommandTimeout = 60;
            else
                cmd.CommandTimeout = 30;

            try
            {
                conn.Open();
            }
            catch (Exception oe)
            {
                string str1 = DateTime.Now.ToString("hh:mm:ss") + " : " + "Cannot Connect to database" + ", please check the " + "OTSIntellectDB" + DBNo + ".udl" + " file";
                frm.Invoke(frm.ConnectionToDatabase, str1);
            }

            if (RequestString == null)
            {
                t.Suspend();
            }

            while (this.Status != ThreadState.Suspended)
            {
                AfterHours = 'N';
                cmd.CommandText = "";
                ExType = 0;
                DeadlockRetry = false;
                ExceptionStr = "";
                RqStr = "";
                ExStr = "";
                ExStr1 = "";
                RequestStr = "";
                ReplyStr = "";
                try
                {
                    switch (OTSReqRec.ReqHdr.RequestType)
                    {
                        case 1:
                            {
                                cmd.CommandText = MakeCommandText();                            
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessLoginReply();
                            }
                            break;
                        case 2: //Password change
                            {
                                cmd.CommandText = MakeCommandText();                             
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessChangePasswordReply();
                            }
                            break;
                        case 3: // Login NewPointer.Exe C# for Currency
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessLoginReplyNew();
                            }
                            break;
                        case 4: // Login With Name
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessLoginReplyWithName();
                            }
                            break;
                        case 5://Trader List - Admin only
                            {
                                cmd.CommandText = MakeCommandText();                             
                                da.Fill(dt);
                                ProcessTraderList();
                            }
                            break;
                        case 7://Trader List - Admin only
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ProcessTraderList();
                            }
                            break;
                        case 8://Trader List - Admin only
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ProcessTraderList();
                            }
                            break;
                        case 6: //Trader List - Admin only
                            {
                                cmd.CommandText = MakeCommandText();                               
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessVerifyTrader();
                            }
                            break;
                        case 10://Place Equity Order
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessEquityPlaceOrderRequest();
                            }
                            break;
                        case 15: //Place Equity Order After Hours
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessAHEquityPlaceOrderRequest();
                            }
                            break;
                        case 20://Modify Equity Order
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessEquityModifyOrderRequest();
                            }
                            break;
                        case 30://Cancel Equity Order
                            {
                                cmd.CommandText = MakeCommandText();                             
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);//BrokerOrderID
                                ProcessEquityCancelOrderRequest();
                            }
                            break;
                        case 41:
                            {
                                cmd.CommandText = MakeCommandText();                               
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessScripMarginReply();
                            }
                            break;

                            // 2015.03.26
                        case Constants.OTS_SECURITY_INFO:
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessScripInfoReply();
                            }
                            break;
                        // 2015.03.26
                        case 61://Trade Report
                            {
                                cmd.CommandText = MakeCommandText();                             
                                da.Fill(dt);
                                ProcessTradeReport();
                            }
                            break;
                        case 62: //Order Report
                            {
                                cmd.CommandText = MakeCommandText();                            
                                da.Fill(dt);
                                ProcessOrderReport();
                            }
                            break;
                        case 78: //Order Report cover                   by himanshu    // sumitp 28_jan_22
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ProcessOrderReport_CoverOrBracket();

                            }
                            break;
                        case 58://Cover Trade Report       // sumitp 28_jan_22
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ProcessTradeReportCover();
                            }
                            break;
                        case 64://Activity Log
                            {
                                cmd.CommandText = MakeCommandText();                             
                                da.Fill(dt);
                                ProcessActivityLog();
                            }
                            break;
                        case 66://Licence Used Report
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ProcessLicUsedReport();
                            }
                            break;
                        case 68://Client,DbNum Report
                            {
                                cmd.CommandText = MakeCommandText();                             
                                da.Fill(dt);
                                ProcessCliDbNumReport();
                                TimeOutFlag = false;
                            }
                            break;
                        case 69:
                            {
                                cmd.CommandText = MakeCommandText();                               
                                da.Fill(dt);
                                ProcessHoldingsReport();
                            }
                            break;

                        case 70://Holdings Valuation Report
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ProcessHoldingsValuationReport();
                            }
                            break;
                        case 218://Holdings Valuation Report
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ProcessHoldingsValuationReport();
                            }
                            break;
                        case 73: //Data base Date
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ProcessDBDate();
                            }
                            break;
                        case 79: //Client Position Report
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ProcessClientPositionReport();
                            }
                            break;
                        case 80: //Trade To Trade  Report
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ProcessTradeToTradeReport();
                            }
                            break;
                        case 81: //Bse Nse Map Report
                            {
                                cmd.CommandText = MakeCommandText();                             
                                da.Fill(dt);
                                ProcessBseNseMapReport();
                            }
                            break;
                        case 82://Deriv Banned Scrip Report
                            {
                                cmd.CommandText = MakeCommandText();                               
                                da.Fill(dt);
                                ProcessDerivBannedScripReport();
                            }
                            break;
                        case 83://Adhoc Report
                            {
                                cmd.CommandText = MakeCommandText();                             
                                da.Fill(dt);
                                ProcessAdhocReport();
                            }
                            break;
                        case 84: //Adhoc Modify Reply
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessAdhocModifyReply();
                            }
                            break;
                        case 86: //MF Debit Client List
                            {
                                cmd.CommandText = MakeCommandText();                            
                                da.Fill(dt);
                                ProcessMFDebitClients();
                            }
                            break;
                        case 87://Maal Fodo Client Holdings
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ProcessMFClientHoldingReport();
                            }
                            break;
                        case 89: //Illiquid Scrip Report
                            {
                                cmd.CommandText = MakeCommandText();                            
                                da.Fill(dt);
                                ProcessIlliquidReport();
                            }
                            break;                        
                        case 110://Place Deriv Order
                            {
                                cmd.CommandText = MakeCommandText();                              
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessDerivPlaceOrderRequest();
                            }
                            break;
                        case 115:
                            {
                                cmd.CommandText = MakeCommandText();                               
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessAHDerivPlaceOrderRequest();
                            }
                            break;
                        case 120://Modify Deriv Order
                            {
                                cmd.CommandText = MakeCommandText();                               
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessDerivModifyOrderRequest();
                            }
                            break;
                        case 130://Cancel Deriv Order
                            {
                                cmd.CommandText = MakeCommandText();                          
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessDerivCancelOrderRequest();
                            }
                            break;
                        case 200://Margin Report
                            {
                                cmd.CommandText = MakeCommandText();                            
                                da.Fill(dt);
                                ProcessMarginReport();
                            }
                            break;
                        case 248:   //Added on 19-Sep-2016 - Rohan
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ProcessSegmentMaster(dt);
                            }
                            break;
                        case 251://Link Failed Message
                            {
                                cmd.CommandText = MakeCommandText();                            
                                da.Fill(dt);
                                //Nothing will be returned
                            }
                            break;
                        case 252://Client Disconnected
                            {
                                cmd.CommandText = MakeCommandText();                            
                                da.Fill(dt);
                                //Nothing will be returned
                            }
                            break;
                        case 247://Client Disconnected
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                //Nothing will be returned
                            }
                            break;
                        #region Nse Currency Derive
                        case 211://Place Curr Deriv Order
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessCurrDerivPlaceOrderRequest();
                            }
                            break;
                        case 221://Modify Curr Deriv Order
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessCurrDerivModifyOrderRequest();
                            }
                            break;
                        case 231://Cancel Curr Deriv Order
                            {
                                cmd.CommandText = MakeCommandText();
                                da.Fill(dt);
                                ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                ProcessCurrDerivCancelOrderRequest();
                            }
                            break;
                        #endregion
                    }
                }
                catch (Exception er)
                {
                    ExceptionStr = er.Message;
                    HandleException();                   
                }

                try
                {
                    if (frm.checkBox1.Checked)
                    {

                        dt.Clear();
                        frm.BeginInvoke(frm.ThreadTermination, new DBResult(DBNo, ThreadNumber, ExType, OTSReplyRec, OTSReqRec, AfterHours, RqStr, ExStr, DeadlockRetry,ThreadStartTick));
                        this.t.Suspend();
                        // frm.Invoke(new ThreadTerminationAction(frm.ThreadTerminationHandler), this);            
                    }

                }
                catch (Exception et)
                {
                    Form1.ErrorLog(et);
                }
            }
        }

        private void ProcessLoginReplyWithName()//02.09.16
        {
            try
            {
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TLoginReplyRecordWithName());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TLoginReplyRecordWithName());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            VList = ReplyStr.Split(splitchar);
            OTSReplyRec.AllReply[0].LoginReplyWithName.FEUserIDLength = OTSReqRec.AllReq.LoginRequestForName.FEUserIDLength;
            OTSReplyRec.AllReply[0].LoginReplyWithName.FEUserID = new string(OTSReqRec.AllReq.LoginRequestForName.FEUserID).PadRight(14).ToCharArray();
            OTSReplyRec.AllReply[0].LoginReplyWithName.FEUserName = VList[1].TrimEnd();
            OTSReplyRec.AllReply[0].LoginReplyWithName.FEUserNameLength = (byte)VList[1].TrimEnd().Length;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessLoginReplyNew()//02.09.16
        {
            string C1;
            int i;
            try
            {
                OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TLoginReplyRecordNew());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TLoginReplyRecordNew());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 3 && VList.Count() != 19 && VList.Count() != 22 && VList.Count() != 24)  //improper string//02.09.16
                {
                    ReplyStr = "1|Unknown Database Error. Please retry|0";
                    VList = ReplyStr.Split(splitchar);
                }
                if (VList[0] == "1") //ERROR indication
                {
                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                    OTSReplyRec.ErrorStringLength = (byte)VList[1].Length;
                    OTSReplyRec.ErrorString = VList[1];
                    OTSReplyRec.AllReply[0].LoginReplyNew.BadLoginCnt = Convert.ToInt32(VList[2]);
                }
                else
                {
                    OTSReplyRec.AllReply[0].LoginReplyNew.FEUserIDLength = OTSReqRec.AllReq.LoginRequest.FEUserIDLength;
                    OTSReplyRec.AllReply[0].LoginReplyNew.FEUserID = new string(OTSReqRec.AllReq.LoginRequest.FEUserID).PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[0].LoginReplyNew.FEUserType = Convert.ToByte(VList[1]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.SystemDateTime = Convert.ToInt32(VList[2]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.LastModifiedTime = Convert.ToInt32(VList[3]);
                    C1 = VList[4];
                    if (C1 == "Y")
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseCash = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseCash = 0;
                    C1 = VList[5];
                    if (C1 == "Y")
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseDeriv = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseDeriv = 0;
                    C1 = VList[6];
                    if (C1 == "Y")
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseCash = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseCash = 0;
                    C1 = VList[7];
                    if (C1 == "Y")
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseCurrency = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseCurrency = 0;
                    C1 = VList[8];
                    if (C1 == "Y")//MCX Changes
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowMcxDeriv = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowMcxDeriv = 0;

                    OTSReplyRec.AllReply[0].LoginReplyNew.LastAccessedTime = Convert.ToInt32(VList[9]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.BranchID = Convert.ToUInt16(VList[10]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_NC = Convert.ToInt64(VList[11]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_ND = Convert.ToInt64(VList[12]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_BC = Convert.ToInt64(VList[13]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_CD = Convert.ToInt64(VList[14]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_MD = Convert.ToInt64(VList[15]);

                    C1 = VList[16];
                    if (C1 == "Y")
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowTotalBuySellQty = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowTotalBuySellQty = 0;
                    OTSReplyRec.AllReply[0].LoginReplyNew.AddMsgLength = (byte)VList[17].TrimEnd().Length;
                    OTSReplyRec.AllReply[0].LoginReplyNew.AddMsg = VList[17].TrimEnd();
                    OTSReplyRec.AllReply[0].LoginReplyNew.AccountType = AcTypeStrToByte(VList[18]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.AllowFastFeed = Convert.ToChar(VList[19]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.AllowHoldingsMargin = Convert.ToChar(VList[20]);

                    if (VList.Count() == 23)
                    {
                        i = Convert.ToInt32(VList[21]);
                        OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsg = VList[22].TrimEnd();
                        OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsgLength = Convert.ToByte(VList[22].TrimEnd().Length);
                    }
                    else
                    {
                        OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsg = "";
                        OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsgLength = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }


        //---------------------------------     

        private void HandleException()
        {
            try
            {
                errorSting = "";
                ExStr = ExceptionStr;
                RqStr = RequestStr;
                if (ExceptionStr.Contains("Rerun the transaction"))//retry if more chance left, else server is busy
                {
                    ExType = 101;
                    if (OTSReqRec.RetryCnt < MaxDeadlockRetryCnt)
                        DeadlockRetry = true;
                    else
                        if (OTSReqRec.RetryCnt >= MaxDeadlockRetryCnt)
                        {
                            DeadlockRetry = false;
                            switch (OTSReqRec.ReqHdr.RequestType)
                            {
                                case 1:
                                    {
                                        ReplyStr = "1|Server is busy. Please retry|0";
                                        ProcessLoginReply();
                                    }
                                    break;
                                case 2:
                                    {
                                        ReplyStr = "1|Server is busy. Please retry";
                                        ProcessChangePasswordReply();
                                    }
                                    break;
                                case 5://Trader List - Admin only
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Trader List Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 7://Trader List - Admin only
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Trader List Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 8://Trader List - Admin only
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Trader List Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 6://Verify Trader - Admin only
                                    {
                                        ReplyStr = "1|Server is busy. Please retry";
                                        ProcessVerifyTrader();
                                    }
                                    break;
                                case 10://Place Equity Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessEquityPlaceOrderRequest();
                                    }
                                    break;
                                case 20://Modify Equity Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessEquityModifyOrderRequest();
                                    }
                                    break;
                                case 30://Cancel Equity Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessEquityCancelOrderRequest();
                                    }
                                    break;

                                case Constants.OTS_SECURITY_INFO://Cancel Equity Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessCancelSecurityInfoRequest();
                                    }
                                    break;

                                case 61://Trade Report
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Order Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 62://Order Report
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Order Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                            case 78://Order Report with Cover Or Bracket               // sumitp 28_jan_22
                                {
                                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    OTSReplyRec.ErrorString = "Server is busy. Retrying Order Report";
                                    OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                }
                                break;
                            case 64://Activity Log
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Activity Log Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 69:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Holdings Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case Constants.OTS_VERIFY_USER: //99
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessVerifyUserReply();
                                    }
                                    break;
                                case 110: //Place Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessDerivPlaceOrderRequest();
                                    }
                                    break;
                                case 120://Modify Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessDerivModifyOrderRequest();
                                    }
                                    break;
                                case 130: //Cancel Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessDerivCancelOrderRequest();
                                    }
                                    break;
                                #region Nse Curr Derive
                                case 211: //Place Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessCurrDerivPlaceOrderRequest();
                                    }
                                    break;
                                case 221://Modify Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessCurrDerivModifyOrderRequest();
                                    }
                                    break;
                                case 231: //Cancel Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessCurrDerivCancelOrderRequest();
                                    }
                                    break;
                                #endregion
                                case 200://Margin Report
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 204://Margin Report
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report MCX";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                            }
                        }
                }
                if (!ExceptionStr.Contains("Rerun the transaction"))//Some Database error, not possible to retry
                {
                    ExType = 104;
                    ExStr1 = ExStr;
                    if (ExceptionStr.Contains("Cannot open database "))
                    {
                        ExStr1 = "";
                    }
                    switch (OTSReqRec.ReqHdr.RequestType)
                    {
                        case 1:
                            {
                                ReplyStr = "1|Database Error." + ExStr1 + "|0";
                                ProcessLoginReply();
                            }
                            break;
                        case 2:
                            {
                                ReplyStr = "1|Database Error." + ExStr1;
                                ProcessChangePasswordReply();
                            }
                            break;
                        case 5://Trader List - Admin only
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Trader List Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 7://Trader List - Admin only
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Trader List Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 8://Trader List - Admin only
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Trader List Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 6://Verify Trader - Admin only
                            {
                                ReplyStr = "1|Database Error." + ExStr1;
                                ProcessVerifyTrader();
                            }
                            break;
                        case 10://Place Equity Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessEquityPlaceOrderRequest();
                            }
                            break;
                        case 20://Modify Equity Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessEquityModifyOrderRequest();
                            }
                            break;
                        case 30://Cancel Equity Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessEquityCancelOrderRequest();
                            }
                            break;
                        case 61://Trade Report
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Trade Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 62://Order Report
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 78://Order Report with Cover or bracket              // Himanshu    // sumitp 28_jan_22
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 64://Activity Log
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Activity Log Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 69:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Holdings Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 110: //Place Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessDerivPlaceOrderRequest();
                            }
                            break;
                        case 120://Modify Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessDerivModifyOrderRequest();
                            }
                            break;
                        case 130: //Cancel Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessDerivCancelOrderRequest();
                            }
                            break;
                        #region Nse Curr Deriv
                        case 211: //Place Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessCurrDerivPlaceOrderRequest();
                            }
                            break;
                        case 221://Modify Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessCurrDerivModifyOrderRequest();
                            }
                            break;
                        case 231: //Cancel Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessCurrDerivCancelOrderRequest();
                            }
                            break;
                        #endregion
                        case 200://Margin Report
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Margin Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 204://Margin Report
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Margin Report MCX-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {                
                Form1.ErrorLog(e);
            }
        }

        //---------------------------------

        private void ProcessAHDerivPlaceOrderRequest()
        {
            int BOId;
            Single MBlk;
            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyType = 110;
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Exch = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Exch;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ExchType = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.ExchType;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ScripCode = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.DerivInfo;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BuySell = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.BuySell;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Qty = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Qty;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AtMarket = 'N';
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Rate = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Rate;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.WithSL = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.WithSL;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TraderRequestID = 0;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.IOC = 0;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AHStatus = 'N';
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessHoldingsValuationReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }

            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsValuationReportRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].HoldingsValuationReportReplyRec.ScName = Convert.ToString(dt.Rows[i]["NAME"]).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[i].HoldingsValuationReportReplyRec.ScNameLength = (byte)new string(OTSReplyRec.AllReply[i].HoldingsValuationReportReplyRec.ScName).TrimEnd().Length;
                OTSReplyRec.AllReply[i].HoldingsValuationReportReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["TOTALQTY"]);
                OTSReplyRec.AllReply[i].HoldingsValuationReportReplyRec.PrevClose = Convert.ToSingle(dt.Rows[i]["PREVCLOSE"]);
                OTSReplyRec.AllReply[i].HoldingsValuationReportReplyRec.HolidingsValue = Convert.ToDouble(dt.Rows[i]["TOTALVALUE"]);
                OTSReplyRec.AllReply[i].HoldingsValuationReportReplyRec.AfterHaircutValue = Convert.ToDouble(dt.Rows[i]["VALUEAFTERHAIRCUT"]);                
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessDerivCancelOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;

            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderCancelBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderCancelBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);

            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.Exch = OTSReqRec.AllReq.DerivOrderCancelRequestRec.Exch;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ExchType = OTSReqRec.AllReq.DerivOrderCancelRequestRec.ExchType;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ScripCode = OTSReqRec.AllReq.DerivOrderCancelRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.DerivInfo = OTSReqRec.AllReq.DerivOrderCancelRequestRec.DerivInfo;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.BuySell = OTSReqRec.AllReq.DerivOrderCancelRequestRec.BuySell;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.Qty = OTSReqRec.AllReq.DerivOrderCancelRequestRec.Qty;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.AtMarket = OTSReqRec.AllReq.DerivOrderCancelRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.Rate = OTSReqRec.AllReq.DerivOrderCancelRequestRec.Rate;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.WithSL = OTSReqRec.AllReq.DerivOrderCancelRequestRec.WithSL;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.DerivOrderCancelRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.DerivOrderCancelRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.MarginBlocked = MBlk;

            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.DerivOrderCancelRequestRec.SLTriggered;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.OldQty = OTSReqRec.AllReq.DerivOrderCancelRequestRec.OldQty;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.TradedQty = OTSReqRec.AllReq.DerivOrderCancelRequestRec.TradedQty;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.PendingQty = OTSReqRec.AllReq.DerivOrderCancelRequestRec.PendingQty;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.DerivOrderCancelRequestRec.ExchangeOrderId;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.DerivOrderCancelRequestRec.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.AHStatus = AHStatus1;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessDerivModifyOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;
            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderModifyBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderModifyBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);

            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.Exch = OTSReqRec.AllReq.DerivOrderModifyRequestRec.Exch;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ExchType = OTSReqRec.AllReq.DerivOrderModifyRequestRec.ExchType;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ScripCode = OTSReqRec.AllReq.DerivOrderModifyRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.DerivInfo = OTSReqRec.AllReq.DerivOrderModifyRequestRec.DerivInfo;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.BuySell = OTSReqRec.AllReq.DerivOrderModifyRequestRec.BuySell;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.Qty = OTSReqRec.AllReq.DerivOrderModifyRequestRec.Qty;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.AtMarket = OTSReqRec.AllReq.DerivOrderModifyRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.Rate = OTSReqRec.AllReq.DerivOrderModifyRequestRec.Rate;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.WithSL = OTSReqRec.AllReq.DerivOrderModifyRequestRec.WithSL;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.DerivOrderModifyRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.DerivOrderModifyRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.MarginBlocked = MBlk;

            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.DerivOrderModifyRequestRec.SLTriggered;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.OldQty = OTSReqRec.AllReq.DerivOrderModifyRequestRec.OldQty;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.TradedQty = OTSReqRec.AllReq.DerivOrderModifyRequestRec.TradedQty;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.PendingQty = OTSReqRec.AllReq.DerivOrderModifyRequestRec.PendingQty;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.DerivOrderModifyRequestRec.ExchangeOrderId;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.DerivOrderModifyRequestRec.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.IOC = OTSReqRec.AllReq.DerivOrderModifyRequestRec.IOC;
            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.AHStatus = AHStatus1;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessDerivPlaceOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;
            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Exch = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Exch;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ExchType = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.ExchType;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ScripCode = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.DerivInfo;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BuySell = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.BuySell;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Qty = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Qty;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AtMarket = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Rate = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Rate;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.WithSL = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.WithSL;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.IOC = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.IOC;
            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AHStatus = AHStatus1;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessEquityCancelOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;

            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderCancelBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderCancelBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Exch = OTSReqRec.AllReq.EquityOrderCancelRequestRec.Exch;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchType = OTSReqRec.AllReq.EquityOrderCancelRequestRec.ExchType;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripCode = OTSReqRec.AllReq.EquityOrderCancelRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.EquityOrderCancelRequestRec.ScripName).PadRight(12).ToCharArray();
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.EquityOrderCancelRequestRec.ScripNameLength;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BuySell = OTSReqRec.AllReq.EquityOrderCancelRequestRec.BuySell;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Qty = OTSReqRec.AllReq.EquityOrderCancelRequestRec.Qty;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Rate = OTSReqRec.AllReq.EquityOrderCancelRequestRec.Rate;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.AtMarket = OTSReqRec.AllReq.EquityOrderCancelRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.WithSL = OTSReqRec.AllReq.EquityOrderCancelRequestRec.WithSL;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.EquityOrderCancelRequestRec.SLTriggered;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.EquityOrderCancelRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.EquityOrderCancelRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TradedQty = OTSReqRec.AllReq.EquityOrderCancelRequestRec.TradedQty;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.PendingQty = OTSReqRec.AllReq.EquityOrderCancelRequestRec.PendingQty;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchangeOrderID = OTSReqRec.AllReq.EquityOrderCancelRequestRec.ExchangeOrderID;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.EquityOrderCancelRequestRec.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.AHStatus = AHStatus1;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        //2015.03.26

        // 2015.03.26 Create method to accept reply from database
        private void ProcessCancelSecurityInfoRequest()
        {
            try
            {
            VList = ReplyStr.Split(splitchar);
            
            if (VList.Count() != 20)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TScripInfoReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TScripInfoReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Exch = Convert.ToChar(VList[0]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.ExchType = Convert.ToChar(VList[1]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Code = Convert.ToInt32(VList[2]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.ISINLength = (byte)VList[3].Length;
            VList[3] = VList[3].PadRight(12, '0');
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.ISIN = VList[3].ToCharArray();
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.SpanMargin = Convert.ToSingle(VList[4]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.ExpMargin = Convert.ToSingle(VList[5]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.IssuedCapital = Convert.ToSingle(VList[6]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.FreezeQty = Convert.ToInt32(VList[7]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.FaceValue = Convert.ToInt32(VList[8]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.PreOpen = Convert.ToChar(VList[9]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.BookClosureStartDate = Convert.ToInt32(VList[10]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.BookClosureEndDate = Convert.ToInt32(VList[11]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.ExpDate = Convert.ToInt32(VList[12]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Category = Convert.ToChar(VList[13]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.VarMargin = Convert.ToSingle(VList[14]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Dividend = Convert.ToChar(VList[15]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Rights = Convert.ToChar(VList[16]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Bonus = Convert.ToChar(VList[17]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Interest = Convert.ToChar(VList[18]);
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.CreditRatingLength = (byte)VList[19].Length;
            VList[19] = VList[19].PadRight(17, '0');
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.CreditRating = VList[19].ToCharArray();

            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved1 = 0; //byte Reserved1
            OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved2 = 0; //int Reserved2
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        // 2015.03.26 Create method to accept reply from database



        private void ProcessEquityModifyOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;

            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderModifyBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderModifyBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Exch = OTSReqRec.AllReq.EquityOrderModifyRequestRec.Exch;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchType = OTSReqRec.AllReq.EquityOrderModifyRequestRec.ExchType;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripCode = OTSReqRec.AllReq.EquityOrderModifyRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.EquityOrderModifyRequestRec.ScripName).PadRight(12).ToCharArray();
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.EquityOrderModifyRequestRec.ScripNameLength;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BuySell = OTSReqRec.AllReq.EquityOrderModifyRequestRec.BuySell;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Qty = OTSReqRec.AllReq.EquityOrderModifyRequestRec.Qty;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.AtMarket = OTSReqRec.AllReq.EquityOrderModifyRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Rate = OTSReqRec.AllReq.EquityOrderModifyRequestRec.Rate;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.WithSL = OTSReqRec.AllReq.EquityOrderModifyRequestRec.WithSL;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.EquityOrderModifyRequestRec.SLTriggered;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.EquityOrderModifyRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.EquityOrderModifyRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.IOC = OTSReqRec.AllReq.EquityOrderModifyRequestRec.IOC;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.EquityOrderModifyRequestRec.DisclosedQty;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.OldQty = OTSReqRec.AllReq.EquityOrderModifyRequestRec.OldQty;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TradedQty = OTSReqRec.AllReq.EquityOrderModifyRequestRec.TradedQty;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.PendingQty = OTSReqRec.AllReq.EquityOrderModifyRequestRec.PendingQty;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.EquityOrderModifyRequestRec.ExchangeOrderId;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.EquityOrderModifyRequestRec.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.AHStatus = AHStatus1;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        #region Nse Currency Derive

        private void ProcessCurrDerivPlaceOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;

            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.Exch = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Exch;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.ExchType = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.ExchType;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.ScripCode = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DerivInfo = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.BuySell = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.BuySell;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.Qty = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Qty;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.AtMarket = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.Rate = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Rate;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.WithSL = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.WithSL;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.IOC = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.IOC;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.AHStatus = AHStatus1;
            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.DisclosedQty;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessCurrDerivModifyOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;

            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderModifyBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderModifyBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);

            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.Exch = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Exch;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ExchType = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.ExchType;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ScripCode = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DerivInfo = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.DerivInfo;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.BuySell = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.BuySell;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.Qty = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Qty;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.AtMarket = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.Rate = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Rate;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.WithSL = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.WithSL;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.MarginBlocked = MBlk;

            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.SLTriggered;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.OldQty = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.OldQty;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.TradedQty = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.TradedQty;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.PendingQty = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.PendingQty;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.ExchangeOrderId;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.IOC = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.IOC;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.AHStatus = AHStatus1;
            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.DisclosedQty;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessCurrDerivCancelOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;
            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;
            if (VList[2] == "1")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderCancelBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderCancelBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);

            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.Exch = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Exch;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ExchType = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.ExchType;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ScripCode = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DerivInfo = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.DerivInfo;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.BuySell = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.BuySell;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.Qty = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Qty;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.AtMarket = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.Rate = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Rate;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.WithSL = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.WithSL;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.MarginBlocked = MBlk;

            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.SLTriggered;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.OldQty = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.OldQty;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.TradedQty = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.TradedQty;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.PendingQty = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.PendingQty;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.ExchangeOrderId;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.AHStatus = AHStatus1;
            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.DisclosedQty;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
       
        #endregion

        private void ProcessAHEquityPlaceOrderRequest()
        {
            int BOId;
            Single MBlk;

            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);

            if (VList[2] == "0")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyType = 10;//we are changing 15 to 10
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Exch = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.Exch;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ExchType = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ExchType;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripCode = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ScripName).PadRight(12).ToCharArray();
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ScripNameLength;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BuySell = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.BuySell;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Qty = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.Qty;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AtMarket = 'N';
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Rate = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.Rate;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.WithSL = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.WithSL;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TraderRequestID = 0;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.IOC = 0;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.DisclosedQty;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AHStatus = 'N';
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessEquityPlaceOrderRequest()
        {
            int BOId;
            Single MBlk;
            char AHStatus1;
            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 5)
                AHStatus1 = Convert.ToChar(VList[4].ToUpper());
            else
                AHStatus1 = 'N';
            AfterHours = AHStatus1;

            if (VList[2] != "0")
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].TrimEnd().Length;
                OTSReplyRec.ErrorString = VList[3];
                MBlk = 0;
            }
            else
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";
                MBlk = Convert.ToSingle(VList[3]);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Exch = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Exch;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ExchType = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ExchType;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripCode = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripName).PadRight(12).ToCharArray();
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripNameLength;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BuySell = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.BuySell;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Qty = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Qty;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AtMarket = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Rate = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Rate;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.WithSL = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.WithSL;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.IOC = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.IOC;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.DisclosedQty;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AHStatus = AHStatus1;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessClientPositionReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TClientPositionReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.ScCode = Convert.ToInt32(dt.Rows[i]["ScripCode"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.HoldingUsedQty = Convert.ToInt32(dt.Rows[i]["BodHolding"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.BoughtQty = Convert.ToInt32(dt.Rows[i]["BuyQty"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.SoldQty = Convert.ToInt32(dt.Rows[i]["SellQty"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.SellAvgRate = Convert.ToSingle(dt.Rows[i]["SellAvgPrice"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.PendBuyQty = Convert.ToInt32(dt.Rows[i]["PendBuyQty"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.PendBuyAvgRate = Convert.ToSingle(dt.Rows[i]["PendBuyAvgPrice"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.PendSellQty = Convert.ToInt32(dt.Rows[i]["PendSellQty"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.PendSellAvgRate = Convert.ToSingle(dt.Rows[i]["PendSellAvgPrice"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.AHV = Convert.ToInt32(dt.Rows[i]["AHV"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.PSV = Convert.ToInt32(dt.Rows[i]["PSV"]);
                OTSReplyRec.AllReply[i].ClientPositionReplyRec.MBPO = Convert.ToInt32(dt.Rows[i]["MBPO"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessVerifyTrader()
        {
            string C1, CCode2;
            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 2 && VList.Count() != 10)//improper string
            {
                ReplyStr = "1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTraderListReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTraderListReplyRecord());
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            if (VList[0] == "1")
            {
                OTSReplyRec.AllReply[0].TraderListReplyRec.TraderName = "";
                OTSReplyRec.AllReply[0].TraderListReplyRec.TraderNameLength = 0;
            }
            else
            {
                OTSReplyRec.AllReply[0].TraderListReplyRec.TraderID = VList[1].PadRight(14).ToCharArray();
                OTSReplyRec.AllReply[0].TraderListReplyRec.TraderIDLength = Convert.ToByte(VList[1].TrimEnd().Length);
                OTSReplyRec.AllReply[0].TraderListReplyRec.TraderName = VList[2];
                OTSReplyRec.AllReply[0].TraderListReplyRec.TraderNameLength = Convert.ToByte(VList[2].TrimEnd().Length);
                OTSReplyRec.AllReply[0].TraderListReplyRec.TraderType = Convert.ToByte(VList[3]);
                C1 = VList[4];
                if (C1 == "Y")
                    OTSReplyRec.AllReply[0].TraderListReplyRec.AllowNseCash = 1;
                else
                    OTSReplyRec.AllReply[0].TraderListReplyRec.AllowNseCash = 0;
                C1 = VList[5];
                if (C1 == "Y")
                    OTSReplyRec.AllReply[0].TraderListReplyRec.AllowNseDeriv = 1;
                else
                    OTSReplyRec.AllReply[0].TraderListReplyRec.AllowNseDeriv = 0;
                C1 = VList[6];
                if (C1 == "Y")
                    OTSReplyRec.AllReply[0].TraderListReplyRec.AllowBseCash = 1;
                else
                    OTSReplyRec.AllReply[0].TraderListReplyRec.AllowBseCash = 0;

                OTSReplyRec.AllReply[0].TraderListReplyRec.BranchID = Convert.ToUInt16(VList[7]);
                CCode2 = VList[8].TrimEnd();//added on 01/02/2008 . Changed Proc_VerifyTrader
                if (CCode2 == "0")
                    OTSReplyRec.AllReply[0].TraderListReplyRec.NilOrders = 1;
                else
                    OTSReplyRec.AllReply[0].TraderListReplyRec.NilOrders = 0;
                OTSReplyRec.AllReply[0].TraderListReplyRec.AccountType = AcTypeStrToByte(VList[9].TrimEnd());
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessMFClientHoldingReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMaalFodoClientHoldingReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.TraderID = Convert.ToString(dt.Rows[i]["CLIENTCODE"]).PadRight(15).ToCharArray();
                OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.TraderIDLength = (byte)new string(OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.TraderID).TrimEnd().Length;
                OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.BrokerBeneQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.TodaySoldQty = Convert.ToInt32(dt.Rows[i]["TODAYSOLDQTY"]);
                OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec.Category = Convert.ToChar(dt.Rows[i]["CATEGORY"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessMFDebitClients()
        {
            ITInteractiveStructures.TMaalFodoClientsReportReplyRecord MFCliReportReplyRec = new ITInteractiveStructures.TMaalFodoClientsReportReplyRecord();
            int j = 0;
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }  
                
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                MFCliReportReplyRec.TraderID = Convert.ToString(dt.Rows[i]["CLIENTCODE"]).PadRight(14).ToCharArray();
                MFCliReportReplyRec.TraderIDLength = (byte)new string(MFCliReportReplyRec.TraderID).TrimEnd().Length;
                MFCliReportReplyRec.ClientName = Convert.ToString(dt.Rows[i]["CLIENTNAME"]);
                MFCliReportReplyRec.ClientNameLength = (byte)MFCliReportReplyRec.ClientName.TrimEnd().Length;
                MFCliReportReplyRec.BranchId = Convert.ToInt32(dt.Rows[i]["BRANCHID"]);
                MFCliReportReplyRec.BranchAddr = Convert.ToString(dt.Rows[i]["BRANCHADDR"]).PadRight(20).ToCharArray();
                MFCliReportReplyRec.BranchAddrLength = (byte)new string(MFCliReportReplyRec.BranchAddr).TrimEnd().Length;
                MFCliReportReplyRec.AdminID = Convert.ToString(dt.Rows[i]["ADMINCODE"]).PadRight(15).ToCharArray();
                MFCliReportReplyRec.AdminIDLength = (byte)new string(MFCliReportReplyRec.AdminID).TrimEnd().Length;
                MFCliReportReplyRec.MFDebit = Convert.ToDouble(dt.Rows[i]["MAALFODODEBIT"]);
                MFCliReportReplyRec.Mgn4OpenPos = Convert.ToDouble(dt.Rows[i]["MGN4OPENPOS"]);
                MFCliReportReplyRec.Receipts = Convert.ToDouble(dt.Rows[i]["RECEIPTS"]);
                MFCliReportReplyRec.Shortage = Convert.ToDouble(dt.Rows[i]["SHORTAGE"]);
                MFCliReportReplyRec.FOBalance = Convert.ToDouble(dt.Rows[i]["FOBALANCE"]);
                MFCliReportReplyRec.FOMgnPayable = Convert.ToDouble(dt.Rows[i]["FOMARGINPAYABLE"]);

                if (MFCliReportReplyRec.Mgn4OpenPos < 0)
                    MFCliReportReplyRec.Mgn4OpenPos = 0;

                MFCliReportReplyRec.FinalMF = MFCliReportReplyRec.MFDebit - MFCliReportReplyRec.Mgn4OpenPos -
                    MFCliReportReplyRec.Receipts - MFCliReportReplyRec.Shortage;

                if (MFCliReportReplyRec.FOBalance < 0)
                    MFCliReportReplyRec.FinalMF = MFCliReportReplyRec.FinalMF + MFCliReportReplyRec.FOBalance;

                if (MFCliReportReplyRec.FinalMF < 0)
                    MFCliReportReplyRec.FinalMF = 0.00f;
                if (MFCliReportReplyRec.FinalMF <= MFDebitAmt) //31/05/2012
                    continue;
                MFCliReportReplyRec.AllowBseCash = Convert.ToChar(dt.Rows[i]["ALLOWBSECASH"]);
                MFCliReportReplyRec.AllowNseCash = Convert.ToChar(dt.Rows[i]["ALLOWNSECASH"]);
                MFCliReportReplyRec.AllowNseDeriv = Convert.ToChar(dt.Rows[i]["ALLOWNSEDERIV"]);
                MFCliReportReplyRec.AccountType = AcTypeStrToByte(Convert.ToString(dt.Rows[i]["ACCOUNTTYPE"]));
                Array.Resize(ref OTSReplyRec.AllReply, j + 1);
                OTSReplyRec.AllReply[j].MFClientsReportReplyRec = MFCliReportReplyRec;
                j++;
            }

            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMaalFodoClientsReportReplyRecord());   
            OTSReplyRec.ReplyHdr.ReplyCnt =(ushort) j;
            OTSReplyRec.ReplyHdr.ReplyLen = j * OTSReplyRec.ReplyHdr.ReplyRecSize;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessAdhocModifyReply()
        {
            try{
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TAdhocModifyReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, 1);

            VList = ReplyStr.Split(splitchar);
            OTSReplyRec.AllReply[0].AdhocModifyReplyRec.ClientCode = VList[0].PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].AdhocModifyReplyRec.ClientCodeLength = Convert.ToByte(VList[0].TrimEnd().Length);
            OTSReplyRec.AllReply[0].AdhocModifyReplyRec.Status = Convert.ToChar(VList[1]);
            OTSReplyRec.AllReply[0].AdhocModifyReplyRec.Msg = VList[2];
            OTSReplyRec.AllReply[0].AdhocModifyReplyRec.MsgLength = Convert.ToByte(VList[2].TrimEnd().Length);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessAdhocReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TAdhocReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.ClientCode = Convert.ToString(dt.Rows[i]["ClientCode"]).PadRight(15).ToCharArray();
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.ClientCodeLength = (byte)new string(OTSReplyRec.AllReply[i].AdhocReportReplyRec.ClientCode).TrimEnd().Length;
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.BodLB = Convert.ToSingle(dt.Rows[i]["BodLB"]);
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.Receipts = Convert.ToSingle(dt.Rows[i]["Receipts"]);
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.Payments = Convert.ToSingle(dt.Rows[i]["Payments"]);
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.Adhoc = Convert.ToSingle(dt.Rows[i]["Adhoc"]);
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.AHVCash = Convert.ToSingle(dt.Rows[i]["AHV"]);
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.PSVCash = Convert.ToSingle(dt.Rows[i]["PSV"]);
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.MBPOCash = Convert.ToSingle(dt.Rows[i]["MBPO"]);
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.AHVFnO = 0.00f;
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.PSVFnO = 0.00f;
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.MBPOFnO = 0.00f;
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.AvlbMgn = Convert.ToSingle(dt.Rows[i]["AvlbMgn"]);
                OTSReplyRec.AllReply[i].AdhocReportReplyRec.MFDebit = Convert.ToSingle(dt.Rows[i]["MaalFodoDebit"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessTraderList()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTraderListReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].TraderListReplyRec.TraderID = Convert.ToString(dt.Rows[i]["ClientCode"]).PadRight(14).ToCharArray();
                OTSReplyRec.AllReply[i].TraderListReplyRec.TraderIDLength = (byte)new string(OTSReplyRec.AllReply[i].TraderListReplyRec.TraderID).TrimEnd().Length;
                OTSReplyRec.AllReply[i].TraderListReplyRec.TraderName = Convert.ToString(dt.Rows[i]["ClientName"]);
                OTSReplyRec.AllReply[i].TraderListReplyRec.TraderNameLength = (byte)OTSReplyRec.AllReply[i].TraderListReplyRec.TraderName.TrimEnd().Length;
                OTSReplyRec.AllReply[i].TraderListReplyRec.TraderType = Convert.ToByte(dt.Rows[i]["ClientType"]);
                OTSReplyRec.AllReply[i].TraderListReplyRec.BranchID = Convert.ToUInt16(dt.Rows[i]["BranchID"]);
                if (Convert.ToString(dt.Rows[i]["AllowNseCash"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseCash = 1;
                else
                    OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseCash = 0;

                if (Convert.ToString(dt.Rows[i]["AllowNseDeriv"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseDeriv = 1;
                else
                    OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseDeriv = 0;

                if (Convert.ToString(dt.Rows[i]["AllowBseCash"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].TraderListReplyRec.AllowBseCash = 1;
                else
                    OTSReplyRec.AllReply[i].TraderListReplyRec.AllowBseCash = 0;

                if (Convert.ToString(dt.Rows[i]["ClientCode2"]).TrimEnd() == "0")
                    OTSReplyRec.AllReply[i].TraderListReplyRec.NilOrders = 1;
                else
                    OTSReplyRec.AllReply[i].TraderListReplyRec.NilOrders = 0;

                OTSReplyRec.AllReply[i].TraderListReplyRec.AccountType = AcTypeStrToByte(Convert.ToString(dt.Rows[i]["AccountType"]).TrimEnd());
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessdDealerOrderReport()
        {
            try
            {
                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeReportsReplyRecDealer());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderID = Convert.ToString(dt.Rows[i]["ClientCode"]).PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderIDLength = (byte)new string(OTSReplyRec.AllReply[i].TraderListReplyRec.TraderID).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderName = Convert.ToString(dt.Rows[i]["ClientName"]);
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderNameLength = (byte)OTSReplyRec.AllReply[i].TraderListReplyRec.TraderName.TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderType = Convert.ToByte(dt.Rows[i]["ClientType"]);
                    OTSReplyRec.AllReply[i].TraderListReplyRec.BranchID = Convert.ToUInt16(dt.Rows[i]["BranchID"]);
                    if (Convert.ToString(dt.Rows[i]["AllowNseCash"]).TrimEnd() == "Y")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseCash = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseCash = 0;

                    if (Convert.ToString(dt.Rows[i]["AllowNseDeriv"]).TrimEnd() == "Y")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseDeriv = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseDeriv = 0;

                    if (Convert.ToString(dt.Rows[i]["AllowBseCash"]).TrimEnd() == "Y")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowBseCash = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowBseCash = 0;

                    if (Convert.ToString(dt.Rows[i]["ClientCode2"]).TrimEnd() == "0")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.NilOrders = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.NilOrders = 0;

                    OTSReplyRec.AllReply[i].TraderListReplyRec.AccountType = AcTypeStrToByte(Convert.ToString(dt.Rows[i]["AccountType"]).TrimEnd());
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessDealerTradesReport()
        {
            try
            {
                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTradeReportReplyRecDealer());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderID = Convert.ToString(dt.Rows[i]["ClientCode"]).PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderIDLength = (byte)new string(OTSReplyRec.AllReply[i].TraderListReplyRec.TraderID).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderName = Convert.ToString(dt.Rows[i]["ClientName"]);
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderNameLength = (byte)OTSReplyRec.AllReply[i].TraderListReplyRec.TraderName.TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TraderListReplyRec.TraderType = Convert.ToByte(dt.Rows[i]["ClientType"]);
                    OTSReplyRec.AllReply[i].TraderListReplyRec.BranchID = Convert.ToUInt16(dt.Rows[i]["BranchID"]);
                    if (Convert.ToString(dt.Rows[i]["AllowNseCash"]).TrimEnd() == "Y")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseCash = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseCash = 0;

                    if (Convert.ToString(dt.Rows[i]["AllowNseDeriv"]).TrimEnd() == "Y")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseDeriv = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowNseDeriv = 0;

                    if (Convert.ToString(dt.Rows[i]["AllowBseCash"]).TrimEnd() == "Y")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowBseCash = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.AllowBseCash = 0;

                    if (Convert.ToString(dt.Rows[i]["ClientCode2"]).TrimEnd() == "0")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.NilOrders = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.NilOrders = 0;

                    OTSReplyRec.AllReply[i].TraderListReplyRec.AccountType = AcTypeStrToByte(Convert.ToString(dt.Rows[i]["AccountType"]).TrimEnd());
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessScripMarginReply()
        {
            try
            {
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 5)//improper string
            {
                ReplyStr = "1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TScripMarginReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TScripMarginReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].ScripMarginReplyRec.ScCate = Convert.ToChar(VList[0]);
            OTSReplyRec.AllReply[0].ScripMarginReplyRec.ExchType = Convert.ToChar(VList[1]);
            OTSReplyRec.AllReply[0].ScripMarginReplyRec.Exch = Convert.ToChar(VList[2]);
            OTSReplyRec.AllReply[0].ScripMarginReplyRec.ScCode = Convert.ToInt32(VList[3]);
            OTSReplyRec.AllReply[0].ScripMarginReplyRec.Margin = Convert.ToSingle(VList[4]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------
        // 2015.03.26 Create method to accept reply from database
        private void ProcessScripInfoReply()
        {
            try
            {
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 21)//improper string
                {
                    ReplyStr = "1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TScripInfoReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TScripInfoReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);

                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Exch = Convert.ToChar(VList[0]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.ExchType = Convert.ToChar(VList[1]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Code = Convert.ToInt32(VList[2]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.ISINLength = (byte)VList[3].Length;
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.ISIN = VList[3].ToCharArray();
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.SpanMargin = Convert.ToSingle(VList[4]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.ExpMargin = Convert.ToSingle(VList[5]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.IssuedCapital = Convert.ToSingle(VList[6]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.FreezeQty = Convert.ToInt32(VList[7]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.FaceValue = Convert.ToInt32(VList[8]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.PreOpen = Convert.ToChar(VList[9]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.BookClosureStartDate = Convert.ToInt32(VList[10]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.BookClosureEndDate = Convert.ToInt32(VList[11]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.ExpDate = Convert.ToInt32(VList[12]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Category = Convert.ToChar(VList[13]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.VarMargin = Convert.ToSingle(VList[14]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Dividend = Convert.ToChar(VList[15]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Rights = Convert.ToChar(VList[16]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Bonus = Convert.ToChar(VList[17]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Interest = Convert.ToChar(VList[18]);
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.CreditRatingLength = (byte)VList[19].Length;
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.CreditRating = VList[19].ToCharArray();
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved1 = 0; //byte Reserved1
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved2 = Convert.ToInt32(VList[20]);//0; //int Reserved2
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        // 2015.03.26 Create method to accept reply from database

        // 99 
        private void ProcessVerifyUserReply()
        {
            try
            {
            if (ReplyStr == "")//improper string
            {
                ReplyStr = "1|Unknown Database Error. Please retry";
            }
            OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TVerifyUserReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TVerifyUserReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            //OTSReplyRec.AllReply[0].VerifyUserReplyRec.ReplyType = 99;
            OTSReplyRec.AllReply[0].VerifyUserReplyRec.Verified = Convert.ToByte(ReplyStr);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessChangePasswordReply()
        {
            try
            {
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 2)//improper string
                {
                    ReplyStr = "1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TChangePasswordReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TChangePasswordReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);

                errorSting = new string(OTSReqRec.AllReq.ChgPswdRequestRec.FEUserID).TrimEnd() + "," + OTSReqRec.AllReq.ChgPswdRequestRec.FEUserIDLength + "," + VList[0] + "," + VList[1] + "," + VList[1].Length;

                OTSReplyRec.AllReply[0].ChgPswdReplyRec.FEUserID = new string(OTSReqRec.AllReq.ChgPswdRequestRec.FEUserID).PadRight(15).ToCharArray();
                OTSReplyRec.AllReply[0].ChgPswdReplyRec.FEUserIDLength = OTSReqRec.AllReq.ChgPswdRequestRec.FEUserIDLength;
                OTSReplyRec.AllReply[0].ChgPswdReplyRec.Status = Convert.ToByte(VList[0]);
                OTSReplyRec.AllReply[0].ChgPswdReplyRec.StatusStr = VList[1];
                OTSReplyRec.AllReply[0].ChgPswdReplyRec.StatusStrLength = Convert.ToByte(VList[1].TrimEnd().Length);
            }
            catch (Exception e)
            {
                frm.listBox1.Items.Add(errorSting);
                frm.listBox1.Items.Add(RqStr);
                Form1.ErrorLog(e);
            }          
        }

        //---------------------------------

        private void ProcessHoldingsReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].HoldingsReportReplyRec.ScripName.TrimEnd().Length);
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportReplyRec.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessActivityLog()
        {
            char ReqType;
            int LotSize, Qty;
            string ScName;
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TActivityLogReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'C') //Qty is received in currency Trade report. Converting in LOT.
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);

                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                }
                else if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'E')//bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);

                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                }

                OTSReplyRec.AllReply[i].ActivityLogReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].ActivityLogReplyRec.ScripName.TrimEnd().Length);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);                
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.OldOrderQty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.TradedQty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.PendingQty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.SLTriggered = 'N';
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                OTSReplyRec.AllReply[i].ActivityLogReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[i].ActivityLogReplyRec.Status.TrimEnd().Length);

                ReqType = Convert.ToChar(dt.Rows[i]["RequestType"]);
                if (ReqType == 'S') //SL Triggering
                    OTSReplyRec.AllReply[i].ActivityLogReplyRec.SLTriggered = 'Y';
                else
                    if (ReqType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[i].ActivityLogReplyRec.AtMarket = 'N';
                        OTSReplyRec.AllReply[i].ActivityLogReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessSegmentMaster(DataTable dt)
        {
            try
            {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TAllowSegmentReply());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            if (dt.Rows.Count <= 0) return;

            OTSReplyRec.AllReply[0].SegmentReply.AllowNseEquity = Convert.ToChar(dt.Rows[0]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowNseDeriv = Convert.ToChar(dt.Rows[1]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowAllEquityDerivCombine = Convert.ToChar(dt.Rows[2]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowNseCurr = Convert.ToChar(dt.Rows[3]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowDerivCombine = Convert.ToChar(dt.Rows[4]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowMCX = Convert.ToChar(dt.Rows[6]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowNCDEX = Convert.ToChar(dt.Rows[7]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowCommCombine = Convert.ToChar(dt.Rows[8]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowBseEquity = Convert.ToChar(dt.Rows[9]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowEquityCombine = Convert.ToChar(dt.Rows[10]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowBseCurr = Convert.ToChar(dt.Rows[11]["Allow"]);
            OTSReplyRec.AllReply[0].SegmentReply.AllowCurrCombine = Convert.ToChar(dt.Rows[12]["Allow"]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessMarginReport()
        {
            try
            {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMarginReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.OpeningMargin = 0.00f;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.AdditionalMargin = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.MarginForOpenPos = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.MarginForPendOrder = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.AvailableMargin = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.LedgerBalance = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.T1Short = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.T2Short = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.Withdrawable = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.MFdebit = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.FOBalance = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.FOMgnPayable = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.FOExcessCol = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.HoldingsMargin = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.Junk1 = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRec.Junk2 = 0.00;
            if (dt.Rows.Count <= 0)
            {
                return;
            }
            OTSReplyRec.AllReply[0].MarginReportReplyRec.OpeningMargin = Convert.ToDouble(dt.Rows[0]["OpeningMargin"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.AdditionalMargin = Convert.ToDouble(dt.Rows[0]["Additional"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.MarginForOpenPos = Convert.ToDouble(dt.Rows[0]["Mgn4Position"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.MarginForPendOrder = Convert.ToDouble(dt.Rows[0]["Mgn4PendOrd"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.AvailableMargin = Convert.ToDouble(dt.Rows[0]["AvailableMargin"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.LedgerBalance = Convert.ToDouble(dt.Rows[0]["LedgerBalance"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.T1Short = Convert.ToDouble(dt.Rows[0]["T1Short"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.T2Short = Convert.ToDouble(dt.Rows[0]["T2Short"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.Withdrawable = Convert.ToDouble(dt.Rows[0]["Withdrawable"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.MFdebit = Convert.ToDouble(dt.Rows[0]["MaalFodoDebit"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.FOBalance = Convert.ToDouble(dt.Rows[0]["FOBalance"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.FOMgnPayable = Convert.ToDouble(dt.Rows[0]["FOMarginPayable"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.FOExcessCol = Convert.ToDouble(dt.Rows[0]["FOExcessCol"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRec.HoldingsMargin = Convert.ToDouble(dt.Rows[0]["PDHV"]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessTradeReport()
        {
            string ScName;
            int LotSize, Qty;
            try
            {

            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }           
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTradeReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].TradeReportReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].TradeReportReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].TradeReportReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                OTSReplyRec.AllReply[i].TradeReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].TradeReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].TradeReportReplyRec.ScripName.TrimEnd().Length);
                OTSReplyRec.AllReply[i].TradeReportReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                if (OTSReplyRec.AllReply[i].TradeReportReplyRec.Exch == 'C') //Qty is received in currency Trade report. Converting in LOT.
                {
                    ScName = OTSReplyRec.AllReply[i].TradeReportReplyRec.ScripName.Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (OTSReplyRec.AllReply[i].TradeReportReplyRec.Exch == 'E')//bseCurr
                {
                    ScName = OTSReplyRec.AllReply[i].TradeReportReplyRec.ScripName.Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                OTSReplyRec.AllReply[i].TradeReportReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);               
                OTSReplyRec.AllReply[i].TradeReportReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                OTSReplyRec.AllReply[i].TradeReportReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHANGEORDERID"]);
                OTSReplyRec.AllReply[i].TradeReportReplyRec.ExchangeTradeID = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADEID"]);
                OTSReplyRec.AllReply[i].TradeReportReplyRec.ExchangeTradeTime = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADETIME"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }

        }

        //---------------------------------

        private void ProcessOrderReport()
        {
            int RPos, RCnt, ScripCode,Qty,LotSize;
            char RType, Exch;
            string Str1,ScName;
            Int64 ExchOrderID;
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            RCnt = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                RPos = -1;
                RType = Convert.ToChar(dt.Rows[i]["RequestType"]);
                ExchOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderID"]);
                Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                
                if (Exch == 'C') //Qty is received in currency order report. Converting in LOT.
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0,6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (Exch == 'E')//bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }

                if (RType == 'P') //Place
                {
                    RPos = RCnt;
                    RCnt++;
                    Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                }
                else
                {
                    Str1 = Convert.ToString(dt.Rows[i]["STATUS"]).TrimEnd(TrimChars);
                    if (Str1 == "Rejected" || Str1 == "Rejected By Broker")
                    {
                        continue;
                    }
                    for (int j = 0; j < RCnt; j++)
                    {
                        if (ExchOrderID != OTSReplyRec.AllReply[j].OrderReportReplyRec.ExchangeOrderID) continue;
                        if (Exch != OTSReplyRec.AllReply[j].OrderReportReplyRec.Exch) continue;
                        if (ScripCode != OTSReplyRec.AllReply[j].OrderReportReplyRec.ScripCode) continue;
                        RPos = j;
                        break;
                    }
                    if (RPos == -1)//impossible condition
                    {
                        RPos = RCnt;
                        RCnt++;
                        Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                    }
                }
                if (RType == 'S')   //SL Triggering  
                {
                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.SLTriggered = 'Y';
                }
                else
                    if (RType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRec.AtMarket = 'N';
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
                    else
                        if (RType == 'P')
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripName.TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);                            
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.SLTriggered = 'N';
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Status.TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                        }
                        else
                            if (RType == 'M')
                            {
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.SLTriggered = 'N';
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Status.TrimEnd().Length);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripName.TrimEnd().Length);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                                OTSReplyRec.AllReply[RPos].OrderReportReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                            }
                            else
                                if (RType == 'C' || RType == 'K')
                                {
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.SLTriggered = 'N';
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Status.TrimEnd().Length);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRec.ScripName.TrimEnd().Length);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                                    OTSReplyRec.AllReply[RPos].OrderReportReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                                }
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderReport_CoverOrBracket()     // sumitp 28_jan_22
        {
            int RPos, RCnt, ScripCode, Qty, LotSize;
            char RType, Exch;
            string Str1, ScName;
            Int64 ExchOrderID;
            try
            {
                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                RCnt = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    RPos = -1;
                    RType = Convert.ToChar(dt.Rows[i]["RequestType"]);
                    ExchOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderID"]);
                    Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);

                    if (Exch == 'C') //Qty is received in currency order report. Converting in LOT.
                    {
                        ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }
                    else if (Exch == 'E')//bseCurr
                    {
                        ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }

                    if (RType == 'P') //Place
                    {
                        RPos = RCnt;
                        RCnt++;
                        Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                    }
                    else
                    {
                        Str1 = Convert.ToString(dt.Rows[i]["STATUS"]).TrimEnd(TrimChars);
                        if (Str1 == "Rejected" || Str1 == "Rejected By Broker")
                        {
                            continue;
                        }
                        for (int j = 0; j < RCnt; j++)
                        {
                            if (ExchOrderID != OTSReplyRec.AllReply[j].OrderReportReplyRecCoverOrBracket.ExchangeOrderID) continue;
                            if (Exch != OTSReplyRec.AllReply[j].OrderReportReplyRecCoverOrBracket.Exch) continue;
                            if (ScripCode != OTSReplyRec.AllReply[j].OrderReportReplyRecCoverOrBracket.ScripCode) continue;
                            RPos = j;
                            break;
                        }
                        if (RPos == -1)//impossible condition
                        {
                            RPos = RCnt;
                            RCnt++;
                            Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                        }
                    }
                    if (RType == 'S')   //SL Triggering  
                    {
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.SLTriggered = 'Y';
                    }
                    else
                        if (RType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.AtMarket = 'N';
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
                    else
                            if (RType == 'P')
                    {
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripName.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.SLTriggered = 'N';
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Status.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.EpochTime = Convert.ToInt32(dt.Rows[i]["IDENTIFIER"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID1 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID2 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        //OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderTypeNew = Convert.ToByte(dt.Rows[i]["ORDERTYPE"]);
                    }
                    else
                                if (RType == 'M')
                    {
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.SLTriggered = 'N';
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Status.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripName.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.EpochTime = Convert.ToInt32(dt.Rows[i]["IDENTIFIER"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID1 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID2 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        //OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderTypeNew = Convert.ToByte(dt.Rows[i]["ORDERTYPE"]);
                    }
                    else
                                    if (RType == 'C' || RType == 'K')
                    {
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.SLTriggered = 'N';
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Status.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripName.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.EpochTime = Convert.ToInt32(dt.Rows[i]["IDENTIFIER"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID1 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID2 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        // OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderTypeNew = Convert.ToByte(dt.Rows[i]["ORDERTYPE"]);
                    }
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderReportReplyRecordCoverOrBracket());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }                 //added by Himanshu


        //---------------------------------

        private void ProcessTradeReportCover()    // sumitp 28_jan_22
        {
            string ScName;
            int LotSize, Qty;
            try
            {

                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTradeReportReplyRecNewCover());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].TradeReportReplyRec.ScripName.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                    if (OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.Exch == 'C') //Qty is received in currency Trade report. Converting in LOT.
                    {
                        ScName = OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ScripName.ToString().Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }
                    else if (OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.Exch == 'E')//bseCurr
                    {
                        ScName = OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ScripName.ToString().Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHANGEORDERID"]);
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ExchangeTradeID = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADEID"]);
                    OTSReplyRec.AllReply[i].TradeReportReplyRecNewCover.ExchangeTradeTime = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADETIME"]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }

        }                    //  added by  Sumit

        //---------------------------------

        //---------------------------------

        private void ProcessLoginReply()
        {
            string C1;
            int i;
            try
            {
            OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TLoginReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TLoginReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 3 && VList.Count() != 17 && VList.Count() != 19)  //improper string
            {
                ReplyStr = "1|Unknown Database Error. Please retry|0";
                VList = ReplyStr.Split(splitchar);
            }
            if (VList[0] == "1") //ERROR indication
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[1].Length;
                OTSReplyRec.ErrorString = VList[1];
                OTSReplyRec.AllReply[0].LoginReply.BadLoginCnt = Convert.ToInt32(VList[2]);
            }
            else
            {
                OTSReplyRec.AllReply[0].LoginReply.FEUserIDLength = OTSReqRec.AllReq.LoginRequest.FEUserIDLength;
                OTSReplyRec.AllReply[0].LoginReply.FEUserID = new string(OTSReqRec.AllReq.LoginRequest.FEUserID).PadRight(14).ToCharArray();
                OTSReplyRec.AllReply[0].LoginReply.FEUserType = Convert.ToByte(VList[1]);
                OTSReplyRec.AllReply[0].LoginReply.SystemDateTime = Convert.ToInt32(VList[2]);
                OTSReplyRec.AllReply[0].LoginReply.LastModifiedTime = Convert.ToInt32(VList[3]);
                C1 = VList[4];
                if (C1 == "Y")
                    OTSReplyRec.AllReply[0].LoginReply.AllowNseCash = 1;
                else
                    OTSReplyRec.AllReply[0].LoginReply.AllowNseCash = 0;
                C1 = VList[5];
                if (C1 == "Y")
                    OTSReplyRec.AllReply[0].LoginReply.AllowNseDeriv = 1;
                else
                    OTSReplyRec.AllReply[0].LoginReply.AllowNseDeriv = 0;
                C1 = VList[6];
                if (C1 == "Y")
                    OTSReplyRec.AllReply[0].LoginReply.AllowBseCash = 1;
                else
                    OTSReplyRec.AllReply[0].LoginReply.AllowBseCash = 0;
                OTSReplyRec.AllReply[0].LoginReply.LastAccessedTime = Convert.ToInt32(VList[7]);
                OTSReplyRec.AllReply[0].LoginReply.BranchID = Convert.ToUInt16(VList[8]);
                OTSReplyRec.AllReply[0].LoginReply.ChannelID_NC = Convert.ToInt64(VList[9]);
                OTSReplyRec.AllReply[0].LoginReply.ChannelID_ND = Convert.ToInt64(VList[10]);
                OTSReplyRec.AllReply[0].LoginReply.ChannelID_BC = Convert.ToInt64(VList[11]);

                C1 = VList[12];
                if (C1 == "Y")
                    OTSReplyRec.AllReply[0].LoginReply.AllowTotalBuySellQty = 1;
                else
                    OTSReplyRec.AllReply[0].LoginReply.AllowTotalBuySellQty = 0;
                OTSReplyRec.AllReply[0].LoginReply.AddMsgLength = (byte)VList[13].TrimEnd().Length;
                OTSReplyRec.AllReply[0].LoginReply.AddMsg = VList[13].TrimEnd();
                OTSReplyRec.AllReply[0].LoginReply.AccountType = AcTypeStrToByte(VList[14]);
                OTSReplyRec.AllReply[0].LoginReply.AllowFastFeed = Convert.ToChar(VList[15]);
                OTSReplyRec.AllReply[0].LoginReply.AllowHoldingsMargin= Convert.ToChar(VList[16]);

                if (VList.Count() == 19)
                {
                    i = Convert.ToInt32(VList[17]);
                    OTSReplyRec.AllReply[0].LoginReply.PopupMsg = VList[18].TrimEnd();
                    OTSReplyRec.AllReply[0].LoginReply.PopupMsgLength = Convert.ToByte(VList[18].TrimEnd().Length);
                }
                else
                {
                    OTSReplyRec.AllReply[0].LoginReply.PopupMsg = "";
                    OTSReplyRec.AllReply[0].LoginReply.PopupMsgLength = 0;
                }
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessIlliquidReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TIlliquidReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].IlliquidReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].IlliquidReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessLicUsedReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TLicenceInfoReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserType = Convert.ToByte(dt.Rows[i]["CLIENTTYPE"]);
                OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserCnt = Convert.ToInt32(dt.Rows[i]["CLIENTCNT"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessCliDbNumReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.ReplyCount = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TClientDbNumReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyCount = dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyCount);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].ClientDbNumReplyRec.ClientCode = Convert.ToString(dt.Rows[i]["ClientCode"]).PadRight(15).ToCharArray();
                OTSReplyRec.AllReply[i].ClientDbNumReplyRec.DBNum = Convert.ToByte(dt.Rows[i]["DbNum"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessDerivBannedScripReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivBannedScripReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].DerivBannedScripReplyRec.ULToken = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }

        }

        //---------------------------------

        private void ProcessBseNseMapReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseNseMapRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].BseNseMapReplyRec.BseCode = Convert.ToInt32(dt.Rows[i]["BCODE"]);
                OTSReplyRec.AllReply[i].BseNseMapReplyRec.NseCode = Convert.ToInt32(dt.Rows[i]["NCODE"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessDBDate()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDBDateReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            OTSReplyRec.AllReply[0].DBDateReplyRec.DBDate = (int)dt.Rows[0]["DD"];
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private void ProcessTradeToTradeReport()
        {
            try
            {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTradeToTradeReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].TradeToTradeReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].TradeToTradeReplyRec.ScripCode = (int)(dt.Rows[i]["SCRIPCODE"]);
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------

        private string MakeCommandText()
        {
            string OrderRequesterId, strName, FEUserId;
            string CheckInfo1 = "", CheckInfo2 = "", s1;
            Result = "";
            
                OrderRequesterId = new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength);
                if (OrderRequesterId == "")
                    OrderRequesterId = new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength);

                switch (OTSReqRec.ReqHdr.RequestType)
                {
                    case 1:
                        {
                            if (OTSReqRec.AllReq.LoginRequest.ChkType1InfoLength > 0)
                                CheckInfo1 = new string(OTSReqRec.AllReq.LoginRequest.ChkType1Info).Substring(0, OTSReqRec.AllReq.LoginRequest.ChkType1InfoLength).ToUpper();

                            if (OTSReqRec.AllReq.LoginRequest.ChkType2InfoLength > 0)
                                CheckInfo2 = OTSReqRec.AllReq.LoginRequest.ChkType2Info.Substring(0, OTSReqRec.AllReq.LoginRequest.ChkType2InfoLength).ToUpper();


                            Result = "Exec Proc_LoginUser2 " + "'" + new string(OTSReqRec.AllReq.LoginRequest.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequest.FEUserIDLength)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.Password1).Substring(0, OTSReqRec.AllReq.LoginRequest.Password1Length) + "','"
                                + new string(OTSReqRec.AllReq.LoginRequest.Password2).Substring(0, OTSReqRec.AllReq.LoginRequest.Password2Length)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.MachineID).Substring(0, OTSReqRec.AllReq.LoginRequest.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.LoginRequest.ITS_IP).Substring(0, OTSReqRec.AllReq.LoginRequest.ITS_IPLength)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.ServerIP).Substring(0, OTSReqRec.AllReq.LoginRequest.ServerIPLength) + "','" + new string(OTSReqRec.AllReq.LoginRequest.ClientIP).Substring(0, OTSReqRec.AllReq.LoginRequest.ClientIPLength) + "'," +
                                 OTSReqRec.AllReq.LoginRequest.ChkType1 + ",'" + CheckInfo1 + "'," +
                                  OTSReqRec.AllReq.LoginRequest.ChkType2 + ",'" + CheckInfo2 + "'";
                        }
                        break;
                    case 2:
                        {
                            Result = "Exec Proc_ChangePassword " + "'" + new string(OTSReqRec.AllReq.ChgPswdRequestRec.FEUserID).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.FEUserIDLength)
                                + "','" + new string(OTSReqRec.AllReq.ChgPswdRequestRec.Password1).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.Password1Length) + "','"
                                + new string(OTSReqRec.AllReq.ChgPswdRequestRec.Password2).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.Password2Length)
                                + "','" + new string(OTSReqRec.AllReq.ChgPswdRequestRec.NewPassword1).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.NewPassword1Length)
                                + "','" + new string(OTSReqRec.AllReq.ChgPswdRequestRec.NewPassword2).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.NewPassword2Length)
                               + "','" + new string(OTSReqRec.AllReq.ChgPswdRequestRec.MachineID).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.ChgPswdRequestRec.ITS_IP).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.ITS_IPLength)
                                + "','" + new string(OTSReqRec.AllReq.ChgPswdRequestRec.ServerIP).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.ServerIPLength) + "','" + new string(OTSReqRec.AllReq.ChgPswdRequestRec.ClientIP).Substring(0, OTSReqRec.AllReq.ChgPswdRequestRec.ClientIPLength) + "'";
                        }
                        break;
                    case 3:
                        {
                            if (OTSReqRec.AllReq.LoginRequest.ChkType1InfoLength > 0)
                                CheckInfo1 = new string(OTSReqRec.AllReq.LoginRequest.ChkType1Info).Substring(0, OTSReqRec.AllReq.LoginRequest.ChkType1InfoLength).ToUpper();

                            if (OTSReqRec.AllReq.LoginRequest.ChkType2InfoLength > 0)
                                CheckInfo2 = OTSReqRec.AllReq.LoginRequest.ChkType2Info.Substring(0, OTSReqRec.AllReq.LoginRequest.ChkType2InfoLength).ToUpper();

                            Result = "Exec Proc_LoginUser3 " + "'" + new string(OTSReqRec.AllReq.LoginRequest.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequest.FEUserIDLength)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.Password1).Substring(0, OTSReqRec.AllReq.LoginRequest.Password1Length) + "','"
                                + new string(OTSReqRec.AllReq.LoginRequest.Password2).Substring(0, OTSReqRec.AllReq.LoginRequest.Password2Length)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.MachineID).Substring(0, OTSReqRec.AllReq.LoginRequest.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.LoginRequest.ITS_IP).Substring(0, OTSReqRec.AllReq.LoginRequest.ITS_IPLength)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.ServerIP).Substring(0, OTSReqRec.AllReq.LoginRequest.ServerIPLength) + "','" + new string(OTSReqRec.AllReq.LoginRequest.ClientIP).Substring(0, OTSReqRec.AllReq.LoginRequest.ClientIPLength) + "'," +
                                 OTSReqRec.AllReq.LoginRequest.ChkType1 + ",'" + CheckInfo1 + "'," +
                                  OTSReqRec.AllReq.LoginRequest.ChkType2 + ",'" + CheckInfo2 + "'";
                        } break;
                    case 4:
                        {
                            //if (OTSReqRec.AllReq.LoginRequestForName.ChkType1InfoLength > 0)
                            //   CheckInfo1 = new string(OTSReqRec.AllReq.LoginRequestForName.ChkType1Info).Substring(0, OTSReqRec.AllReq.LoginRequestForName.ChkType1InfoLength).ToUpper();

                            //  if (OTSReqRec.AllReq.LoginRequestForName.ChkType2InfoLength > 0)
                            //    CheckInfo2 = OTSReqRec.AllReq.LoginRequestForName.ChkType2Info.Substring(0, OTSReqRec.AllReq.LoginRequestForName.ChkType2InfoLength).ToUpper();

                            Result = "Exec Proc_LoginUser3WithName " + "'" + new string(OTSReqRec.AllReq.LoginRequestForName.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequestForName.FEUserIDLength) + "'";
                        } break;
                    case 5:
                        {
                            Result = "Exec Proc_TraderList " + "'" + new string(OTSReqRec.AllReq.TraderListRequestRec.FEUserID).Substring(0, OTSReqRec.AllReq.TraderListRequestRec.FEUserIDLength)
                               + "'," + OTSReqRec.AllReq.TraderListRequestRec.FEUserType + "," + OTSReqRec.AllReq.TraderListRequestRec.BranchID + "," + OTSReqRec.AllReq.TraderListRequestRec.ReqUserType;
                        }
                        break;
                    case 7:
                        {
                            Result = "Exec Proc_DealerOrderReport " + "'" + new string(OTSReqRec.AllReq.TraderListRequestRec.FEUserID).Substring(0, OTSReqRec.AllReq.TraderListRequestRec.FEUserIDLength)
                               + "'," + OTSReqRec.AllReq.TraderListRequestRec.FEUserType + "," + OTSReqRec.AllReq.TraderListRequestRec.BranchID + "," + OTSReqRec.AllReq.TraderListRequestRec.ReqUserType;
                        }
                        break;
                    case 8:
                        {
                            Result = "Exec Proc_DealerTradeReport " + "'" + new string(OTSReqRec.AllReq.TraderListRequestRec.FEUserID).Substring(0, OTSReqRec.AllReq.TraderListRequestRec.FEUserIDLength)
                               + "'," + OTSReqRec.AllReq.TraderListRequestRec.FEUserType + "," + OTSReqRec.AllReq.TraderListRequestRec.BranchID + "," + OTSReqRec.AllReq.TraderListRequestRec.ReqUserType;
                        }
                        break;
                    case 6:
                        {
                            Result = "Exec Proc_VerifyTrader " + "'" + new string(OTSReqRec.AllReq.VerifyTraderRequestRec.FEUserID).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.FEUserIDLength) + "'," + OTSReqRec.AllReq.VerifyTraderRequestRec.FEUserType + "," + OTSReqRec.AllReq.VerifyTraderRequestRec.BranchID + ",'" +
                                new string(OTSReqRec.AllReq.VerifyTraderRequestRec.TraderID).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.TraderIDLength) + "','" + new string(OTSReqRec.AllReq.VerifyTraderRequestRec.MachineID).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.MachineIDLength) + "','" +
                                new string(OTSReqRec.AllReq.VerifyTraderRequestRec.ITS_IP).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.ITS_IPLength) + "','" + new string(OTSReqRec.AllReq.VerifyTraderRequestRec.ServerIP).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.ServerIPLength) + "','" +
                                 new string(OTSReqRec.AllReq.VerifyTraderRequestRec.ClientIP).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.ClientIPLength) + "'";
                        }
                        break;
                    case 10: //Pending
                        {
                            Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                                OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripCode + ",'" +
                                new string(OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripName).Substring(0, OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripNameLength) + "','" + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.BuySell + "'," +
                                 OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Qty + ",'" + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.AtMarket + "'," + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Rate + ",'" + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.WithSL + "'," +
                                  OTSReqRec.AllReq.EquityOrderPlaceRequestRec.TriggerRate + "," + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.DisclosedQty;
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 15:
                        {
                            Result = "Exec Proc_ConvertAH2LiveOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," + OrderRequesterId + ",'" +
                               OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ScripCode + ",'" +
                               new string(OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ScripName).Substring(0, OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ScripNameLength) + "','" + OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.BuySell + "'," +
                                OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.Qty + "," + OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.Rate + ",'" + OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.WithSL + "'," +
                                 OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.TriggerRate + "," + OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.ExchangeOrderID + "," + OTSReqRec.AllReq.AHEquityOrderPlaceRequestRec.DisclosedQty;
                        }
                        break;
                    case 20:
                        {
                            if (OTSReqRec.AllReq.EquityOrderModifyRequestRec.AHStatus != 'Y')
                                OTSReqRec.AllReq.EquityOrderModifyRequestRec.AHStatus = 'N';

                            Result = "Exec Proc_ModifyEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.EquityOrderModifyRequestRec.Exch + "','" + OTSReqRec.AllReq.EquityOrderModifyRequestRec.ExchType + "'," + OTSReqRec.AllReq.EquityOrderModifyRequestRec.ScripCode + ",'" +
                               new string(OTSReqRec.AllReq.EquityOrderModifyRequestRec.ScripName).Substring(0, OTSReqRec.AllReq.EquityOrderModifyRequestRec.ScripNameLength) + "','" + OTSReqRec.AllReq.EquityOrderModifyRequestRec.BuySell + "'," +
                                OTSReqRec.AllReq.EquityOrderModifyRequestRec.Qty + ",'" + OTSReqRec.AllReq.EquityOrderModifyRequestRec.AtMarket + "'," + OTSReqRec.AllReq.EquityOrderModifyRequestRec.Rate + ",'" +
                                 OTSReqRec.AllReq.EquityOrderModifyRequestRec.WithSL + "'," + OTSReqRec.AllReq.EquityOrderModifyRequestRec.TriggerRate + "," + OTSReqRec.AllReq.EquityOrderModifyRequestRec.OldQty + "," +
                                 OTSReqRec.AllReq.EquityOrderModifyRequestRec.TradedQty + "," + OTSReqRec.AllReq.EquityOrderModifyRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.EquityOrderModifyRequestRec.AHStatus + "'," +
                                 OTSReqRec.AllReq.EquityOrderModifyRequestRec.DisclosedQty;
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 30:
                        {
                            if (OTSReqRec.AllReq.EquityOrderCancelRequestRec.AHStatus != 'Y')
                                OTSReqRec.AllReq.EquityOrderCancelRequestRec.AHStatus = 'N';

                            Result = "Exec Proc_CancelEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                              OTSReqRec.AllReq.EquityOrderCancelRequestRec.Exch + "','" + OTSReqRec.AllReq.EquityOrderCancelRequestRec.ExchType + "'," + OTSReqRec.AllReq.EquityOrderCancelRequestRec.ScripCode + ",'" +
                              new string(OTSReqRec.AllReq.EquityOrderCancelRequestRec.ScripName).Substring(0, OTSReqRec.AllReq.EquityOrderCancelRequestRec.ScripNameLength) + "','" + OTSReqRec.AllReq.EquityOrderCancelRequestRec.BuySell + "'," +
                               OTSReqRec.AllReq.EquityOrderCancelRequestRec.Qty + ",'" + OTSReqRec.AllReq.EquityOrderCancelRequestRec.AtMarket + "'," + OTSReqRec.AllReq.EquityOrderCancelRequestRec.Rate + ",'" + OTSReqRec.AllReq.EquityOrderCancelRequestRec.WithSL + "'," +
                                OTSReqRec.AllReq.EquityOrderCancelRequestRec.TriggerRate + "," + OTSReqRec.AllReq.EquityOrderCancelRequestRec.TradedQty + "," + OTSReqRec.AllReq.EquityOrderCancelRequestRec.ExchangeOrderID + ",'" + OTSReqRec.AllReq.EquityOrderCancelRequestRec.AHStatus + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 41:
                        {
                            Result = "Exec Proc_GetScripMargin " + "'" + OTSReqRec.AllReq.ScripMarginRequestRec.Exch + "'," + OTSReqRec.AllReq.ScripMarginRequestRec.ScCode + ",'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;

                    // 2015.03.26
                    case Constants.OTS_SECURITY_INFO:
                        {
                            Result = "Exec Proc_GetSecurityInfo " + "'" + OTSReqRec.AllReq.ScripInfoRequestRec.Exch + "'," + OTSReqRec.AllReq.ScripInfoRequestRec.ScCode;
                        }
                        break;
                    // 2015.03.26
                    case 61:
                        {
                            Result = "Exec Proc_EquityTradeReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                OTSReqRec.AllReq.TradeReportRequestRec.StartTime + "," + OTSReqRec.AllReq.TradeReportRequestRec.EndTime;
                        }
                        break;
                    case 62:
                        {
                            Result = "Exec Proc_EquityOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                OTSReqRec.AllReq.OrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.OrderReportRequestRec.EndTime;
                        }
                        break;
                case 78:            // himanshu  // Cover  25-nov-21
                    {
                        Result = "Exec Proc_EquityOrderReport_CO " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                            OTSReqRec.AllReq.OrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.OrderReportRequestRec.EndTime;
                    }
                    break;
                case 64:
                        {
                            Result = "Exec Proc_EquityOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                 OTSReqRec.AllReq.ActivityLogRequestRec.StartTime + "," + OTSReqRec.AllReq.ActivityLogRequestRec.EndTime;
                        }
                        break;
                    case 65:
                        {
                            Result = "Exec Proc_AfterHoursOrderReport"; //No parameters
                        }
                        break;
                    case 66:
                        {
                            if (DBNo == 1)
                                Result = "Select ClientType as CLIENTTYPE,Count(*) as CLIENTCNT from Clients Where ClientCode < " + "'" + TraderChar + "'" + " group by ClientType Order by ClientType";
                            else
                                Result = "Select ClientType as CLIENTTYPE,Count(*) as CLIENTCNT from Clients Where ClientCode >= " + "'" + TraderChar + "'" + " group by ClientType Order by ClientType";
                        }
                        break;
                    case 68:
                        {
                            Result = "select ClientCode,DbNum from Clients Where ClientCode like " + "'" + (char)OTSReqRec.AllReq.CliDBNumRequestRec.DummyByte + "%" + "'" + " Order by ClientCode";
                        }
                        break;
                    case 69:
                        {
                            Result = "Exec Proc_HoldingsReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;
                    case 70:
                        {
                            Result = "Exec Proc_GetHoldingsMargin " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;
                    case 218:
                        {
                            Result = "Exec Proc_GetHoldingsMargin " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;
                    case 73:
                        {
                            Result = "select cast(floor(cast(getdate() as float)) + 2 as int) as DD";
                        }
                        break;
                    case 79:
                        {
                            Result = "Exec Proc_GetClientPos " + "'" + new string(OTSReqRec.AllReq.ClientPositionRequestRec.ClientCode).Substring(0, OTSReqRec.AllReq.ClientPositionRequestRec.ClientCodeLength) + "'," +
                                OTSReqRec.AllReq.ClientPositionRequestRec.ScripCode;
                        }
                        break;
                    case 80:
                        {
                            Result = "Exec Proc_GetTradeForTradeScrips"; //No parameters
                        }
                        break;
                    case 81:
                        {
                            Result = "Exec Proc_GetBseNseMap"; //No parameters
                        }
                        break;
                    case 82:
                        {
                            Result = "Exec Proc_NseFnOBannedScripReport"; //No parameters
                        }
                        break;
                    case 83:
                        {
                            Result = "Exec Proc_GetAdhoc " + "'" + new string(OTSReqRec.AllReq.AdhocReportRequestRec.AdminCode).Substring(0, OTSReqRec.AllReq.AdhocReportRequestRec.AdminCodeLength)
                               + "','" + new string(OTSReqRec.AllReq.AdhocReportRequestRec.ClientCode).Substring(0, OTSReqRec.AllReq.AdhocReportRequestRec.ClientCodeLength) + "'";
                        }
                        break;
                    case 84:
                        {
                            Result = "Exec Proc_ModifyAdhoc " + "'" + new string(OTSReqRec.AllReq.AdhocModifyRequestRec.ClientCode).Substring(0, OTSReqRec.AllReq.AdhocModifyRequestRec.ClientCodeLength)
                               + "'," + OTSReqRec.AllReq.AdhocModifyRequestRec.AvlbMgn + "," + OTSReqRec.AllReq.AdhocModifyRequestRec.OldAdhoc + "," + OTSReqRec.AllReq.AdhocModifyRequestRec.NewAdhoc;
                        }
                        break;
                    case 86:
                        {
                            Result = "Exec Proc_GetMaalFodoClients " + OTSReqRec.AllReq.MFClientsReportRequestRec.ClientType + "," + OTSReqRec.AllReq.MFClientsReportRequestRec.MinimumDebit;
                            MFDebitAmt = OTSReqRec.AllReq.MFClientsReportRequestRec.MinimumDebit;
                        }
                        break;
                    case 87:
                        {
                            Result = "Exec Proc_GetMaalFodoClientHoldings " + OTSReqRec.AllReq.MFClientHoldingReportRequestRec.ClientType + "," + OTSReqRec.AllReq.MFClientHoldingReportRequestRec.MinimumDebit;
                        }
                        break;
                    case 89:
                        {
                            Result = "Exec Proc_IlliquidReport"; //No parameters
                        }
                        break;
                    case Constants.OTS_VERIFY_USER: //99
                        {
                            Result = "Exec Proc_GetUserVerification" + "'" + OTSReqRec.AllReq.VerifyUserRequestRec.UserId + "','" + OTSReqRec.AllReq.VerifyUserRequestRec.Pan + "','" + OTSReqRec.AllReq.VerifyUserRequestRec.Dob + "'";
                        }
                        break;
                    case 110:
                        {
                            //DisclosedQty not allowed in f&o but we use same sql proc for eq and fo. so we send 0
                            strName = MakeDerivDesc(OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Exch, OTSReqRec.AllReq.DerivOrderPlaceRequestRec.DerivInfo);

                            Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.BuySell + "'," + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Qty + ",'" + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.AtMarket + "'," + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Rate
                               + ",'" + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.WithSL + "'," + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.TriggerRate + "," + 0;
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 115:
                        {
                            strName = MakeDerivDesc(OTSReqRec.AllReq.DerivOrderModifyRequestRec.Exch, OTSReqRec.AllReq.DerivOrderModifyRequestRec.DerivInfo);

                            Result = "Exec Proc_ConvertAH2LiveOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.ScripCode + ",'" +
                            strName + "','" + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.BuySell + "'," + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Qty + "," +
                            OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Rate + ",'" + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.WithSL + "'," +
                              OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.TriggerRate + "," + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.ExchangeOrderID;
                        }
                        break;
                    case 120:
                        {
                            if (OTSReqRec.AllReq.DerivOrderModifyRequestRec.AHStatus != 'Y')
                                OTSReqRec.AllReq.DerivOrderModifyRequestRec.AHStatus = 'N';

                            strName = MakeDerivDesc(OTSReqRec.AllReq.DerivOrderModifyRequestRec.Exch, OTSReqRec.AllReq.DerivOrderModifyRequestRec.DerivInfo);

                            Result = "Exec Proc_ModifyEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.DerivOrderModifyRequestRec.Exch + "','" + OTSReqRec.AllReq.DerivOrderModifyRequestRec.ExchType + "'," + OTSReqRec.AllReq.DerivOrderModifyRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.DerivOrderModifyRequestRec.BuySell + "'," + OTSReqRec.AllReq.DerivOrderModifyRequestRec.Qty + ",'" + OTSReqRec.AllReq.DerivOrderModifyRequestRec.AtMarket + "'," +
                               OTSReqRec.AllReq.DerivOrderModifyRequestRec.Rate + ",'" + OTSReqRec.AllReq.DerivOrderModifyRequestRec.WithSL + "'," +
                                 OTSReqRec.AllReq.DerivOrderModifyRequestRec.TriggerRate + "," + OTSReqRec.AllReq.DerivOrderModifyRequestRec.OldQty + "," + OTSReqRec.AllReq.DerivOrderModifyRequestRec.TradedQty + "," +
                                 OTSReqRec.AllReq.DerivOrderModifyRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.DerivOrderModifyRequestRec.AHStatus + "'," + 0;
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 130:
                        {
                            if (OTSReqRec.AllReq.DerivOrderCancelRequestRec.AHStatus != 'Y')
                                OTSReqRec.AllReq.DerivOrderCancelRequestRec.AHStatus = 'N';

                            strName = MakeDerivDesc(OTSReqRec.AllReq.DerivOrderModifyRequestRec.Exch, OTSReqRec.AllReq.DerivOrderCancelRequestRec.DerivInfo);

                            Result = "Exec Proc_CancelEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.DerivOrderCancelRequestRec.Exch + "','" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.ExchType + "'," + OTSReqRec.AllReq.DerivOrderCancelRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.BuySell + "'," + OTSReqRec.AllReq.DerivOrderCancelRequestRec.Qty + ",'" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.AtMarket + "'," +
                               OTSReqRec.AllReq.DerivOrderCancelRequestRec.Rate + ",'" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.WithSL + "'," +
                                 OTSReqRec.AllReq.DerivOrderCancelRequestRec.TriggerRate + "," + OTSReqRec.AllReq.DerivOrderCancelRequestRec.TradedQty + "," +
                                 OTSReqRec.AllReq.DerivOrderCancelRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.AHStatus + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 200:
                        {
                            Result = "Exec Proc_GetMarginReport2 " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;
                    case 248:
                        {
                            Result = "Select * from SegmentMaster";
                        }
                        break;
                    case 251:
                        {
                            Result = "Exec Proc_LinkFailure " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "'," + OTSReqRec.AllReq.LinkFailRec.RequestType + "," +
                                OTSReqRec.AllReq.LinkFailRec.BrokerOrderID + "," + OTSReqRec.AllReq.LinkFailRec.ErrorLevel + ",'" + OTSReqRec.AllReq.LinkFailRec.ErrorMsg.Substring(0, OTSReqRec.AllReq.LinkFailRec.ErrorMsgLength) + "'";
                        }
                        break;
                    case 252://Client Disconnected
                        {
                            FEUserId = new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength);
                            if (FEUserId == "") FEUserId = new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength);

                            Result = "Exec Proc_LogoutUser " + "'" + FEUserId + "','" + new string(OTSReqRec.AllReq.LogoutRequest.MachineID).Substring(0, OTSReqRec.AllReq.LogoutRequest.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.LogoutRequest.ITS_IP).Substring(0, OTSReqRec.AllReq.LogoutRequest.ITS_IPLength) + "','" +
                                new string(OTSReqRec.AllReq.LogoutRequest.ServerIP).Substring(0, OTSReqRec.AllReq.LogoutRequest.ServerIPLength) + "','" +
                                new string(OTSReqRec.AllReq.LogoutRequest.ClientIP).Substring(0, OTSReqRec.AllReq.LogoutRequest.ClientIPLength) + "'";
                        }
                        break;
                case 247://Client Disconnected
                    {
                        FEUserId = new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength);
                        if (FEUserId == "") FEUserId = new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength);

                        Result = "Exec Proc_LogoutUser " + "'" + FEUserId + "','" + new string(OTSReqRec.AllReq.LogoutRequest_2FA.MachineID).Substring(0, OTSReqRec.AllReq.LogoutRequest_2FA.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.LogoutRequest_2FA.ITS_IP).Substring(0, OTSReqRec.AllReq.LogoutRequest_2FA.ITS_IPLength) + "','" +
                            new string(OTSReqRec.AllReq.LogoutRequest_2FA.ServerIP).Substring(0, OTSReqRec.AllReq.LogoutRequest_2FA.ServerIPLength) + "','" +
                            new string(OTSReqRec.AllReq.LogoutRequest_2FA.ClientIP).Substring(0, OTSReqRec.AllReq.LogoutRequest_2FA.ClientIPLength) + "'";
                    }
                    break;


                #region Currency Derive
                case 211:
                        {
                            strName = MakeDerivDesc(OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Exch, OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo);

                            Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.BuySell + "'," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Qty + ",'" + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.AtMarket + "'," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Rate
                               + ",'" + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.WithSL + "'," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.TriggerRate + "," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.DisclosedQty;

                        }
                        break;
                    case 221:
                        {
                            if (OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.AHStatus != 'Y')
                                OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.AHStatus = 'N';

                            strName = MakeDerivDesc(OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Exch, OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.DerivInfo);

                            Result = "Exec Proc_ModifyEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Exch + "','" + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.ExchType + "'," + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.BuySell + "'," + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Qty + ",'" + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.AtMarket + "'," +
                               OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Rate + ",'" + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.WithSL + "'," +
                                 OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.TriggerRate + "," + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.OldQty + "," + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.TradedQty + "," +
                                 OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.AHStatus + "'," + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.DisclosedQty;
                        }
                        break;
                    case 231:
                        {
                            if (OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.AHStatus != 'Y')
                                OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.AHStatus = 'N';

                            strName = MakeDerivDesc(OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Exch, OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.DerivInfo);

                            Result = "Exec Proc_CancelEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Exch + "','" + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.ExchType + "'," + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.BuySell + "'," + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Qty + ",'" + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.AtMarket + "'," +
                               OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Rate + ",'" + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.WithSL + "'," +
                                 OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.TriggerRate + "," + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.TradedQty + "," +
                                 OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.AHStatus + "'";
                        }
                        break;
                    #endregion
                }
                s1 = Result.Replace("''", "' '");
                if (s1 != Result)
                    Result = s1;
                RequestStr = Result;
                RqStr = RequestStr;
                return Result;

            
        }

        //---------------------------------

        private string MakeDerivDesc(char Exch,ITInteractiveStructures.TDerivInfoRecord DerivInfo)
        {
            string Result, ExDt;

         
            ExDt = ExchStartDate.AddSeconds(DerivInfo.ExpiryDate).ToString("dd MMM yyyy");
            Result = new string(DerivInfo.Symbol).Substring(0, DerivInfo.SymbolLength) + " "+ ExDt;
            if (DerivInfo.CPType > 0)
            {
                if (Exch == 'C')
                    Result = Result + " " + ArrayOptionType[DerivInfo.CPType] + " " + DerivInfo.StrikePrice.ToString("0.0000");
                else
                    Result = Result + " " + ArrayOptionType[DerivInfo.CPType] + " " + DerivInfo.StrikePrice.ToString("0.00");
            }
            return Result;
            
        }

        //---------------------------------

        private int GetCurrTimeInSecsFrom1980()
        {
            int Result;
            Result = Convert.ToInt32(Math.Round((DateTime.Now - ExchStartDate).TotalSeconds));
            return Result;
        }

        //---------------------------------

        private byte AcTypeStrToByte(string AcType)
        {
            byte Result = 0;
            AcType = AcType.ToUpper().TrimEnd(TrimChars);
            if (AcType == "OWN")
                Result = 1;
            else
                if (AcType == "RI")
                    Result = 2;
                else
                    if (AcType == "BANK")
                        Result = 3;
                    else
                        if (AcType == "DFI")
                            Result = 4;
                        else
                            if (AcType == "MF")
                                Result = 5;
                            else
                                if (AcType == "FII")
                                    Result = 6;
                                else
                                    if (AcType == "INS")
                                        Result = 7;
                                    else
                                        if (AcType == "NRI")
                                            Result = 8;
            return Result;
           
        }
    }

    public class DBResult
    {
        public int DBNo, ThreadNumber, ExType;
        public ITInteractiveStructures.TOTSReplyRecord OTSReplyRec;
        public ITInteractiveStructures.TOTSRequestRecord OTSReqRec;
        public char AfterHours;
        public string RqStr, ExStr;
        public bool DeadlockRetry;
        public long ThreadStartTick;


        public DBResult(int DBNo1, int ThreadNumber1, int ExType1, ITInteractiveStructures.TOTSReplyRecord OTSReplyRec1, ITInteractiveStructures.TOTSRequestRecord OTSReqRec1, char AfterHours1, string RqStr, string ExStr, bool DeadlockRetry, long ThreadStartTick)
        {
            DBNo = DBNo1;
            ThreadNumber = ThreadNumber1;
            ExType = ExType1;
            OTSReplyRec = OTSReplyRec1;
            OTSReqRec = OTSReqRec1;
            AfterHours = AfterHours1;
            this.RqStr = RqStr;
            this.ExStr = ExStr;
            this.DeadlockRetry = DeadlockRetry;
            this.ThreadStartTick = ThreadStartTick;
        }

    }
}
