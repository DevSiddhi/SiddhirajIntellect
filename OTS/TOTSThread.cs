using CrashReporterDotNET.com.drdump;
//using System.Diagnostics;
//using System.Text.Json;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OTS
{
    public enum TThreadStatus
    {
        Running,
        Suspended
    };

    public class TOTSThread
    {
        #region Variable Declaration
        ITInteractiveStructures.TOTSRequestRecord OTSReqRec;
        Queue<ITInteractiveStructures.TOTSRequestRecord> _tasks;
        ThreadTerminationActionNew _threadTermination;
        EventWaitHandle _wh;
        Control Mainfrm;
        Form1 frm;
        OleDbCommand cmd;
        OleDbCommand cmd2;   // sumitp 28_jan_22
        OleDbConnection conn;
        OleDbDataAdapter da;
        readonly object _locker;
        int ExType, MFDebitAmt;
        string Result;
        string ResultCo; // sumitp 28_jan_22
        Thread[] t;
        public int DBNo, ThreadNumber;
        public TThreadStatus Status;
        public ushort MaxDeadlockRetryCnt; public char AfterHours, TraderChar = 'Z';
        public string RqStr, ExStr, ExStr1, RequestStr, ExceptionStr;
        public bool DeadlockRetry, TimeOutFlag; public long ThreadStartTick;
        public ITInteractiveStructures.TOTSReplyRecord OTSReplyRec;
        public ITInteractiveStructures.TAllRequestNewRecord AllReq;
        char[] TrimChars = new char[2] { ' ', '\0' };
        char[] splitchar = new char[1] { '|' };
        string[] ArrayOptionType = new string[5] { "XX", "CA", "PA", "CE", "PE" }, VList;
        DateTime ExchStartDate = new DateTime(1980, 1, 1, 0, 0, 0);
        DateTime ExchMcxStartDate = new DateTime(1970, 1, 1, 0, 0, 0);//McxTrading
        int BrokerOFSOrderIdCount = 1000000;

        Random RandomGenerator = new Random();


        #endregion

        public TOTSThread(int WorkerCount, string UdlFileName, Control Invoker, int No)
        {
            DBNo = No;
            conn = new OleDbConnection("File Name = " + UdlFileName);
            cmd = new OleDbCommand();
            da = new OleDbDataAdapter(cmd);
            cmd.Connection = conn;
            Mainfrm = Invoker;
            frm = (Form1)Invoker;
            //_threadTermination = ThreadTerminationEvent;
            Status = TThreadStatus.Suspended;

            try
            {
                conn.Open();
            }
            catch (Exception oe)
            {
                string str1 = DateTime.Now.ToString("hh:mm:ss") + " : " + "Cannot Connect to database" + ", please check the " + UdlFileName + " file";
                Mainfrm.Invoke(frm.ConnectionToDatabase, str1);
            }
            _wh = new AutoResetEvent(false);
            _locker = new object();
            _tasks = new Queue<ITInteractiveStructures.TOTSRequestRecord>();
            t = new Thread[WorkerCount];

            for (int i = 0; i < WorkerCount; i++)
                (t[i] = new Thread(Execute)).Start();
        }

       // public delegate void Invokedprinter(string data);

       // public void invoker(string data)
       // {
          //  frm.listBox1.Items.Add(data);
       // }

        public void EnqueueTask(ITInteractiveStructures.TOTSRequestRecord task)
        {
            lock (_locker)
            {
                _tasks.Enqueue(task);
                Status = TThreadStatus.Running;
                _wh.Set();
            }
        }

        public void Execute()
        {
            string ReplyStr;
            while (true)
            {
                lock (_locker)
                    if (_tasks.Count > 0)
                    {
                        OTSReqRec = _tasks.Dequeue();
                        if (OTSReqRec.ReqHdr.RequestType == 0) return;
                    }

                if (OTSReqRec.ReqHdr.RequestType != 0)
                {
                    cmd.CommandText = "";
                    ExceptionStr = "";
                    RequestStr = "";
                    ReplyStr = "";
                    ExType = 0;
                    DataSet ds = new DataSet();
                    DataTable dt = new DataTable();

                    if (TimeOutFlag)
                        cmd.CommandTimeout = 60;
                    else
                        cmd.CommandTimeout = 30;

                    try
                    {
                        switch (OTSReqRec.ReqHdr.RequestType)
                        {
                            case 1:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessLoginReply(ReplyStr);
                                }
                                break;
                            case 2:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessChangePasswordReply(ReplyStr);
                                }
                                break;
                            case 3:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]); 
                                    ProcessLoginReplyNew(ReplyStr);
                                }
                                break;
                            case 85:
                                {
                                    //Form1.Log2FA("Request : " + JsonSerializer.Serialize(OTSReqRec.AllReq.LoginRequestTwoFactor));
                                    cmd.CommandText = MakeCommandText();
                                    Form1.Log2FA("Query : " + cmd.CommandText);
                                    ds.Clear();
                                    da.Fill(ds);
                                    dt = ds.Tables[0];
                                    //ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessTwoFactorLoginReply(dt);
                                }
                                break;
                            case 4:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessLoginReplyWithName(ReplyStr);
                                }
                                break;
                            case 5://Trader List - Admin only
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    //ProcessTraderList(dt);
                                    ProcessTraderListNew(dt);
                                }
                                break;
                            case 7://Trader List - Admin only
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    //ProcessTraderList(dt);
                                    ProcessDealerOrderReport(dt);
                                }
                                break;
                            case 8://Trader List - Admin only
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    //ProcessTraderList(dt);
                                    ProcessDealerTrdaesReport(dt);
                                }
                                break;
                            case 6: //Trader List - Admin only
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessVerifyTrader(ReplyStr);
                                }
                                break;
                            case 10://Place Equity Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessEquityPlaceOrderRequest(ReplyStr);
                                }
                                break;
                            case 14:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["AllowedIVR"]);
                                    GetDealerDetailsForIVR(ReplyStr);
                                }
                                break;
                            case 15: //Place Equity Order After Hours
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessAHEquityPlaceOrderRequest(ReplyStr);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_PLACE_REQ:
                                 {
                                    //Debug.WriteLine("Place");
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessOrderTypePlaceOrderRequest(ReplyStr); //29.12.14
                                 }
                                break;
                            case Constants.OTS_ORDERTYPE_PLACE_REQ_COVER:   //171     // sumitp 28_jan_22
                                {
                                    //Debug.WriteLine("Place");
                                    //string newtemp[] = 
                                    string tempss = MakeCommandText();
                                    string[] ss = tempss.Split(';');
                                    string Proc = ss[0];

                                    cmd.CommandText = Proc;// MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessCoverOrderTypePlaceOrderRequestLeg1(ReplyStr); //29.12.14

                                    Proc = ss[1];

                                    char BuySell2 = OTSReqRec.AllReq.PlaceCoverOrderReqRec.BuySell == 'B' ? 'S' : 'B';
                                    Single Rate = OTSReqRec.AllReq.PlaceCoverOrderReqRec.SLRateCO;

                                    cmd.CommandText = Proc;// MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[1]["Column1"]);
                                    ProcessCoverOrderTypePlaceOrderRequestLeg2(ReplyStr, BuySell2, Rate); //29.12.14

                                }
                                break;
                            case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                  //  if (frm.listBox1.InvokeRequired)
                                 //   {
                                  //      Invokedprinter temp = new Invokedprinter(invoker);
                                     //   frm.listBox1.BeginInvoke(temp, ReplyStr);
                                    //}
                                    ProcessOrderTypeModifyOrderRequest(ReplyStr); //30.12.14
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                                {
                                  //  Debug.WriteLine("19");
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                   // Debug.WriteLine(ReplyStr);
                                    ProcessOrderTypeCancelOrderRequest(ReplyStr); //31.12.14      
                                }
                                break;
                            case 20://Modify Equity Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessEquityModifyOrderRequest(ReplyStr);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_DERIVCONVERSION:
                                {

                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessOrderTypeConversionWithExpiryRequest(ReplyStr);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_CONVERSION: //14.01.15
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessOrderTypeConversionRequest(ReplyStr);
                                }
                                break;
                            case 30://Cancel Equity Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);//BrokerOrderID
                                    ProcessEquityCancelOrderRequest(ReplyStr);
                                }
                                break;
                            case 41:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessScripMarginReply(ReplyStr);
                                }
                                break;

                            // 2015.03.26 assignmnt of thread
                            case Constants.OTS_SECURITY_INFO:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessScripInfoReply(ReplyStr);
                                }
                                break;
                            case Constants.OTS_SCRIP_MARGIN:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessScripMarginReplyNew(ReplyStr);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ://43 BseMf Krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessBseMFSIPRegistrationRequest(ReplyStr);
                                }break;
                            case Constants.OTS_MANDATE://43 BseMf Krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessBseMFMandate(ReplyStr);
                                } break;
                            case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ://43 BseMf Krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessBseMFXSIPRegistrationRequest(ReplyStr);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessBseMFSIPCancelOrderRequest(ReplyStr);
                                }break;
                            case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessBseMFPlaceOrderRequest(ReplyStr);
                                } break;
                            case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44 BseMf
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessBseMFModifyOrderRequest(ReplyStr);
                                } break;
                            case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45 BseMf
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessBseMFCancelOrderRequest(ReplyStr);
                                } break;
                            // 2015.03.26 assignmnt of thread
                            //case 61://Trade Report
                            //    {
                            //        cmd.CommandText = MakeCommandText();
                            //        da.Fill(dt);
                            //        ProcessTradeReport(dt);
                            //    }
                            //    break;
                            //case 62: //Order Report
                            //    {
                            //        cmd.CommandText = MakeCommandText();
                            //        da.Fill(dt);
                            //        ProcessOrderReport(dt);
                            //    }
                            //    break;
                            case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeTradeReport(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW:    //Added on 18-Apr-2018 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeTradeReportNew(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142:    //Added on 5-Apr-2018 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeTradeReportNew142(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER:             // 58 sumit  // sumitp 28_jan_22
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeTradeReportNewCover(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeOrdersReport(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW: //Added on 18-Apr-2018 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeOrdersReportNew(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_COVER_BRACKET:       //Added on 25-Nov-2021 - Himanshu    // sumitp 28_jan_22
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    //ProcessOrderTypeOrdersReport(dt);
                                    ProcessOrderTypeOrdersReportNew_CoverOrBracket(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR: //Added on 09-May-2019 - Ashish
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeOrdersReportNewWithRejOrder(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141: //Added on 5-Apr-2018 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeOrdersReportNew141(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_EXERCISE_REPORTS_REC://Option exercise
                                {
                                    cmd.CommandText = MakeCommandText();
                                     da.Fill(dt);
                                    ProcessExerciseReport(dt);
                                }
                                break;
                            //case 64://Activity Log
                            //    {
                            //        cmd.CommandText = MakeCommandText();
                            //        da.Fill(dt);
                            //        ProcessActivityLog(dt);
                            //    }
                            //    break;
                            case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeActivityLog(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW: // 24.09.16 
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeNewActivityLogNew(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new: // sumit  21_jan_22 single activity log
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeNewActivityLogNew_single(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW:  //Added on 18-Apr-2018 - Rohan 
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOrderTypeActivityLogNew(dt);
                                }
                                break;
                            case 66://Licence Used Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessLicUsedReport(dt);
                                }
                                break;
                            case 68://Client,DbNum Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessCliDbNumReport(dt);
                                    TimeOutFlag = false;
                                }
                                break;
                            case 69:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReport(dt);
                                }
                                break;

                            case 70://Holdings Valuation Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsValuationReport(dt);
                                }
                                break;
                            case 218://Holdings Valuation Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsValuationReport(dt);
                                }
                                break;
                            case Constants.OTS_HOLDING_VALUATION_REPORT_NEW://Holdings Valuation Report New
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsValuationReportNew(dt);
                                }
                                break;
                            case 73: //Data base Date
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessDBDate(dt);
                                }
                                break;
                            case 74://Holding with collatral
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReportWithCollatral(dt);
                                }
                                break;
                            case 75:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessAllClientHoldingsReport(dt);
                                } break;
                            case 76:    //Added on 08-Nov-2016 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessAllClientHoldingsEquityReport(dt);
                                } break;
                            case 79: //Client Position Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessClientPositionReport(dt);
                                }
                                break;
                            case 80: //Trade To Trade  Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessTradeToTradeReport(dt);
                                }
                                break;
                            case 160:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessSurveillanceIndicatorMessages(dt);
                                }
                                break;
                            case 161:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessSurveillanceIndicatorMessagesBse(dt);
                                }
                                break;
                            case 81: //Bse Nse Map Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessBseNseMapReport(dt);
                                }
                                break;
                            case 82://Deriv Banned Scrip Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessDerivBannedScripReport(dt);
                                }
                                break;
                            case 83://Adhoc Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessAdhocReport(dt);
                                }
                                break;
                            case 84: //Adhoc Modify Reply
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessAdhocModifyReply(ReplyStr);
                                }
                                break;
                            case 86: //MF Debit Client List
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessMFDebitClients(dt);
                                }
                                break;
                            case 87://Maal Fodo Client Holdings
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessMFClientHoldingReport(dt);
                                }
                                break;
                            case 89: //Illiquid Scrip Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessIlliquidReport(dt);
                                }
                                break;
                            case 92://Holding BseMf Krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReportOfBseMf(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_MF_ORDER_REPORTS_REC://BseMf krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessBseMfOrderTypeOrdersReport(dt);
                                }
                                break;
                            case Constants.OTS_Acitivitylog_MF_ORDER_REPORTS_REC://BseMf krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessBseMfActivitylogReport(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC://BseMf krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessBseMfSIPOrderTypeOrdersReport(dt);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC://BseMf krutika
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessManddateOrderTypeOrdersReport(dt);
                                }
                                break;
                            case Constants.OTS_HOLDING_REPORT://94 //19.01.17
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReportWithCollatralNew(dt);
                                } break;
                            case Constants.OTS_HOLDING_REPORT_PreClose://106
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReportPreCloseWithCollatralNew(dt);
                                } break;
                            case Constants.OTS_HOLDING_REPORT_PreClose_FS://112
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReportPreCloseWithCollatralNew_FS(dt);
                                }
                                break;
                            case 67: //New Holdings Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReportNew(dt);
                                }
                                break;
                            case 90: //New Holdings Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReportNewwithMTF(dt);
                                }
                                break;
                            case 194: //New Holdings Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessHoldingsReportNew_All(dt);
                                }
                                break;
                            case Constants.OTS_ALLCLIENTS_HOLDING_REPORT:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessAllClientHoldingsReportNew(dt);
                                } break;
                            case Constants.OTS_ALLCLIENTS_HOLDING_REPORT_New:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessAllClientHoldingsReportWithPreClose(dt);
                                } break;
                            case Constants.OTS_VERIFY_USER: // 99..
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["RetStr"]);
                                    ProcessVerifyUserReply(ReplyStr);
                                }
                                break;
                            case 110://Place Deriv Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessDerivPlaceOrderRequest(ReplyStr);
                                }
                                break;
                            case 115:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessAHDerivPlaceOrderRequest(ReplyStr);
                                }
                                break;
                            case 120:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessDerivModifyOrderRequest(ReplyStr);
                                }
                                break;
                            case 130:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessDerivCancelOrderRequest(ReplyStr);
                                }
                                break;
                            case 134:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessApprovalOrderRequest(ReplyStr);
                                }
                                break;
                            case 135:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessApprovalOrderOTPStatus(ReplyStr);
                                }
                                break;
                            case 136:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    //ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessApprovalOrdersReport(dt);
                                }


                                break;
                            case 191://Place Spread Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessSpreadPlaceOrderRequest(ReplyStr);
                                }
                                break;
                            case 192://Modify Spread Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessSpreadModifyOrderRequest(ReplyStr);
                                } break;
                            case 193://Cancel Spread Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessSpreadCancelOrderRequest(ReplyStr);
                                }
                                break;
                            case 200://Margin Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessMarginReport(dt);
                                }
                                break;
                            case 206://Margin Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessMarginReportMCX(dt);
                                }
                                break;
                            case 208://Margin Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessMarginReportCombined(dt);
                                }
                                break;
                            #region MTM 12-Sep-2016 - Rohan
                            case 203:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                }
                                break;
                            //case 208:
                            //    {
                            //        cmd.CommandText = MakeCommandText();
                            //        da.Fill(dt);
                            //    }
                            //    break;
                            case 205:   //Added on 19-Sep-2016 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessMTMClientsWithAllMargins(dt);
                                }
                                break;
                            case 207:   //Added on 08-Mar-2017 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessMTMClientsWithAllMarginsMcx(dt);
                                }
                                break;
                            #endregion
                            case 211://Place Curr Deriv Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessCurrDerivPlaceOrderRequest(ReplyStr);
                                }
                                break;
                            case 221://Modify Curr Deriv Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessCurrDerivModifyOrderRequest(ReplyStr);
                                }
                                break;
                            case 231://Cancel Curr Deriv Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessCurrDerivCancelOrderRequest(ReplyStr);
                                }
                                break;
                            case 235://Place SLBM Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessSLBMPlaceOrderRequest(ReplyStr);
                                }
                                break;
                            case 236://Modify SLBM Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessSLBMModifyOrderRequest(ReplyStr);
                                }
                                break;
                            case 237://Cancel SLBM Order
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessSLBMCancelOrderRequest(ReplyStr);
                                }
                                break;
                            case 255:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessSLBMHoldingsReport(dt);
                                }
                                break;
                            case 251:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                }
                                break;
                            case 252:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                }
                                break;
                            case 247:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                }
                                break;
                            case 246:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0][0]);
                                    ProcessIISReply(ReplyStr);
                                }
                                break;
                            case 209:// Template Margin Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessTemplateMarginReport(dt);
                                }
                                break;
                            case 172:// Template Margin Report Peak
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessTemplateMarginReport_Peak(dt);
                                }
                                break;
                            case 210://Template Margin Report With Derivative Margin 20.10.22
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessTemplateMarginReportNew(dt);
                                }
                                break;
                            case 173:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessPeakMarginData(dt);

                                }
                                break;
                            case 217:// MTF Template Margin Report
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessTemplateMarginReportMTF(dt);
                                }
                                break;
                            case 250:   //Added on 19-Sep-2016 - Rohan
                                {
                                    //Debug.WriteLine("250");
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessMTMClientsWithAllMarginsInstrumentWise(dt);
                                }
                                break;
                            case 248:   //Added on 19-Sep-2016 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessSegmentMaster(dt);
                                }
                                break;
                            //Option Exercise
                            case Constants.OTS_EXERCISE_PLACE_REQ:
                            case Constants.OTS_EXERCISE_MODIFY_REQ:
                            case Constants.OTS_EXERCISE_CANCEL_REQ:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
                                    ProcessOptionsExerciseRequest(ReplyStr);
                                }
                                break;
                            case 226:   //Added on 18-Apr-2019 - Rohan
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessExchTradeReport(dt);
                                }
                                break;
                            case Constants.OTS_OFS_WATCHLIST:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOFSWatclist(dt);
                                }
                                break;
                            case Constants.OTS_OFS_PLACE_BROKER_REQ:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOFSPlaceOrderReply(dt);
                                }
                                break;
                            case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOFSModifyOrderReply(dt);
                                }
                                break;
                            case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOFSCancelOrderReply(dt);
                                }
                                break;
                            case 125:
                                {
                                    cmd.CommandText = MakeCommandText();
                                    da.Fill(dt);
                                    ProcessOFSOrdersReportReply(dt);
                                }
                                break;
                        }
                    }
                    catch (Exception er)
                    {
                        //frm.listBox1.Items.Add()
                        ExceptionStr = er.Message;
                        HandleException();
                    }
                    try
                    {
                        if(OTSReqRec.ReqHdr.RequestType != 122)
                            dt.Clear();
                        Mainfrm.Invoke(frm._ThreadTerminationNew, new DBResult(DBNo, ThreadNumber, ExType, OTSReplyRec, OTSReqRec, AfterHours, RqStr, ExStr, DeadlockRetry, ThreadStartTick));//04.09.14 BeginInvoke & _ThreadTerminationNew //05.09.14 Invoke
                    }
                    catch (Exception et)
                    {
                        Form1.ErrorLog(et);
                    }

                    Status = TThreadStatus.Suspended;
                    _wh.WaitOne();         // No more tasks - wait for a signal
                }
                else
                {
                    Status = TThreadStatus.Suspended;
                    _wh.WaitOne();         // No more tasks - wait for a signal
                }
            }
        }

        private void ProcessExchTradeReport(DataTable dt)
        {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TradeExchReportReply());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            if (dt.Rows.Count <= 0) return;


            if (OTSReqRec.AllReq.GetTradesReport.Exch == 'B' && OTSReqRec.AllReq.GetTradesReport.ExchType == 'C')
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].TradeReportReply.ScripCode = Convert.ToInt32(dt.Rows[i]["ScripCode"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.Symbol = Convert.ToString(dt.Rows[i]["Symbol"]).Trim();
                    OTSReplyRec.AllReply[i].TradeReportReply.SymbolLength = Convert.ToByte(OTSReplyRec.AllReply[i].TradeReportReply.Symbol.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeNumber = Convert.ToInt32(dt.Rows[i]["TradeNo"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.ClientID = Convert.ToString(dt.Rows[i]["ClientId"]).PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.ClientIDLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.ClientID).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.TradedQty = Convert.ToInt32(dt.Rows[i]["QtyTraded"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.BrokerId = Convert.ToInt16(dt.Rows[i]["BrokerId"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.Reserved1 = Convert.ToInt16(dt.Rows[i]["Reserved"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeEntryTime = Convert.ToDateTime(dt.Rows[i]["TradeEntryTime"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeEntryDate = Convert.ToDateTime(dt.Rows[i]["TradeEntryDate"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradedPrice = Convert.ToSingle(dt.Rows[i]["TradedPrice"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TransactionType = Convert.ToChar(dt.Rows[i]["TransactionType"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.OrderNumber = Convert.ToInt64(dt.Rows[i]["OrderNumber"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.CAClass = Convert.ToString(dt.Rows[i]["CAClass"]).PadRight(5).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.CAClassLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.CAClass).TrimEnd().Length;
                    if (dt.Rows[i]["ProductType"].ToString() == "MARGIN") OTSReplyRec.AllReply[i].TradeReportReply.MarketType = 0;
                    else OTSReplyRec.AllReply[i].TradeReportReply.MarketType = 1;
                }
            }
            else if (OTSReqRec.AllReq.GetTradesReport.Exch == 'N' && OTSReqRec.AllReq.GetTradesReport.ExchType == 'C')
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeNumber = Convert.ToInt32(dt.Rows[i]["TradeNo"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeStatus = Convert.ToString(dt.Rows[i]["TradeStatus"]).PadRight(2).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeStatusLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.TradeStatus).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.Symbol = Convert.ToString(dt.Rows[i]["Symbol"]).Trim();
                    OTSReplyRec.AllReply[i].TradeReportReply.SymbolLength = Convert.ToByte(OTSReplyRec.AllReply[i].TradeReportReply.Symbol.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].TradeReportReply.Series = Convert.ToString(dt.Rows[i]["Series"]).PadRight(2).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.SeriesLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.Series).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.SecurityName = Convert.ToString(dt.Rows[i]["SecurityName"]).Trim().PadRight(50).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.SecurityNameLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.SecurityName).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.InstrumentType = Convert.ToByte(dt.Rows[i]["InstrumentType"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.BookType = Convert.ToByte(dt.Rows[i]["BookType"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.MarketType = Convert.ToByte(dt.Rows[i]["MarketType"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.UserID = Convert.ToString(dt.Rows[i]["UserId"]).PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.UserIDLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.UserID).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.Branch = Convert.ToString(dt.Rows[i]["BranchId"]).PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.BranchIDLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.Branch).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradedQty = Convert.ToInt32(dt.Rows[i]["TradeQty"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradedPrice = Convert.ToSingle(dt.Rows[i]["TradePrice"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.ProClient = Convert.ToByte(dt.Rows[i]["Pro/Cli"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.ClientAc = Convert.ToString(dt.Rows[i]["ClientA/C"]).PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.ClientAcLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.ClientAc).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.ParticipantCode = Convert.ToInt16(dt.Rows[i]["ParticipantCode"]);
                    if (Convert.ToChar(dt.Rows[i]["AuctionPartType"]) == 'N') OTSReplyRec.AllReply[i].TradeReportReply.AuctionPartType = 0;
                    else OTSReplyRec.AllReply[i].TradeReportReply.AuctionPartType = 1;
                    OTSReplyRec.AllReply[i].TradeReportReply.AuctionNumber = Convert.ToByte(dt.Rows[i]["AuctionNo"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.SettPeriod = Convert.ToChar(dt.Rows[i]["Sett.Period"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeEntryDateTime = Convert.ToDateTime(dt.Rows[i]["TradeEntryDateTime"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeModifyDateTime = Convert.ToDateTime(dt.Rows[i]["TradeModifyDateTime"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.OrderNumber = Convert.ToInt64(dt.Rows[i]["OrderNo"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.CPId = Convert.ToString(dt.Rows[i]["CPId"]).PadRight(5).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.CPIdLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.CPId).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.OrderInsertTime = Convert.ToDateTime(dt.Rows[i]["OrderInsertTime"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.CTCLUserInfoNC = Convert.ToInt64(dt.Rows[i]["CTCLUserInfo"]);

                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeNumber = Convert.ToInt32(dt.Rows[i]["TradeNo"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeStatus = Convert.ToString(dt.Rows[i]["TradeStatus"]).PadRight(2).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeStatusLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.TradeStatus).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.InstrumentName = Convert.ToString(dt.Rows[i]["InstrumentName"]).TrimEnd().PadRight(8).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.InstrumentNameLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.InstrumentName).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.Symbol = Convert.ToString(dt.Rows[i]["Symbol"]).TrimEnd().PadRight(50);
                    OTSReplyRec.AllReply[i].TradeReportReply.SymbolLength = Convert.ToByte(OTSReplyRec.AllReply[i].TradeReportReply.Symbol.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].TradeReportReply.ExpiryDate = Convert.ToDateTime(dt.Rows[i]["ExpiryDate"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.StrikePrice = Convert.ToSingle(dt.Rows[i]["StrikePrice"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.OptionType = Convert.ToString(dt.Rows[i]["OptionType"]).TrimEnd().PadRight(2).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.OptionTypeLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.OptionType).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.SecurityName = Convert.ToString(dt.Rows[i]["SecurityName"]).TrimEnd().PadRight(50).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.SecurityNameLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.SecurityName).TrimEnd().Length;
                    if (Convert.ToChar(dt.Rows[i]["BookType"]) == '3') OTSReplyRec.AllReply[i].TradeReportReply.BookType = 0;
                    else OTSReplyRec.AllReply[i].TradeReportReply.BookType = 1;
                    OTSReplyRec.AllReply[i].TradeReportReply.BookTypeName = Convert.ToString(dt.Rows[i]["BookTypeName"]).TrimEnd().PadRight(6).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.BookTypeNameLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.BookTypeName).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.MarketType = Convert.ToByte(dt.Rows[i]["MarketType"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.UserID = Convert.ToString(dt.Rows[i]["UserId"]).TrimEnd().PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.UserIDLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.UserID).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.Branch = Convert.ToString(dt.Rows[i]["BranchId"]).TrimEnd().PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.BranchIDLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.Branch).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradedQty = Convert.ToInt32(dt.Rows[i]["TradeQty"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradedPrice = Convert.ToSingle(dt.Rows[i]["TradePrice"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.ProClient = Convert.ToByte(dt.Rows[i]["Pro/Cli"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.ClientAc = Convert.ToString(dt.Rows[i]["ClientA/C"]).TrimEnd().PadRight(14).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.ClientAcLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.ClientAc).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.ParticipantCode = Convert.ToInt16(dt.Rows[i]["ParticipantCode"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.OpenClose = Convert.ToString(dt.Rows[i]["OpenClose"]).TrimEnd().PadRight(5).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.OpenCloseLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.OpenClose).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.CoverUncover = Convert.ToString(dt.Rows[i]["CoverUncover"]).TrimEnd().PadRight(7).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.CoverUncoverLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.CoverUncover).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeEntryDateTime = Convert.ToDateTime(dt.Rows[i]["TradeEntryDateTime"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.TradeModifyDateTime = Convert.ToDateTime(dt.Rows[i]["TradeModifyDateTime"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.OrderNumber = Convert.ToInt64(dt.Rows[i]["OrderNo"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.CPId = Convert.ToString(dt.Rows[i]["CPId"]).TrimEnd().PadRight(5).ToCharArray();
                    OTSReplyRec.AllReply[i].TradeReportReply.CPIdLength = (byte)new string(OTSReplyRec.AllReply[i].TradeReportReply.CPId).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].TradeReportReply.OrderInsertTime = Convert.ToDateTime(dt.Rows[i]["OrderTime"]);
                    OTSReplyRec.AllReply[i].TradeReportReply.CTCLUserInfoND = Convert.ToInt64(dt.Rows[i]["UserInfo"]);
                }
            }
        }

        private void ProcessOFSWatclist(DataTable dt)
        {
            try
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.OFSWatchListReply());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                if (dt.Rows.Count <= 0) return;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].OFSWatchListReply.Symbol = Convert.ToString(dt.Rows[i]["Symbol"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.SymbolLength = Convert.ToByte(OTSReplyRec.AllReply[i].OFSWatchListReply.Symbol.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.SecurityName = Convert.ToString(dt.Rows[i]["SecurityName"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.SecurityNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].OFSWatchListReply.SecurityName.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.IsinCode = Convert.ToString(dt.Rows[i]["isinCode"]).PadRight(12);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.FaceValue = Convert.ToInt32(dt.Rows[i]["faceValue"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.TickSize = Convert.ToDouble(dt.Rows[i]["tickSize"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.RegularLotSize = Convert.ToInt16(dt.Rows[i]["regularLotSize"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.MinimumPercentageDay = Convert.ToDouble(dt.Rows[i]["minimumPercentageDay"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.MaximumPercentageDay = Convert.ToDouble(dt.Rows[i]["maximumPercentageDay"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.Series = Convert.ToString(dt.Rows[i]["Series"]).PadRight(3);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.IssueSize = Convert.ToInt32(dt.Rows[i]["IssueSize"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.BasePrice = Convert.ToDouble(dt.Rows[i]["basePrice"]);
                    //OTSReplyRec.AllReply[i].OFSWatchListReply.OpenOnDate = Convert.ToDateTime(dt.Rows[i]["OpenOnDate"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.MktOpenTime = Convert.ToString(dt.Rows[i]["mktOpenTime"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.MktCloseTime = Convert.ToString(dt.Rows[i]["mktCloseTime"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.MktModCxlOpenTime = Convert.ToString(dt.Rows[i]["mktModCxlOpenTime"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.MktModCxlCloseTime = Convert.ToString(dt.Rows[i]["mktModCxlCloseTime"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.IsNewOrderAllowed = Convert.ToBoolean(Convert.ToInt32(dt.Rows[i]["isNewOrderAllowed"]));
                    OTSReplyRec.AllReply[i].OFSWatchListReply.IsModificationAllowed = Convert.ToBoolean(Convert.ToInt32(dt.Rows[i]["isModificationAllowed"]));
                    OTSReplyRec.AllReply[i].OFSWatchListReply.IsCancellationAllowed = Convert.ToBoolean(Convert.ToInt32(dt.Rows[i]["IsCancellationAllowed"]));
                    OTSReplyRec.AllReply[i].OFSWatchListReply.QtyModAllowed = Convert.ToString(dt.Rows[i]["QtyModAllowed"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.PriceModAllowed = Convert.ToString(dt.Rows[i]["PriceModAllowed"]);
                    OTSReplyRec.AllReply[i].OFSWatchListReply.IsAccountNoModAllowed = Convert.ToBoolean(Convert.ToInt32(dt.Rows[i]["IsAccountNoModAllowed"]));
                    OTSReplyRec.AllReply[i].OFSWatchListReply.IsCpCodeModAllowed = Convert.ToBoolean(Convert.ToInt32(dt.Rows[i]["IsCpCodeModAllowed"]));
                    OTSReplyRec.AllReply[i].OFSWatchListReply.OrderValueUpperLimit = Convert.ToInt64(dt.Rows[i]["OrderValueUpperLimit"]);

                }


            }
            catch (Exception ex)
            {

                //throw;
            }

        }


        private void ProcessOFSPlaceOrderReply(DataTable dt)
        {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort) dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            OTSReplyRec.ErrorStringLength = 0;
            if (dt.Rows.Count <= 0) return;

            char AHStatus1;
            Single MBlk;
            string ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            int BOId = Convert.ToInt32(VList[0]);
            int TimeNow = Convert.ToInt32(VList[1]);
            string BoidTime = BOId + "," + TimeNow;
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
            string[] split = ReplyStr.Split('|');
            int BrokerOrderId = Convert.ToInt32(split[0]);
            int BrokerOrderTime = Convert.ToInt32(split[1]);
            int test = Convert.ToInt32(split[2]);
            //Single test1 = Convert.ToSingle(split[3]);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Exch = OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchType = OTSReqRec.AllReq.PlaceOfsOrderRequest.ExchType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripName = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripName;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripNameLength = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripNameLength;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripCode = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripCode;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Series = OTSReqRec.AllReq.PlaceOfsOrderRequest.Series;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BuySell = 'B';// OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderType = 1;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ProductType = 1;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Qty = OTSReqRec.AllReq.PlaceOfsOrderRequest.Qty;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Rate = OTSReqRec.AllReq.PlaceOfsOrderRequest.Rate;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.AtMarket = OTSReqRec.AllReq.PlaceOfsOrderRequest.AtMarket;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.MarginType = OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ClientType = OTSReqRec.AllReq.PlaceOfsOrderRequest.ClientType.PadRight(3);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ParticipantId = OTSReqRec.AllReq.PlaceOfsOrderRequest.ParticipantId.PadRight(15);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BrokerOrderID = BrokerOrderId;//BrokerOFSOrderIdCount = BrokerOFSOrderIdCount - 1;// OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BrokerOrderTime = BrokerOrderTime;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderId = "0".PadRight(16).ToCharArray();// OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BatchId = "0".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BatchSerialNo = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.DeletedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.DeletedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchangeOrderID = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchangeOrderTime = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchErrorCode = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchReplyCode = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ModifiedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ModifiedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OfsOrderType = 'P';
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderStatus = "Pending".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.PlacedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.PlacedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Status = "Pending".PadRight(20).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.TradingMemberId = "".PadRight(15).ToCharArray();

        }


        private void ProcessOFSModifyOrderReply(DataTable dt)
        {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS());
            OTSReplyRec.ReplyHdr.ReplyCnt = (byte)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            OTSReplyRec.ErrorStringLength = 0;
            if (dt.Rows.Count <= 0) return;

            char AHStatus1;
            Single MBlk;
            string ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            int BOId = Convert.ToInt32(VList[0]);
            int TimeNow = Convert.ToInt32(VList[1]);
            string BoidTime = BOId + "," + TimeNow;
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

            ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);

            string[] split = ReplyStr.Split('|');
            int BrokerOrderId = Convert.ToInt32(split[0]);
            int BrokerOrderTime = Convert.ToInt32(split[1]);
            int test = Convert.ToInt32(split[2]);
            //Single test1 = Convert.ToSingle(split[3]);

            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Exch = OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchType = OTSReqRec.AllReq.PlaceOfsOrderRequest.ExchType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripName = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripName;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripNameLength = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripNameLength;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripCode = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripCode;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Series = OTSReqRec.AllReq.PlaceOfsOrderRequest.Series;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BuySell = 'B';// OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderType = 1;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ProductType = 1;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Qty = OTSReqRec.AllReq.PlaceOfsOrderRequest.Qty;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Rate = OTSReqRec.AllReq.PlaceOfsOrderRequest.Rate;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.AtMarket = OTSReqRec.AllReq.PlaceOfsOrderRequest.AtMarket;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.MarginType = OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ClientType = OTSReqRec.AllReq.PlaceOfsOrderRequest.ClientType.PadRight(3);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ParticipantId = OTSReqRec.AllReq.PlaceOfsOrderRequest.ParticipantId.PadRight(15);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BrokerOrderID = BrokerOrderId;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BrokerOrderTime = BrokerOrderTime;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderId = "0".PadRight(16).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BatchId = "0".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BatchSerialNo = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.DeletedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.DeletedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchangeOrderID = Convert.ToInt64(OTSReqRec.AllReq.PlaceOfsOrderRequest.OrderId);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchangeOrderTime = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchErrorCode = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchReplyCode = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ModifiedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ModifiedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OfsOrderType = 'P';
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderStatus = "Requested".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.PlacedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.PlacedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Status = "Requested".PadRight(20).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.TradingMemberId = "".PadRight(15).ToCharArray();

        }


        private void ProcessOFSCancelOrderReply(DataTable dt)
        {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            OTSReplyRec.ErrorStringLength = 0;
            if (dt.Rows.Count <= 0) return;
            char AHStatus1;
            Single MBlk;
            string ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            int BOId = Convert.ToInt32(VList[0]);
            int TimeNow = Convert.ToInt32(VList[1]);
            string BoidTime = BOId + "," + TimeNow;
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

            string[] split = ReplyStr.Split('|');
            int BrokerOrderId = Convert.ToInt32(split[0]);
            int BrokerOrderTime = Convert.ToInt32(split[1]);
            int test = Convert.ToInt32(split[2]);
            //Single test1 = Convert.ToSingle(split[3]);

            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Exch = OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchType = OTSReqRec.AllReq.PlaceOfsOrderRequest.ExchType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripName = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripName;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripNameLength = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripNameLength;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ScripCode = OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripCode;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Series = OTSReqRec.AllReq.PlaceOfsOrderRequest.Series;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BuySell = 'B';// OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderType = 1;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ProductType = 1;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Qty = OTSReqRec.AllReq.PlaceOfsOrderRequest.Qty;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Rate = OTSReqRec.AllReq.PlaceOfsOrderRequest.Rate;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.AtMarket = OTSReqRec.AllReq.PlaceOfsOrderRequest.AtMarket;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.MarginType = OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ClientType = OTSReqRec.AllReq.PlaceOfsOrderRequest.ClientType.PadRight(3);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ParticipantId = OTSReqRec.AllReq.PlaceOfsOrderRequest.ParticipantId.PadRight(15);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BrokerOrderID = BrokerOrderId;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BrokerOrderTime = BrokerOrderTime;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderId = "0".PadRight(16).ToCharArray();// OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BatchId = "0".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.BatchSerialNo = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.DeletedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.DeletedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchangeOrderID = Convert.ToInt64(OTSReqRec.AllReq.PlaceOfsOrderRequest.OrderId);
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchangeOrderTime = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchErrorCode = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ExchReplyCode = 0;
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ModifiedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.ModifiedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OfsOrderType = 'P';
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.OrderStatus = "Requested".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.PlacedBy = "".PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.PlacedDate = "".PadRight(50).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.Status = "Requested".PadRight(20).ToCharArray();
            OTSReplyRec.AllReply[0].OfsPlaceOrderReply.TradingMemberId = "".PadRight(15).ToCharArray();

        }

        private void ProcessOFSOrdersReportReply(DataTable dt)
        {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeReportsReplyRecOFS());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);

            OTSReplyRec.ErrorStringLength = 0;
            if (dt.Rows.Count <= 0) return;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.Exch = Convert.ToChar(dt.Rows[i]["EXCH"].ToString().TrimEnd());
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"].ToString().TrimEnd());
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ScripName = Convert.ToString(dt.Rows[i]["SYMBOL"]).PadRight(50).TrimEnd();
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ScripNameLength = (byte)OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ScripName.Length;
                //OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ScripCode = Convert.ToInt32(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.Series = Convert.ToString(dt.Rows[i]["SERIES"]).PadRight(3).TrimEnd();
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.BuySell = 'B';// OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.OrderType = 1;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ProductType = 1;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.Qty = Convert.ToInt32(dt.Rows[i]["QUANTITY"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.Rate = Convert.ToSingle(dt.Rows[i]["PRICE"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.AtMarket = 'N';// Convert.ToChar(dt.Rows[i]["ISMARKETORDER"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.MarginType = Convert.ToInt32(dt.Rows[i]["MARGINTYPE"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ClientType = Convert.ToString(dt.Rows[i]["CLIENTTYPE"]).PadRight(3);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ParticipantId = Convert.ToString(dt.Rows[i]["ParticipantId"]).PadRight(15);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);// OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ExchangeOrderTime = 0;// Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);                                                                                                            //OTSReplyRec.AllReply[0].OfsOrdersReportsReply.BatchId = "0".PadRight(50).ToCharArray();
                string temp = Convert.ToString(dt.Rows[i]["DELETEDBY"]);
                if (temp == "")
                    OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ModifiedBy = 0;
                else
                    OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ModifiedBy = Convert.ToInt32(dt.Rows[i]["DELETEDBY"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.CancelledDate = "".PadRight(20);
                temp = Convert.ToString(dt.Rows[i]["modifiedBy"]);
                if (temp == "")
                    OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ModifiedBy = 0;
                else
                    OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ModifiedBy = Convert.ToInt32(dt.Rows[i]["modifiedBy"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ModifiedDate = "".PadRight(20);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.PlacedBy = Convert.ToInt32(dt.Rows[i]["PLACEDBY"]);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.PlacedDate = "".PadRight(20);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.Status = Convert.ToString(dt.Rows[i]["STATUS"]).PadRight(20).TrimEnd();
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.StatusLength = (byte)OTSReplyRec.AllReply[i].OfsOrdersReportsReply.Status.Length;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.TradedQty = 0;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.CpErrorCode = 0;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.IsCpApproved = 0;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.IsCpOrder = 0;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.OperationTimestamp = 0.ToString().PadRight(20);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.OrderRequestorCode = "".PadRight(15);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.OperationType = "".PadRight(20);
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.OperationUser = 0;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.PendingQty = 0;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.RejectReason = "".PadRight(200).ToCharArray();
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.ScripCode = 0;
                OTSReplyRec.AllReply[i].OfsOrdersReportsReply.RejectReason = dt.Rows[i]["REASON"].ToString().PadRight(200).ToCharArray();

            }
        }

        private void ProcessIISReply(string ReplyStr)
        {
            try
            {
                VList = ReplyStr.Split('$');

                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";

                OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTradeGeneral());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTradeGeneral()); ;
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                OTSReplyRec.ReplyHdr.FEUserID = "IIS".ToString().PadRight(15).ToCharArray();
                OTSReplyRec.ReplyHdr.FEUserIDLength = 3;
                Array.Resize(ref OTSReplyRec.AllReply, 1);

                if (VList.Count() > 1)
                {
                    switch (OTSReplyRec.ReplyHdr.ChannelID_ND)
                    {
                        case 501:
                            {
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.ActiveUSers = VList[0];
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.DateOfDay = Convert.ToInt32(VList[1]);
                            } break;
                        case 502:
                            {
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.ActiveUSers = VList[0];
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.DateOfDay = Convert.ToInt32(VList[1]);
                            } break;
                        case 503:
                            {
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.ActiveUSers = VList[0];
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.DateOfDay = Convert.ToInt32(VList[1]);
                            } break;
                        case 504:
                            {
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.ActiveUSers = VList[0];
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.DateOfDay = Convert.ToInt32(VList[1]);
                            } break;
                        case 901:
                            {
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.ActiveUSers = VList[0];
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.DateOfDay = Convert.ToInt32(VList[1]);
                            } break;
                        default:
                            {
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.ActiveUSers = VList[0];
                                OTSReplyRec.AllReply[0].IISOrderTradeGeneral.DateOfDay = Convert.ToInt32(VList[1]);
                            } break;
                    }
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }


        public void Shutdown(bool waitForWorkers)
        {
            // Enqueue one null item per worker to make each exit.
            foreach (Thread worker in t)
            {
                OTSReqRec.ReqHdr.RequestType = 0;
                EnqueueTask(OTSReqRec);
            }

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in t)
                    worker.Join();
        }
        public static int getEpochTime(DateTime t)       // sumitp 28_jan_22
        {
            TimeSpan ts = t - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)ts.TotalSeconds;
            return secondsSinceEpoch;
        }

        private void HandleException()
        {
            string ReplyStr;
            try
            {
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
                                        ProcessLoginReply(ReplyStr);
                                    }
                                    break;
                                case 2:
                                    {
                                        ReplyStr = "1|Server is busy. Please retry";
                                        ProcessChangePasswordReply(ReplyStr);
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
                                        ProcessVerifyTrader(ReplyStr);
                                    }
                                    break;
                                case 10://Place Equity Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessEquityPlaceOrderRequest(ReplyStr);
                                    }
                                    break;
                                case Constants.OTS_ORDERTYPE_PLACE_REQ:
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessOrderTypePlaceOrderRequest(ReplyStr);
                                    }
                                    break;
                                case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessOrderTypeModifyOrderRequest(ReplyStr);
                                    }
                                    break;
                                case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessOrderTypeCancelOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 20://Modify Equity Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessEquityModifyOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 30://Cancel Equity Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessEquityCancelOrderRequest(ReplyStr);
                                    }
                                    break;
                                case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ://43 BseMf Krutika
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessBseMFSIPRegistrationRequest(ReplyStr);
                                    }break;
                                case Constants.OTS_MANDATE://43 BseMf Krutika
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessBseMFMandate(ReplyStr);
                                    } break;
                            case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ://43 BseMf Krutika
                                {
                                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                    ProcessBseMFXSIPRegistrationRequest(ReplyStr);
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessBseMFSIPCancelOrderRequest(ReplyStr);
                                    }break;
                                case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessBseMFPlaceOrderRequest(ReplyStr);
                                    } break;
                                case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44 BseMf
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessBseMFModifyOrderRequest(ReplyStr);
                                    } break;
                                case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45 BseMf
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessBseMFCancelOrderRequest(ReplyStr);
                                    } break;
                                //case 61://Trade Report
                                //    {
                                //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                //        OTSReplyRec.ErrorString = "Server is busy. Retrying Trade Report";
                                //        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                //    }
                                //    break;
                                //case 62://Order Report
                                //    {
                                //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                //        OTSReplyRec.ErrorString = "Server is busy. Retrying Order Report";
                                //        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                //    }
                                //    break;
                                case 125:
                                case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC:
                                case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142:
                                case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW:    //Added on 18-Apr-2018 - Rohan
                                case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER:   // sumitp 28_jan_22
                                {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Trade Reports";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR:
                                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC: //16.1.15
                                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141: //Added on 5-Apr-2018 - Rohan
                                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW: //Added on 18-Apr-2018 - Rohan
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Order Reports";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                            case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_COVER_BRACKET: //Added on 27-Nov-2021 - himanshu
                                {
                                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    OTSReplyRec.ErrorString = "Server is busy. Retrying Order Reports";
                                    OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_EXERCISE_REPORTS_REC: //Option Exercise
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Order Reports";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                //case 64://Activity Log
                                //    {
                                //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                //        OTSReplyRec.ErrorString = "Server is busy. Retry Later Activity Log Report";
                                //        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                //    }
                                //    break;
                                case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC:
                                case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW:
                                case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW:  //Added on 18-Apr-2018 - Rohan
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Activity Log Reports";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                            case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new: // sumit 21_jan_22 single activity log
                                {
                                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    OTSReplyRec.ErrorString = "Server is busy. Retry Later Activity Log Reports";
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
                                case Constants.OTS_ORDERTYPE_MF_ORDER_REPORTS_REC://BseMf Krutika
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Trade Reports";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                            case Constants.OTS_Acitivitylog_MF_ORDER_REPORTS_REC://BseMf Krutika
                                {
                                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    OTSReplyRec.ErrorString = "Server is busy. Retrying Trade Reports";
                                    OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                }
                                break;
                            case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC://BseMf Krutika
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retrying Trade Reports";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                            case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC://BseMf Krutika
                                {
                                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    OTSReplyRec.ErrorString = "Server is busy. Retrying Trade Reports";
                                    OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                }
                                break;
                            case 110: //Place Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessDerivPlaceOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 120://Modify Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessDerivModifyOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 130: //Cancel Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessDerivCancelOrderRequest(ReplyStr);
                                    }
                                    break;
                            case 134: //02.03.23 IVR-OTP Order Placement 
                                {
                                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                    ProcessApprovalOrderRequest(ReplyStr);
                                }
                                break;
                            case 135:
                                {
                                     ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                }
                                break;
                            case 136:
                                {
                                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                }
                                break;

                                case 211: //Place Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessCurrDerivPlaceOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 221://Modify Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessCurrDerivModifyOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 231: //Cancel Deriv Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessCurrDerivCancelOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 200://Margin Report
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 206://Margin Report
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report MCX";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                #region MTM Added on 12-Sep-2016 - Rohan
                                case 203:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 208:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 205:   //Added on 19-Sep-2016 - Rohan
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 207:   //Added on 08-Mar-2017 - Rohan
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 209:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                            case 172:
                                {
                                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                    OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                }
                                break;
                            case 210: //Derivative margin 20.10.22
                                {
                                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                    OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                }
                                break;

                            case 173:
                                {
                                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    OTSReplyRec.ErrorString = "Server is busy. Retry Later PeakMargin Report";
                                    OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                }
                                break;
                            case 217:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 250:   //Added on 19-Sep-2016 - Rohan
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case 248:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                #endregion
                                case 235://Place SLBM Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessSLBMPlaceOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 236://Modify SLBM Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessSLBMModifyOrderRequest(ReplyStr);
                                    }
                                    break;
                                case 237://Cancel SLBM Order
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry";
                                        ProcessSLBMCancelOrderRequest(ReplyStr);
                                    }
                                    break;

                                //Option Exercise
                                case Constants.OTS_EXERCISE_PLACE_REQ:
                                case Constants.OTS_EXERCISE_MODIFY_REQ:
                                case Constants.OTS_EXERCISE_CANCEL_REQ:
                                    {
                                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Server is busy. Please retry|0";
                                        ProcessOptionsExerciseRequest(ReplyStr);
                                    }
                                    break;
                               case 226:
                                   {
                                       OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                       OTSReplyRec.ErrorString = "Server is busy. Retry Later";
                                       OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                   }
                                   break;
                                case Constants.OTS_OFS_WATCHLIST:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                    }
                                    break;
                                case Constants.OTS_OFS_PLACE_BROKER_REQ:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later";
                                        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                                    }
                                    break;
                                case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                                    {
                                        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                        OTSReplyRec.ErrorString = "Server is busy. Retry Later";
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
                                ProcessLoginReply(ReplyStr);
                            }
                            break;
                        case 2:
                            {
                                ReplyStr = "1|Database Error." + ExStr1;
                                ProcessChangePasswordReply(ReplyStr);
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
                                ProcessVerifyTrader(ReplyStr);
                            }
                            break;
                        case 10://Place Equity Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessEquityPlaceOrderRequest(ReplyStr);
                            }
                            break;
                        case 14:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                GetDealerDetailsForIVR(ReplyStr);
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_PLACE_REQ:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessOrderTypePlaceOrderRequest(ReplyStr);
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessOrderTypeModifyOrderRequest(ReplyStr);
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessOrderTypeCancelOrderRequest(ReplyStr);
                            }
                            break;
                        case 20://Modify Equity Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessEquityModifyOrderRequest(ReplyStr);
                            }
                            break;
                        case 30://Cancel Equity Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessEquityCancelOrderRequest(ReplyStr);
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessBseMFPlaceOrderRequest(ReplyStr);
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessBseMFModifyOrderRequest(ReplyStr);
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessBseMFCancelOrderRequest(ReplyStr);
                            }
                            break;
                        case Constants.OTS_MANDATE://43 BseMf Krutika
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessBseMFMandate(ReplyStr);
                            } break;
                        //case Constants.OTS_MANDATE://43 BseMf Krutika
                        //    {
                        //        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                        //        ProcessBseMFMandate(ReplyStr);
                        //    } break;
                        case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ://34 PendingSIP
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessBseMFSIPRegistrationRequest(ReplyStr);
                            }break;
                        case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ://34 PendingSIP
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessBseMFXSIPRegistrationRequest(ReplyStr);
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1;
                                ProcessBseMFSIPCancelOrderRequest(ReplyStr);
                            }break;
                        //case 61://Trade Report
                        //    {
                        //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                        //        OTSReplyRec.ErrorString = "Trade Report-Database Error." + ExStr1;
                        //        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                        //    }
                        //    break;
                        //case 62://Order Report
                        //    {
                        //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                        //        OTSReplyRec.ErrorString = "Order Report-Database Error." + ExStr1;
                        //        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                        //    }
                        //    break;
                        case 125:
                        case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC:
                        case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142:    //Added on 5-Apr-2018 - Rohan
                        case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW:    //Added on 18-Apr-2018 - Rohan
                        case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER:    // // sumitp 28_jan_22
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Trade Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR:
                        case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC:
                        case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141: //Added on 5-Apr-2018 - Rohan
                        case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW: //Added on 18-Apr-2018 - Rohan
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Order Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_COVER_BRACKET: //Added on 25-Nov-2021 - Himanshu    // sumitp 28_jan_22
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Order Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_EXERCISE_REPORTS_REC://Options Exercise
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Order Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        //case 64://Activity Log
                        //    {
                        //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                        //        OTSReplyRec.ErrorString = "Activity Log Report-Database Error." + ExStr1;
                        //        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                        //    }
                        //    break;
                        case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC:
                        case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW:
                        case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW:  //Added on 18-Apr-2018 - Rohan
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Activity Log Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new: // sumit 21_jan_22 single activity log
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Activity Log Report-Database Error." + ExStr1;
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
                        case Constants.OTS_ORDERTYPE_MF_ORDER_REPORTS_REC: //BseMf Krutika
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Trade Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_Acitivitylog_MF_ORDER_REPORTS_REC: //BseMf Krutika
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Trade Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC: //BseMf Krutika
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Trade Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC: //BseMf Krutika
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Order Types Trade Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        //case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC: //BseMf Krutika
                        //    {
                        //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                        //        OTSReplyRec.ErrorString = "Order Types Trade Report-Database Error." + ExStr1;
                        //        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                        //    } break;
                        //case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC: //BseMf Krutika
                        //    {
                        //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
                        //        OTSReplyRec.ErrorString = "Order Types Trade Report-Database Error." + ExStr1;
                        //        OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                        //    }
                        //    break;
                        case 110: //Place Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessDerivPlaceOrderRequest(ReplyStr);
                            }
                            break;
                        case 120://Modify Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessDerivModifyOrderRequest(ReplyStr);
                            }
                            break;
                        case 130: //Cancel Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessDerivCancelOrderRequest(ReplyStr);
                            }
                            break;
                        case 134:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                
                            }
                            break;
                        case 135:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                            }
                            break;
                        case 136:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                            }
                            break;
                        #region MTM Added on 12-Sep-2016 - Rohan
                        case 203:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 208:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 205:   //Added on 19-Sep-2016 - Rohan
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 207:   //Added on 08-Mar-2017 - Rohan
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        #endregion

                        case 211: //Place Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessCurrDerivPlaceOrderRequest(ReplyStr);
                            }
                            break;
                        case 221://Modify Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessCurrDerivModifyOrderRequest(ReplyStr);
                            }
                            break;
                        case 231: //Cancel Deriv Order
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessCurrDerivCancelOrderRequest(ReplyStr);
                            }
                            break;
                        case 200://Margin Report
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Margin Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 206://Margin Report
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Margin Report MCX-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 209:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 172:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 210:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 173:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later PeakMargin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 217:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 250:   //Added on 19-Sep-2016 - Rohan
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case 248:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Server is busy. Retry Later Margin Report";
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        //Option Exercise 
                        case Constants.OTS_EXERCISE_PLACE_REQ:
                        case Constants.OTS_EXERCISE_MODIFY_REQ:
                        case Constants.OTS_EXERCISE_CANCEL_REQ:
                            {
                                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Database Error." + ExStr1 + "|0";
                                ProcessOptionsExerciseRequest(ReplyStr);
                            }
                            break;
                        case 226:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Trade Report-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_OFS_WATCHLIST:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "OFS Watchlist-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_OFS_PLACE_BROKER_REQ:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Place-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Place-Database Error." + ExStr1;
                                OTSReplyRec.ErrorStringLength = (byte)OTSReplyRec.ErrorString.Length;
                            }
                            break;
                        case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                            {
                                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                                OTSReplyRec.ErrorString = "Place-Database Error." + ExStr1;
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

        private string MakeCommandText()
        {
                string OrderRequesterId, strName, FEUserId, ClientCode;
            int OTPNumber;
            string CheckInfo1 = "", CheckInfo2 = "", s1, ScNameForOType;
                Result = "";
                char RequestType = 'P';
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
                            else //11.10.14 For Version in DB.
                                CheckInfo2 = new string(OTSReqRec.AllReq.LoginRequest.Version, 0, OTSReqRec.AllReq.LoginRequest.VersionLength);

                            Result = "Exec Proc_LoginUser2 " + "'" + new string(OTSReqRec.AllReq.LoginRequest.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequest.FEUserIDLength)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.Password1).Substring(0, OTSReqRec.AllReq.LoginRequest.Password1Length) + "','"
                                + new string(OTSReqRec.AllReq.LoginRequest.Password2).Substring(0, OTSReqRec.AllReq.LoginRequest.Password2Length)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.MachineID).Substring(0, OTSReqRec.AllReq.LoginRequest.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.LoginRequest.ITS_IP).Substring(0, OTSReqRec.AllReq.LoginRequest.ITS_IPLength)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.ServerIP).Substring(0, OTSReqRec.AllReq.LoginRequest.ServerIPLength) + "','" + new string(OTSReqRec.AllReq.LoginRequest.ClientIP).Substring(0, OTSReqRec.AllReq.LoginRequest.ClientIPLength) + "'," +
                                 OTSReqRec.AllReq.LoginRequest.ChkType1 + ",'" + CheckInfo1 + "'," +
                                  OTSReqRec.AllReq.LoginRequest.ChkType2 + ",'" + CheckInfo2 + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
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
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 3:
                        {
                            if (OTSReqRec.AllReq.LoginRequest.ChkType1InfoLength > 0)
                                CheckInfo1 = new string(OTSReqRec.AllReq.LoginRequest.ChkType1Info).Substring(0, OTSReqRec.AllReq.LoginRequest.ChkType1InfoLength).ToUpper();

                            if (OTSReqRec.AllReq.LoginRequest.ChkType2InfoLength > 0)
                                CheckInfo2 = OTSReqRec.AllReq.LoginRequest.ChkType2Info.Substring(0, OTSReqRec.AllReq.LoginRequest.ChkType2InfoLength).ToUpper();
                            else //11.10.14 For Version in DB.                                                                              //Added this else on 20/10/2014 HK
                                CheckInfo2 = new string(OTSReqRec.AllReq.LoginRequest.Version, 0, OTSReqRec.AllReq.LoginRequest.VersionLength);

                            Result = "Exec Proc_LoginUser3 " + "'" + new string(OTSReqRec.AllReq.LoginRequest.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequest.FEUserIDLength)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.Password1).Substring(0, OTSReqRec.AllReq.LoginRequest.Password1Length) + "','"
                                + new string(OTSReqRec.AllReq.LoginRequest.Password2).Substring(0, OTSReqRec.AllReq.LoginRequest.Password2Length)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.MachineID).Substring(0, OTSReqRec.AllReq.LoginRequest.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.LoginRequest.ITS_IP).Substring(0, OTSReqRec.AllReq.LoginRequest.ITS_IPLength)
                                + "','" + new string(OTSReqRec.AllReq.LoginRequest.ServerIP).Substring(0, OTSReqRec.AllReq.LoginRequest.ServerIPLength) + "','" + new string(OTSReqRec.AllReq.LoginRequest.ClientIP).Substring(0, OTSReqRec.AllReq.LoginRequest.ClientIPLength) + "'," +
                                 OTSReqRec.AllReq.LoginRequest.ChkType1 + ",'" + CheckInfo1 + "'," +
                                  OTSReqRec.AllReq.LoginRequest.ChkType2 + ",'" + CheckInfo2 + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        } break;
                case 85:
                    {
                        //if (OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType1InfoLength > 0)
                        //    CheckInfo1 = new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType1Info).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType1InfoLength).ToUpper();

                        //if (OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType2InfoLength > 0)
                        //    CheckInfo2 = OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType2Info.Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType2InfoLength).ToUpper();
                        //else                                                                              
                        //    CheckInfo2 = new string(OTSReqRec.AllReq.LoginRequestTwoFactor.Version, 0, OTSReqRec.AllReq.LoginRequestTwoFactor.VersionLength);

                        //Result = "Exec Proc_LoginUser3 " + "'" + new string





                        switch (OTSReqRec.AllReq.LoginRequestTwoFactor.Journey)
                        {

                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:

                                Result = "Exec PROC_LOGINUSER_2WAY_AUTH " + "'" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.FEUserIDLength)
                                                           //+ "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.Password1).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.Password1Length) 
                                                           + "'," + OTSReqRec.AllReq.LoginRequestTwoFactor.MPin
                                                           + ",'" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceID).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceIDLength).TrimEnd() + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ITS_IP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ITS_IPLength)
                                                           + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ServerIP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ServerIPLength) + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ClientIP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ClientIPLength)
                                                           + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceType).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceTypeLength).TrimEnd() + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.PAN).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.PANLength).TrimEnd()
                                                           + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.DOB).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.DOBLength) + "'," + OTSReqRec.AllReq.LoginRequestTwoFactor.Journey
                                                           + "," + OTSReqRec.AllReq.LoginRequestTwoFactor.OTP + "," + OTSReqRec.AllReq.LoginRequestTwoFactor.NewPin + ",'" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.Version).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.VersionLength).TrimEnd() + "'"
                                                           + "";
                                frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);

                                break;

                        }





                        //Result = "PROC_LOGINUSER_2WAY_AUTH " + "'" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.FEUserIDLength)
                        //    + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.Password1).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.Password1Length) + "','"
                        //    + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.MPin).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.MPinLength)
                        //    + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceID).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceIDLength) + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ITS_IP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ITS_IPLength)
                        //    + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ServerIP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ServerIPLength) + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ClientIP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ClientIPLength)
                        //    + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceType).Substring(0,OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceTypeLength) + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.PAN).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.PANLength)
                        //    + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.DOB).Substring(0,OTSReqRec.AllReq.LoginRequestTwoFactor.DOBLength) + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.Journey).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.JourneyLength)
                        //+ "'";

                        ///*OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType1 + ",'" + CheckInfo1 +*/ "'," +
                        // OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType2 + ",'" + CheckInfo2 + 

                        //                        Result = "PROC_OTP_FOR_2WAY_AUTH " + "'" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.FEUserIDLength)
                        //                            + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.Password1).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.Password1Length) + "','"
                        //                            /* + /*new string(OTSReqRec.AllReq.LoginRequestTwoFactor.Password2).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.Password2Length)*/
                        //                            + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceID).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.DeviceIDLength) + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ITS_IP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ITS_IPLength)
                        //                            + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ServerIP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ServerIPLength) + "','" + new string(OTSReqRec.AllReq.LoginRequestTwoFactor.ClientIP).Substring(0, OTSReqRec.AllReq.LoginRequestTwoFactor.ClientIPLength) + "'," +
                        /////*OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType1 + ",'" + CheckInfo1 +*/ "'," +
                        //// OTSReqRec.AllReq.LoginRequestTwoFactor.ChkType2 + ",'" + CheckInfo2 + 
                        //"'";





                    }
                    break;
                case 4:
                        {
                            // if (OTSReqRec.AllReq.LoginRequestForName.ChkType1InfoLength > 0)
                            //   CheckInfo1 = new string(OTSReqRec.AllReq.LoginRequestForName.ChkType1Info).Substring(0, OTSReqRec.AllReq.LoginRequestForName.ChkType1InfoLength).ToUpper();

                            //    if (OTSReqRec.AllReq.LoginRequestForName.ChkType2InfoLength > 0)
                            //       CheckInfo2 = OTSReqRec.AllReq.LoginRequestForName.ChkType2Info.Substring(0, OTSReqRec.AllReq.LoginRequestForName.ChkType2InfoLength).ToUpper();

                            Result = "Exec Proc_LoginUser3WithName " + "'" + new string(OTSReqRec.AllReq.LoginRequestForName.FEUserID).Substring(0, OTSReqRec.AllReq.LoginRequestForName.FEUserIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        } break;
                    case 5:
                        {
                            Result = "Exec Proc_TraderList " + "'" + new string(OTSReqRec.AllReq.TraderListRequestRec.FEUserID).Substring(0, OTSReqRec.AllReq.TraderListRequestRec.FEUserIDLength)
                               + "'," + OTSReqRec.AllReq.TraderListRequestRec.FEUserType + "," + OTSReqRec.AllReq.TraderListRequestRec.BranchID + "," + OTSReqRec.AllReq.TraderListRequestRec.ReqUserType;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 7:
                        {
                            Result = "Exec Proc_DealerOrderReport " + "'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'," +
                                /*OTSReqRec.AllReq.OrderReportRequestRec.StartTime*/ 0 + "," + /*OTSReqRec.AllReq.OrderReportRequestRec.EndTime*/ 0 + ",'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 8:
                        {
                            Result = "Exec Proc_DealerTradeReport " + "'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'," +
                                /*OTSReqRec.AllReq.OrderReportRequestRec.StartTime*/ 0 + "," + OTSReqRec.AllReq.OrderReportRequestRec.EndTime + ",'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 6:
                        {
                            Result = "Exec Proc_VerifyTrader " + "'" + new string(OTSReqRec.AllReq.VerifyTraderRequestRec.FEUserID).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.FEUserIDLength) + "'," + OTSReqRec.AllReq.VerifyTraderRequestRec.FEUserType + "," + OTSReqRec.AllReq.VerifyTraderRequestRec.BranchID + ",'" +
                                new string(OTSReqRec.AllReq.VerifyTraderRequestRec.TraderID).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.TraderIDLength) + "','" + new string(OTSReqRec.AllReq.VerifyTraderRequestRec.MachineID).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.MachineIDLength) + "','" +
                                new string(OTSReqRec.AllReq.VerifyTraderRequestRec.ITS_IP).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.ITS_IPLength) + "','" + new string(OTSReqRec.AllReq.VerifyTraderRequestRec.ServerIP).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.ServerIPLength) + "','" +
                                 new string(OTSReqRec.AllReq.VerifyTraderRequestRec.ClientIP).Substring(0, OTSReqRec.AllReq.VerifyTraderRequestRec.ClientIPLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 10: //Pending
                        {
                            Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                                OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripCode + ",'" +
                                new string(OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripName).Substring(0, OTSReqRec.AllReq.EquityOrderPlaceRequestRec.ScripNameLength) + "','" + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.BuySell + "'," +
                                 OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Qty + ",'" + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.AtMarket + "'," + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.Rate + ",'" + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.WithSL + "'," +
                                  OTSReqRec.AllReq.EquityOrderPlaceRequestRec.TriggerRate + "," + OTSReqRec.AllReq.EquityOrderPlaceRequestRec.DisclosedQty
                                  + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "'"; //05.09.14
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case 14:
                    {
                        Result = "Proc_GetIVRDealerDetails " + "'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'";
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
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_PLACE_REQ:
                        {
                            if (OTSReqRec.AllReq.OrderTypePlaceReqRec.ExchType == 'C')
                            {
                                ScNameForOType = new string(OTSReqRec.AllReq.OrderTypePlaceReqRec.ScripName).Substring(0, OTSReqRec.AllReq.OrderTypePlaceReqRec.ScripNameLength);
                            }
                            else
                            {
                                ScNameForOType = MakeDerivDescForOrderType(OTSReqRec.AllReq.OrderTypePlaceReqRec.Exch, OTSReqRec.AllReq.OrderTypePlaceReqRec.CPType, OTSReqRec.AllReq.OrderTypePlaceReqRec.ExpiryDate,
                                                  OTSReqRec.AllReq.OrderTypePlaceReqRec.StrikePrice, OTSReqRec.AllReq.OrderTypePlaceReqRec.ScripName, OTSReqRec.AllReq.OrderTypePlaceReqRec.ScripNameLength);
                            } //for FnO, DiscQty should come 0. Check scripname.
                        //if (OTSReqRec.AllReq.OrderTypePlaceReqRec.Reserved2 == 3)
                        //{
                        //    int CoverDate = getEpochTime(DateTime.Now);
                        //    OTSReqRec.AllReq.OrderTypePlaceReqRec.Reserved1 = CoverDate;
                        //}

                            Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                                OTSReqRec.AllReq.OrderTypePlaceReqRec.Exch + "','" + OTSReqRec.AllReq.OrderTypePlaceReqRec.ExchType + "'," + OTSReqRec.AllReq.OrderTypePlaceReqRec.ScripCode + ",'" +
                                ScNameForOType + "','" + OTSReqRec.AllReq.OrderTypePlaceReqRec.BuySell + "'," +
                                 OTSReqRec.AllReq.OrderTypePlaceReqRec.Qty + ",'" + OTSReqRec.AllReq.OrderTypePlaceReqRec.AtMarket + "'," + OTSReqRec.AllReq.OrderTypePlaceReqRec.Rate + ",'" + OTSReqRec.AllReq.OrderTypePlaceReqRec.WithSL + "'," +
                                  OTSReqRec.AllReq.OrderTypePlaceReqRec.TriggerRate + "," + OTSReqRec.AllReq.OrderTypePlaceReqRec.DisclosedQty
                                  + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC)
                             + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + GetOrderType(OTSReqRec.AllReq.OrderTypePlaceReqRec.OrderType) + "'," + OTSReqRec.AllReq.OrderTypePlaceReqRec.AllowBannedScrip + ",'" + GetOrderTypeNew(OTSReqRec.AllReq.OrderTypePlaceReqRec.Reserved2) + "'"  + "," +
                        OTSReqRec.AllReq.OrderTypePlaceReqRec.AlgoID;
                        frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_PLACE_REQ_COVER: //171   // sumitp 28_jan_22
                    {
                        int CoverDate = getEpochTime(DateTime.Now);
                        OTSReqRec.AllReq.PlaceCoverOrderReqRec.EpochTime = CoverDate;

                        if (OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExchType == 'C')
                        {
                            ScNameForOType = new string(OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripName).Substring(0, OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripNameLength);
                        }
                        else
                        {
                            ScNameForOType = MakeDerivDescForOrderType(OTSReqRec.AllReq.PlaceCoverOrderReqRec.Exch, OTSReqRec.AllReq.PlaceCoverOrderReqRec.CPType, OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExpiryDate,
                                              OTSReqRec.AllReq.PlaceCoverOrderReqRec.StrikePrice, OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripName, OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripNameLength);
                        } //for FnO, DiscQty should come 0. Check scripname.

                        //OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderTypeNew = OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderTypeNew == 1 ? "CO" : "BO";
                        Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.PlaceCoverOrderReqRec.Exch + "','" + OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExchType + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripCode + ",'" +
                            ScNameForOType + "','" + OTSReqRec.AllReq.PlaceCoverOrderReqRec.BuySell + "'," +
                             OTSReqRec.AllReq.PlaceCoverOrderReqRec.Qty + ",'" + OTSReqRec.AllReq.PlaceCoverOrderReqRec.AtMarket + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.Rate + ",'" + "N" + "'," +
                              /*OTSReqRec.AllReq.PlaceCoverOrderReqRec.TriggerRate*/ 0 + "," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.DisclosedQty
                              + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC)
                         + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + GetOrderType(OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderType) + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.AllowBannedScrip + ",'" + GetOrderTypeNew(OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderTypeNew) + "'" + ",'" + OTSReqRec.AllReq.PlaceCoverOrderReqRec.SLRateCO + "'," +
                        OTSReqRec.AllReq.PlaceCoverOrderReqRec.EpochTime; //OTSReqRec.AllReq.PlaceCoverOrderReqRec.SLRateCO + ;



                        if (OTSReqRec.AllReq.PlaceCoverOrderReqRec.BuySell.ToString() == "B")
                        {
                            ResultCo = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.PlaceCoverOrderReqRec.Exch + "','" + OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExchType + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripCode + ",'" +
                            ScNameForOType + "','" + (OTSReqRec.AllReq.PlaceCoverOrderReqRec.BuySell == 'B' ? 'S' :'B') + "'," +
                             OTSReqRec.AllReq.PlaceCoverOrderReqRec.Qty + ",'" + "N" + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.SLRateCO + ",'" + "Y" + "'," +
                              OTSReqRec.AllReq.PlaceCoverOrderReqRec.TriggerRate + "," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.DisclosedQty
                              + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC)
                         + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + GetOrderType(OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderType) + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.AllowBannedScrip + ",'" + GetOrderTypeNew(OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderTypeNew) + "'";

                            ResultCo = ResultCo + "," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.EpochTime; //issue:  IN BSE EXCH 'B' was getting replace with 'S'    
                            //ResultCo = ResultCo.Replace(",'B',", ",'S',") + "," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.EpochTime;
                        }
                        else if (OTSReqRec.AllReq.PlaceCoverOrderReqRec.BuySell.ToString() == "S")
                        {
                            ResultCo = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.PlaceCoverOrderReqRec.Exch + "','" + OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExchType + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripCode + ",'" +
                            ScNameForOType + "','" + (OTSReqRec.AllReq.PlaceCoverOrderReqRec.BuySell == 'S' ? 'B' : 'S') + "'," +
                             OTSReqRec.AllReq.PlaceCoverOrderReqRec.Qty + ",'" + "N" + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.SLRateCO + ",'" + "Y" + "'," +
                              OTSReqRec.AllReq.PlaceCoverOrderReqRec.TriggerRate + "," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.DisclosedQty
                              + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC)
                         + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + GetOrderType(OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderType) + "'," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.AllowBannedScrip + ",'" + GetOrderTypeNew(OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderTypeNew) + "'";

                            ResultCo = ResultCo + "," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.EpochTime;  //issue:  IN BSE EXCH 'B' was getting replace with 'S'    
                            //ResultCo = ResultCo.Replace(",'S',", ",'B',") + "," + OTSReqRec.AllReq.PlaceCoverOrderReqRec.EpochTime;
                        }


                        try
                        {
                            Result = Result.Replace(",'" + OTSReqRec.AllReq.PlaceCoverOrderReqRec.SLRateCO + "'", "");
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                            frm.LogReqToDbList.Add(ResultCo + "," + DateTime.Now);

                            //Result = Result + "," + CoverDate;
                            Result = Result + " ; " + ResultCo;

                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    break;
                case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                        {
                            if (OTSReqRec.AllReq.OrderTypeModifyReqRec.AHStatus != 'Y') OTSReqRec.AllReq.OrderTypeModifyReqRec.AHStatus = 'N';
                            if (OTSReqRec.AllReq.OrderTypeModifyReqRec.ExchType == 'C')
                                ScNameForOType = new string(OTSReqRec.AllReq.OrderTypeModifyReqRec.ScripName).Substring(0, OTSReqRec.AllReq.OrderTypeModifyReqRec.ScripNameLength);
                            else
                                ScNameForOType = MakeDerivDescForOrderType(OTSReqRec.AllReq.OrderTypeModifyReqRec.Exch, OTSReqRec.AllReq.OrderTypeModifyReqRec.CPType, OTSReqRec.AllReq.OrderTypeModifyReqRec.ExpiryDate,
                                                  OTSReqRec.AllReq.OrderTypeModifyReqRec.StrikePrice, OTSReqRec.AllReq.OrderTypeModifyReqRec.ScripName, OTSReqRec.AllReq.OrderTypeModifyReqRec.ScripNameLength);

                            Result = "Exec Proc_ModifyEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.OrderTypeModifyReqRec.Exch + "','" + OTSReqRec.AllReq.OrderTypeModifyReqRec.ExchType + "'," + OTSReqRec.AllReq.OrderTypeModifyReqRec.ScripCode + ",'" +
                               ScNameForOType + "','" + OTSReqRec.AllReq.OrderTypeModifyReqRec.BuySell + "'," +
                                OTSReqRec.AllReq.OrderTypeModifyReqRec.Qty + ",'" + OTSReqRec.AllReq.OrderTypeModifyReqRec.AtMarket + "'," + OTSReqRec.AllReq.OrderTypeModifyReqRec.Rate + ",'" +
                                 OTSReqRec.AllReq.OrderTypeModifyReqRec.WithSL + "'," + OTSReqRec.AllReq.OrderTypeModifyReqRec.TriggerRate + "," + OTSReqRec.AllReq.OrderTypeModifyReqRec.OldQty + "," +
                                 OTSReqRec.AllReq.OrderTypeModifyReqRec.TradedQty + "," + OTSReqRec.AllReq.OrderTypeModifyReqRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.OrderTypeModifyReqRec.AHStatus + "'," +
                                 OTSReqRec.AllReq.OrderTypeModifyReqRec.DisclosedQty + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND)
                                 + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + GetOrderType(OTSReqRec.AllReq.OrderTypeModifyReqRec.OrderType) + "'"; //04.05.15
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                        {
                            if (OTSReqRec.AllReq.OrderTypeCancelReqRec.AHStatus != 'Y') OTSReqRec.AllReq.OrderTypeCancelReqRec.AHStatus = 'N';
                            if (OTSReqRec.AllReq.OrderTypeCancelReqRec.ExchType == 'C')
                                ScNameForOType = new string(OTSReqRec.AllReq.OrderTypeCancelReqRec.ScripName).Substring(0, OTSReqRec.AllReq.OrderTypeCancelReqRec.ScripNameLength);
                            else
                                ScNameForOType = MakeDerivDescForOrderType(OTSReqRec.AllReq.OrderTypeCancelReqRec.Exch, OTSReqRec.AllReq.OrderTypeCancelReqRec.CPType, OTSReqRec.AllReq.OrderTypeCancelReqRec.ExpiryDate,
                                                  OTSReqRec.AllReq.OrderTypeCancelReqRec.StrikePrice, OTSReqRec.AllReq.OrderTypeCancelReqRec.ScripName, OTSReqRec.AllReq.OrderTypeCancelReqRec.ScripNameLength);

                            Result = "Exec Proc_CancelEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                              OTSReqRec.AllReq.OrderTypeCancelReqRec.Exch + "','" + OTSReqRec.AllReq.OrderTypeCancelReqRec.ExchType + "'," + OTSReqRec.AllReq.OrderTypeCancelReqRec.ScripCode + ",'" +
                              ScNameForOType + "','" + OTSReqRec.AllReq.OrderTypeCancelReqRec.BuySell + "'," +
                               OTSReqRec.AllReq.OrderTypeCancelReqRec.Qty + ",'" + OTSReqRec.AllReq.OrderTypeCancelReqRec.AtMarket + "'," + OTSReqRec.AllReq.OrderTypeCancelReqRec.Rate + ",'" + OTSReqRec.AllReq.OrderTypeCancelReqRec.WithSL + "'," +
                                OTSReqRec.AllReq.OrderTypeCancelReqRec.TriggerRate + "," + OTSReqRec.AllReq.OrderTypeCancelReqRec.TradedQty + "," + OTSReqRec.AllReq.OrderTypeCancelReqRec.ExchangeOrderID + ",'" + OTSReqRec.AllReq.OrderTypeCancelReqRec.AHStatus + "'"
                                + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) +
                                "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + GetOrderType(OTSReqRec.AllReq.OrderTypeCancelReqRec.OrderType) + "'"; //04.15.15
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
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
                                 OTSReqRec.AllReq.EquityOrderModifyRequestRec.DisclosedQty
                                 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "'"; //05.09.14
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_DERIVCONVERSION:
                    case Constants.OTS_ORDERTYPE_CONVERSION:
                        {
                            Result = "Exec Proc_ConvertIntraDeliveryOrder " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.OrderTypeConversionRec.Exch + "','" + OTSReqRec.AllReq.OrderTypeConversionRec.ExchType + "'," + OTSReqRec.AllReq.OrderTypeConversionRec.ScripCode + ",'" +
                            new string(OTSReqRec.AllReq.OrderTypeConversionRec.ScripName).Substring(0, OTSReqRec.AllReq.OrderTypeConversionRec.ScripNameLength) + "','" + OTSReqRec.AllReq.OrderTypeConversionRec.BuySell + "'," +
                            OTSReqRec.AllReq.OrderTypeConversionRec.Qty + "," + OTSReqRec.AllReq.OrderTypeConversionRec.Rate + "," + OTSReqRec.AllReq.OrderTypeConversionRec.ExchangeOrderID + "," + OTSReqRec.AllReq.OrderTypeConversionRec.ExchangeTradeID +
                            ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC)
                            + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + GetOrderType(OTSReqRec.AllReq.OrderTypeConversionRec.OrderType) + "'";
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
                                OTSReqRec.AllReq.EquityOrderCancelRequestRec.TriggerRate + "," + OTSReqRec.AllReq.EquityOrderCancelRequestRec.TradedQty + "," + OTSReqRec.AllReq.EquityOrderCancelRequestRec.ExchangeOrderID + ",'" + OTSReqRec.AllReq.EquityOrderCancelRequestRec.AHStatus + "'"
                                + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "'"; //05.09.14
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 41:
                        {
                            Result = "Exec Proc_GetScripMargin " + "'" + OTSReqRec.AllReq.ScripMarginRequestRec.Exch + "'," + OTSReqRec.AllReq.ScripMarginRequestRec.ScCode;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    //2015.03.26 Add case to execute procedure
                    case Constants.OTS_SECURITY_INFO:
                        {
                            Result = "Exec Proc_GetSecurityInfo " + "'" + OTSReqRec.AllReq.ScripInfoRequestRec.Exch + "'," + OTSReqRec.AllReq.ScripInfoRequestRec.ScCode;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    //2015.03.26 Add case to execute procedure
                    case Constants.OTS_SCRIP_MARGIN:
                        {
                            Result = "Exec Proc_GetScripMargin " + "'" + OTSReqRec.AllReq.ScripMarginRequestRec.Exch + "'," + OTSReqRec.AllReq.ScripMarginRequestRec.ScCode + ",'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ://34 BseMf Krutika
                        {
                            Result = "Exec Proc_SIPRegistrationBSEMF" + "'" + "P" + "'," + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.MfSIPRegistrationReqRec.Exch + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.ExchType + "','"
                            + new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.UniqueNo).Substring(0, OTSReqRec.AllReq.MfSIPRegistrationReqRec.UniqueNoLength) + "','" +
                            new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.SchemeCode).Substring(0, OTSReqRec.AllReq.MfSIPRegistrationReqRec.SchemeCodeLength) + "','"
                            + OTSReqRec.AllReq.MfSIPRegistrationReqRec.TxnMode + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.DptxnType + "','" + new string (OTSReqRec.AllReq.MfSIPRegistrationReqRec.FolioNo).Substring(0,OTSReqRec.AllReq.MfSIPRegistrationReqRec.FolioNoLength) + "','"+
                             new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.FrequencyType).Substring(0, OTSReqRec.AllReq.MfSIPRegistrationReqRec.FrequencyTypeLength) + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.Amount + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.Firstordertodayflag + "','" +
                             new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.StartDate).TrimEnd() + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.Numberofinstallments + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.SIPregistrationNum + "','" + new string (OTSReqRec.AllReq.MfSIPRegistrationReqRec.EndDate).TrimEnd() + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);

                        }break;
                    case Constants.OTS_MANDATE://34 BseMf Krutika
                        {
                            Result = "Exec Proc_MandateRegistrationBSEMF" + "'" + "P" + "'," + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.XSIPMandateRec.Exch + "','" + OTSReqRec.AllReq.XSIPMandateRec.ExchType + "','"
                            + OTSReqRec.AllReq.XSIPMandateRec.Amount + "','" +
                            OTSReqRec.AllReq.XSIPMandateRec.MandateType+ "','"
                            + OTSReqRec.AllReq.XSIPMandateRec.AccountNo + "','" + new string(OTSReqRec.AllReq.XSIPMandateRec.ACtype).Substring(0, OTSReqRec.AllReq.XSIPMandateRec.ACtype.Length) + "','" + new string(OTSReqRec.AllReq.XSIPMandateRec.IFSC).Substring(0, OTSReqRec.AllReq.XSIPMandateRec.IFSC.Length) + "','" +
                             new string(OTSReqRec.AllReq.XSIPMandateRec.MICR).Substring(0, OTSReqRec.AllReq.XSIPMandateRec.MICR.Length)  + "','" +
                             new string(OTSReqRec.AllReq.XSIPMandateRec.StartDate).TrimEnd() + "','" + new string(OTSReqRec.AllReq.XSIPMandateRec.EndDate).TrimEnd() + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);

                        } break;
                case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ://34 BseMf Krutika
                     {
                        Result = "Exec Proc_XSIPRegistrationBSEMF" + "'" + "P" + "'," + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                           OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Exch + "','" + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.ExchType + "','"
                           + new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.UniqueNo).Substring(0, OTSReqRec.AllReq.XMfSIPRegistrationReqRec.UniqueNoLength) + "','" +
                           new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SchemeCode).Substring(0, OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SchemeCodeLength) + "','"
                           + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.TxnMode + "','" + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.DptxnType + "','" + new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FolioNo).Substring(0, OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FolioNoLength) + "','" +
                            new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FrequencyType).Substring(0, OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FrequencyTypeLength) + "','" + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Amount + "','" + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Firstordertodayflag + "','" +
                            new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.StartDate).TrimEnd() + "','" + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Numberofinstallments + "','" + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SIPregistrationNum + "','" + new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.EndDate).TrimEnd() + "','" + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Mandate + "','" + OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Brokerage + "'";
                        frm.LogReqToDbList.Add(Result + "," + DateTime.Now);

                    }
                    break;
                case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ://35 BseMf Krutika
                        {
                            Result = "Exec Proc_SIPCancellationBSEMF" + "'" + "C" + "'," + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.MfSIPRegistrationReqRec.Exch + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.ExchType + "','"
                            + new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.UniqueNo).Substring(0, OTSReqRec.AllReq.MfSIPRegistrationReqRec.UniqueNoLength) + "','" +
                            new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.SchemeCode).Substring(0, OTSReqRec.AllReq.MfSIPRegistrationReqRec.SchemeCodeLength) + "','"
                            + OTSReqRec.AllReq.MfSIPRegistrationReqRec.TxnMode + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.DptxnType + "','" + new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.FolioNo).Substring(0, OTSReqRec.AllReq.MfSIPRegistrationReqRec.FolioNoLength) + "','" +
                             new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.FrequencyType).Substring(0, OTSReqRec.AllReq.MfSIPRegistrationReqRec.FrequencyTypeLength) + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.Amount + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.Firstordertodayflag + "','" +
                             new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.StartDate).TrimEnd() + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.Numberofinstallments + "','" + OTSReqRec.AllReq.MfSIPRegistrationReqRec.SIPregistrationNum + "','" + new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.EndDate).TrimEnd() + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);

                        } break;
                    case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                    {
                        
                            Result = "Exec Proc_PlaceBSEMFOrderRequest" + "'" + "P" + "'," + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                             OTSReqRec.AllReq.MfAddModifyCancelReqRec.Exch + "','" + OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchType + "','"
                             + new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNo).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNoLength) + "','" +
                             new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCode).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCodeLength) + "','"
                             + OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySell + "','" +
                             new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellType).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellTypeLength) +
                             "','" + OTSReqRec.AllReq.MfAddModifyCancelReqRec.DptxnType + "','"
                             + OTSReqRec.AllReq.MfAddModifyCancelReqRec.Amount + "','"
                             + OTSReqRec.AllReq.MfAddModifyCancelReqRec.KYCStatus + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        
                        } break;
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44 BseMf
                        {
                            Result = "Exec Proc_ModifyBseMfOrderRequest" + "'" + "M" + "'," + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.MfAddModifyCancelReqRec.Exch + "','" + OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchType + "','"
                            + new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNo).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNoLength) + "','" +
                            new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCode).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCodeLength) + "','"
                            + OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySell + "','" +
                            new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellType).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellTypeLength) +
                            "','" + OTSReqRec.AllReq.MfAddModifyCancelReqRec.DptxnType + "','"
                            + OTSReqRec.AllReq.MfAddModifyCancelReqRec.Amount + "','"
                            + OTSReqRec.AllReq.MfAddModifyCancelReqRec.KYCStatus + "','" + OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchOrderNo + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        } break;
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45 BseMf
                        {

                            Result = "Exec Proc_CancelBseMfOrderRequest" + "'" + "C" + "'," + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.MfAddModifyCancelReqRec.Exch + "','" + OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchType + "','"
                            + new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNo).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNoLength) + "','" +
                            new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCode).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCodeLength) + "','"
                            + OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySell + "','" +
                            new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellType).Substring(0, OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellTypeLength) +
                            "','" + OTSReqRec.AllReq.MfAddModifyCancelReqRec.DptxnType + "','"
                            + OTSReqRec.AllReq.MfAddModifyCancelReqRec.Amount + "','"
                            + OTSReqRec.AllReq.MfAddModifyCancelReqRec.KYCStatus + "','" + OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchOrderNo + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        } break;
                    //case 61:
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC: //16.1.15
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142:    //Added on 5-Apr-2018 - Rohan
                        {
                            Result = "Exec Proc_EquityTradeReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                OTSReqRec.AllReq.TradeReportRequestRec.StartTime + "," + OTSReqRec.AllReq.TradeReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW:    //Added on 18-Apr-2018 - Rohan
                        {
                            Result = "Exec Proc_EquityTradeReportWithSpread " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                OTSReqRec.AllReq.TradeReportRequestRec.StartTime + "," + OTSReqRec.AllReq.TradeReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER:    //COVER TRADE REPORT     // sumitp 28_jan_22
                    {
                        Result = "Exec Proc_EquityTradeReport_CO " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                            OTSReqRec.AllReq.TradeReportRequestRec.StartTime + "," + OTSReqRec.AllReq.TradeReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_EXERCISE_REPORTS_REC: //Options Exercise
                        {
                            Result = "Exec Proc_ExerciseReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                OTSReqRec.AllReq.ExerciseReportRequestRec.StartTime + "," + OTSReqRec.AllReq.ExerciseReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    //case 62:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC: //16.1.15
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141: //Added on 5-Apr-2018 - Rohan
                        {
                            Result = "Exec Proc_EquityOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                OTSReqRec.AllReq.OrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.OrderReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";

                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_COVER_BRACKET: //78     // added by Himanshu   25-nov-21  // sumitp 28_jan_22
                    {
                        Result = "Exec Proc_EquityOrderReport_CO " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                            OTSReqRec.AllReq.OrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.OrderReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";

                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }break;
                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR:
                        {
                            Result = "Exec Proc_EquityOrderReportWithRejectedOrder " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                OTSReqRec.AllReq.OrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.OrderReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW: //Added on 18-Apr-2018 - Rohan
                        {
                            Result = "Exec Proc_EquityOrderReportWithSpread " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                OTSReqRec.AllReq.OrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.OrderReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                
                //case 64:
                case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC:
                    case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW: // 24.09.16 Hetal
                        {
                            Result = "Exec Proc_EquityOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                 OTSReqRec.AllReq.ActivityLogRequestRec.StartTime + "," + OTSReqRec.AllReq.ActivityLogRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new: // sumit 21_jan_22 single activity log
                    {
                        Result = "Exec Proc_EquityOrderReport_ExchOrderID " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                             OTSReqRec.AllReq.ActivityLogRequestRecord_new.StartTime + "," + OTSReqRec.AllReq.ActivityLogRequestRecord_new.EndTime + ",'" + OrderRequesterId + "'," +
                                 OTSReqRec.AllReq.ActivityLogRequestRecord_new.ScripCode + ",'" + OTSReqRec.AllReq.ActivityLogRequestRecord_new.ExchangeOrderID
                                 + "','" + OTSReqRec.AllReq.ActivityLogRequestRecord_new.Exch + "','" + OTSReqRec.AllReq.ActivityLogRequestRecord_new.ExchType + "','" + OTSReqRec.AllReq.ActivityLogRequestRecord_new.Status + "'," + OTSReqRec.AllReq.ActivityLogRequestRecord_new.BrokerOrderID + ";";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW:  //Added on 18-Apr-2018 - Rohan
                        {
                            Result = "Exec Proc_EquityOrderReportWithSpread " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                 OTSReqRec.AllReq.ActivityLogRequestRec.StartTime + "," + OTSReqRec.AllReq.ActivityLogRequestRec.EndTime + ",'" + OrderRequesterId + "'";

                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
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

                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 68:
                        {
                            Result = "select ClientCode,DbNum from Clients Where ClientCode like " + "'" + (char)OTSReqRec.AllReq.CliDBNumRequestRec.DummyByte + "%" + "'" + " Order by ClientCode";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 69:
                        {
                            Result = "Exec Proc_HoldingsReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 70:
                        {
                            Result = "Exec Proc_GetHoldingsMargin " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 218:
                        {
                            Result = "Exec Proc_GetHoldingsMargin_MTF " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_HOLDING_VALUATION_REPORT_NEW: //20.05.15
                        {
                            Result = "Exec Proc_GetHoldingsMarginWithLTP " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 73:
                        {
                            Result = "select cast(floor(cast(getdate() as float)) + 2 as int) as DD";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 74:
                        {
                            Result = "Exec Proc_HoldingsReportWithCollateralQty " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 75:
                        {
                            Result = "Exec Proc_AllClientsHoldingsReport " + "'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 76:    //Added on 08-Nov-2016 - Rohan
                        {
                            Result = "Exec Proc_AllClientsHoldingsEquityReport " + "'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 79:
                        {
                            Result = "Exec Proc_GetClientPos " + "'" + new string(OTSReqRec.AllReq.ClientPositionRequestRec.ClientCode).Substring(0, OTSReqRec.AllReq.ClientPositionRequestRec.ClientCodeLength) + "'," +
                                OTSReqRec.AllReq.ClientPositionRequestRec.ScripCode;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 80:
                        {
                            Result = "Exec Proc_GetTradeForTradeScrips"; //No parameters
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case 160:
                    {
                        //Added on 20.03.23 Siddhesh For Getting Surveillance Indicator Messages 
                        Result = "Exec Proc_SurveillanceIndicterMassagesNse";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case 161:
                    {
                        //Added on 21.03.23 Siddhesh For Getting Surveillance Indicator Messages 
                        Result = "Exec Proc_SurveillanceIndicterMassagesBse";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case 81:
                        {
                            Result = "Exec Proc_GetBseNseMap"; //No parameters
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 82:
                        {
                            Result = "Exec Proc_NseFnOBannedScripReport"; //No parameters
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 83:
                        {
                            Result = "Exec Proc_GetAdhoc " + "'" + new string(OTSReqRec.AllReq.AdhocReportRequestRec.AdminCode).Substring(0, OTSReqRec.AllReq.AdhocReportRequestRec.AdminCodeLength)
                               + "','" + new string(OTSReqRec.AllReq.AdhocReportRequestRec.ClientCode).Substring(0, OTSReqRec.AllReq.AdhocReportRequestRec.ClientCodeLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 84:
                        {
                            Result = "Exec Proc_ModifyAdhoc " + "'" + new string(OTSReqRec.AllReq.AdhocModifyRequestRec.ClientCode).Substring(0, OTSReqRec.AllReq.AdhocModifyRequestRec.ClientCodeLength)
                               + "'," + OTSReqRec.AllReq.AdhocModifyRequestRec.AvlbMgn + "," + OTSReqRec.AllReq.AdhocModifyRequestRec.OldAdhoc + "," + OTSReqRec.AllReq.AdhocModifyRequestRec.NewAdhoc;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 86:
                        {
                            Result = "Exec Proc_GetMaalFodoClients " + OTSReqRec.AllReq.MFClientsReportRequestRec.ClientType + "," + OTSReqRec.AllReq.MFClientsReportRequestRec.MinimumDebit;
                            MFDebitAmt = OTSReqRec.AllReq.MFClientsReportRequestRec.MinimumDebit;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 87:
                        {
                            Result = "Exec Proc_GetMaalFodoClientHoldings " + OTSReqRec.AllReq.MFClientHoldingReportRequestRec.ClientType + "," + OTSReqRec.AllReq.MFClientHoldingReportRequestRec.MinimumDebit;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 89:
                        {
                            Result = "Exec Proc_IlliquidReport"; //No parameters
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 92://BseMf Krutika
                        {
                            Result = "Exec Proc_HoldingsReportOf_MF " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_ORDER_REPORTS_REC://Bsemf Krutika
                        {
                            Result = "Exec Proc_BseMfOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                                      OTSReqRec.AllReq.MfOrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.MfOrderReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        } break;
                case Constants.OTS_Acitivitylog_MF_ORDER_REPORTS_REC://Bsemf Krutika
                    {
                        Result = "Exec Proc_BseMfOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                                  OTSReqRec.AllReq.MfActivitylogRequestRec.StartTime + "," + OTSReqRec.AllReq.MfActivitylogRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC://Bsemf Krutika
                        {
                            Result = "Exec Proc_BseMfSIPOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                                      OTSReqRec.AllReq.MfOrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.MfOrderReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        } break;
                case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC://Bsemf Krutika
                    {
                        Result = "Exec Proc_BseMfMandateOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                                                  OTSReqRec.AllReq.MandateOrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.MandateOrderReportRequestRec.EndTime + ",'" + OrderRequesterId + "'";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case Constants.OTS_HOLDING_REPORT://94  //19.01.17
                        {
                            Result = "Exec Proc_HoldingsReportWithCollateralQtyNew " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_HOLDING_REPORT_PreClose://106 jinal 
                        {
                            Result = "Exec Proc_HoldingsReportWithCollateralQtyPreClose  " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case Constants.OTS_HOLDING_REPORT_PreClose_FS://112 Himanshu Soni 
                    {
                        Result = "Exec Proc_HoldingsReportWithCollateralQtyPreClose  " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case 67://106 jinal 
                        {
                            Result = "Exec Proc_HoldingsReportWithCollateralQtyPreClose " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case 90://106 jinal 
                    {
                        Result = "Exec Proc_HoldingsReportWithCollateralQtyPreClose " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case 194://194 // new holding sumit
                    {
                        Result = "Exec Proc_HoldingsReportWithCollateralQtyPreClose " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case Constants.OTS_ALLCLIENTS_HOLDING_REPORT://95 //31.01.17
                        {
                            Result = "Exec Proc_AllClientsHoldingsReportNew " + "'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_ALLCLIENTS_HOLDING_REPORT_New://95 //31.01.17
                        {
                            Result = "Exec Proc_AllClientsHoldingsReportWithPreClose " + "'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_VERIFY_USER://99 
                        {
                            Result = "Exec Proc_GetUserVerification" + " '" + new string(OTSReqRec.AllReq.VerifyUserRequestRec.UserId) + "','" + new string(OTSReqRec.AllReq.VerifyUserRequestRec.Pan) + "','" + new string(OTSReqRec.AllReq.VerifyUserRequestRec.Dob) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 110:
                        {
                            //DisclosedQty not allowed in f&o but we use same sql proc for eq and fo. so we send 0
                            strName = MakeDerivDesc(OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Exch, OTSReqRec.AllReq.DerivOrderPlaceRequestRec.DerivInfo);

                            Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.BuySell + "'," + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Qty + ",'" + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.AtMarket + "'," + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.Rate
                               + ",'" + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.WithSL + "'," + OTSReqRec.AllReq.DerivOrderPlaceRequestRec.TriggerRate + "," + 0
                               + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "'," + 0; //18.10.14 0 For Currency & User Info for Nse FnO
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 115:
                        {
                            //13.04.16 Structure used here was wrong, so using AHDerivOrderPlaceRequestRec
                            strName = MakeDerivDesc(OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Exch, OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.DerivInfo);

                            Result = "Exec Proc_ConvertAH2LiveOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.ScripCode + ",'" +
                            strName + "','" + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.BuySell + "'," + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Qty + "," +
                            OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.Rate + ",'" + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.WithSL + "'," +
                              OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.TriggerRate + "," + OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec.ExchangeOrderID;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
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
                                 OTSReqRec.AllReq.DerivOrderModifyRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.DerivOrderModifyRequestRec.AHStatus + "'," + 0
                                 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "'," + 0; //18.10.14 0 For Currency & User Info for Nse FnO
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 130:
                        {
                            if (OTSReqRec.AllReq.DerivOrderCancelRequestRec.AHStatus != 'Y')
                                OTSReqRec.AllReq.DerivOrderCancelRequestRec.AHStatus = 'N';

                            //13.04.16 Structure used here was wrong for Exch, Modification: DerivOrderModifyRequestRec was used, so using DerivOrderCancelRequestRec
                            strName = MakeDerivDesc(OTSReqRec.AllReq.DerivOrderCancelRequestRec.Exch, OTSReqRec.AllReq.DerivOrderCancelRequestRec.DerivInfo);

                            Result = "Exec Proc_CancelEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.DerivOrderCancelRequestRec.Exch + "','" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.ExchType + "'," + OTSReqRec.AllReq.DerivOrderCancelRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.BuySell + "'," + OTSReqRec.AllReq.DerivOrderCancelRequestRec.Qty + ",'" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.AtMarket + "'," +
                               OTSReqRec.AllReq.DerivOrderCancelRequestRec.Rate + ",'" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.WithSL + "'," +
                                 OTSReqRec.AllReq.DerivOrderCancelRequestRec.TriggerRate + "," + OTSReqRec.AllReq.DerivOrderCancelRequestRec.TradedQty + "," +
                                 OTSReqRec.AllReq.DerivOrderCancelRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.DerivOrderCancelRequestRec.AHStatus + "'"
                                 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "'," + 0; //18.10.14 0 For Currency & User Info for Nse FnO
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case 134:
                    {
                        char ReqType = 'S';
                        switch (OTSReqRec.AllReq.orderPlaceReqRecForApproval.RequestType)
                        {
                            case 10:
                            case 110:
                            case 211:
                            case 17: ReqType = 'P'; break;
                            case 20:
                            case 120:
                            case 221:
                            case 18: ReqType = 'M'; break;
                            case 30:
                            case 130:
                            case 231:
                            case 19: ReqType = 'C'; break;
                        }

                        string ScName, Symbol = "";
                        Symbol = new string(OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripName).Substring(0, OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripNameLength);
                        if (OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchType == 'D')
                        {
                            //ScName = MakeDerivDesc(OTSReqRec.AllReq.orderPlaceReqRecForApproval.Exch, OTSReqRec.AllReq.orderPlaceReqRecForApproval.CPType,
                            //OTSReqRec.AllReq.orderPlaceReqRecForApproval.StrikePrice, OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExpiryDate, OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripName, OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripNameLength);
                            ScName = new string(OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripName).Substring(0, OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripNameLength);
                        }
                        else ScName = Symbol;
                        OTPNumber = RandomGenerator.Next(100000, 999999);

                        //Result = "exec Proc_PlaceOrderReqForApproval 'DEMO1','ITDEMO1','N','C',22,'ACC','B',10,0,'Y','P',2250.00,'N','N',0,'D',123456,'111111111111128','1111111111111088',17,'N',0,10,0,0,0,1,'ACC',0,0,0,0";



                        Result = "Exec Proc_PlaceOrderReqForApproval " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                        OTSReqRec.AllReq.orderPlaceReqRecForApproval.Exch + "','" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchType + "'," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripCode + ",'" +
                        ScName + "','" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.BuySell + "'," +
                        OTSReqRec.AllReq.orderPlaceReqRecForApproval.Qty + "," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.DisclosedQty + ",'" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.AtMarket + "','" + ReqType + "'," +
                        OTSReqRec.AllReq.orderPlaceReqRecForApproval.Rate + ",'" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.IOC + "','" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.WithSL + "'," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.TriggerRate + ",'" +
                        OTSReqRec.AllReq.orderPlaceReqRecForApproval.OrderType + "'," + OTPNumber + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "'," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.RequestType + ",'" +
                        OTSReqRec.AllReq.orderPlaceReqRecForApproval.SLTriggered + "'," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.TradedQty + "," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.PendingQty + "," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.OldQty + "," +
                        OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchangeOrderId + "," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchangeOrderTime + "," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.IntraMultiplierReq + ",'" + Symbol + "'," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.InstrType + "," +
                        OTSReqRec.AllReq.orderPlaceReqRecForApproval.CPType + "," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExpiryDate + "," + OTSReqRec.AllReq.orderPlaceReqRecForApproval.StrikePrice;
                        frm.LogReqToDbList.Add(OTSReqRec.ReqTime + ":- " + Result);

                        try
                        {




                            // var client = new RestClient($"{Form1.OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}&EmailId={EmailAdd}&MobNo={MobileNo}");
                            string IsBuySell = "", Exch;
                            if (OTSReqRec.AllReq.orderPlaceReqRecForApproval.BuySell == 'B')
                            {
                                IsBuySell = "BUY";
                            }
                            else
                            {
                                IsBuySell = "SELL";
                            }

                            if (OTSReqRec.AllReq.orderPlaceReqRecForApproval.Exch == 'N')
                            {
                                Exch = "NSE";
                            }
                            else if (OTSReqRec.AllReq.orderPlaceReqRecForApproval.Exch == 'B')
                            {
                                Exch = "BSE";
                            }
                            else
                            {
                                Exch = "";
                            }

                            //string IVROTPString = $"{Form1.IVROTPLink}?LoginId={new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength)}&OTP={OTPNumber}" +
                            //    $"&OrderDetails={Exch.TrimEnd()}" + $"%20{ScName.TrimEnd()}" + $"%20{IsBuySell.TrimEnd()}" + " Qty/Lot" + $"%20{OTSReqRec.AllReq.orderPlaceReqRecForApproval.Qty}" + $"&Price={"@"}{OTSReqRec.AllReq.orderPlaceReqRecForApproval.Rate}";

                            ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Tls
                                | SecurityProtocolType.Tls11
                                | SecurityProtocolType.Tls12
                                | (SecurityProtocolType)12288; // TLS 1.3 (for older .NET)

                            // SSL certificate validation bypass (use ONLY for testing / trusted internal APIs)
                            ServicePointManager.ServerCertificateValidationCallback =
                                delegate (object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                                {
                                    return true;
                                };

                            var client = new RestClient(Form1.IVROTPLink);
                            var request = new RestRequest(Method.GET);
                            request.Timeout = -1;

                            request.AddParameter("LoginId",
                                new string(OTSReqRec.ReqHdr.TraderID)
                                    .Substring(0, OTSReqRec.ReqHdr.TraderIDLength));

                            request.AddParameter("OTP", OTPNumber);

                            request.AddParameter(
                                "OrderDetails",
                                $"{Exch.TrimEnd()} {ScName.TrimEnd()} {IsBuySell.TrimEnd()} Qty/Lot {OTSReqRec.AllReq.orderPlaceReqRecForApproval.Qty}"
                            );

                            request.AddParameter(
                                "Price",
                                "@" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.Rate
                            );

                            client.ExecuteAsync(request, (response, handle) =>
                            {
                                Form1.WriteLog(new List<string>
                                {
                                    "===== IVR OTP API CALL =====",
                                    $"Time: {DateTime.Now}",
                                    $"URL: {client.BuildUri(request)}",
                                    $"StatusCode: {response.StatusCode}",
                                    $"IsSuccessful: {response.StatusCode == HttpStatusCode.OK}",
                                    $"Error: {response.ErrorMessage}",
                                    $"Response: {response.Content}"
                                });
                            });
                        }
                        catch (Exception ex)
                        {
                            Form1.ErrorLog(ex);
                        }
                    }
                    break;
                case 135:
                    {
                        string OrderRemarks = "";
                        OTPNumber = RandomGenerator.Next(100000, 999999);
                        if (OTSReqRec.AllReq.UpdateOTPOrderStatusRec.ResendOTP == 1)
                            OTSReqRec.AllReq.UpdateOTPOrderStatusRec.ClientOTPNumber = OTPNumber;


                        if (OTSReqRec.AllReq.UpdateOTPOrderStatusRec.OTPApproved == 'Y') OrderRemarks = new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + " - OTP VERIFIED SUCCESSFULLY.";
                        else if (OTSReqRec.AllReq.UpdateOTPOrderStatusRec.ResendOTP == 1) OrderRemarks = "OTP Resend.";
                        Result = "Exec Proc_UpdateOrderDetailsOfApproval " + "'" + new string(OTSReqRec.AllReq.UpdateOTPOrderStatusRec.ClientCode, 0, OTSReqRec.AllReq.UpdateOTPOrderStatusRec.ClientCodeLength) + "'," + OTSReqRec.AllReq.UpdateOTPOrderStatusRec.ReferenceID + ",'O'," +
                                  OTSReqRec.AllReq.UpdateOTPOrderStatusRec.ClientOTPNumber + "," + OTSReqRec.AllReq.UpdateOTPOrderStatusRec.ResendOTP + ",'" + OrderRemarks + "'";
                                               

                        //string Request = "http://111.119.243.70/Dealer_IVR/OTP/SendIVROTP?LoginId=" +
                        //        new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) +
                        //                        "&OTP=" + OTPNumber +
                        //                        "&OrderDetails=" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.Exch +
                        //                        "%20" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.BuySell +
                        //                        "%20" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.Qty +
                        //                        "&Price=" + OTSReqRec.AllReq.orderPlaceReqRecForApproval.Rate;



                        //var client = new RestClient(Request);
                        //client.Timeout = -1;
                        //var request = new RestRequest(Method.GET);
                        //request.Timeout = 10000;
                        //client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) =>
                        //{

                        //}));


                        frm.LogReqToDbList.Add(OTSReqRec.ReqTime + ":- " + Result);
                    }
                    break;
                case 136:
                    {
                        Result = "Exec Proc_GetPendingApprovalOrdersReport " + "'" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "'," + OTSReqRec.AllReq.OrderInProcessReportRequestRec.StartTime + "," + OTSReqRec.AllReq.OrderInProcessReportRequestRec.EndTime;

                        frm.LogReqToDbList.Add(OTSReqRec.ReqTime + ":- " + Result);
                    }
                    break;
                case 191:
                        {
                            //string ExDt;
                            //ExDt = ExchStartDate.AddSeconds(OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.ExpiryDate).ToString("dd MMM yyyy");
                            //strName = new string(OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.Symbol).Substring(0, OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.SymbolLength) + " " + ExDt;
                            strName = MakeDerivDescForSpread(OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.ExpiryDate, OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.Symbol, OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.SymbolLength);
                            //Note : We need to add second expiry column in table
                            Result = "Exec Proc_PlaceSpreadOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.Token + "," + OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.Token + ",'" +
                               strName + "','" + OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.BuySell + "','" + OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.BuySell + "'," + OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.Qty + ",'" + OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.AtMarket + "'," +
                               OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.RateDiff + ",'" + "N" + "'," + 0.00 + "," + 0 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 192:
                        {
                            if (OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AHStatus != 'Y') OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AHStatus = 'N';
                            //string ExDt = ExchStartDate.AddSeconds(OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.ExpiryDate).ToString("dd MMM yyyy");
                            //ScNameForOType = new string(OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Symbol).Substring(0, OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.SymbolLength) + " " + ExDt;
                            ScNameForOType = MakeDerivDescForSpread(OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.ExpiryDate, OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Symbol, OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.SymbolLength);
                            Result = "Exec Proc_ModifySpreadOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.Exch + "','" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchType + "'," + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Token + "," + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.Token + ",'" +
                               ScNameForOType + "','" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.BuySell + "','" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.BuySell + "'," +
                                OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Qty + ",'" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AtMarket + "'," + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.RateDiff + ",'" +
                                "N" + "'," + 0.00 + "," +
                               OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.OldQty + "," +
                                 OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.TradedQty + "," + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AHStatus + "'," + 0 + ",'" +
                                  Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND)
                                 + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 193:
                        {
                            if (OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AHStatus != 'Y') // two
                                OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AHStatus = 'N';

                            //string ExDt;
                            //ExDt = ExchStartDate.AddSeconds(OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.ExpiryDate).ToString("dd MMM yyyy");
                            //strName = new string(OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Symbol).Substring(0, OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.SymbolLength) + " " + ExDt;
                            strName = MakeDerivDescForSpread(OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.ExpiryDate, OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Symbol, OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.SymbolLength);

                            Result = "Exec Proc_CancelSpreadOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.Exch + "','" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchType + "'," + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Token + "," + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.Token + ",'" +
                               strName + "','" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.BuySell + "','" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.BuySell + "'," + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Qty + ",'" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AtMarket + "'," +
                               OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.RateDiff + ",'" + "N" + "'," + 0.00 + "," +
                               OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.TradedQty + "," +
                                 OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AHStatus
                                 + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "'"; //18.10.14 0 For Currency & User Info for Nse FnO
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    #region MTM - Added on 12-Sep-2016 - Rohan
                    case 200:
                        {
                            Result = "Exec Proc_GetMarginReport2 " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;
                    case 206:
                        {
                            Result = "Exec Proc_GetMarginReport2MCX " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;
                    case 208:
                        {
                            Result = "Exec Proc_GetMarginReportCombined " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'";
                        }
                        break;
                    case 203:
                        {
                            Result = "Exec Proc_UpdateMTMLimitHit " + "'" + new string(OTSReqRec.AllReq.UpdateMTMLimitHitReq.Dummy).Substring(0, OTSReqRec.AllReq.UpdateMTMLimitHitReq.DummyLength) + "','" + OTSReqRec.AllReq.UpdateMTMLimitHitReq.MTMLimitHit + "','" + OTSReqRec.AllReq.UpdateMTMLimitHitReq.SkipFromMTM + "'";
                        }
                        break;
                    //case 208:   //Added on 08-Mar-2017 - Rohan
                    //    {
                    //        Result = "Exec Proc_UpdateMTMLimitHitMcx " + "'" + new string(OTSReqRec.AllReq.UpdateMTMLimitHitReqMcx.Dummy).Substring(0, OTSReqRec.AllReq.UpdateMTMLimitHitReqMcx.DummyLength) + "','" + OTSReqRec.AllReq.UpdateMTMLimitHitReqMcx.MTMLimitHit + "','" + OTSReqRec.AllReq.UpdateMTMLimitHitReqMcx.SkipFromMTM + "'";
                    //    }
                    //    break;
                    case 205:   //Added on 19-Sep-2016 - Rohan
                        {
                            Result = "Exec Proc_GetMTMClientsWithAllMargins " + "'" + OTSReqRec.AllReq.ClientsForMTMReq.SkipFromMTM + "', " + OTSReqRec.AllReq.ClientsForMTMReq.LastUpdatedMarginTime + ", " + OTSReqRec.AllReq.ClientsForMTMReq.LastUpdatedClientsTime;
                        }
                        break;
                    case 207:   //Added on 08-Mar-2017 - Rohan
                        {
                            Result = "Exec Proc_GetMTMClientsWithAllMarginsMcx " + "'" + OTSReqRec.AllReq.ClientsForMTMReqMcx.SkipFromMTM + "', " + OTSReqRec.AllReq.ClientsForMTMReqMcx.LastUpdatedMarginTime + ", " + OTSReqRec.AllReq.ClientsForMTMReqMcx.LastUpdatedClientsTime;
                        }
                        break;
                    #endregion
                    case 251:
                        {
                            Result = "Exec Proc_LinkFailure " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "'," + OTSReqRec.AllReq.LinkFailRec.RequestType + "," +
                                OTSReqRec.AllReq.LinkFailRec.BrokerOrderID + "," + OTSReqRec.AllReq.LinkFailRec.ErrorLevel + ",'" + OTSReqRec.AllReq.LinkFailRec.ErrorMsg.Substring(0, OTSReqRec.AllReq.LinkFailRec.ErrorMsgLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 252:
                        {
                            FEUserId = new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength);
                            if (FEUserId == "") FEUserId = new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength);

                            Result = "Exec Proc_LogoutUser " + "'" + FEUserId + "','" + new string(OTSReqRec.AllReq.LogoutRequest.MachineID).Substring(0, OTSReqRec.AllReq.LogoutRequest.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.LogoutRequest.ITS_IP).Substring(0, OTSReqRec.AllReq.LogoutRequest.ITS_IPLength) + "','" +
                                new string(OTSReqRec.AllReq.LogoutRequest.ServerIP).Substring(0, OTSReqRec.AllReq.LogoutRequest.ServerIPLength) + "','" +
                                new string(OTSReqRec.AllReq.LogoutRequest.ClientIP).Substring(0, OTSReqRec.AllReq.LogoutRequest.ClientIPLength) + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                case 247:
                    {
                        FEUserId = new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength);
                        if (FEUserId == "") FEUserId = new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength);

                        Result = "Exec Proc_LogoutUser " + "'" + FEUserId + "','" + new string(OTSReqRec.AllReq.LogoutRequest_2FA.MachineID).Substring(0, OTSReqRec.AllReq.LogoutRequest_2FA.MachineIDLength) + "','" + new string(OTSReqRec.AllReq.LogoutRequest_2FA.ITS_IP).Substring(0, OTSReqRec.AllReq.LogoutRequest_2FA.ITS_IPLength) + "','" +
                            new string(OTSReqRec.AllReq.LogoutRequest_2FA.ServerIP).Substring(0, OTSReqRec.AllReq.LogoutRequest_2FA.ServerIPLength) + "','" +
                            new string(OTSReqRec.AllReq.LogoutRequest_2FA.ClientIP).Substring(0, OTSReqRec.AllReq.LogoutRequest_2FA.ClientIPLength) + "'";
                        frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                    }
                    break;
                case 211:
                        {
                            strName = MakeDerivDesc(OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Exch, OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo);

                            Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.BuySell + "'," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Qty + ",'" + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.AtMarket + "'," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Rate
                               + ",'" + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.WithSL + "'," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.TriggerRate + "," + OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.DisclosedQty
                               + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "'," + 0 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "'"; //18.10.14 0 for Nse FnO & UserInfo for Currency.
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);

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
                                 OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.AHStatus + "'," + OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.DisclosedQty
                                 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "'," + 0 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "'"; //18.10.14 0 for Nse FnO & UserInfo for Currency.
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
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
                                 OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.AHStatus + "'"
                                 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "'," + 0 + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "'"; //18.10.14 0 for Nse FnO & UserInfo for Currency.
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 246://IIS Request Handled By Sandeep Jadhav
                        {
                            if (OTSReqRec.ReqHdr.ChannelID_ND == 901)//TurnOver For Each Segments
                                Result = "Exec Proc_IIS_FetchMISReportAllExch";
                            else
                                Result = "Exec Proc_IISRequest " + OTSReqRec.ReqHdr.ChannelID_ND;

                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        } break;
                    case 209:
                        {
                            Result = "Exec Proc_Admin_GetMarginTemplate " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," + OTSReqRec.AllReq.TemplateMargin.SegmentId + "";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 172:
                        {
                            Result = "Exec Proc_Admin_GetMarginTemplate " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," + OTSReqRec.AllReq.TemplateMargin_Peak.SegmentId + "";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 210: //Derivative margin //20.10.22
                        {
                            Result = "Exec Proc_Admin_GetMarginTemplate " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," + OTSReqRec.AllReq.TemplateMarginNew.SegmentId + "";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 173:
                        {
                            Result = "exec Proc_GetPeakMarginDetail" + "'" + new string(OTSReqRec.AllReq.PeakMarginData.FEUserID).Substring(0, OTSReqRec.AllReq.PeakMarginData.FEUserIDLength).Trim() + "'," + OTSReqRec.AllReq.PeakMarginData.SegmentId;
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 217:
                        {
                            Result = "Exec Proc_Admin_GetMarginTemplate_MTF " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," + OTSReqRec.AllReq.TemplateMargin.SegmentId + "";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 250:   //Added on 19-Sep-2016 - Rohan
                        {
                            Result = "Exec Proc_GetMTMClientsWithAllMarginsInstrumentWise_FetchAll" + "'" + OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.Status + "', " + OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.LastUpdatedMarginTime + ", " + OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.LastUpdatedClientsTime + ",'" + new string(OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.SegmentID).Substring(0, OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.SegmentIDLength).Trim() + "','" +
                            new string(OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.BranchID).Substring(0, OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.BranchIDLength).Trim() + "','" + new string(OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.DealerID).Substring(0, OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.DealerIDLength).Trim() + "','" + new string(OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.FEUserID).Substring(0, OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise.FEUserIDLength).Trim() + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                            break;
                    case 235: //05.12.18 SLBM Place Order
                        {
                            char RecallRepayOrder = 'N';
                            if (new string(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.RecallRepayOrder) == "RC") RecallRepayOrder = 'Y';
                            strName = new string(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ScripName).Substring(0, OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ScripNameLength) + " " + ExchStartDate.AddSeconds(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ExpiryDate).ToString("dd MMM yyyy") + " " + new string(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.Series);

                            Result = "Exec Proc_PlaceEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                                OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.Exch + "','" + OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ExchType + "'," + OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ScripCode + ",'" +
                                strName + "','" + OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.BuySell + "'," +
                                OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.Qty + ",'" + OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.AtMarket + "'," + OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.Rate + ",'" + OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.WithSL + "'," +
                                OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.TriggerRate + "," + OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.DisclosedQty
                                + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "', 0, 'D', '"
                                + GetIOCValue(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.IOC) + "'," + RecallRepayOrder;

                            frm.LogReqToDbList.Add(Result);

                        }
                        break;
                    case 236: //07.12.18 SLBM Modify Order
                        {
                            if (OTSReqRec.AllReq.SLBMOrderModifyRequestRec.AHStatus != 'Y') OTSReqRec.AllReq.SLBMOrderModifyRequestRec.AHStatus = 'N';

                            strName = new string(OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ScripName).Substring(0, OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ScripNameLength) + " " + ExchStartDate.AddSeconds(OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ExpiryDate).ToString("dd MMM yyyy") + " " + new string(OTSReqRec.AllReq.SLBMOrderModifyRequestRec.Series);

                            Result = "Exec Proc_ModifyEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                               OTSReqRec.AllReq.SLBMOrderModifyRequestRec.Exch + "','" + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ExchType + "'," + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ScripCode + ",'" +
                               strName + "','" + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.BuySell + "'," +
                               OTSReqRec.AllReq.SLBMOrderModifyRequestRec.Qty + ",'" + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.AtMarket + "'," + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.Rate + ",'" +
                               OTSReqRec.AllReq.SLBMOrderModifyRequestRec.WithSL + "'," + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.TriggerRate + "," + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.OldQty + "," +
                               OTSReqRec.AllReq.SLBMOrderModifyRequestRec.TradedQty + "," + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.SLBMOrderModifyRequestRec.AHStatus + "'," +
                               OTSReqRec.AllReq.SLBMOrderModifyRequestRec.DisclosedQty + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" +
                               Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "', 0, 'D', '" + GetIOCValue(OTSReqRec.AllReq.SLBMOrderModifyRequestRec.IOC) + "'";
                            frm.LogReqToDbList.Add(Result);
                        }
                        break;
                    case 237: //07.12.18 SLBM Cancel Order
                        {
                            if (OTSReqRec.AllReq.SLBMOrderCancelRequestRec.AHStatus != 'Y') OTSReqRec.AllReq.SLBMOrderCancelRequestRec.AHStatus = 'N';

                            strName = new string(OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ScripName).Substring(0, OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ScripNameLength) + " " + ExchStartDate.AddSeconds(OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ExpiryDate).ToString("dd MMM yyyy") + " " + new string(OTSReqRec.AllReq.SLBMOrderCancelRequestRec.Series);

                            Result = "Exec Proc_CancelEquityOrderRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" +
                            OTSReqRec.AllReq.SLBMOrderCancelRequestRec.Exch + "','" + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ExchType + "'," + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ScripCode + ",'" +
                            strName + "','" + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.BuySell + "'," +
                            OTSReqRec.AllReq.SLBMOrderCancelRequestRec.Qty + ",'" + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.AtMarket + "'," + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.Rate + ",'" + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.WithSL + "'," +
                            OTSReqRec.AllReq.SLBMOrderCancelRequestRec.TriggerRate + "," + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.TradedQty + "," + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ExchangeOrderId + ",'" + OTSReqRec.AllReq.SLBMOrderCancelRequestRec.AHStatus + "'"
                            + ",'" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_NC) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_ND) + "','" + Convert.ToString(OTSReqRec.ReqHdr.ChannelID_BC) + "'";
                            frm.LogReqToDbList.Add(Result);
                        }
                        break;
                    case 255: Result = "Exec Proc_HoldingsReportSLBM " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'"; break;
                    case 248:
                        {
                            Result = "Select * from SegmentMaster";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    //Option Exercise
                    case Constants.OTS_EXERCISE_PLACE_REQ:
                    case Constants.OTS_EXERCISE_MODIFY_REQ:
                    case Constants.OTS_EXERCISE_CANCEL_REQ:
                        {

                            ScNameForOType = MakeDerivDescForOrderType(OTSReqRec.AllReq.OptionExerciseRequestRec.Exch, OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.CPType, OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.ExpiryDate,
                                                  OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.StrikePrice, OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.Symbol, OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.SymbolLength);
                            if (OTSReqRec.ReqHdr.RequestType == Constants.OTS_EXERCISE_PLACE_REQ)
                                RequestType = 'P';
                            else if (OTSReqRec.ReqHdr.RequestType == Constants.OTS_EXERCISE_PLACE_REQ)
                                RequestType = 'M';
                            else
                                RequestType = 'C';

                            Result = "Exec Proc_OptionExerciseRequest " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + OrderRequesterId + "','" + RequestType + "','" + OTSReqRec.AllReq.OptionExerciseRequestRec.Exch + "','" + OTSReqRec.AllReq.OptionExerciseRequestRec.ExchType + "'," + OTSReqRec.AllReq.OptionExerciseRequestRec.ScripCode + ",'" +
                              ScNameForOType + "','" + OTSReqRec.AllReq.OptionExerciseRequestRec.Exercise + "'," +
                               OTSReqRec.AllReq.OptionExerciseRequestRec.Qty + "," + OTSReqRec.AllReq.OptionExerciseRequestRec.OldQty + "," + OTSReqRec.AllReq.OptionExerciseRequestRec.ExchangeOrderId + ",'" +
                                 OTSReqRec.AllReq.OptionExerciseRequestRec.BuySell + "'," + OTSReqRec.AllReq.OptionExerciseRequestRec.Reserved1;
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 226:
                        {
                            Result = "exec Proc_Admin_AuditReportTradesExch " + "'" + OTSReqRec.AllReq.GetTradesReport.Exch + "','" + OTSReqRec.AllReq.GetTradesReport.ExchType + "','" + new string(OTSReqRec.AllReq.GetTradesReport.FEUserID).Substring(0, OTSReqRec.AllReq.GetTradesReport.FEIDLength) + "','" + OTSReqRec.AllReq.GetTradesReport.Type + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_OFS_WATCHLIST:
                        {
                            Result = "exec Proc_GetOFSWatchlist";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_OFS_PLACE_BROKER_REQ:
                        {
                            Result = "exec Proc_PlaceOFSOrderRequest" + "'" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.ExchType + "','" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "','" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "','" +
                            OTSReqRec.AllReq.PlaceOfsOrderRequest.ClientType + "'," +
                            "'" + OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.ParticipantId.TrimEnd() + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Rate + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Qty + "','" +
                            new string(OTSReqRec.AllReq.PlaceOfsOrderRequest.Series).TrimEnd() + "','" + new string(OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripName).TrimEnd() + "','" +  OTSReqRec.AllReq.PlaceOfsOrderRequest.AtMarket + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                        {
                            Result = "exec Proc_ModifyOFSOrderRequest" + "'" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.ExchType + "','" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "','" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) +
                            "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.ClientType + "'," +
                            "'" + OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.ParticipantId.TrimEnd() + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Rate + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Qty + "','" +
                            new string(OTSReqRec.AllReq.PlaceOfsOrderRequest.Series).TrimEnd() + "','" + new string(OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripName).TrimEnd() + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.AtMarket + "'" + ",'" + OTSReqRec.AllReq.PlaceOfsOrderRequest.OrderId + "'";
                        frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                        {
                            Result = "exec Proc_CancelOFSOrderRequest" + "'" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Exch + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.ExchType + "','" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) + "','" + new string(OTSReqRec.ReqHdr.FEUserID).Substring(0, OTSReqRec.ReqHdr.FEUserIDLength) +
                            "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.ClientType + "'," +
                            "'" + OTSReqRec.AllReq.PlaceOfsOrderRequest.MarginType + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.ParticipantId.TrimEnd() + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Rate + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.Qty + "','" +
                            new string(OTSReqRec.AllReq.PlaceOfsOrderRequest.Series).TrimEnd() + "','" + new string(OTSReqRec.AllReq.PlaceOfsOrderRequest.ScripName).TrimEnd() + "','" + OTSReqRec.AllReq.PlaceOfsOrderRequest.AtMarket + "'" + ",'" + OTSReqRec.AllReq.PlaceOfsOrderRequest.OrderId + "'";
                            frm.LogReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                        break;
                    case 125:
                        {
                            Result = "Exec Proc_OFSOrderReport " + "'" + new string(OTSReqRec.ReqHdr.TraderID).Substring(0, OTSReqRec.ReqHdr.TraderIDLength) + "'," +
                            OTSReqRec.AllReq.OrderReportRequestRec.StartTime + "," + OTSReqRec.AllReq.OrderReportRequestRec.EndTime + "";// + OrderRequesterId + "'";
                            frm.LogAllReqToDbList.Add(Result + "," + DateTime.Now);
                        }
                         break;

            }
                s1 = Result.Replace("''", "' '");
                if (s1 != Result)
                    Result = s1;
                RequestStr = Result;
                RqStr = RequestStr;
                return Result;
           
        }

        private void ProcessMTMClientsWithAllMargins(DataTable dt1)  //Changed on 27-Mar-2017 - Rohan
        {
            #region OLD LOGIC - Commeneted on 24-Mar-2017 - Rohan
            //OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.MTMClientsWithAllMargins());
            //OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            //OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            //Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.FEUserID = dt.Rows[i]["ClientCode"].ToString().PadRight(15).ToCharArray();
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.FELength = (byte)dt.Rows[i]["ClientCode"].ToString().Trim().Length;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.AvailableMargin = Convert.ToDouble(dt.Rows[i]["AvailableMargin"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.MBPO = Convert.ToDouble(dt.Rows[i]["MBPO"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.MBOP = Convert.ToDouble(dt.Rows[i]["MBOP"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.HoldingMargin = Convert.ToDouble(dt.Rows[i]["HoldingMargin"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.LedgerBalance = Convert.ToDouble(dt.Rows[i]["LedgerBalance"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.AdHoc = Convert.ToDouble(dt.Rows[i]["AdHoc"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.Receipts = Convert.ToDouble(dt.Rows[i]["Receipts"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.Payments = Convert.ToDouble(dt.Rows[i]["Payments"]);
            //    if (Convert.ToString(dt.Rows[i]["SkipFromMTM"]).TrimEnd() == "Y")
            //        OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.SkipFromMTM = 1;
            //    else
            //        OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.SkipFromMTM = 0;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.Reserved1 = 0;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.Reserved2 = 0;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.Reserved3 = 0;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsReply.Reserved4 = 0;
            //}
            #endregion

            #region NEW LOGIC - Added on 27-Mar-2017 - Rohan
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0, dtRowsCount, k, i;
                DataTable dt;

                dt = dt1;
                dtRowsCount = dt.Rows.Count;

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dtRowsCount;
                while (Reply > 5000)
                {
                    Reply = Reply - 5000;
                    cnt = cnt + 5000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = dtRowsCount;

                TotalRecords = dtRowsCount;
                Reach = 0;
                for (k = 0; k < rows; k++)
                {
                    TotalRecords = (dtRowsCount) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(5000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.FEUserID = dt.Rows[i]["ClientCode"].ToString().PadRight(15).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.FELength = (byte)dt.Rows[i]["ClientCode"].ToString().Trim().Length;
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.AvailableMargin = Convert.ToDouble(dt.Rows[i]["AvailableMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.MBPO = Convert.ToDouble(dt.Rows[i]["MBPO"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.MBOP = Convert.ToDouble(dt.Rows[i]["MBOP"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.DerivMarginBlocked = Convert.ToDouble(dt.Rows[i]["DerivMarginBlocked"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.HoldingMargin = Convert.ToDouble(dt.Rows[i]["HoldingMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.LedgerBalance = Convert.ToDouble(dt.Rows[i]["LedgerBalance"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.AdHoc = Convert.ToDouble(dt.Rows[i]["AdHoc"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.Receipts = Convert.ToDouble(dt.Rows[i]["Receipts"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.Payments = Convert.ToDouble(dt.Rows[i]["Payments"]);
                        if (Convert.ToString(dt.Rows[i]["SkipFromMTM"]).TrimEnd() == "Y")
                            OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.SkipFromMTM = 1;
                        else
                            OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.SkipFromMTM = 0;
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.MTMLimitAmount = Convert.ToDouble(dt.Rows[i]["MTMLimitAmount"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.MTMLimitHit = Convert.ToByte(dt.Rows[i]["MTMLimitHit"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.MaxLastUpdatedMarginTime = Convert.ToInt64(dt.Rows[i]["LastUpdatedMarginTime"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.MaxLastUpdatedClientsTime = Convert.ToInt64(dt.Rows[i]["LastUpdatedClientsTime"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.LastValMargin = Convert.ToInt64(dt.Rows[i]["LastValMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReply.LastValClients = Convert.ToInt64(dt.Rows[i]["LastValClients"]);

                        RCnt++;
                    }

                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = 205;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.MTMClientsWithAllMargins());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception et)
            {
                OTSReplyRec.ErrorString = et.Message;
                Form1.ErrorLog(et);
            }
            #endregion
        }

        private void ProcessPeakMarginData(DataTable dt1)
        {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0, dtRowsCount, k, i;
                DataTable dt;

                dt = dt1;
                dtRowsCount = dt.Rows.Count;

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                Reply = dtRowsCount;
                while (Reply > 5000)
                {
                    Reply = Reply - 5000;
                    cnt = cnt + 5000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = dtRowsCount;

                TotalRecords = dtRowsCount;
                Reach = 0;
                for (k = 0; k < rows; k++)
                {
                    TotalRecords = (dtRowsCount) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(5000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;

                    if (k == rows - 1)
                        OTSReplyRec.AllReplyNew[k][0].PeakMarginReplyData.Flag = 'Y';
                    else
                        OTSReplyRec.AllReplyNew[k][0].PeakMarginReplyData.Flag = 'N';

                    for (i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].PeakMarginReplyData.FEUserID = dt.Rows[i]["ClientCode"].ToString().PadRight(200).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].PeakMarginReplyData.FELength = (byte)dt.Rows[i]["ClientCode"].ToString().Trim().Length;
                        OTSReplyRec.AllReplyNew[k][RCnt].PeakMarginReplyData.T1PeakMgn = Convert.ToDouble(dt.Rows[i]["T1PeakMgn"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].PeakMarginReplyData.T2PeakMgn = Convert.ToDouble(dt.Rows[i]["T2PeakMgn"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].PeakMarginReplyData.IntraDayPeakMgn = Convert.ToDouble(dt.Rows[i]["IntraDayPeakMgn"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].PeakMarginReplyData.PeakMargin = Convert.ToDouble(dt.Rows[i]["PeakMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].PeakMarginReplyData.AvaliableMargin = Convert.ToDouble(dt.Rows[i]["Avaliable Margin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].PeakMarginReplyData.DiffPeakMargin = Convert.ToDouble(dt.Rows[i]["DiffPeakMargin"]);
                        RCnt++;
                    }
                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = 173;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.PeakMarginReplyData());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessMTMClientsWithAllMarginsInstrumentWise(DataTable dt1)
         {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0, dtRowsCount, k, i;
                DataTable dt;

                dt = dt1;
                dtRowsCount = dt.Rows.Count;

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dtRowsCount;
                while (Reply > 5000)
                {
                    Reply = Reply - 5000;
                    cnt = cnt + 5000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = dtRowsCount;

                TotalRecords = dtRowsCount;
                Reach = 0;
                for (k = 0; k < rows; k++)
                {
                    TotalRecords = (dtRowsCount) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(5000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;

                    if (k == rows - 1)
                        OTSReplyRec.AllReplyNew[k][0].MTMClientsWithAllMarginsReplyInstrumentWise.Flag = 'Y';
                    else
                        OTSReplyRec.AllReplyNew[k][0].MTMClientsWithAllMarginsReplyInstrumentWise.Flag = 'N';

                    for (i = Reach; i < rem; i++)
                    {
                        Reach++;
                        
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.FEUserID = dt.Rows[i]["ClientCode"].ToString().PadRight(15).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.FELength = (byte)dt.Rows[i]["ClientCode"].ToString().Trim().Length;
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.AvailableMargin = Convert.ToDouble(dt.Rows[i]["AvailableMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MBPO = Convert.ToDouble(dt.Rows[i]["MBPO"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MBOP = Convert.ToDouble(dt.Rows[i]["MBOP"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.DerivMarginBlocked = Convert.ToDouble(dt.Rows[i]["DerivMarginBlocked"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.HoldingMargin = Convert.ToDouble(dt.Rows[i]["HoldingMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.LedgerBalance = Convert.ToDouble(dt.Rows[i]["LedgerBalance"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.AdHoc = Convert.ToDouble(dt.Rows[i]["AdHoc"]);
                        //OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.Receipts = Convert.ToDouble(dt.Rows[i]["Receipts"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.Payments = Convert.ToDouble(dt.Rows[i]["Payments"]);
                        //if (Convert.ToString(dt.Rows[i]["SkipFromMTM"]).TrimEnd() == "Y")
                        //    OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.SkipFromMTM = 1;
                        //else
                        //    OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.SkipFromMTM = 0;
                        //OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MTMLimitHit = Convert.ToByte(dt.Rows[i]["MTMLimitHit"]);
                       // OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MTMLimitAmount = Convert.ToDouble(dt.Rows[i]["MTMLimitAmount"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MaxLastUpdatedMarginTime = Convert.ToInt64(dt.Rows[i]["LastUpdatedMarginTime"]);
                        //OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MaxLastUpdatedClientsTime = Convert.ToInt64(dt.Rows[i]["LastUpdatedClientsTime"]);
                        //OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.LastValMargin = Convert.ToInt64(dt.Rows[i]["LastValMargin"]);
                       // OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.LastValClients = Convert.ToInt64(dt.Rows[i]["LastValClients"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.BookedPL = Convert.ToDouble(dt.Rows[i]["BookedPL"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MTMPL = Convert.ToDouble(dt.Rows[i]["MTMPL"]);
                        //OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.RealTimeMTMPL = Convert.ToDouble(dt.Rows[i]["RealTimeMTMPL"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MTMLoss = Convert.ToDouble(dt.Rows[i]["MTMLoss"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MTMProfit = Convert.ToDouble(dt.Rows[i]["MTMProfit"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.BookedProfit = Convert.ToDouble(dt.Rows[i]["BookedProfit"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.BookedLoss = Convert.ToDouble(dt.Rows[i]["BookedLoss"]);
                        //OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MTMValue = Convert.ToDouble(dt.Rows[i]["MTMValue"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MTMLimitSet = Convert.ToDouble(dt.Rows[i]["MTMLimitSet"]);
                        //OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.TotalValueForMTM = Convert.ToDouble(dt.Rows[i]["TotalValueForMTM"]);
                        //OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.PopUpPercent = Convert.ToSingle(dt.Rows[i]["PopUpPercent"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.SqOffModePercent = Convert.ToSingle(dt.Rows[i]["SquareOffPercent"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.SegmentId = Convert.ToUInt16(dt.Rows[i]["SegmentID"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.GrossExposureLimit = Convert.ToDouble(dt.Rows[i]["ExpiryExposure"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.GrossExposureLimitUtilized = Convert.ToDouble(dt.Rows[i]["ExpiryExposureUtilised"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.NetPurchaseLimit = Convert.ToDouble(dt.Rows[i]["ExpiryPurchase"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.NetPurchaseLimitUtilized = Convert.ToDouble(dt.Rows[i]["ExpiryPurchaseUtilised"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.NetSaleLimit = Convert.ToDouble(dt.Rows[i]["ExpirySale"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.NetSaleLimitUtilized = Convert.ToDouble(dt.Rows[i]["ExpirySaleUtilised"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.NetQty = Convert.ToDouble(dt.Rows[i]["ExpiryQuantity"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.NetQtyUtilized = Convert.ToDouble(dt.Rows[i]["ExpiryQuantityUtilised"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.NetPositionLimit = Convert.ToDouble(dt.Rows[i]["ExpiryPosition"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.NetPositionLimitUtilized = Convert.ToDouble(dt.Rows[i]["ExpiryPositionUtilised"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.TurnOverLimit = Convert.ToDouble(dt.Rows[i]["ExpiryTurnOver"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.TurnOverLimitUtilized = Convert.ToDouble(dt.Rows[i]["ExpiryTurnOverUtilised"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsReplyInstrumentWise.MarginLimit = Convert.ToDouble(dt.Rows[i]["ExpiryMargin"]);
                        RCnt++;
                    }
                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = 250;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.MTMClientsWithAllMarginsInstrumentWise());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception et)
            {
                OTSReplyRec.ErrorString = et.Message;
                Form1.ErrorLog(et);
            }
        }

        private void ProcessMTMClientsWithAllMarginsMcx(DataTable dt1)  //Changed on 27-Mar-2017 - Rohan
        {
            #region OLD LOGIC - Commented on 24-Mar-2017 - Rohan
            //OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.MTMClientsWithAllMargins());
            //OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            //OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            //Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.FEUserID = dt.Rows[i]["ClientCode"].ToString().PadRight(15).ToCharArray();
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.FELength = (byte)dt.Rows[i]["ClientCode"].ToString().Trim().Length;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.AvailableMargin = Convert.ToDouble(dt.Rows[i]["AvailableMargin"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.MBPO = Convert.ToDouble(dt.Rows[i]["MBPO"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.MBOP = Convert.ToDouble(dt.Rows[i]["MBOP"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.HoldingMargin = Convert.ToDouble(dt.Rows[i]["HoldingMargin"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.LedgerBalance = Convert.ToDouble(dt.Rows[i]["LedgerBalance"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.AdHoc = Convert.ToDouble(dt.Rows[i]["AdHoc"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.Receipts = Convert.ToDouble(dt.Rows[i]["Receipts"]);
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.Payments = Convert.ToDouble(dt.Rows[i]["Payments"]);
            //    if (Convert.ToString(dt.Rows[i]["SkipFromMTM"]).TrimEnd() == "Y")
            //        OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.SkipFromMTM = 1;
            //    else
            //        OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.SkipFromMTM = 0;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.Reserved1 = 0;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.Reserved2 = 0;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.Reserved3 = 0;
            //    OTSReplyRec.AllReply[i].MTMClientsWithAllMarginsMcxReply.Reserved4 = 0;
            //}
            #endregion

            #region NEW LOGIC - Added on 27-Mar-2017 - Rohan
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0, k, i;
                DataTable dt;

                dt = dt1;
                int dtRowsCount = dt.Rows.Count;

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dtRowsCount;
                while (Reply > 5000)
                {
                    Reply = Reply - 5000;
                    cnt = cnt + 5000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = dtRowsCount;

                TotalRecords = dtRowsCount;
                Reach = 0;
                for (k = 0; k < rows; k++)
                {
                    TotalRecords = (dtRowsCount) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(5000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.FEUserID = dt.Rows[i]["ClientCode"].ToString().PadRight(15).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.FELength = (byte)dt.Rows[i]["ClientCode"].ToString().Trim().Length;
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.AvailableMargin = Convert.ToDouble(dt.Rows[i]["AvailableMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.MBPO = Convert.ToDouble(dt.Rows[i]["MBPO"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.MBOP = Convert.ToDouble(dt.Rows[i]["MBOP"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.DerivMarginBlocked = Convert.ToDouble(dt.Rows[i]["DerivMarginBlocked"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.HoldingMargin = Convert.ToDouble(dt.Rows[i]["HoldingMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.LedgerBalance = Convert.ToDouble(dt.Rows[i]["LedgerBalance"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.AdHoc = Convert.ToDouble(dt.Rows[i]["AdHoc"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.Receipts = Convert.ToDouble(dt.Rows[i]["Receipts"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.Payments = Convert.ToDouble(dt.Rows[i]["Payments"]);
                        if (Convert.ToString(dt.Rows[i]["SkipFromMTM"]).TrimEnd() == "Y")
                            OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.SkipFromMTM = 1;
                        else
                            OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.SkipFromMTM = 0;
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.MTMLimitAmount = Convert.ToDouble(dt.Rows[i]["MTMLimitAmount"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.MTMLimitHit = Convert.ToByte(dt.Rows[i]["MTMLimitHit"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.MaxLastUpdatedMarginTime = Convert.ToInt64(dt.Rows[i]["LastUpdatedMarginTime"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.MaxLastUpdatedClientsTime = Convert.ToInt64(dt.Rows[i]["LastUpdatedClientsTime"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.LastValMargin = Convert.ToInt64(dt.Rows[i]["LastValMargin"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].MTMClientsWithAllMarginsMcxReply.LastValClients = Convert.ToInt64(dt.Rows[i]["LastValClients"]);

                        RCnt++;
                    }

                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = 207;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.MTMClientsWithAllMargins());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception et)
            {
                OTSReplyRec.ErrorString = et.Message;
                Form1.ErrorLog(et);
            }
            #endregion
        }

        private void ProcessLoginReply(string ReplyStr)
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
                    OTSReplyRec.AllReply[0].LoginReply.AllowHoldingsMargin = Convert.ToChar(VList[16]);

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

        private void ProcessChangePasswordReply(string ReplyStr)
        {
            string errorSting = "";
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

        private void ProcessLoginReplyWithName(string ReplyStr)//02.09.16
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


        private void ProcessLoginReplyNew(string ReplyStr)  // Bse Curr
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
                if (VList.Count() != 3 && VList.Count() != 19 && VList.Count() != 22 && VList.Count() != 25 && VList.Count() != 23 && VList.Count() != 26 && VList.Count() != 28 && VList.Count() != 29)  //improper string//02.09.16//SLBM 34 check
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
                    //if (VList.Length >= 24)//29-05-2018 Commented As AllowBseCurr Segment value not updated in Pointer As discuss with Devesh Sir
                    //{
                    C1 = VList[9];
                    if (C1 == "Y")
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseCurr = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseCurr = 0;
                    //}

                    OTSReplyRec.AllReply[0].LoginReplyNew.LastAccessedTime = Convert.ToInt32(VList[10]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.BranchID = Convert.ToUInt16(VList[11]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_NC = Convert.ToInt64(VList[12]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_ND = Convert.ToInt64(VList[13]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_BC = Convert.ToInt64("0" + VList[14]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_CD = Convert.ToInt64("0" + VList[15]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_MD = Convert.ToInt64("0" + VList[16]);
                    if (string.IsNullOrWhiteSpace(Convert.ToString(VList[17])))// == "                ")
                        OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_ED = 0;//jinal
                    else OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_ED = Convert.ToInt64(VList[17]);//jinal

                    C1 = VList[18];
                    if (C1 == "Y")
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowTotalBuySellQty = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowTotalBuySellQty = 0;
                    OTSReplyRec.AllReply[0].LoginReplyNew.AddMsgLength = (byte)VList[19].TrimEnd().Length;
                    OTSReplyRec.AllReply[0].LoginReplyNew.AddMsg = VList[19].TrimEnd();
                    OTSReplyRec.AllReply[0].LoginReplyNew.AccountType = AcTypeStrToByte(VList[20]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.AllowFastFeed = Convert.ToChar(VList[21]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.AllowHoldingsMargin = Convert.ToChar(VList[22]);
                    //OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_SLBM = Convert.ToInt64(VList[27]);
                    OTSReplyRec.AllReply[0].LoginReplyNew.DealerType = Convert.ToChar(VList[23]); //15.10.2016

                    if (VList.Count() > 25)
                    {
                        C1 = VList[25];
                        if (C1 == "Y") OTSReplyRec.AllReply[0].LoginReplyNew.AllowMarginTrading = 1;
                        else OTSReplyRec.AllReply[0].LoginReplyNew.AllowMarginTrading = 0;
                    }

                    if (VList.Count() > 30)         // changed from 28 to 30 due to addition of BseDeriv    
                    {
                        //i = Convert.ToInt32(VList[24]);
                        OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsg = VList[29].TrimEnd(); // VList[28].ToTrim();
                        OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsgLength = Convert.ToByte(VList[29].TrimEnd().Length);
                    }
                    else
                    {
                        OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsg = "";
                        OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsgLength = 0;
                    }


                    C1 = VList[24];//BseMf Krutika
                    if (C1 == "Y")
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseMf = 1;
                    else
                        OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseMf = 0;

                    //Commented on 03.03.23 as receiving error as SLBM is not there
                    if (VList.Count() > 27)
                    {
                        C1 = VList[26];
                        if (C1 == "Y")
                            OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseSLBM = 1;
                        else
                            OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseSLBM = 0;
                    }

                    if (VList.Count() > 28)  // Added on 10.02.2026 Kaustubh
                    {
                        C1 = VList[27];
                        if (C1 == "Y")
                        {
                            OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseDeriv = 1;
                            OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_BD = Convert.ToInt64(VList[28]);
                        }                            
                        else
                            OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseDeriv = 0;
                    }

                    //C1 = VList[28];
                    //if (C1 == "Y")
                    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                    //else
                    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

                    //C1 = VList[28];
                    //if (C1 == "Y")
                    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                    //else
                    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

                    //C1 = VList[25];//BseMf Krutika
                    //if (C1 == "Y")
                    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                    //else
                    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        #region Two-Factor Authentication
        ////private void ProcessTwoFactorLoginReply(DataTable dt)
        ////{
        ////    var ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
        ////    Form1.Log2FA("Reply(DB) : " + ReplyStr);
        ////    string C1;
        ////    int i;
        ////    try
        ////    {
        ////        OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TTwoFactorLoginReplyRecord());
        ////        OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTwoFactorLoginReplyRecord());
        ////        OTSReplyRec.ReplyHdr.ReplyCnt = 1;
        ////        Array.Resize(ref OTSReplyRec.AllReply, 1);
        ////        VList = ReplyStr.Split(splitchar);

        ////        int Journey = Convert.ToInt32(VList[2]);
        ////        string userID = VList[VList.Length - 1].Trim();

        ////        OTSReplyRec.ReplyHdr.FEUserID = userID.PadRight(15).ToCharArray();
        ////        OTSReplyRec.ReplyHdr.FEUserIDLength = (byte)userID.TrimEnd().Length;
        ////        OTSReplyRec.ReplyHdr.TraderID = userID.PadRight(15).ToCharArray();
        ////        OTSReplyRec.ReplyHdr.TraderIDLength = (byte)userID.TrimEnd().Length;

        ////        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserIDLength = (byte)userID.Length;
        ////        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserID = userID.PadRight(14).ToCharArray();
        ////        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.Journey = Journey;


        ////        if (VList[0] == "0")// if user is reg for 1st time
        ////        {
        ////            switch (Journey)
        ////            {
        ////                case 0:      // if session is already exits journey

        ////                    break;
        ////                case 1:     // 1st time user login journey
        ////                case 7:     // Resend OTP
        ////                case 8:
        ////                    string EmailAdd = VList[4];
        ////                    int startIndex = 1;
        ////                    int endIndex = EmailAdd.IndexOf('@') - 1;
        ////                    string maskedEmail = "";
        ////                    if (endIndex > 1)
        ////                    {
        ////                        maskedEmail = EmailAdd.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "*****");
        ////                    }
        ////                    string MobileNo = VList[5];
        ////                    startIndex = 1;
        ////                    endIndex = MobileNo.Length - 4;
        ////                    string maskedMobile = MobileNo.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "*****");
        ////                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = $"OTP has been sent to {maskedEmail} and {maskedMobile}";
        ////                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = Convert.ToByte(OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg.Length);

        ////                    try
        ////                    {
        ////                        if (Journey == 8 || Journey  == 1 || Journey == 7)
        ////                        {
        ////                            var client = new RestClient($"{Form1.OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}");
        ////                            var request = new RestRequest(Method.GET);
        ////                            request.Timeout = 10000;

        ////                            client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) =>
        ////                            {

        ////                            }));
        ////                        }


        ////                    }
        ////                    catch (Exception ex)
        ////                    {


        ////                    }

        ////                    //try
        ////                    //{
        ////                    //    if (Journey == 8)
        ////                    //    {
        ////                    //        var client = new RestClient($"{Form1.Forget_OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}");
        ////                    //        var request = new RestRequest(Method.GET);
        ////                    //        request.Timeout = 10000;

        ////                    //        client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) => {

        ////                    //        }));
        ////                    //    }
        ////                    //    else if (Journey == 1 || Journey == 7)
        ////                    //    {
        ////                    //        var client = new RestClient($"{Form1.OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}");
        ////                    //        var request = new RestRequest(Method.GET);
        ////                    //        request.Timeout = 10000;

        ////                    //        client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) => {

        ////                    //        }));
        ////                    //    }


        ////                    //}
        ////                    //catch (Exception ex)
        ////                    //{


        ////                    //}

        ////                    break;
        ////                case 2:      // OTP journey
        ////                    {
        ////                        var loginStr = Convert.ToString(dt.Rows[0]["Column2"]);

        ////                        var VListLogin = loginStr.Split(splitchar);

        ////                        DateTime.TryParse(VListLogin[3], out var LastModifiedTime);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastModifiedTime = (int)(LastModifiedTime - ExchStartDate).TotalSeconds;
        ////                    }
        ////                    break;
        ////                //case 8:     // forget MPin
        ////                //    {
        ////                //        string EmailAdd = VList[4];
        ////                //        int startIndex = 1;
        ////                //        int endIndex = EmailAdd.IndexOf('@') - 1;
        ////                //        string maskedEmail = EmailAdd.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "*****");
        ////                //        string MobileNo = VList[5];
        ////                //        startIndex = 1;
        ////                //        endIndex = MobileNo.Length - 4;
        ////                //        string maskedMobile = MobileNo.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "*****");
        ////                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = $"OTP has been sent to {maskedEmail} and {maskedMobile}";
        ////                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = Convert.ToByte(OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg.Length);


        ////                //        try
        ////                //        {
        ////                //            var client = new RestClient($"{Form1.Forget_OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}");
        ////                //            var request = new RestRequest(Method.GET);
        ////                //            request.Timeout = 10000;

        ////                //            client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) => {

        ////                //            }));

        ////                //        }
        ////                //        catch (Exception ex)
        ////                //        {


        ////                //        }
        ////                //    }
        ////                //    break;
        ////                case 3:    // MPin journey
        ////                case 4:
        ////                    {
        ////                        var loginStr = Convert.ToString(dt.Rows[0]["Column2"]);

        ////                        var VListLogin = loginStr.Split(splitchar);

        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserType = Convert.ToByte(VListLogin[1]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.SystemDateTime = Convert.ToInt32(VListLogin[2]);
        ////                        DateTime.TryParse(VListLogin[3], out var LastModifiedTime);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastModifiedTime = (int)(LastModifiedTime - ExchStartDate).TotalSeconds;
        ////                        C1 = VListLogin[4];
        ////                        if (C1 == "Y")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCash = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCash = 0;
        ////                        C1 = VListLogin[5];
        ////                        if (C1 == "Y")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseDeriv = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseDeriv = 0;
        ////                        C1 = VListLogin[6];
        ////                        if (C1 == "Y")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCash = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCash = 0;
        ////                        C1 = VListLogin[7];
        ////                        if (C1 == "Y")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCurrency = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCurrency = 0;
        ////                        C1 = VListLogin[8];
        ////                        if (C1 == "Y")//MCX Changes
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMcxDeriv = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMcxDeriv = 0;
        ////                        //if (VListLogin.Length >= 24)//29-05-2018 Commented As AllowBseCurr Segment value not updated in Pointer As discuss with Devesh Sir
        ////                        //{
        ////                        C1 = VListLogin[9];
        ////                        if (C1 == "Y")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCurr = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCurr = 0;
        ////                        //}

        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastAccessedTime = Convert.ToInt32(VListLogin[10]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BranchID = Convert.ToUInt16(VListLogin[11]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_NC = Convert.ToInt64(VListLogin[12]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ND = Convert.ToInt64(VListLogin[13]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_BC = Convert.ToInt64(VListLogin[14]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_CD = Convert.ToInt64(VListLogin[15]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_MD = Convert.ToInt64(VListLogin[16]);
        ////                        if (string.IsNullOrWhiteSpace(Convert.ToString(VListLogin[17])))// == "                ")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ED = 0;//jinal
        ////                        else OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ED = Convert.ToInt64(VListLogin[17]);//jinal

        ////                        C1 = VListLogin[18];
        ////                        if (C1 == "Y")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowTotalBuySellQty = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowTotalBuySellQty = 0;
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AddMsgLength = (byte)VListLogin[19].TrimEnd().Length;
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AddMsg = VListLogin[19].TrimEnd();
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AccountType = AcTypeStrToByte(VListLogin[20]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowFastFeed = Convert.ToChar(VListLogin[21]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowHoldingsMargin = Convert.ToChar(VListLogin[22]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_SLBM = Convert.ToInt64(VListLogin[27]);
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.DealerType = Convert.ToChar(VListLogin[23]); //15.10.2016

        ////                        if (VListLogin.Count() > 25)
        ////                        {
        ////                            C1 = VListLogin[25];
        ////                            if (C1 == "Y") OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMarginTrading = 1;
        ////                            else OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMarginTrading = 0;
        ////                        }

        ////                        if (VListLogin.Count() > 28)
        ////                        {
        ////                            //i = Convert.ToInt32(VListLogin[24]);
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = VListLogin[28].TrimEnd();
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = Convert.ToByte(VListLogin[28].TrimEnd().Length);
        ////                        }
        ////                        else
        ////                        {
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = "";
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = 0;
        ////                        }


        ////                        C1 = VListLogin[24];//BseMf Krutika
        ////                        if (C1 == "Y")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseMf = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseMf = 0;

        ////                        C1 = VListLogin[26];
        ////                        if (C1 == "Y")
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseSLBM = 1;
        ////                        else
        ////                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseSLBM = 0;


        ////                        //C1 = VList[28];
        ////                        //if (C1 == "Y")
        ////                        //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
        ////                        //else
        ////                        //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

        ////                        //C1 = VList[28];
        ////                        //if (C1 == "Y")
        ////                        //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
        ////                        //else
        ////                        //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

        ////                        //C1 = VList[25];//BseMf Krutika
        ////                        //if (C1 == "Y")
        ////                        //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
        ////                        //else
        ////                        //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);
        ////                    }
        ////                    break;
        ////            }
        ////            //OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BadLoginCnt = Convert.ToInt32(VList[2]);
        ////        }
        ////        else if (VList[0] == "1")
        ////        {
        ////            ushort errorCode;
        ////            if (ushort.TryParse(VList[1], out errorCode))
        ////            {
        ////                OTSReplyRec.ErrorStringLength = (byte)VList[3].Length;
        ////                OTSReplyRec.ErrorString = VList[3];
        ////                OTSReplyRec.ReplyHdr.ErrorCode = errorCode;

        ////                switch (errorCode)
        ////                {
        ////                    case 15:      // if session is already exits journey
        ////                    case 18:      // if session is already exits journey
        ////                    case 9:      // if session is already exits journey
        ////                    case 10:      // if session is already exits journey
        ////                    case 11:      // if session is already exits journey
        ////                        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BadLoginCnt = Convert.ToInt32(VList[4]);
        ////                        break;
        ////                }
        ////            }
        ////        }
        ////        else
        ////        {
        ////        }



        ////        //if (VList.Count() != 3 && VList.Count() != 19 && VList.Count() != 22 && VList.Count() != 25 && VList.Count() != 23 && VList.Count() != 26 && VList.Count() != 28 && VList.Count() != 29)  //improper string//02.09.16//SLBM 34 check
        ////        //{
        ////        //    ReplyStr = "1|Unknown Database Error. Please retry|0";
        ////        //    VList = ReplyStr.Split(splitchar);
        ////        //}
        ////        //if (VList[0] == "1") //ERROR indication
        ////        //{
        ////        //    OTSReplyRec.ReplyHdr.ErrorCode = 1;
        ////        //    OTSReplyRec.ErrorStringLength = (byte)VList[1].Length;
        ////        //    OTSReplyRec.ErrorString = VList[1];
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BadLoginCnt = Convert.ToInt32(VList[2]);
        ////        //}
        ////        //else
        ////        //{
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserIDLength = OTSReqRec.AllReq.LoginRequest.FEUserIDLength;
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserID = new string(OTSReqRec.AllReq.LoginRequest.FEUserID).PadRight(14).ToCharArray();
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserType = Convert.ToByte(VList[1]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.SystemDateTime = Convert.ToInt32(VList[2]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastModifiedTime = Convert.ToInt32(VList[3]);
        ////        //    C1 = VList[4];
        ////        //    if (C1 == "Y")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCash = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCash = 0;
        ////        //    C1 = VList[5];
        ////        //    if (C1 == "Y")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseDeriv = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseDeriv = 0;
        ////        //    C1 = VList[6];
        ////        //    if (C1 == "Y")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCash = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCash = 0;
        ////        //    C1 = VList[7];
        ////        //    if (C1 == "Y")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCurrency = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCurrency = 0;
        ////        //    C1 = VList[8];
        ////        //    if (C1 == "Y")//MCX Changes
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMcxDeriv = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMcxDeriv = 0;
        ////        //    //if (VList.Length >= 24)//29-05-2018 Commented As AllowBseCurr Segment value not updated in Pointer As discuss with Devesh Sir
        ////        //    //{
        ////        //    C1 = VList[9];
        ////        //    if (C1 == "Y")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCurr = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCurr = 0;
        ////        //    //}

        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastAccessedTime = Convert.ToInt32(VList[10]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BranchID = Convert.ToUInt16(VList[11]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_NC = Convert.ToInt64(VList[12]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ND = Convert.ToInt64(VList[13]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_BC = Convert.ToInt64(VList[14]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_CD = Convert.ToInt64(VList[15]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_MD = Convert.ToInt64(VList[16]);
        ////        //    if (string.IsNullOrWhiteSpace(Convert.ToString(VList[17])))// == "                ")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ED = 0;//jinal
        ////        //    else OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ED = Convert.ToInt64(VList[17]);//jinal

        ////        //    C1 = VList[18];
        ////        //    if (C1 == "Y")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowTotalBuySellQty = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowTotalBuySellQty = 0;
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AddMsgLength = (byte)VList[19].TrimEnd().Length;
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AddMsg = VList[19].TrimEnd();
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AccountType = AcTypeStrToByte(VList[20]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowFastFeed = Convert.ToChar(VList[21]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowHoldingsMargin = Convert.ToChar(VList[22]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_SLBM = Convert.ToInt64(VList[27]);
        ////        //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.DealerType = Convert.ToChar(VList[23]); //15.10.2016

        ////        //    if (VList.Count() > 25)
        ////        //    {
        ////        //        C1 = VList[25];
        ////        //        if (C1 == "Y") OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMarginTrading = 1;
        ////        //        else OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMarginTrading = 0;
        ////        //    }

        ////        //    if (VList.Count() > 28)
        ////        //    {
        ////        //        //i = Convert.ToInt32(VList[24]);
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = VList[28].TrimEnd();
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = Convert.ToByte(VList[28].TrimEnd().Length);
        ////        //    }
        ////        //    else
        ////        //    {
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = "";
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = 0;
        ////        //    }


        ////        //    C1 = VList[24];//BseMf Krutika
        ////        //    if (C1 == "Y")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseMf = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseMf = 0;

        ////        //    C1 = VList[26];
        ////        //    if (C1 == "Y")
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseSLBM = 1;
        ////        //    else
        ////        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseSLBM = 0;





        ////        //    //C1 = VList[28];
        ////        //    //if (C1 == "Y")
        ////        //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
        ////        //    //else
        ////        //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

        ////        //    //C1 = VList[28];
        ////        //    //if (C1 == "Y")
        ////        //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
        ////        //    //else
        ////        //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

        ////        //    //C1 = VList[25];//BseMf Krutika
        ////        //    //if (C1 == "Y")
        ////        //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
        ////        //    //else
        ////        //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);
        ////        //}
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        Form1.ErrorLog(ex);
        ////    }
        ////}
        #endregion


        #region Two-Factor Authentication
        private void ProcessTwoFactorLoginReply(DataTable dt)
        {
            var ReplyStr = Convert.ToString(dt.Rows[0]["Column1"]);
            Form1.Log2FA("Reply(DB) : " + ReplyStr);
            string C1;
            int i;
            try
            {
                OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TTwoFactorLoginReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTwoFactorLoginReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                VList = ReplyStr.Split(splitchar);

                int Journey = Convert.ToInt32(VList[2]);
                string userID = VList[VList.Length - 1].Trim();

                if (userID.Length > 14)
                    userID = userID.Substring(0, 14);

                OTSReplyRec.ReplyHdr.FEUserID = userID.PadRight(15).ToCharArray();
                OTSReplyRec.ReplyHdr.FEUserIDLength = (byte)userID.TrimEnd().Length;
                OTSReplyRec.ReplyHdr.TraderID = userID.PadRight(14).ToCharArray();
                OTSReplyRec.ReplyHdr.TraderIDLength = (byte)userID.TrimEnd().Length;

                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserIDLength = (byte)userID.Length;
                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserID = userID.PadRight(14).ToCharArray();
                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.Journey = Journey;


                if (VList[0] == "0")// if user is reg for 1st time
                {
                    switch (Journey)
                    {
                        case 0:      // if session is already exits journey

                            break;
                        case 1:     // 1st time user login journey
                        case 7:     // Resend OTP
                        case 8:
                            string EmailAdd = VList[4];
                            int startIndex = 1;
                            int endIndex = EmailAdd.IndexOf('@') - 1;
                            string maskedEmail = "";
                            if (endIndex > 1)
                            {
                                maskedEmail = EmailAdd.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "*****");
                            }
                            string MobileNo = VList[5];
                            MobileNo = MobileNo.TrimEnd();
                            startIndex = 1;
                            endIndex = MobileNo.Length - 4;
                            string maskedMobile = MobileNo.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "*****");
                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = $"OTP has been sent to {maskedEmail} and {maskedMobile}";
                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = Convert.ToByte(OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg.Length);


                            //try
                            //{
                            //    if (Journey == 8)
                            //    {
                            //        var client = new RestClient($"{Form1.OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}");
                            //        var request = new RestRequest(Method.GET);
                            //        request.Timeout = 10000;

                            //        client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) => {

                            //        }));
                            //    }


                            //}
                            //catch (Exception ex)
                            //{


                            //}


                            try
                            {
                                int otpFor;
                                if (Journey == 7)
                                    otpFor = Convert.ToInt32(VList[6]);
                                else
                                    otpFor = Journey;
                                if (otpFor == 1)
                                {
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 
                                        | SecurityProtocolType.Ssl3 
                                        | SecurityProtocolType.Tls11
                                        | SecurityProtocolType.Tls
                                        | (SecurityProtocolType)12288;

                                    ServicePointManager.ServerCertificateValidationCallback +=
                                        (sender, cert, chain, sslPolicyErrors) => true;

                                    //var client = new RestClient($"{Form1.OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}&EmailId={EmailAdd}&MobNo={MobileNo}");

                                    //var request = new RestRequest(Method.GET);
                                    //request.Timeout = 10000;
                                    ////var res = client.Execute(request);

                                    //client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) =>
                                    //{

                                    //}));

                                    var client = new RestClient(Form1.Forget_OTPLink);

                                    var request = new RestRequest(Method.GET);
                                    request.Timeout = -1;

                                    request.AddParameter("LoginId", userID.Trim());
                                    request.AddParameter("OTP", VList[3]);
                                    request.AddParameter("EmailId", EmailAdd);
                                    request.AddParameter("MobNo", MobileNo);

                                    client.ExecuteAsync(request, (response, handle) =>
                                    {
                                        Form1.WriteLog(new List<string>
                                        {
                                            "===== OTP API CALL =====",
                                            $"Time: {DateTime.Now}",
                                            $"URL: {client.BuildUri(request).ToString()}",
                                            $"StatusCode: {response.StatusCode}",
                                            $"IsSuccessful: {response.StatusCode == HttpStatusCode.OK}",
                                            $"Error: {response.ErrorMessage}",
                                            $"Response: {response.Content}"
                                        });
                                    });

                                }
                                else if (otpFor == 8)
                                {
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                                        | SecurityProtocolType.Ssl3
                                        | SecurityProtocolType.Tls11
                                        | SecurityProtocolType.Tls
                                        | (SecurityProtocolType)12288;

                                    ServicePointManager.ServerCertificateValidationCallback +=
                                        (sender, cert, chain, sslPolicyErrors) => true;

                                    //var client = new RestClient($"{Form1.Forget_OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}&EmailId={EmailAdd}&MobNo={MobileNo}");
                                    //var request = new RestRequest(Method.GET);
                                    //request.Timeout = 10000;

                                    //client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) =>
                                    //{

                                    //}));

                                    var client = new RestClient(Form1.Forget_OTPLink);

                                    var request = new RestRequest(Method.GET);
                                    request.Timeout = -1;

                                    request.AddParameter("LoginId", userID.Trim());
                                    request.AddParameter("OTP", VList[3]);
                                    request.AddParameter("EmailId", EmailAdd);
                                    request.AddParameter("MobNo", MobileNo);

                                    client.ExecuteAsync(request, (response, handle) =>
                                    {
                                        Form1.WriteLog(new List<string>
                                        {
                                            "===== OTP API CALL =====",
                                            $"Time: {DateTime.Now}",
                                            $"URL: {client.BuildUri(request).ToString()}",
                                            $"StatusCode: {response.StatusCode}",
                                            $"IsSuccessful: {response.StatusCode == HttpStatusCode.OK}",
                                            $"Error: {response.ErrorMessage}",
                                            $"Response: {response.Content}"
                                        });
                                    });
                                }

                            }
                            catch (Exception ex)
                            {
                                Form1.ErrorLog(ex);

                            }
                            break;
                        case 2:      // OTP journey
                            {
                                var loginStr = Convert.ToString(dt.Rows[0]["Column2"]);

                                var VListLogin = loginStr.Split(splitchar);

                                DateTime.TryParse(VListLogin[3], out var LastModifiedTime);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastModifiedTime = (int)(LastModifiedTime - ExchStartDate).TotalSeconds;
                            }
                            break;
                        //case 8:     // forget MPin
                        //    {
                        //        string EmailAdd = VList[4];
                        //        int startIndex = 1;
                        //        int endIndex = EmailAdd.IndexOf('@') - 1;
                        //        string maskedEmail = EmailAdd.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "*****");
                        //        string MobileNo = VList[5];
                        //        startIndex = 1;
                        //        endIndex = MobileNo.Length - 4;
                        //        string maskedMobile = MobileNo.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "*****");
                        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = $"OTP has been sent to {maskedEmail} and {maskedMobile}";
                        //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = Convert.ToByte(OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg.Length);


                        //        try
                        //        {
                        //            var client = new RestClient($"{Form1.Forget_OTPLink}?LoginId={userID.TrimEnd()}&OTP={VList[3]}");
                        //            var request = new RestRequest(Method.GET);
                        //            request.Timeout = 10000;

                        //            client.ExecuteAsync(request, new Action<IRestResponse, RestRequestAsyncHandle>((a, b) => {

                        //            }));

                        //        }
                        //        catch (Exception ex)
                        //        {


                        //        }
                        //    }
                        //    break;
                        case 3:    // MPin journey
                        case 4:
                            {
                                var loginStr = Convert.ToString(dt.Rows[0]["Column2"]);

                                var VListLogin = loginStr.Split(splitchar);

                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserType = Convert.ToByte(VListLogin[1]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.SystemDateTime = Convert.ToInt32(VListLogin[2]);
                                DateTime.TryParse(VListLogin[3], out var LastModifiedTime);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastModifiedTime = (int)(LastModifiedTime - ExchStartDate).TotalSeconds;
                                C1 = VListLogin[4];
                                if (C1 == "Y")
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCash = 1;
                                else
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCash = 0;
                                C1 = VListLogin[5];
                                if (C1 == "Y")
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseDeriv = 1;
                                else
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseDeriv = 0;
                                C1 = VListLogin[6];
                                if (C1 == "Y")
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCash = 1;
                                else
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCash = 0;
                                C1 = VListLogin[7];
                                if (C1 == "Y")
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCurrency = 1;
                                else
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCurrency = 0;
                                C1 = VListLogin[8];
                                if (C1 == "Y")//MCX Changes
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMcxDeriv = 1;
                                else
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMcxDeriv = 0;
                                //if (VListLogin.Length >= 24)//29-05-2018 Commented As AllowBseCurr Segment value not updated in Pointer As discuss with Devesh Sir
                                //{
                                C1 = VListLogin[9];
                                if (C1 == "Y")
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCurr = 1;
                                else
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCurr = 0;
                                //}

                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastAccessedTime = Convert.ToInt32(VListLogin[10]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BranchID = Convert.ToUInt16(VListLogin[11]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_NC = Convert.ToInt64(VListLogin[12]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ND = Convert.ToInt64(VListLogin[13]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_BC = Convert.ToInt64(VListLogin[14]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_CD = Convert.ToInt64(VListLogin[15]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_MD = Convert.ToInt64(VListLogin[16]);
                                if (string.IsNullOrWhiteSpace(Convert.ToString(VListLogin[17])))// == "                ")
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ED = 0;//jinal
                                else OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ED = Convert.ToInt64(VListLogin[17]);//jinal

                                C1 = VListLogin[18];
                                if (C1 == "Y")
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowTotalBuySellQty = 1;
                                else
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowTotalBuySellQty = 0;
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AddMsgLength = (byte)VListLogin[19].TrimEnd().Length;
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AddMsg = VListLogin[19].TrimEnd();
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AccountType = AcTypeStrToByte(VListLogin[20]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowFastFeed = Convert.ToChar(VListLogin[21]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowHoldingsMargin = Convert.ToChar(VListLogin[22]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_SLBM = 0;// Convert.ToInt64(VListLogin[27]);
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.DealerType = Convert.ToChar(VListLogin[23]); //15.10.2016

                                if (VListLogin.Count() > 25)
                                {
                                    C1 = VListLogin[25];
                                    if (C1 == "Y") OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMarginTrading = 1;
                                    else OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMarginTrading = 0;
                                }

                                if (VListLogin.Count() > 28)
                                {
                                    //i = Convert.ToInt32(VListLogin[24]);
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = VListLogin[28].TrimEnd();
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = Convert.ToByte(VListLogin[28].TrimEnd().Length);
                                }
                                else
                                {
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = "";
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = 0;
                                }


                                C1 = VListLogin[24];//BseMf Krutika
                                if (C1 == "Y")
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseMf = 1;
                                else
                                    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseMf = 0;

                                //C1 = VListLogin[26];
                                //if (C1 == "Y")
                                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseSLBM = 1;
                                //else
                                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseSLBM = 0;


                                //C1 = VList[28];
                                //if (C1 == "Y")
                                //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                                //else
                                //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

                                //C1 = VList[28];
                                //if (C1 == "Y")
                                //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                                //else
                                //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

                                //C1 = VList[25];//BseMf Krutika
                                //if (C1 == "Y")
                                //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                                //else
                                //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);
                            }
                            break;
                    }
                    //OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BadLoginCnt = Convert.ToInt32(VList[2]);
                }
                else if (VList[0] == "1")
                {
                    ushort errorCode;
                    if (ushort.TryParse(VList[1], out errorCode))
                    {
                        OTSReplyRec.ErrorStringLength = (byte)VList[3].Length;
                        OTSReplyRec.ErrorString = VList[3];
                        OTSReplyRec.ReplyHdr.ErrorCode = errorCode;

                        switch (errorCode)
                        {
                            case 15:      // if session is already exits journey
                            case 18:      // if session is already exits journey
                            case 9:      // if session is already exits journey
                            case 10:      // if session is already exits journey
                            case 11:      // if session is already exits journey
                                OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BadLoginCnt = Convert.ToInt32(VList[4]);
                                break;
                        }
                    }
                }
                else
                {
                }



                //if (VList.Count() != 3 && VList.Count() != 19 && VList.Count() != 22 && VList.Count() != 25 && VList.Count() != 23 && VList.Count() != 26 && VList.Count() != 28 && VList.Count() != 29)  //improper string//02.09.16//SLBM 34 check
                //{
                //    ReplyStr = "1|Unknown Database Error. Please retry|0";
                //    VList = ReplyStr.Split(splitchar);
                //}
                //if (VList[0] == "1") //ERROR indication
                //{
                //    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                //    OTSReplyRec.ErrorStringLength = (byte)VList[1].Length;
                //    OTSReplyRec.ErrorString = VList[1];
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BadLoginCnt = Convert.ToInt32(VList[2]);
                //}
                //else
                //{
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserIDLength = OTSReqRec.AllReq.LoginRequest.FEUserIDLength;
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserID = new string(OTSReqRec.AllReq.LoginRequest.FEUserID).PadRight(14).ToCharArray();
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserType = Convert.ToByte(VList[1]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.SystemDateTime = Convert.ToInt32(VList[2]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastModifiedTime = Convert.ToInt32(VList[3]);
                //    C1 = VList[4];
                //    if (C1 == "Y")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCash = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCash = 0;
                //    C1 = VList[5];
                //    if (C1 == "Y")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseDeriv = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseDeriv = 0;
                //    C1 = VList[6];
                //    if (C1 == "Y")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCash = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCash = 0;
                //    C1 = VList[7];
                //    if (C1 == "Y")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCurrency = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseCurrency = 0;
                //    C1 = VList[8];
                //    if (C1 == "Y")//MCX Changes
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMcxDeriv = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMcxDeriv = 0;
                //    //if (VList.Length >= 24)//29-05-2018 Commented As AllowBseCurr Segment value not updated in Pointer As discuss with Devesh Sir
                //    //{
                //    C1 = VList[9];
                //    if (C1 == "Y")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCurr = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseCurr = 0;
                //    //}

                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.LastAccessedTime = Convert.ToInt32(VList[10]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.BranchID = Convert.ToUInt16(VList[11]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_NC = Convert.ToInt64(VList[12]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ND = Convert.ToInt64(VList[13]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_BC = Convert.ToInt64(VList[14]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_CD = Convert.ToInt64(VList[15]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_MD = Convert.ToInt64(VList[16]);
                //    if (string.IsNullOrWhiteSpace(Convert.ToString(VList[17])))// == "                ")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ED = 0;//jinal
                //    else OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_ED = Convert.ToInt64(VList[17]);//jinal

                //    C1 = VList[18];
                //    if (C1 == "Y")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowTotalBuySellQty = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowTotalBuySellQty = 0;
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AddMsgLength = (byte)VList[19].TrimEnd().Length;
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AddMsg = VList[19].TrimEnd();
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AccountType = AcTypeStrToByte(VList[20]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowFastFeed = Convert.ToChar(VList[21]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowHoldingsMargin = Convert.ToChar(VList[22]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.ChannelID_SLBM = Convert.ToInt64(VList[27]);
                //    OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.DealerType = Convert.ToChar(VList[23]); //15.10.2016

                //    if (VList.Count() > 25)
                //    {
                //        C1 = VList[25];
                //        if (C1 == "Y") OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMarginTrading = 1;
                //        else OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowMarginTrading = 0;
                //    }

                //    if (VList.Count() > 28)
                //    {
                //        //i = Convert.ToInt32(VList[24]);
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = VList[28].TrimEnd();
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = Convert.ToByte(VList[28].TrimEnd().Length);
                //    }
                //    else
                //    {
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsg = "";
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.PopupMsgLength = 0;
                //    }


                //    C1 = VList[24];//BseMf Krutika
                //    if (C1 == "Y")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseMf = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowBseMf = 0;

                //    C1 = VList[26];
                //    if (C1 == "Y")
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseSLBM = 1;
                //    else
                //        OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.AllowNseSLBM = 0;





                //    //C1 = VList[28];
                //    //if (C1 == "Y")
                //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                //    //else
                //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

                //    //C1 = VList[28];
                //    //if (C1 == "Y")
                //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                //    //else
                //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);

                //    //C1 = VList[25];//BseMf Krutika
                //    //if (C1 == "Y")
                //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(1);
                //    //else
                //    //    OTSReplyRec.AllReply[0].LoginReplyNew.Reserved2 = BitConverter.GetBytes(0);
                //}
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        #endregion
        //private void ProcessLoginReplyNew(string ReplyStr)
        //{
        //    string C1;
        //    int i;

        //    OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TLoginReplyRecordNew());
        //    OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TLoginReplyRecordNew());
        //    OTSReplyRec.ReplyHdr.ReplyCnt = 1;
        //    Array.Resize(ref OTSReplyRec.AllReply, 1);
        //    VList = ReplyStr.Split(splitchar);
        //    if (VList.Count() != 3 && VList.Count() != 19 && VList.Count() != 22 && VList.Count() != 24)  //improper string//02.09.16
        //    {
        //        ReplyStr = "1|Unknown Database Error. Please retry|0";
        //        VList = ReplyStr.Split(splitchar);
        //    }
        //    if (VList[0] == "1") //ERROR indication
        //    {
        //        OTSReplyRec.ReplyHdr.ErrorCode = 1;
        //        OTSReplyRec.ErrorStringLength = (byte)VList[1].Length;
        //        OTSReplyRec.ErrorString = VList[1];
        //        OTSReplyRec.AllReply[0].LoginReplyNew.BadLoginCnt = Convert.ToInt32(VList[2]);
        //    }
        //    else
        //    {
        //        OTSReplyRec.AllReply[0].LoginReplyNew.FEUserIDLength = OTSReqRec.AllReq.LoginRequest.FEUserIDLength;
        //        OTSReplyRec.AllReply[0].LoginReplyNew.FEUserID = new string(OTSReqRec.AllReq.LoginRequest.FEUserID).PadRight(14).ToCharArray();
        //        OTSReplyRec.AllReply[0].LoginReplyNew.FEUserType = Convert.ToByte(VList[1]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.SystemDateTime = Convert.ToInt32(VList[2]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.LastModifiedTime = Convert.ToInt32(VList[3]);
        //        C1 = VList[4];
        //        if (C1 == "Y")
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseCash = 1;
        //        else
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseCash = 0;
        //        C1 = VList[5];
        //        if (C1 == "Y")
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseDeriv = 1;
        //        else
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseDeriv = 0;
        //        C1 = VList[6];
        //        if (C1 == "Y")
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseCash = 1;
        //        else
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowBseCash = 0;
        //        C1 = VList[7];
        //        if (C1 == "Y")
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseCurrency = 1;
        //        else
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowNseCurrency = 0;
        //        C1 = VList[8];
        //        if (C1 == "Y")//MCX Changes
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowMcxDeriv = 1;
        //        else
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowMcxDeriv = 0;

        //        OTSReplyRec.AllReply[0].LoginReplyNew.LastAccessedTime = Convert.ToInt32(VList[9]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.BranchID = Convert.ToUInt16(VList[10]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_NC = Convert.ToInt64(VList[11]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_ND = Convert.ToInt64(VList[12]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_BC = Convert.ToInt64(VList[13]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_CD = Convert.ToInt64(VList[14]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.ChannelID_MD = Convert.ToInt64(VList[15]);

        //        C1 = VList[16];
        //        if (C1 == "Y")
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowTotalBuySellQty = 1;
        //        else
        //            OTSReplyRec.AllReply[0].LoginReplyNew.AllowTotalBuySellQty = 0;
        //        OTSReplyRec.AllReply[0].LoginReplyNew.AddMsgLength = (byte)VList[17].TrimEnd().Length;
        //        OTSReplyRec.AllReply[0].LoginReplyNew.AddMsg = VList[17].TrimEnd();
        //        OTSReplyRec.AllReply[0].LoginReplyNew.AccountType = AcTypeStrToByte(VList[18]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.AllowFastFeed = Convert.ToChar(VList[19]);
        //        OTSReplyRec.AllReply[0].LoginReplyNew.AllowHoldingsMargin = Convert.ToChar(VList[20]);

        //        if (VList.Count() == 24)
        //        {
        //            i = Convert.ToInt32(VList[22]);
        //            OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsg = VList[23].TrimEnd();
        //            OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsgLength = Convert.ToByte(VList[23].TrimEnd().Length);
        //        }
        //        else
        //        {
        //            OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsg = "";
        //            OTSReplyRec.AllReply[0].LoginReplyNew.PopupMsgLength = 0;
        //        }

        //        OTSReplyRec.AllReply[0].LoginReplyNew.DealerType = Convert.ToChar(VList[21]); //15.10.2016
        //    }
        //}
        private void ProcessAHDerivPlaceOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
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

        private void ProcessDerivCancelOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessDerivModifyOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessDerivPlaceOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AlgoID = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.AlgoID;
                OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerTime = TimeNow;
                OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.IOC = OTSReqRec.AllReq.DerivOrderPlaceRequestRec.IOC;
                OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AHStatus = AHStatus1;
                OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = 0;
            }
            catch (Exception ex) 
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessApprovalOrderRequest(string ReplyStr) //Added on 02.03.23 For IVR-OTP Order Placement Siddhesh
        {
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.OrderPlaceReplyRecOfApproval());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.OrderPlaceReplyRecOfApproval());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;

            int OrderReferenceID, OrderOTPNumber, OrderReqTime;
            string ClientCode; char SendOTP;

            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4)
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
            //    VList = ReplyStr.Split(splitchar);
            }

            ClientCode = VList[0].Trim();
            OrderReferenceID = Convert.ToInt32(VList[1]);
            OrderReqTime = Convert.ToInt32(VList[2]);
            OrderOTPNumber = Convert.ToInt32(VList[3]);
            SendOTP = 'Y';//Convert.ToChar(VList[6]);

            Array.Resize(ref OTSReplyRec.AllReply, 1);

            
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ReplyType = 134;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.RequestType;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.Exch = 'N'; //OTSReqRec.AllReq.orderPlaceReqRecForApproval.Exch;

            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ExchType = 'C';//OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchType;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ScripCode = 22;//OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripCode;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ScripName = "ACC".ToCharArray(); //new string(OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripName).PadRight(12).ToCharArray();
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ScripNameLength = 3;//OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripNameLength;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.BuySell = 'B';// OTSReqRec.AllReq.orderPlaceReqRecForApproval.BuySell;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ReferenceID = 11;// OrderReferenceID;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.OTPNumber = 123456;// OrderOTPNumber;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.DisclosedQty = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.DisclosedQty;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.Qty = 15;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.Qty;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.AtMarket = 'N';// OTSReqRec.AllReq.orderPlaceReqRecForApproval.AtMarket;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.Rate = 2250;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.Rate;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.WithSL = 'N';// OTSReqRec.AllReq.orderPlaceReqRecForApproval.WithSL;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.TriggerRate = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.TriggerRate;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.TraderRequestID = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.TraderRequestID;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.IOC = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.IOC;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.OrderReqTime = 1362329755;// OrderReqTime;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ExchangeOrderId = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchangeOrderId;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ExchangeOrderTime = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchangeOrderTime;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.PendingQty = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.PendingQty;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.OldQty = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.OldQty;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.AHStatus = 'N';// OTSReqRec.AllReq.orderPlaceReqRecForApproval.AHStatus;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.SLTriggered = 'N';// OTSReqRec.AllReq.orderPlaceReqRecForApproval.SLTriggered;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.TradedQty = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.TradedQty;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.OrderType = 1;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.OrderType;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ExpiryDate = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExpiryDate;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.InstrType = 1;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.InstrType;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.CPType = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.CPType;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.CALevel = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.CALevel;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.StrikePrice = 0;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.StrikePrice;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.Series = "".ToCharArray();// OTSReqRec.AllReq.orderPlaceReqRecForApproval.Series;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.RecallRepayOrder = "".ToCharArray();// OTSReqRec.AllReq.orderPlaceReqRecForApproval.RecallRepayOrder;
            //if (SendOTP == 'Y') OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.SendOTP = 1;
            //else OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.SendOTP = 0;
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.IntraMultiplierReq = 1;// OTSReqRec.AllReq.orderPlaceReqRecForApproval.IntraMultiplierReq;






            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.TraderID = ClientCode.PadRight(14).ToCharArray();
            //OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.TraderIDLength = (byte)ClientCode.Length;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ReplyType = OTSReqRec.AllReq.orderPlaceReqRecForApproval.RequestType;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.Exch = OTSReqRec.AllReq.orderPlaceReqRecForApproval.Exch;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ExchType = OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchType;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ScripCode = OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripCode;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ScripName = new string(OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripName).PadRight(12).ToCharArray();
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ScripNameLength = OTSReqRec.AllReq.orderPlaceReqRecForApproval.ScripNameLength;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.BuySell = OTSReqRec.AllReq.orderPlaceReqRecForApproval.BuySell;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ReferenceID = OrderReferenceID;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.OTPNumber = OrderOTPNumber;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.DisclosedQty = OTSReqRec.AllReq.orderPlaceReqRecForApproval.DisclosedQty;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.Qty = OTSReqRec.AllReq.orderPlaceReqRecForApproval.Qty;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.AtMarket = OTSReqRec.AllReq.orderPlaceReqRecForApproval.AtMarket;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.Rate = OTSReqRec.AllReq.orderPlaceReqRecForApproval.Rate;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.WithSL = OTSReqRec.AllReq.orderPlaceReqRecForApproval.WithSL;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.TriggerRate = OTSReqRec.AllReq.orderPlaceReqRecForApproval.TriggerRate;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.TraderRequestID = OTSReqRec.AllReq.orderPlaceReqRecForApproval.TraderRequestID;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.IOC = OTSReqRec.AllReq.orderPlaceReqRecForApproval.IOC;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.OrderReqTime = OrderReqTime;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ExchangeOrderId = OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchangeOrderId;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ExchangeOrderTime = OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.PendingQty = OTSReqRec.AllReq.orderPlaceReqRecForApproval.PendingQty;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.OldQty = OTSReqRec.AllReq.orderPlaceReqRecForApproval.OldQty;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.AHStatus = OTSReqRec.AllReq.orderPlaceReqRecForApproval.AHStatus;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.SLTriggered = OTSReqRec.AllReq.orderPlaceReqRecForApproval.SLTriggered;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.TradedQty = OTSReqRec.AllReq.orderPlaceReqRecForApproval.TradedQty;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.OrderType = OTSReqRec.AllReq.orderPlaceReqRecForApproval.OrderType;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.ExpiryDate = OTSReqRec.AllReq.orderPlaceReqRecForApproval.ExpiryDate;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.InstrType = OTSReqRec.AllReq.orderPlaceReqRecForApproval.InstrType;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.CPType = OTSReqRec.AllReq.orderPlaceReqRecForApproval.CPType;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.CALevel = OTSReqRec.AllReq.orderPlaceReqRecForApproval.CALevel;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.StrikePrice = OTSReqRec.AllReq.orderPlaceReqRecForApproval.StrikePrice;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.Series = OTSReqRec.AllReq.orderPlaceReqRecForApproval.Series;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.RecallRepayOrder = OTSReqRec.AllReq.orderPlaceReqRecForApproval.RecallRepayOrder;
            if (SendOTP == 'Y') OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.SendOTP = 1;
            else OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.SendOTP = 0;
            OTSReplyRec.AllReply[0].orderPlaceReplyRecOfApproval.IntraMultiplierReq = OTSReqRec.AllReq.orderPlaceReqRecForApproval.IntraMultiplierReq;

            


        }

        private void ProcessApprovalOrderStatus()
        {
 
        }

        private void ProcessApprovalOrderOTPStatus(string ReplyStr)
        {
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.UpdateOTPOrderStatusResponse());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.UpdateOTPOrderStatusResponse());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;

            int OrderReferenceID;
            string ClientCode; char OTPStatusUpdate;

            VList = ReplyStr.Split(splitchar);
            ClientCode = VList[0].Trim();
            OrderReferenceID = Convert.ToInt32(VList[1]);
            OTPStatusUpdate = Convert.ToChar(VList[2]);
            if (VList.Count() == 4)
            {
                OTSReplyRec.ReplyHdr.ErrorCode = 1;
                OTSReplyRec.ErrorStringLength = (byte)VList[3].Length;
                OTSReplyRec.ErrorString = VList[3];
            }
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].UpdateOTPOrderStatusResponseRec.ClientCode = ClientCode.PadRight(14).ToCharArray();
            OTSReplyRec.AllReply[0].UpdateOTPOrderStatusResponseRec.ClientCodeLength = (byte)ClientCode.Trim().Length;
            OTSReplyRec.AllReply[0].UpdateOTPOrderStatusResponseRec.ReferenceID = OrderReferenceID;
            OTSReplyRec.AllReply[0].UpdateOTPOrderStatusResponseRec.OTPStatusUpdated = OTPStatusUpdate;
            if (OTPStatusUpdate == 'N') OTSReplyRec.AllReply[0].UpdateOTPOrderStatusResponseRec.OTPNumberAttempt = 1;
        }

        private void ProcessApprovalOrdersReport(DataTable dt)
        {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.PendingApprovalOrderReport());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);

            /*ClientCode, OrderRequesterCode, OrderAcceptTime, RequestType, Exch, ExchType, ScripCode, ScripName, BuySell, 
            Qty, Rate, WithSL, SLTriggerRate, AtMarket, IOC, ApprovalStatus, DisclosedQty, PendingQty, OrderOTPNumber, UserInfoNSE, UserInfoBSE, OrderInsertTime, MachineID, IntraDelivery,
            RequestType2, SLTriggered, TradedQty, OldOrderQty, ExchangeOrderId, ExchangeOrderTime, IsIntradayMultiplierRequest, FOSymbol, FOInstrType, FOCPType, FOExpiry, FOStrikeRate*/

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.TraderID = Convert.ToString(dt.Rows[i]["ClientCode"]).PadRight(14).ToCharArray();
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.TraderIDLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["ClientCode"]).TrimEnd().Length);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ReplyType = Convert.ToByte(dt.Rows[i]["RequestType2"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                if (OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ExchType == 'C')
                {
                    OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).PadRight(12).ToCharArray();
                    OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ScripNameLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["SCRIPNAME"]).TrimEnd().Length);
                }
                else
                {
                    OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["FOSymbol"]).PadRight(12).ToCharArray();
                    OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ScripNameLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["FOSymbol"]).TrimEnd().Length);
                    OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ExpiryDate = Convert.ToInt32(dt.Rows[i]["FOExpiry"]);
                    OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.StrikePrice = Convert.ToSingle(dt.Rows[i]["FOStrikeRate"]);
                    OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.CPType = Convert.ToByte(dt.Rows[i]["FOCPType"]);
                    OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.InstrType = Convert.ToByte(dt.Rows[i]["FOInstrType"]);
                }
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.PendingQty = Convert.ToInt32(dt.Rows[i]["PendingQty"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.TradedQty = Convert.ToInt32(dt.Rows[i]["TradedQty"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.OldQty = Convert.ToInt32(dt.Rows[i]["OldOrderQty"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.OTPNumber = Convert.ToInt32(dt.Rows[i]["OrderOTPNumber"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ReferenceID = Convert.ToInt32(dt.Rows[i]["OrderReferenceID"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["AtMarket"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.SLTriggered = Convert.ToChar(dt.Rows[i]["SLTriggered"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WithSL"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTriggerRate"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ExchangeOrderId = Convert.ToInt64(dt.Rows[i]["ExchangeOrderId"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchangeOrderTime"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.OrderReqTime = Convert.ToInt32(dt.Rows[i]["OrderAcceptTime"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.OrderType = Convert.ToByte(dt.Rows[i]["IntraDelivery"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.IntraMultiplierReq = Convert.ToByte(dt.Rows[i]["IsIntradayMultiplierRequest"]);
                OTSReplyRec.AllReply[i].PendingApprovalOrderReportReplyRec.ApprovalStatus = Convert.ToChar(dt.Rows[i]["IsIVRApproved"]);
            }
        }

        private void ProcessSpreadCancelOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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


                OTSReplyRec.ReplyHdr.ErrorCode = 0;
                OTSReplyRec.ErrorStringLength = 0;
                OTSReplyRec.ErrorString = "";

                OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderCancelBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderCancelBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);

                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.Reserved = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.OrderCategory;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.Exch = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.Exch;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.AtMarket = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AtMarket;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchType;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.RateDiff = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.RateDiff;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ULToken = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ScripCode;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.TraderRequestID;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.BrokerOrderTime = TimeNow;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchangeOrderId;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchangeOrderTime;

                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.BuySell = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.BuySell;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.CALevel = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.CALevel;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.CPType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.CPType;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.ExpiryDate = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.ExpiryDate;

                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.InstrType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.InstrType;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.Qty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Qty;

                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.StrikePrice = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.StrikePrice;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.Symbol = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Symbol;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.SymbolLength = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.SymbolLength;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.Token = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Token;

                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.BuySell = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.BuySell;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.CALevel = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.CALevel;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.CPType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.CPType;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.ExpiryDate = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.ExpiryDate;

                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.InstrType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.InstrType;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.Qty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.Qty;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.StrikePrice = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.StrikePrice;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.Symbol = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.Symbol;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.SymbolLength = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.SymbolLength;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.Token = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.Token;

                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.PendingQty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.PendingQty;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.PendingQty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.PendingQty;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.TradedQty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.TradedQty;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.TradedQty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.TradedQty;
                OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.AHStatus = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AHStatus;
                if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessSpreadModifyOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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

                OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderModifyBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderModifyBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);

                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.Reserved = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.OrderCategory;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.Exch = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.Exch;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.AtMarket = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AtMarket;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchType;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.RateDiff = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.RateDiff;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ULToken = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ScripCode;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.TraderRequestID;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.BrokerOrderTime = TimeNow;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchangeOrderId;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.ExchangeOrderTime;

                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.BuySell = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.BuySell;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.CALevel = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.CALevel;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.CPType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.CPType;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.ExpiryDate = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.ExpiryDate;

                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.InstrType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.InstrType;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.Qty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Qty;

                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.StrikePrice = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.StrikePrice;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.Symbol = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Symbol;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.SymbolLength = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.SymbolLength;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.Token = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.Token;

                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.BuySell = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.BuySell;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.CALevel = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.CALevel;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.CPType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.CPType;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.ExpiryDate = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.ExpiryDate;

                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.InstrType = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.InstrType;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.Qty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.Qty;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.StrikePrice = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.StrikePrice;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.Symbol = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.Symbol;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.SymbolLength = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.SymbolLength;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.Token = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.Token;

                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.PendingQty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.PendingQty;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.PendingQty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.PendingQty;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.TradedQty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg1.TradedQty;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.TradedQty = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.SpdLeg2.TradedQty;
                OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.AHStatus = OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec.AHStatus;
                if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
            }
            catch (Exception ex)
            {

                Form1.ErrorLog(ex);
            }
        }


        private void ProcessSpreadPlaceOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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

                OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderPlaceBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderPlaceBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);

                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.Reserved = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.OrderCategory;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.Exch = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.Exch;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.AtMarket = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.AtMarket;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.IOC = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.DayIOC;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.ExchType = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.ExchType;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.RateDiff = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.RateDiff;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.ULToken = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.ScripCode;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.TraderRequestID;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.BrokerOrderTime = TimeNow;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;

                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.BuySell = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.BuySell;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.CALevel = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.CALevel;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.CPType = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.CPType;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.ExpiryDate = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.ExpiryDate;

                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.InstrType = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.InstrType;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.Qty = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.Qty;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.StrikePrice = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.StrikePrice;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.Symbol = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.Symbol;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.SymbolLength = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.SymbolLength;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.Token = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg1.Token;

                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.BuySell = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.BuySell;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.CALevel = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.CALevel;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.CPType = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.CPType;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.ExpiryDate = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.ExpiryDate;

                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.InstrType = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.InstrType;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.Qty = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.Qty;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.StrikePrice = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.StrikePrice;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.Symbol = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.Symbol;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.SymbolLength = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.SymbolLength;
                OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.Token = OTSReqRec.AllReq.SpreadOrderPlaceRequestRec.SpdLeg2.Token;
                OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = 0;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessHoldingsValuationReport(DataTable dt)
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

        private void ProcessHoldingsValuationReportNew(DataTable dt)
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

                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsValuationReportWithMemVar());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.ScName = Convert.ToString(dt.Rows[i]["NAME"]).PadRight(12).ToCharArray();
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.ScNameLength = (byte)new string(OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.ScName).TrimEnd().Length;
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["TOTALQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.LastRatePrice = Convert.ToSingle(dt.Rows[i]["LASTRATE"]);
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.HolidingsValue = Convert.ToDouble(dt.Rows[i]["TOTALVALUE"]);
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.AfterHaircutValue = Convert.ToDouble(dt.Rows[i]["VALUEAFTERHAIRCUT"]);
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.BODVarMargin = Convert.ToSingle(dt.Rows[i]["BODVARMARGIN"]);
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.Reserved1 = 0;
                    OTSReplyRec.AllReply[i].HoldingsValuationReportWithMemVarReplyRec.Reserved2 = 0;

                }
            }
            catch (Exception e)
            {
                Form1.ErrorLog(e);
            }
        }

        private void ProcessEquityCancelOrderRequest(string ReplyStr)
        {

            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeCancelOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderCancelBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderCancelBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.Exch = OTSReqRec.AllReq.OrderTypeCancelReqRec.Exch;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchType = OTSReqRec.AllReq.OrderTypeCancelReqRec.ExchType;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripCode = OTSReqRec.AllReq.OrderTypeCancelReqRec.ScripCode;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.OrderTypeCancelReqRec.ScripName).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.OrderTypeCancelReqRec.ScripNameLength;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.BuySell = OTSReqRec.AllReq.OrderTypeCancelReqRec.BuySell;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.Qty = OTSReqRec.AllReq.OrderTypeCancelReqRec.Qty;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerTime = TimeNow;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.Rate = OTSReqRec.AllReq.OrderTypeCancelReqRec.Rate;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.AtMarket = OTSReqRec.AllReq.OrderTypeCancelReqRec.AtMarket;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.WithSL = OTSReqRec.AllReq.OrderTypeCancelReqRec.WithSL;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.OrderTypeCancelReqRec.SLTriggered;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.OrderTypeCancelReqRec.TriggerRate;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.OrderTypeCancelReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.TradedQty = OTSReqRec.AllReq.OrderTypeCancelReqRec.TradedQty;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.PendingQty = OTSReqRec.AllReq.OrderTypeCancelReqRec.PendingQty;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderID = OTSReqRec.AllReq.OrderTypeCancelReqRec.ExchangeOrderID;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.OrderTypeCancelReqRec.ExchangeOrderTime;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.AHStatus = AHStatus1;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.OrderType = OTSReqRec.AllReq.OrderTypeCancelReqRec.OrderType; //05.04.15
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ProductType = OTSReqRec.AllReq.OrderTypeCancelReqRec.ProductType;
                OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.AlgoID = OTSReqRec.AllReq.OrderTypeCancelReqRec.AlgoID;     // sumitp 28_jan_22
                if (OTSReqRec.AllReq.OrderTypeCancelReqRec.ExchType == 'D')
                {
                    OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.CALevel = OTSReqRec.AllReq.OrderTypeCancelReqRec.CALevel;
                    OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.CPType = OTSReqRec.AllReq.OrderTypeCancelReqRec.CPType;
                    OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ExpiryDate = OTSReqRec.AllReq.OrderTypeCancelReqRec.ExpiryDate;
                    OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.InstrType = OTSReqRec.AllReq.OrderTypeCancelReqRec.InstrType;
                    OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.StrikePrice = OTSReqRec.AllReq.OrderTypeCancelReqRec.StrikePrice;
                }
                if (OTSReqRec.AllReq.OrderTypeCancelReqRec.Exch != 'M')
                {
                    if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                    else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }

        }

        private void ProcessCurrDerivPlaceOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Exch == 'M')
                {
                    if (VList.Count() != 5 && VList.Count() != 6)//improper string
                    {
                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                        VList = ReplyStr.Split(splitchar);
                    }
                }
                else
                {
                    if (VList.Count() != 4 && VList.Count() != 5)//improper string
                    {
                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                        VList = ReplyStr.Split(splitchar);
                    }
                }            
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec.Exch == 'M')
                {
                    if (VList.Count() == 5)
                        AHStatus1 = Convert.ToChar(VList[4].ToUpper());
                    else
                        AHStatus1 = 'N';
                }
                else
                {
                    if (VList.Count() == 6)
                        AHStatus1 = Convert.ToChar(VList[5].ToUpper());
                    else
                        AHStatus1 = 'N';
                }                      
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
                OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = 0;
            }
            catch (Exception ex)
            {

                Form1.ErrorLog(ex);
            }
        }


        private void ProcessCurrDerivModifyOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Exch == 'M')
                {
                    if (VList.Count() != 4 && VList.Count() != 5)//improper string
                    {
                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                        VList = ReplyStr.Split(splitchar);
                    }
                }
                else
                {
                    if (VList.Count() != 5 && VList.Count() != 6)//improper string
                    {
                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                        VList = ReplyStr.Split(splitchar);
                    }
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Exch == 'M')
                {
                    if (VList.Count() == 5)
                        AHStatus1 = Convert.ToChar(VList[4].ToUpper());
                    else
                        AHStatus1 = 'N';
                }
                else
                {
                    if (VList.Count() == 6)
                        AHStatus1 = Convert.ToChar(VList[5].ToUpper());
                    else
                        AHStatus1 = 'N';
                }               
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
                if (OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec.Exch != 'M')
                {
                    if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                    else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessCurrDerivCancelOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Exch == 'M')
                {
                    if (VList.Count() != 4 && VList.Count() != 5)//improper string
                    {
                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                        VList = ReplyStr.Split(splitchar);
                    }
                }
                else
                {
                    if (VList.Count() != 5 && VList.Count() != 6)//improper string
                    {
                        ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                        VList = ReplyStr.Split(splitchar);
                    }
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Exch == 'M')
                {
                    if (VList.Count() == 5)
                        AHStatus1 = Convert.ToChar(VList[4].ToUpper());
                    else
                        AHStatus1 = 'N';
                }
                else
                {
                    if (VList.Count() == 6)
                        AHStatus1 = Convert.ToChar(VList[5].ToUpper());
                    else
                        AHStatus1 = 'N';
                }             
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
                if (OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec.Exch != 'M')
                {
                    if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                    else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessEquityModifyOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeConversionWithExpiryRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 4 && VList.Count() != 5 && VList.Count() != 6)//improper string
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
                    if (OTSReqRec.AllReq.OrderTypeConversionRec.Reserved1 == 1)//Flag For Checking Bod Order Conversion
                        MBlk = Convert.ToSingle(VList[5]);
                    else
                        MBlk = Convert.ToSingle(VList[3]);
                }
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderConversionRecReply());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderConversionRecReply());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Exch = OTSReqRec.AllReq.OrderTypeConversionRec.Exch;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchType = OTSReqRec.AllReq.OrderTypeConversionRec.ExchType;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ScripCode = OTSReqRec.AllReq.OrderTypeConversionRec.ScripCode;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ScripName = new string(OTSReqRec.AllReq.OrderTypeConversionRec.ScripName).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ScripNameLength = OTSReqRec.AllReq.OrderTypeConversionRec.ScripNameLength;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.BuySell = OTSReqRec.AllReq.OrderTypeConversionRec.BuySell;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Qty = OTSReqRec.AllReq.OrderTypeConversionRec.Qty;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Rate = OTSReqRec.AllReq.OrderTypeConversionRec.Rate;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.BrokerTime = TimeNow;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchangeOrderId = OTSReqRec.AllReq.OrderTypeConversionRec.ExchangeOrderID;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchangeTradeID = OTSReqRec.AllReq.OrderTypeConversionRec.ExchangeTradeID;
                if (OTSReqRec.AllReq.OrderTypeConversionRec.Reserved1 == 1)//Flag For Checking Bod Order Conversion
                {
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchangeOrderId = Convert.ToInt64(VList[2]);//ExchageOrderId
                    if (VList[2] != "1")
                        OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchangeTradeID = Convert.ToInt64(VList[3]);//ExchageTradeId
                }
                else
                {
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchangeOrderId = OTSReqRec.AllReq.OrderTypeConversionRec.ExchangeOrderID;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchangeTradeID = OTSReqRec.AllReq.OrderTypeConversionRec.ExchangeTradeID;
                }
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.OrderType = OTSReqRec.AllReq.OrderTypeConversionRec.OrderType;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ProductType = OTSReqRec.AllReq.OrderTypeConversionRec.ProductType;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Reserved1 = OTSReqRec.AllReq.OrderTypeConversionRec.Reserved1;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Reserved2 = OTSReqRec.AllReq.OrderTypeConversionRec.Reserved2;

                if (OTSReqRec.AllReq.OrderTypeConversionRec.ExchType == 'D')
                {
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.CALevel = OTSReqRec.AllReq.OrderTypeConversionRec.CALevel;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.CPType = OTSReqRec.AllReq.OrderTypeConversionRec.CPType;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExpiryDate = OTSReqRec.AllReq.OrderTypeConversionRec.ExpiryDate;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.InstrType = OTSReqRec.AllReq.OrderTypeConversionRec.InstrType;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.StrikePrice = OTSReqRec.AllReq.OrderTypeConversionRec.StrikePrice;
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeConversionRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderConversionRecReply());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderConversionRecReply());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Exch = OTSReqRec.AllReq.OrderTypeConversionRec.Exch;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchType = OTSReqRec.AllReq.OrderTypeConversionRec.ExchType;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ScripCode = OTSReqRec.AllReq.OrderTypeConversionRec.ScripCode;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ScripName = new string(OTSReqRec.AllReq.OrderTypeConversionRec.ScripName).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ScripNameLength = OTSReqRec.AllReq.OrderTypeConversionRec.ScripNameLength;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.BuySell = OTSReqRec.AllReq.OrderTypeConversionRec.BuySell;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Qty = OTSReqRec.AllReq.OrderTypeConversionRec.Qty;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Rate = OTSReqRec.AllReq.OrderTypeConversionRec.Rate;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.BrokerTime = TimeNow;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchangeOrderId = OTSReqRec.AllReq.OrderTypeConversionRec.ExchangeOrderID;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExchangeTradeID = OTSReqRec.AllReq.OrderTypeConversionRec.ExchangeTradeID;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.OrderType = OTSReqRec.AllReq.OrderTypeConversionRec.OrderType;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ProductType = OTSReqRec.AllReq.OrderTypeConversionRec.ProductType;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Reserved1 = OTSReqRec.AllReq.OrderTypeConversionRec.Reserved1;
                OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.Reserved2 = OTSReqRec.AllReq.OrderTypeConversionRec.Reserved2;

                if (OTSReqRec.AllReq.OrderTypeConversionRec.ExchType == 'D')
                {
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.CALevel = OTSReqRec.AllReq.OrderTypeConversionRec.CALevel;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.CPType = OTSReqRec.AllReq.OrderTypeConversionRec.CPType;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.ExpiryDate = OTSReqRec.AllReq.OrderTypeConversionRec.ExpiryDate;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.InstrType = OTSReqRec.AllReq.OrderTypeConversionRec.InstrType;
                    OTSReplyRec.AllReply[0].OrderTypeConversionReplyRec.StrikePrice = OTSReqRec.AllReq.OrderTypeConversionRec.StrikePrice;
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeModifyOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;

                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderModifyBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderModifyBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.Exch = OTSReqRec.AllReq.OrderTypeModifyReqRec.Exch;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchType = OTSReqRec.AllReq.OrderTypeModifyReqRec.ExchType;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripCode = OTSReqRec.AllReq.OrderTypeModifyReqRec.ScripCode;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.OrderTypeModifyReqRec.ScripName).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.OrderTypeModifyReqRec.ScripNameLength;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.BuySell = OTSReqRec.AllReq.OrderTypeModifyReqRec.BuySell;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.Qty = OTSReqRec.AllReq.OrderTypeModifyReqRec.Qty;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.AtMarket = OTSReqRec.AllReq.OrderTypeModifyReqRec.AtMarket;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.Rate = OTSReqRec.AllReq.OrderTypeModifyReqRec.Rate;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.WithSL = OTSReqRec.AllReq.OrderTypeModifyReqRec.WithSL;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.OrderTypeModifyReqRec.SLTriggered;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.OrderTypeModifyReqRec.TriggerRate;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.OrderTypeModifyReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.IOC = OTSReqRec.AllReq.OrderTypeModifyReqRec.IOC;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.OrderTypeModifyReqRec.DisclosedQty;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerTime = TimeNow;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.OldQty = OTSReqRec.AllReq.OrderTypeModifyReqRec.OldQty;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.TradedQty = OTSReqRec.AllReq.OrderTypeModifyReqRec.TradedQty;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.PendingQty = OTSReqRec.AllReq.OrderTypeModifyReqRec.PendingQty;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.OrderTypeModifyReqRec.ExchangeOrderId;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.OrderTypeModifyReqRec.ExchangeOrderTime;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.AHStatus = AHStatus1;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.OrderType = OTSReqRec.AllReq.OrderTypeModifyReqRec.OrderType; //05.04.15
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ProductType = OTSReqRec.AllReq.OrderTypeModifyReqRec.ProductType;
                OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.AlgoID = OTSReqRec.AllReq.OrderTypeModifyReqRec.AlgoID; // OrderTypeNew---- Cover(3)
                if (OTSReqRec.AllReq.OrderTypeModifyReqRec.ExchType == 'D')
                {
                    OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.CALevel = OTSReqRec.AllReq.OrderTypeModifyReqRec.CALevel;
                    OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.CPType = OTSReqRec.AllReq.OrderTypeModifyReqRec.CPType;
                    OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ExpiryDate = OTSReqRec.AllReq.OrderTypeModifyReqRec.ExpiryDate;
                    OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.InstrType = OTSReqRec.AllReq.OrderTypeModifyReqRec.InstrType;
                    OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.StrikePrice = OTSReqRec.AllReq.OrderTypeModifyReqRec.StrikePrice;
                }
                if (OTSReqRec.AllReq.OrderTypeModifyReqRec.Exch != 'M')
                {
                    if (AHStatus1 == 'Y') OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 2]);
                    else OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = Convert.ToInt64(VList[VList.Length - 1]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessAHEquityPlaceOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;

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

        private void ProcessEquityPlaceOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AlgoID = OTSReqRec.AllReq.EquityOrderPlaceRequestRec.AlgoID;
                OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerTime = TimeNow;
                OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AHStatus = AHStatus1;
                OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = 0;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypePlaceOrderRequest(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.Exch = OTSReqRec.AllReq.OrderTypePlaceReqRec.Exch;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.ExchType = OTSReqRec.AllReq.OrderTypePlaceReqRec.ExchType;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripCode = OTSReqRec.AllReq.OrderTypePlaceReqRec.ScripCode;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.OrderTypePlaceReqRec.ScripName).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.OrderTypePlaceReqRec.ScripNameLength;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.BuySell = OTSReqRec.AllReq.OrderTypePlaceReqRec.BuySell;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.Qty = OTSReqRec.AllReq.OrderTypePlaceReqRec.Qty;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.AtMarket = OTSReqRec.AllReq.OrderTypePlaceReqRec.AtMarket;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.Rate = OTSReqRec.AllReq.OrderTypePlaceReqRec.Rate;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.WithSL = OTSReqRec.AllReq.OrderTypePlaceReqRec.WithSL;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.OrderTypePlaceReqRec.TriggerRate;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.OrderTypePlaceReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.IOC = OTSReqRec.AllReq.OrderTypePlaceReqRec.IOC;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.OrderTypePlaceReqRec.DisclosedQty;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerTime = TimeNow;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.AHStatus = AHStatus1;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.OrderType = OTSReqRec.AllReq.OrderTypePlaceReqRec.OrderType;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.AlgoID = OTSReqRec.AllReq.OrderTypePlaceReqRec.AlgoID;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.Reserved2 = OTSReqRec.AllReq.OrderTypePlaceReqRec.Reserved2;
                OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.ProductType = OTSReqRec.AllReq.OrderTypePlaceReqRec.ProductType;
                if (OTSReqRec.AllReq.OrderTypePlaceReqRec.ExchType == 'D')
                {
                    OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.CALevel = OTSReqRec.AllReq.OrderTypePlaceReqRec.CALevel;
                    OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.CPType = OTSReqRec.AllReq.OrderTypePlaceReqRec.CPType;
                    OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.ExpiryDate = OTSReqRec.AllReq.OrderTypePlaceReqRec.ExpiryDate;
                    OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.InstrType = OTSReqRec.AllReq.OrderTypePlaceReqRec.InstrType;
                    OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.StrikePrice = OTSReqRec.AllReq.OrderTypePlaceReqRec.StrikePrice;
                }

                OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = 0;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //This method is for Cover Place Order Only
        private void ProcessCoverOrderTypePlaceOrderRequestLeg1(string ReplyStr)     // sumitp 28_jan_22
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecordCoverAndBracket());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecordCoverAndBracket());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch = OTSReqRec.AllReq.PlaceCoverOrderReqRec.Exch;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExchType;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripCode;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName = new string(OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripName).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripNameLength;


                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty = OTSReqRec.AllReq.PlaceCoverOrderReqRec.Qty;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket = OTSReqRec.AllReq.PlaceCoverOrderReqRec.AtMarket;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate = OTSReqRec.AllReq.PlaceCoverOrderReqRec.Rate;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.WithSL = OTSReqRec.AllReq.PlaceCoverOrderReqRec.WithSL;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate = OTSReqRec.AllReq.PlaceCoverOrderReqRec.TriggerRate;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID = OTSReqRec.AllReq.PlaceCoverOrderReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC = OTSReqRec.AllReq.PlaceCoverOrderReqRec.IOC;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty = OTSReqRec.AllReq.PlaceCoverOrderReqRec.DisclosedQty;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1 = BOId;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1 = OTSReqRec.AllReq.PlaceCoverOrderReqRec.BuySell;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1 = TimeNow;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus = AHStatus1;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderType;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ProductType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ProductType;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO = OTSReqRec.AllReq.PlaceCoverOrderReqRec.SLRateCO;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderTypeNew = OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderTypeNew;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.EpochTime = OTSReqRec.AllReq.PlaceCoverOrderReqRec.EpochTime;

                if (OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExchType == 'D')
                {
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CALevel = OTSReqRec.AllReq.PlaceCoverOrderReqRec.CALevel;
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CPType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.CPType;
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExpiryDate = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExpiryDate;
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.InstrType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.InstrType;
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.StrikePrice = OTSReqRec.AllReq.PlaceCoverOrderReqRec.StrikePrice;
                }

                OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = 0;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }


        //This method is for Cover Place Order Only 
        private void ProcessCoverOrderTypePlaceOrderRequestLeg2(string ReplyStr, char BuySell2, Single Rate)     // sumitp 28_jan_22
        {
            try
            {
                int BOId, TimeNow;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                if (VList.Count() == 6)
                    AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecordCoverAndBracket());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecordCoverAndBracket());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch = OTSReqRec.AllReq.PlaceCoverOrderReqRec.Exch;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExchType;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripCode;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName = new string(OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripName).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ScripNameLength;


                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty = OTSReqRec.AllReq.PlaceCoverOrderReqRec.Qty;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket = OTSReqRec.AllReq.PlaceCoverOrderReqRec.AtMarket;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate = OTSReqRec.AllReq.PlaceCoverOrderReqRec.Rate;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.WithSL = OTSReqRec.AllReq.PlaceCoverOrderReqRec.WithSL;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate = OTSReqRec.AllReq.PlaceCoverOrderReqRec.TriggerRate;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID = OTSReqRec.AllReq.PlaceCoverOrderReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC = OTSReqRec.AllReq.PlaceCoverOrderReqRec.IOC;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty = OTSReqRec.AllReq.PlaceCoverOrderReqRec.DisclosedQty;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID2 = BOId;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2 = BuySell2;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime2 = TimeNow;


                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO = Rate;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked = MBlk;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus = AHStatus1;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderType;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ProductType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ProductType;

                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderTypeNew = OTSReqRec.AllReq.PlaceCoverOrderReqRec.OrderTypeNew;
                OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.EpochTime = OTSReqRec.AllReq.PlaceCoverOrderReqRec.EpochTime;

                if (OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExchType == 'D')
                {
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CALevel = OTSReqRec.AllReq.PlaceCoverOrderReqRec.CALevel;
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CPType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.CPType;
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExpiryDate = OTSReqRec.AllReq.PlaceCoverOrderReqRec.ExpiryDate;
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.InstrType = OTSReqRec.AllReq.PlaceCoverOrderReqRec.InstrType;
                    OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.StrikePrice = OTSReqRec.AllReq.PlaceCoverOrderReqRec.StrikePrice;
                }

                OTSReplyRec.AllReply[0].LastActReference.LastActivityReference = 0;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOptionsExerciseRequest(string ReplyStr)//03.08.17
        {
            try
            {
                int BOId, TimeNow;
                char AHStatus1;
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

                }
                else
                {
                    OTSReplyRec.ReplyHdr.ErrorCode = 0;
                    OTSReplyRec.ErrorStringLength = 0;
                    OTSReplyRec.ErrorString = "";

                }
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOptionExerciseBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOptionExerciseBrokerReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.Exch = OTSReqRec.AllReq.OptionExerciseRequestRec.Exch;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.ExchType = OTSReqRec.AllReq.OptionExerciseRequestRec.ExchType;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.ScripCode = OTSReqRec.AllReq.OptionExerciseRequestRec.ScripCode;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.Exercise = OTSReqRec.AllReq.OptionExerciseRequestRec.Exercise;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.Qty = OTSReqRec.AllReq.OptionExerciseRequestRec.Qty;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.BrokerTime = TimeNow;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.OptionExerciseRequestRec.ExchangeOrderId;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.OptionExerciseRequestRec.ExchangeOrderTime;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.BuySell = OTSReqRec.AllReq.OptionExerciseRequestRec.BuySell;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.Reserved1 = OTSReqRec.AllReq.OptionExerciseRequestRec.Reserved1;
                OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.Reserved2 = 0;

                if (OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.ExchType == 'D')
                {
                    OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.DerivInfo.InstrType = OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.InstrType;
                    OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.DerivInfo.Symbol = OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.Symbol;
                    OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.DerivInfo.SymbolLength = OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.SymbolLength;
                    OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.DerivInfo.ExpiryDate = OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.ExpiryDate;
                    OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.DerivInfo.StrikePrice = OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.StrikePrice;
                    OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.DerivInfo.CALevel = OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.CALevel;
                    OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.DerivInfo.CPType = OTSReqRec.AllReq.OptionExerciseRequestRec.DerivInfo.CPType;

                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessExerciseReport(DataTable dt)
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
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TExerciseReportReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.Exercise = Convert.ToChar(dt.Rows[i]["EXERCISE"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]).ToCharArray();
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.Status.Length);
                    OTSReplyRec.AllReply[i].OptionExerciseReportReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                }

            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
           
        }


        private void ProcessHoldingsReportWithCollatral(DataTable dt)
        {
            try
            {
                int dtRowsCount = dt.Rows.Count, i;    //Optimization changes - Rohan 19-Apr-2017

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportReplyRecord_WithCollatral());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dtRowsCount;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (i = 0; i < dtRowsCount; i++)
                {
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.ScripName.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRec.Collatral = Convert.ToInt32(dt.Rows[i]["COLLATERALQTY"]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessHoldingsReportWithCollatralNew(DataTable dt)//16.12.16
        {
            try
            {
                int dtRowsCount = dt.Rows.Count, i;    //Optimization changes - Rohan 19-Apr-2017

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportReplyRecord_WithCollatralNew());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dtRowsCount;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (i = 0; i < dtRowsCount; i++)
                {
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.ScripName.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralReplyRecNew.Collatral = Convert.ToInt32(dt.Rows[i]["COLLATERALQTY"]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);

            }
        }

        private void ProcessHoldingsReportNew(DataTable dt)
        {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportPreCloseReplyRecord_WithMTF());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].HoldingsReportMTF.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.ScripNameLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["SCRIPNAME"]).TrimEnd().Length);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.FundedStockQty = Convert.ToInt32(dt.Rows[i]["FundedQty"]);// Convert.ToInt32(dt.Rows[i]["FundedStockQty"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTF.Collatral = Convert.ToInt32(dt.Rows[i]["COLLATERALQTY"]);
               // OTSReplyRec.AllReply[i].HoldingsReportMTF.ExchRcvQty2 = Convert.ToInt32(dt.Rows[i]["ExchRcvQty2"]);
                if (dt.Rows[i]["PreClose"] != DBNull.Value)
                    OTSReplyRec.AllReply[i].HoldingsReportMTF.PrevClose = Convert.ToDouble(dt.Rows[i]["PreClose"]);
                else
                    OTSReplyRec.AllReply[i].HoldingsReportMTF.PrevClose = Convert.ToDouble("0.00");
                OTSReplyRec.AllReply[i].HoldingsReportMTF.PendingQty = Convert.ToInt32(dt.Rows[i]["PendingQty"]);
                if (dt.Rows[i]["PurchasePrice"] != DBNull.Value)
                    OTSReplyRec.AllReply[i].HoldingsReportMTF.PurchasePrice = Convert.ToDouble(dt.Rows[i]["PurchasePrice"]);
                else
                    OTSReplyRec.AllReply[i].HoldingsReportMTF.PurchasePrice = Convert.ToDouble("0.00");
            }
        }
        private void ProcessHoldingsReportNewwithMTF(DataTable dt)
        {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportPreCloseReplyRecord_WithMTFNew());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.ScripNameLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["SCRIPNAME"]).TrimEnd().Length);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.FundedStockQty = Convert.ToInt32(dt.Rows[i]["FundedQty"]);// Convert.ToInt32(dt.Rows[i]["FundedStockQty"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.Collatral = Convert.ToInt32(dt.Rows[i]["COLLATERALQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.ExchRcvQty2 = Convert.ToInt32(dt.Rows[i]["ExchRcvQty2"]);
                if (dt.Rows[i]["PreClose"] != DBNull.Value)
                    OTSReplyRec.AllReply[i].HoldingsReportMTFNew.PrevClose = Convert.ToDouble(dt.Rows[i]["PreClose"]);
                else
                    OTSReplyRec.AllReply[i].HoldingsReportMTFNew.PrevClose = Convert.ToDouble("0.00");
                OTSReplyRec.AllReply[i].HoldingsReportMTFNew.PendingQty = Convert.ToInt32(dt.Rows[i]["PendingQty"]);
                if (dt.Rows[i]["PurchasePrice"] != DBNull.Value)
                    OTSReplyRec.AllReply[i].HoldingsReportMTFNew.PurchasePrice = Convert.ToDouble(dt.Rows[i]["PurchasePrice"]);
                else
                    OTSReplyRec.AllReply[i].HoldingsReportMTFNew.PurchasePrice = Convert.ToDouble("0.00");
                //OTSReplyRec.AllReply[i].HoldingsReportMTFNew.FSCollateralQty = Convert.ToInt32(dt.Rows[i]["FSCollateralQty"]);
            }
        }

        private void ProcessHoldingsReportNew_All(DataTable dt)
        {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new IntellectSharedClassLibrary.ISCL_HoldingEquityReply());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.ScripNameLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["SCRIPNAME"]).TrimEnd().Length);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.FundedStockQty = Convert.ToInt32(dt.Rows[i]["FundedQty"]);// Convert.ToInt32(dt.Rows[i]["FundedStockQty"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.Collatral = Convert.ToInt32(dt.Rows[i]["COLLATERALQTY"]);
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.ExchRcvQty2 = Convert.ToInt32(dt.Rows[i]["ExchRcvQty2"]);
                if (dt.Rows[i]["PreClose"] != DBNull.Value)
                    OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.PrevClose = Convert.ToDouble(dt.Rows[i]["PreClose"]);
                else
                    OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.PrevClose = Convert.ToDouble("0.00");
                OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.PendingQty = Convert.ToInt32(dt.Rows[i]["PendingQty"]);
                if (dt.Rows[i]["PurchasePrice"] != DBNull.Value)
                    OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.PurchasePrice = Convert.ToDouble(dt.Rows[i]["PurchasePrice"]);
                else
                    OTSReplyRec.AllReply[i].ISCL_HoldingEquityReply.PurchasePrice = Convert.ToDouble("0.00");
            }
        }



        private void ProcessHoldingsReportPreCloseWithCollatralNew(DataTable dt)//106 jinal 
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
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportPreCloseReplyRecord_WithCollatralNew());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.ScripNameLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["SCRIPNAME"]).TrimEnd().Length);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                    //OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.FundedStockQty = Convert.ToInt32(dt.Rows[i]["FUNDEDQTY"]); // ad by Himanshu - 01.10.2022
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.Collatral = Convert.ToInt32(dt.Rows[i]["COLLATERALQTY"]);
                    if (dt.Rows[i]["PreClose"] != DBNull.Value)
                        OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.PrevClose = Convert.ToDouble(dt.Rows[i]["PreClose"]);
                    else
                        OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.PrevClose = Convert.ToDouble("0.00");
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.PendingQty = Convert.ToInt32(dt.Rows[i]["PendingQty"]);
                    if (dt.Rows[i]["PurchasePrice"] != DBNull.Value)
                      OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.PurchasePrice = Convert.ToDouble(dt.Rows[i]["PurchasePrice"]);
                    else
                        OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.PurchasePrice = Convert.ToDouble("0.00");
                   // OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew.FSCollateralQty = Convert.ToInt32(dt.Rows[i]["FSCOLLATERALQTY"]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessHoldingsReportPreCloseWithCollatralNew_FS(DataTable dt)//112 Himanshu Soni
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
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportPreCloseReplyRecord_WithCollatralNew_FS());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.ScripNameLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["SCRIPNAME"]).TrimEnd().Length);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.FundedStockQty = Convert.ToInt32(dt.Rows[i]["FUNDEDQTY"]); // ad by Himanshu - 01.10.2022
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.Collatral = Convert.ToInt32(dt.Rows[i]["COLLATERALQTY"]);
                    if (dt.Rows[i]["PreClose"] != DBNull.Value)
                        OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.PrevClose = Convert.ToDouble(dt.Rows[i]["PreClose"]);
                    else
                        OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.PrevClose = Convert.ToDouble("0.00");
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.PendingQty = Convert.ToInt32(dt.Rows[i]["PendingQty"]);
                    if (dt.Rows[i]["PurchasePrice"] != DBNull.Value)
                        OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.PurchasePrice = Convert.ToDouble(dt.Rows[i]["PurchasePrice"]);
                    else
                        OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.PurchasePrice = Convert.ToDouble("0.00");
                    OTSReplyRec.AllReply[i].HoldingsReportWithCollatralPreCloseReplyRecNew_FS.FSCollateralQty = Convert.ToInt32(dt.Rows[i]["FSCOLLATERALQTY"]);
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessAllClientHoldingsReportNew(DataTable dt) //31.01.17
        {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0;
                int dtRowsCount = dt.Rows.Count;    //Optimization changes - Rohan 19-Apr-2017

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dtRowsCount;
                while (Reply > 60000)
                {
                    Reply = Reply - 60000;
                    cnt = cnt + 60000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;

                TotalRecords = dtRowsCount;
                Reach = 0;
                for (int k = 0; k < rows; k++)
                {
                    TotalRecords = (dtRowsCount) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(60000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (int i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.TraderID = Convert.ToString(dt.Rows[i]["CLIENTCODE"]).PadRight(15);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.TraderIDLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.TraderID.ToString().TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).PadRight(50).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.ScripName.ToString().TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecNew.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                        RCnt++;
                    }
                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = OTSReplyRec.ReplyHdr.ReplyType;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TAllClientsHoldingsReportReplyRecord());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessAllClientHoldingsReportWithPreClose(DataTable dt) //31.01.17
        {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0;

                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dt.Rows.Count;
                while (Reply > 60000)
                {
                    Reply = Reply - 60000;
                    cnt = cnt + 60000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;

                TotalRecords = dt.Rows.Count;
                Reach = 0;
                for (int k = 0; k < rows; k++)
                {
                    TotalRecords = (dt.Rows.Count) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(60000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (int i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.TraderID = Convert.ToString(dt.Rows[i]["CLIENTCODE"]).PadRight(15);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.TraderIDLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.TraderID.ToString().TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).PadRight(50).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.ScripName.ToString().TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.PrevClose = Convert.ToDouble(dt.Rows[i]["PrevClose"]);
                        bool exists = dt.Columns.Contains("PurchasePrice");
                        if (exists == true)
                        {
                            if (dt.Rows[i]["PurchasePrice"] != DBNull.Value)
                                OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.PurchasePrice = Convert.ToDouble(dt.Rows[i]["PurchasePrice"]);
                            else
                                OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRecWithPreClose.PurchasePrice = Convert.ToDouble("0.00");
                        }
                        RCnt++;
                    }
                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = OTSReplyRec.ReplyHdr.ReplyType;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TAllClientsHoldingsReportReplyRecordWithPreClose());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }


        private void ProcessClientPositionReport(DataTable dt)
        {
            try
            {
                int dtRowsCount = dt.Rows.Count;    //Optimization changes - Rohan 19-Apr-2017
                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TClientPositionReportReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dtRowsCount;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dtRowsCount; i++)
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

        private void ProcessAllClientHoldingsReport(DataTable dt)
        {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0;
                int dtRowsCount = dt.Rows.Count;    //Optimization changes - Rohan 19-Apr-2017

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dtRowsCount;
                while (Reply > 60000)
                {
                    Reply = Reply - 60000;
                    cnt = cnt + 60000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;

                TotalRecords = dtRowsCount;
                Reach = 0;
                for (int k = 0; k < rows; k++)
                {
                    TotalRecords = (dtRowsCount) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(60000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (int i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.TraderID = Convert.ToString(dt.Rows[i]["CLIENTCODE"]).PadRight(15);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.TraderIDLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.TraderID.ToString().TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).PadRight(50).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.ScripName.ToString().TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsReportReplyRec.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                        RCnt++;
                    }
                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = OTSReplyRec.ReplyHdr.ReplyType;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportReplyRecordNew());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessAllClientHoldingsEquityReport(DataTable dt)
        {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0;
                int dtRowsCount = dt.Rows.Count;    //Optimization changes - Rohan 19-Apr-2017

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dtRowsCount;
                while (Reply > 5000)
                {
                    Reply = Reply - 5000;
                    cnt = cnt + 5000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;

                TotalRecords = dtRowsCount;
                Reach = 0;
                for (int k = 0; k < rows; k++)
                {
                    TotalRecords = (dtRowsCount) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(5000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (int i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.TraderID = Convert.ToString(dt.Rows[i]["CLIENTCODE"]).PadRight(15);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.TraderIDLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.TraderID.ToString().TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).PadRight(50).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.ScripName.ToString().TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].AllClientsHoldingsEquityReportReplyRec.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                        RCnt++;
                    }
                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = OTSReplyRec.ReplyHdr.ReplyType;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsEquityReportReplyRecordNew());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessVerifyTrader(string ReplyStr)
        {
            try
            {
                string C1, CCode2;
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

        private void GetDealerDetailsForIVR(string ReplyStr)
        {
            string C1, CCode2;
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 1)//improper string
            {
                ReplyStr = "1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.GetIVRDealerDetails());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.GetIVRDealerDetails());
            Array.Resize(ref OTSReplyRec.AllReply, 1);

            C1 = VList[0];
            if (C1 == "Y")
                OTSReplyRec.AllReply[0].GetIVRDealerDetailsReply.IsIVRDealer = true;
            else
                OTSReplyRec.AllReply[0].GetIVRDealerDetailsReply.IsIVRDealer = false;


        }

        private void ProcessMFClientHoldingReport(DataTable dt)
        {
            try
            {
                int dtRowsCount = dt.Rows.Count, i;    //Optimization changes - Rohan 19-Apr-2017
                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMaalFodoClientHoldingReportReplyRecord());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dtRowsCount;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (i = 0; i < dtRowsCount; i++)
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

        private void ProcessMFDebitClients(DataTable dt)
        {
            try
            {
                ITInteractiveStructures.TMaalFodoClientsReportReplyRecord MFCliReportReplyRec = new ITInteractiveStructures.TMaalFodoClientsReportReplyRecord();
                int j = 0;
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
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)j;
                OTSReplyRec.ReplyHdr.ReplyLen = j * OTSReplyRec.ReplyHdr.ReplyRecSize;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessAdhocModifyReply(string ReplyStr)
        {
            try
            {
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

        private void ProcessAdhocReport(DataTable dt)
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

        private void ProcessTraderList(DataTable dt)
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
                    if (Convert.ToString(dt.Rows[i]["AllowNseCurrDeriv"]).TrimEnd() == "Y")
                        OTSReplyRec.AllReply[i].TraderListReplyRec.TraderType = 1;
                    else
                        OTSReplyRec.AllReply[i].TraderListReplyRec.TraderType = 0;
                    //OTSReplyRec.AllReply[i].TraderListReplyRec.TraderType = Convert.ToByte(dt.Rows[i]["ClientType"]);
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

        private void ProcessTraderListNew(DataTable dt1)
        {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0;
                DataTable dt;

                dt = dt1;

                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dt.Rows.Count;
                while (Reply > 5000)
                {
                    Reply = Reply - 5000;
                    cnt = cnt + 5000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                //Array.Resize(ref  OTSReplyRec.AllReplyNew, rows);
                //Array.Resize(ref OTSReplyRec.NewReplyHdr, rows);

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = dt.Rows.Count;

                TotalRecords = dt.Rows.Count;
                Reach = 0;
                for (int k = 0; k < rows; k++)
                {
                    TotalRecords = (dt.Rows.Count) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(5000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (int i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderID = Convert.ToString(dt.Rows[i]["ClientCode"]).PadRight(14).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderIDLength = (byte)new string(OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderID).TrimEnd().Length;
                        OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderName = Convert.ToString(dt.Rows[i]["ClientName"]);

                        if ((byte)OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderName.TrimEnd().Length > 29)
                        {
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderNameLength = 29;
                        }
                        else
                        {
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderNameLength = (byte)OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderName.TrimEnd().Length;
                        }
                        //09.05.2016 Pranoti
                        if (Convert.ToString(dt.Rows[i]["AllowNseCurrDeriv"]).TrimEnd() == "Y")
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderType = 1;
                        else
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.TraderType = 0;

                        OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.BranchID = Convert.ToUInt16(dt.Rows[i]["BranchID"]);
                        if (Convert.ToString(dt.Rows[i]["AllowNseCash"]).TrimEnd() == "Y")
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.AllowNseCash = 1;
                        else
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.AllowNseCash = 0;

                        if (Convert.ToString(dt.Rows[i]["AllowNseDeriv"]).TrimEnd() == "Y")
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.AllowNseDeriv = 1;
                        else
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.AllowNseDeriv = 0;

                        if (Convert.ToString(dt.Rows[i]["AllowBseCash"]).TrimEnd() == "Y")
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.AllowBseCash = 1;
                        else
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.AllowBseCash = 0;

                        if (Convert.ToString(dt.Rows[i]["ClientCode2"]).TrimEnd() == "0")
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.NilOrders = 1;
                        else
                            OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.NilOrders = 0;

                        OTSReplyRec.AllReplyNew[k][RCnt].TraderListReplyRec.AccountType = AcTypeStrToByte(Convert.ToString(dt.Rows[i]["AccountType"]).TrimEnd());
                        //}
                        RCnt++;
                    }

                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = OTSReplyRec.ReplyHdr.ReplyType;

                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTraderListReplyRecord());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;// dt.Rows.Count;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception et)
            {
                OTSReplyRec.ErrorString = et.Message;
                Form1.ErrorLog(et);
            }
        }

        private void ProcessDealerOrderReport(DataTable dt1)
        {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0;
                DataTable dt;
                int RPos, ScripCode, Qty, LotSize;
                char RType, Exch;
                string Str1, ScName;
                Int64 ExchOrderID;
                dt = dt1;

                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dt.Rows.Count;
                while (Reply > 5000)
                {
                    Reply = Reply - 5000;
                    cnt = cnt + 5000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                //Array.Resize(ref  OTSReplyRec.AllReplyNew, rows);
                //Array.Resize(ref OTSReplyRec.NewReplyHdr, rows);

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = dt.Rows.Count;

                TotalRecords = dt.Rows.Count;
                Reach = 0;
                for (int k = 0; k < rows; k++)
                {
                    TotalRecords = (dt.Rows.Count) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(5000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (int i = Reach; i < rem; i++)
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
                            if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                            {
                                string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                                LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                            }
                            Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            dt.Rows[i]["QTY"] = (Qty / LotSize);
                        }
                        else if (Exch == 'E') //bseCurr
                        {
                            ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                            if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                            {
                                string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                                LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                            }
                            Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            dt.Rows[i]["QTY"] = (Qty / LotSize);
                        }

                        if (RType == 'P') //Place
                        {
                            RPos = RCnt;
                            //RCnt++;
                            //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                        }
                        else
                        {
                            Str1 = Convert.ToString(dt.Rows[i]["STATUS"]).TrimEnd(TrimChars);
                            if (Str1 == "Rejected" || Str1 == "Rejected By Broker")
                            {
                                RCnt++;
                                continue;
                            }
                            for (int j = 0; j < RCnt; j++)
                            {
                                if (ExchOrderID != OTSReplyRec.AllReplyNew[k][j].OrdersReportDealer.ExchangeOrderID) continue;
                                if (Exch != OTSReplyRec.AllReplyNew[k][j].OrdersReportDealer.Exch) continue;
                                if (ScripCode != OTSReplyRec.AllReplyNew[k][j].OrdersReportDealer.ScripCode) continue;
                                RPos = j;
                                break;
                            }
                            if (RPos == -1)//impossible condition
                            {
                                RPos = RCnt;
                                //RCnt++;
                                //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                            }
                        }
                        if (RType == 'S')   //SL Triggering  
                        {
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.SLTriggered = 'Y';
                        }
                        else if (RType == 'L')//Mkt To Limit
                        {
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.AtMarket = 'N';
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        }
                        else if (RType == 'P')
                        {
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripName.TrimEnd().Length);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.SLTriggered = 'N';
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.StatusLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Status.TrimEnd().Length);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                            if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                            {
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.IsSpread = true;
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                            }
                            else
                            {
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.IsSpread = false;
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode2 = 0;
                            }
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.RejectReason = dt.Rows[i]["REASON"].ToString().ToCharArray();
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.OrderRequestorCode = dt.Rows[i]["ORDERREQUESTERCODE"].ToString();
                        }
                        else if (RType == 'M')
                        {
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.SLTriggered = 'N';
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.StatusLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Status.TrimEnd().Length);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripName.TrimEnd().Length);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                            if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                            {
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.IsSpread = true;
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                            }
                            else
                            {
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.IsSpread = false;
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode2 = 0;
                            }
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.RejectReason = dt.Rows[i]["REASON"].ToString().ToCharArray();
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.OrderRequestorCode = dt.Rows[i]["ORDERREQUESTERCODE"].ToString();
                        }
                        else if (RType == 'C' || RType == 'K')
                        {
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.SLTriggered = 'N';
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.StatusLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Status.TrimEnd().Length);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripName.TrimEnd().Length);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                            if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                            {
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.IsSpread = true;
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                            }
                            else
                            {
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.IsSpread = false;
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ScripCode2 = 0;
                            }
                            try
                            {
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.RejectReason = dt.Rows[i]["REASON"].ToString().ToCharArray();
                                OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.OrderRequestorCode = dt.Rows[i]["ORDERREQUESTERCODE"].ToString();
                            }
                            catch (Exception)
                            {

                                //throw;
                            }
                        }
                        if(RType != 'L' && RType != 'S')
                            OTSReplyRec.AllReplyNew[k][RCnt].OrdersReportDealer.ClientCode = dt.Rows[i]["Clientcode"].ToString();
                        RCnt++;
                        Reach = RCnt;
                    }

                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = OTSReplyRec.ReplyHdr.ReplyType;

                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeReportsReplyRecDealer());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;// dt.Rows.Count;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception et)
            {
                OTSReplyRec.ErrorString = et.Message;
                Form1.ErrorLog(et);
            }
        }

        private void ProcessDealerTrdaesReport(DataTable dt1)
        {
            try
            {
                int Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0 ,LotSize = 0 ,Qty = 0;
                DataTable dt;
                string ScName;

                dt = dt1;

                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dt.Rows.Count;
                while (Reply > 5000)
                {
                    Reply = Reply - 5000;
                    cnt = cnt + 5000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                //Array.Resize(ref  OTSReplyRec.AllReplyNew, rows);
                //Array.Resize(ref OTSReplyRec.NewReplyHdr, rows);

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = dt.Rows.Count;

                TotalRecords = dt.Rows.Count;
                Reach = 0;
                for (int k = 0; k < rows; k++)
                {
                    TotalRecords = (dt.Rows.Count) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(5000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (int i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ClientCode = Convert.ToString(dt.Rows[i]["Clientcode"]).TrimEnd();
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).ToCharArray();
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ScripNameLength = Convert.ToByte(new string(OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ScripName).TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                        if (OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.Exch == 'C') //Qty is received in currency Trade report. Converting in LOT.
                        {
                            ScName = OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ScripName.ToString().Trim().Substring(0, 6);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                            Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            dt.Rows[i]["QTY"] = (Qty / LotSize);
                        }
                        else if (OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.Exch == 'E')//bseCurr
                        {
                            ScName = OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ScripName.ToString().Trim().Substring(0, 6);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                            Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            dt.Rows[i]["QTY"] = (Qty / LotSize);
                        }
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHANGEORDERID"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ExchangeTradeID = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADEID"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.ExchangeTradeTime = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADETIME"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].TradesReportDealer.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                        RCnt++;
                    }

                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = OTSReplyRec.ReplyHdr.ReplyType;

                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TTradeReportReplyRecDealer());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;// dt.Rows.Count;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception et)
            {
                OTSReplyRec.ErrorString = et.Message;
                Form1.ErrorLog(et);
            }
        }

        private void ProcessScripMarginReply(string ReplyStr)
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



        private void ProcessScripMarginReplyNew(string ReplyStr)
        {
            try
            {
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 7 && VList.Count() != 5)//improper string
                {
                    ReplyStr = "1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                OTSReplyRec.ReplyHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TScripMarginReplyRecord_New());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TScripMarginReplyRecord_New());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.IntraScCate = Convert.ToChar(VList[0]);
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.ExchType = Convert.ToChar(VList[1]);
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.Exch = Convert.ToChar(VList[2]);
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.ScCode = Convert.ToInt32(VList[3]);
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.IntraMargin = Convert.ToSingle(VList[4]);
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.DelScCate = Convert.ToChar(VList[5]);
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.DelMargin = Convert.ToSingle(VList[6]);
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.Reserved = 0;
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.Reserved1 = 0;
                OTSReplyRec.AllReply[0].NewScripMarginReplyRec.Reserve2 = 0;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessScripInfoReply(string ReplyStr)
        {
            try
            {
                VList = ReplyStr.Split(splitchar);
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
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved1 = 0;
                OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved2 = 0;   
                if (OTSReplyRec.AllReply[0].ScripInfoReplyRec.Exch == 'M')
                {
                    OTSReplyRec.AllReply[0].ScripInfoReplyRec.IntraVarMgn = 0;  //20.05.15
                    OTSReplyRec.AllReply[0].ScripInfoReplyRec.DelVarmgn = Convert.ToSingle(VList[20]);  //20.05.15 
                    OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved3 = Convert.ToSingle(VList[21]);
                }
                else
                {
                    OTSReplyRec.AllReply[0].ScripInfoReplyRec.IntraVarMgn = 0;// Convert.ToSingle(VList[20]);  //20.05.15
                    OTSReplyRec.AllReply[0].ScripInfoReplyRec.DelVarmgn = 0;// Convert.ToSingle(VList[21]);  //20.05.15 
                    if(VList.Length == 21)
                        OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved2 = Convert.ToInt32(VList[20]);
                    OTSReplyRec.AllReply[0].ScripInfoReplyRec.Reserved3 = 0.00f;
                }                           
            }
            catch (Exception e)
            {
                Form1.ErrorLog(e);
            }
        }

        private void ProcessHoldingsReport(DataTable dt)
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

        // 99
        private void ProcessVerifyUserReply(string ReplyStr)
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
                OTSReplyRec.AllReply[0].VerifyUserReplyRec.Verified = Convert.ToByte(ReplyStr);
                //OTSReplyRec.AllReply[0].VerifyUserReplyRec.ReplyType = 99;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessActivityLog(DataTable dt)
        {
            try
            {
                char ReqType;
                int LotSize, Qty;
                string ScName;
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
                        if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //24.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                        dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                        dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                        dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                    }
                    else if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'E') //bseCurr
                    {
                        ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                        if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //24.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                        }
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

        private void ProcessOrderTypeActivityLog(DataTable dt)
        {
             try
            {
            char ReqType;
            int LotSize, Qty;
            string ScName;
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TActivityLogReplyRecordNew());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'C') //Qty is received in currency Trade report. Converting in LOT.
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                }
                else if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'E') //bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                }

                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.ScripName.TrimEnd().Length);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.OldOrderQty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.TradedQty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.PendingQty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.SLTriggered = 'N';
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.Status.TrimEnd().Length);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));

                ReqType = Convert.ToChar(dt.Rows[i]["RequestType"]);
                if (ReqType == 'S') //SL Triggering
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.SLTriggered = 'Y';
                else
                    if (ReqType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.AtMarket = 'N';
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
            }
            }
             catch (Exception ex)
             {
                 Form1.ErrorLog(ex);
             }
        }

        private void ProcessOrderTypeNewActivityLogNew(DataTable dt) // 15.10.16 Hetal
        {
            try
            {
            char ReqType;
            int LotSize, Qty;
            string ScName;
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TActivityLogReplyRecordNewForRequestor());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'C') //Qty is received in currency Trade report. Converting in LOT.
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                }
                else if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'E') //bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                }

                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripName.TrimEnd().Length);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.OldOrderQty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.TradedQty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.PendingQty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.SLTriggered = 'N';
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Status.TrimEnd().Length);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                //23.09.16
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.RequestorCode = Convert.ToString(dt.Rows[i]["OrderRequesterCode"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.RequestorCodeLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.RequestorCode.TrimEnd().Length);

                //if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')//By Sandeep
                //{
                //    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.IsSpread = true;
                //    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripCode2 = Convert.ToInt32(dt.Rows[i]["SCRIPCODE2"]);
                //}
                //else
                //{
                //    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.IsSpread = false;
                //    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripCode2 = 0;
                //}

                ReqType = Convert.ToChar(dt.Rows[i]["RequestType"]);
                if (ReqType == 'S') //SL Triggering
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.SLTriggered = 'Y';
                else
                    if (ReqType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.AtMarket = 'N';
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        private void ProcessOrderTypeNewActivityLogNew_single(DataTable dt)
        {
            try
            {
                char ReqType;
                int LotSize, Qty;
                string ScName;
                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TActivityLogReplyRecordNewForRequestor());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'C') //Qty is received in currency Trade report. Converting in LOT.
                    {
                        ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                        if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                        dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                        dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                        dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                    }
                    else if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'E') //bseCurr
                    {
                        ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                        if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                        dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                        dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                        Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                        dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                    }

                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripName.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.OldOrderQty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.TradedQty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.PendingQty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.SLTriggered = 'N';
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Status.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                    //23.09.16
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.RequestorCode = Convert.ToString(dt.Rows[i]["OrderRequesterCode"]);
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.RequestorCodeLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.RequestorCode.TrimEnd().Length);

                    //if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')//By Sandeep
                    //{
                    //    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.IsSpread = true;
                    //    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripCode2 = Convert.ToInt32(dt.Rows[i]["SCRIPCODE2"]);
                    //}
                    //else
                    //{
                    //    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.IsSpread = false;
                    //    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.ScripCode2 = 0;
                    //}

                    ReqType = Convert.ToChar(dt.Rows[i]["RequestType"]);
                    if (ReqType == 'S') //SL Triggering
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.SLTriggered = 'Y';
                    else
                        if (ReqType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.AtMarket = 'N';
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestor.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        private void ProcessOrderTypeActivityLogNew(DataTable dt) // Added on 18-Apr-2018 - Rohan
        {
            try
            {
            char ReqType;
            int LotSize, Qty;
            string ScName;
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TActivityLogReplyRecordNewForRequestorNew());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'C') //Qty is received in currency Trade report. Converting in LOT.
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                }
                else if (Convert.ToChar(dt.Rows[i]["EXCH"]) == 'E') //bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                    dt.Rows[i]["OLDORDERQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                    dt.Rows[i]["TRADEDQTY"] = (Qty / LotSize);

                    Qty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                    dt.Rows[i]["PENDINGQTY"] = (Qty / LotSize);
                }

                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ScripName.TrimEnd().Length);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.OldOrderQty = Convert.ToInt32(dt.Rows[i]["OLDORDERQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.TradedQty = Convert.ToInt32(dt.Rows[i]["TRADEDQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.PendingQty = Convert.ToInt32(dt.Rows[i]["PENDINGQTY"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.SLTriggered = 'N';
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.Status.TrimEnd().Length);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                //23.09.16
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.RequestorCode = Convert.ToString(dt.Rows[i]["OrderRequesterCode"]);
                OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.RequestorCodeLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.RequestorCode.TrimEnd().Length);

                if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')//By Sandeep
                {
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.IsSpread = true;
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ScripCode2 = Convert.ToInt32(dt.Rows[i]["SCRIPCODE2"]);
                }
                else
                {
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.IsSpread = false;
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.ScripCode2 = 0;
                }

                ReqType = Convert.ToChar(dt.Rows[i]["RequestType"]);
                if (ReqType == 'S') //SL Triggering
                    OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.SLTriggered = 'Y';
                else
                    if (ReqType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.AtMarket = 'N';
                        OTSReplyRec.AllReply[i].OrderTypeActivityLogReplyRecForRequestorNew.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessTemplateMarginReportMTF(DataTable dt)  //forMTF Margin by himanshu
        {
            try
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMarginReportReplyRecordTemplate());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                if (dt.Rows.Count <= 0) return;

                OTSReplyRec.AllReply[0].TemplateMarginReply.OpeningMargin = Convert.ToDouble(dt.Rows[0]["LedgerBalance"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.Adhoc = Convert.ToDouble(dt.Rows[0]["Adhoc"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.Payments = Convert.ToDouble(dt.Rows[0]["Payments"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.HoldingCFS = Convert.ToDouble(dt.Rows[0]["HoldingCFS"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.OptionCFS = Convert.ToDouble(dt.Rows[0]["OptionCFS"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.MBPO = Convert.ToDouble(dt.Rows[0]["MBPO"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.MarginForOpenPos = Convert.ToDouble(dt.Rows[0]["Mgn4Position"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.AvailableMargin = Convert.ToDouble(dt.Rows[0]["AVAILABLEMARGIN"]);
                if (dt.Columns.Contains("NetOptionPremium"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply.Junk1 = Convert.ToDouble(dt.Rows[0]["NetOptionPremium"]);
                if (dt.Columns.Contains("AvailableMarginForoption"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply.Junk2 = Convert.ToDouble(dt.Rows[0]["AvailableMarginForoption"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.Junk3 = OTSReqRec.AllReq.TemplateMargin.SegmentId;   // add 21.09.2022
                //Siddhesh with Sumit Added Fund Web Margin
                if (dt.Columns.Contains("FundWeb"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply.Junk4 = (float)(Convert.ToDouble(dt.Rows[0]["FundWeb"]));
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessTemplateMarginReport(DataTable dt)
        {
            try
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMarginReportReplyRecordTemplate());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                if (dt.Rows.Count <= 0) return;

                OTSReplyRec.AllReply[0].TemplateMarginReply.OpeningMargin = Convert.ToDouble(dt.Rows[0]["LedgerBalance"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.Adhoc = Convert.ToDouble(dt.Rows[0]["Adhoc"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.Payments = Convert.ToDouble(dt.Rows[0]["Payments"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.HoldingCFS = Convert.ToDouble(dt.Rows[0]["HoldingCFS"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.OptionCFS = Convert.ToDouble(dt.Rows[0]["OptionCFS"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.MBPO = Convert.ToDouble(dt.Rows[0]["MBPO"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.MarginForOpenPos = Convert.ToDouble(dt.Rows[0]["Mgn4Position"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.AvailableMargin = Convert.ToDouble(dt.Rows[0]["AVAILABLEMARGIN"]);
                if (dt.Columns.Contains("NetOptionPremium"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply.Junk1 = Convert.ToDouble(dt.Rows[0]["NetOptionPremium"]);
                if (dt.Columns.Contains("AvailableMarginForoption"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply.Junk2 = Convert.ToDouble(dt.Rows[0]["AvailableMarginForoption"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply.Junk3 = OTSReqRec.AllReq.TemplateMargin.SegmentId;   // add 21.09.2022
                //Siddhesh with Sumit Added Fund Web Margin
                if (dt.Columns.Contains("FundWeb"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply.Junk4 = (float)(Convert.ToDouble(dt.Rows[0]["FundWeb"]));
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessTemplateMarginReport_Peak(DataTable dt)
        {
            try
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMarginReportReplyRecordTemplate_Peak());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                if (dt.Rows.Count <= 0) return;

                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.OpeningMargin = Convert.ToDouble(dt.Rows[0]["LedgerBalance"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.Adhoc = Convert.ToDouble(dt.Rows[0]["Adhoc"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.Payments = Convert.ToDouble(dt.Rows[0]["Payments"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.HoldingCFS = Convert.ToDouble(dt.Rows[0]["HoldingCFS"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.OptionCFS = Convert.ToDouble(dt.Rows[0]["OptionCFS"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.MBPO = Convert.ToDouble(dt.Rows[0]["MBPO"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.MarginForOpenPos = Convert.ToDouble(dt.Rows[0]["Mgn4Position"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.AvailableMargin = Convert.ToDouble(dt.Rows[0]["AVAILABLEMARGIN"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.PeakMargin = Convert.ToDouble(dt.Rows[0]["PeakMargin"]);
                if (dt.Columns.Contains("NetOptionPremium"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.Junk1 = Convert.ToDouble(dt.Rows[0]["NetOptionPremium"]);
                if (dt.Columns.Contains("AvailableMarginForoption"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.Junk2 = Convert.ToDouble(dt.Rows[0]["AvailableMarginForoption"]);
                OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.Junk3 = OTSReqRec.AllReq.TemplateMargin.SegmentId;   // add 21.09.2022
                //Siddhesh with Sumit Added Fund Web Margin
                if (dt.Columns.Contains("FundWeb"))
                    OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.Junk4 = (float)(Convert.ToDouble(dt.Rows[0]["FundWeb"]));
                if (dt.Columns.Contains("AllPortFolioMgn")) //20.10.22
                    OTSReplyRec.AllReply[0].TemplateMarginReply_Peak.DerivativeMargin = Convert.ToDouble(dt.Rows[0]["AllPortFolioMgn"]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        #region Derivative Margin 20.10.22
        private void ProcessTemplateMarginReportNew(DataTable dt)
        {
            try
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMarginReportReplyRecordTemplateWithDerivMargin());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                if (dt.Rows.Count <= 0) return;

                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.OpeningMargin = Convert.ToDouble(dt.Rows[0]["LedgerBalance"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.Adhoc = Convert.ToDouble(dt.Rows[0]["Adhoc"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.Payments = Convert.ToDouble(dt.Rows[0]["Payments"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.HoldingCFS = Convert.ToDouble(dt.Rows[0]["HoldingCFS"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.OptionCFS = Convert.ToDouble(dt.Rows[0]["OptionCFS"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.MBPO = Convert.ToDouble(dt.Rows[0]["MBPO"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.MarginForOpenPos = Convert.ToDouble(dt.Rows[0]["Mgn4Position"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.AvailableMargin = Convert.ToDouble(dt.Rows[0]["AVAILABLEMARGIN"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.PeakMargin = Convert.ToDouble(dt.Rows[0]["PeakMargin"]);
                if (dt.Columns.Contains("NetOptionPremium"))
                    OTSReplyRec.AllReply[0].TemplateWithDerivMargin.Junk1 = Convert.ToDouble(dt.Rows[0]["NetOptionPremium"]);
                if (dt.Columns.Contains("AvailableMarginForoption"))
                    OTSReplyRec.AllReply[0].TemplateWithDerivMargin.Junk2 = Convert.ToDouble(dt.Rows[0]["AvailableMarginForoption"]);
                OTSReplyRec.AllReply[0].TemplateWithDerivMargin.Junk3 = OTSReqRec.AllReq.TemplateMargin.SegmentId;   // add 21.09.2022
                //Siddhesh with Sumit Added Fund Web Margin
                if (dt.Columns.Contains("FundWeb"))
                    OTSReplyRec.AllReply[0].TemplateWithDerivMargin.Junk4 = (float)(Convert.ToDouble(dt.Rows[0]["FundWeb"]));
                if (dt.Columns.Contains("AllPortFolioMgn")) //20.10.22
                    OTSReplyRec.AllReply[0].TemplateWithDerivMargin.DerivativeMargin = Convert.ToDouble(dt.Rows[0]["AllPortFolioMgn"]);
                //Siddhesh 09.10.23 Added MgnUtilizedMTF
                if (dt.Columns.Contains("MgnUtilizedMTF"))
                    OTSReplyRec.AllReply[0].TemplateWithDerivMargin.Reserved1 = Convert.ToDouble(dt.Rows[0]["MgnUtilizedMTF"]);
                //OTSReplyRec.AllReply[0].TemplateWithDerivMargin.Reserved1 = 100.50;

            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        #endregion

        private void ProcessSegmentMaster(DataTable dt)
        {
            try{
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
            OTSReplyRec.AllReply[0].SegmentReply.Reserved1 = Convert.ToChar(dt.Rows[5]["Allow"]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessMarginReportCombined(DataTable dt)
        {
            try
            {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMarginReportReplyRecordCombined());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            if (dt.Rows.Count <= 0) return;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.OpeningMargin = 0.00f;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.AdditionalMargin = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MarginForOpenPos = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MarginForPendOrder = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.AvailableMargin = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.LedgerBalance = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.T1Short = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.T2Short = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Withdrawable = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MFdebit = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOBalance = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOMgnPayable = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOExcessCol = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.HoldingsMargin = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Junk1 = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Junk2 = 0.00;

            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.OpeningMargin = 0.00f;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.AdditionalMargin = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MarginForOpenPos = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MarginForPendOrder = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.AvailableMargin = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.LedgerBalance = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.T1Short = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.T2Short = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Withdrawable = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MFdebit = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOBalance = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOMgnPayable = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOExcessCol = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.HoldingsMargin = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Junk1 = 0.00;
            //OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Junk2 = 0.00;

            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.OpeningMargin = Convert.ToDouble(dt.Rows[0]["OpeningMargin"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.AdditionalMargin = Convert.ToDouble(dt.Rows[0]["Additional"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MarginForOpenPos = Convert.ToDouble(dt.Rows[0]["Mgn4Position"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MarginForPendOrder = Convert.ToDouble(dt.Rows[0]["Mgn4PendOrd"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.AvailableMargin = Convert.ToDouble(dt.Rows[0]["AvailableMargin"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.LedgerBalance = Convert.ToDouble(dt.Rows[0]["LedgerBalance"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.T1Short = Convert.ToDouble(dt.Rows[0]["T1Short"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.T2Short = Convert.ToDouble(dt.Rows[0]["T2Short"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Withdrawable = Convert.ToDouble(dt.Rows[0]["Withdrawable"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MFdebit = Convert.ToDouble(dt.Rows[0]["MaalFodoDebit"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOBalance = Convert.ToDouble(dt.Rows[0]["FOBalance"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOMgnPayable = Convert.ToDouble(dt.Rows[0]["FOMarginPayable"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOExcessCol = Convert.ToDouble(dt.Rows[0]["FOExcessCol"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.HoldingsMargin = Convert.ToDouble(dt.Rows[0]["PDHV"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Junk1 = Convert.ToDouble(dt.Rows[0]["FnOMTM"]);//01.02.15
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Junk2 = Convert.ToDouble(dt.Rows[0]["MBOP"]);//23.04.15 in Monarch on 20.03.15 09.02.2017

            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.OpeningMargin2 = Convert.ToDouble(dt.Rows[0]["OpeningMargin2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.AdditionalMargin2 = Convert.ToDouble(dt.Rows[0]["Additional2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MarginForOpenPos2 = Convert.ToDouble(dt.Rows[0]["Mgn4Position2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MarginForPendOrder2 = Convert.ToDouble(dt.Rows[0]["Mgn4PendOrd2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.AvailableMargin2 = Convert.ToDouble(dt.Rows[0]["AvailableMargin2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.LedgerBalance2 = Convert.ToDouble(dt.Rows[0]["LedgerBalance2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.T1Short2 = Convert.ToDouble(dt.Rows[0]["T1Short2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.T2Short2 = Convert.ToDouble(dt.Rows[0]["T2Short2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Withdrawable2 = Convert.ToDouble(dt.Rows[0]["Withdrawable2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.MFdebit2 = Convert.ToDouble(dt.Rows[0]["MaalFodoDebit2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOBalance2 = Convert.ToDouble(dt.Rows[0]["FOBalance2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOMgnPayable2 = Convert.ToDouble(dt.Rows[0]["FOMarginPayable2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.FOExcessCol2 = Convert.ToDouble(dt.Rows[0]["FOExcessCol2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.HoldingsMargin2 = Convert.ToDouble(dt.Rows[0]["PDHV2"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Junk3 = Convert.ToDouble(dt.Rows[0]["FnOMTM2"]);//01.02.15
            OTSReplyRec.AllReply[0].MarginReportReplyRecCombined.Junk4 = Convert.ToDouble(dt.Rows[0]["MBOP2"]);//23.04.15 in Monarch on 20.03.15
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
            }

        private void ProcessMarginReportMCX(DataTable dt)
        {
            try
            {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMarginReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.OpeningMargin = 0.00f;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.AdditionalMargin = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.MarginForOpenPos = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.MarginForPendOrder = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.AvailableMargin = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.LedgerBalance = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.T1Short = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.T2Short = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.Withdrawable = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.MFdebit = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.FOBalance = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.FOMgnPayable = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.FOExcessCol = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.HoldingsMargin = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.Junk1 = 0.00;
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.Junk2 = 0.00;
            if (dt.Rows.Count <= 0) return;

            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.OpeningMargin = Convert.ToDouble(dt.Rows[0]["OpeningMargin"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.AdditionalMargin = Convert.ToDouble(dt.Rows[0]["Additional"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.MarginForOpenPos = Convert.ToDouble(dt.Rows[0]["Mgn4Position"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.MarginForPendOrder = Convert.ToDouble(dt.Rows[0]["Mgn4PendOrd"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.AvailableMargin = Convert.ToDouble(dt.Rows[0]["AvailableMargin"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.LedgerBalance = Convert.ToDouble(dt.Rows[0]["LedgerBalance"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.T1Short = Convert.ToDouble(dt.Rows[0]["T1Short"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.T2Short = Convert.ToDouble(dt.Rows[0]["T2Short"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.Withdrawable = Convert.ToDouble(dt.Rows[0]["Withdrawable"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.MFdebit = Convert.ToDouble(dt.Rows[0]["MaalFodoDebit"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.FOBalance = Convert.ToDouble(dt.Rows[0]["FOBalance"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.FOMgnPayable = Convert.ToDouble(dt.Rows[0]["FOMarginPayable"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.FOExcessCol = Convert.ToDouble(dt.Rows[0]["FOExcessCol"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.HoldingsMargin = Convert.ToDouble(dt.Rows[0]["PDHV"]);
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.Junk1 = Convert.ToDouble(dt.Rows[0]["FnOMTM"]);//01.02.15
            OTSReplyRec.AllReply[0].MarginReportReplyRecMCX.Junk2 = Convert.ToDouble(dt.Rows[0]["MBOP"]);//23.04.15 in Monarch on 20.03.15
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessMarginReport(DataTable dt)
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
            if (dt.Rows.Count <= 0) return;

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
            OTSReplyRec.AllReply[0].MarginReportReplyRec.Junk1 = Convert.ToDouble(dt.Rows[0]["FnOMTM"]);//01.02.15
            OTSReplyRec.AllReply[0].MarginReportReplyRec.Junk2 = Convert.ToDouble(dt.Rows[0]["MBOP"]);//23.04.15 in Monarch on 20.03.15 09.02.2017
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        #region MTM - Added on 12-Sep-2016 - Rohan
        private void ProcessAllClientMarginReport(DataTable dt)
        {
            try
            {
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TAllClientMarginReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);


            OTSReplyRec.AllReply[0].AllClientMargin.FEUserID = dt.Rows[0]["ClientCode"].ToString().PadRight(15).ToCharArray();
            OTSReplyRec.AllReply[0].AllClientMargin.FEUserIDLength = (byte)dt.Rows[0]["ClientCode"].ToString().Trim().Length;
            OTSReplyRec.AllReply[0].AllClientMargin.OpeningMargin = Convert.ToDouble(dt.Rows[0]["OpeningMargin"]);
            OTSReplyRec.AllReply[0].AllClientMargin.AdditionalMargin = Convert.ToDouble(dt.Rows[0]["Additional"]);
            OTSReplyRec.AllReply[0].AllClientMargin.MarginForOpenPos = Convert.ToDouble(dt.Rows[0]["Mgn4Position"]);
            OTSReplyRec.AllReply[0].AllClientMargin.MarginForPendOrder = Convert.ToDouble(dt.Rows[0]["Mgn4PendOrd"]);
            OTSReplyRec.AllReply[0].AllClientMargin.AvailableMargin = Convert.ToDouble(dt.Rows[0]["AvailableMargin"]);
            OTSReplyRec.AllReply[0].AllClientMargin.LedgerBalance = Convert.ToDouble(dt.Rows[0]["LedgerBalance"]);
            OTSReplyRec.AllReply[0].AllClientMargin.Adhoc = Convert.ToDouble(dt.Rows[0]["Adhoc"]);
            OTSReplyRec.AllReply[0].AllClientMargin.Receipts = Convert.ToDouble(dt.Rows[0]["Receipts"]);
            OTSReplyRec.AllReply[0].AllClientMargin.Payments = Convert.ToDouble(dt.Rows[0]["Payments"]);
            OTSReplyRec.AllReply[0].AllClientMargin.HoldingMargin = Convert.ToDouble(dt.Rows[0]["PDHV"]);
            OTSReplyRec.AllReply[0].AllClientMargin.Reserved = 0.00;
            OTSReplyRec.AllReply[0].AllClientMargin.Reserved1 = 0.00;
            OTSReplyRec.AllReply[0].AllClientMargin.Reserved2 = 0.00;
            OTSReplyRec.AllReply[0].AllClientMargin.Reserved3 = 0.00;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessClientsForMTMDetails(DataTable dt)
        {
            try{
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.ClientsForMTMSquareOff());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].ClientsforMTMReply.FEUserID = dt.Rows[i]["ClientCode"].ToString().PadRight(15).ToCharArray();
                OTSReplyRec.AllReply[i].ClientsforMTMReply.FELength = (byte)dt.Rows[i]["ClientCode"].ToString().Trim().Length;
                if (Convert.ToString(dt.Rows[i]["AdhocForMTM"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].ClientsforMTMReply.Adhoc = 0;
                else OTSReplyRec.AllReply[i].ClientsforMTMReply.Adhoc = 1;
                if (Convert.ToString(dt.Rows[i]["RecieptsForMTM"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].ClientsforMTMReply.Receipts = 0;
                else OTSReplyRec.AllReply[i].ClientsforMTMReply.Receipts = 1;
                if (Convert.ToString(dt.Rows[i]["PaymentsForMtm"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].ClientsforMTMReply.Payments = 0;
                else OTSReplyRec.AllReply[i].ClientsforMTMReply.Payments = 1;
                if (Convert.ToString(dt.Rows[i]["HoldingMarginForMTM"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].ClientsforMTMReply.HoldingMargin = 0;
                else OTSReplyRec.AllReply[i].ClientsforMTMReply.HoldingMargin = 1;
                if (Convert.ToString(dt.Rows[i]["LedgerForMTM"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].ClientsforMTMReply.LedgerBalance = 0;
                else OTSReplyRec.AllReply[i].ClientsforMTMReply.LedgerBalance = 1;
                if (Convert.ToString(dt.Rows[i]["SkipFromMTM"]).TrimEnd() == "Y")
                    OTSReplyRec.AllReply[i].ClientsforMTMReply.SkipFromMTM = 0;
                else OTSReplyRec.AllReply[i].ClientsforMTMReply.SkipFromMTM = 1;
                OTSReplyRec.AllReply[i].ClientsforMTMReply.MBOP = 0;
                OTSReplyRec.AllReply[i].ClientsforMTMReply.MBPO = 0;
                OTSReplyRec.AllReply[i].ClientsforMTMReply.Reserved = 0;
                OTSReplyRec.AllReply[i].ClientsforMTMReply.Reserved1 = 0;
                OTSReplyRec.AllReply[i].ClientsforMTMReply.Reserved2 = 0;
                OTSReplyRec.AllReply[i].ClientsforMTMReply.Reserved3 = 0;
                OTSReplyRec.AllReply[i].ClientsforMTMReply.Reserved4 = 0;

            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        #endregion

        private void ProcessOrderTypeTradeReport(DataTable dt)
        {
            try{
            string ScName;
            int LotSize, Qty;

            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeTradeReportReplyRec());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ScripName.TrimEnd().Length);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                if (OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.Exch == 'C') //Qty is received in currency Trade report. Converting in LOT.
                {
                    ScName = OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ScripName.Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.Exch == 'E') //bseCurr
                {
                    ScName = OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ScripName.Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHANGEORDERID"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ExchangeTradeID = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADEID"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.ExchangeTradeTime = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADETIME"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                //try
                //{
                //    if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                //        OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.IsSpread = true;
                //    else
                //        OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.IsSpread = false;
                //    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                //}
                //catch (Exception es)
                //{
                //    Console.WriteLine("Exception");
                //}
            }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeTradeReportNew(DataTable dt)   //Added on 18-Apr-2017 - Rohan
        {
            string ScName;
            int LotSize, Qty;

            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeTradeReportReplyRecNew());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ScripName.TrimEnd().Length);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                if (OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.Exch == 'C') //Qty is received in currency Trade report. Converting in LOT.
                {
                    ScName = OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ScripName.Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.Exch == 'E') //bseCurr
                {
                    ScName = OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ScripName.Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHANGEORDERID"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ExchangeTradeID = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADEID"]);
                OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.ExchangeTradeTime = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADETIME"]);
                try
                {
                    if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                        OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.IsSpread = true;
                    else
                        OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.IsSpread = false;
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNew.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                }
                catch (Exception es)
                {
                    Console.WriteLine("Exception");
                }
            }
        }
        private void ProcessOrderTypeTradeReportNewCover(DataTable dt)   // // sumitp 28_jan_22
        {
            try
            {
                string ScName;
                int LotSize, Qty;

                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeTradeReportReplyRecNewCover());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ScripName.TrimEnd().Length);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                    if (OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.Exch == 'C') //Qty is received in currency Trade report. Converting in LOT.
                    {
                        ScName = OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ScripName.Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                        if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }
                    else if (OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.Exch == 'E') //bseCurr
                    {
                        ScName = OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ScripName.Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                        if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHANGEORDERID"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ExchangeTradeID = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADEID"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.ExchangeTradeTime = Convert.ToInt32(dt.Rows[i]["EXCHANGETRADETIME"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.OrderTypeNew = ReturnNewOrderType(Convert.ToString(dt.Rows[i]["OrderType"]));
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.EpochTime = Convert.ToInt32(dt.Rows[i]["Identifier"]);
                    OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                    //try
                    //{
                    //    if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                    //        OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.IsSpread = true;
                    //    else
                    //        OTSReplyRec.AllReply[i].OrderTypeTradeReportReplyRecNewCover.IsSpread = false;
                    //}
                    //catch (Exception es)
                    //{
                    //    Console.WriteLine("Exception");
                    //}
                }
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        private void ProcessOrderTypeTradeReportNew142(DataTable dt)   //Added new function on 05-Apr-2018 - Rohan
        {
            try
            {
                int dtRowsCount = dt.Rows.Count, Reply, cnt = 0, rows = 0, Reach = 0, TotalRecords = 0, Records = 0, rem = 0, RCnt = 0, k, i, LotSize, Qty;
                string ScName;
                DataTable dt1 = dt;

                if (dtRowsCount < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }

                Reply = dtRowsCount;
                while (Reply > 1000)
                {
                    Reply = Reply - 1000;
                    cnt = cnt + 1000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = dtRowsCount;

                TotalRecords = dtRowsCount;
                Reach = 0;

                for (k = 0; k < rows; k++)
                {
                    TotalRecords = (dtRowsCount) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(1000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    RCnt = 0;
                    for (i = Reach; i < rem; i++)
                    {
                        Reach++;

                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.Exch = Convert.ToChar(dt1.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ExchType = Convert.ToChar(dt1.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ScripCode = Convert.ToInt32(dt1.Rows[i]["SCRIPCODE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ScripName = Convert.ToString(dt1.Rows[i]["SCRIPNAME"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ScripName.TrimEnd().Length);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.BuySell = Convert.ToChar(dt1.Rows[i]["BUYSELL"]);
                        if (OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.Exch == 'C') //Qty is received in currency Trade report. Converting in LOT.
                        {
                            ScName = OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ScripName.Substring(0, 6);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScName]);
                            if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                            {
                                string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                                LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                            }
                            Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            dt1.Rows[i]["QTY"] = (Qty / LotSize);
                        }
                        else if (OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.Exch == 'E') //bseCurr
                        {
                            ScName = OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ScripName.Substring(0, 6);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                            if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                            {
                                string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                                LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                            }
                            Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            dt1.Rows[i]["QTY"] = (Qty / LotSize);
                        }
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.Qty = Convert.ToInt32(dt1.Rows[i]["QTY"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.Rate = Convert.ToSingle(dt1.Rows[i]["RATE"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ExchangeOrderID = Convert.ToInt64(dt1.Rows[i]["EXCHANGEORDERID"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ExchangeTradeID = Convert.ToInt32(dt1.Rows[i]["EXCHANGETRADEID"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.ExchangeTradeTime = Convert.ToInt32(dt1.Rows[i]["EXCHANGETRADETIME"]);
                        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt1.Rows[i]["IntraDelivery"]));
                        //try
                        //{
                        //    if (Convert.ToChar(dt1.Rows[i]["IsSpreadOrder"]) == 'Y')
                        //        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.IsSpread = true;
                        //    else
                        //        OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.IsSpread = false;
                        //    OTSReplyRec.AllReplyNew[k][RCnt].OrderTypeTradeReportReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt1.Rows[i]["IntraDelivery"]));
                        //}
                        //catch (Exception es)
                        //{
                        //    Console.WriteLine("Exception");
                        //}
                        RCnt++;
                    }

                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = 142;
                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeTradeReportReplyRec());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)RCnt;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception et)
            {
                OTSReplyRec.ErrorString = et.Message;
                Form1.ErrorLog(et);
            }
        }

        private void ProcessTradeReport(DataTable dt)
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
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //24.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (OTSReplyRec.AllReply[i].TradeReportReplyRec.Exch == 'E') //bseCurr
                {
                    ScName = OTSReplyRec.AllReply[i].TradeReportReplyRec.ScripName.Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //24.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
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

        private void ProcessOrderTypeOrdersReport(DataTable dt)
        {
            try{
            int RPos, RCnt, ScripCode, Qty, LotSize;
            char RType, Exch;
            string Str1, ScName;
            Int64 ExchOrderID;
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            Array.Resize(ref OTSReplyRec.AllReply, dt.Rows.Count);
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
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (Exch == 'E') //bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }

                if (RType == 'P') //Place
                {
                    RPos = RCnt;
                    RCnt++;
                    //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
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
                        if (ExchOrderID != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRec.ExchangeOrderID) continue;
                        if (Exch != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRec.Exch) continue;
                        if (ScripCode != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRec.ScripCode) continue;
                        RPos = j;
                        break;
                    }
                    if (RPos == -1)//impossible condition
                    {
                        RPos = RCnt;
                        RCnt++;
                        //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                    }
                }
                if (RType == 'S')   //SL Triggering  
                {
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'Y';
                }
                else
                    if (RType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = 'N';
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
                    else
                        if (RType == 'P')
                        {
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                            //if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                            //{
                            //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = true;
                            //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                            //}
                            //else
                            //{
                            //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = false;
                            //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = 0;
                            //}
                        }
                        else
                            if (RType == 'M')
                            {
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                                //if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                                //{
                                //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = true;
                                //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                                //}
                                //else
                                //{
                                //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = false;
                                //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = 0;
                                //}
                            }
                            else
                                if (RType == 'C' || RType == 'K')
                                {
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                                    //if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                                    //{
                                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = true;
                                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                                    //}
                                    //else
                                    //{
                                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = false;
                                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = 0;
                                    //}
                                }
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeReportsReplyRec());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, RCnt);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeOrdersReportNew(DataTable dt)  //Added on 18-Apr-2017 - Rohan
        {
            try{
            int RPos, RCnt, ScripCode, Qty, LotSize;
            char RType, Exch;
            string Str1, ScName;
            Int64 ExchOrderID;
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            Array.Resize(ref OTSReplyRec.AllReply,dt.Rows.Count);
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
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (Exch == 'E') //bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }

                if (RType == 'P') //Place
                {
                    RPos = RCnt;
                    RCnt++;
                    //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
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
                        if (ExchOrderID != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRecNew.ExchangeOrderID) continue;
                        if (Exch != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRecNew.Exch) continue;
                        if (ScripCode != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRecNew.ScripCode) continue;
                        RPos = j;
                        break;
                    }
                    if (RPos == -1)//impossible condition
                    {
                        RPos = RCnt;
                        RCnt++;
                        //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                    }
                }
                if (RType == 'S')   //SL Triggering  
                {
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.SLTriggered = 'Y';
                }
                else
                    if (RType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.AtMarket = 'N';
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
                    else
                        if (RType == 'P')
                        {
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripName.TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.SLTriggered = 'N';
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Status.TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                            if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                            {
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.IsSpread = true;
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                            }
                            else
                            {
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.IsSpread = false;
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode2 = 0;
                            }
                        }
                        else
                            if (RType == 'M')
                            {
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.SLTriggered = 'N';
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Status.TrimEnd().Length);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripName.TrimEnd().Length);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                                OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                                if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                                {
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.IsSpread = true;
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                                }
                                else
                                {
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.IsSpread = false;
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode2 = 0;
                                }
                            }
                            else
                                if (RType == 'C' || RType == 'K')
                                {
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.SLTriggered = 'N';
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Status.TrimEnd().Length);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripName.TrimEnd().Length);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                                    if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                                    {
                                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.IsSpread = true;
                                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                                    }
                                    else
                                    {
                                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.IsSpread = false;
                                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNew.ScripCode2 = 0;
                                    }
                                }
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeReportsReplyRecNew());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, RCnt);
           }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeOrdersReportNew_CoverOrBracket(DataTable dt)  //Added on 25-Nov-2021 - Himanshu
        {
            try
            {
                int RPos, RCnt, ScripCode, Qty, LotSize;
                char RType, Exch;
                string Str1, ScName;
                Int64 ExchOrderID;
                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                Array.Resize(ref OTSReplyRec.AllReply, dt.Rows.Count);
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
                        if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }
                    else if (Exch == 'E') //bseCurr
                    {
                        ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                        if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }

                    if (RType == 'P') //Place
                    {
                        RPos = RCnt;
                        RCnt++;
                        //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
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
                            //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
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
                        //OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID1 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        //OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID2 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);

                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderTypeNew = ReturnNewOrderType(Convert.ToString(dt.Rows[i]["ORDERTYPE"]));
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));

                        if (OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread == true)
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread = true;
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                        }
                        else
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread = false;
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode2 = 0;
                        }
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
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.EpochTime = Convert.ToInt32(dt.Rows[i]["IDENTIFIER"]);
                        //OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID1 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        //OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.BrokerOrderID2 = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderTypeNew = ReturnNewOrderType(Convert.ToString(dt.Rows[i]["ORDERTYPE"]));
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                        if (OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread == true)
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread = true;
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                        }
                        else
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread = false;
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode2 = 0;
                        }
                        if (OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread == true)
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread = true;
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                        }
                        else
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread = false;
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode2 = 0;
                        }
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
                        OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                        if (OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread == true)
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread = true;
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                        }
                        else
                        {
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.IsSpread = false;
                            OTSReplyRec.AllReply[RPos].OrderReportReplyRecCoverOrBracket.ScripCode2 = 0;
                        }
                    }
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderReportReplyRecordCoverOrBracket());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, RCnt);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeOrdersReportNewWithRejOrder(DataTable dt)  //Added on 09-May-2019 - Ashish
        {
            try
            {
                int RPos, RCnt, ScripCode, Qty, LotSize;
                char RType, Exch;
                string Str1, ScName;
                Int64 ExchOrderID;
                if (dt.Rows.Count < 1)
                {
                    OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                    OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                    OTSReplyRec.ReplyHdr.ReplyLen = 0;
                    OTSReplyRec.AllReply = null;
                    return;
                }
                Array.Resize(ref OTSReplyRec.AllReply, dt.Rows.Count);
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
                        if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }
                    else if (Exch == 'E') //bseCurr
                    {
                        ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                        if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                        {
                            string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                            LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                        }
                        Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        dt.Rows[i]["QTY"] = (Qty / LotSize);
                    }

                    if (RType == 'P') //Place
                    {
                        RPos = RCnt;
                        RCnt++;
                        //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
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
                            if (ExchOrderID != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchangeOrderID) continue;
                            if (Exch != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRecNewWithRejectOrder.Exch) continue;
                            if (ScripCode != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode) continue;
                            RPos = j;
                            break;
                        }
                        if (RPos == -1)//impossible condition
                        {
                            RPos = RCnt;
                            RCnt++;
                            //Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                        }
                    }
                    if (RType == 'S')   //SL Triggering  
                    {
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.SLTriggered = 'Y';
                    }
                    else
                        if (RType == 'L')//Mkt To Limit
                    {
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.AtMarket = 'N';
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    }
                    else
                            if (RType == 'P')
                    {
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripName.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.SLTriggered = 'N';
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Status.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                        if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                        {
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.IsSpread = true;
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                        }
                        else
                        {
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.IsSpread = false;
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode2 = 0;
                        }
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.RejectReason = dt.Rows[i]["REASON"].ToString().ToCharArray();
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.OrderRequestorCode = dt.Rows[i]["ORDERREQUESTERCODE"].ToString();
                    }
                    else
                                if (RType == 'M')
                    {
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.SLTriggered = 'N';
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Status.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripName.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                        if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                        {
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.IsSpread = true;
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                        }
                        else
                        {
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.IsSpread = false;
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode2 = 0;
                        }
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.RejectReason = dt.Rows[i]["REASON"].ToString().ToCharArray();
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.OrderRequestorCode = dt.Rows[i]["ORDERREQUESTERCODE"].ToString();
                    }
                    else
                                    if (RType == 'C' || RType == 'K')
                    {
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.SLTriggered = 'N';
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Status.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripName.TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                        if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                        {
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.IsSpread = true;
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                        }
                        else
                        {
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.IsSpread = false;
                            OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.ScripCode2 = 0;
                        }
                        try
                        {
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.RejectReason = dt.Rows[i]["REASON"].ToString().ToCharArray();
                        OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRecNewWithRejectOrder.OrderRequestorCode = dt.Rows[i]["ORDERREQUESTERCODE"].ToString();
                        }
                        catch (Exception)
                        {

                            //throw;
                        }
                    }
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeReportsReplyRecNewWithRejetOrder());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
                Array.Resize(ref OTSReplyRec.AllReply, RCnt);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderTypeOrdersReportNew141(DataTable dt)  //Added new function on 05-Apr-2018 - Rohan
        {
            try
            {
            int RPos, RCnt, ScripCode, Qty, LotSize, rows = 0, cnt = 0, Reply = 0;
            int Reach = 0, TotalRecords = 0, Records = 0, rem = 0, dtRowsCount, k, i, Rcnt1 = 0;
            char RType, Exch;
            string Str1, ScName;
            Int64 ExchOrderID;
            dtRowsCount = dt.Rows.Count;
            if (dtRowsCount < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            RCnt = 0;
            for (i = 0; i < dtRowsCount; i++)
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
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (Exch == 'E') //bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //31.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
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
                        if (ExchOrderID != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRec.ExchangeOrderID) continue;
                        if (Exch != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRec.Exch) continue;
                        if (ScripCode != OTSReplyRec.AllReply[j].OrdeTypeReportsReplyRec.ScripCode) continue;
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
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'Y';
                }
                else if (RType == 'L')//Mkt To Limit
                {
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = 'N';
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                }
                else if (RType == 'P')
                {
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                    //if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                    //{
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = true;
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                    //}
                    //else
                    //{
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = false;
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = 0;
                    //}
                }
                else if (RType == 'M')
                {
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                    //if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                    //{
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = true;
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                    //}
                    //else
                    //{
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = false;
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = 0;
                    //}
                }
                else if (RType == 'C' || RType == 'K')
                {
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                    //if (Convert.ToChar(dt.Rows[i]["IsSpreadOrder"]) == 'Y')
                    //{
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = true;
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = Convert.ToInt32(dt.Rows[i]["ScripCode2"]);
                    //}
                    //else
                    //{
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.IsSpread = false;
                    //    OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode2 = 0;
                    //}
                }
            }

            #region New Way of Processing Order Reports - Rohan - 05-Apr-2018
            try
            {
                Reply = RCnt;
                while (Reply > 1000)
                {
                    Reply = Reply - 1000;
                    cnt = cnt + 1000;
                    rows++;
                }
                if ((Reply) > 0)
                {
                    rows++;
                }

                OTSReplyRec.AllReplyNew = new ITInteractiveStructures.TAllReplyNewRecord[rows][];
                OTSReplyRec.NewReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord[rows];
                OTSReplyRec.ReportReplyCount = rows;
                OTSReplyRec.ReplyCount = RCnt;

                TotalRecords = RCnt;
                Reach = 0;
                for (k = 0; k < rows; k++)
                {
                    TotalRecords = (RCnt) - Reach;

                    if (TotalRecords < 0)
                        TotalRecords = TotalRecords * -1;
                    Records = Math.Min(1000, TotalRecords);

                    rem = rem + Records;
                    Array.Resize(ref  OTSReplyRec.AllReplyNew[k], Records);

                    Rcnt1 = 0;
                    for (i = Reach; i < rem; i++)
                    {
                        Reach++;
                        OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec;
                        #region Commented by Rohan
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.ExchangeOrderID = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.ExchangeOrderID;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.ExchangeOrderTime = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.ExchangeOrderTime;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.Qty = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.Qty;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.Rate = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.Rate;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.TriggerRate = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.TriggerRate;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.AtMarket = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.AtMarket;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.WithSL = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.WithSL;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.SLTriggered = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.SLTriggered;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.Status = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.Status;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.StatusLength = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.StatusLength;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.Exch = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.Exch;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.ExchType = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.ExchType;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.ScripCode = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.ScripCode;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.ScripName = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.ScripName;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.ScripNameLength = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.ScripNameLength;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.BuySell = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.BuySell;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.BrokerOrderID = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.BrokerOrderID;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.BrokerOrderTime = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.BrokerOrderTime;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.AHStatus = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.AHStatus;
                        //OTSReplyRec.AllReplyNew[k][Rcnt1].OrdeTypeReportsReplyRec.OrderType = OTSReplyRec.AllReply[i].OrdeTypeReportsReplyRec.OrderType;
                        #endregion
                        Rcnt1++;
                    }
                    OTSReplyRec.NewReplyHdr[k].AccountType = OTSReplyRec.ReplyHdr.AccountType;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_BC = OTSReplyRec.ReplyHdr.ChannelID_BC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_NC = OTSReplyRec.ReplyHdr.ChannelID_NC;
                    OTSReplyRec.NewReplyHdr[k].ChannelID_ND = OTSReplyRec.ReplyHdr.ChannelID_ND;
                    OTSReplyRec.NewReplyHdr[k].ClientType = OTSReplyRec.ReplyHdr.ClientType;
                    OTSReplyRec.NewReplyHdr[k].ErrorCode = OTSReplyRec.ReplyHdr.ErrorCode;
                    OTSReplyRec.NewReplyHdr[k].FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                    OTSReplyRec.NewReplyHdr[k].FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                    OTSReplyRec.NewReplyHdr[k].SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                    OTSReplyRec.NewReplyHdr[k].TraderID = OTSReplyRec.ReplyHdr.TraderID;
                    OTSReplyRec.NewReplyHdr[k].TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                    OTSReplyRec.NewReplyHdr[k].ReplyType = 141;

                    OTSReplyRec.NewReplyHdr[k].ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderTypeReportsReplyRec());
                    OTSReplyRec.NewReplyHdr[k].ReplyCnt = (ushort)Rcnt1;
                    OTSReplyRec.NewReplyHdr[k].ReplyLen = OTSReplyRec.NewReplyHdr[k].ReplyCnt * OTSReplyRec.NewReplyHdr[k].ReplyRecSize;
                }
            }
            catch (Exception et)
            {
                OTSReplyRec.ErrorString = et.Message;
                Form1.ErrorLog(et);
            }
            #endregion
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessOrderReport(DataTable dt)
        {
            try{
            int RPos, RCnt, ScripCode, Qty, LotSize;
            char RType, Exch;
            string Str1, ScName;
            Int64 ExchOrderID;
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
                    if (LotSize == 0 && frm.CurrencyLotToQtyMap.Count > 4) //24.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMap[ScNameForBond]);
                    }
                    Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    dt.Rows[i]["QTY"] = (Qty / LotSize);
                }
                else if (Exch == 'E') // bseCurr
                {
                    ScName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 6);
                    LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScName]);
                    if (LotSize == 0 && frm.CurrencyLotToQtyMapBSECURR.Count > 4) //24.1.15 For FUTIRC 
                    {
                        string ScNameForBond = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).Substring(0, 9);
                        LotSize = Convert.ToInt32(frm.CurrencyLotToQtyMapBSECURR[ScNameForBond]);
                    }
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

        private void ProcessIlliquidReport(DataTable dt)
        {
            try{
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

        private void ProcessLicUsedReport(DataTable dt)
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

        private void ProcessCliDbNumReport(DataTable dt)
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

        private void ProcessDerivBannedScripReport(DataTable dt)
        {
            try{
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

        private void ProcessBseNseMapReport(DataTable dt)
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

        private void ProcessDBDate(DataTable dt)
        {
            try{
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

        private void ProcessTradeToTradeReport(DataTable dt)
        {
            try{
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

        private void ProcessSurveillanceIndicatorMessages(DataTable dt)
        {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.SurveillanceIndicatorMessagesNse());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["ScripCode"]);
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec.ISIN = Convert.ToString(dt.Rows[i]["ISINNO"]).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec.ISINLength = (byte)new string(OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec.ISIN).TrimEnd().Length;
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec.GsmFlag = Convert.ToInt32(dt.Rows[i]["GSMFlag"]);
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec.SurveillanceMessages = Convert.ToString(dt.Rows[i]["SurveillanceIndicaterMassage"]);
            }
        }

        private void ProcessSurveillanceIndicatorMessagesBse(DataTable dt)
        {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.SurveillanceIndicatorMessagesBse());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["ScripCode"]);
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec.ISIN = Convert.ToString(dt.Rows[i]["ISINNO"]).PadRight(12).ToCharArray();
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec.ISINLength = (byte)new string(OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec.ISIN).TrimEnd().Length;
                if (dt.Rows[i]["GSMFlag"] != "")
                    OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec.GsmFlag = Convert.ToInt32(dt.Rows[i]["GSMFlag"]);
                else
                    OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec.GsmFlag = 0;
                OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec.SurveillanceMessages = Convert.ToString(dt.Rows[i]["SurveillanceIndicaterMassage"]);
            }
        }

        #region NSE SLBM
        private void ProcessSLBMPlaceOrderRequest(string ReplyStr)
        {
            int BOId, TimeNow;
            Single MBlk;
            char AHStatus1;
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 5 && VList.Count() != 6)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 6)
                AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TSLBMOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSLBMOrderPlaceBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.Exch = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.Exch;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.ExchType = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ExchType;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.ScripCode = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ScripName).PadRight(12).ToCharArray();
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.Series = new string(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.Series).PadRight(2).ToCharArray();
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ScripNameLength;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.ExpiryDate = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.ExpiryDate;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.BuySell = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.BuySell;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.Qty = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.Qty;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.AtMarket = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.Rate = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.Rate;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.WithSL = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.WithSL;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.IOC = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.IOC;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.DisclosedQty;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.AHStatus = AHStatus1;
            OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.RecallRepayOrder = new string(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec.RecallRepayOrder).ToCharArray();

        }

        private void ProcessSLBMModifyOrderRequest(string ReplyStr)
        {
            int BOId, TimeNow;
            Single MBlk;
            char AHStatus1;

            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 5 && VList.Count() != 6)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 6)
                AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TSLBMOrderModifyBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSLBMOrderModifyBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.Exch = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.Exch;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.ExchType = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ExchType;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.ScripCode = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ScripName).PadRight(12).ToCharArray();
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.Series = new string(OTSReqRec.AllReq.SLBMOrderModifyRequestRec.Series).PadRight(2).ToCharArray();
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ScripNameLength;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.ExpiryDate = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ExpiryDate;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.BuySell = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.BuySell;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.Qty = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.Qty;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.AtMarket = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.Rate = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.Rate;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.WithSL = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.WithSL;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.SLTriggered;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.IOC = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.IOC;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.DisclosedQty = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.DisclosedQty;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.OldQty = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.OldQty;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.TradedQty = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.TradedQty;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.PendingQty = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.PendingQty;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ExchangeOrderId;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.SLBMOrderModifyRequestRec.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.AHStatus = AHStatus1;
            OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.RecallRepayOrder = new string(OTSReqRec.AllReq.SLBMOrderModifyRequestRec.RecallRepayOrder).ToCharArray();
        }

        private void ProcessSLBMCancelOrderRequest(string ReplyStr)
        {
            int BOId, TimeNow;
            Single MBlk;
            char AHStatus1;

            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 5 && VList.Count() != 6)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            if (VList.Count() == 6)
                AHStatus1 = Convert.ToChar(VList[5].ToUpper());
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
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TSLBMOrderCancelBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSLBMOrderCancelBrokerReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.Exch = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.Exch;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.ExchType = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ExchType;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.ScripCode = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ScripCode;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.ScripName = new string(OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ScripName).PadRight(12).ToCharArray();
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.Series = new string(OTSReqRec.AllReq.SLBMOrderCancelRequestRec.Series).PadRight(2).ToCharArray();
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.ScripNameLength = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ScripNameLength;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.ExpiryDate = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ExpiryDate;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.BuySell = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.BuySell;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.Qty = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.Qty;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.BrokerOrderID = BOId;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.BrokerTime = TimeNow;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.MarginBlocked = MBlk;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.Rate = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.Rate;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.AtMarket = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.AtMarket;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.WithSL = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.WithSL;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.SLTriggered = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.SLTriggered;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.TriggerRate = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.TriggerRate;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.TraderRequestID;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.TradedQty = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.TradedQty;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.PendingQty = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.PendingQty;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.ExchangeOrderId = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ExchangeOrderId;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReqRec.AllReq.SLBMOrderCancelRequestRec.ExchangeOrderTime;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.AHStatus = AHStatus1;
            OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.RecallRepayOrder = new string(OTSReqRec.AllReq.SLBMOrderCancelRequestRec.RecallRepayOrder).ToCharArray();
        }

        private void ProcessSLBMHoldingsReport(DataTable dt)
        {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSLBMHoldingsReportReplyRecord());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.ScripName = Convert.ToString(dt.Rows[i]["NAME"]).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.ScripNameLength = Convert.ToByte(Convert.ToString(dt.Rows[i]["NAME"]).TrimEnd().Length);
                OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.Series = Convert.ToString(dt.Rows[i]["SERIES"]).PadRight(2).ToCharArray();
                OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.BfQty = Convert.ToInt32(dt.Rows[i]["BFQTY"]);
                OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.NetQty = Convert.ToInt32(dt.Rows[i]["NETQTY"]);

                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.BseCode = 0;
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.ReceivingDeliveringFlag = ' ';
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.DeliveryDate = " ".PadRight(11).ToCharArray();
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.TradeNo = 0;
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.SettlementType = ' ';
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.SettlementNo = 0;
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.ExpiryDate = " ".PadRight(11).ToCharArray();
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.QuantityToBedelivered = " ".PadRight(11).ToCharArray();
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.CustodialParticipantCode
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.LendingPrice
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.TotalAmount
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.RolloverIndicator
                //OTSReplyRec.AllReply[i].SLBMHoldingsReportReplyRec.Reserved        
            }
        }
        #endregion

        private string MakeDerivDesc(char Exch, ITInteractiveStructures.TDerivInfoRecord DerivInfo)
        {
            string Result, ExDt;
            if (Exch == 'M')//McxTrading
                ExDt = ExchMcxStartDate.AddSeconds(DerivInfo.ExpiryDate).ToString("dd MMM yyyy");
            else
                ExDt = ExchStartDate.AddSeconds(DerivInfo.ExpiryDate).ToString("dd MMM yyyy");
            Result = new string(DerivInfo.Symbol).Substring(0, DerivInfo.SymbolLength) + " " + ExDt;
            if (DerivInfo.CPType > 0)
            {
                if (Exch == 'C' || Exch == 'E')
                    Result = Result + " " + ArrayOptionType[DerivInfo.CPType] + " " + DerivInfo.StrikePrice.ToString("0.0000");
                else
                    Result = Result + " " + ArrayOptionType[DerivInfo.CPType] + " " + DerivInfo.StrikePrice.ToString("0.00");
            }
            return Result;
          
        }

        private string MakeDerivDescForSpread(int ExpiryDate, char[] Symbol, byte SymbolLength)
        {
            string Result, ExDt;
            ExDt = ExchStartDate.AddSeconds(ExpiryDate).ToString("dd MMM yyyy");
            Result = new string(Symbol).Substring(0, SymbolLength) + " " + ExDt;
            return Result;
            
        }

        private int GetCurrTimeInSecsFrom1980()
        {
            int Result;
            Result = Convert.ToInt32(Math.Round((DateTime.Now - ExchStartDate).TotalSeconds));
            return Result;
        }

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

        private char GetOrderType(byte OrderType)
        {
            char Result = 'I';
            switch (OrderType)
            {
                case 0: Result = 'I'; break;
                case 1: Result = 'D'; break;
                case 2: Result = 'M'; break;
                case 3: Result = 'C'; break; // sumitp 28_jan_22
                case 4: Result = 'B'; break; // sumitp 28_jan_22
                default: break;
            }
            return Result;
        }

        private byte ReturnOrderType(char OrderType)
        {
            byte Result = 0;
            switch (OrderType)
            {
                case 'I': Result = 0; break;
                case 'D': Result = 1; break;
                case 'M': Result = 2; break;
                case 'C': Result = 3; break;
                case 'B': Result = 4; break;
                default: break;
            }
            return Result;
        }
        private char GetIOCValue(byte IOC)
        {
            char Result = 'N';
            switch (IOC)
            {
                case 0: Result = 'N'; break;
                case 1: Result = 'Y'; break;
                default: break;
            }
            return Result;
        }

        private byte ReturnNewOrderType(string NewOrderType)           //Himanshu   
        {
            byte Result = 0;
            switch (NewOrderType)
            {
                case "CO": Result = 3; break;
                case "BO": Result = 4; break;
                default: break;
            }
            return Result;
        }
        private string GetOrderTypeNew(byte OrderTypeNew)//This is to convert OrderTypeNew from 1 and 2 to "CO" and "BO" because in db OrderTypeNew accepts "CO" or "BO"
        {
            string Result = "NA";
            switch (OrderTypeNew)
            {
                case 1: Result = "RESERVED1"; break;
                case 2: Result = "RESERVED2"; break;
                case 3: Result = "CO"; break;
                default: break;
            }
            return Result;
        }

        //private byte ReturnOrderTypeNew(string OrderTypeNew)
        //{
        //    byte Result = 1;
        //    switch (OrderTypeNew)
        //    {
        //        case "CO": Result = 1; break;
        //        case "BO": Result = 2; break;
        //        default: break;
        //    }
        //    return Result;
        //}

        private string MakeDerivDescForOrderType(char Exch, byte CPType, int ExpiryDate, float StrikePrice, char[] Symbol, byte SymbolLength)
        {
            string Result, ExDt;
            if (Exch == 'M')//McxTrading
                ExDt = ExchMcxStartDate.AddSeconds(ExpiryDate).ToString("dd MMM yyyy");
            else
                ExDt = ExchStartDate.AddSeconds(ExpiryDate).ToString("dd MMM yyyy");
            Result = new string(Symbol).Substring(0, SymbolLength) + " " + ExDt;
            if (CPType > 0)
            {
                if (Exch == 'C' || Exch == 'E')
                    Result = Result + " " + ArrayOptionType[CPType] + " " + StrikePrice.ToString("0.0000");
                else
                    Result = Result + " " + ArrayOptionType[CPType] + " " + StrikePrice.ToString("0.00");
            }
            return Result;

        }

        //---------------------------------

        #region BSemf Krutika
        private void ProcessBseMFPlaceOrderRequest(string ReplyStr)//BseMf Krutika
        {
            try
            {
            int BOId, TimeNow;
            string BoidTime;
            Single MBlk;
            char AHStatus1;
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            BoidTime = BOId + "," + TimeNow;
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
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelBrokerResponse());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelBrokerResponse());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Exch = OTSReqRec.AllReq.MfAddModifyCancelReqRec.Exch;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.ExchType = OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchType;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.UniqueNo = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNo).PadRight(5).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.UniqueNoLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNoLength;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.SchemeCode = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCode).PadRight(20).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.SchemeCodeLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCodeLength;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySell = OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySell;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySellTypeLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellTypeLength;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySellType = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellType).PadRight(10).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.DptxnType = OTSReqRec.AllReq.MfAddModifyCancelReqRec.DptxnType;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Amount = OTSReqRec.AllReq.MfAddModifyCancelReqRec.Amount;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.RedeemQty = OTSReqRec.AllReq.MfAddModifyCancelReqRec.RedeemQty;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.FolioNo = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.FolioNo).PadRight(16).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.KYCStatus = OTSReqRec.AllReq.MfAddModifyCancelReqRec.KYCStatus;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Remarks = BoidTime.PadRight(200).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.TraderRequestID = OTSReqRec.AllReq.MfAddModifyCancelReqRec.TraderRequestID;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.ExchOrderNo = OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchOrderNo;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BrokerId = BOId;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BrokerTime = TimeNow;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        //---------------------------------
        private void ProcessBseMFModifyOrderRequest(string ReplyStr)//BseMf Krutika
        {
            try
            {
            int BOId, TimeNow;
            string BoidTime;
            Single MBlk;
            char AHStatus1;
            VList = ReplyStr.Split(splitchar);
            if (VList.Count() != 4 && VList.Count() != 5)//improper string
            {
                ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                VList = ReplyStr.Split(splitchar);
            }
            BOId = Convert.ToInt32(VList[0]);
            TimeNow = Convert.ToInt32(VList[1]);
            BoidTime = BOId + "," + TimeNow;
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
            OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelBrokerResponse());
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelBrokerResponse());
            OTSReplyRec.ReplyHdr.ReplyCnt = 1;
            Array.Resize(ref OTSReplyRec.AllReply, 1);
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Exch = OTSReqRec.AllReq.MfAddModifyCancelReqRec.Exch;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.ExchType = OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchType;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.UniqueNo = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNo).PadRight(5).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.UniqueNoLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNoLength;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.SchemeCode = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCode).PadRight(20).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.SchemeCodeLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCodeLength;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySell = OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySell;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySellType = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellType).PadRight(10).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySellTypeLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellTypeLength;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.DptxnType = OTSReqRec.AllReq.MfAddModifyCancelReqRec.DptxnType;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Amount = OTSReqRec.AllReq.MfAddModifyCancelReqRec.Amount;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.RedeemQty = OTSReqRec.AllReq.MfAddModifyCancelReqRec.RedeemQty;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.FolioNo = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.FolioNo).PadRight(16).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.KYCStatus = OTSReqRec.AllReq.MfAddModifyCancelReqRec.KYCStatus;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Remarks = BoidTime.PadRight(200).ToCharArray();
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.TraderRequestID = OTSReqRec.AllReq.MfAddModifyCancelReqRec.TraderRequestID;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.ExchOrderNo = OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchOrderNo;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BrokerId = BOId;
            OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BrokerTime = TimeNow;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }

        }

        //--------------------------------

        private void ProcessBseMFCancelOrderRequest(string ReplyStr)//BseMf Krutika
        {
            try
            {
                int BOId, TimeNow;
                string BoidTime;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 4 && VList.Count() != 5)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                BoidTime = BOId + "," + TimeNow;
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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelBrokerResponse());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelBrokerResponse());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Exch = OTSReqRec.AllReq.MfAddModifyCancelReqRec.Exch;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.ExchType = OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchType;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.UniqueNo = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNo).PadRight(5).ToCharArray();
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.UniqueNoLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.UniqueNoLength;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.SchemeCode = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCode).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.SchemeCodeLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.SchemeCodeLength;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySell = OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySell;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySellType = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellType).PadRight(10).ToCharArray();
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BuySellTypeLength = OTSReqRec.AllReq.MfAddModifyCancelReqRec.BuySellTypeLength;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.DptxnType = OTSReqRec.AllReq.MfAddModifyCancelReqRec.DptxnType;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Amount = OTSReqRec.AllReq.MfAddModifyCancelReqRec.Amount;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.RedeemQty = OTSReqRec.AllReq.MfAddModifyCancelReqRec.RedeemQty;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.FolioNo = new string(OTSReqRec.AllReq.MfAddModifyCancelReqRec.FolioNo).PadRight(16).ToCharArray();
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.KYCStatus = OTSReqRec.AllReq.MfAddModifyCancelReqRec.KYCStatus;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Remarks = BoidTime.PadRight(200).ToCharArray();
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.TraderRequestID = OTSReqRec.AllReq.MfAddModifyCancelReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.ExchOrderNo = OTSReqRec.AllReq.MfAddModifyCancelReqRec.ExchOrderNo;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BrokerId = BOId;
                OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BrokerTime = TimeNow;
            
             }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }


        private void ProcessBseMfOrderTypeOrdersReport(DataTable dt)
        {

            try
            {
            int RPos, RCnt, UniqueNo, Qty, LotSize;
            char RType, Exch;
            string Str1, ScName;
            Int64 ExchOrderID;
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
                ExchOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderId"]);
                Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                UniqueNo = Convert.ToInt32(dt.Rows[i]["UniqueNo"]);

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
                        if (ExchOrderID != OTSReplyRec.AllReply[j].BseMfOrdeTypeReportsReplyRec.ExchangeOrderID) continue;
                        if (Exch != OTSReplyRec.AllReply[j].BseMfOrdeTypeReportsReplyRec.Exch) continue;
                        if (UniqueNo != Convert.ToInt32(new string(OTSReplyRec.AllReply[j].BseMfOrdeTypeReportsReplyRec.UniqueNo).TrimEnd())) continue;
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

                if (RType == 'P')
                {

                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNo).TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCode).TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellType = Convert.ToString(dt.Rows[i]["BuySellType"]).PadRight(10).ToCharArray();
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellTypeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellType).TrimEnd().Length);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Reason = Convert.ToString(dt.Rows[i]["Reason"]).PadRight(200).ToCharArray();
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DpTxnType"]);

                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["Amount"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.RedeemQty = Convert.ToInt64(dt.Rows[i]["RedeemQty"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.KYCStatus = Convert.ToChar(dt.Rows[i]["KYCStatus"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(200).ToCharArray();
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderId"]);
                    OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);

                }
                else
                    if (RType == 'M')
                    {
                        #region OldCode
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                        //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                        #endregion

                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNo).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCode).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellType = Convert.ToString(dt.Rows[i]["BuySellType"]).PadRight(10).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellTypeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellType).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Reason = Convert.ToString(dt.Rows[i]["Reason"]).PadRight(200).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DpTxnType"]);

                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["Amount"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.RedeemQty = Convert.ToInt64(dt.Rows[i]["RedeemQty"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.KYCStatus = Convert.ToChar(dt.Rows[i]["KYCStatus"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(200).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderId"]);
                        OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);
                    }
                    else
                        if (RType == 'C' || RType == 'K')
                        {
                            #region Oldcode
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                            //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                            #endregion

                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.UniqueNo).TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.SchemeCode).TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellType = Convert.ToString(dt.Rows[i]["BuySellType"]).PadRight(10).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellTypeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BuySellType).TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Reason = Convert.ToString(dt.Rows[i]["Reason"]).PadRight(200).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DpTxnType"]);

                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["Amount"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.RedeemQty = Convert.ToInt64(dt.Rows[i]["RedeemQty"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.KYCStatus = Convert.ToChar(dt.Rows[i]["KYCStatus"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(200).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderId"]);
                            OTSReplyRec.AllReply[RPos].BseMfOrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);
                        }
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFOrderTypeReportsReplyRec());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessBseMfActivitylogReport(DataTable dt)
        {

            try
            {
                int RPos, RCnt, UniqueNo, Qty, LotSize;
                char RType, Exch;
                string Str1, ScName;
                Int64 ExchOrderID;
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
                    ExchOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderId"]);
                    Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                    UniqueNo = Convert.ToInt32(dt.Rows[i]["UniqueNo"]);

                   // if (RType == 'P') //Place
                  //  {
                        RPos = RCnt;
                        RCnt++;
                        Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                  //  }
                    //else
                    //{
                    //    Str1 = Convert.ToString(dt.Rows[i]["STATUS"]).TrimEnd(TrimChars);
                    //    if (Str1 == "Rejected" || Str1 == "Rejected By Broker")
                    //    {
                    //        continue;
                    //    }
                    //    for (int j = 0; j < RCnt; j++)
                    //    {
                    //        if (ExchOrderID != OTSReplyRec.AllReply[j].TBseMFActivitylogReplyRec.ExchangeOrderID) continue;
                    //        if (Exch != OTSReplyRec.AllReply[j].TBseMFActivitylogReplyRec.Exch) continue;
                    //        if (UniqueNo != Convert.ToInt32(new string(OTSReplyRec.AllReply[j].TBseMFActivitylogReplyRec.UniqueNo).TrimEnd())) continue;
                    //        RPos = j;
                    //        break;
                    //    }
                    //    if (RPos == -1)//impossible condition
                    //    {
                    //        RPos = RCnt;
                    //        RCnt++;
                    //        Array.Resize(ref OTSReplyRec.AllReply, RCnt);
                    //    }
                    //}

                    //if (RType == 'P')
                    //{

                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNo).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCode).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellType = Convert.ToString(dt.Rows[i]["BuySellType"]).PadRight(10).ToCharArray();
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellTypeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellType).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Reason = Convert.ToString(dt.Rows[i]["Reason"]).PadRight(200).ToCharArray();
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DpTxnType"]);

                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["Amount"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.RedeemQty = Convert.ToInt64(dt.Rows[i]["RedeemQty"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.KYCStatus = Convert.ToChar(dt.Rows[i]["KYCStatus"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(200).ToCharArray();
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderId"]);
                        OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);

                    //}
                    //else
                    //    if (RType == 'M')
                    //{
                    //    #region OldCode
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                    //    #endregion

                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNo).TrimEnd().Length);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCode).TrimEnd().Length);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellType = Convert.ToString(dt.Rows[i]["BuySellType"]).PadRight(10).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellTypeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellType).TrimEnd().Length);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Reason = Convert.ToString(dt.Rows[i]["Reason"]).PadRight(200).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DpTxnType"]);

                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["Amount"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.RedeemQty = Convert.ToInt64(dt.Rows[i]["RedeemQty"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.KYCStatus = Convert.ToChar(dt.Rows[i]["KYCStatus"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(200).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderId"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);
                    //}
                    //else
                    //        if (RType == 'C' || RType == 'K')
                    //{
                    //    #region Oldcode
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["EXCHORDERID"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["EXCHORDERTIME"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Qty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Rate = Convert.ToSingle(dt.Rows[i]["RATE"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.TriggerRate = Convert.ToSingle(dt.Rows[i]["SLTRIGGERRATE"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AtMarket = Convert.ToChar(dt.Rows[i]["ATMARKET"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.WithSL = Convert.ToChar(dt.Rows[i]["WITHSL"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.SLTriggered = 'N';
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["STATUS"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.StatusLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Status.TrimEnd().Length);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["EXCH"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["EXCHTYPE"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripCode = Convert.ToInt32(dt.Rows[i]["SCRIPCODE"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripNameLength = Convert.ToByte(OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.ScripName.TrimEnd().Length);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BUYSELL"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BROKERORDERID"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BROKERORDERTIME"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.AHStatus = Convert.ToChar(dt.Rows[i]["AFTERHOURS"]);
                    //    //OTSReplyRec.AllReply[RPos].OrdeTypeReportsReplyRec.OrderType = ReturnOrderType(Convert.ToChar(dt.Rows[i]["IntraDelivery"]));
                    //    #endregion

                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.UniqueNo).TrimEnd().Length);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.SchemeCode).TrimEnd().Length);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySell = Convert.ToChar(dt.Rows[i]["BuySell"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellType = Convert.ToString(dt.Rows[i]["BuySellType"]).PadRight(10).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellTypeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BuySellType).TrimEnd().Length);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Reason = Convert.ToString(dt.Rows[i]["Reason"]).PadRight(200).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DpTxnType"]);

                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["Amount"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.RedeemQty = Convert.ToInt64(dt.Rows[i]["RedeemQty"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.KYCStatus = Convert.ToChar(dt.Rows[i]["KYCStatus"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(200).ToCharArray();
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchangeOrderID = Convert.ToInt64(dt.Rows[i]["ExchOrderId"]);
                    //    OTSReplyRec.AllReply[RPos].TBseMFActivitylogReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);
                    //}
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFActivitylogReplyRec());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessBseMfSIPOrderTypeOrdersReport(DataTable dt)
        {

            try
            {
                int RPos, RCnt, UniqueNo, Qty, LotSize;
                char RType, Exch;
                string Str1, ScName;
                Int64 ExchOrderID;
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
                    ExchOrderID = Convert.ToInt64(dt.Rows[i]["SIPRegistrationNumber"]);
                    Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                    UniqueNo = Convert.ToInt32(dt.Rows[i]["UniqueNo"]);

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
                            if (ExchOrderID != OTSReplyRec.AllReply[j].BseMfSIPOrdeTypeReportsReplyRec.SIPRegistrationID) continue;
                            if (Exch != OTSReplyRec.AllReply[j].BseMfSIPOrdeTypeReportsReplyRec.Exch) continue;
                            if (UniqueNo != Convert.ToInt32(new string(OTSReplyRec.AllReply[j].BseMfSIPOrdeTypeReportsReplyRec.UniqueNo).TrimEnd())) continue;
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

                    if (RType == 'P')
                    {

                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNo).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCode).TrimEnd().Length);

                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.RegistrationDate = Convert.ToString(dt.Rows[i]["RegistrationDate"]).PadRight(10).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.StartDate = Convert.ToString(dt.Rows[i]["StartDate"]).PadRight(10).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.EndDate = Convert.ToString(dt.Rows[i]["EndDate"]).PadRight(20).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.FrequencyType = Convert.ToString(dt.Rows[i]["FrequencyType"]).PadRight(20).ToCharArray();

                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.TxnMode = Convert.ToChar(dt.Rows[i]["TranscationMode"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DPTxnMode"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.FirstOrderFlag = Convert.ToChar(dt.Rows[i]["Generate1stordertoday"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.InstallmentNumber = Convert.ToInt32(dt.Rows[i]["Numofinstallments"]);

                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["InstallmentAmount"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(20).ToCharArray();
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SIPRegistrationID = Convert.ToInt64(dt.Rows[i]["SIPRegistrationNumber"]);
                        OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);

                    }
                    else
                        if (RType == 'M')
                        {
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNo).TrimEnd().Length);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCode).TrimEnd().Length);

                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.RegistrationDate = Convert.ToString(dt.Rows[i]["RegistrationDate"]).PadRight(10).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.StartDate = Convert.ToString(dt.Rows[i]["StartDate"]).PadRight(10).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.EndDate = Convert.ToString(dt.Rows[i]["EndDate"]).PadRight(20).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.FrequencyType = Convert.ToString(dt.Rows[i]["FrequencyType"]).PadRight(20).ToCharArray();

                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.TxnMode = Convert.ToChar(dt.Rows[i]["TranscationMode"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DPTxnMode"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.FirstOrderFlag = Convert.ToChar(dt.Rows[i]["Generate1stordertoday"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.InstallmentNumber = Convert.ToInt32(dt.Rows[i]["Numofinstallments"]);

                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["InstallmentAmount"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(20).ToCharArray();
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SIPRegistrationID = Convert.ToInt64(dt.Rows[i]["SIPRegistrationNumber"]);
                            OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);
                        }
                        else
                            if (RType == 'C' || RType == 'K')
                            {
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNo = Convert.ToString(dt.Rows[i]["UniqueNo"]).PadRight(5).ToCharArray();
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNoLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.UniqueNo).TrimEnd().Length);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCode = Convert.ToString(dt.Rows[i]["SchemeCode"]).PadRight(20).ToCharArray();
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCodeLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCode).TrimEnd().Length);

                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.RegistrationDate = Convert.ToString(dt.Rows[i]["RegistrationDate"]).PadRight(10).ToCharArray();
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.StartDate = Convert.ToString(dt.Rows[i]["StartDate"]).PadRight(10).ToCharArray();
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.EndDate = Convert.ToString(dt.Rows[i]["EndDate"]).PadRight(20).ToCharArray();
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.FrequencyType = Convert.ToString(dt.Rows[i]["FrequencyType"]).PadRight(20).ToCharArray();
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.TxnMode = Convert.ToChar(dt.Rows[i]["TranscationMode"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.DptxnType = Convert.ToChar(dt.Rows[i]["DPTxnMode"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.FirstOrderFlag = Convert.ToChar(dt.Rows[i]["Generate1stordertoday"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.InstallmentNumber = Convert.ToInt32(dt.Rows[i]["Numofinstallments"]);

                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Amount = Convert.ToSingle(dt.Rows[i]["InstallmentAmount"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]).PadRight(20).ToCharArray();
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.BrokerId = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.BrokerTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SIPRegistrationID = Convert.ToInt64(dt.Rows[i]["SIPRegistrationNumber"]);
                                OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.ExchangeOrderTime = Convert.ToInt32(dt.Rows[i]["ExchOrderTime"]);
                            }
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPOrderTypeReportsReplyRec());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        private void ProcessManddateOrderTypeOrdersReport(DataTable dt)
        {

            try
            {
                int RPos, RCnt, UniqueNo, Qty, LotSize;
                char RType, Exch;
                string Str1, ScName, MANDATEID;
                Int64 ExchOrderID;
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
                    MANDATEID =Convert.ToString( dt.Rows[i]["MandateID"]);
                    Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                   

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
                            if (MANDATEID !=Convert.ToString( OTSReplyRec.AllReply[j].MandateOrderTypeReportsReplyRec.MANDATEID).Substring(0, OTSReplyRec.AllReply[j].MandateOrderTypeReportsReplyRec.MandateidLength).TrimEnd() )continue;
                            if (Exch != OTSReplyRec.AllReply[j].BseMfSIPOrdeTypeReportsReplyRec.Exch) continue;
                           // if (UniqueNo != Convert.ToInt32(new string(OTSReplyRec.AllReply[j].BseMfSIPOrdeTypeReportsReplyRec.UniqueNo).TrimEnd())) continue;
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

                    if (RType == 'P')
                    {

                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.Exch = Convert.ToChar(dt.Rows[i]["Exch"]);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.ExchType = Convert.ToChar(dt.Rows[i]["ExchType"]);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.IFSC = Convert.ToString(dt.Rows[i]["IFSCCode"]).PadRight(15).ToCharArray();
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.MANDATEID = Convert.ToString(dt.Rows[i]["MandateID"]).PadRight(15).ToCharArray();
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.MICR = Convert.ToString(dt.Rows[i]["MICRCode"]).PadRight(15).ToCharArray();
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.ACtype = Convert.ToString(dt.Rows[i]["AccType"]).PadRight(2).ToCharArray();// Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].BseMfSIPOrdeTypeReportsReplyRec.SchemeCode).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.MandateType = Convert.ToChar(dt.Rows[i]["MandateType"]);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.MandateidLength = Convert.ToByte(new string(OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.MANDATEID).TrimEnd().Length);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.AccountNo =Convert.ToString( dt.Rows[i]["AccountNo"]);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.Amount = Convert.ToUInt64(dt.Rows[i]["Amount"]);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.BrokerOrderID = Convert.ToInt32(dt.Rows[i]["BrokerOrderId"]);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.BrokerOrderTime = Convert.ToInt32(dt.Rows[i]["BrokerOrderTime"]);
                        OTSReplyRec.AllReply[RPos].MandateOrderTypeReportsReplyRec.Status = Convert.ToString(dt.Rows[i]["Status"]);
                       
                       

                    }
                    
                }
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.MandateOrderTypeReportsReplyRec());
                OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)RCnt;
                OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessHoldingsReportOfBseMf(DataTable dt)//Bsemf Krutika
        {
            if (dt.Rows.Count < 1)
            {
                OTSReplyRec.ReplyHdr.ReplyRecSize = 0;
                OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                OTSReplyRec.ReplyHdr.ReplyLen = 0;
                OTSReplyRec.AllReply = null;
                return;
            }
            OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.THoldingsReportReplyRecord_MF());
            OTSReplyRec.ReplyHdr.ReplyCnt = (ushort)dt.Rows.Count;
            OTSReplyRec.ReplyHdr.ReplyLen = OTSReplyRec.ReplyHdr.ReplyCnt * OTSReplyRec.ReplyHdr.ReplyRecSize;
            Array.Resize(ref OTSReplyRec.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.BseCode = Convert.ToInt32(dt.Rows[i]["BSECODE"]);
                // OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.NseCode = Convert.ToInt32(dt.Rows[i]["NSECODE"]);
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.UniqueNo = Convert.ToInt32(dt.Rows[i]["UNIQUENO"]);
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.SchemeName = Convert.ToString(dt.Rows[i]["SCRIPNAME"]).PadRight(200).ToCharArray();
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.SchemeNameLength = Convert.ToByte(new string(OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.SchemeName).TrimEnd().Length);
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.TotalQty = Convert.ToInt32(dt.Rows[i]["QTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.SoldQty = Convert.ToInt32(dt.Rows[i]["QTYSOLD"]);
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.BrokerBeniQty = Convert.ToInt32(dt.Rows[i]["BROKERBENIQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.ClientDPQty = Convert.ToInt32(dt.Rows[i]["CLIENTDPQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.ExchRcvQty = Convert.ToInt32(dt.Rows[i]["EXCHRCVQTY"]);
                OTSReplyRec.AllReply[i].HoldingsReportofBseMfReplyRec.Collatral = Convert.ToInt32(dt.Rows[i]["COLLATERALQTY"]);
            }
        }
        private void ProcessBseMFMandate(string ReplyStr)
        {
            try
            {
                int BOId, TimeNow;
                string BoidTime;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 3 && VList.Count() != 4)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                BoidTime = BOId + "," + TimeNow;
                if (VList.Count() == 5)
                    AHStatus1 = Convert.ToChar(VList[4].ToUpper());
                else
                    AHStatus1 = 'N';
                AfterHours = AHStatus1;

                if (VList[3] != "0")
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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.XSIPMandateBrokerReply());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.XSIPMandateBrokerReply());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.Exch = OTSReqRec.AllReq.XSIPMandateRec.Exch;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.ExchType = OTSReqRec.AllReq.XSIPMandateRec.ExchType;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.Amount = OTSReqRec.AllReq.XSIPMandateRec.Amount;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.MandateType = OTSReqRec.AllReq.XSIPMandateRec.MandateType;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.AccountNo = OTSReqRec.AllReq.XSIPMandateRec.AccountNo;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.ACtype = OTSReqRec.AllReq.XSIPMandateRec.ACtype;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.IFSC = OTSReqRec.AllReq.XSIPMandateRec.IFSC;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.MICR = OTSReqRec.AllReq.XSIPMandateRec.MICR;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.StartDate = OTSReqRec.AllReq.XSIPMandateRec.StartDate;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.EndDate = OTSReqRec.AllReq.XSIPMandateRec.EndDate;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.TraderRequestID = OTSReqRec.AllReq.XSIPMandateRec.TraderRequestID;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.BrokerOrderID = BOId; 
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.BrokerOrderTime = TimeNow;
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.Remarks = BoidTime.PadRight(200).ToCharArray();
                OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.RemarksLength = (byte)BoidTime.TrimEnd().Length;


            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        private void ProcessBseMFSIPRegistrationRequest(string ReplyStr)//BseMf Krutika
        {
            try
            {
                int BOId, TimeNow;
                string BoidTime;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 4 && VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                BoidTime = BOId + "," + TimeNow;
                if (VList.Count() == 5)
                    AHStatus1 = Convert.ToChar(VList[4].ToUpper());
                else
                    AHStatus1 = 'N';
                AfterHours = AHStatus1;

                if (VList[5] != "0")
                {
                    OTSReplyRec.ReplyHdr.ErrorCode = 1;
                    OTSReplyRec.ErrorStringLength = (byte)VList[5].TrimEnd().Length;
                    OTSReplyRec.ErrorString = VList[5];
                    MBlk = 0;
                }
                else
                {
                    OTSReplyRec.ReplyHdr.ErrorCode = 0;
                    OTSReplyRec.ErrorStringLength = 0;
                    OTSReplyRec.ErrorString = "";
                    MBlk = Convert.ToSingle(VList[5]);
                }
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationMsgBrokerReply());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationMsgBrokerReply());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Exch = OTSReqRec.AllReq.MfSIPRegistrationReqRec.Exch;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.ExchType = OTSReqRec.AllReq.MfSIPRegistrationReqRec.ExchType;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.UniqueNo = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.UniqueNo).PadRight(5).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.UniqueNoLength = OTSReqRec.AllReq.MfSIPRegistrationReqRec.UniqueNoLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.AMCName = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.AMCCode).PadRight(50).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.AMCNameLength = OTSReqRec.AllReq.MfSIPRegistrationReqRec.AMCCodeLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SchemeCode = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.SchemeCode).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SchemeCodeLength = OTSReqRec.AllReq.MfSIPRegistrationReqRec.SchemeCodeLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.TransactionMode = OTSReqRec.AllReq.MfSIPRegistrationReqRec.TxnMode;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.DPTransactionMode = OTSReqRec.AllReq.MfSIPRegistrationReqRec.DptxnType;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FolioNo = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.FolioNo).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FrequencyType = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.FrequencyType).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FrequencyAllowed = OTSReqRec.AllReq.MfSIPRegistrationReqRec.FrequencyAllowed;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Amount = OTSReqRec.AllReq.MfSIPRegistrationReqRec.Amount;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Firstordertodayflag = OTSReqRec.AllReq.MfSIPRegistrationReqRec.Firstordertodayflag;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.StartDate = OTSReqRec.AllReq.MfSIPRegistrationReqRec.StartDate;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.InstallmentNumb = OTSReqRec.AllReq.MfSIPRegistrationReqRec.Numberofinstallments;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Remarks = BoidTime.PadRight(200).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.RemarksLength = (byte)BoidTime.TrimEnd().Length;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.TraderRequestID = OTSReqRec.AllReq.MfSIPRegistrationReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SIPregistrationNum = OTSReqRec.AllReq.MfSIPRegistrationReqRec.SIPregistrationNum;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderTime = TimeNow;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.EndDate = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.EndDate).PadRight(20).ToCharArray();
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        private void ProcessBseMFXSIPRegistrationRequest(string ReplyStr)//BseMf Krutika
        {
            try
            {
                int BOId, TimeNow;
                string BoidTime;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 5 && VList.Count() != 6)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                BoidTime = BOId + "," + TimeNow;
                if (VList.Count() == 5)
                    AHStatus1 = Convert.ToChar(VList[4].ToUpper());
                else
                    AHStatus1 = 'N';
                AfterHours = AHStatus1;

                if (VList[4] != "0")
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
                    MBlk = Convert.ToSingle(VList[5]);
                }
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationMsgBrokerReply());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationMsgBrokerReply());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Exch = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Exch;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.ExchType = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.ExchType;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.UniqueNo = new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.UniqueNo).PadRight(5).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.UniqueNoLength = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.UniqueNoLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.AMCName = new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.AMCCode).PadRight(50).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.AMCNameLength = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.AMCCodeLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SchemeCode = new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SchemeCode).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SchemeCodeLength = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SchemeCodeLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.TransactionMode = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.TxnMode;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.DPTransactionMode = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.DptxnType;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FolioNo = new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FolioNo).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FrequencyType = new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FrequencyType).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FrequencyAllowed = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FrequencyAllowed;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Amount = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Amount;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Firstordertodayflag = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Firstordertodayflag;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.StartDate = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.StartDate;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.InstallmentNumb = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Numberofinstallments;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Remarks = BoidTime.PadRight(200).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.RemarksLength = (byte)BoidTime.TrimEnd().Length;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.TraderRequestID = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SIPregistrationNum = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SIPregistrationNum;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderTime = TimeNow;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.EndDate = new string(OTSReqRec.AllReq.XMfSIPRegistrationReqRec.EndDate).PadRight(20).ToCharArray();
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.AMCCode = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.AMCCode;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.AMCCodeLength = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.AMCCodeLength;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.DPCFlag = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.DPCFlag;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.DptxnType = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.DptxnType;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.EndDate = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.EndDate;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Exch = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Exch;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.ExchType = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.ExchType;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Firstordertodayflag = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Firstordertodayflag;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.FolioNo = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FolioNo;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.FolioNoLength = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FolioNoLength;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.FrequencyAllowed = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FrequencyAllowed;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.FrequencyType = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FrequencyType;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.FrequencyTypeLength = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.FrequencyTypeLength;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Numberofinstallments = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Numberofinstallments;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Reserved1 = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Reserved1;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Reserved2 = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Reserved2;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Reserved3 = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Reserved3;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Reserved4 = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Reserved4;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Reserved5 = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Reserved5;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.Reserved6 = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Reserved6;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.SchemeCode = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SchemeCode;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.SchemeCodeLength = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SchemeCodeLength;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.SIPregistrationNum = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.SIPregistrationNum;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.StartDate = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.StartDate;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.TraderRequestID = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.TraderRequestID;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.TxnMode = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.TxnMode;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.UniqueNo = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.UniqueNo;
                //OTSReplyRec.AllReply[0].XMfSIPRegistrationReqRec.UniqueNoLength = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.UniqueNoLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Brokerage =Convert.ToDouble( VList[3]);
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Mandate = OTSReqRec.AllReq.XMfSIPRegistrationReqRec.Mandate;//Convert.ToInt32(VList[2]);
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }
        private void ProcessBseMFSIPCancelOrderRequest(string ReplyStr)//BseMf Krutika
        {
            try
            {
                int BOId, TimeNow;
                string BoidTime;
                Single MBlk;
                char AHStatus1;
                VList = ReplyStr.Split(splitchar);
                if (VList.Count() != 4 && VList.Count() != 5)//improper string
                {
                    ReplyStr = "0|" + GetCurrTimeInSecsFrom1980() + "|1|Unknown Database Error. Please retry";
                    VList = ReplyStr.Split(splitchar);
                }
                BOId = Convert.ToInt32(VList[0]);
                TimeNow = Convert.ToInt32(VList[1]);
                BoidTime = BOId + "," + TimeNow;
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
                OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationMsgBrokerReply());
                OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationMsgBrokerReply());
                OTSReplyRec.ReplyHdr.ReplyCnt = 1;
                Array.Resize(ref OTSReplyRec.AllReply, 1);
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Exch = OTSReqRec.AllReq.MfSIPRegistrationReqRec.Exch;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.ExchType = OTSReqRec.AllReq.MfSIPRegistrationReqRec.ExchType;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.UniqueNo = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.UniqueNo).PadRight(5).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.UniqueNoLength = OTSReqRec.AllReq.MfSIPRegistrationReqRec.UniqueNoLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.AMCName = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.AMCCode).PadRight(50).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.AMCNameLength = OTSReqRec.AllReq.MfSIPRegistrationReqRec.AMCCodeLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SchemeCode = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.SchemeCode).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SchemeCodeLength = OTSReqRec.AllReq.MfSIPRegistrationReqRec.SchemeCodeLength;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.TransactionMode = OTSReqRec.AllReq.MfSIPRegistrationReqRec.TxnMode;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.DPTransactionMode = OTSReqRec.AllReq.MfSIPRegistrationReqRec.DptxnType;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FolioNo = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.FolioNo).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FrequencyType = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.FrequencyType).PadRight(20).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.FrequencyAllowed = OTSReqRec.AllReq.MfSIPRegistrationReqRec.FrequencyAllowed;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Amount = OTSReqRec.AllReq.MfSIPRegistrationReqRec.Amount;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Firstordertodayflag = OTSReqRec.AllReq.MfSIPRegistrationReqRec.Firstordertodayflag;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.StartDate = OTSReqRec.AllReq.MfSIPRegistrationReqRec.StartDate;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.InstallmentNumb = OTSReqRec.AllReq.MfSIPRegistrationReqRec.Numberofinstallments;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Remarks = BoidTime.PadRight(200).ToCharArray();
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.RemarksLength = (byte)BoidTime.TrimEnd().Length;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.TraderRequestID = OTSReqRec.AllReq.MfSIPRegistrationReqRec.TraderRequestID;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.SIPregistrationNum = OTSReqRec.AllReq.MfSIPRegistrationReqRec.SIPregistrationNum;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderID = BOId;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderTime = TimeNow;
                OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.EndDate = new string(OTSReqRec.AllReq.MfSIPRegistrationReqRec.EndDate).PadRight(20).ToCharArray();
            }
            catch (Exception ex)
            {
                Form1.ErrorLog(ex);
            }
        }

        #endregion
    }
}
