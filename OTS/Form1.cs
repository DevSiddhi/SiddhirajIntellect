using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Ini;
using OverbyteIcsWSocketS;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using IntellectSharedClassLibrary;
//using System.Text.Json;
using static OTS.ITInteractiveStructures;
using static OTS.TCustomWSocketLogger;
using System.Threading.Tasks;

namespace OTS
{
    public delegate void ThreadTerminationDelegate(DBResult DB);
    public delegate void ConnectionToDatabaseDelegate(string str1);
    public delegate void ThreadTerminationActionNew(DBResult db);

    public  partial class Form1 : Form
    {
        #region Variable Declaration

        public static string OTSVersion;
        int BytesFromITS, BytesToITS, BytesToEC1, BytesFromEC1, BytesToEC2, BytesFromEC2, MaxThreads, DBRequestCnt, DBReplyCnt, Ex1Cnt, Ex2Cnt, Ex3Cnt, ECNo, ThreadsInUseCnt = 0;
        int MaxDB = 2, MaxEC = 2, Counter = 0, DB1Req = 0, DB2Req = 0;
        bool ExcessLicenseUsed;
        DateTime BytesFromEC1Time;
        ITInteractiveStructures.TOTSReplyRecord OTSReplyRec_TradeToTrade, OTSReplyRec_BseNseMap, OTSReplyRec_DerivBannedScrip, OTSReplyRec_Illiquid, OTSReplyRec_LotQty, OTSReplyRec_SurveillanceIndicatorNse, OTSReplyRec_SurveillanceIndicatorBse;
        FileStream OTSRetryRrdStream, ITSRawFileStream, OutTextFile, ECRawFileStream;
        BinaryWriter OTSRetryRrdWriter, ITSRawFileWriter, ECRawFileWriter;
        StreamWriter OutTextFileWriter;

        //public static string OTPLink = "http://103.174.106.22/OTPAPI/OTP/SendOTP";
        // public static string OTPLink = "https://online.barodaetrade.com/OTPAPI/OTP/SendOTP";
        public static string OTPLink = "";
        public static string Forget_OTPLink = "";// "https://employee.bobcaps.in/OTPAPI/OTP/SendOTP";
        public static string IVROTPLink = "";
        public static bool BseCurrDeriv = false;

        public static object locker = new object();
        public static bool logEnable2FA = false;

        ITInteractiveStructures.TScripDerivRecord[] DerivMasterInfo;
        ITInteractiveStructures.TMaalFodoClientHoldingReportReplyRecord[] ArrayMFHoldings;
        ITInteractiveStructures.TThreadUserRecord[] UserArray;
        ITInteractiveStructures.TClientDbNumReportReplyRecord[] ArrayClientDbNum = new ITInteractiveStructures.TClientDbNumReportReplyRecord[0];
        ITInteractiveStructures.TLotQuantityRecord[] ArrayLotQty;//
        ITInteractiveStructures.TLotQuantityRecord[] ArrayLotQtyBseCurr;//bseCurr

        byte[] UserTypeArr = new byte[7] { 1, 2,3, 5, 15, 16, 100 };
        byte[] OTSReplyType = new byte[5] { 80, 81, 82, 68, 89 };
        char[] TrimChars = new char[2] { ' ', '\0' };

        public ThreadTerminationDelegate ThreadTermination;
        public ConnectionToDatabaseDelegate ConnectionToDatabase;
        public ThreadTerminationActionNew _ThreadTerminationNew;
        MCastSocketClassNew MCastIntellectHandlerNew;
        public bool AutoStartup = true;
        public bool SkipMessages = true;

        MCastSocketClass McastOTS;
        TWSocketServer ListenToITS;
        SocketClient ExchConnectorSoc1, ExchConnectorSoc2;
        //ThreadDB[,] DBThreadArray;
        TOTSThread[,] DBThreadArrayNew;
        Buffer ConvertedBuff = new Buffer();

        List<string> SLCliDbNum, SL;
        List<string> SLReport = new List<string>();
        List<ITInteractiveStructures.TOTSRequestRecord> PendingRequestList = new List<ITInteractiveStructures.TOTSRequestRecord>();
        List<ITInteractiveStructures.TOTSReplyRecord> PendingReplyList = new List<ITInteractiveStructures.TOTSReplyRecord>();
        List<ITInteractiveStructures.TOTSReplyRecord> OrderTypeList = new List<ITInteractiveStructures.TOTSReplyRecord>();
        public List<ITInteractiveStructures.TOTSReplyRecord> DealerTypeList = new List<ITInteractiveStructures.TOTSReplyRecord>(); //15.10.2016
        bool ProcessFileITS = false;
        List<string> SLW2D = new List<string>();
        public List<string> LogReqToDbList = new List<string>();//15.8.14     
        public List<string> LogAllReqToDbList = new List<string>();//16.05.2018 Pranoti
        int count = 0;
        public Hashtable CurrencyLotToQtyMap,CurrencyLotToQtyMapBSECURR;//bseCurr
        
        BrokerNameRecords BrokerCodeOTS;
        public enum BrokerNameRecords
        {
            BROKER_VENTURA = 0, BROKER_IN_NIVESH = 1, BROKER_KIFS = 2, BROKER_LKP = 3, BROKER_MONARCH_NETWORTH = 4, BROKER_PINNACLE = 5, BROKER_SUNLIGHT = 6, BROKER_SWASTIKA = 7, BROKER_PURE = 8, BROKER_NIRMAN = 9, BROKER_SHAH_INVESTORS = 10, BROKER_FAIRWEALTH = 11, BROKER_INDIRA = 12, BROKER_ASTHA = 13, BROKER_EMKAY = 14, BROKER_LKPTRADESMART = 15, BROKER_AMBALAL = 16, BROKER_INDOTHAI = 17, BROKER_LATIN = 18, BROKER_PUMARTH = 19, BROKER_F6 = 20, BROKER_LAXMISHREE = 21, BROKER_MANGALKESHAV = 22, BROKER_Aranox = 23, BROKER_PlusWealth = 24, BROKER_MSEFSL = 25, BROKER_TradeJini = 26,
            BROKER_AJMERA = 27, BROKER_MARFATIA = 28, BROKER_PROSTOCKS = 29, BROKER_JHAVERI = 30, BROKER_ACHINTYA = 31//23.03.20 Ashishk added Prostoks BrokerCode
        };

        StringBuilder DaysLeft = new StringBuilder(100);//added on 14/10/2015 --Yashwant
        StringBuilder DateLeft = new StringBuilder(100);//added on 14/10/2015 --Yashwant
        DateTime DateExp; //20.08.18 Priyanka
        public static string BName;
        #endregion

        #region Form Region
        public Form1()
        {
            InitializeComponent();
            //Text = "OTS 23.03.09.0"; //"OTS 22.10.07.0";    
            Text = "OTS 26.03.09";//"OTS 26.01.08.0"; //"OTS 25.12.30.0";//"OTS 24.10.08.1"; //"OTS 23.07.18.0"; // "OTS 23.04.24.0"; //"OTS 23.04.20.0"; ////"OTS 23.03.27.0"; //"OTS 23.03.16.0"; //"OTS 23.03.08.0"; //"OTS 22.10.07.0";    
            string path = "ChangeLog_"+Text+".txt";

            if (!File.Exists(path))
            {
                //File.Create(path);
                File.AppendAllText(path, new ChangeLog().chnagelog);
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
            ThreadTermination = new ThreadTerminationDelegate(this.ThreadTerminationHandler);
            ConnectionToDatabase = new ConnectionToDatabaseDelegate(this.AccessControls);
            _ThreadTerminationNew = new ThreadTerminationActionNew(this.ThreadTerminationHandler);

            Kernel32.GetEnvironmentVariable("WLRegDateDaysLeft", DateLeft, 100);//added on 14/10/2015 --Yashwant
            Kernel32.GetEnvironmentVariable("WLRegDaysLeft", DaysLeft, 100);//added on 14/10/2015 --Yashwant
            if (DaysLeft.ToString() != "" && DateLeft.ToString() != "")//added on 14/10/2015 --Yashwant
            {
                MessageBox.Show((Math.Max(int.Parse(DateLeft.ToString()), int.Parse(DaysLeft.ToString())).ToString() + " Days remaining before license expire"), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            //20.08.18 Priyanka OTS License
            #region Comment
            //string DateExpiry = "01-10-2018";
            //DateTime DtExp; int Diff = 0;
            //if (DateTime.TryParseExact(DateExpiry, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DtExp))
            //{
            //    TimeSpan span = (DtExp - DateTime.Now);
            //    Diff = span.Days;
            //}
            //DateExp = DtExp;
            //if (Diff <= 5 && Diff > 0)
            //{
            //    if (Diff > 1)
            //        MessageBox.Show("This is a reminder for OTS License, which will get expired in " + Diff.ToString() + " days. You can contact Intellect for extension.", "License Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    else
            //        MessageBox.Show("This is a reminder for OTS License, which will get expired tommorrow. You can contact Intellect for extension.", "License Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    SendMailOnLicenseExpire(Diff);
            //}
            //if (Diff <= 0)
            //{
            //    MessageBox.Show("Your OTS license is expired. You can contact Intellect for extension.", "License Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    SendMailOnLicenseExpire(Diff);
            //    this.Dispose();
            //}
            #endregion
        }

        public static void Log2FA(string Data)
        {
            if (logEnable2FA)
                File.AppendAllText($"2FA_Log_{DateTime.Today.ToString("ddMMyy")}.txt", Data + Environment.NewLine);
        }

        private void SendMailOnLicenseExpire(int Diff)
        {
            DateTime Dt = DateExp.AddDays(-1);
            string DtExp = Dt.ToString("dd-MM-yyyy");
            string MailPassword = "launchpad12!", From = "info@intellectsoftwares.com";
            MailMessage mm = new MailMessage();
            mm.From = new MailAddress("info@intellectsoftwares.com");
            mm.To.Add(new MailAddress("pkasliwal92@gmail.com"));
            mm.To.Add(new MailAddress("mkasliwal11@gmail.com"));
            mm.CC.Add(new MailAddress("kapil@intellectsoftwares.com"));
            mm.CC.Add(new MailAddress("devesh@intellectsoftwares.com"));
            mm.CC.Add(new MailAddress("sudesh@intellectsoftwares.com"));
            mm.Subject = "System License Expire";
            if (Diff <= 5 && Diff > 0)
                mm.Body = "Dear Sir," + Environment.NewLine + "" + Environment.NewLine + "This is a reminder for your System License, which will get expired in " + Diff.ToString() + " days (01-10-2018). You can contact Intellect for extension." + Environment.NewLine + "" + Environment.NewLine + "Intellect Software Solutions Pvt Ltd." + Environment.NewLine + "Tel: 022 - 6561 0061 / 022 28707900 Ext:921";
            if (Diff <= 0) mm.Body = "Dear Sir," + Environment.NewLine + "" + Environment.NewLine + "Your System license is expired. You can contact Intellect for extension." + Environment.NewLine + "" + Environment.NewLine + "Intellect Software Solutions Pvt Ltd." + Environment.NewLine + "Tel: 022 - 6561 0061 / 022 28707900 Ext:921";
            mm.IsBodyHtml = false;
            SmtpClient smtp2 = new SmtpClient();
            smtp2.Host = "mail.intellectsoftwares.com";
            smtp2.EnableSsl = false;
            NetworkCredential NetworkCred = new NetworkCredential(From.TrimEnd(), MailPassword.TrimEnd());
            smtp2.UseDefaultCredentials = true;
            try
            {
                smtp2.Credentials = NetworkCred;
                smtp2.Port = 587;
                smtp2.Send(mm);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                File.AppendAllText("FailedClientUpload.txt", "Error in Sending Email");
                File.AppendAllText("FailedClientUpload.txt", Environment.NewLine);
                return;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                logEnable2FA = true;
            }

            if (!File.Exists("LicenseOTS.dat"))
            {
                MessageBox.Show("License not Found. You can contact Intellect for license.", "License Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Dispose();
                return;
            }
            else
            {
                //string data = $"LicenseMode:Prepaid,Validity:{dateTimePicker1.Value:dd-MMM-yyyy},Key:Intellect,App:OTS,Broker:{textBox1.Text}";
                string Data = LicenseCreator_Intellect.Encryption.DecryptString(File.ReadAllBytes("LicenseOTS.dat"));
                string[] dataparams = Data.Split(',');
                bool licenseFound = false;
                foreach (var item in dataparams)
                {
                    string[] strings = item.Split(':');
                    if (strings[0].Equals("Validity", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (DateTime.TryParseExact(strings[1], "dd-MMM-yyyy", null, DateTimeStyles.None, out var dt))
                        {
                           int daysLeft = (int)((dt - DateTime.Today).TotalDays); licenseFound = true;
                            //daysLeft = 10;  //kaustubh
                            if (daysLeft < 0)
                            {
                                MessageBox.Show($"Your License has Expired on {dt:dd-MMM-yyyy}. You can contact Intellect for extension.", "License Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                this.Dispose();
                                return;

                            }
                            else if (daysLeft <= 15)
                            {
                                MessageBox.Show($"Your License will expire in {daysLeft} days. You can contact Intellect for extension.", "License Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                if (!licenseFound)
                {
                    MessageBox.Show("Proper License not Found. You can contact Intellect for license.", "License Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Dispose();
                    return;
                }
            }

            if (File.Exists("OTS.ini"))
            {
                IniFile ini = new IniFile(Path.GetDirectoryName(Application.ExecutablePath) + "\\OTS.ini");
                textBoxOTSID.Text = ini.IniReadValue("General Info", "OTSID");
                textBoxThreadCnt.Text = ini.IniReadValue("General Info", "ThreadCount");
                textBoxMcastGrpIP.Text = ini.IniReadValue("General Info", "MCast IP");
                textBoxMcastGrpPort.Text = ini.IniReadValue("General Info", "MCast Port");
                textBoxExchConn1IP.Text = ini.IniReadValue("General Info", "ExchConnector1 IP");
                textBoxExchConn2IP.Text = ini.IniReadValue("General Info", "ExchConnector2 IP");
                textBoxRetryAttemptsCnt.Text = ini.IniReadValue("General Info", "Retry attempts");
                Location = new System.Drawing.Point(Convert.ToInt32(ini.IniReadValue("Location Info", "X")), Convert.ToInt32(ini.IniReadValue("Location Info", "Y")));
                lbl_BrokerName.Text = GetBrokerName(Convert.ToInt32(ini.IniReadValue("General Info", "BrokerCode")));
                BName = lbl_BrokerName.Text.TrimEnd();
                try
                {
                    checkwritereply.Checked = Convert.ToBoolean(ini.IniReadValue("General Info", "WriteReplyCheck"));
                    checkBox2.Checked = Convert.ToBoolean(ini.IniReadValue("General Info", "Log2FACheck"));
                    string IP = ini.IniReadValue("Settings", "GroupIP");
                    int Port = Convert.ToInt32(ini.IniReadValue("Settings", "GroupPort"));
                    int ID = Convert.ToInt32(ini.IniReadValue("Settings", "GroupID"));

                    txtMulticastIP.Text = IP;
                    txtMulticastPort.Text = Port.ToString();
                    txtMulticastID.Text = ID.ToString();
                }
                catch (Exception ex)
                {

                }
            }
            OTPLink = ConfigurationSettings.AppSettings["OTPLink_Login"];
            Forget_OTPLink = ConfigurationSettings.AppSettings["OTPLink_Forgot"];
            if (OTPLink == null)
            {
                MessageBox.Show("Please Update Config file with proper URL");
            }
            IVROTPLink = ConfigurationSettings.AppSettings["IVROTPLink"];
            if (IVROTPLink == null)
            {
                MessageBox.Show("Please Update Config file with proper URL");
            }
            SLReport.Add("CurrTime|ThNum|PendReqCnt|ThreadsInUse|ReqType|TimeTaken|Status|ReqStr|ExStr");
            textBoxSaveToFileITSReq.Text = DateTime.Now.ToString("ddMMyyyy") + "_ITSRequest.Dat";
            textBoxFileNameEC.Text = DateTime.Now.ToString("ddMMyyyy") + "_ECReply.Dat";
            dataGridViewLicense.Rows.Add("Trader", 0, 0);
            dataGridViewLicense.Rows.Add("Non FE", 0, 0);
            dataGridViewLicense.Rows.Add("Admin", 0, 0);
            dataGridViewLicense.Rows.Add("Call Center", 0, 0);
            dataGridViewLicense.Rows.Add("DB Admin", 0, 0);
            dataGridViewLicense.Rows[0].Height = 20;
            dataGridViewLicense.Rows[1].Height = 20;
            dataGridViewLicense.Rows[2].Height = 20;
            dataGridViewLicense.Rows[3].Height = 20;
            dataGridViewLicense.Rows[4].Height = 20;

            if (BrokerCodeOTS != BrokerNameRecords.BROKER_VENTURA)
            {
                MaxDB = 1;
                textBoxExchConn2IP.Enabled = false;
                btnExchConnect2.Enabled = false;
                checkBoxUseNewECLogic.Enabled = false;

                tabControl1.TabPages.RemoveAt(3);
                //tabControl1.TabPages.RemoveAt(4);
                tabControl1.TabPages.Remove(tabPage6);
            }

            MCastIntellectHandlerNew = new MCastSocketClassNew(txtMulticastIP.Text, txtMulticastPort.Text, MCastIntellectHandlerDataIn, this);

            Task UITask = Task.Delay(1000).ContinueWith(async (n) =>
            {
                int countDown = 11;
                lblMastersStatus.Visible = true;
                do
                {
                    lblMastersStatus.Text = $"Auto Startup in {--countDown}";
                    if (countDown == 0)
                    {
                        this.AutoStartup = false;
                        buttonStartUp_Click(buttonStartUp, e);
                        await Task.Delay(1000);
                        buttonListenITS_Click(sender, e);
                        await Task.Delay(1000);
                        btnExchConnect1_Click(sender, e);
                        break;
                    }
                    await Task.Delay(1000);
                } while (this.AutoStartup);
                lblMastersStatus.Visible = false;

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void MCastIntellectHandlerDataIn(byte[] dataToRead)
        {
            string Data = UTF8Encoding.UTF8.GetString(dataToRead);
            string[] lines = Data.Split('\n');
            foreach (var lineData in lines)
            {
                if (lineData == "") continue;
                string[] items = lineData.Split('|');
                if (items[1] == "OTS" && items[2] == txtMulticastID.Text)
                {
                    string ProcessType = items[3].ToLower();
                    if (ProcessType == "restart")
                    {
                        Application.Restart();
                    }
                    listLogs.Items.Add(lineData);
                }
            }
        }

        //string A = lbl_BrokerName.Text;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to exit this application", "Exit Application?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (result == System.Windows.Forms.DialogResult.No) //user clicked No
                {
                    e.Cancel = true;
                    return;
                }
            }
            //if (DBThreadArray != null)
            //{
            //    for (int i = 0; i < MaxDB; i++)
            //    {
            //        for (int j = 0; j < MaxThreads; j++)
            //        {
            //            DBThreadArray[i, j].t.IsBackground = true;                

            //        }
            //    }
            //}
            //27.8.14
            if (DBThreadArrayNew != null)
                {
                    for (int i = 0; i < MaxDB; i++)
                    {
                        for (int j = 0; j < MaxThreads; j++)
                        {
                            DBThreadArrayNew[i, j].Shutdown(false);
                        }
                    }
                }

                File.WriteAllLines("OTSdb_" + DateTime.Now.ToString("yyyyMMdd") + ".Txt", SLReport);
                TCustomWSocketLogger.flushAllandClose();
                //saves the required data from the design in INI file
                IniFile ini = new IniFile(Path.GetDirectoryName(Application.ExecutablePath) + "\\OTS.ini");
                ini.IniWriteValue("General Info", "OTSID", textBoxOTSID.Text);
                ini.IniWriteValue("General Info", "ThreadCount", textBoxThreadCnt.Text);
                ini.IniWriteValue("General Info", "MCast IP", textBoxMcastGrpIP.Text);
                ini.IniWriteValue("General Info", "MCast Port", textBoxMcastGrpPort.Text);
                ini.IniWriteValue("General Info", "ExchConnector1 IP", textBoxExchConn1IP.Text);
                ini.IniWriteValue("General Info", "ExchConnector2 IP", textBoxExchConn2IP.Text);
                ini.IniWriteValue("General Info", "Retry attempts", textBoxRetryAttemptsCnt.Text);


                ini.IniWriteValue("General Info", "WriteReplyCheck", checkwritereply.Checked.ToString());
                ini.IniWriteValue("General Info", "Log2FACheck", checkBox2.Checked.ToString());

                checkwritereply.Checked = Convert.ToBoolean(ini.IniReadValue("General Info", "WriteReplyCheck"));
                checkBox2.Checked = Convert.ToBoolean(ini.IniReadValue("General Info", "Log2FACheck"));

                ini.IniWriteValue("Settings", "GroupIP", txtMulticastIP.Text);
                ini.IniWriteValue("Settings", "GroupPort", txtMulticastPort.Text);
                ini.IniWriteValue("Settings", "GroupID", txtMulticastID.Text);

                if (Location.X > 0 && Location.Y > 0)
                {
                    ini.IniWriteValue("Location Info", "X", Location.X.ToString());
                    ini.IniWriteValue("Location Info", "Y", Location.Y.ToString());
                }
                else
                {
                    ini.IniWriteValue("Location Info", "X", 44.ToString());
                    ini.IniWriteValue("Location Info", "Y", 58.ToString());
                }
                File.WriteAllLines("OTSReqToDbF_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".txt", LogReqToDbList);//17.09.14  'F' Diff name of file here to differentiate.             
                File.WriteAllLines("OTSAllReqToDb_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".txt", LogAllReqToDbList);//16.05.2018 Pranoti Gad
            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ListenToITS != null) //11.09.14
                ListenToITS.Abort(); //10.09.14
            //Process.GetCurrentProcess().Kill();
        }
        #endregion

        private void buttonStartUp_Click(object sender, EventArgs e)
        {
            AutoStartup = false;

            try
            {
                if (lbl_BrokerName.Text == "BROKER NAME" || lbl_BrokerName.Text == "Unknown") //21.05.15
                {
                    MessageBox.Show("Please update OTS Ini with Proper Broker Code", "UnRegistered Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (BrokerCodeOTS == BrokerNameRecords.BROKER_IN_NIVESH)
                {
                    for (int RowCnt = 0; RowCnt < dataGridViewLicense.RowCount; RowCnt++)
                    {
                        dataGridViewLicense.Rows[0].Cells[1].Value = 160;
                        dataGridViewLicense.Rows[1].Cells[1].Value = 0;
                        dataGridViewLicense.Rows[2].Cells[1].Value = 10;
                        dataGridViewLicense.Rows[3].Cells[1].Value = 0;
                        dataGridViewLicense.Rows[4].Cells[1].Value = 1;
                    }
                }
                SLCliDbNum = new List<string>();
                SLCliDbNum.Add("CliCode,DbNum");
                BytesFromITS = 0;
                BytesToITS = 0;
                BytesToEC1 = 0;
                BytesFromEC1 = 0;
                BytesToEC2 = 0;
                BytesFromEC2 = 0;
                BytesFromEC1Time = DateTime.Now;

                MaxThreads = Convert.ToInt32(textBoxThreadCnt.Text);
                DBThreadArrayNew = new TOTSThread[MaxDB, MaxThreads]; //27.8.14
                for (int i = 0; i < MaxDB; i++)
                {
                    string FileName = "OTSIntellectDB" + (i + 1) + ".udl";
                    if (File.Exists(FileName))
                    {
                        for (int j = 0; j < MaxThreads; j++)
                        {
                            //DBThreadArrayNew[i, j] = new TOTSThread(1, FileName, this, new ThreadTerminationActionNew(ThreadTerminationHandler), (i + 1));
                            DBThreadArrayNew[i, j] = new TOTSThread(1, FileName, this, (i + 1));      //4.9..14                      
                            DBThreadArrayNew[i, j].ThreadNumber = j + 1;
                        }
                    }
                    else
                    {
                        MessageBox.Show(FileName + " Does not exist in the current folder");
                        return;
                    }
                }

                UserArray = new ITInteractiveStructures.TThreadUserRecord[MaxThreads];
                for (int i = 0; i < MaxThreads; i++)
                {
                    UserArray[i].User = "";
                    UserArray[i].Exch = ' ';
                    UserArray[i].ScCode = 0;
                    UserArray[i].InUse = false;

                }
                //LoadNseDerivativeMaster("NseDerivMaster.Dat");
                CurrencyLotToQtyMapBSECURR = new Hashtable();//bseCurr
                ArrayLotQtyBseCurr = new ITInteractiveStructures.TLotQuantityRecord[0];
                BseCurrDeriv = Convert.ToBoolean(ConfigurationSettings.AppSettings["BseCurrDeriv"]);
                if (BseCurrDeriv == true)
                {
                    LoadBseDerivativeMaster("BseCurrDerivMaster.Dat");
                }
                DBRequestCnt = 0;
                DBReplyCnt = 0;
                Ex1Cnt = 0;
                Ex2Cnt = 0;
                Ex3Cnt = 0;
                ExchConnectorSoc1 = new SocketClient();
                ExchConnectorSoc2 = new SocketClient();

                McastOTS = new MCastSocketClass(textBoxMcastGrpIP.Text, textBoxMcastGrpPort.Text, this);
                McastOTS.Join();
                ListenToITS = new TWSocketServer();
                ListenToITS.OnClientConnect += new TWSocketClientConnectEvent(OnITSClientConnect);
                ListenToITS.OnBgException += new OverbyteIcsWSocket.TBgExceptionEvent(Client_OnBgException);

                #region LotQty
                ArrayLotQty = new ITInteractiveStructures.TLotQuantityRecord[0];
                
                #endregion

                CurrencyLotToQtyMap = new Hashtable();
                string Fname = "CurrencyLotInfoOTS.Csv";
                if (!File.Exists(Fname))
                {
                    MessageBox.Show(Fname + " is not available.\nQty to Lot mapping for currency order and trade reports will fail.");
                }
                else
                {
                    int k = 0;
                    string[] Data = File.ReadAllLines(Fname);
                    for (int i = 1; i < Data.Length; i++)
                    {
                        string[] Parts = Data[i].Split(',');
                        CurrencyLotToQtyMap[Parts[0]] = Parts[1];

                        #region LotQty
                        Array.Resize(ref  ArrayLotQty, ArrayLotQty.Length + 1);
                        ArrayLotQty[k].ScName = Parts[0].PadRight(12).ToCharArray();
                        ArrayLotQty[k].ScNameLength = Convert.ToByte(Parts[0].Length);
                        ArrayLotQty[k].Qty = Convert.ToInt32(Parts[1]);
                        k++;
                        #endregion
                    }
                }
                buttonStartUp.Enabled = false;
                timer1.Enabled = true;
                timer2.Enabled = true;
                AddDBDateRequest();
                textBoxRetryAttemptsCnt.Enabled = false;
            }
            catch (Exception er)
            {
                ErrorLog(er);
            }
        }

        private void AddDBDateRequest()
        {
            try
            {
                ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();
                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserID = "A".PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderID = " ".PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 73;
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDummyRecord());
                OTSReqRec.AllReq.LicenceInfoRequestRec.Dummy = "000".PadRight(15).ToCharArray();
                PendingRequestList.Add(OTSReqRec);
                DBRequestCnt++;
                SendPendingRequestFromList();
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        private string GetBrokerName(int BrokerCode)
        {
           
                string BrokerName = "";
                foreach (BrokerNameRecords BrokerRec in Enum.GetValues(typeof(BrokerNameRecords)))
                {
                    if ((int)BrokerRec != BrokerCode) continue;
                    BrokerCodeOTS = BrokerRec;
                    break;
                }
                switch ((BrokerNameRecords)BrokerCode)
                {
                    case BrokerNameRecords.BROKER_VENTURA: BrokerName = "Ventura Securities";
                        break;
                    case BrokerNameRecords.BROKER_IN_NIVESH: BrokerName = "India Nivesh Securities";
                        break;
                    case BrokerNameRecords.BROKER_KIFS: BrokerName = "KiFS Securities";
                        break;
                    case BrokerNameRecords.BROKER_LKP: BrokerName = "LKP Securities";
                        break;
                    case BrokerNameRecords.BROKER_MONARCH_NETWORTH: BrokerName = "Monarch NetWorth";
                        break;
                    case BrokerNameRecords.BROKER_PINNACLE: BrokerName = "Pinnacle Trades";
                        break;
                    case BrokerNameRecords.BROKER_SUNLIGHT: BrokerName = "Sunlight Broking";
                        break;
                    case BrokerNameRecords.BROKER_SWASTIKA: BrokerName = "Swastika Investmart";
                        break;
                    case BrokerNameRecords.BROKER_PURE: BrokerName = "Pure Broking";
                        break;
                    case BrokerNameRecords.BROKER_NIRMAN: BrokerName = "Nirman Broking";
                        break;
                    case BrokerNameRecords.BROKER_SHAH_INVESTORS: BrokerName = "Shah Investors";
                        break;
                    case BrokerNameRecords.BROKER_FAIRWEALTH: BrokerName = "FairWealth";
                        break;
                    case BrokerNameRecords.BROKER_INDIRA: BrokerName = "IndiraSecurities";
                        break;
                    case BrokerNameRecords.BROKER_ASTHA: BrokerName = "Astha Securities";
                        break;
                    case BrokerNameRecords.BROKER_EMKAY: BrokerName = "Emkay Global";
                        break;
                    case BrokerNameRecords.BROKER_LKPTRADESMART: BrokerName = "TradeSmart@LKP";
                        break;
                    case BrokerNameRecords.BROKER_AMBALAL: BrokerName = "Ambalal Shares";
                        break;
                    case BrokerNameRecords.BROKER_INDOTHAI: BrokerName = "Indo Thai Securities";
                        break;
                    case BrokerNameRecords.BROKER_LATIN: BrokerName = "Latin Manharlal"; break; //02.01.18 Priyanka added BrokerCode
                    case BrokerNameRecords.BROKER_PUMARTH: BrokerName = "ShopStock"; break;
                    case BrokerNameRecords.BROKER_F6: BrokerName = "F6 Online"; break;
                    case BrokerNameRecords.BROKER_LAXMISHREE: BrokerName = "Varahi"; break;
                    case BrokerNameRecords.BROKER_MANGALKESHAV: BrokerName = "MangalTraders"; break;
                    case BrokerNameRecords.BROKER_Aranox: BrokerName = "Aranox Commodities"; break;
                    case BrokerNameRecords.BROKER_PlusWealth: BrokerName = "PlusWealth"; break;
                    case BrokerNameRecords.BROKER_MSEFSL: BrokerName = "MSEFSL"; break;
                    case BrokerNameRecords.BROKER_TradeJini: BrokerName = "TradeJini"; break;
                    case BrokerNameRecords.BROKER_AJMERA: BrokerName = "Ajmera Securities"; break;
                    case BrokerNameRecords.BROKER_MARFATIA: BrokerName = "Marfatia Stock Broking"; break;
                    case BrokerNameRecords.BROKER_PROSTOCKS: BrokerName = "Prostocks"; break;
                    case BrokerNameRecords.BROKER_JHAVERI: BrokerName = "Jhaveri Securities"; break;
                    case BrokerNameRecords.BROKER_ACHINTYA: BrokerName = "Achintya Securities"; break;
                    default: BrokerName = "Unknown";
                        break;
                }
                return BrokerName;
            
        }

        private void SendPendingRequestFromList()
        {
            try
            {
                TOTSThread DB;
                int ThNum, NextReqNum;
                ITInteractiveStructures.TOTSRequestRecord CurrReq = new ITInteractiveStructures.TOTSRequestRecord();
                ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();

                if (checkBoxWriteToDisplayThreads.Checked)
                    listBox1.Items.Insert(0, "Items in Pending Request List :" + PendingRequestList.Count);

                for (int j = 0; j < MaxDB; j++)
                {
                    while (PendingRequestList.Count > 0)
                    {
                        ThNum = -1;
                        DB = null;
                        for (int i = 0; i < MaxThreads; i++)
                        {
                            if (DBThreadArrayNew[j, i].Status == TThreadStatus.Running)
                            {
                                continue;
                            }
                            if (DBThreadArrayNew[j, i].Status == TThreadStatus.Suspended)
                            {
                                DB = DBThreadArrayNew[j, i];
                                ThNum = i;
                                break;
                            }
                        }
                        if (ThNum == -1)
                            break;
                        NextReqNum = GetNextReqNumForDB(j + 1);
                        if (NextReqNum == -1)
                            break;
                        CurrReq = PendingRequestList[NextReqNum];
                        CurrReq.ThreadStartTime = DateTime.Now;
                        OTSReqRec = CurrReq; //27.08.14 To Avoid Multiple assignments.                

                        DB.OTSReplyRec.IP = new string(CurrReq.IP).PadRight(15).ToCharArray();
                        DB.OTSReplyRec.IPLength = CurrReq.IPLength;
                        DB.OTSReplyRec.DB_Num = j + 1;
                        if (DB.OTSReplyRec.DB_Num == 1)
                            DB1Req++;
                        else
                            DB2Req++;

                        DB.OTSReplyRec.ReplyHdr.SocketNum = CurrReq.ReqHdr.SocketNum;
                        DB.OTSReplyRec.ReplyHdr.ClientType = CurrReq.ReqHdr.ClientType;
                        DB.OTSReplyRec.ReplyHdr.TraderID = new string(CurrReq.ReqHdr.TraderID).PadRight(14).ToCharArray();
                        DB.OTSReplyRec.ReplyHdr.TraderIDLength = CurrReq.ReqHdr.TraderIDLength;
                        DB.OTSReplyRec.ReplyHdr.FEUserID = new string(CurrReq.ReqHdr.FEUserID).PadRight(15).ToCharArray();
                        DB.OTSReplyRec.ReplyHdr.FEUserIDLength = CurrReq.ReqHdr.FEUserIDLength;
                        DB.OTSReplyRec.ReplyHdr.AccountType = CurrReq.ReqHdr.AccountType;
                        DB.OTSReplyRec.ReplyHdr.ReplyType = CurrReq.ReqHdr.RequestType;
                        if (CurrReq.ReqHdr.RequestType == 68)
                            DB.TimeOutFlag = true;
                        DB.OTSReplyRec.ReplyHdr.ChannelID_NC = CurrReq.ReqHdr.ChannelID_NC;
                        DB.OTSReplyRec.ReplyHdr.ChannelID_ND = CurrReq.ReqHdr.ChannelID_ND;
                        DB.OTSReplyRec.ReplyHdr.ChannelID_BC = CurrReq.ReqHdr.ChannelID_BC;
                        DB.OTSReplyRec.ReplyHdr.ReplyLen = 0;
                        DB.OTSReplyRec.ReplyHdr.ReplyCnt = 0;
                        DB.OTSReplyRec.ReplyHdr.ErrorCode = 0;
                        DB.OTSReplyRec.ErrorString = "";
                        DB.OTSReplyRec.AllReply = null;
                        DB.OTSReplyRec.ReplyCount = 0;
                        DB.OTSReplyRec.AllReplyNew = null;
                        DB.OTSReplyRec.NewReplyHdr = null;
                        DB.OTSReplyRec.ReportReplyCount = 0;
                        DB.MaxDeadlockRetryCnt = Convert.ToUInt16(textBoxRetryAttemptsCnt.Text);
                        DB.OTSReplyRec.ITS_Num = CurrReq.ITS_Num;
                        PendingRequestList.RemoveAt(NextReqNum);
                        if (checkBoxWriteToDisplayThreads.Checked)
                            listBox1.Items.Insert(0, "Calling Thread " + ThNum);
                        ThreadsInUseCnt++;
                        if (new string(DB.OTSReplyRec.ReplyHdr.TraderID).Substring(0, DB.OTSReplyRec.ReplyHdr.TraderIDLength) != "")
                            UserArray[ThNum].User = new string(DB.OTSReplyRec.ReplyHdr.TraderID).Substring(0, DB.OTSReplyRec.ReplyHdr.TraderIDLength);
                        else
                            UserArray[ThNum].User = new string(DB.OTSReplyRec.ReplyHdr.FEUserID).Substring(0, DB.OTSReplyRec.ReplyHdr.FEUserIDLength);

                        UserArray[ThNum].InUse = true;
                        DB.ThreadStartTick = DateTime.Now.Ticks;
                        DB.EnqueueTask(OTSReqRec);
                    }
                }
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        private int GetNextReqNumForDB(int DBNo)
        {
                #region Variable Declaration
                int Result, UserOldThread, DB2;
                string TraderID;
                ITInteractiveStructures.TOTSRequestRecord CurrReq = new ITInteractiveStructures.TOTSRequestRecord();
                bool flUserPresent;
                #endregion

                Result = -1;
                for (int i = 0; i < PendingRequestList.Count; i++)
                {
                    CurrReq = PendingRequestList[i];
                    TraderID = new string(CurrReq.ReqHdr.TraderID).TrimEnd();
                    if (TraderID == "")
                        TraderID = new string(CurrReq.ReqHdr.FEUserID).TrimEnd();

                    //Code added on 12/08/2010 to force single ots request per user to db
                    flUserPresent = false;
                    UserOldThread = -1;
                    for (int j = 0; j < MaxThreads; j++)
                    {
                        if (!UserArray[j].InUse)
                            continue;
                        if (UserArray[j].User.TrimEnd(TrimChars) != TraderID)
                            continue;
                        flUserPresent = true;
                        UserOldThread = j;
                        break;
                    }
                    if (flUserPresent)
                    {
                        if (checkBox7.Checked)
                            listBox3.Items.Add(DateTime.Now.ToString("hh:mm:ss") + " " + TraderID + " is already using thread " + UserOldThread);
                        continue;
                    }
                    //upto this on 12/08/2010
                    DB2 = GetDBNo(TraderID);
                    if (DB2 != DBNo)
                        continue;
                    Result = i;
                    break;

                }
                return Result;
            
        }

        private int GetDBNo(string TraderID)
        {
                int Result;
                Result = 1;
                if (MaxDB == 1)
                    Result = 1;
                else
                    if (TraderID.Length <= 0)
                        Result = 1;
                    else
                        if (TraderID == "db_one")
                            Result = 1;
                        else
                            if (TraderID == "db_two")
                            {
                                Result = 2;
                            }
                            else
                                Result = CommonFunctions.BinSearchClient(ref ArrayClientDbNum, TraderID);

                return Result;
           
        }

        private void LoadBseDerivativeMaster(string FileName)//bseCurr
        {
            try
            {
                #region Variable Declaration

                long ScripCnt;
                ITInteractiveStructures.TScripDerivRecord ScripDerivRec = new ITInteractiveStructures.TScripDerivRecord();
                string MasterFileName, ScripName;
                FileStream ScripFile;
                #endregion

                MasterFileName = FileName;
                if (!File.Exists(MasterFileName))
                {
                    MessageBox.Show(MasterFileName + " is missing. Not Possible to Process After Hours Deriv Orders");
                    return;
                }
                ScripFile = new FileStream(MasterFileName, FileMode.Open, FileAccess.Read);
                ScripCnt = ScripFile.Length / Marshal.SizeOf(ScripDerivRec);
                DerivMasterInfo = new ITInteractiveStructures.TScripDerivRecord[ScripCnt];
                int k = 0;
                for (int i = 0; i < ScripCnt; i++)
                {
                    DerivMasterInfo[i] = ReadStruct<ITInteractiveStructures.TScripDerivRecord>(ScripFile);
                    ScripName = new string(DerivMasterInfo[i].name).Substring(0, DerivMasterInfo[i].Length);
                    if (CurrencyLotToQtyMapBSECURR.ContainsKey(ScripName)) continue;
                    CurrencyLotToQtyMapBSECURR[ScripName] = DerivMasterInfo[i].MinimumLotQuantity;
                    #region LotQty
                    Array.Resize(ref  ArrayLotQtyBseCurr, ArrayLotQtyBseCurr.Length + 1);
                    ArrayLotQtyBseCurr[k].ScName = ScripName.PadRight(12).ToCharArray();
                    ArrayLotQtyBseCurr[k].ScNameLength = Convert.ToByte(DerivMasterInfo[i].Length);
                    ArrayLotQtyBseCurr[k].Qty = Convert.ToInt32(DerivMasterInfo[i].MinimumLotQuantity);
                    k++;
                    #endregion

                }
                ScripFile.Close();
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        public static T ReadStruct<T>(FileStream fs)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return temp;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Counter % 5 == 0)
                {
                    MemoryHelper.ClearMemory();
                    Counter = 0;
                }
                UpdateStatusBar();
                SendPendingRequestFromList();
                SendPendingReplyFromList();
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        private void UpdateStatusBar()
        {
            try
            {
            int count = 0;
            TWSocketClient client;

            for (int i = 0; i < ListenToITS.FClientList.Count; i++)
            {
                client = (TWSocketClient)ListenToITS.FClientList[i];
                if (client == null) continue;
                count += client.SockBuf.BufUsed;
            }
           
                Counter++;
                toolStripStatusLabelClientBuff.Text = count.ToString();
                toolStripStatusLblFrmITS.Text = BytesFromITS.ToString();
                toolStripStatusLblToITS.Text = BytesToITS.ToString();
                toolStripStatusLblToEC2.Text = BytesToEC2.ToString();
                toolStripStatusLblFrmEC2.Text = BytesFromEC2.ToString();
                if (ExchConnectorSoc2 != null && ExchConnectorSoc2.SockBufExchConn != null)
                    toolStripStatusLabelEC2BuffUsed.Text = ExchConnectorSoc2.SockBufExchConn.GetBuffUsed().ToString();

                toolStripStatusLblNow.Text = DateTime.Now.ToString("hh:mm:ss");
                toolStripStatusLblEC1Time.Text = BytesFromEC1Time.ToString("hh:mm:ss");
                toolStripStatusLbl.Text = Math.Round((DateTime.Now - BytesFromEC1Time).TotalSeconds).ToString();
                toolStripStatusLblToEC1.Text = BytesToEC1.ToString();
                toolStripStatusLblFrmEC1.Text = BytesFromEC1.ToString();
                if (ExchConnectorSoc1 != null && ExchConnectorSoc1.SockBufExchConn != null)
                {
                    toolStripStatusLabelEC1BuffUsed.Text = ExchConnectorSoc1.SockBufExchConn.GetBuffUsed().ToString();
                }
                toolStripStatusLblDBReqCnt.Text = DBRequestCnt.ToString();
                toolStripStatusLabel2.Text = Ex1Cnt.ToString();
                toolStripStatusLabel3.Text = Ex2Cnt.ToString();
                toolStripStatusLabel4.Text = Ex3Cnt.ToString();
                toolStripStatusLblDBReplyCnt.Text = DBReplyCnt.ToString();
                toolStripStatusLabelfreeThread1.Text = GetFreeThreadCnt(0).ToString();
                if (MaxDB == 2)
                    toolStripStatusLabelFreeThread2.Text = GetFreeThreadCnt(1).ToString();
                else
                    toolStripStatusLabelFreeThread2.Text = "0";
                textBoxITSCnt.Text = ListenToITS.FClientList.Count.ToString(); // not confirmed.
                textBoxPrendingRequest.Text = PendingRequestList.Count.ToString();
                toolStripStatusLabelDB1Cnt.Text = DB1Req.ToString();
                toolStripStatusLabelDB2Cnt.Text = DB2Req.ToString();
            }
            catch (Exception ee)
            {
                ErrorLog(ee);
            }
        }

        private void buttonListenITS_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Startup()) return;
                ListenToITS.Addr = "0.0.0.0";
                ListenToITS.Port = "18101";
                ListenToITS.MaxClients = 0;
                ListenToITS.Banner = "";
                ListenToITS.BannerTooBusy = "";
                ListenToITS.ListenBacklog = 200;
                ListenToITS.LingerOnOff = OverbyteIcsWSocket.TSocketLingerOnOff.wsLingerOff;
                ListenToITS.Listen();
                StatusLabelConnectedImg.Image = ImgGreen.Image;
                buttonListenITS.Enabled = false;
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        private void OnITSClientConnect(object Sender, TWSocketClient Client, ushort Error)
        {
            Client.IPAddress = Client.PeerAddr;
            Client.SockBuf = new OverbyteIcsWSockBuf.TIcsBuffer(1024 * 200);
            Client.SockCreateTime = DateTime.Now;

            Client.OnDataAvailable += new OverbyteIcsWSocket.TDataAvailable(ClientDataIn);
            Client.OnSessionClosed += new OverbyteIcsWSocket.TSessionClosed(Client_OnSessionClosed);
            Client.OnBgException += new OverbyteIcsWSocket.TBgExceptionEvent(Client_OnBgException);
        }

        private void Client_OnBgException(object Sender, Exception E, ref bool CanClose)
        {
            Form1.ErrorLog(E);
            CanClose = false;
        }

        private void Client_OnSessionClosed(object Sender, ushort ErrCode)
        {
            TWSocketClient client;
            client = Sender as TWSocketClient;
            ListenToITS.FClientList.Remove(client);
            client.SockBuf.Free();
            client.SockDestroyTime = DateTime.Now;
            listBox1.Items.Add(client.IPAddress + " Disconnected at :" + client.SockDestroyTime);

        }

        bool Startup()
        {
            bool status = true;
            if (buttonStartUp.Enabled)
            {
                MessageBox.Show("Please Click the StartUp Button first", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                status = false;
            }
            return status;
        }

        public static void ErrorLog(Exception e)
        {
            try
            {
                bool bReturnLog = false;
                Logger.ErrorLog.LogFilePath = "eLog\\OTSErrorLogFile" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                //false for writing log entry to customized text file
                bReturnLog = Logger.ErrorLog.ErrorRoutine(false, e);
                if (false == bReturnLog) { }
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        public void ThreadTerminationHandler(DBResult DB)
        {
            #region Variable Declaration
            int ThNum, DBNo, x, C1, C2, LicMax, LicUsed, Balance, ExType;
            string str, LicType, RqStr, ExStr;
            char AfterHours; List<byte> SendData = new List<byte>();
            bool DeadLockRetry, flHasHold, SendReplyToClient = true;
            ITInteractiveStructures.TDbInfoRecord DbInfoRec = new ITInteractiveStructures.TDbInfoRecord();
            ITInteractiveStructures.TOTSReplyRecord OTSReplyRec = new ITInteractiveStructures.TOTSReplyRecord();
            ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();
            ITInteractiveStructures.TOTSReplyRecord OTSReplyRecForEC = new ITInteractiveStructures.TOTSReplyRecord(); //30.12.14
            #endregion

            try
            {
                DBReplyCnt++;
                ThreadsInUseCnt--;
                ThNum = DB.ThreadNumber;
                str = ThNum + "," + DB.OTSReqRec.ThreadStartTime.ToString("hh:mm:ss:fff") + "," + DateTime.Now.ToString("hh:mm:ss:fff") + "," + DB.OTSReplyRec.ReplyHdr.ReplyType;

                if (checkBoxWriteToDisplayThreads.Checked)
                    listBox1.Items.Insert(0, str);

                OTSReplyRec = DB.OTSReplyRec;
                OTSReqRec = DB.OTSReqRec;
                AfterHours = DB.AfterHours;
                ExType = DB.ExType;
                ExStr = DB.ExStr;
                RqStr = DB.RqStr;
                DBNo = DB.DBNo;
                //DBThreadArrayNew[DBNo - 1, ThNum - 1].Status = TThreadStatus.Suspended; //30.8.14 //4.9.14

                DeadLockRetry = DB.DeadlockRetry;
                UserArray[ThNum - 1].User = "";
                UserArray[ThNum - 1].Exch = ' ';
                UserArray[ThNum - 1].ScCode = 0;
                UserArray[ThNum - 1].InUse = false;

                if (DBNo == 2)
                {
                    DbInfoRec.CurrTime = DateTime.Now;
                    DbInfoRec.ThNum = ThNum;
                    DbInfoRec.PendReqCnt = PendingRequestList.Count;
                    DbInfoRec.ThreadsInUse = ThreadsInUseCnt;
                    DbInfoRec.ReqType = DB.OTSReplyRec.ReplyHdr.ReplyType;
                    DbInfoRec.TimeTaken = Convert.ToInt32(DateTime.Now.Ticks - DB.ThreadStartTick);
                    DbInfoRec.Status = ExType;
                    DbInfoRec.ReqStrLength = (byte)RqStr.Length;
                    DbInfoRec.ReqStr = RqStr;
                    if (ExStr == null) ExStr = "";//30.8.14
                    DbInfoRec.ExStrLength = (byte)ExStr.Length;
                    DbInfoRec.ExStr = ExStr;
                    if (DbInfoRec.ReqType == 1)
                        DbInfoRec.ReqStr = DbInfoRec.ReqStr.Substring(0, 30);
                    str = DbInfoRec.CurrTime.ToString("hh:mm:ss:fff") + "|" + ThNum + "|" + DbInfoRec.PendReqCnt + "|" + DbInfoRec.ThreadsInUse
                        + "|" + DbInfoRec.ReqType + "|" + DbInfoRec.TimeTaken + "|" + ExType + "|" + DbInfoRec.ReqStr.TrimEnd() + "|" + DbInfoRec.ExStr.TrimEnd();
                    SLReport.Add(str);
                }
                // DBThreadArray[DB.DBNo-1, DB.ThreadNumber-1].t.Suspend();
                //DB.t.Suspend();
                if (ExType != 0)
                {
                    if (ExType == 101)
                    {
                        if (DeadLockRetry)
                        {
                            OTSReqRec.RetryCnt = (ushort)(OTSReqRec.RetryCnt + 1);
                            Ex1Cnt++;
                            AddToPendingRequestList(OTSReqRec);
                            return;
                        }
                        else
                        {
                            Ex2Cnt++;
                            WriteExToFile2(OTSReqRec, RqStr, ExStr, 101);
                        }
                    }
                    else
                        if (ExType == 104)
                        {
                            try
                            {
                                Ex3Cnt++;
                                WriteExToFile2(OTSReqRec, RqStr, ExStr, 104);
                            }
                            catch (Exception er)
                            {
                            }
                        }
                }
                switch (OTSReplyRec.ReplyHdr.ReplyType)
                {
                    case 1:
                        {
                            if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                            {
                                SendReplyToClient = false;
                            }
                            OTSReplyRec.AllReply[0].LoginReply.DbNum = (byte)OTSReplyRec.DB_Num;

                            if (OTSReplyRec.ErrorString == "" && OTSReplyRec.ReplyHdr.ReplyCnt == 1)
                            {
                                str = "LOGIN|" + new string(OTSReplyRec.AllReply[0].LoginReply.FEUserID).Substring(0, OTSReplyRec.AllReply[0].LoginReply.FEUserIDLength) + "|" + new string(OTSReplyRec.IP).Substring(0, OTSReplyRec.IPLength) + "|" + OTSReplyRec.ReplyHdr.SocketNum + "|" +
                                    OTSReplyRec.ITS_Num;
                                byte[] sendBytes = Encoding.ASCII.GetBytes(str);
                                McastOTS.Sender.BeginSend(sendBytes, sendBytes.Length, McastOTS.remoteep, new AsyncCallback(SendCallback1), McastOTS.Sender);
                            }
                        }
                        break;
                    case 3:
                        {
                            if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                            {
                                SendReplyToClient = false;
                            }
                            OTSReplyRec.AllReply[0].LoginReplyNew.DbNum = (byte)OTSReplyRec.DB_Num;

                            if (OTSReplyRec.ErrorString == "" && OTSReplyRec.ReplyHdr.ReplyCnt == 1)
                            {
                                str = "LOGIN|" + new string(OTSReplyRec.AllReply[0].LoginReplyNew.FEUserID).Substring(0, OTSReplyRec.AllReply[0].LoginReplyNew.FEUserIDLength) + "|" + new string(OTSReplyRec.IP).Substring(0, OTSReplyRec.IPLength) + "|" + OTSReplyRec.ReplyHdr.SocketNum + "|" +
                                    OTSReplyRec.ITS_Num;
                                byte[] sendBytes = Encoding.ASCII.GetBytes(str);
                                McastOTS.Sender.BeginSend(sendBytes, sendBytes.Length, McastOTS.remoteep, new AsyncCallback(SendCallback1), McastOTS.Sender);
                            }
                        }
                        break;
                    case 85:
                        {
                            if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                            {
                                SendReplyToClient = false;
                            }
                            OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.DbNum = (byte)OTSReplyRec.DB_Num;

                            //Form1.Log2FA("Reply(ITS) : " + JsonSerializer.Serialize(OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord));

                            if (OTSReplyRec.ErrorString == "" && OTSReplyRec.ReplyHdr.ReplyCnt == 1)
                            {
                                str = "LOGIN|" + new string(OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserID).Substring(0, OTSReplyRec.AllReply[0].twoFactorLoginReplyRecord.FEUserIDLength) + "|" + new string(OTSReplyRec.IP).Substring(0, OTSReplyRec.IPLength) + "|" + OTSReplyRec.ReplyHdr.SocketNum + "|" +
                                    OTSReplyRec.ITS_Num;
                                byte[] sendBytes = Encoding.ASCII.GetBytes(str);
                                McastOTS.Sender.BeginSend(sendBytes, sendBytes.Length, McastOTS.remoteep, new AsyncCallback(SendCallback1), McastOTS.Sender);
                            }


                        }
                        break;
                    case 4:
                        {
                            if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                            {
                                SendReplyToClient = false;
                            }

                            if (OTSReplyRec.ErrorString == "" && OTSReplyRec.ReplyHdr.ReplyCnt == 1)
                            {
                                str = "LOGIN|" + new string(OTSReplyRec.AllReply[0].LoginReplyWithName.FEUserID).Substring(0, OTSReplyRec.AllReply[0].LoginReplyWithName.FEUserIDLength) + "|" + new string(OTSReplyRec.IP).Substring(0, OTSReplyRec.IPLength) + "|" + OTSReplyRec.ReplyHdr.SocketNum + "|" +
                                    OTSReplyRec.ITS_Num;
                                byte[] sendBytes = Encoding.ASCII.GetBytes(str);
                                McastOTS.Sender.BeginSend(sendBytes, sendBytes.Length, McastOTS.remoteep, new AsyncCallback(SendCallback1), McastOTS.Sender);
                            }
                        }
                        break;
                    case 10:
                    case Constants.OTS_ORDERTYPE_PLACE_REQ:
                    case Constants.OTS_ORDERTYPE_PLACE_REQ_COVER: //171   // sumitp 28_jan_22
                    case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                    case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                    case Constants.OTS_ORDERTYPE_CONVERSION:
                    case Constants.OTS_ORDERTYPE_DERIVCONVERSION:
                    case 20:
                    case 30:
                    case 110:
                    case 120:
                    case 130:
                    case 211:
                    case 221:
                    case 231:
                    case Constants.OTS_EXERCISE_PLACE_REQ:// Options Exercise
                    case Constants.OTS_EXERCISE_MODIFY_REQ:
                    case Constants.OTS_EXERCISE_CANCEL_REQ:
                    case 191:
                    case 192:
                    case 193:
                    case 235: //21.11.18 SLBM Order Place
                    case 236: //21.11.18 SLBM Order Modify
                    case 237: //21.11.18 SLBM Order Cancel
                    //case Constants.OTS_OFS_PLACE_BROKER_REQ:
                    //case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                    //case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                        {
                            if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                                SendReplyToClient = false;
                            if (AfterHours == 'N' && OTSReplyRec.ErrorString.Substring(0, OTSReplyRec.ErrorStringLength) == "" && OTSReplyRec.ReplyHdr.ReplyCnt == 1)
                            {
                                if (checkBoxUseNewECLogic.Checked)
                                    ECNo = OTSReplyRec.DB_Num;
                                else
                                    ECNo = GetECNo(new string(OTSReplyRec.ReplyHdr.TraderID).Substring(0, OTSReplyRec.ReplyHdr.TraderIDLength));
                                if (ECNo == 1)
                                {
                                    if (ExchConnectorSoc1.Connected)
                                    {
                                        BytesToEC1++;
                                        if (OTSReplyRec.ReplyHdr.ReplyType == Constants.OTS_ORDERTYPE_PLACE_REQ ||
                                            OTSReplyRec.ReplyHdr.ReplyType == Constants.OTS_ORDERTYPE_MODIFY_REQ ||
                                            OTSReplyRec.ReplyHdr.ReplyType == 191 ||
                                            OTSReplyRec.ReplyHdr.ReplyType == 192 ||
                                            OTSReplyRec.ReplyHdr.ReplyType == 193 ||
                                            OTSReplyRec.ReplyHdr.ReplyType == Constants.OTS_ORDERTYPE_CANCEL_REQ ||
                                            OTSReplyRec.ReplyHdr.ReplyType  == 122)
                                        {
                                            DealerTypeList.Add(OTSReplyRec); //15.10.2016 Hetal
                                            OTSReplyRecForEC = ConvertOrderTypeToNormalOrderForEC(OTSReplyRec, OTSReplyRec.ReplyHdr.ReplyType);
                                            ExchConnectorSoc1.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRecForEC.ReplyHdr));
                                            SendData.AddRange(ConvertStructureToByteArr(OTSReplyRecForEC.AllReply[0], OTSReplyRecForEC.ReplyHdr.ReplyType));
                                            SendData.AddRange(BitConverter.GetBytes(OTSReplyRecForEC.AllReply[0].LastActReference.LastActivityReference));
                                            ExchConnectorSoc1.clientSocket.Send(SendData.ToArray());
                                        }
                                        else if (OTSReplyRec.ReplyHdr.ReplyType == 171) //14.1.15
                                        {
                                            DealerTypeList.Add(OTSReplyRec); //15.10.2016 Hetal
                                            OTSReplyRecForEC = ConvertOrderTypeToNormalOrderForEC(OTSReplyRec, OTSReplyRec.ReplyHdr.ReplyType, 1);
                                            ExchConnectorSoc1.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRecForEC.ReplyHdr));
                                            SendData.AddRange(ConvertStructureToByteArr(OTSReplyRecForEC.AllReply[0], OTSReplyRecForEC.ReplyHdr.ReplyType));
                                            SendData.AddRange(BitConverter.GetBytes(OTSReplyRecForEC.AllReply[0].LastActReference.LastActivityReference));
                                            ExchConnectorSoc1.clientSocket.Send(SendData.ToArray());

                                            //if (SendReplyToClient)
                                            //{
                                            //    AddToPendingReplyList(OTSReplyRec);
                                            //}
                                            //SendPendingRequestFromList();

                                            SendData.Clear();

                                            DealerTypeList.Add(OTSReplyRec); //15.10.2016 Hetal
                                            OTSReplyRecForEC = ConvertOrderTypeToNormalOrderForEC(OTSReplyRec, OTSReplyRec.ReplyHdr.ReplyType, 2);
                                            ExchConnectorSoc1.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRecForEC.ReplyHdr));
                                            SendData.AddRange(ConvertStructureToByteArr(OTSReplyRecForEC.AllReply[0], OTSReplyRecForEC.ReplyHdr.ReplyType));
                                            SendData.AddRange(BitConverter.GetBytes(OTSReplyRecForEC.AllReply[0].LastActReference.LastActivityReference));
                                            ExchConnectorSoc1.clientSocket.Send(SendData.ToArray());

                                            //if (SendReplyToClient)
                                            //{
                                            //    AddToPendingReplyList(OTSReplyRec);
                                            //}
                                            //SendPendingRequestFromList();
                                        }
                                        else if (OTSReplyRec.ReplyHdr.ReplyType != (Constants.OTS_ORDERTYPE_CONVERSION) || OTSReplyRec.ReplyHdr.ReplyType != (Constants.OTS_ORDERTYPE_DERIVCONVERSION)) //14.1.15
                                        {
                                            DealerTypeList.Add(OTSReplyRec); //15.10.2016 Hetal
                                            ExchConnectorSoc1.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRec.ReplyHdr));
                                            SendData.AddRange(ConvertStructureToByteArr(OTSReplyRec.AllReply[0], OTSReplyRec.ReplyHdr.ReplyType));
                                            SendData.AddRange(BitConverter.GetBytes(OTSReplyRec.AllReply[0].LastActReference.LastActivityReference));
                                            ExchConnectorSoc1.clientSocket.Send(SendData.ToArray());
                                        }
                                    }

                                    else
                                    {
                                        SendReplyToClient = false;
                                        UnableToProcessRequest(OTSReplyRec);
                                        ReconnectToExchangeConnector(1);
                                    }

                                }
                                if (ECNo == 2)
                                {
                                    if (ExchConnectorSoc2.Connected)
                                    {
                                        BytesToEC2++;
                                        ExchConnectorSoc2.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRec.ReplyHdr));
                                        ExchConnectorSoc2.clientSocket.Send(ConvertStructureToByteArr(OTSReplyRec.AllReply[0], OTSReplyRec.ReplyHdr.ReplyType));
                                    }

                                    else
                                    {
                                        SendReplyToClient = false;
                                        UnableToProcessRequest(OTSReplyRec);
                                        ReconnectToExchangeConnector(2);
                                    }
                                }
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                    case Constants.OTS_MANDATE:
                        {

                            if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                                SendReplyToClient = false;
                            if (OTSReplyRec.ErrorString.Substring(0, OTSReplyRec.ErrorStringLength) == "" && OTSReplyRec.ReplyHdr.ReplyCnt == 1)
                            {
                            if (checkBoxUseNewECLogic.Checked)
                                ECNo = OTSReplyRec.DB_Num;
                            else
                                ECNo = GetECNo(new string(OTSReplyRec.ReplyHdr.TraderID).Substring(0, OTSReplyRec.ReplyHdr.TraderIDLength));
                            if (ECNo == 1)
                            {
                                if (ExchConnectorSoc1.Connected)
                                {
                                    BytesToEC1++;

                                    DealerTypeList.Add(OTSReplyRec);
                                    ExchConnectorSoc1.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRec.ReplyHdr));
                                    ExchConnectorSoc1.clientSocket.Send(ConvertStructureToByteArr(OTSReplyRec.AllReply[0], OTSReplyRec.ReplyHdr.ReplyType));

                                }
                                else
                                {
                                    SendReplyToClient = false;
                                    UnableToProcessRequest(OTSReplyRec);
                                    ReconnectToExchangeConnector(1);
                                }

                            }
                            if (ECNo == 2)
                            {
                                if (ExchConnectorSoc2.Connected)
                                {
                                    BytesToEC2++;
                                    ExchConnectorSoc2.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRec.ReplyHdr));
                                    ExchConnectorSoc2.clientSocket.Send(ConvertStructureToByteArr(OTSReplyRec.AllReply[0], OTSReplyRec.ReplyHdr.ReplyType));
                                }

                                else
                                {
                                    SendReplyToClient = false;
                                    UnableToProcessRequest(OTSReplyRec);
                                    ReconnectToExchangeConnector(2);
                                }
                            }
                            }

                        } break;
                    //case Constants.OTS_OFS_PLACE_BROKER_REQ:
                    //case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                    //case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                    //    {
                    //        if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                    //            SendReplyToClient = false;
                    //        if (OTSReplyRec.ErrorString.Substring(0, OTSReplyRec.ErrorStringLength) == "" && OTSReplyRec.ReplyHdr.ReplyCnt == 1)
                    //        {
                    //            if (checkBoxUseNewECLogic.Checked)
                    //                ECNo = OTSReplyRec.DB_Num;
                    //            else
                    //                ECNo = GetECNo(new string(OTSReplyRec.ReplyHdr.TraderID).Substring(0, OTSReplyRec.ReplyHdr.TraderIDLength));
                    //            if (ECNo == 1)
                    //            {
                    //                if (ExchConnectorSoc1.Connected)
                    //                {
                    //                    BytesToEC1++;

                    //                    DealerTypeList.Add(OTSReplyRec);
                    //                    ExchConnectorSoc1.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRec.ReplyHdr));
                    //                    ExchConnectorSoc1.clientSocket.Send(ConvertStructureToByteArr(OTSReplyRec.AllReply[0], OTSReplyRec.ReplyHdr.ReplyType));

                    //                }
                    //                else
                    //                {
                    //                    SendReplyToClient = false;
                    //                    UnableToProcessRequest(OTSReplyRec);
                    //                    ReconnectToExchangeConnector(1);
                    //                }

                    //            }
                    //            if (ECNo == 2)
                    //            {
                    //                if (ExchConnectorSoc2.Connected)
                    //                {
                    //                    BytesToEC2++;
                    //                    ExchConnectorSoc2.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRec.ReplyHdr));
                    //                    ExchConnectorSoc2.clientSocket.Send(ConvertStructureToByteArr(OTSReplyRec.AllReply[0], OTSReplyRec.ReplyHdr.ReplyType));
                    //                }

                    //                else
                    //                {
                    //                    SendReplyToClient = false;
                    //                    UnableToProcessRequest(OTSReplyRec);
                    //                    ReconnectToExchangeConnector(2);
                    //                }
                    //            }
                    //        }
                    //    } break;
                    case 122:
                    case 123:
                    case 124:
                        {
                            if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                                SendReplyToClient = false;
                            if (OTSReplyRec.ErrorString.Substring(0, OTSReplyRec.ErrorStringLength) == "" && OTSReplyRec.ReplyHdr.ReplyCnt == 1)
                            {
                                if (checkBoxUseNewECLogic.Checked)
                                    ECNo = OTSReplyRec.DB_Num;
                                else
                                    ECNo = GetECNo(new string(OTSReplyRec.ReplyHdr.TraderID).Substring(0, OTSReplyRec.ReplyHdr.TraderIDLength));
                                if (ECNo == 1)
                                {
                                    if (ExchConnectorSoc1.Connected)
                                    {
                                        BytesToEC1++;

                                        DealerTypeList.Add(OTSReplyRec);
                                        ExchConnectorSoc1.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRec.ReplyHdr));
                                        ExchConnectorSoc1.clientSocket.Send(ConvertStructureToByteArr(OTSReplyRec.AllReply[0], OTSReplyRec.ReplyHdr.ReplyType));

                                    }
                                    else
                                    {
                                        SendReplyToClient = false;
                                        UnableToProcessRequest(OTSReplyRec);
                                        ReconnectToExchangeConnector(1);
                                    }

                                }
                                if (ECNo == 2)
                                {
                                    if (ExchConnectorSoc2.Connected)
                                    {
                                        BytesToEC2++;
                                        ExchConnectorSoc2.clientSocket.Send(CommonFunctions.RawSerialize(OTSReplyRec.ReplyHdr));
                                        ExchConnectorSoc2.clientSocket.Send(ConvertStructureToByteArr(OTSReplyRec.AllReply[0], OTSReplyRec.ReplyHdr.ReplyType));
                                    }

                                    else
                                    {
                                        SendReplyToClient = false;
                                        UnableToProcessRequest(OTSReplyRec);
                                        ReconnectToExchangeConnector(2);
                                    }
                                }
                            }
                        }
                        break;
                    case 66: //Lic Info Processing
                        {
                            SendReplyToClient = false;
                            int DealerLicenseCnt = 0;
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                if (!UserTypeArr.Contains(OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserType))
                                {
                                    MessageBox.Show(OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserCnt + " license(s) of unknown type " + OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserType +
                                        " found in Database " + DB.DBNo + "."); //21.05.15                                   
                                    ExcessLicenseUsed = true;
                                }

                                str = DB.DBNo + "," + OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserType + "," + OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserCnt;
                                listBox1.Items.Add(str);

                                if (OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserType == 5 || OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserType == 15)
                                {
                                    DealerLicenseCnt += OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserCnt;
                                }

                                switch (OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserType)
                                {
                                    case 1: dataGridViewLicense.Rows[0].Cells[2].Value = OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserCnt; break;
                                    case 2: dataGridViewLicense.Rows[1].Cells[2].Value = OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserCnt; break;
                                    case 5:
                                    case 15: dataGridViewLicense.Rows[2].Cells[2].Value = DealerLicenseCnt; break;
                                    case 16: dataGridViewLicense.Rows[3].Cells[2].Value = OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserCnt; break;
                                    case 100: dataGridViewLicense.Rows[4].Cells[2].Value = OTSReplyRec.AllReply[i].LicenceInfoReplyRec.UserCnt; break;
                                    default: break;
                                }
                            }
                            if (BrokerCodeOTS == BrokerNameRecords.BROKER_IN_NIVESH)
                            {
                                int EndClientLicenseMax = 0, EndClientLicenseUsed = 0, DealerLicenseMax = 0, DealerLicenseUsed = 0, SuperAdminLicenseMax = 0, SuperAdminLicUsed = 0;

                                EndClientLicenseMax = Convert.ToInt32(dataGridViewLicense.Rows[0].Cells[1].Value) + Convert.ToInt32(dataGridViewLicense.Rows[1].Cells[1].Value);
                                DealerLicenseMax = Convert.ToInt32(dataGridViewLicense.Rows[2].Cells[1].Value) + Convert.ToInt32(dataGridViewLicense.Rows[3].Cells[1].Value);
                                SuperAdminLicenseMax = Convert.ToInt32(dataGridViewLicense.Rows[4].Cells[1].Value);

                                EndClientLicenseUsed = Convert.ToInt32(dataGridViewLicense.Rows[0].Cells[2].Value) + Convert.ToInt32(dataGridViewLicense.Rows[1].Cells[2].Value);
                                DealerLicenseUsed = Convert.ToInt32(dataGridViewLicense.Rows[2].Cells[2].Value) + Convert.ToInt32(dataGridViewLicense.Rows[3].Cells[2].Value);
                                SuperAdminLicUsed = Convert.ToInt32(dataGridViewLicense.Rows[4].Cells[2].Value);

                                if (EndClientLicenseMax - EndClientLicenseUsed < 0)
                                {
                                    MessageBox.Show("Trader/Non FE Trader Licences Exceeded - " + Math.Abs(EndClientLicenseMax - EndClientLicenseUsed).ToString() + "\nSystem will not work. Consider purchasing License.", "License Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    buttonListenITS.Enabled = false; btnExchConnect1.Enabled = false;
                                    return;
                                }
                                if (DealerLicenseMax - DealerLicenseUsed < 0)
                                {
                                    MessageBox.Show("Dealer/Admin Licences Exceeded - " + Math.Abs(DealerLicenseMax - DealerLicenseUsed).ToString() + "\nSystem will not work. Consider purchasing License.", "License Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    buttonListenITS.Enabled = false; btnExchConnect1.Enabled = false;
                                    return;
                                }
                                if (SuperAdminLicenseMax - SuperAdminLicUsed < 0)
                                {
                                    MessageBox.Show("Super Admin Licences Exceeded - " + Math.Abs(SuperAdminLicenseMax - SuperAdminLicUsed).ToString() + "\nSystem will not work. Consider purchasing License.", "License Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    buttonListenITS.Enabled = false; btnExchConnect1.Enabled = false;
                                    return;
                                }
                                MessageBox.Show("Trader/Non FE Trader Licences Purchased = " + EndClientLicenseMax + " Licences Used = " + EndClientLicenseUsed + System.Environment.NewLine +
                                                "Dealer/Admin Licences Purchased = " + DealerLicenseMax + " Licences Used = " + DealerLicenseUsed + System.Environment.NewLine +
                                                "Super Admin Licences Purchased = " + SuperAdminLicenseMax + " Licences Used = " + SuperAdminLicUsed, "License Balance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                        }
                        break;
                    case 68:
                        {
                            SendReplyToClient = false;
                            x = ArrayClientDbNum.Length;
                            Array.Resize(ref ArrayClientDbNum, x + OTSReplyRec.ReplyCount);
                            C1 = Convert.ToInt32(labelDb1CliCnt.Text);
                            C2 = Convert.ToInt32(labelDb2CliCnt.Text);
                            for (int i = 0; i < OTSReplyRec.ReplyCount; i++)
                            {
                                ArrayClientDbNum[x + i].ClientCode = new string(OTSReplyRec.AllReply[i].ClientDbNumReplyRec.ClientCode).TrimEnd().ToCharArray();
                                if (new string(OTSReplyRec.AllReply[i].ClientDbNumReplyRec.ClientCode) == "99992828")
                                {
                                }
                                ArrayClientDbNum[x + i].DBNum = OTSReplyRec.AllReply[i].ClientDbNumReplyRec.DBNum;
                                if (ArrayClientDbNum[x + i].DBNum == 1)
                                    C1++;
                                else
                                    C2++;

                                str = new string(ArrayClientDbNum[x + i].ClientCode) + "," + ArrayClientDbNum[x + i].DBNum;
                                SLCliDbNum.Add(str);
                            }

                            OTSReqRec.AllReq.CliDBNumRequestRec.DummyByte++;
                            if (OTSReqRec.AllReq.CliDBNumRequestRec.DummyByte > 90)
                            {
                                File.WriteAllLines("CliDb.Csv", SLCliDbNum);
                                SLCliDbNum.Clear();
                                AddLicenceInfoRequest();
                            }
                            else
                            {
                                DBRequestCnt++;
                                PendingRequestList.Add(OTSReqRec);
                            }

                            labelDb1CliCnt.Text = C1.ToString();
                            labelDb2CliCnt.Text = C2.ToString();

                        }
                        break;
                    case 73:
                        {
                            if (OTSReplyRec.AllReply[0].DBDateReplyRec.DBDate != Convert.ToInt32(Math.Truncate(DateTime.Now.ToOADate())))
                            {
                                MessageBox.Show("Database Date [" + OTSReplyRec.AllReply[0].DBDateReplyRec.DBDate + "] is NOT same as OTS Server Date [" + Math.Truncate(DateTime.Now.ToOADate()) + "]");
                            }
                            else
                            {
                                AddReportRequest();
                            }
                        }
                        break;
                    case 80:
                        {
                            SendReplyToClient = false;
                            labelT2T.Text = OTSReplyRec.ReplyHdr.ReplyCnt.ToString();
                            OTSReplyRec_TradeToTrade.IP = "".PadRight(15).ToCharArray();
                            OTSReplyRec_TradeToTrade.ReplyTime = DateTime.Now;
                            OTSReplyRec_TradeToTrade.ReplyHdr = OTSReplyRec.ReplyHdr;
                            OTSReplyRec_TradeToTrade.ErrorString = "";
                            Array.Resize(ref OTSReplyRec_TradeToTrade.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                OTSReplyRec_TradeToTrade.AllReply[i].TradeToTradeReplyRec = OTSReplyRec.AllReply[i].TradeToTradeReplyRec;
                            }
                        }
                        break;
                    case 160:
                        {
                            
                            labelT2T.Text = OTSReplyRec.ReplyHdr.ReplyCnt.ToString();
                            OTSReplyRec_SurveillanceIndicatorNse.IP = "".PadRight(15).ToCharArray();
                            OTSReplyRec_SurveillanceIndicatorNse.ReplyTime = DateTime.Now;
                            OTSReplyRec_SurveillanceIndicatorNse.ReplyHdr = OTSReplyRec.ReplyHdr;
                            OTSReplyRec_SurveillanceIndicatorNse.ErrorString = "";
                            Array.Resize(ref OTSReplyRec_SurveillanceIndicatorNse.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                OTSReplyRec_SurveillanceIndicatorNse.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec = OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesNseReplyRec;
                            }
                        }
                        break;
                    case 161:
                        {                            
                            labelT2T.Text = OTSReplyRec.ReplyHdr.ReplyCnt.ToString();
                            OTSReplyRec_SurveillanceIndicatorBse.IP = "".PadRight(15).ToCharArray();
                            OTSReplyRec_SurveillanceIndicatorBse.ReplyTime = DateTime.Now;
                            OTSReplyRec_SurveillanceIndicatorBse.ReplyHdr = OTSReplyRec.ReplyHdr;
                            OTSReplyRec_SurveillanceIndicatorBse.ErrorString = "";
                            Array.Resize(ref OTSReplyRec_SurveillanceIndicatorBse.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                OTSReplyRec_SurveillanceIndicatorBse.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec = OTSReplyRec.AllReply[i].SurveillanceIndicatorMessagesBseReplyRec;
                            }
                        }
                        break;
                    case 81:
                        {
                            SendReplyToClient = false;
                            labelBNMap.Text = OTSReplyRec.ReplyHdr.ReplyCnt.ToString();
                            OTSReplyRec_BseNseMap.IP = "".PadRight(15).ToCharArray();
                            OTSReplyRec_BseNseMap.ReplyTime = DateTime.Now;
                            OTSReplyRec_BseNseMap.ReplyHdr = OTSReplyRec.ReplyHdr;
                            OTSReplyRec_BseNseMap.ErrorString = "";
                            Array.Resize(ref OTSReplyRec_BseNseMap.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                OTSReplyRec_BseNseMap.AllReply[i].BseNseMapReplyRec = OTSReplyRec.AllReply[i].BseNseMapReplyRec;
                            }
                        }
                        break;
                    case 86:
                        {
                            SendReplyToClient = checkBoxSendMFMsgToITS.Checked;
                            SL = new List<string>();
                            SL.Add("ClientCode,Admin,Blank1,MFDebi");
                            SL.Add("S476,9999,,2222");
                            SL.Add("150V018,9999,,2222");
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                flHasHold = false;
                                if (OTSReplyRec.AllReply[i].MFClientsReportReplyRec.FinalMF <= 0) continue;

                                for (int j = 0; j < ArrayMFHoldings.Length; j++)
                                {
                                    if (new string(OTSReplyRec.AllReply[i].MFClientsReportReplyRec.TraderID).Substring(0, OTSReplyRec.AllReply[i].MFClientsReportReplyRec.TraderIDLength) != new string(ArrayMFHoldings[j].TraderID).Substring(0, ArrayMFHoldings[j].TraderIDLength))
                                        continue;
                                    if (ArrayMFHoldings[j].NseCode > 25999) //deriv
                                        continue;
                                    if (ArrayMFHoldings[j].BrokerBeneQty + ArrayMFHoldings[j].ExchRcvQty - ArrayMFHoldings[j].TodaySoldQty <= 0)
                                        continue;
                                    flHasHold = true;
                                    break;
                                }
                                if (flHasHold)
                                {
                                    str = new string(OTSReplyRec.AllReply[i].MFClientsReportReplyRec.TraderID).Substring(0, OTSReplyRec.AllReply[i].MFClientsReportReplyRec.TraderIDLength) + "," +
                                        new string(OTSReplyRec.AllReply[i].MFClientsReportReplyRec.AdminID).Substring(0, OTSReplyRec.AllReply[i].MFClientsReportReplyRec.AdminIDLength) + "," +
                                        OTSReplyRec.AllReply[i].MFClientsReportReplyRec.BranchId + "," + OTSReplyRec.AllReply[i].MFClientsReportReplyRec.FinalMF.ToString("0.00") + "," +
                                         OTSReplyRec.AllReply[i].MFClientsReportReplyRec.MFDebit.ToString("0.00");
                                    listBox4.Items.Add(str);

                                    str = new string(OTSReplyRec.AllReply[i].MFClientsReportReplyRec.TraderID).Substring(0, OTSReplyRec.AllReply[i].MFClientsReportReplyRec.TraderIDLength) + "," +
                                          new string(OTSReplyRec.AllReply[i].MFClientsReportReplyRec.AdminID).Substring(0, OTSReplyRec.AllReply[i].MFClientsReportReplyRec.AdminIDLength) + ",," +
                                           OTSReplyRec.AllReply[i].MFClientsReportReplyRec.FinalMF.ToString("0.00");
                                    SL.Add(str);
                                }
                                else
                                {
                                    OTSReplyRec.AllReply[i].MFClientsReportReplyRec.TraderID = "ZZZZZZZZZZ".PadRight(14).ToCharArray(); //Do not send message as no hold
                                    OTSReplyRec.AllReply[i].MFClientsReportReplyRec.TraderIDLength = 10;
                                }
                            }
                            File.WriteAllLines("OTS_SMS_MFDebit_" + CommonFunctions.GetSelectedRadioButtonText(groupBoxDBNum) + ".Csv", SL);
                            SL.Clear();
                        }
                        break;
                    case 87:
                        {
                            SendReplyToClient = false;
                            Array.Resize(ref ArrayMFHoldings, OTSReplyRec.ReplyHdr.ReplyCnt);
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                ArrayMFHoldings[i] = OTSReplyRec.AllReply[i].MFClientHoldingReportReplyRec;
                            }
                            listBox4.Items.Add("Fetched MF Holdings : " + OTSReplyRec.ReplyHdr.ReplyCnt);
                            SendRequestForMFClients();
                        }
                        break;
                    case 82:
                        {
                            SendReplyToClient = false;
                            labelDBan.Text = OTSReplyRec.ReplyHdr.ReplyCnt.ToString();
                            OTSReplyRec_DerivBannedScrip.IP = "".PadRight(15).ToCharArray();
                            OTSReplyRec_DerivBannedScrip.ReplyTime = DateTime.Now;
                            OTSReplyRec_DerivBannedScrip.ReplyHdr = OTSReplyRec.ReplyHdr;
                            OTSReplyRec_DerivBannedScrip.ErrorString = "";
                            Array.Resize(ref OTSReplyRec_DerivBannedScrip.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                OTSReplyRec_DerivBannedScrip.AllReply[i].DerivBannedScripReplyRec = OTSReplyRec.AllReply[i].DerivBannedScripReplyRec;
                            }
                        }
                        break;
                    case 89:
                        {
                            SendReplyToClient = false;
                            labelIlliquidCnt.Text = OTSReplyRec.ReplyHdr.ReplyCnt.ToString();
                            OTSReplyRec_Illiquid.IP = "".PadRight(15).ToCharArray();
                            OTSReplyRec_Illiquid.ReplyTime = DateTime.Now;
                            OTSReplyRec_Illiquid.ReplyHdr = OTSReplyRec.ReplyHdr;
                            OTSReplyRec_Illiquid.ErrorString = "";
                            Array.Resize(ref OTSReplyRec_Illiquid.AllReply, OTSReplyRec.ReplyHdr.ReplyCnt);
                            for (int i = 0; i < OTSReplyRec.ReplyHdr.ReplyCnt; i++)
                            {
                                OTSReplyRec_Illiquid.AllReply[i].IlliquidReplyRec = OTSReplyRec.AllReply[i].IlliquidReplyRec;
                            }
                        }
                        break;
                    case 251:
                    case 252:
                    case 247:
                        {
                            SendReplyToClient = false;
                        }
                        break;
                }
                if (checkBoxWriteToDisplayDBReply.Checked)
                {
                    switch (OTSReplyRec.ReplyHdr.ReplyType)
                    {
                        case 1:
                            {
                                listBox1.Items.Insert(0, "Login Reply Received");
                            }
                            break;
                        case 2:
                            {
                                listBox1.Items.Insert(0, "Password Change Reply Received");
                            }
                            break;
                        case 10://send  to Exchange Connector
                        case Constants.OTS_ORDERTYPE_PLACE_REQ:
                        case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                        case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                        case 20:
                        case 30:
                        case 110:
                        case 120:
                        case 130:
                        case 211:
                        case 221:
                        case 231:
                        case Constants.OTS_EXERCISE_PLACE_REQ:// Options Exercise
                        case Constants.OTS_EXERCISE_MODIFY_REQ:
                        case Constants.OTS_EXERCISE_CANCEL_REQ:
                        case 191:
                        case 192:
                        case 193:
                        case 235: //21.11.18 SLBM Order Place
                        case 236: //21.11.18 SLBM Order Modify
                        case 237: //21.11.18 SLBM Order Cance
                        case Constants.OTS_OFS_PLACE_BROKER_REQ:
                        case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                        case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                        {
                                if (OTSReplyRec.ReplyHdr.ReplyCnt != 1)
                                    listBox1.Items.Insert(0, "DB Exception in RecType " + OTSReplyRec.ReplyHdr.ReplyType);
                                else
                                    if (OTSReplyRec.ErrorString == "" && ExchConnectorSoc1.clientSocket.Connected)
                                        listBox1.Items.Insert(0, "Sent RecType " + OTSReplyRec.ReplyHdr.ReplyType + " to Exch Conn");
                                    else
                                        if (!ExchConnectorSoc1.clientSocket.Connected)
                                            listBox1.Items.Insert(0, "Exch Conn is down. Rejected Request.");
                            }
                            break;
                        case 86:
                            {
                                listBox1.Items.Insert(0, "Will send MF Client List to ITS for " + OTSReplyRec.ReplyHdr.ReplyCnt + " clients");
                            }
                            break;
                    }
                }
                if (OTSReplyRec.ErrorString != "" && checkBoxWriteToDisplayDBReply.Checked)
                    listBox1.Items.Insert(0, "Error : " + OTSReplyRec.ErrorString.Substring(0, OTSReplyRec.ErrorStringLength));

                if (SendReplyToClient)
                {
                    AddToPendingReplyList(OTSReplyRec);
                }
                SendPendingRequestFromList();

            }
            catch (Exception e)
            {
                ErrorLog(e);
            }
        }

        private ITInteractiveStructures.TOTSReplyRecord ConvertOrderTypeToNormalOrderForEC(ITInteractiveStructures.TOTSReplyRecord OTSReplyRec, int OrderPlacement, int OrderCount = 0)  // sumitp 28_jan_22  last parametor added by sumit for cover
        {
            
                char OrderPlaceModify = 'P';
                ITInteractiveStructures.TOTSReplyRecord OTSReplyRecForEC = new ITInteractiveStructures.TOTSReplyRecord();
                OrderTypeList.Add(OTSReplyRec);
                OTSReplyRecForEC = OTSReplyRec;
                if (OrderPlacement == Constants.OTS_ORDERTYPE_PLACE_REQ)
                {
                    OrderPlaceModify = 'P';
                    OTSReplyRecForEC.ReplyHdr.ReplyType = GetReqTypeOnExch(OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Exch, OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ExchType, OrderPlaceModify);
                }
                else if (OrderPlacement == Constants.OTS_ORDERTYPE_PLACE_REQ_COVER)     // sumitp 28_jan_22
                {
                    OrderPlaceModify = 'B'; //Cover bracket
                    OTSReplyRecForEC.ReplyHdr.ReplyType = GetReqTypeOnExch(OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch, OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType, OrderPlaceModify);
                }
                else if (OrderPlacement == Constants.OTS_ORDERTYPE_MODIFY_REQ)
                {
                    OrderPlaceModify = 'M';
                    OTSReplyRecForEC.ReplyHdr.ReplyType = GetReqTypeOnExch(OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Exch, OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchType, OrderPlaceModify);
                }
                else if (OrderPlacement == Constants.OTS_ORDERTYPE_CANCEL_REQ)
                {
                    OrderPlaceModify = 'C';
                    OTSReplyRecForEC.ReplyHdr.ReplyType = GetReqTypeOnExch(OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Exch, OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchType, OrderPlaceModify);
                }
                else if (OrderPlacement == 191)
                {
                    OrderPlaceModify = 'P';
                    OTSReplyRecForEC.ReplyHdr.ReplyType = 191;
                }
                else if (OrderPlacement == 192)
                {
                    OrderPlaceModify = 'M';
                    OTSReplyRecForEC.ReplyHdr.ReplyType = 192;
                }
                else if (OrderPlacement == 193)
                {
                    OrderPlaceModify = 'C';
                    OTSReplyRecForEC.ReplyHdr.ReplyType = 193;
                }
                switch (OTSReplyRecForEC.ReplyHdr.ReplyType)
                {
                case 9: // Cover order Reply to EC and ITS to Pointer     // sumitp 28_jan_22
                    {
                        OTSReplyRecForEC.ReplyHdr.ReplyType = 10;

                        OTSReplyRecForEC.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecordCoverAndBracket());
                        OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecordCoverAndBracket());

                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName = new string(OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName).PadRight(12).ToCharArray();
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength;

                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket;

                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.EpochTime = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.EpochTime;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;



                        if (OrderCount == 1)
                        {
                            //OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate = 0.0f;// OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                            OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.WithSL = 'N';
                            OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;
                            OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1 = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1;
                            OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1 = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1;
                            OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1 = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1;
                        }
                        else if (OrderCount == 2)
                        {
                            //OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                            OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.WithSL = 'Y';
                            OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO;
                            OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2 = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2;
                            OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID2 = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID2;
                            OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime2 = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime2;
                        }
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderTypeNew = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderTypeNew;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked;
                        OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus;





                        OTSReplyRecForEC.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());
                        OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());

                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripName = new string(OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName).PadRight(12).ToCharArray();
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripNameLength = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength;
                        if (OrderCount == 1)
                        {
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TriggerRate = 0.0f; //OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1;

                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.WithSL = 'N';
                        }
                        else if (OrderCount == 2)
                        {

                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID2;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime2;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.WithSL = 'Y';
                        }

                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket;

                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.DisclosedQty = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus;

                        //PendingReplyList.Add(OTSReplyRec);
                        //SendPendingReplyFromList();

                    }
                    break;
                case 10:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripName = new string(OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripName).PadRight(12).ToCharArray();
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripNameLength = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripNameLength;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.DisclosedQty = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.DisclosedQty;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AHStatus;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AlgoID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AlgoID;
                    }
                    break;
                    case 20:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderModifyBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderModifyBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripName = new string(OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripName).PadRight(12).ToCharArray();
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripNameLength = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripNameLength;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.SLTriggered = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.SLTriggered;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.DisclosedQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.DisclosedQty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.OldQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.OldQty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TradedQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TradedQty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.PendingQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.PendingQty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderId;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderTime;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AHStatus;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.AlgoID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AlgoID;
                    }
                    break;
                    case 30:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderCancelBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderCancelBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripName = new string(OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripName).PadRight(12).ToCharArray();
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripNameLength = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripNameLength;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.SLTriggered = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.SLTriggered;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TradedQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TradedQty;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.PendingQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.PendingQty;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchangeOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderID;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderTime;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AHStatus;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.AlgoID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AlgoID;
                    }
                    break;

                    # region  Mcx Merging 19.01.17
                    case 101:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripName = new string(OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripName).PadRight(12).ToCharArray();
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.ScripNameLength = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripNameLength;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.DisclosedQty = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.DisclosedQty;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AHStatus;
                        }
                        break;
                    case 102:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderModifyBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderModifyBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripName = new string(OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripName).PadRight(12).ToCharArray();
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ScripNameLength = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripNameLength;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.SLTriggered = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.SLTriggered;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.DisclosedQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.DisclosedQty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.OldQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.OldQty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TradedQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TradedQty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.PendingQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.PendingQty;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderId;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderTime;
                            OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AHStatus;
                        }
                        break;
                    case 103:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderCancelBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderCancelBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripName = new string(OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripName).PadRight(12).ToCharArray();
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ScripNameLength = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripNameLength;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.SLTriggered = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.SLTriggered;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TradedQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TradedQty;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.PendingQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.PendingQty;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchangeOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderID;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderTime;
                            OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AHStatus;
                        }
                        break;

                #endregion
                case 109:    // f&O Cover order       // sumitp 28_jan_22
                    {
                        OTSReplyRecForEC.ReplyHdr.ReplyType = 110;

                        OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord());
                        OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord());

                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.Symbol = new string(OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName).PadRight(10).ToCharArray(); //CHECK HERE
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.SymbolLength = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength; //CHECK HERE
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.CALevel = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CALevel;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.CPType = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CPType;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.ExpiryDate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExpiryDate;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.InstrType = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.InstrType;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.StrikePrice = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.StrikePrice;
                        //OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.WithSL;

                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID;
                        //OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID;
                        //OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC;
                        OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus;

                        if (OrderCount == 1)
                        {
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TriggerRate = 0.0f;// OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1;

                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.WithSL = 'N';
                        }
                        else if (OrderCount == 2)
                        {
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID2;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime2;

                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.WithSL = 'Y';
                        }

                    }
                    break;
                case 110:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.Symbol = new string(OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripName).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.SymbolLength = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripNameLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.CALevel = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.CALevel;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.CPType = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.CPType;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.ExpiryDate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ExpiryDate;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.InstrType = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.InstrType;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.DerivInfo.StrikePrice = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.StrikePrice;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AHStatus;
                            OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.AlgoID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AlgoID;
                    }
                    break;
                    case 120:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderModifyBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderModifyBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.DerivInfo.Symbol = new string(OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripName).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.DerivInfo.SymbolLength = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripNameLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.DerivInfo.CALevel = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.CALevel;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.DerivInfo.CPType = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.CPType;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.DerivInfo.ExpiryDate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExpiryDate;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.DerivInfo.InstrType = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.InstrType;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.DerivInfo.StrikePrice = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.StrikePrice;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.SLTriggered = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.SLTriggered;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.OldQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.OldQty;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.TradedQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TradedQty;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.PendingQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.PendingQty;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderId;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderTime;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AHStatus;
                            OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.AlgoID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AlgoID;
                    }
                    break;
                    case 130:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderCancelBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderCancelBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.DerivInfo.Symbol = new string(OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripName).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.DerivInfo.SymbolLength = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripNameLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.DerivInfo.CALevel = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.CALevel;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.DerivInfo.CPType = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.CPType;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.DerivInfo.ExpiryDate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExpiryDate;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.DerivInfo.InstrType = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.InstrType;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.DerivInfo.StrikePrice = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.StrikePrice;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.MarginBlocked;

                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.SLTriggered = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.SLTriggered;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.OldQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.OldQty;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.TradedQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TradedQty;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.PendingQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.PendingQty;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ExchangeOrderId = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderID;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderTime;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AHStatus;
                            OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.AlgoID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AlgoID;
                    }
                    break;
                    # region Added Currency and BSE Curr Intra/Delivery 31.08.17
                    case 211:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DerivInfo.Symbol = new string(OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripName).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DerivInfo.SymbolLength = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ScripNameLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DerivInfo.CALevel = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.CALevel;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DerivInfo.CPType = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.CPType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DerivInfo.ExpiryDate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.ExpiryDate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DerivInfo.InstrType = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.InstrType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.DerivInfo.StrikePrice = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.StrikePrice;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypePlaceBrokerReplyRec.AHStatus;
                        }
                        break;
                    case 221:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderModifyBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderModifyBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DerivInfo.Symbol = new string(OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripName).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DerivInfo.SymbolLength = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ScripNameLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DerivInfo.CALevel = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.CALevel;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DerivInfo.CPType = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.CPType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DerivInfo.ExpiryDate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExpiryDate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DerivInfo.InstrType = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.InstrType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.DerivInfo.StrikePrice = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.StrikePrice;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.SLTriggered = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.SLTriggered;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.OldQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.OldQty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.TradedQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.TradedQty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.PendingQty = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.PendingQty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderId;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchangeOrderTime;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypeModifyBrokerReplyRec.AHStatus;
                        }
                        break;
                    case 231:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderCancelBrokerReplyRecord());
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderCancelBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ScripCode = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripCode;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DerivInfo.Symbol = new string(OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripName).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DerivInfo.SymbolLength = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ScripNameLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DerivInfo.CALevel = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.CALevel;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DerivInfo.CPType = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.CPType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DerivInfo.ExpiryDate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExpiryDate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DerivInfo.InstrType = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.InstrType;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.DerivInfo.StrikePrice = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.StrikePrice;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.BuySell = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BuySell;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.Qty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Qty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.Rate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.Rate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.WithSL = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.WithSL;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.TriggerRate = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TriggerRate;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.BrokerTime = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerTime;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.MarginBlocked;

                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.SLTriggered = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.SLTriggered;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.OldQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.OldQty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.TradedQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.TradedQty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.PendingQty = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.PendingQty;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ExchangeOrderId = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderID;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchangeOrderTime;
                            OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].OrderTypeCancelBrokerReplyRec.AHStatus;

                        }
                        break;
                    #endregion
                    case 191:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderPlaceBrokerReplyRecord());//Sandy
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderPlaceBrokerReplyRecord());
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.ULToken = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.ULToken;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.Symbol = new string(OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.Symbol).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.SymbolLength = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.SymbolLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.CALevel = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.CALevel;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.CPType = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.CPType;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.ExpiryDate = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.ExpiryDate;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.InstrType = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.InstrType;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.StrikePrice = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.StrikePrice;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.BuySell = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.BuySell;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.Qty = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_1.Qty;

                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.Symbol = new string(OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.Symbol).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.SymbolLength = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.SymbolLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.CALevel = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.CALevel;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.CPType = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.CPType;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.ExpiryDate = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.ExpiryDate;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.InstrType = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.InstrType;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.StrikePrice = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.StrikePrice;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.BuySell = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.BuySell;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.Qty = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.SPREAD_LEG_2.Qty;


                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.RateDiff = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.RateDiff;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.BrokerOrderTime = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.BrokerOrderTime;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.IOC = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.IOC;
                            OTSReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].SpdOrderPlaceBrokerReplyRec.AHStatus;
                        }
                        break;
                    case 192:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderModifyBrokerReplyRecord());//Sandy
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderModifyBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ULToken = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.ULToken;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.Symbol = new string(OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.Symbol).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.SymbolLength = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.SymbolLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.CALevel = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.CALevel;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.CPType = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.CPType;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.ExpiryDate = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.ExpiryDate;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.InstrType = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.InstrType;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.StrikePrice = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.StrikePrice;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.BuySell = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.BuySell;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.Qty = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_1.Qty;

                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.Symbol = new string(OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.Symbol).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.SymbolLength = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.SymbolLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.CALevel = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.CALevel;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.CPType = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.CPType;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.ExpiryDate = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.ExpiryDate;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.InstrType = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.InstrType;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.StrikePrice = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.StrikePrice;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.BuySell = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.BuySell;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.Qty = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.SPREAD_LEG_2.Qty;


                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.RateDiff = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.RateDiff;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.BrokerOrderTime = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.BrokerOrderTime;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.AHStatus;

                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchangeOrderId = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchangeOrderId;
                            OTSReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchangeOrderTime;
                        }
                        break;
                    case 193:
                        {
                            OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderCancelBrokerReplyRecord());//Sandy //five
                            OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderCancelBrokerReplyRecord());

                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.Exch = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.Exch;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchType = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchType;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ULToken = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.ULToken;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.Symbol = new string(OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.Symbol).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.SymbolLength = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.SymbolLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.CALevel = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.CALevel;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.CPType = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.CPType;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.ExpiryDate = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.ExpiryDate;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.InstrType = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.InstrType;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.StrikePrice = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.StrikePrice;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.BuySell = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.BuySell;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.Qty = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_1.Qty;

                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.Symbol = new string(OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.Symbol).PadRight(10).ToCharArray(); //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.SymbolLength = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.SymbolLength; //CHECK HERE
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.CALevel = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.CALevel;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.CPType = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.CPType;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.ExpiryDate = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.ExpiryDate;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.InstrType = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.InstrType;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.StrikePrice = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.StrikePrice;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.BuySell = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.BuySell;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.Qty = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.SPREAD_LEG_2.Qty;


                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.AtMarket = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.AtMarket;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.RateDiff = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.RateDiff;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.TraderRequestID = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.TraderRequestID;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.BrokerOrderID = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.BrokerOrderID;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.BrokerOrderTime = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.BrokerOrderTime;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.MarginBlocked = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.MarginBlocked;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.AHStatus = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.AHStatus;

                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchangeOrderId = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchangeOrderId;
                            OTSReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchangeOrderTime = OTSReplyRecForEC.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchangeOrderTime;
                        }
                        break;
                    //Old logic without Intradaydelivery
                    //case 211:
                    //    {
                    //        OTSReplyRecForEC.ReplyHdr.ReplyLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord());
                    //        OTSReplyRecForEC.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord());
                    //    }
                    //    break;
                    default:
                        break;
                }
                return OTSReplyRecForEC;
           
        }

        private bool CheckForOrderTypeReplyfrmEC(char Exch, char ExchType, int BrokerOrderId, int ReplyType, out int OrderPositionInList)
        {
             bool Result = false;
                OrderPositionInList = -1;
                ITInteractiveStructures.TOTSReplyRecord ListedOrderTypeReplyRec;
                switch (ReplyType)
                {
                    case Constants.OTS_ORDERTYPE_PLACE_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < OrderTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = OrderTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        if (!Result)
                        {
                            for (int OrderPos = 0; OrderPos < OrderTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = OrderTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1 != BrokerOrderId && ListedOrderTypeReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID2 != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                    }
                        break;
                    case Constants.OTS_ORDERTYPE_MODIFY_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < OrderTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = OrderTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < OrderTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = OrderTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case 241:
                        {
                            for (int OrderPos = 0; OrderPos < OrderTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = OrderTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case 242:
                        {
                            for (int OrderPos = 0; OrderPos < OrderTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = OrderTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case 243:
                        {
                            for (int OrderPos = 0; OrderPos < OrderTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = OrderTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }


                return Result;
            
        }

        private bool CheckForDealerOrderfrmEC(char Exch, char ExchType, int BrokerOrderId, int ReplyType, out int OrderPositionInList)
        {
                bool Result = false;
                OrderPositionInList = -1;
                ITInteractiveStructures.TOTSReplyRecord ListedOrderTypeReplyRec;
                switch (ReplyType)
                {
                    case Constants.OTS_ORDERTYPE_PLACE_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MODIFY_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_DERIV_PLACE_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_DERIV_MODIFY_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_DERIV_CANCEL_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CURR_PLACE_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CURR_MODIFY_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CURR_CANCEL_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case 241:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderPlaceBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case 242:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderModifyBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }

                        }
                        break;
                    case 243:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderCancelBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }

                        }
                        break;
                    case 249:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderExchReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderExchReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].SpdOrderExchReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_EXCH_REPLY:
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_EXCH_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    case Constants.OTS_MANDATE_EXCH_REPLY:
                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.BrokerOrderID != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }

                        }break;
                    case Constants.OTS_ORDERTYPE_MF_PLACE_EXCH_REPLY :
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_EXCH_REPLY:
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_EXCH_REPLY:

                        {
                            for (int OrderPos = 0; OrderPos < DealerTypeList.Count; OrderPos++)
                            {
                                ListedOrderTypeReplyRec = DealerTypeList[OrderPos];
                                if (ListedOrderTypeReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.Exch != Exch) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.ExchType != ExchType) continue;
                                if (ListedOrderTypeReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BrokerId != BrokerOrderId) continue;
                                Result = true;
                                OrderPositionInList = OrderPos;
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }


                return Result;
           
        } // 15.10.2016

        private byte GetReqTypeOnExch(char Exch, char ExchType, char OrderPlacement)
        {
            
                byte ReplyType = 0;
                switch (OrderPlacement)
                {
                    case 'B'://CoverTrading    // sumitp 28_jan_22
                    {
                            if (ExchType == 'C') ReplyType = 9;
                            else if (Exch == 'M' && ExchType == 'D') ReplyType = 110;
                            else if (Exch == 'N' && ExchType == 'D') ReplyType = 109;
                            else ReplyType = 211;
                        }
                        break;
                    case 'P':
                        {
                            if (ExchType == 'C') ReplyType = 10;
                            else if (Exch == 'M' && ExchType == 'D') ReplyType = 110;//McxTrading
                            else if (Exch == 'N' && ExchType == 'D') ReplyType = 110;
                            else if (Exch == 'F' && ExchType == 'D') ReplyType = 110; // BSE FO Trading
                        else ReplyType = 211;
                        }
                        break;
                    case 'M':
                        {
                            if (ExchType == 'C') ReplyType = 20;
                            else if (Exch == 'M' && ExchType == 'D') ReplyType = 120;//McxTrading
                            else if (Exch == 'N' && ExchType == 'D') ReplyType = 120;
                            else if (Exch == 'F' && ExchType == 'D') ReplyType = 120;// BSE FO Trading
                        else ReplyType = 221;
                        }
                        break;
                    case 'C':
                        {
                            if (ExchType == 'C') ReplyType = 30;
                            else if (Exch == 'M' && ExchType == 'D') ReplyType = 130;//McxTrading
                            else if (Exch == 'N' && ExchType == 'D') ReplyType = 130;
                            else if (Exch == 'F' && ExchType == 'D') ReplyType = 130;// BSE FO Trading
                        else ReplyType = 231;
                        }
                        break;
                    default:
                        break;
                }

                return ReplyType;
            
        }

        private void ReconnectToExchangeConnector(int ECNO)
        {
            try
            {
                if (ECNO == 1)
                {
                    btnExchConnect1_Click(null, null);
                }
                else
                {
                    btnExchConnect2_Click(null, null);
                }
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        private void UnableToProcessRequest(ITInteractiveStructures.TOTSReplyRecord OTSReplyRec)
        {
            try
            {
                ITInteractiveStructures.TIAReplyHeaderRecord IAHdr = new ITInteractiveStructures.TIAReplyHeaderRecord();
                ITInteractiveStructures.TLinkFailedRecord LinkFailRec = new ITInteractiveStructures.TLinkFailedRecord();
                ITInteractiveStructures.TIARequestHeaderRecord ReqHdr = new ITInteractiveStructures.TIARequestHeaderRecord();
                ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();

                TWSocketClient CurrClient;

                //Reverse Database entry and send info to client.
                //Broker Reply is NOT sent to client in this case

                IAHdr.SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                IAHdr.TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                IAHdr.TraderID = OTSReplyRec.ReplyHdr.TraderID;
                IAHdr.FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                IAHdr.FEUserID = OTSReplyRec.ReplyHdr.FEUserID;
                IAHdr.ClientType = OTSReplyRec.ReplyHdr.ClientType;
                IAHdr.ReplyType = 251;
                IAHdr.ReplyLen = Marshal.SizeOf(new ITInteractiveStructures.TLinkFailedRecord());
                IAHdr.ReplyRecSize = (ushort)IAHdr.ReplyLen;
                IAHdr.ReplyCnt = 1;
                IAHdr.ErrorCode = 0;

                LinkFailRec.RequestType = OTSReplyRec.ReplyHdr.ReplyType;
                LinkFailRec.TraderRequestID = 0;
                LinkFailRec.BrokerOrderID = 0;
                LinkFailRec.ErrorLevel = 40;
                LinkFailRec.ErrorMsg = "OTS to Exch Connector Link is down.";
                LinkFailRec.ErrorMsgLength = (byte)LinkFailRec.ErrorMsg.Length;
                switch (OTSReplyRec.ReplyHdr.ReplyType)
                {
                    case 10:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 20:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].EquityOrderModifyBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 30:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].EquityOrderCancelBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    //26.11.2015 Pranoti
                    case Constants.OTS_ORDERTYPE_PLACE_REQ:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].OrderTypeModifyBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].OrderTypeCancelBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 110:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].DerivOrderPlaceBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 120:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].DerivOrderModifyBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 130:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].DerivOrderCancelBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    #region Nse Curr Derive
                    case 211:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].CurrDerivOrderPlaceBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 221:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].CurrDerivOrderModifyBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 231:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].CurrDerivOrderCancelBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    #endregion
                    case 235: //21.11.18 SLBM
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].SLBMOrderPlaceBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 236: //21.11.18 SLBM
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].SLBMOrderModifyBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    case 237: //21.11.18 SLBM
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].SLBMOrderCancelBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    //Option Exercise
                    case Constants.OTS_EXERCISE_PLACE_REQ:
                    case Constants.OTS_EXERCISE_MODIFY_REQ:
                    case Constants.OTS_EXERCISE_CANCEL_REQ:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.TraderRequestID;
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].OptionExerciseBrokerReplyRec.BrokerOrderID;
                        }
                        break;
                    #region BseMf Krutika
                    case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.TraderRequestID;//BseMf
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].MFAddModifyCancelBrokerResRec.BrokerId;
                        } break;
                    case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.TraderRequestID;//BseMf
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderID;
                        }break;
                    case Constants.OTS_MANDATE:
                        {
                            LinkFailRec.TraderRequestID = OTSReplyRec.AllReply[0].XSIPMandateBrokerReplyRec.TraderRequestID;//BseMf
                            LinkFailRec.BrokerOrderID = OTSReplyRec.AllReply[0].MFSIPRegistrationBrokerResRec.BrokerOrderID;
                        }break;
                    #endregion

                }
                for (int i = 0; i < ListenToITS.FClientList.Count; i++)
                {
                    CurrClient = (TWSocketClient)ListenToITS.FClientList[i];
                    if (CurrClient == null)
                        continue;
                    if (CurrClient.IPAddress != new string(OTSReplyRec.IP).Substring(0, OTSReplyRec.IPLength))
                        continue;
                    CurrClient.PutDataInSendBuffer(CommonFunctions.RawSerialize(IAHdr), Marshal.SizeOf(IAHdr));
                    CurrClient.Send(CommonFunctions.RawSerialize(LinkFailRec), Marshal.SizeOf(LinkFailRec));
                    break;
                }

                ReqHdr.SocketNum = OTSReplyRec.ReplyHdr.SocketNum;
                ReqHdr.TraderIDLength = OTSReplyRec.ReplyHdr.TraderIDLength;
                ReqHdr.TraderID = new string(OTSReplyRec.ReplyHdr.TraderID).PadRight(14).ToCharArray();
                ReqHdr.FEUserIDLength = OTSReplyRec.ReplyHdr.FEUserIDLength;
                ReqHdr.FEUserID = new string(OTSReplyRec.ReplyHdr.FEUserID).PadRight(15).ToCharArray();
                ReqHdr.ClientType = OTSReplyRec.ReplyHdr.ClientType;
                ReqHdr.RequestType = 251;
                ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TLinkFailedRecord());

                OTSReqRec.IP = new string(OTSReplyRec.IP).PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr = ReqHdr;
                OTSReqRec.AllReq.LinkFailRec = LinkFailRec;
                AddToPendingRequestList(OTSReqRec);
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        private void SendRequestForMFClients()
        {
            try
            {
                string str1;
                ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();
                listBox4.Items.Clear();
                if (CommonFunctions.GetSelectedRadioButtonText(groupBoxDBNum) == 1)
                {
                    str1 = "db_one";
                }
                else
                    str1 = "db_two";
                listBox4.Items.Add("Sending Holding Request to " + str1);
                OTSReqRec.IPLength = 7;
                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserIDLength = (byte)str1.Length;
                OTSReqRec.ReqHdr.FEUserID = str1.PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderIDLength = (byte)str1.Length;
                OTSReqRec.ReqHdr.TraderID = str1.PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 86;
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMaalFodoClientsReportRequestRecord());
                OTSReqRec.AllReq.MFClientsReportRequestRec.ClientType = CommonFunctions.GetSelectedRadioButtonText(groupBoxMFCliType);
                OTSReqRec.AllReq.MFClientsReportRequestRec.MinimumDebit = Convert.ToInt32(textBoxMinDebit.Text);
                AddToPendingRequestList(OTSReqRec);
                DBRequestCnt++;
            }
            catch (Exception EX)
            {
                ErrorLog(EX);
            }
        }

        private int GetECNo(string TraderID)
        {
               int Result = 1;
                if (MaxEC == 1)
                    return Result;
                else if (TraderID.Length < 0)
                    return Result;
                else
                {
                    Result = CommonFunctions.BinSearchClient(ref ArrayClientDbNum, TraderID);
                    return Result;
                }
           
        }

        private void SendPendingReplyFromList()
        {

            ITInteractiveStructures.TOTSReplyRecord CurrReply = new ITInteractiveStructures.TOTSReplyRecord();
            ITInteractiveStructures.TOTSReplyRecord OTSReply = new ITInteractiveStructures.TOTSReplyRecord();
            ITInteractiveStructures.TIAErrorMessageRecord ErrorMsg = new ITInteractiveStructures.TIAErrorMessageRecord();
            TWSocketClient CurrCli = null;
            byte[] Compress = null;
            List<byte> Data = new List<byte>();

            //reply can have
            //1. Just header - report with no data fetched
            //2. Only Error structure
            //3.error reply as well as Allreply
            //In case of Type 3, Error will be sent first, followed by All Reply
            try
            {
                while (PendingReplyList.Count > 0)
                {
                    CurrReply = PendingReplyList[0];
                    //send to ITS here and then dispose
                    OTSReply = CurrReply;
                    for (int i = 0; i < ListenToITS.FClientList.Count; i++)
                    {
                        CurrCli = (TWSocketClient)ListenToITS.FClientList[i];
                        if (CurrCli == null) continue;
                        //if (CurrCli.IPAddress != new string(OTSReply.IP).Substring(0, OTSReply.IPLength)) continue;
                        //if (CurrCli.ITS_Num != OTSReply.ITS_Num) continue;
                        if (OTSReply.ReportReplyCount == 0)
                        {
                            if (OTSReply.ReplyHdr.ReplyType != 86 && OTSReply.ReplyHdr.ReplyType != Constants.OTS_ORDERTYPE_CONVERSION
                                && OTSReply.ReplyHdr.ReplyType != Constants.OTS_ORDERTYPE_PLACE_REQ_COVER   // sumitp 28_jan_22
                                && OTSReply.ReplyHdr.ReplyType != Constants.OTS_ORDERTYPE_PLACE_REQ
                                && OTSReply.ReplyHdr.ReplyType != Constants.OTS_ORDERTYPE_MODIFY_REQ
                                && OTSReply.ReplyHdr.ReplyType != Constants.OTS_ORDERTYPE_CANCEL_REQ
                                && OTSReply.ReplyHdr.ReplyType != Constants.OTS_ORDERTYPE_DERIVCONVERSION
                                && OTSReply.ReplyHdr.ReplyType != Constants.OTS_OFS_PLACE_BROKER_REQ 
                                && OTSReply.ReplyHdr.ReplyType != Constants.OTS_OFS_MODIFY_BROKER_REQ
                                && OTSReply.ReplyHdr.ReplyType != Constants.OTS_OFS_CANCEL_BROKER_REQ)
                                //&& OTSReply.ReplyHdr.ErrorCode != 1) //26.04.16
                            {
                                if (CurrCli.IPAddress != new string(OTSReply.IP).Substring(0, OTSReply.IPLength)) continue;
                                if (CurrCli.ITS_Num != OTSReply.ITS_Num) continue;
                            }
                            if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;


                            byte[] hdrData = CommonFunctions.RawSerialize(OTSReply.ReplyHdr, out int hdrSize);
                            TCustomWSocketLogger.logData(hdrData, hdrSize, CurrCli.ITS_Num, 0);
                            CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.ReplyHdr), Marshal.SizeOf(OTSReply.ReplyHdr));
                            //CurrCli.Send(hdrData, hdrSize);
                            BytesToITS++;
                            if (checkBoxWriteToDisplayITS.Checked)
                                listBox1.Items.Insert(0, "To ITS - Hdr sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAReplyHeaderRecord()));
                            if (OTSReply.ReplyHdr.ErrorCode != 0)
                            {
                                ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;


                                byte[] errData = CommonFunctions.RawSerialize(ErrorMsg, out int errLen);
                                TCustomWSocketLogger.logData(errData, errLen, CurrCli.ITS_Num, 1);
                                CurrCli.Send(errData, errLen);

                                //CurrCli.Send(CommonFunctions.RawSerialize(ErrorMsg), Marshal.SizeOf(ErrorMsg));
                                if (checkBoxWriteToDisplayITS.Checked)
                                    listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                            }
                            if (checkBoxWriteToDisplayITS.Checked)
                            {
                                //listBox1.Items.Insert(0, "Database Error =" + OTSReply.ErrorString + " . ReplyType = " + OTSReply.ReplyHdr.ReplyType);
                                //listBox1.Items.Insert(0, "Database Reply Rec Cnt =" + OTSReply.ReplyCount + " . ReplyType = " + OTSReply.ReplyHdr.ReplyType);
                                listBox1.Items.Insert(0, "Reply Rec Cnt =" + OTSReply.ReplyHdr.ReplyCnt + " . ReplyType = " + OTSReply.ReplyHdr.ReplyType);
                            }
                            if (OTSReply.ReplyHdr.ReplyCnt == 0)
                                continue;
                            for (int j = 0; j < OTSReply.ReplyHdr.ReplyCnt; j++)
                            {
                                if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected)
                                    break;
                                //CurrCli.PutDataInSendBuffer(ConvertStructureToByteArr(OTSReply.AllReply[j], OTSReply.ReplyHdr.ReplyType), OTSReply.ReplyHdr.ReplyRecSize);
                                byte[] data = ConvertStructureToByteArr(OTSReply.AllReply[j], OTSReply.ReplyHdr.ReplyType);
                                TCustomWSocketLogger.logData(data, OTSReply.ReplyHdr.ReplyRecSize, CurrCli.ITS_Num, 2);
                                CurrCli.PutDataInSendBuffer(data, OTSReply.ReplyHdr.ReplyRecSize);


                                if (checkBoxWriteToDisplayITS.Checked && OTSReplyType.Contains(OTSReply.ReplyHdr.ReplyType))
                                    listBox1.Items.Insert(0, j + 1 + " . Data sent " + OTSReply.ReplyHdr.ReplyRecSize);
                            }
                            if (CurrCli != null)
                            {
                                CurrCli.Send(null, 0);
                            }
                        }
                        else
                        {
                            for (int k = 0; k < OTSReply.ReportReplyCount; k++)
                            {
                                if (OTSReply.NewReplyHdr[k].ReplyType != 86)
                                {
                                    if (CurrCli.IPAddress != new string(OTSReply.IP).Substring(0, OTSReply.IPLength))
                                        continue;
                                    if (CurrCli.ITS_Num != OTSReply.ITS_Num) continue;
                                }

                                if (OTSReply.NewReplyHdr[k].ReplyType == 5)// changes done to handle junk in Web ITS
                                {
                                    if (CurrCli.ITS_Num != OTSReply.ITS_Num)
                                        continue;
                                }
                                if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                if (OTSReply.NewReplyHdr[k].ReplyType == 5)
                                {
                                    Data.Clear();
                                    Compress = null;

                                    //CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));
                                    //BytesToITS++;
                                    //if (checkBoxWriteToDisplayITS.Checked)
                                    //    listBox1.Items.Insert(0, "To ITS - Hdr sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAReplyHeaderRecord()));


                                    //if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                    //{
                                    //    ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                    //    ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                    //    if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                    //    CurrCli.Send(CommonFunctions.RawSerialize(ErrorMsg), Marshal.SizeOf(ErrorMsg));
                                    //    if (checkBoxWriteToDisplayITS.Checked)
                                    //        listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                    //}
                                    if (checkBoxWriteToDisplayITS.Checked)
                                    {
                                        listBox1.Items.Insert(0, "Database Error =" + OTSReply.ErrorString + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Database Reply Rec Cnt =" + OTSReply.ReplyCount + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Reply Rec Cnt =" + OTSReply.NewReplyHdr[k].ReplyCnt + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                    }

                                    if (OTSReply.NewReplyHdr[k].ReplyCnt == 0)
                                        continue;
                                    for (int j = 0; j < OTSReply.NewReplyHdr[k].ReplyCnt; j++)
                                    {
                                        //if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected)
                                        //    break;
                                        Data.AddRange(ConvertStructureToByteArr(OTSReply.AllReplyNew[k][j], OTSReply.NewReplyHdr[k].ReplyType));

                                        //if (checkBoxWriteToDisplayITS.Checked && OTSReplyType.Contains(OTSReply.NewReplyHdr[k].ReplyType))
                                        //    listBox1.Items.Insert(0, j + 1 + " . Data sent " + OTSReply.NewReplyHdr[k].ReplyRecSize);
                                    }

                                    byte[] Result = Data.ToArray();
                                    Compress = Result.BZipCompress(Result.Length);
                                    OTSReply.NewReplyHdr[k].ReplyLen = Compress.Length;
                                    OTSReply.NewReplyHdr[k].TraderIDLength = 1;

                                    if (k == 0)
                                    {
                                        OTSReply.NewReplyHdr[k].TraderID = "START".PadRight(14).ToCharArray();
                                    }
                                    if (k == OTSReply.ReportReplyCount - 1)
                                    {
                                        OTSReply.NewReplyHdr[k].TraderID = "OVER".PadRight(14).ToCharArray();
                                    }

                                    if (CurrCli != null)
                                    {
                                        //CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));

                                        byte[] hdrData = CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k], out int hdrSize);
                                        TCustomWSocketLogger.logData(hdrData, hdrSize, CurrCli.ITS_Num, 0);
                                        CurrCli.Send(hdrData, hdrSize);

                                        if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                        {
                                            ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                            ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                            if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                            byte[] errData = CommonFunctions.RawSerialize(ErrorMsg, out int errLen);
                                            TCustomWSocketLogger.logData(errData, errLen, CurrCli.ITS_Num, 1);
                                            CurrCli.Send(errData, errLen);
                                            if (checkBoxWriteToDisplayITS.Checked)
                                                listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                        }

                                        TCustomWSocketLogger.logData(Compress, Compress.Length, CurrCli.ITS_Num, 2);



                                        CurrCli.PutDataInSendBuffer(Compress, Compress.Length);
                                        CurrCli.Send(null, 0);
                                    }
                                }
                                else if (OTSReply.NewReplyHdr[k].ReplyType == 207)  //Added on 27-Mar-2017 - Rohan
                                {
                                    Data.Clear();
                                    Compress = null;

                                    //CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));
                                    //BytesToITS++;
                                    //if (checkBoxWriteToDisplayITS.Checked)
                                    //    listBox1.Items.Insert(0, "To ITS - Hdr sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAReplyHeaderRecord()));
                                    //if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                    //{
                                    //    ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                    //    ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                    //    if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                    //    CurrCli.Send(CommonFunctions.RawSerialize(ErrorMsg), Marshal.SizeOf(ErrorMsg));
                                    //    if (checkBoxWriteToDisplayITS.Checked)
                                    //        listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                    //}


                                    if (checkBoxWriteToDisplayITS.Checked)
                                    {
                                        listBox1.Items.Insert(0, "Database Error =" + OTSReply.ErrorString + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Database Reply Rec Cnt =" + OTSReply.ReplyCount + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Reply Rec Cnt =" + OTSReply.NewReplyHdr[k].ReplyCnt + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                    }

                                    if (OTSReply.NewReplyHdr[k].ReplyCnt == 0)
                                        continue;
                                    for (int j = 0; j < OTSReply.NewReplyHdr[k].ReplyCnt; j++)
                                    {
                                        //if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected)
                                        //    break;
                                        Data.AddRange(ConvertStructureToByteArr(OTSReply.AllReplyNew[k][j], OTSReply.NewReplyHdr[k].ReplyType));

                                        //if (checkBoxWriteToDisplayITS.Checked && OTSReplyType.Contains(OTSReply.NewReplyHdr[k].ReplyType))
                                        //    listBox1.Items.Insert(0, j + 1 + " . Data sent " + OTSReply.NewReplyHdr[k].ReplyRecSize);
                                    }

                                    byte[] Result = Data.ToArray();
                                    Compress = Result.BZipCompress(Result.Length);
                                    OTSReply.NewReplyHdr[k].ReplyLen = Compress.Length;
                                    OTSReply.NewReplyHdr[k].TraderIDLength = 1;

                                    if (k == 0)
                                    {
                                        OTSReply.NewReplyHdr[k].TraderID = "START".PadRight(14).ToCharArray();
                                    }
                                    if (k == OTSReply.ReportReplyCount - 1)
                                    {
                                        OTSReply.NewReplyHdr[k].TraderID = "OVER".PadRight(14).ToCharArray();
                                    }

                                    if (CurrCli != null)
                                    {
                                        //CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));
                                        byte[] hdrData = CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k], out int hdrSize);
                                        TCustomWSocketLogger.logData(hdrData, hdrSize, CurrCli.ITS_Num, 0);
                                        CurrCli.Send(hdrData, hdrSize);

                                        if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                        {
                                            ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                            ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                            if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                            byte[] errData = CommonFunctions.RawSerialize(ErrorMsg, out int errLen);
                                            TCustomWSocketLogger.logData(errData, errLen, CurrCli.ITS_Num, 1);
                                            CurrCli.Send(errData, errLen);
                                            if (checkBoxWriteToDisplayITS.Checked)
                                                listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                        }
                                        TCustomWSocketLogger.logData(Compress, Compress.Length, CurrCli.ITS_Num, 2);



                                        CurrCli.PutDataInSendBuffer(Compress, Compress.Length);
                                        CurrCli.Send(null, 0);
                                    }
                                }
                                else if (OTSReply.NewReplyHdr[k].ReplyType == 250)  //Added on 27-Mar-2017 - Rohan
                                {
                                    Data.Clear();
                                    Compress = null;

                                    //CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));
                                    //BytesToITS++;
                                    //if (checkBoxWriteToDisplayITS.Checked)
                                    //    listBox1.Items.Insert(0, "To ITS - Hdr sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAReplyHeaderRecord()));
                                    //if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                    //{
                                    //    ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                    //    ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                    //    if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                    //    CurrCli.Send(CommonFunctions.RawSerialize(ErrorMsg), Marshal.SizeOf(ErrorMsg));
                                    //    if (checkBoxWriteToDisplayITS.Checked)
                                    //        listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                    //}
                                    if (checkBoxWriteToDisplayITS.Checked)
                                    {
                                        listBox1.Items.Insert(0, "Database Error =" + OTSReply.ErrorString + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Database Reply Rec Cnt =" + OTSReply.ReplyCount + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Reply Rec Cnt =" + OTSReply.NewReplyHdr[k].ReplyCnt + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                    }

                                    if (OTSReply.NewReplyHdr[k].ReplyCnt == 0)
                                        continue;
                                    for (int j = 0; j < OTSReply.NewReplyHdr[k].ReplyCnt; j++)
                                    {
                                        //if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected)
                                        //    break;
                                        Data.AddRange(ConvertStructureToByteArr(OTSReply.AllReplyNew[k][j], OTSReply.NewReplyHdr[k].ReplyType));

                                        //if (checkBoxWriteToDisplayITS.Checked && OTSReplyType.Contains(OTSReply.NewReplyHdr[k].ReplyType))
                                        //    listBox1.Items.Insert(0, j + 1 + " . Data sent " + OTSReply.NewReplyHdr[k].ReplyRecSize);
                                    }

                                    byte[] Result = Data.ToArray();
                                    Compress = Result.BZipCompress(Result.Length);
                                    OTSReply.NewReplyHdr[k].ReplyLen = Compress.Length;
                                    OTSReply.NewReplyHdr[k].TraderIDLength = 1;

                                    if (k == 0)
                                    {
                                        OTSReply.NewReplyHdr[k].TraderID = "START".PadRight(14).ToCharArray();
                                    }
                                    if (k == OTSReply.ReportReplyCount - 1)
                                    {
                                        OTSReply.NewReplyHdr[k].TraderID = "OVER".PadRight(14).ToCharArray();
                                    }

                                    if (CurrCli != null)
                                    {
                                        //CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));

                                        byte[] hdrData = CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k], out int hdrSize);
                                        TCustomWSocketLogger.logData(hdrData, hdrSize, CurrCli.ITS_Num, 0);
                                        CurrCli.Send(hdrData, hdrSize);

                                        if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                        {
                                            ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                            ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                            if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                            byte[] errData = CommonFunctions.RawSerialize(ErrorMsg, out int errLen);
                                            TCustomWSocketLogger.logData(errData, errLen, CurrCli.ITS_Num, 1);
                                            CurrCli.Send(errData, errLen);
                                            if (checkBoxWriteToDisplayITS.Checked)
                                                listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                        }
                                        TCustomWSocketLogger.logData(Compress, Compress.Length, CurrCli.ITS_Num, 2);


                                        CurrCli.PutDataInSendBuffer(Compress, Compress.Length);
                                        CurrCli.Send(null, 0);
                                    }
                                }
                                else if (OTSReply.NewReplyHdr[k].ReplyType == 141 || OTSReply.NewReplyHdr[k].ReplyType == 142)  //Added on 05-Apr-2018 - Rohan
                                {
                                    Data.Clear();
                                    Compress = null;

                                    //BytesToITS++;
                                    //if (checkBoxWriteToDisplayITS.Checked)
                                    //    listBox1.Items.Insert(0, "To ITS - Hdr sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAReplyHeaderRecord()));
                                    //if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                    //{
                                    //    ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                    //    ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                    //    if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                    //    CurrCli.Send(CommonFunctions.RawSerialize(ErrorMsg), Marshal.SizeOf(ErrorMsg));
                                    //    if (checkBoxWriteToDisplayITS.Checked)
                                    //        listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                    //}
                                    if (checkBoxWriteToDisplayITS.Checked)
                                    {
                                        listBox1.Items.Insert(0, "Database Error =" + OTSReply.ErrorString + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Database Reply Rec Cnt =" + OTSReply.ReplyCount + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Reply Rec Cnt =" + OTSReply.NewReplyHdr[k].ReplyCnt + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                    }

                                    if (OTSReply.NewReplyHdr[k].ReplyCnt == 0)
                                        continue;

                                    if (OTSReply.NewReplyHdr[k].ReplyCnt <= 500)
                                    {
                                        for (int j = 0; j < OTSReply.NewReplyHdr[k].ReplyCnt; j++)
                                        {
                                            Data.AddRange(ConvertStructureToByteArr(OTSReply.AllReplyNew[k][j], OTSReply.NewReplyHdr[k].ReplyType));
                                        }
                                        byte[] Result = Data.ToArray();

                                        //for (int j = 0; j < OTSReply.NewReplyHdr[k].ReplyCnt; j++)
                                        //{
                                        //    if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected)
                                        //        break;
                                        //    CurrCli.PutDataInSendBuffer(ConvertStructureToByteArr(OTSReply.AllReplyNew[k][j], OTSReply.NewReplyHdr[k].ReplyType), OTSReply.NewReplyHdr[k].ReplyRecSize);
                                        //    if (checkBoxWriteToDisplayITS.Checked && OTSReplyType.Contains(OTSReply.NewReplyHdr[k].ReplyType))
                                        //        listBox1.Items.Insert(0, j + 1 + " . Data sent " + OTSReply.NewReplyHdr[k].ReplyRecSize);
                                        //}

                                        if (k == 0)
                                        {
                                            OTSReply.NewReplyHdr[k].ChannelID_NC = 1;
                                        }
                                        if (k == OTSReply.ReportReplyCount - 1)
                                        {
                                            OTSReply.NewReplyHdr[k].ChannelID_NC = 0;
                                        }

                                        if (CurrCli != null)
                                        {
                                            //CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));

                                            byte[] hdrData = CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k], out int hdrSize);
                                            TCustomWSocketLogger.logData(hdrData, hdrSize, CurrCli.ITS_Num, 0);
                                            CurrCli.Send(hdrData, hdrSize);

                                            if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                            {
                                                ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                                ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                                if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                                byte[] errData = CommonFunctions.RawSerialize(ErrorMsg, out int errLen);
                                                TCustomWSocketLogger.logData(errData, errLen, CurrCli.ITS_Num, 1);
                                                CurrCli.Send(errData, errLen);
                                                if (checkBoxWriteToDisplayITS.Checked)
                                                    listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                            }
                                            TCustomWSocketLogger.logData(Compress, Compress.Length, CurrCli.ITS_Num, 2);


                                            CurrCli.PutDataInSendBuffer(Result, Result.Length);
                                            CurrCli.Send(null, 0);
                                        }
                                    }
                                    else
                                    {
                                        for (int j = 0; j < OTSReply.NewReplyHdr[k].ReplyCnt; j++)
                                        {
                                            Data.AddRange(ConvertStructureToByteArr(OTSReply.AllReplyNew[k][j], OTSReply.NewReplyHdr[k].ReplyType));
                                        }

                                        byte[] Result = Data.ToArray();
                                        Compress = Result.BZipCompress(Result.Length);
                                        OTSReply.NewReplyHdr[k].ReplyLen = Compress.Length;
                                        OTSReply.NewReplyHdr[k].TraderIDLength = 1;

                                        if (k == 0)
                                        {
                                            OTSReply.NewReplyHdr[k].ChannelID_NC = 1;
                                        }
                                        if (k == OTSReply.ReportReplyCount - 1)
                                        {
                                            OTSReply.NewReplyHdr[k].ChannelID_NC = 0;
                                        }

                                        if (CurrCli != null)
                                        {
                                            CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));
                                            CurrCli.PutDataInSendBuffer(Compress, Compress.Length);
                                            CurrCli.Send(null, 0);
                                        }
                                    }
                                }
                                else
                                {
                                    byte[] hdrData = CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k], out int hdrSize);
                                    TCustomWSocketLogger.logData(hdrData, hdrSize, CurrCli.ITS_Num, 0);
                                    CurrCli.Send(hdrData, hdrSize);

                                    //CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.NewReplyHdr[k]), Marshal.SizeOf(OTSReply.NewReplyHdr[k]));
                                    BytesToITS++;
                                    if (checkBoxWriteToDisplayITS.Checked)
                                        listBox1.Items.Insert(0, "To ITS - Hdr sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAReplyHeaderRecord()));
                                    if (OTSReply.NewReplyHdr[k].ErrorCode != 0)
                                    {
                                        ErrorMsg.ErrorStr = OTSReply.ErrorString;
                                        ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
                                        if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

                                        byte[] errData = CommonFunctions.RawSerialize(ErrorMsg, out int errLen);
                                        TCustomWSocketLogger.logData(errData, errLen, CurrCli.ITS_Num, 1);
                                        CurrCli.Send(errData, errLen);

                                        //CurrCli.Send(CommonFunctions.RawSerialize(ErrorMsg), Marshal.SizeOf(ErrorMsg));
                                        if (checkBoxWriteToDisplayITS.Checked)
                                            listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
                                    }
                                    if (checkBoxWriteToDisplayITS.Checked)
                                    {
                                        listBox1.Items.Insert(0, "Database Error =" + OTSReply.ErrorString + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                        listBox1.Items.Insert(0, "Reply Rec Cnt =" + OTSReply.NewReplyHdr[k].ReplyCnt + " . ReplyType = " + OTSReply.NewReplyHdr[k].ReplyType);
                                    }
                                    if (OTSReply.NewReplyHdr[k].ReplyCnt == 0)
                                        continue;
                                    for (int j = 0; j < OTSReply.NewReplyHdr[k].ReplyCnt; j++)
                                    {
                                        if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected)
                                            break;


                                        byte[] data = ConvertStructureToByteArr(OTSReply.AllReplyNew[k][j], OTSReply.NewReplyHdr[k].ReplyType);
                                        TCustomWSocketLogger.logData(data, OTSReply.NewReplyHdr[k].ReplyRecSize, CurrCli.ITS_Num, 2);
                                        CurrCli.PutDataInSendBuffer(data, OTSReply.NewReplyHdr[k].ReplyRecSize);


                                        //CurrCli.PutDataInSendBuffer(ConvertStructureToByteArr(OTSReply.AllReplyNew[k][j], OTSReply.NewReplyHdr[k].ReplyType), OTSReply.NewReplyHdr[k].ReplyRecSize);
                                        if (checkBoxWriteToDisplayITS.Checked && OTSReplyType.Contains(OTSReply.NewReplyHdr[k].ReplyType))
                                            listBox1.Items.Insert(0, j + 1 + " . Data sent " + OTSReply.NewReplyHdr[k].ReplyRecSize);
                                    }
                                    if (CurrCli != null)
                                    {
                                        CurrCli.Send(null, 0);
                                    }
                                }
                            }
                            //OTSReply.ReportReplyCount = 0;
                        }
                    }
                    PendingReplyList.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
            }
        }

        //private void SendPendingReplyFromList()
        //{
        //    ITInteractiveStructures.TOTSReplyRecord CurrReply = new ITInteractiveStructures.TOTSReplyRecord();
        //    ITInteractiveStructures.TOTSReplyRecord OTSReply = new ITInteractiveStructures.TOTSReplyRecord();
        //    ITInteractiveStructures.TIAErrorMessageRecord ErrorMsg = new ITInteractiveStructures.TIAErrorMessageRecord();
        //    TWSocketClient CurrCli=null;
        //    //reply can have
        //    //1. Just header - report with no data fetched
        //    //2. Only Error structure
        //    //3.error reply as well as Allreply
        //    //In case of Type 3, Error will be sent first, followed by All Reply
        //    try
        //    {
        //        while (PendingReplyList.Count > 0)
        //        {
        //            CurrReply = PendingReplyList[0];
        //            //send to ITS here and then dispose
        //            OTSReply = CurrReply;
        //            for (int i = 0; i < ListenToITS.FClientList.Count; i++)
        //            {
        //                CurrCli = (TWSocketClient)ListenToITS.FClientList[i];
        //                if (CurrCli == null)
        //                    continue;
        //                if (OTSReply.ReplyHdr.ReplyType != 86)
        //                {
        //                    if (CurrCli.IPAddress !=new string(OTSReply.IP).Substring(0,OTSReply.IPLength))
        //                        continue;
        //                }
        //                if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

        //                CurrCli.Send(CommonFunctions.RawSerialize(OTSReply.ReplyHdr), Marshal.SizeOf(OTSReply.ReplyHdr));    
        //                BytesToITS++;
        //                if (checkBoxWriteToDisplayITS.Checked)
        //                    listBox1.Items.Insert(0, "To ITS - Hdr sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAReplyHeaderRecord()));
        //                if (OTSReply.ReplyHdr.ErrorCode != 0)
        //                {
        //                    ErrorMsg.ErrorStr = OTSReply.ErrorString;
        //                    ErrorMsg.ErrorStrLength = (byte)ErrorMsg.ErrorStr.Length;
        //                    if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected) continue;

        //                    CurrCli.Send(CommonFunctions.RawSerialize(ErrorMsg), Marshal.SizeOf(ErrorMsg));    
        //                    if (checkBoxWriteToDisplayITS.Checked)
        //                        listBox1.Items.Insert(0, "To ITS - Error sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()));
        //                }
        //                if (checkBoxWriteToDisplayITS.Checked)
        //                    listBox1.Items.Insert(0, "Reply Rec Cnt =" + OTSReply.ReplyHdr.ReplyCnt + " . ReplyType = " + OTSReply.ReplyHdr.ReplyType);
        //                if (OTSReply.ReplyHdr.ReplyCnt == 0)
        //                    continue;                     
        //                    for (int j = 0; j < OTSReply.ReplyHdr.ReplyCnt; j++)
        //                    {
        //                        if (CurrCli.State != OverbyteIcsWSocket.TSocketState.wsConnected)
        //                            break;
        //                        CurrCli.PutDataInSendBuffer(ConvertStructureToByteArr(OTSReply.AllReply[j], OTSReply.ReplyHdr.ReplyType), OTSReply.ReplyHdr.ReplyRecSize);                                                            
        //                        if (checkBoxWriteToDisplayITS.Checked && OTSReplyType.Contains(OTSReply.ReplyHdr.ReplyType))
        //                            listBox1.Items.Insert(0, j + 1 + " . Data sent " + OTSReply.ReplyHdr.ReplyRecSize);
        //                    }
        //                    if (CurrCli != null)
        //                    {
        //                        CurrCli.Send(null, 0);
        //                    }                                                
        //            }                   
        //            PendingReplyList.RemoveAt(0);
        //        }
        //    }
        //    catch (Exception e)
        //    {               
        //    }
        //}

        private void SendCallback1(IAsyncResult ar)
        {
            try
            {
                UdpClient client = (UdpClient)ar.AsyncState;
                client.EndSend(ar);
                McastOTS.BeginReceive();
            }
            catch (Exception e)
            {
                ErrorLog(e);
            }
        }

        private byte[] ConvertStructureToByteArr(ITInteractiveStructures.TAllReplyNewRecord ReplyToSend, byte ReplyType)
        {
           
                byte[] SendData = null;
                switch (ReplyType)
                {
                    case 1:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.LoginReply);
                        }
                        break;
                    case 2:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.ChgPswdReplyRec);
                        }
                        break;
                    case 3:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.LoginReplyNew);
                        } break;
                case 85:
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.twoFactorLoginReplyRecord);
                    }
                    break;
                case 4:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.LoginReplyWithName);
                        } break;
                    case 5:
                    case 6:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.TraderListReplyRec);
                        }
                        break;
                    case 7:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrdersReportDealer);
                        }break;
                    case 8:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.TradesReportDealer);
                        } break;
                    case 10:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderPlaceBrokerReplyRec);
                        }
                        break;
                case 14:
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.GetIVRDealerDetailsReply); //24.04.23 For getting IVR Dealer Details		
                    }
                    break;
                    case Constants.OTS_ORDERTYPE_PLACE_REQ:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypePlaceBrokerReplyRec);
                        }
                        break;
                case 9:
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderPlaceBrokerReplyRec);

                    }
                    break;
                case Constants.OTS_ORDERTYPE_PLACE_REQ_COVER:
                    {
                        if (ReplyToSend.OrderTypePlaceBrokerReplyRec_Cover.ExchType == 'D')
                        {
                            //SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderPlaceBrokerReplyRec);
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypePlaceBrokerReplyRec_Cover);
                            //SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderPlaceBrokerReplyRec);
                        }
                        else
                        {
                            //SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderPlaceBrokerReplyRec);
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderPlaceBrokerReplyRecord_cover);
                            //SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderPlaceBrokerReplyRec);
                        }

                    }
                    break;
                case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeModifyBrokerReplyRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeCancelBrokerReplyRec);
                        }
                        break;

                    case 101://McxTrading
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderPlaceBrokerReplyRec);
                        }
                        break;
                    case 102://McxTrading
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderModifyBrokerReplyRec);
                        }
                        break;
                    case 103://McxTrading
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderCancelBrokerReplyRec);
                        }
                        break;
                    case 20:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderModifyBrokerReplyRec);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_DERIVCONVERSION:
                    case Constants.OTS_ORDERTYPE_CONVERSION:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeConversionReplyRec);
                        }
                        break;
                    case 30:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.EquityOrderCancelBrokerReplyRec);
                        }
                        break;
                    //case 61:
                    //    {
                    //        SendData = CommonFunctions.RawSerialize(ReplyToSend.TradeReportReplyRec);
                    //    }
                    //    break;
                    case 41:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.ScripMarginReplyRec);
                        }
                        break;
                    // 2015.03.26
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ://34
                    case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MFSIPRegistrationBrokerResRec);
                            //SendData = CommonFunctions.RawSerialize(ReplyToSend.XMfSIPRegistrationReqRec);
                    } break;
                    case Constants.OTS_MANDATE:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.XSIPMandateBrokerReplyRec);
                        }break;
                    // BseMf Krutika
                    case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MFAddModifyCancelBrokerResRec);
                        }
                        break;
                    //
                    case Constants.OTS_SECURITY_INFO:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.ScripInfoReplyRec);
                        }
                        break;
                    case Constants.OTS_SCRIP_MARGIN:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.NewScripMarginReplyRec);
                        }
                        break;
                    // 2015.03.26
                    //case 62:
                    //    {
                    //        SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderReportReplyRec);
                    //    }
                    //    break;
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC:
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142:    //Added on 5-Apr-2018 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeTradeReportReplyRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW:    //Added on 18-Apr-2018 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeTradeReportReplyRecNew);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER:     // sumitp 28_jan_22
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeTradeReportReplyRecNewCover);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141: //Added on 5-Apr-2018 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrdeTypeReportsReplyRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrdeTypeReportsReplyRecNewWithRejectOrder);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW: //Added on 18-Apr-2018 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrdeTypeReportsReplyRecNew);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_COVER_BRACKET: //Added on 15-Nov-2021 - Himanshu
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderReportReplyRecCoverOrBracket);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_EXERCISE_REPORTS_REC://Options Exercise
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OptionExerciseReportReplyRec);
                        }
                        break;
                    //case 64:
                    //    {
                    //        SendData = CommonFunctions.RawSerialize(ReplyToSend.ActivityLogReplyRec);
                    //    }
                    //    break;
                    case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeActivityLogReplyRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeActivityLogReplyRecForRequestor);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new: // Sumit 21_jan_22 For Single Activity Log
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeActivityLogReplyRecForRequestor);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW:  //Added on 18-Apr-2018 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OrderTypeActivityLogReplyRecForRequestorNew);
                        }
                        break;
                    case 69:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsReportReplyRec);
                        }
                        break;
                    case 70:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsValuationReportReplyRec);
                        }
                        break;
                    case 218:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsValuationReportReplyRec);
                        }
                        break;
                    case Constants.OTS_HOLDING_VALUATION_REPORT_NEW:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsValuationReportWithMemVarReplyRec);
                        }
                        break;
                    case 74:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsReportWithCollatralReplyRec);
                        }
                        break;
                    case 75:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.AllClientsHoldingsReportReplyRec);
                        }
                        break;
                    case 76:    //Added on 08-Nov-2016 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.AllClientsHoldingsEquityReportReplyRec);
                        }
                        break;
                    case 79:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.ClientPositionReplyRec);
                        }
                        break;
                    case 80:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.TradeToTradeReplyRec);
                        }
                        break;
                    case 160:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SurveillanceIndicatorMessagesNseReplyRec);
                        }
                        break;
                    case 161:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SurveillanceIndicatorMessagesBseReplyRec);
                        }
                        break;
                    case 81:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.BseNseMapReplyRec);
                        }
                        break;
                    case 82:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.DerivBannedScripReplyRec);
                        }
                        break;
                    case 83:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.AdhocReportReplyRec);
                        }
                        break;
                    case 84:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.AdhocModifyReplyRec);
                        }
                        break;
                    case 86:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MFClientsReportReplyRec);
                        }
                        break;
                    case 89:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.IlliquidReplyRec);
                        }
                        break;
                    case 91:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.LotQtyReplyRec);
                        }
                        break;
                    case 92://BseMf Krutika
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsReportofBseMfReplyRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_ORDER_REPORTS_REC://BseMf Krutika
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.BseMfOrdeTypeReportsReplyRec);
                        } break;
                   case Constants.OTS_Acitivitylog_MF_ORDER_REPORTS_REC://BseMf Krutika
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.TBseMFActivitylogReplyRec);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC://BseMf Krutika
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.BseMfSIPOrdeTypeReportsReplyRec);
                        } break;
                case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC:
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.MandateOrderTypeReportsReplyRec);
                    }
                    break;
                    case Constants.OTS_HOLDING_REPORT://94 Mcx
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsReportWithCollatralReplyRecNew);
                        } break;
                    case Constants.OTS_HOLDING_REPORT_PreClose://106 jinal
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsReportWithCollatralPreCloseReplyRecNew);
                        } break;
                    case Constants.OTS_HOLDING_REPORT_PreClose_FS://112 Himanshu
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsReportWithCollatralPreCloseReplyRecNew_FS);
                        }
                        break;
                case 67:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsReportMTF);
                        }
                        break;
                   case 90:
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.HoldingsReportMTFNew);
                    }
                    break;
                case 194:
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.ISCL_HoldingEquityReply);
                    }
                    break;
                case Constants.OTS_ALLCLIENTS_HOLDING_REPORT://95  31.01.17
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.AllClientsHoldingsReportReplyRecNew);
                        }
                        break;
                    case Constants.OTS_ALLCLIENTS_HOLDING_REPORT_New://95  31.01.17
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.AllClientsHoldingsReportReplyRecWithPreClose);
                        }
                        break;
                    case Constants.OTS_VERIFY_USER: //99
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.VerifyUserReplyRec);
                        }
                        break;
                    case 110:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.DerivOrderPlaceBrokerReplyRec);
                        }
                        break;
                    case 120:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.DerivOrderModifyBrokerReplyRec);
                        }
                        break;
                    case 130:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.DerivOrderCancelBrokerReplyRec);
                        }
                        break;
                    case 134:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.orderPlaceReplyRecOfApproval);
                        }
                        break;
                    case 135:
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.UpdateOTPOrderStatusResponseRec);
                    }
                    break;
                case 136:
                    {
                        SendData = CommonFunctions.RawSerialize(ReplyToSend.PendingApprovalOrderReportReplyRec);
                    }
                    break;
                    case 191:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SpdOrderPlaceBrokerReplyRec);
                        }
                        break;
                    case 192:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SpdOrderModifyBrokerReplyRec); // total 169 bytes                        
                        } break;
                    case 193:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SpdOrderCancelBrokerReplyRec); // total 169 bytes                        
                        }
                        break;
                    case 211:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.CurrDerivOrderPlaceBrokerReplyRec);
                        }
                        break;
                    case 221:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.CurrDerivOrderModifyBrokerReplyRec);
                        }
                        break;
                    case 231:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.CurrDerivOrderCancelBrokerReplyRec);
                        }
                        break;
                    case 200:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MarginReportReplyRec);
                        }
                        break;
                    case 206:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MarginReportReplyRecMCX);
                        }
                        break;
                    case 208:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MarginReportReplyRecCombined);
                        }
                        break;
                    #region MTM Added on 12-Sep-2016 - Rohan
                    case 203:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.ClientsforMTMReply);
                        }
                        break;
                    //case 208:   //Added on 08-Mar-2017 - Rohan
                    //    {
                    //        SendData = CommonFunctions.RawSerialize(ReplyToSend.ClientsforMTMReply);
                    //    }
                    //    break;
                    case 205:   //Added on 19-Sep-2016 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MTMClientsWithAllMarginsReply);
                        }
                        break;
                    case 207:   //Added on 08-Mar-2017 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MTMClientsWithAllMarginsMcxReply);
                        }
                        break;
                    #endregion
                    case 246:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.IISOrderTradeGeneral);
                        }
                        break;
                    case 209:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.TemplateMarginReply);
                        }
                        break;
                    case 172:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.TemplateMarginReply_Peak);
                        }
                        break;
                    case 210:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.TemplateWithDerivMargin);
                        }
                        break;
                    case 173:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.PeakMarginReplyData);
                        }
                        break;
                    case 217:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.TemplateMarginReply);
                        }
                        break;
                    case 250:   //Added on 19-Sep-2016 - Rohan
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.MTMClientsWithAllMarginsReplyInstrumentWise);
                        }
                        break;
                    case 248:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SegmentReply);
                        }
                        break;
                    //Option Exercise
                    case Constants.OTS_EXERCISE_PLACE_REQ:
                    case Constants.OTS_EXERCISE_MODIFY_REQ:
                    case Constants.OTS_EXERCISE_CANCEL_REQ:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OptionExerciseBrokerReplyRec);
                        }
                        break;
                    case 226:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.TradeReportReply);
                        }
                        break;
                    case Constants.OTS_OFS_WATCHLIST:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OFSWatchListReply);
                        }
                        break;
                    case Constants.OTS_OFS_PLACE_BROKER_REQ:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OfsPlaceOrderReply);
                        }
                        break;
                    case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OfsPlaceOrderReply);
                        }
                        break;
                    case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OfsPlaceOrderReply);
                        }
                        break;
                    case 125:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.OfsOrdersReportsReply);
                        }
                        break;
                    case 235:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SLBMOrderPlaceBrokerReplyRec);
                        }
                        break;
                    case 236:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SLBMOrderModifyBrokerReplyRec);
                        }
                        break;
                    case 237:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SLBMOrderCancelBrokerReplyRec);
                        }
                        break;
                    case 255:
                        {
                            SendData = CommonFunctions.RawSerialize(ReplyToSend.SLBMHoldingsReportReplyRec);
                        }
                        break;

            }
                return SendData;
            
        }

        private void AddLicenceInfoRequest()
        {
            try
            {
                ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();
                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserID = "db_one".PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderID = "db_one".PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 66;
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDummyRecord());
                OTSReqRec.AllReq.LicenceInfoRequestRec.Dummy = "000".PadRight(15).ToCharArray();
                PendingRequestList.Add(OTSReqRec);
                DBRequestCnt++;

                if (MaxDB == 1) return;

                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserID = "db_two".PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderID = "db_two".PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 66;
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDummyRecord());
                OTSReqRec.AllReq.LicenceInfoRequestRec.Dummy = "OTS".PadRight(15).ToCharArray();

                PendingRequestList.Add(OTSReqRec);
                DBRequestCnt++;
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }

        }

        private void AddReportRequest()
        {
            try
            {
                ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();
                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserID = "A".PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderID = "".PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 80;  //TradeToTrade
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDummyRecord());
                OTSReqRec.AllReq.LicenceInfoRequestRec.Dummy = "OTS".PadRight(15).ToCharArray();

                PendingRequestList.Add(OTSReqRec);

                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserID = "Z".PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderID = "".PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 81;  //BseNseMap
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDummyRecord());
                OTSReqRec.AllReq.LicenceInfoRequestRec.Dummy = "OTS".PadRight(15).ToCharArray();

                PendingRequestList.Add(OTSReqRec);

                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserID = "Z".PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderID = "".PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 82;  //Deriv Banned Scrip
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDummyRecord());
                OTSReqRec.AllReq.LicenceInfoRequestRec.Dummy = "OTS".PadRight(15).ToCharArray();


                PendingRequestList.Add(OTSReqRec);
                DBRequestCnt = DBRequestCnt + 3;

                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserID = "Z".PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderID = "".PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 68;  //Client DbNum
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDummyRecord());
                OTSReqRec.AllReq.CliDBNumRequestRec.DummyByte = 48;

                PendingRequestList.Add(OTSReqRec);
                DBRequestCnt++;

                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserID = "Z".PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderID = "".PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 89; //Illiquid
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDummyRecord());

                PendingRequestList.Add(OTSReqRec);
                DBRequestCnt++;
                SendPendingRequestFromList();
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void ClientDataIn(object Sender, ushort ErrCode)
        {
            try
            {
                ITInteractiveStructures.RawHeader RdHeader = new ITInteractiveStructures.RawHeader();
                TWSocketClient token;
                byte[] buff;
                token = Sender as TWSocketClient;
                int DataRcvdLen, ActualDataRcvdLen;

                DataRcvdLen = token.RcvdCount + 10;
                buff = new byte[DataRcvdLen];
                ActualDataRcvdLen = token.Receive(buff, DataRcvdLen);
                if (ActualDataRcvdLen <= 0)
                    return;
                if (ActualDataRcvdLen > 0)
                    token.SockBuf.Write(buff, ActualDataRcvdLen);

                if (checkBoxWriteToDisplayITS.Checked)
                    listBox1.Items.Insert(0, ActualDataRcvdLen + "  bytes recvd from ITS");

                //if (checkBoxWriteTodisply.Checked && token.ITS_Num != 0 && Convert.ToInt32(textBoxITSNum.Text) == token.ITS_Num)
                if (checkBoxWriteTodisply.Checked)
                {
                    byte[] temp = new byte[ActualDataRcvdLen + 12];
                    RdHeader.CurrTime = DateTime.Now;
                    RdHeader.ReadSize = ActualDataRcvdLen;
                    byte[] temp1 = CommonFunctions.RawSerialize(RdHeader);
                    Array.Copy(temp1, 0, temp, 0, temp1.Length);
                    Array.Copy(buff, 0, temp, 12, ActualDataRcvdLen);
                    ITSRawFileWriter.Write(temp);
                }
                ProcessITSRequest(token);
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void ProcessITSRequest(TWSocketClient Client)
        {
            #region Variable Declaration

            ITInteractiveStructures.TIARequestHeaderRecord IAReqHdr = new ITInteractiveStructures.TIARequestHeaderRecord();
            ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();
            ITInteractiveStructures.TAllRequestNewRecord AllReq = new ITInteractiveStructures.TAllRequestNewRecord();
            byte[] buffToRead = new byte[Marshal.SizeOf(new ITInteractiveStructures.TAllRequestNewRecord())];


            #endregion
            try
            {
                while (Client.SockBuf.BufUsed >= Marshal.SizeOf(IAReqHdr))
                {
                    Client.SockBuf.Peek(out buffToRead);
                    IAReqHdr = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TIARequestHeaderRecord>(buffToRead);
                    if (Client.SockBuf.BufUsed < (Marshal.SizeOf(IAReqHdr) + IAReqHdr.RequestLen))
                        return;
                    BytesFromITS++;
                    Client.SockBuf.Read(out buffToRead, Marshal.SizeOf(new ITInteractiveStructures.TIARequestHeaderRecord()));
                    IAReqHdr = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TIARequestHeaderRecord>(buffToRead);
                    if (ProcessFileITS)
                        WriteToDisplay(IAReqHdr);
                    Client.SockBuf.Read(out buffToRead, IAReqHdr.RequestLen);
                    AllReq = ReadBodyWithCases(IAReqHdr.RequestType, buffToRead);
                    count++;
                    string s = new string(IAReqHdr.FEUserID).Substring(0, IAReqHdr.FEUserIDLength);
                    string s1 = new string(IAReqHdr.TraderID).Substring(0, IAReqHdr.FEUserIDLength);
                    if (ProcessFileITS)
                        WriteToDisplay(IAReqHdr.RequestType, AllReq);
                    OTSReqRec.IP = Client.IPAddress.ToCharArray();
                    OTSReqRec.IPLength = (byte)Client.IPAddress.TrimEnd(TrimChars).Length;
                    OTSReqRec.ReqTime = DateTime.Now;
                    OTSReqRec.ReqHdr = IAReqHdr;
                    OTSReqRec.AllReq = AllReq;
                    OTSReqRec.RetryCnt = 0;
                    OTSReqRec.ITS_Num = Client.ITS_Num;
                    if (OTSReqRec.ReqHdr.RequestType == 0 || OTSReqRec.ReqHdr.SocketNum == 0)
                    {
                        Client.SockBuf.RdCount = 0;
                        Client.SockBuf.WrCount = 0;
                        Client.Close();
                        return;
                    }

                    if (OTSReqRec.ReqHdr.RequestType == 80)  //Trade To Trade ,  send from memory
                    {
                        OTSReplyRec_TradeToTrade.IPLength = OTSReqRec.IPLength;
                        OTSReplyRec_TradeToTrade.IP = new string(OTSReqRec.IP).PadRight(15).ToCharArray();
                        OTSReplyRec_TradeToTrade.ReplyHdr.SocketNum = OTSReqRec.ReqHdr.SocketNum;
                        OTSReplyRec_TradeToTrade.ReplyHdr.ClientType = OTSReqRec.ReqHdr.ClientType;
                        OTSReplyRec_TradeToTrade.ReplyHdr.TraderIDLength = OTSReqRec.ReqHdr.TraderIDLength;
                        OTSReplyRec_TradeToTrade.ReplyHdr.TraderID = new string(OTSReqRec.ReqHdr.TraderID).PadRight(14).ToCharArray();
                        OTSReplyRec_TradeToTrade.ReplyHdr.FEUserIDLength = OTSReqRec.ReqHdr.FEUserIDLength;
                        OTSReplyRec_TradeToTrade.ReplyHdr.FEUserID = new string(OTSReqRec.ReqHdr.FEUserID).PadRight(15).ToCharArray();
                        OTSReplyRec_TradeToTrade.ReplyHdr.ReplyType = OTSReqRec.ReqHdr.RequestType;
                        OTSReplyRec_TradeToTrade.ReplyHdr.ErrorCode = 0;
                        OTSReplyRec_TradeToTrade.ITS_Num = OTSReqRec.ITS_Num;
                        AddToPendingReplyList(OTSReplyRec_TradeToTrade);
                        continue;
                    }
                    else
                        if (IAReqHdr.RequestType == 81) //BseNseMap ,  send from memory
                        {
                            OTSReplyRec_BseNseMap.IPLength = OTSReqRec.IPLength;
                            OTSReplyRec_BseNseMap.IP = new string(OTSReqRec.IP).PadRight(15).ToCharArray();
                            OTSReplyRec_BseNseMap.ReplyHdr.SocketNum = OTSReqRec.ReqHdr.SocketNum;
                            OTSReplyRec_BseNseMap.ReplyHdr.ClientType = OTSReqRec.ReqHdr.ClientType;
                            OTSReplyRec_BseNseMap.ReplyHdr.TraderIDLength = OTSReqRec.ReqHdr.TraderIDLength;
                            OTSReplyRec_BseNseMap.ReplyHdr.TraderID = new string(OTSReqRec.ReqHdr.TraderID).PadRight(14).ToCharArray();
                            OTSReplyRec_BseNseMap.ReplyHdr.FEUserIDLength = OTSReqRec.ReqHdr.FEUserIDLength;
                            OTSReplyRec_BseNseMap.ReplyHdr.FEUserID = new string(OTSReqRec.ReqHdr.FEUserID).PadRight(15).ToCharArray();
                            OTSReplyRec_BseNseMap.ReplyHdr.ReplyType = OTSReqRec.ReqHdr.RequestType;
                            OTSReplyRec_BseNseMap.ReplyHdr.ErrorCode = 0;
                            OTSReplyRec_BseNseMap.ITS_Num = OTSReqRec.ITS_Num;
                            AddToPendingReplyList(OTSReplyRec_BseNseMap);
                            continue;
                        }
                        else
                            if (IAReqHdr.RequestType == 82)//Deriv Banned Scrip ,  send from memory
                            {
                                OTSReplyRec_DerivBannedScrip.IPLength = OTSReqRec.IPLength;
                                OTSReplyRec_DerivBannedScrip.IP = new string(OTSReqRec.IP).PadRight(15).ToCharArray();
                                OTSReplyRec_DerivBannedScrip.ReplyHdr.SocketNum = OTSReqRec.ReqHdr.SocketNum;
                                OTSReplyRec_DerivBannedScrip.ReplyHdr.ClientType = OTSReqRec.ReqHdr.ClientType;
                                OTSReplyRec_DerivBannedScrip.ReplyHdr.TraderIDLength = OTSReqRec.ReqHdr.TraderIDLength;
                                OTSReplyRec_DerivBannedScrip.ReplyHdr.TraderID = new string(OTSReqRec.ReqHdr.TraderID).PadRight(14).ToCharArray();
                                OTSReplyRec_DerivBannedScrip.ReplyHdr.FEUserIDLength = OTSReqRec.ReqHdr.FEUserIDLength;
                                OTSReplyRec_DerivBannedScrip.ReplyHdr.FEUserID = new string(OTSReqRec.ReqHdr.FEUserID).PadRight(15).ToCharArray();
                                OTSReplyRec_DerivBannedScrip.ReplyHdr.ReplyType = OTSReqRec.ReqHdr.RequestType;
                                OTSReplyRec_DerivBannedScrip.ReplyHdr.ErrorCode = 0;
                                OTSReplyRec_DerivBannedScrip.ITS_Num = OTSReqRec.ITS_Num;
                                AddToPendingReplyList(OTSReplyRec_DerivBannedScrip);
                                continue;
                            }
                            else
                                if (IAReqHdr.RequestType == 89)//Illiquid Report ,  send from memory
                                {
                                    OTSReplyRec_Illiquid.IPLength = OTSReqRec.IPLength;
                                    OTSReplyRec_Illiquid.IP = new string(OTSReqRec.IP).PadRight(15).ToCharArray();
                                    OTSReplyRec_Illiquid.ReplyHdr.SocketNum = OTSReqRec.ReqHdr.SocketNum;
                                    OTSReplyRec_Illiquid.ReplyHdr.ClientType = OTSReqRec.ReqHdr.ClientType;
                                    OTSReplyRec_Illiquid.ReplyHdr.TraderIDLength = OTSReqRec.ReqHdr.TraderIDLength;
                                    OTSReplyRec_Illiquid.ReplyHdr.TraderID = new string(OTSReqRec.ReqHdr.TraderID).PadRight(14).ToCharArray();
                                    OTSReplyRec_Illiquid.ReplyHdr.FEUserIDLength = OTSReqRec.ReqHdr.FEUserIDLength;
                                    OTSReplyRec_Illiquid.ReplyHdr.FEUserID = new string(OTSReqRec.ReqHdr.FEUserID).PadRight(15).ToCharArray();
                                    OTSReplyRec_Illiquid.ReplyHdr.ReplyType = OTSReqRec.ReqHdr.RequestType;
                                    OTSReplyRec_Illiquid.ReplyHdr.ErrorCode = 0;
                                    OTSReplyRec_Illiquid.ITS_Num = OTSReqRec.ITS_Num;
                                    AddToPendingReplyList(OTSReplyRec_Illiquid);
                                    continue;
                                }
                                else
                                    if (IAReqHdr.RequestType == 245)//ITS ID is coming
                                    {
                                        Client.IPAddress = new string(AllReq.ITS2OTSNumRec.Dummy).Substring(0, AllReq.ITS2OTSNumRec.DummyLength);
                                        Client.ITS_Num = AllReq.ITS2OTSNumRec.DummyByte;
                                        listBox1.Items.Add(Client.IPAddress + " Connected at :" + Client.SockCreateTime);
                                        continue;
                                    }
                                    else if (OTSReqRec.ReqHdr.RequestType == 91)  //Lot Quantity Report ,  send from memory
                                    {
                                        OTSReplyRec_LotQty.IPLength = OTSReqRec.IPLength;
                                        OTSReplyRec_LotQty.IP = new string(OTSReqRec.IP).PadRight(15).ToCharArray();
                                        OTSReplyRec_LotQty.ReplyHdr.SocketNum = OTSReqRec.ReqHdr.SocketNum;
                                        OTSReplyRec_LotQty.ReplyHdr.ClientType = OTSReqRec.ReqHdr.ClientType;
                                        OTSReplyRec_LotQty.ReplyHdr.TraderIDLength = OTSReqRec.ReqHdr.TraderIDLength;
                                        OTSReplyRec_LotQty.ReplyHdr.TraderID = new string(OTSReqRec.ReqHdr.TraderID).PadRight(14).ToCharArray();
                                        OTSReplyRec_LotQty.ReplyHdr.FEUserIDLength = OTSReqRec.ReqHdr.FEUserIDLength;
                                        OTSReplyRec_LotQty.ReplyHdr.FEUserID = new string(OTSReqRec.ReqHdr.FEUserID).PadRight(15).ToCharArray();
                                        OTSReplyRec_LotQty.ReplyHdr.ReplyType = OTSReqRec.ReqHdr.RequestType;
                                        OTSReplyRec_LotQty.ReplyHdr.ErrorCode = 0;
                                        OTSReplyRec_LotQty.ITS_Num = OTSReqRec.ITS_Num;
                                        OTSReplyRec_LotQty.ReplyHdr.ReplyCnt = Convert.ToUInt16(ArrayLotQty.Length);
                                        OTSReplyRec_LotQty.ReplyHdr.ReplyRecSize = Convert.ToUInt16(Marshal.SizeOf(new ITInteractiveStructures.TLotQuantityRecord()));
                                        OTSReplyRec_LotQty.ReplyHdr.ReplyLen = OTSReplyRec_LotQty.ReplyHdr.ReplyCnt * OTSReplyRec_LotQty.ReplyHdr.ReplyRecSize;

                                        Array.Resize(ref OTSReplyRec_LotQty.AllReply, OTSReplyRec_LotQty.ReplyHdr.ReplyCnt);

                                        for (int i = 0; i < OTSReplyRec_LotQty.ReplyHdr.ReplyCnt; i++)
                                        {
                                            OTSReplyRec_LotQty.AllReply[i].LotQtyReplyRec = ArrayLotQty[i];
                                        }
                                        AddToPendingReplyList(OTSReplyRec_LotQty);
                                        continue;
                                    }
                                    else
                                    {
                                        if (!ProcessFileITS)
                                        {
                                            DBRequestCnt++;
                                            AddToPendingRequestList(OTSReqRec);
                                        }
                                    }
                }

            }
            catch (ArgumentOutOfRangeException e)
            {
                Client.SockBuf.RdCount = 0;
                Client.SockBuf.WrCount = 0;
                Client.Close();
            }
        }

        private void AddToPendingReplyList(ITInteractiveStructures.TOTSReplyRecord OTSReplyRec)
        {
            if (OTSReplyRec.ReplyHdr.ReplyType == 171)
            {
                if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType == 'C')
                {
                    OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord_cover());
                    OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord_cover());

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Exch = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ExchType = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripCode = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripName = new string(OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName).PadRight(12).ToCharArray();
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripNameLength = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Qty = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.AtMarket = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.TriggerRate = 0.0F;// OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.TraderRequestID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.IOC = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.DisclosedQty = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.MarginBlocked = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.AHStatus = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus;
                    //if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1 == 'B')
                    //{
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell2 = " "[0];
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.OrderTypeNew = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderTypeNew;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.EpochTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.EpochTime;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerOrderID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1;

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.WithSL = 'N';
                    //}
                    //else if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1 == 'S')
                    //{
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell2 = " "[0];
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO;
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1;
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerOrderID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1;
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1;

                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.WithSL = 'N';
                    //}



                    PendingReplyList.Add(OTSReplyRec);
                    SendPendingReplyFromList();

                    OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord_cover());
                    OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord_cover());

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Exch = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ExchType = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripCode = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripName = new string(OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName).PadRight(12).ToCharArray();
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripNameLength = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Qty = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.AtMarket = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.TriggerRate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.TraderRequestID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.IOC = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.DisclosedQty = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.MarginBlocked = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.AHStatus = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus;
                    //if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2 == 'S')
                    //{
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.OrderTypeNew = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderTypeNew;

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.EpochTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.EpochTime;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerOrderID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID2;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime2;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.WithSL = 'Y';
                    // }
                    PendingReplyList.Add(OTSReplyRec);
                    SendPendingReplyFromList();
                }
                else if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType == 'D')
                {
                    OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecord_Cover());
                    OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecord_Cover());

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Exch = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ExchType = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripCode = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripName = new string(OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName).PadRight(12).ToCharArray();
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripNameLength = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Qty = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.AtMarket = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.TriggerRate = 0.0f; // OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.TraderRequestID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.IOC = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.DisclosedQty = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.MarginBlocked = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.AHStatus = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus;
                    //if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1 == 'B')
                    //{
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell2 = " "[0];
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.OrderTypeNew = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderTypeNew;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.EpochTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.EpochTime;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerOrderID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1;

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.WithSL = 'N';
                    //}
                    //else if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1 == 'S')
                    //{
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell2 = " "[0];
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO;
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell1;
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerOrderID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID1;
                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime1;

                    //OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.WithSL = 'N';
                    //}
                    if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType == 'D')
                    {
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.CALevel = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CALevel;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.CPType = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CPType;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.StrikePrice = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.StrikePrice;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.InstrType = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.InstrType;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ExpiryDate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExpiryDate;
                    }

                    PendingReplyList.Add(OTSReplyRec);
                    SendPendingReplyFromList();


                    OTSReplyRec.ReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecord_Cover());
                    OTSReplyRec.ReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderPlaceBrokerReplyRecord_Cover());

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Exch = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Exch;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ExchType = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripCode = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripCode;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripName = new string(OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripName).PadRight(12).ToCharArray();
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ScripNameLength = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ScripNameLength;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Qty = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Qty;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.AtMarket = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AtMarket;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.Rate;

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.TriggerRate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TriggerRate;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.TraderRequestID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.TraderRequestID;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.IOC = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.IOC;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.DisclosedQty = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.DisclosedQty;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.MarginBlocked = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.MarginBlocked;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.AHStatus = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.AHStatus;
                    //if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2 == 'S')
                    //{
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.OrderTypeNew = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderTypeNew;

                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.EpochTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.EpochTime;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.Rate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.SLRateCO;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BuySell = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BuySell2;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerOrderID = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerOrderID2;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.BrokerTime = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.BrokerTime2;
                    OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.WithSL = 'Y';
                    // }

                    if (OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExchType == 'D')
                    {
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.CALevel = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CALevel;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.CPType = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.CPType;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.StrikePrice = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.StrikePrice;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.InstrType = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.InstrType;
                        OTSReplyRec.AllReply[0].EquityOrderPlaceBrokerReplyRecord_cover.ExpiryDate = OTSReplyRec.AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ExpiryDate;
                    }

                    PendingReplyList.Add(OTSReplyRec);
                    SendPendingReplyFromList();

                }

            }
            else
            {
                PendingReplyList.Add(OTSReplyRec);
                SendPendingReplyFromList();
            }
        }

        private void AddToPendingRequestList(ITInteractiveStructures.TOTSRequestRecord OTSReqRec)
        {
            PendingRequestList.Add(OTSReqRec);
            SendPendingRequestFromList();
        }

        private ITInteractiveStructures.TAllRequestNewRecord ReadBodyWithCases(byte ReqType, byte[] buffToReadBody)
        {
                ITInteractiveStructures.TAllRequestNewRecord AllReq = new ITInteractiveStructures.TAllRequestNewRecord();
                switch (ReqType)
                {
                    case 1:
                    case 3:
                        {
                            AllReq.LoginRequest = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TLoginRequestRecord>(buffToReadBody);
                        }
                        break;
                case 85:
                    {
                        AllReq.LoginRequestTwoFactor = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TLoginRequestRecordTwoFactor>(buffToReadBody);
                    }
                    break;
                case 4:
                        {
                            AllReq.LoginRequestForName = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TFetchClientInfoRequest>(buffToReadBody);
                        }
                        break;
                    case 2:
                        {
                            AllReq.ChgPswdRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TChangePasswordRequestRecord>(buffToReadBody);
                            AllReq.ChgPswdRequestRec.MachineIDLength = Math.Min(AllReq.ChgPswdRequestRec.MachineIDLength, (byte)15);
                        }
                        break;
                    case 5:
                        {
                            AllReq.TraderListRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderListRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 6:
                        {
                            AllReq.VerifyTraderRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TVerifyTraderRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 10:
                        {
                            AllReq.EquityOrderPlaceRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderPlaceRequestRecord>(buffToReadBody);
                        }
                        break;
                case 14:
                    {
                        AllReq.GetIVRDealerDetailsReq = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.GetIVRDealerDetailsReq>(buffToReadBody); //24.04.23 For getting IVR Dealer Details		
                    }
                    break;
                    case 15:
                        {
                            AllReq.AHEquityOrderPlaceRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TAHEquityOrderPlaceRequestRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_PLACE_REQ:
                        {
                            AllReq.OrderTypePlaceReqRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceRequestRecord>(buffToReadBody);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_PLACE_REQ_COVER:
                    {
                        AllReq.PlaceCoverOrderReqRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceRequestRecordCoverAndBracket>(buffToReadBody);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                        {
                            AllReq.OrderTypeModifyReqRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderModifyRequestRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                        {
                            AllReq.OrderTypeCancelReqRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderCancelRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 20:
                        {
                            AllReq.EquityOrderModifyRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderModifyRequestRecord>(buffToReadBody);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_DERIVCONVERSION:
                    case Constants.OTS_ORDERTYPE_CONVERSION: //14.01.15
                        {
                            AllReq.OrderTypeConversionRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderConversionRec>(buffToReadBody);
                        }
                        break;
                    case 30:
                        {
                            AllReq.EquityOrderCancelRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderCancelRequestRecord>(buffToReadBody);
                        }
                        break;

                    case 41:
                        {
                            AllReq.ScripMarginRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TScripMarginRequestRecord>(buffToReadBody);
                        }
                        break;
                    //Addition of Scrip Margin 2015.03.26
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ://34
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                        {
                            AllReq.MfSIPRegistrationReqRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFSIPRegistrationMsgRequestRecord>(buffToReadBody);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ://34
                    {
                        AllReq.XMfSIPRegistrationReqRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.XBseMFSIPRegistrationMsgRequestRecord>(buffToReadBody);
                    }
                    break;
                case Constants.OTS_MANDATE:
                    {
                        AllReq.XSIPMandateRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.XSIPMandate>(buffToReadBody);
                    }break;
                case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                        {
                            AllReq.MfAddModifyCancelReqRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFAddModifyCancelRequestRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_SECURITY_INFO:
                        {
                            AllReq.ScripInfoRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TScripInfoRequestRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_SCRIP_MARGIN:
                        {
                            AllReq.ScripMarginRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TScripMarginRequestRecord>(buffToReadBody);
                        }
                        break;
                    //end Addition of Scrip Margin 2015.03.26
                    //case 61:
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC:
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142:    //Added on 5-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW:    //Added on 18-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER:     // sumitp 28_jan_22
                    {
                            AllReq.TradeReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTradeReportRequestRecord>(buffToReadBody);
                        }
                        break;
                //case 62:
                    case 125:
                    case 7:
                    case 8:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141: //Added on 5-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW: //Added on 18-Apr-2018 - Rohan
                        {
                            AllReq.OrderReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderReportRequestRecord>(buffToReadBody);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_COVER_BRACKET: // sumitp 28_jan_22
                    {
                        AllReq.OrderReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderReportRequestRecord>(buffToReadBody);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_EXERCISE_REPORTS_REC://Options Exercise
                        {
                            AllReq.ExerciseReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TExerciseReportRequestRecord>(buffToReadBody);

                        }
                        break;
                    //case 64:
                    case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC:
                    case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW:
                    case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW:  //Added on 18-Apr-2018 -Rohan
                        {
                            AllReq.ActivityLogRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TActivityLogRequestRecord>(buffToReadBody);
                        }
                        break;
                case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new:  //Added on 18-Apr-2018 -Rohan
                    {
                        AllReq.ActivityLogRequestRecord_new = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TActivityLogRequestRecord_new>(buffToReadBody);
                    }
                    break;
                case 69:
                    case 74:
                    case 75:
                    case 92:
                    case Constants.OTS_HOLDING_REPORT:// 94 Mcx 19.01.16
                    case Constants.OTS_HOLDING_REPORT_PreClose: // 106 jinal 
                        {
                            AllReq.HoldingsReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_HOLDING_REPORT_PreClose_FS: // 112 Himanshu
                        {
                            AllReq.HoldingsReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                case 67:
                        {
                            AllReq.HoldingsReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                 case 90:
                    {
                        AllReq.HoldingsReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                    }
                    break;
                case 194:
                    {
                        AllReq.HoldingsReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                    }
                    break;
                case Constants.OTS_ALLCLIENTS_HOLDING_REPORT://95 31.01.17
                        {
                            AllReq.HoldingsReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_ALLCLIENTS_HOLDING_REPORT_New://95 31.01.17
                        {
                            AllReq.HoldingsReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                    case 76:    //Added on 08-Nov-2016 - Rohan
                        {
                            AllReq.HoldingsEquityReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                    case 70:
                        {
                            AllReq.HoldingsValuationReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                    case 218:
                        {
                            AllReq.HoldingsValuationReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_HOLDING_VALUATION_REPORT_NEW://20.05.15
                        {
                            AllReq.HoldingsValuationReportWithMemVarRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                    case 79:
                        {
                            AllReq.ClientPositionRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TClientPositionReportRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 83:
                        {
                            AllReq.AdhocReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TAdhocReportRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 84:
                        {
                            AllReq.AdhocModifyRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TAdhocModifyRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 86://Pending to check
                        {
                            AllReq.MFClientsReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TMaalFodoClientsReportRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 87://Pending to check
                        {
                            AllReq.MFClientHoldingReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TMaalFodoClientHoldingReportRequestRecord>(buffToReadBody);
                        }
                        break;

                    case Constants.OTS_VERIFY_USER:
                        {
                            AllReq.VerifyUserRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TVerifyUserRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 91://Pending to check
                        {
                            AllReq.LotQtyRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_ORDER_REPORTS_REC://bsemf Krutika
                        {
                            AllReq.MfOrderReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMfOrderReportRequestRecord>(buffToReadBody);
                        } break;
                case Constants.OTS_Acitivitylog_MF_ORDER_REPORTS_REC://bsemf Krutika
                    {
                        AllReq.MfActivitylogRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMfActivitylogRequestRecord>(buffToReadBody);
                    }
                    break;
                case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC://bsemf Krutika
                        {
                            AllReq.MfOrderReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMfOrderReportRequestRecord>(buffToReadBody);
                        } break;
                case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC:
                    {
                        AllReq.MandateOrderReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.MandateOrderReportRequestRecord>(buffToReadBody);
                    } break;
                case 110:
                        {
                            AllReq.DerivOrderPlaceRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderPlaceRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 120:
                        {
                            AllReq.DerivOrderModifyRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderModifyRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 130:
                        {
                            AllReq.DerivOrderCancelRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderCancelRequestRecord>(buffToReadBody);
                        }
                        break;
                    //02.03.0
                case 134:
                    {
                        AllReq.orderPlaceReqRecForApproval = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.OrderPlaceReqRecForApproval>(buffToReadBody);
                    }
                    break;
                case 135:
                    {
                        AllReq.UpdateOTPOrderStatusRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.UpdateOTPOrderStatus>(buffToReadBody);
                    }
                    break;
                case 136:
                    {
                        AllReq.OrderInProcessReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderReportRequestRecord>(buffToReadBody);//CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceRequestRecord>(buffToReadBody);
                    }
                    break;
                    case 191:
                        {
                            AllReq.SpreadOrderPlaceRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSpreadOrderPlaceRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 192:
                    case 193:
                        {
                            AllReq.SpreadOrderModifyCancelRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSpreadOrderCancelRequestRecord>(buffToReadBody);
                        }
                        break;
                    #region MTM 12-Sep-2016 - Rohan
                    case 203:
                        {
                            AllReq.UpdateMTMLimitHitReq = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TUpdateClientsForMTM>(buffToReadBody);
                        }
                        break;
                    case 208:
                        {
                            AllReq.UpdateMTMLimitHitReqMcx = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TUpdateClientsForMTM>(buffToReadBody);
                        }
                        break;
                    case 205:   //Added on 19-Sep-2016 - Rohan
                        {
                            AllReq.ClientsForMTMReq = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecordForMTM>(buffToReadBody);
                        }
                        break;
                    case 207:   //Added on 08-Mar-2017 - Rohan
                        {
                            AllReq.ClientsForMTMReqMcx = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecordForMTM>(buffToReadBody);
                        }
                        break;
                    #endregion
                    case 245:
                        {
                            AllReq.ITS2OTSNumRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDummyRecord>(buffToReadBody);
                        }
                        break;
                    case 252:
                        {
                            AllReq.LogoutRequest = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TLogoutRequestRecord>(buffToReadBody);
                        }
                        break;
                case 247:
                    {
                        AllReq.LogoutRequest_2FA = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TLogoutRequestRecord_2FA>(buffToReadBody);
                    }
                    break;
                case 211:
                        {
                            AllReq.CurrDerivOrderPlaceRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderPlaceRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 221:
                        {
                            AllReq.CurrDerivOrderModifyRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderModifyRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 231:
                        {
                            AllReq.CurrDerivOrderCancelRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderCancelRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 246:
                        {
                            AllReq.IISOrderTradeGenreal = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderTradeGeneral>(buffToReadBody);
                        }
                        break;
                    case 209:
                        {
                            AllReq.TemplateMargin = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTemplateMarginRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 172:
                        {
                            AllReq.TemplateMargin_Peak = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTemplateMarginRequestRecord_Peak>(buffToReadBody);
                        }
                        break;
                    case 210:
                        {
                            AllReq.TemplateMarginNew = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTemplateMarginRequestRecordNew>(buffToReadBody);
                        }
                        break;
                    case 173:
                        {
                            AllReq.PeakMarginData = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TPeakMarginData>(buffToReadBody);
                        }
                        break;
                    case 217:
                        {
                            AllReq.TemplateMargin = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTemplateMarginRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 250:   //Added on 19-Sep-2016 - Rohan
                        {
                            AllReq.ClientsForMTMReqInstrumentWise = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTraderIDRecordForSurveillance>(buffToReadBody);
                        }
                        break;
                    //Option Exercise
                    case Constants.OTS_EXERCISE_PLACE_REQ:
                    case Constants.OTS_EXERCISE_MODIFY_REQ:
                    case Constants.OTS_EXERCISE_CANCEL_REQ:
                        {
                            AllReq.OptionExerciseRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOptionExerciseRequestRecord>(buffToReadBody);
                        }
                        break;
                    case 226:
                        {
                            AllReq.GetTradesReport = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TTradesReportExchFormatRequest>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_OFS_WATCHLIST:
                        {
                            AllReq.OrderReportRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderReportRequestRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_OFS_PLACE_BROKER_REQ:
                        {
                            AllReq.PlaceOfsOrderRequest = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyRequestRecordOFS>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                        {
                            AllReq.PlaceOfsOrderRequest = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyRequestRecordOFS>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                        {
                            AllReq.PlaceOfsOrderRequest = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyRequestRecordOFS>(buffToReadBody);
                        }
                        break;
                    case 235: //21.11.18 SLBM Order Place
                    {
                        AllReq.SLBMOrderPlaceRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSLBMOrderPlaceRequestRecord>(buffToReadBody);
                    }
                    break;
                    case 236: //21.11.18 SLBM Order Modify
                    {
                        AllReq.SLBMOrderModifyRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSLBMOrderModifyRequestRecord>(buffToReadBody);
                    }
                    break;
                    case 237: //21.11.18 SLBM Order Cancel
                    {
                        AllReq.SLBMOrderCancelRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSLBMOrderCancelRequestRecord>(buffToReadBody);
                    }
                    break;

            }
                return AllReq;
            
        }

        private void btnExchConnect1_Click(object sender, EventArgs e)
        {
            if (!Startup()) return;

            if (btnExchConnect1.Text == "Connect")
            {
                ExchConnectorSoc1 = new SocketClient(1, this);
                if (!ExchConnectorSoc1.Connected)
                {
                    try
                    {
                        ExchConnectorSoc1.address = IPAddress.Parse(textBoxExchConn1IP.Text);
                        ExchConnectorSoc1.port = 18601;
                    }
                    catch (Exception er)
                    {
                        MessageBox.Show("An Invalid IP and/or port is specified in EC1");
                        return;
                    }
                    ExchConnectorSoc1.Connect();
                }
            }
            else
            {
                if (ExchConnectorSoc1.Connected)
                    ExchConnectorSoc1.Close();
                else
                    ExchConnectorConnectionStatusChanged(1);

            }
        }

        public void ExchConnectorConnectionStatusChanged(int Ecno)
        {
            try
            {
                if (Ecno == 1)
                {
                    if (ExchConnectorSoc1.clientSocket.Connected)
                    {
                        StatusLabelEC1Connect.Image = ImgGreen.Image;
                        btnExchConnect1.Text = "Disconnect";

                    }
                    else
                    {
                        btnExchConnect1.Text = "Connect";
                        StatusLabelEC1Connect.Image = ImgRed.Image;
                    }
                }
                else
                {
                    if (ExchConnectorSoc2.clientSocket.Connected)
                    {
                        StatusLabelEC2Connect.Image = ImgGreen.Image;
                        btnExchConnect2.Text = "Disconnect";

                    }
                    else
                    {
                        btnExchConnect2.Text = "Connect";
                        StatusLabelEC2Connect.Image = ImgRed.Image;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        public void ProcessDataAvailable(int ECNo, SocketAsyncEventArgs e)
        {
            try
            {
                ITInteractiveStructures.RawHeader RdHeader = new ITInteractiveStructures.RawHeader();
                byte[] buff = new byte[e.BytesTransferred];
                if (e.BytesTransferred <= 0)
                    return;
                Array.Copy(e.Buffer, e.Offset, buff, 0, e.BytesTransferred);
                if (ECNo == 1)
                {
                    ExchConnectorSoc1.SockBufExchConn.write(buff, e.BytesTransferred);
                    if (checkBoxWriteToDisplayEC1.Checked)
                        listBox1.Items.Insert(0, e.BytesTransferred + "  bytes recvd from EC 1");
                    BytesFromEC1Time = DateTime.Now;
                    BytesFromEC1++;
                    if (checkBoxWriteEC.Checked)
                    {
                        byte[] temp = new byte[e.BytesTransferred + 12];
                        RdHeader.CurrTime = DateTime.Now;
                        RdHeader.ReadSize = e.BytesTransferred;
                        byte[] temp1 = CommonFunctions.RawSerialize(RdHeader);
                        Array.Copy(temp1, 0, temp, 0, temp1.Length);
                        Array.Copy(buff, 0, temp, 12, e.BytesTransferred);
                        ECRawFileWriter.Write(temp);
                    }
                    ProcessExchDataIn(1);

                }
                else
                {
                    ExchConnectorSoc2.SockBufExchConn.write(buff, e.BytesTransferred);
                    if (checkBoxWriteToDisplayEC2.Checked)
                        listBox1.Items.Insert(0, e.BytesTransferred + "  bytes recvd from EC 2");
                    BytesFromEC2++;
                    ProcessExchDataIn(2);
                }
            }
            catch (Exception er)
            {
                ErrorLog(er);
            }
        }

        private void ProcessExchDataIn(int ECNo)
        {
            ITInteractiveStructures.TIAReplyHeaderRecord IAReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord();
            ITInteractiveStructures.TAllReplyNewRecord AllReply = new ITInteractiveStructures.TAllReplyNewRecord();
            ITInteractiveStructures.TIAErrorMessageRecord IAError = new ITInteractiveStructures.TIAErrorMessageRecord();
            ITInteractiveStructures.TIAReplyHeaderRecord IAReplyHdrNew;
            ITInteractiveStructures.TIAReplyHeaderRecord IAReplyHdrNewDealer; //24.09.16 Hetal
            TWSocketClient CurrCli;
            byte[] buffToReadHeader = new byte[Marshal.SizeOf(IAReplyHdr)];
            byte[] buffToReadError = new byte[Marshal.SizeOf(IAError)];
            byte[] buffToReadBody = new byte[Marshal.SizeOf(AllReply)];
            int ReqLen; bool isOrderType = false, IsCheckDealerType; byte[] NewBytesToSend, NewHdrBytes;
            byte[] NewBytesToSendDealer, NewHdrBytesForDealer;  //15.10.2016 Hetal
            Buffer SockBuf;
            if (ECNo == 1)
                SockBuf = ExchConnectorSoc1.SockBufExchConn;
            else
                SockBuf = ExchConnectorSoc2.SockBufExchConn;
            try
            {
                while (SockBuf.GetBuffUsed() >= Marshal.SizeOf(IAReplyHdr))
                {
                    SockBuf.peekUpto(buffToReadHeader, buffToReadHeader.Length);
                    IAReplyHdr = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TIAReplyHeaderRecord>(buffToReadHeader);
                    ReqLen = Marshal.SizeOf(IAReplyHdr) + IAReplyHdr.ReplyLen;
                    if (IAReplyHdr.ErrorCode != 0)
                    {
                        ReqLen = Marshal.SizeOf(new ITInteractiveStructures.TIAErrorMessageRecord()) + ReqLen;
                    }
                    if (SockBuf.GetBuffUsed() < ReqLen)
                        return;
                    if (checkBoxWriteToDisplayITS.Checked)
                        listBox1.Items.Insert(0, "Sending to ITS " + ReqLen + " bytes of msg type " + IAReplyHdr.ReplyType + " for " + new string(IAReplyHdr.TraderID).Substring(0, IAReplyHdr.TraderIDLength));

                    SockBuf.Read(buffToReadHeader, buffToReadHeader.Length);
                    IAReplyHdr = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TIAReplyHeaderRecord>(buffToReadHeader);
                    if (IAReplyHdr.ErrorCode != 0)
                    {
                        SockBuf.Read(buffToReadError, buffToReadError.Length);
                        IAError = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TIAErrorMessageRecord>(buffToReadError);
                    }
                    SockBuf.Read(buffToReadBody, IAReplyHdr.ReplyLen);
                    AllReply = ReadBodyWithExchConnBodyNew(IAReplyHdr.ReplyType, buffToReadBody, IAReplyHdr, out isOrderType, out NewBytesToSend, out IAReplyHdrNew);
                    AllReply = ReadBodyWithExchConnBodyNewWithDealerCheck(IAReplyHdr.ReplyType, buffToReadBody, IAReplyHdr, out IsCheckDealerType, out NewBytesToSendDealer, out IAReplyHdrNewDealer); // 15.10.2016 Hetal
                    if (isOrderType)
                    {
                        NewHdrBytes = CommonFunctions.RawSerialize(IAReplyHdrNew);
                        for (int i = 0; i < ListenToITS.FClientList.Count; i++)
                        {
                            CurrCli = (TWSocketClient)ListenToITS.FClientList[i];
                            if (CurrCli == null) continue;
                            logData(NewHdrBytes, NewHdrBytes.Length, CurrCli.ITS_Num, 0);
                            CurrCli.PutDataInSendBuffer(NewHdrBytes, NewHdrBytes.Length);

                            if (IAReplyHdrNew.ErrorCode != 0)
                            {
                                logData(buffToReadError, buffToReadError.Length, CurrCli.ITS_Num, 1);
                                CurrCli.PutDataInSendBuffer(buffToReadError, buffToReadError.Length);
                            }
                            logData(NewBytesToSend, IAReplyHdrNew.ReplyLen, CurrCli.ITS_Num, 2);
                            CurrCli.Send(NewBytesToSend, IAReplyHdrNew.ReplyLen);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ListenToITS.FClientList.Count; i++)
                        {
                            CurrCli = (TWSocketClient)ListenToITS.FClientList[i];
                            if (CurrCli == null) continue;
                            logData(buffToReadHeader, buffToReadHeader.Length, CurrCli.ITS_Num, 0);
                            CurrCli.PutDataInSendBuffer(buffToReadHeader, buffToReadHeader.Length);

                            if (IAReplyHdr.ErrorCode != 0)
                            {
                                logData(buffToReadError, buffToReadError.Length, CurrCli.ITS_Num, 1);
                                CurrCli.PutDataInSendBuffer(buffToReadError, buffToReadError.Length);
                            }
                            logData(buffToReadBody, IAReplyHdr.ReplyLen, CurrCli.ITS_Num, 2);
                            CurrCli.Send(buffToReadBody, IAReplyHdr.ReplyLen);
                        }
                    }
                }
            }
            catch (Exception er)
            {
                ErrorLog(er);
            }
        }

        private ITInteractiveStructures.TAllReplyNewRecord ReadBodyWithExchConnBody(byte ReplyType, byte[] buffToReadBody)
        {
                ITInteractiveStructures.TAllReplyNewRecord AllReply = new ITInteractiveStructures.TAllReplyNewRecord();
                switch (ReplyType)
                {
                    case 10:
                        {
                            AllReply.EquityOrderPlaceBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderPlaceBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 11:
                        {
                            AllReply.EquityOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderExchReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 20:
                        {
                            AllReply.EquityOrderModifyBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderModifyBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 30:
                        {
                            AllReply.EquityOrderCancelBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderCancelBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 110:
                        {
                            AllReply.DerivOrderPlaceBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderPlaceBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 120:
                        {
                            AllReply.DerivOrderModifyBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderModifyBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 130:
                        {
                            AllReply.DerivOrderCancelBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderCancelBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 51:
                        {
                            AllReply.SLTriggerRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSLTriggerRecord>(buffToReadBody);
                        }
                        break;
                    case 212:
                        {
                            AllReply.CurrDerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderExchReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 211:
                        {
                            AllReply.CurrDerivOrderPlaceBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 221:
                        {
                            AllReply.CurrDerivOrderModifyBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderModifyBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 231:
                        {
                            AllReply.CurrDerivOrderCancelBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderCancelBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                        {
                            AllReply.MFSIPRegistrationBrokerResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFSIPRegistrationMsgBrokerReply>(buffToReadBody);//BseMf
                        }break;
                    case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                        {
                            AllReply.MFAddModifyCancelBrokerResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFAddModifyCancelBrokerResponse>(buffToReadBody);//BseMf
                        }
                        break;
                    case Constants.OTS_MANDATE:
                        {
                            AllReply.XSIPMandateBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.XSIPMandateBrokerReply>(buffToReadBody);//BseMf
                        }break;
                }
                return AllReply;
           
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            ServiceReference2.MFUploadServiceClient client = new ServiceReference2.MFUploadServiceClient();

            var z = client.getPassword(textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text);
            //string[] bsePassArray = textBox3.Split('|');
            if (z == "100")
                MessageBox.Show("Login Successfully");
            // toolStripMFStatus.Image = ImgGreen.Image;
            else
                MessageBox.Show("Login Failed");
        }

        private void checkwritereply_CheckedChanged(object sender, EventArgs e)
        {
            TCustomWSocketLogger.isLogEnabled = checkwritereply.Checked;
        }


        private void btnCtrlProcessFile_Click(object sender, EventArgs e)
        {

            //string[] data = File.ReadAllLines(IDayDataPath + txtCtrlFName.Text);

            //TIARequestHeaderRecord IAHdr;
            //TIAErrorMessageRecord IAError;
            //TAllReplyNewRecord[] AllReplyArray;
            //TGlobalHeader GHdr;
            //List<string> list = new List<string>();

            //int junkCount = 0;
            //byte[] b;
            //string Fname = txtCtrlFName.Text;

            //FileStream fs = new FileStream(IDayDataPath + Fname, FileMode.Open, FileAccess.Read, FileShare.Read);
            //AllReplyArray = new TAllReplyNewRecord[0];
            //while (fs.Position < fs.Length)
            //{
            //    GHdr = fs.ReadStruct<TGlobalHeader>();

            //    list.Add("");
            //    list.Add("");
            //    list.Add("--------Time- " + GHdr.DateTime.ToString("hh:mm:ss:fff") + ", Flow- ST" + (char)GHdr.SentFrom + "----------------------------------------------------------------------------------------");
            //    byte[] arr = new byte[GHdr.Size];
            //    fs.Read(arr, 0, GHdr.Size);
            //    IAHdr = arr.ToStructure<TIARequestHeaderRecord>();// fs.ReadStruct<TIAReplyHeaderRecord>();
            //    //IAHdr = fs.ReadStruct<TIARequestHeaderRecord>();
            //    try
            //    {
            //        list.Add("Header - " + IAHdr.RequestType);
            //        list.Add("-------------------------");
            //        list.Add("AccountType-" + IAHdr.AccountType);
            //        list.Add("ChannelID_BC-" + IAHdr.ChannelID_BC);
            //        list.Add("ChannelID_NC-" + IAHdr.ChannelID_NC);
            //        list.Add("ChannelID_ND-" + IAHdr.ChannelID_ND);
            //        list.Add("ClientType-" + IAHdr.ClientType);
            //        //list.Add("ErrorCode-" + IAHdr.ErrorCode);
            //        list.Add("FELength-" + IAHdr.FELength);
            //        list.Add("FEUserID-" + new string(IAHdr.FEUserID, 0, IAHdr.FELength));
            //        //list.Add("ReplyCnt-" + IAHdr.ReplyCnt);
            //        list.Add("ReqLen-" + IAHdr.RequestLen);
            //        //list.Add("ReplyRecSize-" + IAHdr.ReplyRecSize);
            //        list.Add("ReqType-" + IAHdr.RequestType);
            //        list.Add("SocketNum-" + IAHdr.SocketNum);
            //        list.Add("TraderID-" + new string(IAHdr.TraderID, 0, IAHdr.TraderLength));
            //        list.Add("TraderLength-" + IAHdr.TraderLength);

            //        list.Add("");
            //        list.Add("Body - " + IAHdr.RequestLen);
            //        list.Add("-------------------------");
            //        list.Add(Encoding.ASCII.GetString(arr, 62, GHdr.Size - 62));
            //        //b = new byte[(GHdr.Size - 62)];
            //        //fs.Read(b, 0, b.Length);
            //    }
            //    catch (Exception ex)
            //    {
            //        list.Add("JUNK - " + ++junkCount);
            //        list.Add(Encoding.ASCII.GetString(arr));
            //    }
            //}
            //fs.Close();
            //File.WriteAllLines(ReportsFilePathName + Fname + ".txt", list);
            //MessageBox.Show("Done.");

            TIAReplyHeaderRecord IAHdr;
            TIAErrorMessageRecord IAError;
            TAllReplyNewRecord[] AllReplyArray;
            TGlobalHeader GHdr;
            List<string> list = new List<string>();


            byte[] b;
            string Fname = txtCtrlFName.Text;

            FileStream fs = new FileStream(/*IDayDataPath + */Fname, FileMode.Open, FileAccess.Read, FileShare.Read);
            AllReplyArray = new TAllReplyNewRecord[0];
            int junkCount = 0;
            int BodyCount = 0, BodyLen = 0;
            TGlobalHeader BodyHdr = new TGlobalHeader();
            List<string> listCSV = new List<string>();
            listCSV.Add("Time,SentFrom,Size,ReplyType,AccountType,ChannelID_BC,ChannelID_NC,ChannelID_ND" +
                ",ClientType,ErrorCode,FEUserIDLength,FEUserID,ReplyCnt" +
                ",ReplyLen,ReplyRecSize,ReplyType,SocketNum,TraderID,TraderIDLength");
            DateTime LastDateTime = DateTime.Now;
            TimeSpan ts = new TimeSpan(09, 00, 00);
            while (fs.Position < fs.Length)
            {
                GHdr = fs.ReadStruct<TGlobalHeader>();
                if ((GHdr.DateTime - LastDateTime).TotalSeconds > 20 && GHdr.DateTime.TimeOfDay > ts)//Break of 20 Seconds. Mark as Data Missing
                    listCSV.Add("Missing Data");
                LastDateTime = GHdr.DateTime;
                if (GHdr.SentFrom == 2)
                {
                    if (BodyCount == 0)
                        BodyHdr = GHdr;
                    BodyCount++;
                    BodyLen += GHdr.Size;
                }
                else
                {
                    if (BodyCount > 0)
                    {
                        list.Add("");
                        list.Add($"--------Time- {BodyHdr.DateTime:hh:mm:ss:fff}, Flow-{BodyHdr.SentFrom}, Size-{BodyHdr.Size}, Count-{BodyCount}, TotalSize-{BodyLen}---------");
                    }
                    list.Add("");
                    list.Add("");
                    list.Add("--------Time- " + GHdr.DateTime.ToString("hh:mm:ss:fff") + ", Flow- ST" + GHdr.SentFrom.ToString() + "---------Size-" + GHdr.Size + "-------------------------------------------------------------------------------");
                    BodyCount = 0;
                    BodyLen = 0;
                }
                byte[] arr = new byte[GHdr.Size];
                fs.Read(arr, 0, GHdr.Size);
                if (GHdr.SentFrom == 0)
                {
                    IAHdr = arr.ToStructure<TIAReplyHeaderRecord>();// fs.ReadStruct<TIAReplyHeaderRecord>();
                    try
                    {
                        list.Add("Header - " + IAHdr.ReplyType);
                        list.Add("-------------------------");
                        list.Add("AccountType-" + IAHdr.AccountType);
                        list.Add("ChannelID_BC-" + IAHdr.ChannelID_BC);
                        list.Add("ChannelID_NC-" + IAHdr.ChannelID_NC);
                        list.Add("ChannelID_ND-" + IAHdr.ChannelID_ND);
                        list.Add("ClientType-" + IAHdr.ClientType);
                        list.Add("ErrorCode-" + IAHdr.ErrorCode);
                        list.Add("FELength-" + IAHdr.FEUserIDLength);
                        list.Add("FEUserID-" + new string(IAHdr.FEUserID).Substring(0, IAHdr.FEUserIDLength));
                        list.Add("ReplyCnt-" + IAHdr.ReplyCnt);
                        list.Add("ReplyLen-" + IAHdr.ReplyLen);
                        list.Add("ReplyRecSize-" + IAHdr.ReplyRecSize);
                        list.Add("ReplyType-" + IAHdr.ReplyType);
                        list.Add("SocketNum-" + IAHdr.SocketNum);
                        list.Add("TraderID-" + new string(IAHdr.TraderID).Substring(0, IAHdr.TraderIDLength));
                        list.Add("TraderLength-" + IAHdr.TraderIDLength);

                        listCSV.Add($"{GHdr.DateTime.ToString("hh:mm:ss:fff")},{GHdr.SentFrom},{GHdr.Size},{IAHdr.ReplyType},{IAHdr.AccountType},{IAHdr.ChannelID_BC},{IAHdr.ChannelID_NC},{IAHdr.ChannelID_ND}" +
                            $",{IAHdr.ClientType},{IAHdr.ErrorCode},{IAHdr.FEUserIDLength},{new string(IAHdr.FEUserID).Substring(0, IAHdr.FEUserIDLength)},{IAHdr.ReplyCnt}" +
                            $",{IAHdr.ReplyLen},{IAHdr.ReplyRecSize},{IAHdr.ReplyType},{IAHdr.SocketNum},{new string(IAHdr.TraderID).Substring(0, IAHdr.TraderIDLength)},{IAHdr.TraderIDLength}");


                    }
                    catch (Exception ex)
                    {
                        list.Add("JUNK - " + ++junkCount);
                        list.Add(Encoding.ASCII.GetString(arr));
                        byte[] temp = new byte[arr.Length + 5];
                        arr.CopyTo(temp, 1);
                        for (int i = 0; i < GHdr.Size - 70; i++)
                        {

                            IAHdr = temp.ToStructure<TIAReplyHeaderRecord>(i);// fs.ReadStruct<TIAReplyHeaderRecord>();
                            if ((IAHdr.ReplyLen > 0) && (IAHdr.ReplyLen == IAHdr.ReplyCnt * IAHdr.ReplyRecSize))
                            {

                                list.Add("Junk Header at Index - " + i + "-- ReplyType -" + IAHdr.ReplyType);
                                list.Add("-------------------------");
                                list.Add("AccountType-" + IAHdr.AccountType);
                                list.Add("ChannelID_BC-" + IAHdr.ChannelID_BC);
                                list.Add("ChannelID_NC-" + IAHdr.ChannelID_NC);
                                list.Add("ChannelID_ND-" + IAHdr.ChannelID_ND);
                                list.Add("ClientType-" + IAHdr.ClientType);
                                list.Add("ErrorCode-" + IAHdr.ErrorCode);
                                list.Add("FELength-" + IAHdr.FEUserIDLength);
                                list.Add("FEUserID-" + new string(IAHdr.FEUserID).Substring(0, IAHdr.FEUserIDLength));
                                list.Add("ReplyCnt-" + IAHdr.ReplyCnt);
                                list.Add("ReplyLen-" + IAHdr.ReplyLen);
                                list.Add("ReplyRecSize-" + IAHdr.ReplyRecSize);
                                list.Add("ReplyType-" + IAHdr.ReplyType);
                                list.Add("SocketNum-" + IAHdr.SocketNum);
                                list.Add("TraderID-" + new string(IAHdr.TraderID).Substring(0, IAHdr.TraderIDLength));
                                list.Add("TraderLength-" + IAHdr.TraderIDLength);

                                listCSV.Add($"{GHdr.DateTime.ToString("hh:mm:ss:fff")},{GHdr.SentFrom},{GHdr.Size},{IAHdr.ReplyType},{IAHdr.AccountType},{IAHdr.ChannelID_BC},{IAHdr.ChannelID_NC},{IAHdr.ChannelID_ND}" +
                                    $",{IAHdr.ClientType},{IAHdr.ErrorCode},{IAHdr.FEUserIDLength},{new string(IAHdr.FEUserID).Substring(0, IAHdr.FEUserIDLength)},{IAHdr.ReplyCnt}" +
                                    $",{IAHdr.ReplyLen},{IAHdr.ReplyRecSize},{IAHdr.ReplyType},{IAHdr.SocketNum},{new string(IAHdr.TraderID).Substring(0, IAHdr.TraderIDLength)},{IAHdr.TraderIDLength}");
                            }
                        }
                    }

                }
                else if (GHdr.SentFrom == 1)
                {
                    IAError = arr.ToStructure<TIAErrorMessageRecord>();
                    list.Add("");
                    list.Add("Error");
                    list.Add("-------------------------");
                    list.Add("Errorlength-" + IAError.ErrorStrLength);
                    list.Add("ErrorStr-" + IAError.ErrorStr.Trim());
                }
            }

            if (BodyCount > 0)
            {
                list.Add("");
                list.Add($"--------Time- {BodyHdr.DateTime:hh:mm:ss:fff}, Flow-{BodyHdr.SentFrom}, Size-{BodyHdr.Size}, Count-{BodyCount}, TotalSize-{BodyLen}---------");
            }
            fs.Close();
            File.WriteAllLines(/*ReportsFilePathName + */Fname + ".txt", list);
            File.WriteAllLines(/*ReportsFilePathName + */Fname + ".csv", listCSV);
            MessageBox.Show("Done.");

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private ITInteractiveStructures.TAllReplyNewRecord ReadBodyWithExchConnBodyNew(byte ReplyType, byte[] buffToReadBody, ITInteractiveStructures.TIAReplyHeaderRecord Hdr, out bool isOrderType, out byte[] BytesToSend, out ITInteractiveStructures.TIAReplyHeaderRecord NewHeader)
        {
                int OrderPositionInList, OrderPositionInListForDealer;
                ITInteractiveStructures.TAllReplyNewRecord AllReply = new ITInteractiveStructures.TAllReplyNewRecord();
                NewHeader = new ITInteractiveStructures.TIAReplyHeaderRecord();
                isOrderType = false;
                bool IsCheckDealerType = false; // 15.10.2016 HBetal
                BytesToSend = null;
                switch (ReplyType)
                {
                    case 11:
                        {
                            #region Equity Exch Place Reply
                            AllReply.EquityOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.EquityOrderExchReplyRec.Exch, AllReply.EquityOrderExchReplyRec.ExchType, AllReply.EquityOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_PLACE_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_PLACE_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.EquityOrderExchReplyRec.Exch, AllReply.EquityOrderExchReplyRec.ExchType, AllReply.EquityOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_PLACE_REPLY, out OrderPositionInListForDealer); // 15.10.2016 Hetal
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }

                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.EquityOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.EquityOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.EquityOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = AllReply.EquityOrderExchReplyRec.ScripName;
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = AllReply.EquityOrderExchReplyRec.ScripNameLength;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.EquityOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.EquityOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.EquityOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.EquityOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.EquityOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.EquityOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.EquityOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = AllReply.EquityOrderExchReplyRec.IOC;
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.EquityOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.EquityOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.EquityOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.EquityOrderExchReplyRec.ExchangeOrderID;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.EquityOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.EquityOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.EquityOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.EquityOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.EquityOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.EquityOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                            if (OrderTypeList[OrderPositionInList].AllReply[0].OrderTypePlaceBrokerReplyRec.BrokerOrderID > 0)    // sumitp 28_jan_22
                            {
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypePlaceBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypePlaceBrokerReplyRec.ProductType;//CHECK HERE                            
                            }
                            else         // sumitp 28_jan_22
                            {
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderPlaceBrokerReplyRecordCoverAndBracket.ProductType;//CHECK HERE 
                            }

                            BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 21:
                        {
                            #region Equity Exch Modify Reply
                            AllReply.EquityOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.EquityOrderExchReplyRec.Exch, AllReply.EquityOrderExchReplyRec.ExchType, AllReply.EquityOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MODIFY_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_MODIFY_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                //15.10.2016 Hetal
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.EquityOrderExchReplyRec.Exch, AllReply.EquityOrderExchReplyRec.ExchType, AllReply.EquityOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MODIFY_REPLY, out OrderPositionInListForDealer);
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }

                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.EquityOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.EquityOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.EquityOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = AllReply.EquityOrderExchReplyRec.ScripName;
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = AllReply.EquityOrderExchReplyRec.ScripNameLength;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.EquityOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.EquityOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.EquityOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.EquityOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.EquityOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.EquityOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.EquityOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = AllReply.EquityOrderExchReplyRec.IOC;
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.EquityOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.EquityOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.EquityOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.EquityOrderExchReplyRec.ExchangeOrderID;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.EquityOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.EquityOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.EquityOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.EquityOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.EquityOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.EquityOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeModifyBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeModifyBrokerReplyRec.ProductType;//CHECK HERE                            
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 31:
                        {
                            #region Equity Exch Cancel Reply
                            AllReply.EquityOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TEquityOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.EquityOrderExchReplyRec.Exch, AllReply.EquityOrderExchReplyRec.ExchType, AllReply.EquityOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CANCEL_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_CANCEL_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                //15.10.16 Hetal
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.EquityOrderExchReplyRec.Exch, AllReply.EquityOrderExchReplyRec.ExchType, AllReply.EquityOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CANCEL_REPLY, out OrderPositionInListForDealer);
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }

                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.EquityOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.EquityOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.EquityOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = AllReply.EquityOrderExchReplyRec.ScripName;
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = AllReply.EquityOrderExchReplyRec.ScripNameLength;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.EquityOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.EquityOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.EquityOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.EquityOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.EquityOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.EquityOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.EquityOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = AllReply.EquityOrderExchReplyRec.IOC;
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.EquityOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.EquityOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.EquityOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.EquityOrderExchReplyRec.ExchangeOrderID;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.EquityOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.EquityOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.EquityOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.EquityOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.EquityOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.EquityOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeCancelBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeCancelBrokerReplyRec.ProductType;//CHECK HERE                            
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 111:
                        {
                            #region NSE FnO Exch Place Reply
                            AllReply.DerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_PLACE_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_PLACE_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                //15.10.16 Hetal
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_PLACE_REPLY, out OrderPositionInListForDealer);
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }
                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.DerivOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.DerivOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.DerivOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = new string(AllReply.DerivOrderExchReplyRec.DerivInfo.Symbol).PadRight(12).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.DerivOrderExchReplyRec.DerivInfo.SymbolLength;   // 02.08.16 Hetal Wrong Length is taken for ScripName
                                AllReply.OrderTypeExchReplyRecord.CALevel = AllReply.DerivOrderExchReplyRec.DerivInfo.CALevel;
                                AllReply.OrderTypeExchReplyRecord.CPType = AllReply.DerivOrderExchReplyRec.DerivInfo.CPType;
                                AllReply.OrderTypeExchReplyRecord.InstrType = AllReply.DerivOrderExchReplyRec.DerivInfo.InstrType;
                                AllReply.OrderTypeExchReplyRecord.ExpiryDate = AllReply.DerivOrderExchReplyRec.DerivInfo.ExpiryDate;
                                AllReply.OrderTypeExchReplyRecord.StrikePrice = AllReply.DerivOrderExchReplyRec.DerivInfo.StrikePrice;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.DerivOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.DerivOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.DerivOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.DerivOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.DerivOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.DerivOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.DerivOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = 0;  //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.DerivOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0; //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.DerivOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.DerivOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.DerivOrderExchReplyRec.ExchangeOrderId;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.DerivOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.DerivOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.DerivOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.DerivOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.DerivOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.DerivOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypePlaceBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypePlaceBrokerReplyRec.ProductType;//CHECK HERE                            
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 121:
                        {
                            #region NSE FnO Exch Modify Reply
                            AllReply.DerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MODIFY_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_MODIFY_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                //15.10.16 Hetal
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MODIFY_REPLY, out OrderPositionInListForDealer);
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }

                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.DerivOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.DerivOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.DerivOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = new string(AllReply.DerivOrderExchReplyRec.DerivInfo.Symbol).PadRight(12).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.DerivOrderExchReplyRec.DerivInfo.SymbolLength;   // 02.08.16 Hetal Wrong Length is taken for ScripName
                                AllReply.OrderTypeExchReplyRecord.CALevel = AllReply.DerivOrderExchReplyRec.DerivInfo.CALevel;
                                AllReply.OrderTypeExchReplyRecord.CPType = AllReply.DerivOrderExchReplyRec.DerivInfo.CPType;
                                AllReply.OrderTypeExchReplyRecord.InstrType = AllReply.DerivOrderExchReplyRec.DerivInfo.InstrType;
                                AllReply.OrderTypeExchReplyRecord.ExpiryDate = AllReply.DerivOrderExchReplyRec.DerivInfo.ExpiryDate;
                                AllReply.OrderTypeExchReplyRecord.StrikePrice = AllReply.DerivOrderExchReplyRec.DerivInfo.StrikePrice;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.DerivOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.DerivOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.DerivOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.DerivOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.DerivOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.DerivOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.DerivOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = 0;  //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.DerivOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0; //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.DerivOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.DerivOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.DerivOrderExchReplyRec.ExchangeOrderId;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.DerivOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.DerivOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.DerivOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.DerivOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.DerivOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.DerivOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeModifyBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeModifyBrokerReplyRec.ProductType;//CHECK HERE                            
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 131:
                        {
                            #region NSE FnO Exch Cancel Reply
                            AllReply.DerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CANCEL_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_CANCEL_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                //15.10.16 Hetal
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CANCEL_REPLY, out OrderPositionInListForDealer);
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }

                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.DerivOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.DerivOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.DerivOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = new string(AllReply.DerivOrderExchReplyRec.DerivInfo.Symbol).PadRight(12).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.DerivOrderExchReplyRec.DerivInfo.SymbolLength;   // 02.08.16 Hetal Wrong Length is taken for ScripName
                                AllReply.OrderTypeExchReplyRecord.CALevel = AllReply.DerivOrderExchReplyRec.DerivInfo.CALevel;
                                AllReply.OrderTypeExchReplyRecord.CPType = AllReply.DerivOrderExchReplyRec.DerivInfo.CPType;
                                AllReply.OrderTypeExchReplyRecord.InstrType = AllReply.DerivOrderExchReplyRec.DerivInfo.InstrType;
                                AllReply.OrderTypeExchReplyRecord.ExpiryDate = AllReply.DerivOrderExchReplyRec.DerivInfo.ExpiryDate;
                                AllReply.OrderTypeExchReplyRecord.StrikePrice = AllReply.DerivOrderExchReplyRec.DerivInfo.StrikePrice;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.DerivOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.DerivOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.DerivOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.DerivOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.DerivOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.DerivOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.DerivOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = 0;  //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.DerivOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0; //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.DerivOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.DerivOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.DerivOrderExchReplyRec.ExchangeOrderId;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.DerivOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.DerivOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.DerivOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.DerivOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.DerivOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.DerivOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeCancelBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeCancelBrokerReplyRec.ProductType;//CHECK HERE                            
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 212:
                        {
                            #region NSE Currency Exch Place Reply
                            AllReply.CurrDerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_PLACE_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_PLACE_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                //15.10.16 Hetal
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_PLACE_REPLY, out OrderPositionInListForDealer);
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }

                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.CurrDerivOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.CurrDerivOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.CurrDerivOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = new string(AllReply.CurrDerivOrderExchReplyRec.DerivInfo.Symbol).PadRight(12).ToCharArray();//CHECK HERE
                                //AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.DerivOrderExchReplyRec.DerivInfo.Symbol.Length;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.CurrDerivOrderExchReplyRec.DerivInfo.SymbolLength; // 02.08.16 Hetal Wrong Length is taken for ScripName
                                AllReply.OrderTypeExchReplyRecord.CALevel = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.CALevel;
                                AllReply.OrderTypeExchReplyRecord.CPType = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.CPType;
                                AllReply.OrderTypeExchReplyRecord.InstrType = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.InstrType;
                                AllReply.OrderTypeExchReplyRecord.ExpiryDate = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.ExpiryDate;
                                AllReply.OrderTypeExchReplyRecord.StrikePrice = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.StrikePrice;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.CurrDerivOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.CurrDerivOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.CurrDerivOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.CurrDerivOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.CurrDerivOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.CurrDerivOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.CurrDerivOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = 0;  //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.CurrDerivOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0; //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.CurrDerivOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.CurrDerivOrderExchReplyRec.ExchangeOrderId;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.CurrDerivOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.CurrDerivOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.CurrDerivOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.CurrDerivOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.CurrDerivOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.CurrDerivOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypePlaceBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypePlaceBrokerReplyRec.ProductType;//CHECK HERE                            
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 222:
                        {
                            #region NSE Currency Exch Modify Reply
                            AllReply.CurrDerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MODIFY_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_MODIFY_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                //15.10.16 Hetal
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MODIFY_REPLY, out OrderPositionInListForDealer);
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }

                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.CurrDerivOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.CurrDerivOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.CurrDerivOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = new string(AllReply.CurrDerivOrderExchReplyRec.DerivInfo.Symbol).PadRight(12).ToCharArray();//CHECK HERE
                                //AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.DerivOrderExchReplyRec.DerivInfo.Symbol.Length;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.CurrDerivOrderExchReplyRec.DerivInfo.SymbolLength; // 02.08.16 Hetal Wrong Length is taken for ScripName
                                AllReply.OrderTypeExchReplyRecord.CALevel = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.CALevel;
                                AllReply.OrderTypeExchReplyRecord.CPType = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.CPType;
                                AllReply.OrderTypeExchReplyRecord.InstrType = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.InstrType;
                                AllReply.OrderTypeExchReplyRecord.ExpiryDate = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.ExpiryDate;
                                AllReply.OrderTypeExchReplyRecord.StrikePrice = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.StrikePrice;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.CurrDerivOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.CurrDerivOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.CurrDerivOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.CurrDerivOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.CurrDerivOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.CurrDerivOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.CurrDerivOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = 0;  //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.CurrDerivOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0; //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.CurrDerivOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.CurrDerivOrderExchReplyRec.ExchangeOrderId;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.CurrDerivOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.CurrDerivOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.CurrDerivOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.CurrDerivOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.CurrDerivOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.CurrDerivOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeModifyBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeModifyBrokerReplyRec.ProductType;//CHECK HERE                            
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 232:
                        {
                            #region NSE Currency Exch Cancel Reply
                            AllReply.CurrDerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CANCEL_REPLY, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_CANCEL_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TOrderExchReplyRecord());
                                //15.10.16 Hetal
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CANCEL_REPLY, out OrderPositionInListForDealer);
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }

                                AllReply.OrderTypeExchReplyRecord.Exch = AllReply.CurrDerivOrderExchReplyRec.Exch;
                                AllReply.OrderTypeExchReplyRecord.ExchType = AllReply.CurrDerivOrderExchReplyRec.ExchType;
                                AllReply.OrderTypeExchReplyRecord.ScripCode = AllReply.CurrDerivOrderExchReplyRec.ScripCode;
                                AllReply.OrderTypeExchReplyRecord.ScripName = new string(AllReply.CurrDerivOrderExchReplyRec.DerivInfo.Symbol).PadRight(12).ToCharArray();//CHECK HERE
                                //AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.DerivOrderExchReplyRec.DerivInfo.Symbol.Length;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ScripNameLength = (byte)AllReply.CurrDerivOrderExchReplyRec.DerivInfo.SymbolLength; // 02.08.16 Hetal Wrong Length is taken for ScripName
                                AllReply.OrderTypeExchReplyRecord.CALevel = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.CALevel;
                                AllReply.OrderTypeExchReplyRecord.CPType = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.CPType;
                                AllReply.OrderTypeExchReplyRecord.InstrType = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.InstrType;
                                AllReply.OrderTypeExchReplyRecord.ExpiryDate = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.ExpiryDate;
                                AllReply.OrderTypeExchReplyRecord.StrikePrice = AllReply.CurrDerivOrderExchReplyRec.DerivInfo.StrikePrice;
                                AllReply.OrderTypeExchReplyRecord.BuySell = AllReply.CurrDerivOrderExchReplyRec.BuySell;
                                AllReply.OrderTypeExchReplyRecord.Qty = AllReply.CurrDerivOrderExchReplyRec.Qty;
                                AllReply.OrderTypeExchReplyRecord.AtMarket = AllReply.CurrDerivOrderExchReplyRec.AtMarket;
                                AllReply.OrderTypeExchReplyRecord.Rate = AllReply.CurrDerivOrderExchReplyRec.Rate;
                                AllReply.OrderTypeExchReplyRecord.WithSL = AllReply.CurrDerivOrderExchReplyRec.WithSL;
                                AllReply.OrderTypeExchReplyRecord.TriggerRate = AllReply.CurrDerivOrderExchReplyRec.TriggerRate;
                                AllReply.OrderTypeExchReplyRecord.TradedQty = AllReply.CurrDerivOrderExchReplyRec.TradedQty;
                                AllReply.OrderTypeExchReplyRecord.IOC = 0;  //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.PendingQty = AllReply.CurrDerivOrderExchReplyRec.PendingQty;
                                AllReply.OrderTypeExchReplyRecord.DisclosedQty = 0; //CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderID = AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID;
                                AllReply.OrderTypeExchReplyRecord.BrokerOrderTime = AllReply.CurrDerivOrderExchReplyRec.BrokerOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderID = AllReply.CurrDerivOrderExchReplyRec.ExchangeOrderId;
                                AllReply.OrderTypeExchReplyRecord.ExchangeOrderTime = AllReply.CurrDerivOrderExchReplyRec.ExchangeOrderTime;
                                AllReply.OrderTypeExchReplyRecord.ExchReplyCode = AllReply.CurrDerivOrderExchReplyRec.ExchReplyCode;
                                AllReply.OrderTypeExchReplyRecord.ExchReasonCode = AllReply.CurrDerivOrderExchReplyRec.ExchReasonCode;
                                AllReply.OrderTypeExchReplyRecord.ExchErrorCode = AllReply.CurrDerivOrderExchReplyRec.ExchErrorCode;
                                AllReply.OrderTypeExchReplyRecord.StatusLength = AllReply.CurrDerivOrderExchReplyRec.StatusLength;
                                AllReply.OrderTypeExchReplyRecord.Status = AllReply.CurrDerivOrderExchReplyRec.Status.PadRight(20).ToCharArray();//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.OrderType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeCancelBrokerReplyRec.OrderType;//CHECK HERE
                                AllReply.OrderTypeExchReplyRecord.ProductType = OrderTypeList[OrderPositionInList].AllReply[0].OrderTypeCancelBrokerReplyRec.ProductType;//CHECK HERE                            
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.OrderTypeExchReplyRecord);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 211:
                        {
                            AllReply.CurrDerivOrderPlaceBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderPlaceBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 221:
                        {
                            AllReply.CurrDerivOrderModifyBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderModifyBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    case 231:
                        {
                            AllReply.CurrDerivOrderCancelBrokerReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderCancelBrokerReplyRecord>(buffToReadBody);
                        }
                        break;
                    //Option Exercise
                    case Constants.OTS_EXERCISE_PLACE_REPLY:
                    case Constants.OTS_EXERCISE_MODIFY_REPLY:
                    case Constants.OTS_EXERCISE_CANCEL_REPLY:
                        {
                            AllReply.OptionExerciseExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOptionExerciseExchReplyRecord>(buffToReadBody);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_EXCH_REPLY:
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_EXCH_REPLY:

                    {
                            AllReply.MFSIPRegistrationExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFSIPRegistrationExchangeReplyRecord>(buffToReadBody);
                        }break;
                    case Constants.OTS_MANDATE_EXCH_REPLY:
                        {
                            AllReply.XSIPMandateExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.XSIPMandateExchangeReply>(buffToReadBody);
                        }break;
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_EXCH_REPLY:
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_EXCH_REPLY:
                    case Constants.OTS_ORDERTYPE_MF_PLACE_EXCH_REPLY://BseMf krutika
                        {
                            AllReply.MFAddModifyCancelExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply>(buffToReadBody);
                        }
                        break;
                    case 241:
                    case 242:
                    case 243:
                    case 249:
                        {
                            //AllReply.SpdOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSpreadOrderExchReplyRecord>(buffToReadBody);
                            AllReply.SpdOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSpreadOrderExchReplyRecord>(buffToReadBody);
                            isOrderType = CheckForOrderTypeReplyfrmEC(AllReply.SpdOrderExchReplyRec.Exch, AllReply.SpdOrderExchReplyRec.ExchType, AllReply.SpdOrderExchReplyRec.BrokerOrderID, ReplyType, out OrderPositionInList);
                            if (isOrderType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = ReplyType;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderExchReplyRecord());
                                IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.SpdOrderExchReplyRec.Exch, AllReply.SpdOrderExchReplyRec.ExchType, AllReply.SpdOrderExchReplyRec.BrokerOrderID, ReplyType, out OrderPositionInListForDealer); // 15.10.2016 Hetal
                                if (IsCheckDealerType)
                                {
                                    NewHeader.FEUserID = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserID;
                                    NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInListForDealer].ReplyHdr.FEUserIDLength;
                                    DealerTypeList.RemoveAt(OrderPositionInListForDealer);
                                }
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.SpdOrderExchReplyRec);
                                OrderTypeList.RemoveAt(OrderPositionInList);
                            }
                        }
                        break;
                        case 126:
                        {
                            AllReply.OfsPlaceOrderReply = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS>(buffToReadBody);
                        }break;
                        case 127:
                        {
                            AllReply.OfsPlaceOrderReply = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS>(buffToReadBody);
                        }
                        break;
                        case 128:
                        {
                            AllReply.OfsPlaceOrderReply = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS>(buffToReadBody);
                        }
                        break;
            
                    case 238:
                    case 239:
                    case 240:
                        {
                            #region NSE SLBM Exch Place/Modify/Cancel Reply
                            AllReply.SLBMOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSLBMOrderExchReplyRecord>(buffToReadBody);
                            #endregion
                        }
                        break;
                }
                return AllReply;
          
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                logEnable2FA = true;
            }
            else
            {
                logEnable2FA = true;
            }
            //logEnable2FA = checkBox2.Checked;
        }


        private ITInteractiveStructures.TAllReplyNewRecord ReadBodyWithExchConnBodyNewWithDealerCheck(byte ReplyType, byte[] buffToReadBody, ITInteractiveStructures.TIAReplyHeaderRecord Hdr, out bool IsCheckDealerType, out byte[] BytesToSend, out ITInteractiveStructures.TIAReplyHeaderRecord NewHeader) //15.10.2016 Hetal
        {
                int OrderPositionInList;
                ITInteractiveStructures.TAllReplyNewRecord AllReply = new ITInteractiveStructures.TAllReplyNewRecord();
                NewHeader = new ITInteractiveStructures.TIAReplyHeaderRecord();
                IsCheckDealerType = false; // 29.07.16
                BytesToSend = null;
                switch (ReplyType)
                {
                    case 111:
                        {
                            #region Deriv Exch Place Reply
                            AllReply.DerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderExchReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_DERIV_PLACE_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_DERIV_PLACE_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderExchReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.DerivOrderExchReplyRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 121:
                        {
                            #region Deriv Exch Modify Reply
                            AllReply.DerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderExchReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_DERIV_MODIFY_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_DERIV_MODIFY_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderExchReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.DerivOrderExchReplyRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 131:
                        {
                            #region Deriv Exch Cancel Reply
                            AllReply.DerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderExchReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.DerivOrderExchReplyRec.Exch, AllReply.DerivOrderExchReplyRec.ExchType, AllReply.DerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_DERIV_CANCEL_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_DERIV_CANCEL_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TDerivOrderExchReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.DerivOrderExchReplyRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case 212:
                        {
                            #region NSE Currency Place Reply
                            AllReply.CurrDerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderExchReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CURR_PLACE_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_CURR_PLACE_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderExchReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.CurrDerivOrderExchReplyRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion

                        }
                        break;
                    case 222:
                        {
                            #region NSE Currency Modify Reply
                            AllReply.CurrDerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderExchReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CURR_MODIFY_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_CURR_MODIFY_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderExchReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.CurrDerivOrderExchReplyRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion

                        }
                        break;
                    case 232:
                        {
                            #region NSE Currency Cancel Reply
                            AllReply.CurrDerivOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TCurrDerivOrderExchReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.CurrDerivOrderExchReplyRec.Exch, AllReply.CurrDerivOrderExchReplyRec.ExchType, AllReply.CurrDerivOrderExchReplyRec.BrokerOrderID, Constants.OTS_ORDERTYPE_CURR_CANCEL_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_CURR_CANCEL_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TCurrDerivOrderExchReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.CurrDerivOrderExchReplyRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                            #endregion
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_EXCH_REPLY:
                        {
                            AllReply.MFSIPRegistrationExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFSIPRegistrationExchangeReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.MFSIPRegistrationExchangeResRec.Exch, AllReply.MFSIPRegistrationExchangeResRec.ExchType, AllReply.MFSIPRegistrationExchangeResRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_EXCH_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_EXCH_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationExchangeReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationExchangeReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.MFSIPRegistrationExchangeResRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                        } break;
                    case Constants.OTS_MANDATE_EXCH_REPLY:
                        {
                            AllReply.XSIPMandateExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.XSIPMandateExchangeReply>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.XSIPMandateExchangeResRec.Exch, AllReply.XSIPMandateExchangeResRec.ExchType, AllReply.XSIPMandateExchangeResRec.BrokerOrderID, Constants.OTS_MANDATE_EXCH_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_MANDATE_EXCH_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.XSIPMandateExchangeReply());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.XSIPMandateExchangeReply());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.XSIPMandateExchangeResRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                        } break;

                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_EXCH_REPLY:
                        {
                            AllReply.MFSIPRegistrationExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFSIPRegistrationExchangeReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.MFSIPRegistrationExchangeResRec.Exch, AllReply.MFSIPRegistrationExchangeResRec.ExchType, AllReply.MFSIPRegistrationExchangeResRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MF_SIPCANCEL_EXCH_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_MF_SIPCANCEL_EXCH_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationExchangeReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFSIPRegistrationExchangeReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.MFSIPRegistrationExchangeResRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                        }break;
                    case Constants.OTS_ORDERTYPE_MF_PLACE_EXCH_REPLY:
                        {
                            AllReply.MFAddModifyCancelExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.MFAddModifyCancelExchangeResRec.Exch, AllReply.MFAddModifyCancelExchangeResRec.ExchType, AllReply.MFAddModifyCancelExchangeResRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MF_PLACE_EXCH_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_MF_PLACE_EXCH_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.MFAddModifyCancelExchangeResRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                        } break;
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_EXCH_REPLY:
                        {
                            AllReply.MFAddModifyCancelExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.MFAddModifyCancelExchangeResRec.Exch, AllReply.MFAddModifyCancelExchangeResRec.ExchType, AllReply.MFAddModifyCancelExchangeResRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MF_MODIFY_EXCH_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_MF_MODIFY_EXCH_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.MFAddModifyCancelExchangeResRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                        } break;
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_EXCH_REPLY:
                        {
                            AllReply.MFAddModifyCancelExchangeResRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.MFAddModifyCancelExchangeResRec.Exch, AllReply.MFAddModifyCancelExchangeResRec.ExchType, AllReply.MFAddModifyCancelExchangeResRec.BrokerOrderID, Constants.OTS_ORDERTYPE_MF_CANCEL_EXCH_REPLY, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = Constants.OTS_ORDERTYPE_MF_CANCEL_EXCH_REPLY;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TBseMFAddModifyCancelExchangeOrderReply());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.MFAddModifyCancelExchangeResRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }
                        } break;
                    case 241:
                    case 242:
                    case 243:
                    case 249:
                        {

                            AllReply.SpdOrderExchReplyRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TSpreadOrderExchReplyRecord>(buffToReadBody);
                            IsCheckDealerType = CheckForDealerOrderfrmEC(AllReply.SpdOrderExchReplyRec.Exch, AllReply.SpdOrderExchReplyRec.ExchType, AllReply.SpdOrderExchReplyRec.BrokerOrderID, ReplyType, out OrderPositionInList);
                            if (IsCheckDealerType)
                            {
                                NewHeader = Hdr;
                                NewHeader.ReplyType = ReplyType;
                                NewHeader.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderExchReplyRecord());
                                NewHeader.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TSpreadOrderExchReplyRecord());
                                NewHeader.FEUserID = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserID;
                                NewHeader.FEUserIDLength = DealerTypeList[OrderPositionInList].ReplyHdr.FEUserIDLength;
                                BytesToSend = CommonFunctions.RawSerialize(AllReply.SpdOrderExchReplyRec);
                                DealerTypeList.RemoveAt(OrderPositionInList);
                            }

                        }
                        break;
                    case 126:
                        {
                            AllReply.OfsPlaceOrderReply = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS>(buffToReadBody);
                        }
                        break;
                    case 127:
                        {
                            AllReply.OfsPlaceOrderReply = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS>(buffToReadBody);
                        }
                        break;
                    case 128:
                        {
                            AllReply.OfsPlaceOrderReply = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TOrderPlaceModifyCancelReplyRecordOFS>(buffToReadBody);
                        }
                        break;
                default:
                        break;
                }
                return AllReply;
           
        }
        
        private void buttonClear_Click_1(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void buttonSendMFMsg_Click(object sender, EventArgs e)
        {
            SendRequestForMFClientHoldings();
        }

        private void SendRequestForMFClientHoldings()
        {
            try
            {
                string str1;
                ITInteractiveStructures.TOTSRequestRecord OTSReqRec = new ITInteractiveStructures.TOTSRequestRecord();
                if (CommonFunctions.GetSelectedRadioButtonText(groupBoxDBNum) == 1)
                {
                    str1 = "db_one";
                }
                else
                    str1 = "db_two";
                listBox4.Items.Add("Sending Holding Request to " + str1);
                OTSReqRec.IPLength = 7;
                OTSReqRec.IP = "1.1.1.1".PadRight(15).ToCharArray();
                OTSReqRec.ReqTime = DateTime.Now;
                OTSReqRec.ReqHdr.SocketNum = 0;
                OTSReqRec.ReqHdr.FEUserIDLength = 6;
                OTSReqRec.ReqHdr.FEUserID = str1.PadRight(15).ToCharArray();
                OTSReqRec.ReqHdr.TraderIDLength = 6;
                OTSReqRec.ReqHdr.TraderID = str1.PadRight(14).ToCharArray();
                OTSReqRec.ReqHdr.ClientType = 0;
                OTSReqRec.ReqHdr.RequestType = 87;
                OTSReqRec.ReqHdr.RequestLen = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TMaalFodoClientHoldingReportRequestRecord());
                OTSReqRec.AllReq.MFClientHoldingReportRequestRec.ClientType = CommonFunctions.GetSelectedRadioButtonText(groupBoxMFCliType);
                OTSReqRec.AllReq.MFClientHoldingReportRequestRec.MinimumDebit = Convert.ToInt32(textBoxMinDebit.Text);
                AddToPendingRequestList(OTSReqRec);
                DBRequestCnt++;
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        public void ipwMCast1DataIn(byte[] dataToRead)
        {
            string Data = null;
            try
            {
                ITInteractiveStructures.TIAReplyHeaderRecord IAReplyHdr = new ITInteractiveStructures.TIAReplyHeaderRecord();
                ITInteractiveStructures.TLogoutUserRecord LogoutUserRec = new ITInteractiveStructures.TLogoutUserRecord();
                ITInteractiveStructures.TAHOrderRejectedRecord AHOrderRejectedRec = new ITInteractiveStructures.TAHOrderRejectedRecord();
                TWSocketClient CurrCli;
                string[] SL;
                string StuctureType;

                Data = Encoding.ASCII.GetString(dataToRead, 0, dataToRead.Length);
                if (checkBoxWriteToDisplayMCast.Checked)
                    listBox1.Items.Insert(0, "MCast " + Data);
                SL = Data.Split('|');
                if (SL.Count() != 5 && SL.Count() != 4)
                {
                    SL = null;
                    Array.Clear(dataToRead, 0, dataToRead.Length);
                    McastOTS.BeginReceive();
                    return;
                }
                StuctureType = SL[0].TrimEnd(TrimChars);
                if (StuctureType != "LOGIN" && StuctureType != "AHReject")
                {
                    SL = null;
                    Array.Clear(dataToRead, 0, dataToRead.Length);
                    McastOTS.BeginReceive();
                    return;
                }
                if (StuctureType == "LOGIN")
                {
                    IAReplyHdr.SocketNum = Convert.ToUInt16(SL[3]);
                    IAReplyHdr.FEUserIDLength = 0;
                    IAReplyHdr.FEUserID = "".PadRight(15).ToCharArray();

                    IAReplyHdr.TraderIDLength = Convert.ToByte(SL[1].TrimEnd(TrimChars).Length);
                    IAReplyHdr.TraderID = SL[1].PadRight(14).ToCharArray();
                    IAReplyHdr.ClientType = 0;
                    IAReplyHdr.ReplyType = 9;
                    IAReplyHdr.ReplyLen = (int)Marshal.SizeOf(new ITInteractiveStructures.TLogoutUserRecord());
                    IAReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(new ITInteractiveStructures.TLogoutUserRecord());
                    IAReplyHdr.ReplyCnt = 1;
                    IAReplyHdr.ErrorCode = 0;

                    LogoutUserRec.FEUserIDLength = Convert.ToByte(SL[1].TrimEnd(TrimChars).Length);
                    LogoutUserRec.FEUserID = SL[1].PadRight(14).ToCharArray();
                    LogoutUserRec.IPLength = Convert.ToByte(SL[2].TrimEnd(TrimChars).Length);
                    LogoutUserRec.IP = SL[2].PadRight(15).ToCharArray();
                    LogoutUserRec.ITS_Num = Convert.ToByte(SL[4].TrimEnd(TrimChars));
                }
                else
                {
                    IAReplyHdr.SocketNum = 0;
                    IAReplyHdr.FEUserIDLength = 0;
                    IAReplyHdr.FEUserID = "".PadRight(15).ToCharArray();

                    IAReplyHdr.TraderIDLength = Convert.ToByte(SL[1].TrimEnd(TrimChars).Length);
                    IAReplyHdr.TraderID = SL[1].PadRight(14).ToCharArray();
                    IAReplyHdr.ClientType = 0;
                    IAReplyHdr.ReplyType = 16;
                    IAReplyHdr.ReplyLen = (int)Marshal.SizeOf(AHOrderRejectedRec);
                    IAReplyHdr.ReplyRecSize = (ushort)Marshal.SizeOf(AHOrderRejectedRec);
                    IAReplyHdr.ReplyCnt = 1;
                    IAReplyHdr.ErrorCode = 0;

                    AHOrderRejectedRec.ErrorMsgLength = Convert.ToByte(SL[3].TrimEnd(TrimChars).Length);
                    AHOrderRejectedRec.ErrorMsg = SL[3];
                    AHOrderRejectedRec.BrokerOrderID = Convert.ToInt32(SL[2].TrimEnd(TrimChars));
                }

                SL = null;
                for (int i = 0; i < ListenToITS.FClientList.Count; i++)
                {
                    CurrCli = (TWSocketClient)ListenToITS.FClientList[i];
                    if (checkBoxWriteToDisplayITS.Checked)
                        listBox1.Items.Insert(0, "To ITS - sent " + Marshal.SizeOf(new ITInteractiveStructures.TIAReplyHeaderRecord()) + " bytes as header");

                    CurrCli.PutDataInSendBuffer(CommonFunctions.RawSerialize(IAReplyHdr), Marshal.SizeOf(IAReplyHdr));
                    if (checkBoxWriteToDisplayITS.Checked)
                    {
                        if (StuctureType == "LOGIN")
                            listBox1.Items.Insert(0, "To ITS - sent " + Marshal.SizeOf(new ITInteractiveStructures.TLogoutUserRecord()) + " bytes as body");
                        else
                            listBox1.Items.Insert(0, "To ITS - sent " + Marshal.SizeOf(new ITInteractiveStructures.TAHOrderRejectedRecord()) + " bytes as body");
                    }
                    if (StuctureType == "LOGIN")
                        CurrCli.Send(CommonFunctions.RawSerialize(LogoutUserRec), Marshal.SizeOf(LogoutUserRec));
                    else
                        CurrCli.Send(CommonFunctions.RawSerialize(AHOrderRejectedRec), Marshal.SizeOf(AHOrderRejectedRec));
                }
                Array.Clear(dataToRead, 0, dataToRead.Length);
                McastOTS.BeginReceive();
            }
            catch (Exception er)
            {
                //ErrorLog(er);
            }
        }

        private void WriteExToFile2(ITInteractiveStructures.TOTSRequestRecord OTSReqRec, string RqStr, string ExStr, int ExType)
        {
            string FName;
            ITInteractiveStructures.TNewRequestRecord NewReqRec = new ITInteractiveStructures.TNewRequestRecord();
            ITInteractiveStructures.Dates Dates1 = new ITInteractiveStructures.Dates();
            ITInteractiveStructures.TOTSRequestRecord1 OTSReqRec1 = new ITInteractiveStructures.TOTSRequestRecord1();
            byte[] OTSReqRecArr;
            FName = textBoxOTSID.Text + "_OTS_Ex" + DateTime.Now.ToString("yyyyMMdd") + ".Dat";
            WriteExToTxtFile(RqStr, ExStr, ExType, OTSReqRec.ReqHdr.RequestType, OTSReqRec); //20.10.15
            try
            {
                if (File.Exists(FName))
                {
                    OTSRetryRrdStream = new FileStream(FName, FileMode.Open, FileAccess.ReadWrite);
                    OTSRetryRrdWriter = new BinaryWriter(OTSRetryRrdStream);
                    OTSRetryRrdStream.Seek(0, SeekOrigin.End);
                }
                else
                {
                    OTSRetryRrdStream = new FileStream(FName, FileMode.Create, FileAccess.ReadWrite);
                    OTSRetryRrdWriter = new BinaryWriter(OTSRetryRrdStream);
                }
                OTSReqRec1.IPLength = OTSReqRec.IPLength;
                OTSReqRec1.IP = new string(OTSReqRec.IP).PadRight(15).ToCharArray();
                Dates1.ReqTime = OTSReqRec.ReqTime;
                Dates1.ThreadStartTime = OTSReqRec.ThreadStartTime;
                OTSReqRec1.Dates = CommonFunctions.RawSerialize(Dates1);
                OTSReqRec1.ReqHdr = OTSReqRec.ReqHdr;
                OTSReqRec1.AllReq = OTSReqRec.AllReq;
                OTSReqRec1.RetryCnt = OTSReqRec.RetryCnt;
                OTSReqRec1.ITS_Num = OTSReqRec.ITS_Num;
                ConvertToProperReplyRec(OTSReqRec1);
                OTSReqRecArr = new byte[769];


                NewReqRec.RqStrLength = (byte)RqStr.TrimEnd(TrimChars).Length;
                NewReqRec.RqStr = RqStr;
                NewReqRec.ExStrLength = (byte)ExStr.TrimEnd(TrimChars).Length;
                NewReqRec.ExStr = ExStr;
                NewReqRec.ExType = ExType;
                ConvertedBuff.write(CommonFunctions.RawSerialize(NewReqRec), Marshal.SizeOf(NewReqRec));
                ConvertedBuff.Read(OTSReqRecArr, ConvertedBuff.WrCount);

                OTSRetryRrdStream.Write(OTSReqRecArr, 0, OTSReqRecArr.Length);
                OTSRetryRrdWriter.Close();
                OTSRetryRrdStream.Close();
            }
            catch (Exception er)
            {
                Form1.ErrorLog(er);
            }
        }

        private void ConvertToProperReplyRec(ITInteractiveStructures.TOTSRequestRecord1 OTSReqRec)
        {
            try
            {
                byte[] temp1 = new byte[1024];
                byte[] temp = null;
                temp1[0] = OTSReqRec.IPLength;
                ConvertedBuff.write(temp1, 1);
                ConvertedBuff.write(Encoding.ASCII.GetBytes(new string(OTSReqRec.IP).PadRight(15, '\0')), 15);
                ConvertedBuff.write(OTSReqRec.Dates, 16);
                ConvertedBuff.write(CommonFunctions.RawSerialize(OTSReqRec.ReqHdr), Marshal.SizeOf(OTSReqRec.ReqHdr));
                switch (OTSReqRec.ReqHdr.RequestType)
                {
                    case 1:
                    case 3: //20.10.15
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LoginRequest);
                        }
                        break;
                    case 85: //20.10.15
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LoginRequestTwoFactor);
                        }
                        break;
                    case 4: //20.10.15
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LoginRequestForName);
                        }
                        break;
                    case 2:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ChgPswdRequestRec);
                        }
                        break;
                    case 5:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TraderListRequestRec);
                        }
                        break;
                    case 6:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.VerifyTraderRequestRec);
                        }
                        break;
                    case 7: 
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderReportRequestRec);
                        }
                        break;
                    case 8:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderReportRequestRec);
                        }
                        break;
                    case 10:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.EquityOrderPlaceRequestRec);
                        }
                        break;
                    case 14:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.GetIVRDealerDetailsReq); //24.04.23 For getting IVR Dealer Details		
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_PLACE_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderTypePlaceReqRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_PLACE_REQ_COVER:                    // sumitp 28_jan_22
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PlaceCoverOrderReqRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderTypeModifyReqRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderTypeCancelReqRec);
                        }
                        break;
                    case 20:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.EquityOrderModifyRequestRec);
                        }
                        break;
                    case 30:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.EquityOrderCancelRequestRec);
                        }
                        break;
                    case 41:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ScripMarginRequestRec);
                        }
                        break;
                    // Addition of Raw serialization 2015.03.26
                    case Constants.OTS_SECURITY_INFO:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ScripInfoRequestRec);
                        }
                        break;
                    // Addition of Raw serialization 2015.03.26
                    case Constants.OTS_SCRIP_MARGIN:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ScripMarginRequestRec);
                        }
                        break;
                   
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfSIPRegistrationReqRec);//BseMf
                        }break;
                    case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.XMfSIPRegistrationReqRec);//BseMf
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfAddModifyCancelReqRec);//BseMf
                        }
                        break;
                    //case 61:
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC:
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142:    //Added on 5-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW:    //Added on 18-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER:    //58     // sumitp 28_jan_22
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TradeReportRequestRec);
                        }
                        break;
                    //case 62:
                    case 125:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141: //Added on 5-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW: //Added on 18-Apr-2018 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_COVER_BRACKET: //Order Report for Cover orders   // sumitp 28_jan_22
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_EXERCISE_REPORTS_REC://Options Exercise
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ExerciseReportRequestRec);
                        }
                        break;
                    //case 64:
                    case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC:
                    case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW:
                    case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW:  //Added on 18-Apr-2018 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ActivityLogRequestRec);

                        }
                        break;
                    case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new:  //Added on 18-Apr-2018 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ActivityLogRequestRecord_new);

                        }
                        break;
                    case 66:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LicenceInfoRequestRec);
                        }
                        break;
                    case 68:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.CliDBNumRequestRec);
                        }
                        break;
                    case 69:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.HoldingsReportRequestRec);
                        }
                        break;
                    case 70:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.HoldingsValuationReportRequestRec);
                        }
                        break;
                    case 218:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.HoldingsValuationReportRequestRec);
                        }
                        break;
                    case Constants.OTS_HOLDING_VALUATION_REPORT_NEW:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.HoldingsValuationReportWithMemVarRequestRec);
                        }
                        break;
                    case 73:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DBDateRequestRec);
                        }
                        break;
                    case 79:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ClientPositionRequestRec);
                        }
                        break;
                    case 80:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TradeToTradeRequestRec);
                        }
                        break;
                    case 160:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SurveillanceIndicatorReq);
                        }
                        break;
                    case 161:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SurveillanceIndicatorReq);
                        }
                        break;
                    case 81:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.BseNseMapRequestRec);
                        }
                        break;
                    case 82:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DerivBannedScripRequestRec);
                        }
                        break;
                    case 83:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.AdhocReportRequestRec);
                        }
                        break;
                    case 84:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.AdhocModifyRequestRec);
                        }
                        break;
                    case 86:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MFClientsReportRequestRec);
                        }
                        break;
                    case 87:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MFClientHoldingReportRequestRec);
                        }
                        break;
                    case 89:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.IlliquidRequestRec);
                        }
                        break;
                    case 90:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PreOpenScripRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_ORDER_REPORTS_REC://BseMf Krutika
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfOrderReportRequestRec);
                        } break;
                    case Constants.OTS_Acitivitylog_MF_ORDER_REPORTS_REC:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfActivitylogRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC://BseMf Krutika
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfOrderReportRequestRec);
                        } break;
                    case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC://BseMf Krutika
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MandateOrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_VERIFY_USER:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.VerifyUserRequestRec);
                        }
                        break;
                    case 110:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DerivOrderPlaceRequestRec);
                        }
                        break;
                    case 115:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec);
                        }
                        break;
                    case 120:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DerivOrderModifyRequestRec);
                        }
                        break;
                    case 130:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DerivOrderCancelRequestRec);
                        }
                        break;
                        //02.03.23
                    case 134:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.orderPlaceReqRecForApproval);
                        }
                        break;
                    case 135:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.UpdateOTPOrderStatusRec);
                        }
                        break;
                    case 136:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderInProcessReportRequestRec);
                        }
                        break;
                    case 191:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SpreadOrderPlaceRequestRec);
                        }
                        break;
                    case 192:
                    case 193:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec);
                        }
                        break;
                    case 200:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MarginReportRequestRec);
                        }
                        break;
                    case 206:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MarginReportRequestRecMCX);
                        }
                        break;
                    case 208:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MarginReportRequestReccombined);
                        }
                        break;
                    #region MTM 12-Sep-2016 - Rohan
                    case 203:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.UpdateMTMLimitHitReq);
                        }
                        break;
                    //case 208:   //Added on 08-Mar-2017 - Rohan
                    //    {
                    //        temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.UpdateMTMLimitHitReqMcx);
                    //    }
                    //    break;
                    case 205:   //Added on 19-Sep-2016 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ClientsForMTMReq);
                        }
                        break;
                    case 207:   //Added on 08-Mar-2017 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ClientsForMTMReqMcx);
                        }
                        break;
                    #endregion

                    #region Nse Curr Derive
                    case 211:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec);
                        }
                        break;
                    case 221:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec);
                        }
                        break;
                    case 231:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec);
                        }
                        break;
                    #endregion
                    case 251:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LinkFailRec);
                        }
                        break;
                    case 252:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LogoutRequest);
                        }
                        break;
                    case 247:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LogoutRequest_2FA);
                        }
                        break;
                    case 209:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TemplateMargin);
                        }
                        break;
                    case 172:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TemplateMargin_Peak);
                        }
                        break;
                    case 210:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TemplateMarginNew);
                        }
                        break;
                    case 173:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PeakMarginData);
                        }
                        break;
                    case 217:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TemplateMargin);
                        }
                        break;
                    case 248:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.GetSegmentMaster);
                        }
                        break;
                    case 250:   //Added on 19-Sep-2016 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise);
                        }
                        break;
                    //Option Exercise
                    case Constants.OTS_EXERCISE_PLACE_REQ:
                    case Constants.OTS_EXERCISE_MODIFY_REQ:
                    case Constants.OTS_EXERCISE_CANCEL_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OptionExerciseRequestRec);
                        }
                        break;
                    case 226:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.GetTradesReport);
                        }
                        break;
                    case Constants.OTS_OFS_WATCHLIST:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_OFS_PLACE_BROKER_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PlaceOfsOrderRequest);
                        }
                        break;
                    case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PlaceOfsOrderRequest);
                        }
                        break;
                    case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PlaceOfsOrderRequest);
                        }
                        break;
                    case 235: //21.11.18 SLBM
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SLBMOrderPlaceRequestRec);
                        }
                        break;
                    case 236: //21.11.18 SLBM
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SLBMOrderModifyRequestRec);
                        }
                        break;
                    case 237: //21.11.18 SLBM
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SLBMOrderCancelRequestRec);
                        }
                        break;
                }
                Array.Copy(temp, temp1, temp.Length);
                ConvertedBuff.write(temp1, 163);
                ConvertedBuff.write(BitConverter.GetBytes(OTSReqRec.RetryCnt), 2);
                ConvertedBuff.write(BitConverter.GetBytes(OTSReqRec.ITS_Num), 4);
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void btnExchConnect2_Click(object sender, EventArgs e)
        {
            if (!Startup()) return;

            if (btnExchConnect2.Text == "Connect")
            {
                ExchConnectorSoc2 = new SocketClient(2, this);
                if (!ExchConnectorSoc2.Connected)   
                {
                    try
                    {
                        ExchConnectorSoc2.address = IPAddress.Parse(textBoxExchConn2IP.Text);
                        ExchConnectorSoc2.port = 18601;
                    }
                    catch (Exception er)
                    {
                        MessageBox.Show("An Invalid IP and/or port is specified in EC2");
                        return;
                    }
                    ExchConnectorSoc2.Connect();

                }
            }
            else
            {
                if (ExchConnectorSoc2.Connected)
                    ExchConnectorSoc2.Close();
                else
                    ExchConnectorConnectionStatusChanged(2);
            }
        }

        private void buttonShowException_Click(object sender, EventArgs e)
        {
            string FName = "OTS " + textBoxOTSID.Text + "_OTS_Ex" + DateTime.Now.ToString("yyyyMMdd") + ".Dat";
            System.Diagnostics.Process.Start("ExcProc.exe", FName);

        }

        private int GetFreeThreadCnt(int DBNo)
        {
            int x = 0;
            for (int i = 0; i < MaxThreads; i++)
            {
                if (DBThreadArrayNew[DBNo, i].Status == TThreadStatus.Suspended)
                    x++;
            }
            return x;
        }

        private void buttonMcastSave_Click(object sender, EventArgs e)
        {
            List<string> lines = new List<string>();
            foreach (string item in listBox1.Items)
            {
                lines.Add(item);
            }
            File.WriteAllLines("Mcast.txt", lines);
        }

        public void AccessControls(string str)
        {
            if (listBox1.Items.IndexOf(str) < 0)
                listBox1.Items.Add(str);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            File.WriteAllLines("OTSdb_" + DateTime.Now.ToString("yyyyMMdd") + ".Txt", SLReport);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            File.WriteAllLines("OTSdb_" + DateTime.Now.ToString("yyyyMMdd") + ".Txt", SLReport);
        }

        private void buttonSaveError_Click(object sender, EventArgs e)
        {
            List<string> lines = new List<string>();
            foreach (string item in listBox2.Items)
            {
                lines.Add(item);
            }
            File.WriteAllLines(textBoxErrors.Text, lines);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ITInteractiveStructures.TOTSRequestRecord CurrReq = new ITInteractiveStructures.TOTSRequestRecord();
            listBox3.Items.Clear();
            for (int i = 0; i < PendingRequestList.Count; i++)
            {
                CurrReq = PendingRequestList[i];
                listBox3.Items.Add("Trader =" + new string(CurrReq.ReqHdr.TraderID).Substring(0, CurrReq.ReqHdr.TraderIDLength) + ", FEUser =" + new string(CurrReq.ReqHdr.FEUserID).Substring(0, CurrReq.ReqHdr.FEUserIDLength) + ", Rectype =" + CurrReq.ReqHdr.RequestType);
            }
        }

        private void checkBoxWriteTodisply_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxWriteTodisply.Checked)
            {
                ITSRawFileStream = new FileStream(textBoxSaveToFileITSReq.Text, FileMode.Create, FileAccess.Write);
                ITSRawFileWriter = new BinaryWriter(ITSRawFileStream);
            }
            else
            {
                ITSRawFileWriter.Close();
                ITSRawFileStream.Close();
            }
        }

        private void buttonProcessFile_Click(object sender, EventArgs e)
        {
            BinaryReader ITSRawReader;
            int ActualDataRcvdLen;
            DateTime CurrTime;
            ITInteractiveStructures.RawHeader Rwheader = new ITInteractiveStructures.RawHeader();

            byte[] BuffToRead = new byte[1024 * 10];
            TWSocketClient CurrCli = null;
            if (!File.Exists(textBoxSaveToFileITSReq.Text))
            {
                MessageBox.Show(textBoxSaveToFileITSReq.Text + " missing");
                return;
            }
            if (checkBoxWriteTodisply.Checked)
                checkBoxWriteTodisply.Checked = false;
            try
            {
                ITSRawFileStream = new FileStream(textBoxSaveToFileITSReq.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                ITSRawReader = new BinaryReader(ITSRawFileStream);
                OutTextFile = new FileStream(textBoxSaveToFileITSReq.Text + ".txt", FileMode.Create, FileAccess.Write);
                OutTextFileWriter = new StreamWriter(OutTextFile);
                for (int i = 0; i < ListenToITS.FClientList.Count; i++)
                {
                    CurrCli = (TWSocketClient)ListenToITS.FClientList[i];
                    //if (Convert.ToInt32(textBoxITSNum.Text) != CurrCli.ITS_Num)
                    //{
                    //    CurrCli = null;
                    //    continue;
                    //}
                }
                if (CurrCli != null)
                {
                    ProcessFileITS = true;
                    while (ITSRawReader.BaseStream.Position < ITSRawReader.BaseStream.Length)
                    {
                        ITSRawReader.Read(BuffToRead, 0, Marshal.SizeOf(Rwheader));
                        Rwheader = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.RawHeader>(BuffToRead);
                        ActualDataRcvdLen = Rwheader.ReadSize;
                        CurrTime = Rwheader.CurrTime;
                        ITSRawReader.Read(BuffToRead, 0, ActualDataRcvdLen);
                        CurrCli.SockBuf.Write(BuffToRead, ActualDataRcvdLen);
                        ProcessITSRequest(CurrCli);

                    }
                }
                ProcessFileITS = false;
                ITSRawReader.Close();
                ITSRawFileStream.Close();
                OutTextFileWriter.Close();
                OutTextFile.Close();
                MessageBox.Show("Saved to " + textBoxSaveToFileITSReq.Text + ".txt");
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void WriteToDisplay(ITInteractiveStructures.TIARequestHeaderRecord IAReqHdr)
        {
            try
            {
                SLW2D.Clear();
                SLW2D.Add("");
                SLW2D.Add("");
                SLW2D.Add("ITS Header Info");
                SLW2D.Add("-----------------");
                SLW2D.Add("Account Type : " + IAReqHdr.AccountType);
                SLW2D.Add("Channel BC   : " + IAReqHdr.ChannelID_BC);
                SLW2D.Add("Channel NC   : " + IAReqHdr.ChannelID_NC);
                SLW2D.Add("Channel ND   : " + IAReqHdr.ChannelID_ND);
                SLW2D.Add("ClientType   : " + IAReqHdr.ClientType);
                SLW2D.Add("FEUserID     : " + new string(IAReqHdr.FEUserID));
                SLW2D.Add("FEUserIDLength : " + IAReqHdr.FEUserIDLength);
                SLW2D.Add("TraderID     : " + new string(IAReqHdr.TraderID));
                SLW2D.Add("TraderIDLength : " + IAReqHdr.TraderIDLength);
                SLW2D.Add("SocketNum    : " + IAReqHdr.SocketNum);
                SLW2D.Add("RequestType  : " + IAReqHdr.RequestType);
                SLW2D.Add("RequestLen   : " + IAReqHdr.RequestLen);

                for (int i = 0; i < SLW2D.Count; i++)
                {
                    OutTextFileWriter.WriteLine(SLW2D[i]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void WriteToDisplay(byte ReqType, ITInteractiveStructures.TAllRequestNewRecord AllReq)
        {
            try
            {
                switch (ReqType)
                {
                    case 1:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("ChkType1Info       : " + new string(AllReq.LoginRequest.ChkType1Info));
                            SLW2D.Add("ChkType1InfoLength : " + AllReq.LoginRequest.ChkType1InfoLength);
                            SLW2D.Add("ChkType2Info   : " + AllReq.LoginRequest.ChkType2Info);
                            SLW2D.Add("ChkType2InfoLength : " + AllReq.LoginRequest.ChkType2InfoLength);
                            SLW2D.Add("ClientIP       : " + new string(AllReq.LoginRequest.ClientIP));
                            SLW2D.Add("ClientIPLength : " + AllReq.LoginRequest.ClientIPLength);
                            SLW2D.Add("FEUserID       : " + new string(AllReq.LoginRequest.FEUserID));
                            SLW2D.Add("FEUserIDLength : " + AllReq.LoginRequest.FEUserIDLength);
                            SLW2D.Add("ITS_IP         : " + new string(AllReq.LoginRequest.ITS_IP));
                            SLW2D.Add("ITS_IPLength   : " + AllReq.LoginRequest.ITS_IPLength);
                            SLW2D.Add("FEUserID       : " + new string(AllReq.LoginRequest.MachineID));
                            SLW2D.Add("MachineIDLength: " + AllReq.LoginRequest.MachineIDLength);
                            SLW2D.Add("Password1      : " + new string(AllReq.LoginRequest.Password1));
                            SLW2D.Add("Password1Length: " + AllReq.LoginRequest.Password1Length);
                            SLW2D.Add("Password2      : " + new string(AllReq.LoginRequest.Password2));
                            SLW2D.Add("Password2Length: " + AllReq.LoginRequest.Password2Length);
                            SLW2D.Add("ServerIP       : " + new string(AllReq.LoginRequest.ServerIP));
                            SLW2D.Add("ServerIPLength : " + AllReq.LoginRequest.ServerIPLength);
                            SLW2D.Add("Version        : " + new string(AllReq.LoginRequest.Version));
                            SLW2D.Add("VersionLength  : " + AllReq.LoginRequest.VersionLength);
                        }
                        break;
                    case 2:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("ClientIP : " + new string(AllReq.ChgPswdRequestRec.ClientIP));
                            SLW2D.Add("ClientIPLength : " + AllReq.ChgPswdRequestRec.ClientIPLength);
                            SLW2D.Add("NewPassword1   : " + AllReq.ChgPswdRequestRec.NewPassword1);
                            SLW2D.Add("NewPassword1Length : " + AllReq.ChgPswdRequestRec.NewPassword1Length);
                            SLW2D.Add("Channel ND   : " + new string(AllReq.ChgPswdRequestRec.FEUserID));
                            SLW2D.Add("FEUserIDLength : " + AllReq.ChgPswdRequestRec.FEUserIDLength);
                            SLW2D.Add("ITS_IP   : " + new string(AllReq.ChgPswdRequestRec.ITS_IP));
                            SLW2D.Add("ITS_IPLength : " + AllReq.ChgPswdRequestRec.ITS_IPLength);
                            SLW2D.Add("FEUserID     : " + new string(AllReq.ChgPswdRequestRec.MachineID));
                            SLW2D.Add("MachineIDLength : " + AllReq.ChgPswdRequestRec.MachineIDLength);
                            SLW2D.Add("Password1     : " + new string(AllReq.ChgPswdRequestRec.Password1));
                            SLW2D.Add("Password1Length : " + AllReq.ChgPswdRequestRec.Password1Length);
                            SLW2D.Add("Password2     : " + new string(AllReq.ChgPswdRequestRec.Password2));
                            SLW2D.Add("Password2Length : " + AllReq.ChgPswdRequestRec.Password2Length);
                            SLW2D.Add("ServerIP     : " + new string(AllReq.ChgPswdRequestRec.ServerIP));
                            SLW2D.Add("ServerIPLength : " + AllReq.ChgPswdRequestRec.ServerIPLength);
                            SLW2D.Add("NewPassword2     : " + new string(AllReq.ChgPswdRequestRec.NewPassword2));
                            SLW2D.Add("NewPassword2Length : " + AllReq.ChgPswdRequestRec.NewPassword2Length);
                        }
                        break;

                    case 10:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AtMarket : " + AllReq.EquityOrderPlaceRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.EquityOrderPlaceRequestRec.BuySell);
                            SLW2D.Add("DisclosedQty : " + AllReq.EquityOrderPlaceRequestRec.DisclosedQty);
                            SLW2D.Add("Exch : " + AllReq.EquityOrderPlaceRequestRec.Exch);
                            SLW2D.Add("ExchType : " + AllReq.EquityOrderPlaceRequestRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.EquityOrderPlaceRequestRec.IOC);
                            SLW2D.Add("Qty : " + AllReq.EquityOrderPlaceRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.EquityOrderPlaceRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.EquityOrderPlaceRequestRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.EquityOrderPlaceRequestRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.EquityOrderPlaceRequestRec.ScripNameLength);
                            SLW2D.Add("TraderRequestID : " + AllReq.EquityOrderPlaceRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.EquityOrderPlaceRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.EquityOrderPlaceRequestRec.WithSL);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_PLACE_REQ:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("OrderType : " + AllReq.OrderTypePlaceReqRec.OrderType);
                            SLW2D.Add("ProductType: " + AllReq.OrderTypePlaceReqRec.ProductType);
                            SLW2D.Add("AtMarket : " + AllReq.OrderTypePlaceReqRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.OrderTypePlaceReqRec.BuySell);
                            SLW2D.Add("DisclosedQty : " + AllReq.OrderTypePlaceReqRec.DisclosedQty);
                            SLW2D.Add("Exch : " + AllReq.OrderTypePlaceReqRec.Exch);
                            SLW2D.Add("ExchType : " + AllReq.OrderTypePlaceReqRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.OrderTypePlaceReqRec.IOC);
                            SLW2D.Add("Qty : " + AllReq.OrderTypePlaceReqRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.OrderTypePlaceReqRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.OrderTypePlaceReqRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.OrderTypePlaceReqRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.OrderTypePlaceReqRec.ScripNameLength);
                            SLW2D.Add("CALevel : " + AllReq.OrderTypePlaceReqRec.CALevel);
                            SLW2D.Add("CPType : " + AllReq.OrderTypePlaceReqRec.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.OrderTypePlaceReqRec.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.OrderTypePlaceReqRec.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.OrderTypePlaceReqRec.StrikePrice);
                            SLW2D.Add("TraderRequestID : " + AllReq.OrderTypePlaceReqRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.OrderTypePlaceReqRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.OrderTypePlaceReqRec.WithSL);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_PLACE_REQ_COVER:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("OrderType : " + AllReq.PlaceCoverOrderReqRec.OrderType);
                            SLW2D.Add("ProductType: " + AllReq.PlaceCoverOrderReqRec.ProductType);
                            SLW2D.Add("AtMarket : " + AllReq.PlaceCoverOrderReqRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.PlaceCoverOrderReqRec.BuySell);
                            SLW2D.Add("DisclosedQty : " + AllReq.PlaceCoverOrderReqRec.DisclosedQty);
                            SLW2D.Add("Exch : " + AllReq.PlaceCoverOrderReqRec.Exch);
                            SLW2D.Add("ExchType : " + AllReq.PlaceCoverOrderReqRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.PlaceCoverOrderReqRec.IOC);
                            SLW2D.Add("Qty : " + AllReq.PlaceCoverOrderReqRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.PlaceCoverOrderReqRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.PlaceCoverOrderReqRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.PlaceCoverOrderReqRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.PlaceCoverOrderReqRec.ScripNameLength);
                            SLW2D.Add("CALevel : " + AllReq.PlaceCoverOrderReqRec.CALevel);
                            SLW2D.Add("CPType : " + AllReq.PlaceCoverOrderReqRec.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.PlaceCoverOrderReqRec.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.PlaceCoverOrderReqRec.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.PlaceCoverOrderReqRec.StrikePrice);
                            SLW2D.Add("TraderRequestID : " + AllReq.PlaceCoverOrderReqRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.PlaceCoverOrderReqRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.PlaceCoverOrderReqRec.WithSL);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("OrderType : " + AllReq.OrderTypeModifyReqRec.OrderType);
                            SLW2D.Add("ProductType: " + AllReq.OrderTypeModifyReqRec.ProductType);
                            SLW2D.Add("AHStatus : " + AllReq.OrderTypeModifyReqRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.OrderTypeModifyReqRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.OrderTypeModifyReqRec.BuySell);
                            SLW2D.Add("DisclosedQty : " + AllReq.OrderTypeModifyReqRec.DisclosedQty);
                            SLW2D.Add("Exch : " + AllReq.OrderTypeModifyReqRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.OrderTypeModifyReqRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.OrderTypeModifyReqRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.OrderTypeModifyReqRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.OrderTypeModifyReqRec.IOC);
                            SLW2D.Add("OldQty : " + AllReq.OrderTypeModifyReqRec.OldQty);
                            SLW2D.Add("PendingQty : " + AllReq.OrderTypeModifyReqRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.OrderTypeModifyReqRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.OrderTypeModifyReqRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.OrderTypeModifyReqRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.OrderTypeModifyReqRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.OrderTypeModifyReqRec.ScripNameLength);
                            SLW2D.Add("CALevel : " + AllReq.OrderTypeModifyReqRec.CALevel);
                            SLW2D.Add("CPType : " + AllReq.OrderTypeModifyReqRec.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.OrderTypeModifyReqRec.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.OrderTypeModifyReqRec.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.OrderTypeModifyReqRec.StrikePrice);
                            SLW2D.Add("SLTriggered : " + AllReq.OrderTypeModifyReqRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.OrderTypeModifyReqRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.OrderTypeModifyReqRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.OrderTypeModifyReqRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.OrderTypeModifyReqRec.WithSL);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("OrderType : " + AllReq.OrderTypeCancelReqRec.OrderType);
                            SLW2D.Add("ProductType: " + AllReq.OrderTypeCancelReqRec.ProductType);
                            SLW2D.Add("AHStatus : " + AllReq.OrderTypeCancelReqRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.OrderTypeCancelReqRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.OrderTypeCancelReqRec.BuySell);
                            SLW2D.Add("Exch : " + AllReq.OrderTypeCancelReqRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.OrderTypeCancelReqRec.ExchangeOrderID);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.OrderTypeCancelReqRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.OrderTypeCancelReqRec.ExchType);
                            SLW2D.Add("PendingQty : " + AllReq.OrderTypeCancelReqRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.OrderTypeCancelReqRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.OrderTypeCancelReqRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.OrderTypeCancelReqRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.OrderTypeCancelReqRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.OrderTypeCancelReqRec.ScripNameLength);
                            SLW2D.Add("CALevel : " + AllReq.OrderTypeCancelReqRec.CALevel);
                            SLW2D.Add("CPType : " + AllReq.OrderTypeCancelReqRec.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.OrderTypeCancelReqRec.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.OrderTypeCancelReqRec.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.OrderTypeCancelReqRec.StrikePrice);
                            SLW2D.Add("SLTriggered : " + AllReq.OrderTypeCancelReqRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.OrderTypeCancelReqRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.OrderTypeCancelReqRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.OrderTypeCancelReqRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.OrderTypeCancelReqRec.WithSL);
                        }
                        break;
                    case 20:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AHStatus : " + AllReq.EquityOrderModifyRequestRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.EquityOrderModifyRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.EquityOrderModifyRequestRec.BuySell);
                            SLW2D.Add("DisclosedQty : " + AllReq.EquityOrderModifyRequestRec.DisclosedQty);
                            SLW2D.Add("Exch : " + AllReq.EquityOrderModifyRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.EquityOrderModifyRequestRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.EquityOrderModifyRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.EquityOrderModifyRequestRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.EquityOrderModifyRequestRec.IOC);
                            SLW2D.Add("OldQty : " + AllReq.EquityOrderModifyRequestRec.OldQty);
                            SLW2D.Add("PendingQty : " + AllReq.EquityOrderModifyRequestRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.EquityOrderModifyRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.EquityOrderModifyRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.EquityOrderModifyRequestRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.EquityOrderModifyRequestRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.EquityOrderModifyRequestRec.ScripNameLength);
                            SLW2D.Add("SLTriggered : " + AllReq.EquityOrderModifyRequestRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.EquityOrderModifyRequestRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.EquityOrderModifyRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.EquityOrderModifyRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.EquityOrderModifyRequestRec.WithSL);
                        }
                        break;
                    case 30:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AHStatus : " + AllReq.EquityOrderCancelRequestRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.EquityOrderCancelRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.EquityOrderCancelRequestRec.BuySell);
                            SLW2D.Add("Exch : " + AllReq.EquityOrderCancelRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.EquityOrderCancelRequestRec.ExchangeOrderID);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.EquityOrderCancelRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.EquityOrderCancelRequestRec.ExchType);
                            SLW2D.Add("PendingQty : " + AllReq.EquityOrderCancelRequestRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.EquityOrderCancelRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.EquityOrderCancelRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.EquityOrderCancelRequestRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.EquityOrderCancelRequestRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.EquityOrderCancelRequestRec.ScripNameLength);
                            SLW2D.Add("SLTriggered : " + AllReq.EquityOrderCancelRequestRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.EquityOrderCancelRequestRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.EquityOrderCancelRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.EquityOrderCancelRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.EquityOrderCancelRequestRec.WithSL);
                        }
                        break;
                    case 110:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AtMarket : " + AllReq.DerivOrderPlaceRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.DerivOrderPlaceRequestRec.BuySell);
                            SLW2D.Add("CALevel : " + AllReq.DerivOrderPlaceRequestRec.DerivInfo.CALevel);
                            SLW2D.Add("CPType : " + AllReq.DerivOrderPlaceRequestRec.DerivInfo.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.DerivOrderPlaceRequestRec.DerivInfo.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.DerivOrderPlaceRequestRec.DerivInfo.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.DerivOrderPlaceRequestRec.DerivInfo.StrikePrice);
                            SLW2D.Add("Symbol : " + new string(AllReq.DerivOrderPlaceRequestRec.DerivInfo.Symbol));
                            SLW2D.Add("SymbolLength : " + AllReq.DerivOrderPlaceRequestRec.DerivInfo.SymbolLength);
                            SLW2D.Add("Exch : " + AllReq.DerivOrderPlaceRequestRec.Exch);
                            SLW2D.Add("ExchType : " + AllReq.DerivOrderPlaceRequestRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.DerivOrderPlaceRequestRec.IOC);
                            SLW2D.Add("Qty : " + AllReq.DerivOrderPlaceRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.DerivOrderPlaceRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.DerivOrderPlaceRequestRec.ScripCode);
                            SLW2D.Add("TraderRequestID : " + AllReq.DerivOrderPlaceRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.DerivOrderPlaceRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.DerivOrderPlaceRequestRec.WithSL);
                        }
                        break;
                    case 120:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AHStatus : " + AllReq.DerivOrderModifyRequestRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.DerivOrderModifyRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.DerivOrderModifyRequestRec.BuySell);
                            SLW2D.Add("CALevel : " + AllReq.DerivOrderModifyRequestRec.DerivInfo.CALevel);
                            SLW2D.Add("CPType : " + AllReq.DerivOrderModifyRequestRec.DerivInfo.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.DerivOrderModifyRequestRec.DerivInfo.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.DerivOrderModifyRequestRec.DerivInfo.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.DerivOrderModifyRequestRec.DerivInfo.StrikePrice);
                            SLW2D.Add("Symbol : " + new string(AllReq.DerivOrderModifyRequestRec.DerivInfo.Symbol));
                            SLW2D.Add("SymbolLength : " + AllReq.DerivOrderModifyRequestRec.DerivInfo.SymbolLength);
                            SLW2D.Add("Exch : " + AllReq.DerivOrderModifyRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.DerivOrderModifyRequestRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.DerivOrderModifyRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.DerivOrderModifyRequestRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.DerivOrderModifyRequestRec.IOC);
                            SLW2D.Add("OldQty : " + AllReq.DerivOrderModifyRequestRec.OldQty);
                            SLW2D.Add("PendingQty : " + AllReq.DerivOrderModifyRequestRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.DerivOrderModifyRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.DerivOrderModifyRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.DerivOrderModifyRequestRec.ScripCode);
                            SLW2D.Add("SLTriggered : " + AllReq.DerivOrderModifyRequestRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.DerivOrderModifyRequestRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.DerivOrderModifyRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.DerivOrderModifyRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.DerivOrderModifyRequestRec.WithSL);
                        }
                        break;
                    case 130:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AHStatus : " + AllReq.DerivOrderCancelRequestRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.DerivOrderCancelRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.DerivOrderCancelRequestRec.BuySell);
                            SLW2D.Add("CALevel : " + AllReq.DerivOrderCancelRequestRec.DerivInfo.CALevel);
                            SLW2D.Add("CPType : " + AllReq.DerivOrderCancelRequestRec.DerivInfo.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.DerivOrderCancelRequestRec.DerivInfo.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.DerivOrderCancelRequestRec.DerivInfo.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.DerivOrderCancelRequestRec.DerivInfo.StrikePrice);
                            SLW2D.Add("Symbol : " + new string(AllReq.DerivOrderCancelRequestRec.DerivInfo.Symbol));
                            SLW2D.Add("SymbolLength : " + AllReq.DerivOrderCancelRequestRec.DerivInfo.SymbolLength);
                            SLW2D.Add("Exch : " + AllReq.DerivOrderCancelRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.DerivOrderCancelRequestRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.DerivOrderCancelRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.DerivOrderCancelRequestRec.ExchType);
                            SLW2D.Add("PendingQty : " + AllReq.DerivOrderCancelRequestRec.OldQty);
                            SLW2D.Add("PendingQty : " + AllReq.DerivOrderCancelRequestRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.DerivOrderCancelRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.DerivOrderCancelRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.DerivOrderCancelRequestRec.ScripCode);
                            SLW2D.Add("SLTriggered : " + AllReq.DerivOrderCancelRequestRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.DerivOrderCancelRequestRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.DerivOrderCancelRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.DerivOrderCancelRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.DerivOrderCancelRequestRec.WithSL);
                        }
                        break;
                    case 211:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AtMarket : " + AllReq.CurrDerivOrderPlaceRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.CurrDerivOrderPlaceRequestRec.BuySell);
                            SLW2D.Add("CALevel : " + AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo.CALevel);
                            SLW2D.Add("CPType : " + AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo.StrikePrice);
                            SLW2D.Add("Symbol : " + new string(AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo.Symbol));
                            SLW2D.Add("SymbolLength : " + AllReq.CurrDerivOrderPlaceRequestRec.DerivInfo.SymbolLength);
                            SLW2D.Add("Exch : " + AllReq.CurrDerivOrderPlaceRequestRec.Exch);
                            SLW2D.Add("ExchType : " + AllReq.CurrDerivOrderPlaceRequestRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.CurrDerivOrderPlaceRequestRec.IOC);
                            SLW2D.Add("Qty : " + AllReq.CurrDerivOrderPlaceRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.CurrDerivOrderPlaceRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.CurrDerivOrderPlaceRequestRec.ScripCode);
                            SLW2D.Add("TraderRequestID : " + AllReq.CurrDerivOrderPlaceRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.CurrDerivOrderPlaceRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.CurrDerivOrderPlaceRequestRec.WithSL);
                        }
                        break;
                    case 221:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AHStatus : " + AllReq.CurrDerivOrderModifyRequestRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.CurrDerivOrderModifyRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.CurrDerivOrderModifyRequestRec.BuySell);
                            SLW2D.Add("CALevel : " + AllReq.CurrDerivOrderModifyRequestRec.DerivInfo.CALevel);
                            SLW2D.Add("CPType : " + AllReq.CurrDerivOrderModifyRequestRec.DerivInfo.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.CurrDerivOrderModifyRequestRec.DerivInfo.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.CurrDerivOrderModifyRequestRec.DerivInfo.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.CurrDerivOrderModifyRequestRec.DerivInfo.StrikePrice);
                            SLW2D.Add("Symbol : " + new string(AllReq.CurrDerivOrderModifyRequestRec.DerivInfo.Symbol));
                            SLW2D.Add("SymbolLength : " + AllReq.CurrDerivOrderModifyRequestRec.DerivInfo.SymbolLength);
                            SLW2D.Add("Exch : " + AllReq.CurrDerivOrderModifyRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.CurrDerivOrderModifyRequestRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.CurrDerivOrderModifyRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.CurrDerivOrderModifyRequestRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.CurrDerivOrderModifyRequestRec.IOC);
                            SLW2D.Add("OldQty : " + AllReq.CurrDerivOrderModifyRequestRec.OldQty);
                            SLW2D.Add("PendingQty : " + AllReq.CurrDerivOrderModifyRequestRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.CurrDerivOrderModifyRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.CurrDerivOrderModifyRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.CurrDerivOrderModifyRequestRec.ScripCode);
                            SLW2D.Add("SLTriggered : " + AllReq.CurrDerivOrderModifyRequestRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.CurrDerivOrderModifyRequestRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.CurrDerivOrderModifyRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.CurrDerivOrderModifyRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.CurrDerivOrderModifyRequestRec.WithSL);
                        }
                        break;
                    case 231:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AHStatus : " + AllReq.CurrDerivOrderCancelRequestRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.CurrDerivOrderCancelRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.CurrDerivOrderCancelRequestRec.BuySell);
                            SLW2D.Add("CALevel : " + AllReq.CurrDerivOrderCancelRequestRec.DerivInfo.CALevel);
                            SLW2D.Add("CPType : " + AllReq.CurrDerivOrderCancelRequestRec.DerivInfo.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.CurrDerivOrderCancelRequestRec.DerivInfo.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.CurrDerivOrderCancelRequestRec.DerivInfo.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.CurrDerivOrderCancelRequestRec.DerivInfo.StrikePrice);
                            SLW2D.Add("Symbol : " + new string(AllReq.CurrDerivOrderCancelRequestRec.DerivInfo.Symbol));
                            SLW2D.Add("SymbolLength : " + AllReq.CurrDerivOrderCancelRequestRec.DerivInfo.SymbolLength);
                            SLW2D.Add("Exch : " + AllReq.CurrDerivOrderCancelRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.CurrDerivOrderCancelRequestRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.CurrDerivOrderCancelRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.CurrDerivOrderCancelRequestRec.ExchType);
                            SLW2D.Add("PendingQty : " + AllReq.CurrDerivOrderCancelRequestRec.OldQty);
                            SLW2D.Add("PendingQty : " + AllReq.CurrDerivOrderCancelRequestRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.CurrDerivOrderCancelRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.CurrDerivOrderCancelRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.CurrDerivOrderCancelRequestRec.ScripCode);
                            SLW2D.Add("SLTriggered : " + AllReq.CurrDerivOrderCancelRequestRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.CurrDerivOrderCancelRequestRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.CurrDerivOrderCancelRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.CurrDerivOrderCancelRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.CurrDerivOrderCancelRequestRec.WithSL);
                        }
                        break;
                    //Option Exercise
                    case Constants.OTS_EXERCISE_PLACE_REQ:
                    case Constants.OTS_EXERCISE_MODIFY_REQ:
                    case Constants.OTS_EXERCISE_CANCEL_REQ:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("Buy/Sell            : " + AllReq.OptionExerciseRequestRec.BuySell);
                            SLW2D.Add("Exercise : " + AllReq.OptionExerciseRequestRec.Exercise);
                            SLW2D.Add("Exch : " + AllReq.OptionExerciseRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.OptionExerciseRequestRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.OptionExerciseRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.OptionExerciseRequestRec.ExchType);
                            SLW2D.Add("OldQty : " + AllReq.OptionExerciseRequestRec.OldQty);
                            SLW2D.Add("Qty : " + AllReq.OptionExerciseRequestRec.Qty);
                            SLW2D.Add("ScripCode : " + AllReq.OptionExerciseRequestRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.OptionExerciseRequestRec.DerivInfo.Symbol));
                            SLW2D.Add("ScripNameLength : " + AllReq.OptionExerciseRequestRec.DerivInfo.SymbolLength);
                            SLW2D.Add("CALevel : " + AllReq.OptionExerciseRequestRec.DerivInfo.CALevel);
                            SLW2D.Add("CPType : " + AllReq.OptionExerciseRequestRec.DerivInfo.CPType);
                            SLW2D.Add("ExpiryDate : " + AllReq.OptionExerciseRequestRec.DerivInfo.ExpiryDate);
                            SLW2D.Add("InstrType : " + AllReq.OptionExerciseRequestRec.DerivInfo.InstrType);
                            SLW2D.Add("StrikePrice : " + AllReq.OptionExerciseRequestRec.DerivInfo.StrikePrice);


                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("Exch : " + AllReq.MfAddModifyCancelReqRec.Exch);
                            SLW2D.Add("ExchType : " + AllReq.MfAddModifyCancelReqRec.ExchType);
                            SLW2D.Add("UniqueNo : " + new string(AllReq.MfAddModifyCancelReqRec.UniqueNo));
                            SLW2D.Add("SchemeCode : " + new string(AllReq.MfAddModifyCancelReqRec.SchemeCode));
                            SLW2D.Add("BuySell : " + AllReq.MfAddModifyCancelReqRec.BuySell);
                            SLW2D.Add("BuySellType : " + new string(AllReq.MfAddModifyCancelReqRec.BuySellType));
                            SLW2D.Add("DptxnType : " + AllReq.MfAddModifyCancelReqRec.DptxnType);
                            SLW2D.Add("Amount : " + AllReq.MfAddModifyCancelReqRec.Amount);
                            SLW2D.Add("RedeemQty : " + AllReq.MfAddModifyCancelReqRec.RedeemQty);
                            SLW2D.Add("FolioNo : " + new string(AllReq.MfAddModifyCancelReqRec.FolioNo));
                            SLW2D.Add("KYCStatus : " + AllReq.MfAddModifyCancelReqRec.KYCStatus);
                            SLW2D.Add("ExchOrderNo : " + AllReq.MfAddModifyCancelReqRec.ExchOrderNo);
                            SLW2D.Add("DPCFlag : " + AllReq.MfAddModifyCancelReqRec.DPCFlag);
                            SLW2D.Add("TraderRequestID : " + AllReq.MfAddModifyCancelReqRec.TraderRequestID);
                        } break;
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("Exch : " + AllReq.MfSIPRegistrationReqRec.Exch);
                            SLW2D.Add("ExchType : " + AllReq.MfSIPRegistrationReqRec.ExchType);
                            SLW2D.Add("SchemeCode : " + new string(AllReq.MfSIPRegistrationReqRec.SchemeCode));
                            SLW2D.Add("TxnMode : " + AllReq.MfSIPRegistrationReqRec.TxnMode);
                            SLW2D.Add("DptxnType : " + AllReq.MfSIPRegistrationReqRec.DptxnType);
                            SLW2D.Add("Amount : " + AllReq.MfSIPRegistrationReqRec.Amount);
                            SLW2D.Add("FolioNo : " + new string(AllReq.MfSIPRegistrationReqRec.FolioNo));
                            SLW2D.Add("SIPregistrationNum : " + AllReq.MfSIPRegistrationReqRec.SIPregistrationNum);
                            SLW2D.Add("Numberofinstallments : " + AllReq.MfSIPRegistrationReqRec.Numberofinstallments);
                            SLW2D.Add("DPCFlag : " + AllReq.MfSIPRegistrationReqRec.DPCFlag);
                            SLW2D.Add("TraderRequestID : " + AllReq.MfSIPRegistrationReqRec.TraderRequestID);
                        } break;
                    case 235:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AtMarket : " + AllReq.SLBMOrderPlaceRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.SLBMOrderPlaceRequestRec.BuySell);
                            SLW2D.Add("DisclosedQty : " + AllReq.SLBMOrderPlaceRequestRec.DisclosedQty);
                            SLW2D.Add("Exch : " + AllReq.SLBMOrderPlaceRequestRec.Exch);
                            SLW2D.Add("ExchType : " + AllReq.SLBMOrderPlaceRequestRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.SLBMOrderPlaceRequestRec.IOC);
                            SLW2D.Add("Qty : " + AllReq.SLBMOrderPlaceRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.SLBMOrderPlaceRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.SLBMOrderPlaceRequestRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.SLBMOrderPlaceRequestRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.SLBMOrderPlaceRequestRec.ScripNameLength);
                            SLW2D.Add("Series : " + new string(AllReq.SLBMOrderPlaceRequestRec.Series));
                            SLW2D.Add("TraderRequestID : " + AllReq.SLBMOrderPlaceRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.SLBMOrderPlaceRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.SLBMOrderPlaceRequestRec.WithSL);
                        }
                        break;
                    case 236:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AHStatus : " + AllReq.SLBMOrderModifyRequestRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.SLBMOrderModifyRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.SLBMOrderModifyRequestRec.BuySell);
                            SLW2D.Add("DisclosedQty : " + AllReq.SLBMOrderModifyRequestRec.DisclosedQty);
                            SLW2D.Add("Exch : " + AllReq.SLBMOrderModifyRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.SLBMOrderModifyRequestRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.SLBMOrderModifyRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.SLBMOrderModifyRequestRec.ExchType);
                            SLW2D.Add("IOC : " + AllReq.SLBMOrderModifyRequestRec.IOC);
                            SLW2D.Add("OldQty : " + AllReq.SLBMOrderModifyRequestRec.OldQty);
                            SLW2D.Add("PendingQty : " + AllReq.SLBMOrderModifyRequestRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.SLBMOrderModifyRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.SLBMOrderModifyRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.SLBMOrderModifyRequestRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.SLBMOrderModifyRequestRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.SLBMOrderModifyRequestRec.ScripNameLength);
                            SLW2D.Add("Series : " + new string(AllReq.SLBMOrderModifyRequestRec.Series));
                            SLW2D.Add("SLTriggered : " + AllReq.SLBMOrderModifyRequestRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.SLBMOrderModifyRequestRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.SLBMOrderModifyRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.SLBMOrderModifyRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.SLBMOrderModifyRequestRec.WithSL);
                        }
                        break;
                    case 237:
                        {
                            SLW2D.Clear();
                            SLW2D.Add("");
                            SLW2D.Add("");
                            SLW2D.Add("ITS Body Info");
                            SLW2D.Add("-----------------");
                            SLW2D.Add("AHStatus : " + AllReq.SLBMOrderCancelRequestRec.AHStatus);
                            SLW2D.Add("AtMarket : " + AllReq.SLBMOrderCancelRequestRec.AtMarket);
                            SLW2D.Add("Buy/Sell            : " + AllReq.SLBMOrderCancelRequestRec.BuySell);
                            SLW2D.Add("Exch : " + AllReq.SLBMOrderCancelRequestRec.Exch);
                            SLW2D.Add("ExchangeOrderId : " + AllReq.SLBMOrderCancelRequestRec.ExchangeOrderId);
                            SLW2D.Add("ExchangeOrderTime : " + AllReq.SLBMOrderCancelRequestRec.ExchangeOrderTime);
                            SLW2D.Add("ExchType : " + AllReq.SLBMOrderCancelRequestRec.ExchType);
                            SLW2D.Add("PendingQty : " + AllReq.SLBMOrderCancelRequestRec.PendingQty);
                            SLW2D.Add("Qty : " + AllReq.SLBMOrderCancelRequestRec.Qty);
                            SLW2D.Add("Rate : " + AllReq.SLBMOrderCancelRequestRec.Rate);
                            SLW2D.Add("ScripCode : " + AllReq.SLBMOrderCancelRequestRec.ScripCode);
                            SLW2D.Add("ScripName : " + new string(AllReq.SLBMOrderCancelRequestRec.ScripName));
                            SLW2D.Add("ScripNameLength : " + AllReq.SLBMOrderCancelRequestRec.ScripNameLength);
                            SLW2D.Add("Series : " + new string(AllReq.SLBMOrderCancelRequestRec.Series));
                            SLW2D.Add("SLTriggered : " + AllReq.SLBMOrderCancelRequestRec.SLTriggered);
                            SLW2D.Add("TradedQty : " + AllReq.SLBMOrderCancelRequestRec.TradedQty);
                            SLW2D.Add("TraderRequestID : " + AllReq.SLBMOrderCancelRequestRec.TraderRequestID);
                            SLW2D.Add("TriggerRate : " + AllReq.SLBMOrderCancelRequestRec.TriggerRate);
                            SLW2D.Add("WithSL : " + AllReq.SLBMOrderCancelRequestRec.WithSL);
                        }
                        break;

                }
                for (int i = 0; i < SLW2D.Count; i++)
                {
                    OutTextFileWriter.WriteLine(SLW2D[i]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void checkBoxWriteEC_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBoxWriteEC.Checked)
                {
                    ECRawFileStream = new FileStream(textBoxFileNameEC.Text, FileMode.Create, FileAccess.Write);
                    ECRawFileWriter = new BinaryWriter(ECRawFileStream);
                }
                else
                {
                    ECRawFileWriter.Close();
                    ECRawFileStream.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void btn_LogReqtoDB_Click(object sender, EventArgs e)
        {
            if (!Startup()) return;
            File.WriteAllLines("OTSReqToDb_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".txt", LogReqToDbList);//17.09.14
            File.WriteAllLines("OTSAllReqToDb_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".txt", LogAllReqToDbList);//16.05.2018 Pranoti Gad
            MessageBox.Show("Log File is generated.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void WriteExToTxtFile(string ReqStr, string ExStr, int ExType, byte ReqType, ITInteractiveStructures.TOTSRequestRecord OTSReqRec)
        {
            try
            {
                string FName = textBoxOTSID.Text + "_OTSEx_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", DateTimeVar = DateTime.Now.ToString("HH:mm:ss"); //04.04.16
                if (ReqStr == "" || ReqStr == string.Empty || ExStr.Contains("Index and length")) //12.04.16 Sanchit
                {
                    File.AppendAllText(FName, "\r\n" + "------------------------------------------------------------------------------------------------");
                    File.AppendAllText(FName, "\r\n" + "Index and length Error........");
                    ReqStr = "Request String Unavaillable. RequestRec Hdr Details: " + new string(OTSReqRec.ReqHdr.TraderID, 0, OTSReqRec.ReqHdr.TraderIDLength) + "|" +
                              new string(OTSReqRec.ReqHdr.FEUserID, 0, OTSReqRec.ReqHdr.FEUserIDLength) + "|" + OTSReqRec.ReqHdr.ChannelID_NC.ToString() + "|" +
                              OTSReqRec.ReqHdr.ChannelID_ND.ToString() + "|" + OTSReqRec.ReqHdr.ChannelID_BC.ToString() + "|" + OTSReqRec.ITS_Num + "|" + new string(OTSReqRec.IP, 0, OTSReqRec.IPLength);
                    ConvertToBytesForExFile(OTSReqRec); //13.04.16
                }
                File.AppendAllText(FName, "\r\n" + DateTimeVar + " Exception Type: " + ExType + " Request Hdr ReqType: " + ReqType);
                File.AppendAllText(FName, "\r\n" + DateTimeVar + " Request String: " + ReqStr);
                File.AppendAllText(FName, "\r\n" + DateTimeVar + " Exceptn String: " + ExStr);
                File.AppendAllText(FName, "\r\n" + "------------------------------------------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void ConvertToBytesForExFile(ITInteractiveStructures.TOTSRequestRecord OTSReqRec)
        {
            try
            {
                string FName = "OTSRawDataEx_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".dat";
                FileStream Raw = new FileStream(FName, FileMode.Create, FileAccess.Write);
                BinaryWriter RawWriter = new BinaryWriter(Raw);
                byte[] temp = null, Hdr = CommonFunctions.RawSerialize(OTSReqRec.ReqHdr).ToArray();
                RawWriter.Write(Hdr);
                switch (OTSReqRec.ReqHdr.RequestType)
                {
                    case 1:
                    case 3: //20.10.15
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LoginRequest);
                        }
                        break;
                    case 85:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LoginRequestTwoFactor);
                        }
                        break;
                    case 4: //20.10.15
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LoginRequestForName);
                        }
                        break;
                    case 2:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ChgPswdRequestRec);
                        }
                        break;
                    case 5:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TraderListRequestRec);
                        }
                        break;
                    case 6:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.VerifyTraderRequestRec);
                        }
                        break;
                    case 10:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.EquityOrderPlaceRequestRec);
                        }
                        break;
                    case 14:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.GetIVRDealerDetailsReq); //24.04.23 For getting IVR Dealer Details
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_PLACE_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderTypePlaceReqRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MODIFY_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderTypeModifyReqRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_CANCEL_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderTypeCancelReqRec);
                        }
                        break;
                    case 20:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.EquityOrderModifyRequestRec);
                        }
                        break;
                    case 30:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.EquityOrderCancelRequestRec);
                        }
                        break;
                    case 41:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ScripMarginRequestRec);
                        }
                        break;
                    case Constants.OTS_SECURITY_INFO:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ScripInfoRequestRec);
                        }
                        break;
                    case Constants.OTS_SCRIP_MARGIN:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ScripMarginRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ:
                    case Constants.OTS_ORDERTYPE_MF_SIPCANCEL_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfSIPRegistrationReqRec);
                        }break;
                    case Constants.OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.XMfSIPRegistrationReqRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_PLACE_REQ://43 BseMf Krutika
                    case Constants.OTS_ORDERTYPE_MF_MODIFY_REQ://44
                    case Constants.OTS_ORDERTYPE_MF_CANCEL_REQ://45
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfAddModifyCancelReqRec);//BseMf
                        }
                        break;
                    case Constants.OTS_MANDATE:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.XSIPMandateRec);//BseMf
                        }break;
                    //case 61:
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC:
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142:    //Added on 5-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW:    //Added on 18-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER:     // sumitp 28_jan_22
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TradeReportRequestRec);
                        }
                        break;
                    //case 62:
                    case 7:
                    case 8:
                    case 125:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC:
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141: //Added on 5-Apr-2018 - Rohan
                    case Constants.OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW: //Added on 18-Apr-2018 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_EXERCISE_REPORTS_REC://Options Exercise
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ExerciseReportRequestRec);
                        }
                        break;
                    //case 64:
                    case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC:
                    case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW:
                    case Constants.OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW:  //Added on 18-Apr-2018 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ActivityLogRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new: //sumit 21_jan_22   single activity log
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ActivityLogRequestRec);
                        }
                        break;
                    case 66:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LicenceInfoRequestRec);
                        }
                        break;
                    case 68:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.CliDBNumRequestRec);
                        }
                        break;
                    case 69:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.HoldingsReportRequestRec);
                        }
                        break;
                    case 70:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.HoldingsValuationReportRequestRec);
                        }
                        break;
                    case 218:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.HoldingsValuationReportRequestRec);
                        }
                        break;
                    case Constants.OTS_HOLDING_VALUATION_REPORT_NEW:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.HoldingsValuationReportWithMemVarRequestRec);
                        }
                        break;
                    case 73:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DBDateRequestRec);
                        }
                        break;
                    case 79:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ClientPositionRequestRec);
                        }
                        break;
                    case 80:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TradeToTradeRequestRec);
                        }
                        break;
                    case 160:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SurveillanceIndicatorReq);
                        }
                        break;
                    case 161:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SurveillanceIndicatorReq);
                        }
                        break;
                    case 81:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.BseNseMapRequestRec);
                        }
                        break;
                    case 82:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DerivBannedScripRequestRec);
                        }
                        break;
                    case 83:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.AdhocReportRequestRec);
                        }
                        break;
                    case 84:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.AdhocModifyRequestRec);
                        }
                        break;
                    case 86:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MFClientsReportRequestRec);
                        }
                        break;
                    case 87:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MFClientHoldingReportRequestRec);
                        }
                        break;
                    case 89:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.IlliquidRequestRec);
                        }
                        break;
                    case 90:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PreOpenScripRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_ORDER_REPORTS_REC://bseMf Krutika
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfOrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_Acitivitylog_MF_ORDER_REPORTS_REC://bseMf Krutika
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfActivitylogRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC://bseMf Krutika
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MfOrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC://bseMf Krutika
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MandateOrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_VERIFY_USER:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.VerifyUserRequestRec);
                        }
                        break;
                    case 110:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DerivOrderPlaceRequestRec);
                        }
                        break;
                    case 115:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.AHDerivOrderPlaceRequestRec);
                        }
                        break;
                    case 120:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DerivOrderModifyRequestRec);
                        }
                        break;
                    case 130:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.DerivOrderCancelRequestRec);
                        }
                        break;
                    //02.03.23
                    case 134:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.orderPlaceReqRecForApproval);
                        }
                        break;
                    case 135:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.UpdateOTPOrderStatusRec);
                        }
                        break;
                    case 136:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderInProcessReportRequestRec);
                        }
                        break;
                    case 191:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SpreadOrderPlaceRequestRec);
                        }
                        break;
                    case 192:
                    case 193:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.SpreadOrderModifyCancelRequestRec);
                        }
                        break;
                    case 200:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MarginReportRequestRec);
                        }
                        break;
                    case 206:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MarginReportRequestRecMCX);
                        }
                        break;
                    case 208:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.MarginReportRequestReccombined);
                        }
                        break;
                    #region MTM 12-Sep-2016 - Rohan
                    case 203:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.UpdateMTMLimitHitReq);
                        }
                        break;
                    //case 208:
                    //    {
                    //        temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.UpdateMTMLimitHitReqMcx);
                    //    }
                    //    break;
                    case 205:   //Added on 19-Sep-2016 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ClientsForMTMReq);
                        }
                        break;
                    case 207:   //Added on 08-Mar-2017 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ClientsForMTMReqMcx);
                        }
                        break;
                    #endregion

                    #region Nse Curr Derive
                    case 211:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.CurrDerivOrderPlaceRequestRec);
                        }
                        break;
                    case 221:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.CurrDerivOrderModifyRequestRec);
                        }
                        break;
                    case 231:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.CurrDerivOrderCancelRequestRec);
                        }
                        break;
                    #endregion
                    case 251:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LinkFailRec);
                        }
                        break;
                    case 252:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LogoutRequest);
                        }
                        break;
                    case 247:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.LogoutRequest_2FA);
                        }
                        break;  
                    case 209:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TemplateMargin);
                        }
                        break;
                    case 172:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TemplateMargin_Peak);
                        }
                        break;
                    case 210://Derivative Margin 20.10.22
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TemplateMarginNew);
                        }
                        break;
                    case 173:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PeakMarginData);
                        }
                        break;
                    case 217:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.TemplateMargin);
                        }
                        break;
                    case 248:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.GetSegmentMaster);
                        }
                        break;
                    case 250:   //Added on 19-Sep-2016 - Rohan
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.ClientsForMTMReqInstrumentWise);
                        }
                        break;
                    //Option Exercise 03.08.17
                    case Constants.OTS_EXERCISE_PLACE_REQ:
                    case Constants.OTS_EXERCISE_MODIFY_REQ:
                    case Constants.OTS_EXERCISE_CANCEL_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OptionExerciseRequestRec);
                        }
                        break;
                    case 226:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.GetTradesReport);
                        }
                        break;
                    case Constants.OTS_OFS_WATCHLIST:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.OrderReportRequestRec);
                        }
                        break;
                    case Constants.OTS_OFS_PLACE_BROKER_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PlaceOfsOrderRequest);
                        }
                        break;
                    case Constants.OTS_OFS_MODIFY_BROKER_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PlaceOfsOrderRequest);
                        }
                        break;
                    case Constants.OTS_OFS_CANCEL_BROKER_REQ:
                        {
                            temp = CommonFunctions.RawSerialize(OTSReqRec.AllReq.PlaceOfsOrderRequest);
                        }
                        break;
                }
                RawWriter.Write(temp);
                RawWriter.Close();
                Raw.Close();
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void btn_DebugExRawFile_Click(object sender, EventArgs e)
        {
            try
            {
                BinaryReader RawReader;
                ITInteractiveStructures.TIARequestHeaderRecord Rwheader = new ITInteractiveStructures.TIARequestHeaderRecord();
                ITInteractiveStructures.TDerivOrderCancelRequestRecord DerivOrderCancelRequestRec = new ITInteractiveStructures.TDerivOrderCancelRequestRecord();
                byte[] BuffToRead = new byte[1024 * 10];

                if (!File.Exists(txt_ExRawFile.Text))
                {
                    MessageBox.Show(txt_ExRawFile.Text + " missing");
                    return;
                }
                FileStream RawFileStream;
                RawFileStream = new FileStream(txt_ExRawFile.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                RawReader = new BinaryReader(RawFileStream);
                RawReader.Read(BuffToRead, 0, Marshal.SizeOf(Rwheader));
                Rwheader = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TIARequestHeaderRecord>(BuffToRead);
                RawReader.Read(BuffToRead, 0, Rwheader.RequestLen);
                DerivOrderCancelRequestRec = CommonFunctions.ReadUsingMarshalUnsafe<ITInteractiveStructures.TDerivOrderCancelRequestRecord>(BuffToRead);
                // ITSRawReader.Read(BuffToRead, 0, ActualDataRcvdLen);
                RawReader.Close();
                RawFileStream.Close();
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        } 

        internal class Kernel32
        {
            [DllImport("Kernel32.dll", EntryPoint = "GetEnvironmentVariable", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetEnvironmentVariable(string lpName, StringBuilder lpBuffer, int nSize);

        }        
    
        public static string Version { get; set; }

        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        public static void WriteLog(IEnumerable<string> logLines, string filePrefix = "ServiceLog")
        {
            try
            {
                string logDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "logs"
                );

                Directory.CreateDirectory(logDirectory);

                string filePath = Path.Combine(
                    logDirectory,
                    $"{filePrefix}_{DateTime.Now:yyyyMMdd}.txt"
                );

                File.WriteAllLines(filePath, logLines);
            }
            catch
            {
                // Intentionally swallowed – logging must never crash the app
            }
        }
    }
}