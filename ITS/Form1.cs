using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using IntellectUtilities;
using System.Collections;
using System.Reflection;
using ICSharpCode.SharpZipLib.BZip2;
using System.Runtime.InteropServices;
using IntellectSharedClassLibrary;
using System.Threading.Tasks;
//using System.Diagnostics;

namespace ITS
{
    public partial class Form1 : Form
    {
        int TimerCnt, Reach = 0;
        int MaxConnections;
        int BadClientRejectCnt;
        int ErrorCnt;
        bool flAutoConnToOTS;
        bool flAllRepT2TAssigned;
        bool flAllRepBNMapAssigned;
        bool flAllRepBANDerivAssigned;
        bool flAllRepIlliqAssigned, flAllRepSegemntAssigned;
        ushort LoginCnt;
        string LocalMachineIP;
        public string Version;
        string ReportsFilePathName;
        string SessionFName;
        string iniPath;
        string IDayDataPath, InputFile;
        string IDayFileName_REQ;
        string IDayFileName_REP;
        string IDayErrorFileName;

        public const long NSE_Cash_Internet_UserInfo = 111111111111100;
        public const long NSE_Deriv_Internet_UserInfo = 111111111111100;
        public const long BSE_Cash_Internet_UserInfo = 1111111111111088;

        IBuffer SockBufOTSReply, Commpress;
        DateTime LastRcvdFromClientTime;
        DateTime LastSentToClientTime;
        DateTime LastRcvdFromOTSTime;
        DateTime LastSentToOTSTime;
        DataTable dt;
        Hashtable ClientsList;
        List<string> SL_ERROR;
        List<ushort> DisconnPosList;
        List<string> Changes;
        List<string> NewPointerCList;
        FileStream ITS_ErrorDatFileStream;
        FileStream ITS_REQ_DatFileStream;
        FileStream ITS_REP_DatFileStream;
        TIAReplyHeaderRecord LastHeader = new TIAReplyHeaderRecord();

        TAllReplyNewRecord[] AllRepT2T;
        TAllReplyNewRecord[] AllRepBseNseMap;
        TAllReplyNewRecord[] AllRepDerivBanScrip;
        TAllReplyNewRecord[] AllRepIlliquid;
        TAllReplyNewRecord[] AllRepSegment;
        TAllReplyNewRecord[] AllReplyArrayNew, AllReplyArrayMgn, AllReplyArrayMgnMCX, AllReplyArrayMgnInstrumentWise, AllReplyArrayOR, AllReplyArrayTR;

        public static object Log2FA_locker = new object();
        public static bool logEnable2FA = false;
        public bool SkipMessages = true;
        public bool AutoStartup = true;
        MCastSocketClass MCastIntellectHandler;

        ChangesLog CL;
        public List<string> Data = new List<string>();
        public List<TClientConnectionToITS> RetrySending = new List<TClientConnectionToITS>();
        //-----------        
        public static string Vname;
        public Form1()
        {
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

            InitializeComponent();

            AllReplyArrayNew = new TAllReplyNewRecord[0];
            AllReplyArrayMgn = new TAllReplyNewRecord[0];
            AllReplyArrayMgnMCX = new TAllReplyNewRecord[0];
            AllReplyArrayMgnInstrumentWise = new TAllReplyNewRecord[0];
        }

        public static void Log2FA(string Data)
        {
            lock (Log2FA_locker)
                File.AppendAllText($"2FA_Log_ITS_{DateTime.Today.ToString("ddMMyy")}.txt", Data + Environment.NewLine);
        }

        private void Form1_Load(object sender, EventArgs e)
        {  

            cmbServerType.SelectedIndex = 0;
            string DTF = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            if (DTF != "dd/MM/yyyy")
            {
                MessageBox.Show("Date Format should be in dd/MM/yyyy", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Version = "26.03.09";//"26.02.19";//"24.07.06.0";//23.03.14.1";//"23.01.31.0"; //"22.11.02.0";     
            this.Text += " - " + Version;
            LocalMachineIP = "";
            ErrorCnt = 0;
            BadClientRejectCnt = 0;
            LoginCnt = 0;
            TimerCnt = 0;
            flAutoConnToOTS = false;
            Vname = Version;

            DateTime Now = DateTime.Now;
            LastRcvdFromClientTime = Now;
            LastSentToClientTime = Now;
            LastRcvdFromOTSTime = Now;
            LastSentToOTSTime = Now;

            Changes = new List<string>();
            SL_ERROR = new List<string>();
            DisconnPosList = new List<ushort>();
            ClientsList = new Hashtable();
            NewPointerCList = new List<string>();
            CL = new ChangesLog();
            iniPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Settings\\"; // 26.6.14
            LoadUserPreferences();//24.6.14

            ReportsFilePathName = "Reports\\";
            IDayDataPath = "IntradayData\\";
            InputFile = "InputFiles\\";
            //iniPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Settings\\";
            SessionFName = "ITS_" + txtSettingsITSID.Text.Trim() + "_Session_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".Csv";//24.6.14
            IDayFileName_REQ = "ITS_REQ_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".dat";
            IDayFileName_REP = "ITS_REP_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".dat";
            IDayErrorFileName = "ITS_ERR_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".dat";

            if (checkBox2.Checked)
            {
                logEnable2FA = true;
            }
            //LoadUserPreferences();
            InitSocks();
            InitActivesTable();
            AddChanges();

            CL.listBox1.Items.AddRange(Changes.ToArray());

            File.WriteAllText(ReportsFilePathName + SessionFName, "SocNo,ConnectionID,UserID,ClientIP,ServerIP,ITSIP,ConnectionTime,DisconnectionTime, Version");
            ITS_ErrorDatFileStream = new FileStream(IDayDataPath + IDayErrorFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            ITS_REQ_DatFileStream = new FileStream(IDayDataPath + IDayFileName_REQ, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            ITS_REP_DatFileStream = new FileStream(IDayDataPath + IDayFileName_REP, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            timer1.Enabled = true;
            ReadClientsListFile();
            File.Create(InputFile + "ITSMessage.txt");

            MCastIntellectHandler = new MCastSocketClass(txtMulticastIP.Text, txtMulticastPort.Text, MCastIntellectHandlerDataIn, this);

            Task UITask = TaskEx.Delay(1000).ContinueWith(async (n) =>
            {
                int countDown = 11;
                lblMastersStatus.Visible = true;
                do
                {
                    lblMastersStatus.Text = $"Auto Startup in {--countDown}";
                    if (countDown == 0)
                    {
                        this.AutoStartup = false;
                        CheckBoxConnectToOfficeServer.Checked = true;
                        CheckBoxListenToClients.Checked = true;
                        //CheckBoxConnectToOfficeServer_CheckedChanged(sender, e);
                        //CheckBoxListenToClients_CheckedChanged(sender, e);
                        break;
                    }
                    await TaskEx.Delay(1000);
                } while (this.AutoStartup);
                lblMastersStatus.Visible = false;

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void showMessageBox(string Message, string Caption = "ITS", MessageBoxButtons Buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            if (SkipMessages)
            {
                listLogs.Items.AddRange(Message.Split('\n'));
            }
            else
            {
                MessageBox.Show(Message, Caption, Buttons, icon);
            }
        }


        public void MCastIntellectHandlerDataIn(byte[] dataToRead)
        {
            string Data = UTF8Encoding.UTF8.GetString(dataToRead);
            string[] lines = Data.Split('\n');
            foreach (var lineData in lines)
            {
                if (lineData == "") continue;
                string[] items = lineData.Split('|');
                if (items[1] == "ITS" && items[2] == txtMulticastID.Text)
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

        private void ReadClientsListFile()
        {
            string[] ClientsArray;
            NewPointerCList.Clear();
            if (!File.Exists(ReportsFilePathName + "NewPointerClientList.txt"))
            {
                MessageBox.Show("NewPointerClientList.txt is Not Present in Reports Folder.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ClientsArray = File.ReadAllLines(ReportsFilePathName + "NewPointerClientList.txt");
            for (int i = 0; i < ClientsArray.Length; i++)
            {
                NewPointerCList.Add(ClientsArray[i]);
            }
        }

        private void AddChanges()
        {
            string Str;

            Str = "13.12.18.0";
            Changes.Add(Str);
            Str = "             -> T2T,Bann and Illiquid scrips request's reply is handled locally.";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.5.20.0 - (Not working)";
            Changes.Add(Str);
            Str = "             -> For Currency, Added New Login Req Type = 3.";
            Changes.Add(Str);
            Str = "             -> GoDaddy SSL Certificate included, Expiry on 29 March 2015.";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.5.20.1 - (False working)";
            Changes.Add(Str);
            Str = "             -> SSL Certificate import error in prev exe, solved.";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.5.20.2";
            Changes.Add(Str);
            Str = "             -> Timer based disconnection modified exactly as per ver 13.12.18.0";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.5.21.0";
            Changes.Add(Str);
            Str = "             -> For Currency, Added New Lot Qty Req Type = 91.";
            Changes.Add(Str);
            Str = "             -> For Currency, Added New Order Place,Modify,Cancel Req Type = 211,221,231.";
            Changes.Add(Str);
            Str = "             -> For Currency, Added New Order Place,Modify,Cancel Rep Type = 212,222,232,233.";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.6.7.0";
            Changes.Add(Str);
            Str = "             -> Writing Version In User ConnList & Session File.";
            Changes.Add(Str);
            Str = "             -> New Check for Sending Msg to clients for New Pointer version exe.";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.6.9.0";
            Changes.Add(Str);
            Str = "             -> Updating Client list on Checking Send Particular Clients.";
            Changes.Add(Str);
            Str = "             -> Maintaing new ITS_Session file on every closing of ITS.";
            Changes.Add(Str);
            //Str = "             -> ";
            //Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.6.24.0";
            Changes.Add(Str);
            Str = "             -> Loading Ini before report file declarations & Session files naming as per ITS ID.";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.6.26.0";
            Changes.Add(Str);
            Str = "             -> iniPath Setting For Loading Ini,since Ini was not Loading.On Startup,checking for System DateTime format0 ";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "14.07.07.0";
            Changes.Add(Str);
            Str = "             -> Added new button for Sending message to connected pointers(Send Message To All). New file is created for popup messages - ITSMessage.txt";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");


            Str = "14.07.10.0";
            Changes.Add(Str);
            Str = "             -> New Combobox added to seperate IBT and CTCL clients.Checking added in login reply . wheather client is allowed to login on IBT/CTCL server, according to that disconnection is handeled for the same. ";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");

            Str = "15.05.20.0";
            Changes.Add(Str);
            Str = "             -> Member BOD Var in Holdings & New Security Info. ";
            Changes.Add(Str);

            Str = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
            Changes.Add(Str);
            Changes.Add("");



        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult Result = MessageBox.Show("Are you sure you want to exit this application", "Exit Application?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (Result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (ITS_ErrorDatFileStream != null)
            {
                ITS_ErrorDatFileStream.Flush();
                ITS_ErrorDatFileStream.Close();
            }
            if (ITS_REQ_DatFileStream != null)
            {
                ITS_REQ_DatFileStream.Flush();
                ITS_REQ_DatFileStream.Close();
            }
            if (Data.Count > 0)
            {
                File.WriteAllLines(ReportsFilePathName + "Missingclientsdata.CSV", Data.ToArray());
            }
            SaveUserPreferences();

            e.Cancel = false;
        }

        private void SaveUserPreferences()
        {
            IniFile ini = new IniFile(iniPath + "ITSC#.ini");
            //IniFile ini = new IniFile(Path.GetDirectoryName(Application.ExecutablePath) + "\\ITSC#.ini");
            ini.IniWriteValue("General Info", "OTS IP", txtSettingsOTSIP.Text);
            ini.IniWriteValue("General Info", "Trade Version", Txtbox_Trade_Version.Text);
            ini.IniWriteValue("General Info", "OldVerMsg", Txtbox_OldVerMsg.Text);
            ini.IniWriteValue("General Info", "ITS ID", txtSettingsITSID.Text);

            ini.IniWriteValue("Settings", "GroupIP", txtMulticastIP.Text);
            ini.IniWriteValue("Settings", "GroupPort", txtMulticastPort.Text);
            ini.IniWriteValue("Settings", "GroupID", txtMulticastID.Text);

            if (chkbox_AllowTradeVersion.Checked)
                ini.IniWriteValue("General Info", "AllowTradeVersion", "true");
            else
                ini.IniWriteValue("General Info", "AllowTradeVersion", "false");

            if (chkbox_SendLogoutsToOTS.Checked)
                ini.IniWriteValue("General Info", "SendLogouts", "true");
            else
                ini.IniWriteValue("General Info", "SendLogouts", "false");

            ini.IniWriteValue("General Info", "MaxConnections", txtMaxClients.Text);

            if (chk_SendAdminExtReqs.Checked)
                ini.IniWriteValue("General Info", "ChkIBTCTCL", "true");
            else
                ini.IniWriteValue("General Info", "ChkIBTCTCL", "false");

            ini.IniWriteValue("General Info", "ComboIBTCTCL", Convert.ToString(cmbServerType.SelectedIndex));
        }



        private void LoadUserPreferences()
        {
            if (!File.Exists(iniPath + "ITSC#.ini")) return;

            try
            {
                IniFile ini = new IniFile(iniPath + "ITSC#.ini");
                //IniFile ini = new IniFile(Path.GetDirectoryName(Application.ExecutablePath) + "\\ITSC#.ini");
                txtSettingsOTSIP.Text = ini.IniReadValue("General Info", "OTS IP");
                Txtbox_Trade_Version.Text = ini.IniReadValue("General Info", "Trade Version");
                Txtbox_OldVerMsg.Text = ini.IniReadValue("General Info", "OldVerMsg");
                txtSettingsITSID.Text = ini.IniReadValue("General Info", "ITS ID");
                chkbox_AllowTradeVersion.Checked = Convert.ToBoolean(ini.IniReadValue("General Info", "AllowTradeVersion"));
                chkbox_SendLogoutsToOTS.Checked = Convert.ToBoolean(ini.IniReadValue("General Info", "SendLogouts"));
                txtMaxClients.Text = ini.IniReadValue("General Info", "MaxConnections");
                chk_SendAdminExtReqs.Checked = Convert.ToBoolean(ini.IniReadValue("General Info", "ChkIBTCTCL"));
                cmbServerType.SelectedIndex = Convert.ToInt32(ini.IniReadValue("General Info", "ComboIBTCTCL"));       //, Convert.ToString(cmbServerType.SelectedIndex));

                string IP = ini.IniReadValue("Settings", "GroupIP");
                int Port = Convert.ToInt32(ini.IniReadValue("Settings", "GroupPort"));
                int ID = Convert.ToInt32(ini.IniReadValue("Settings", "GroupID"));

                txtMulticastIP.Text = IP;
                txtMulticastPort.Text = Port.ToString();
                txtMulticastID.Text = ID.ToString();
            }
            catch (Exception ex)
            {
                Program.ErrorLog(ex);
            }
        }

        private void InitSocks()
        {
            OTSSocket.RuntimeLicense = "31534E3941413153554252413153554239364339333633370000000000000000000000000000000048434431333154350000413944343953594B57474E540000";
            OTSSocket.InvokeThrough = this;
            OTSSocket.Config("OutBufferSize=5000000");
            OTSSocket.Config("INBufferSize=5000000");
            SockBufOTSReply = new IBuffer(102400000);

            SSLServer.RuntimeLicense = "31534E3941413153554252413153554239364339333633370000000000000000000000000000000048434431333154350000413944343953594B57474E540000";
            SSLServer.InvokeThrough = this;
            SSLServer.LocalPort = ConstantUnits.PORT_ITS;
            SSLServer.Config("OutBufferSize=5000000");
        }

        private void InitActivesTable()
        {
            dt = new DataTable();

            dt.Columns.Add("Sr", typeof(int));
            dt.Columns.Add("SocNum", typeof(int));
            dt.Columns.Add("IP", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Type", typeof(int));
            dt.Columns.Add("ConnectTime", typeof(string));
            dt.Columns.Add("Version", typeof(string));
            dt.Columns.Add("ConId", typeof(string));
            dt.Columns.Add("Clients", typeof(int));
            dt.Columns.Add("BuffUsed", typeof(int));

            dataGridView1.DataSource = dt;

            dataGridView1.Columns[0].Width = 25;
            dataGridView1.Columns[1].Width = 25;
            dataGridView1.Columns[2].Width = 50;
            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns[4].Width = 25;
            dataGridView1.Columns[5].Width = 50;
            dataGridView1.Columns[6].Width = 30;
            dataGridView1.Columns[7].Width = 50;
            dataGridView1.Columns[8].Width = 25;
            dataGridView1.Columns[9].Width = 30;
        }

        private void CheckBoxConnectToOfficeServer_Click(object sender, EventArgs e)
        {
            AutoStartup = false;
            if (CheckBoxConnectToOfficeServer.Checked)
            {
                //06/06/2012
                //if SockBufOTSReply.GetBufUsed > 0 then
                //   begin
                //   fbname := 'ITS_Buf_' + FormatDateTime('yyyymmdd_hhmmss',now) + '.dat' ;
                //   assignfile(fb,fbname) ;
                //   rewrite(fb) ;
                //   for i := 1 to SockBufOTSReply.GetBufUsed do
                //      begin
                //      SockBufOTSReply.Read(@b,1) ;
                //      Write(fb,b) ;
                //      end;
                //   closefile(fb) ;
                //   end;

                flAutoConnToOTS = true;
                SockBufOTSReply.Clear();
                OTSSocket.Connect(txtSettingsOTSIP.Text, ConstantUnits.PORT_OTS);
            }
            else
            {
                byte[] bytes = new byte[SockBufOTSReply.BufUsed];
                SockBufOTSReply.Read(bytes, SockBufOTSReply.BufUsed);
                File.WriteAllBytes($"ITSJunk_{DateTime.Now:ddMMyy_HHmmss}.dat", bytes);
                File.WriteAllBytes($"ITSJunk_{DateTime.Now:ddMMyy_HHmmss}_H.dat", LastHeader.ToByteArray());
                OTSSocket.Disconnect();
            }
        }

        private void CheckBoxListenToClients_Click(object sender, EventArgs e)
        {
            if (CheckBoxListenToClients.Checked)
            {
                txtMaxClients.Enabled = false;
                MaxConnections = Convert.ToInt32(txtMaxClients.Text);
                //SSLServer.Config("MAXCONNECTIONS = " + MaxConnections);
                SSLServer.Listening = true;
                Statuslabel_ClientsConnection.Image = pictureBoxGreen.Image;
            }
            else
            {
                txtMaxClients.Enabled = true;
                SSLServer.Listening = false;
                Statuslabel_ClientsConnection.Image = pictureBoxRed.Image;
            }
        }


        private void btnSettingsSendSqOffMsg_Click(object sender, EventArgs e)
        {
            List<byte> bToSend;
            TIAReplyHeaderRecord IAHdr2;
            TMessageStringRecord MsgRec;
            TClientConnectionToITS Client;
            ICollection<nsoftware.IPWorksSSL.Connection> ValCollection;

            DialogResult Result = MessageBox.Show("Do you want to send the message now?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (Result == DialogResult.No) return;

            bToSend = new List<byte>();
            IAHdr2.ReplyType = ConstantUnits.REP_ITS_TO_FE_MESSAGE;
            IAHdr2.ReplyLen = ConstantUnits.TMessageStringRecordSize;
            IAHdr2.ReplyRecSize = ConstantUnits.TMessageStringRecordSize;
            IAHdr2.ReplyCnt = 1;
            IAHdr2.ErrorCode = 0;
            IAHdr2.ChannelID_NC = 0;
            IAHdr2.ChannelID_ND = 0;
            IAHdr2.ChannelID_BC = 0;
            IAHdr2.AccountType = 0;
            IAHdr2.ClientType = 1;
            IAHdr2.TraderID = ("ITS").PadRight(14).ToCharArray();
            IAHdr2.TraderLength = 3;
            IAHdr2.FEUserID = ("ITS").PadRight(15).ToCharArray();
            IAHdr2.FELength = 3;
            IAHdr2.SocketNum = 0;
            bToSend.AddRange(IAHdr2.ToByteArray());

            string MSG = "This is a reminder to square off your intra-day short sale positions, if any, in CASH segment."
                       + "\n"
                       + "Please do so before close of market to avoid loss due to auction/close out.";

            MsgRec.MsgStr = MSG.PadRight(250).ToCharArray();
            MsgRec.MsgStrLength = (byte)MSG.Length;
            bToSend.AddRange(MsgRec.ToByteArray());

            //SendToSocket(client,IAHdr2, MsgArr, null);
            ValCollection = SSLServer.Connections.Values;
            foreach (nsoftware.IPWorksSSL.Connection TClient in ValCollection)
            {
                if (!TClient.Connected) continue;
                Client = ClientsList[TClient.ConnectionId] as TClientConnectionToITS;
                if (Client == null)
                {
                    ErrorCnt++;
                    SL_ERROR.Add("SQF MSG,Conid unavailable," + TClient.ConnectionId);
                    continue;
                }

                if (Client.FEUserType != 1) continue;

                LastSentToClientTime = DateTime.Now;
                TClient.DataToSendB = bToSend.ToArray();
            }
        }

        private void btnActivesUpdate_Click(object sender, EventArgs e) 
        {
            int Sr, Type, ClntCnt, BufUsed, Soc;
            string IP, Name, ConnTime, Version, Connid;

            dt.Clear();
            Sr = 0;
            foreach (TClientConnectionToITS Client in ClientsList.Values)
            {
                if (Client.FEUserType == 0 || Client.FEUserType == 46) continue;

                Sr++;
                Soc = Client.SockNum;
                Type = Client.FEUserType;
                ClntCnt = Client.ClientCnt;
                IP = Client.ServerIP;
                if (Type == 1)
                    Name = Client.TraderID;
                else
                    Name = Client.FEUserID;
                ConnTime = Client.SockCreateTime.ToString("dd/MM/yyyy hh:mm:ss");
                Version = Client.Version;
                Connid = Client.ConnectionID.ToString();
                BufUsed = Client.Buff.BufUsed;

                dt.Rows.Add(new object[]
                {
                    Sr,
                    Soc,
                    IP,
                    Name,
                    Type,
                    ConnTime,
                    Version,
                    Connid,
                    ClntCnt,
                    BufUsed
                });
            }
        }

        private void btnActivitiesSave2File_Click(object sender, EventArgs e)
        {
            List<string> SL = new List<string>();

            SL.Add("SockNo,ConnID,ConnTime,UserID,UserType,ClientIP,ServerIP,ITS_IP,Version");
            foreach (TClientConnectionToITS Client in ClientsList.Values)
            {
                SL.Add(Client.SockNum + ","
                     + Client.ConnectionID + ","
                     + Client.SockCreateTime + ","
                     + Client.FEUserID + ","
                     + Client.FEUserType + ","
                     + Client.ClientIP + ","
                     + Client.ServerIP + ","
                     + Client.ITS_IP + ","
                     + Client.Version);//7.6.14 S
            }
            File.WriteAllLines(ReportsFilePathName + txtActivitiesFileName.Text, SL);
            MessageBox.Show("User List Created " + txtActivitiesFileName.Text);
        }

        private void btnActivesDisconn_Click(object sender, EventArgs e)
        {
            string id;
            ArrayList List;
            TClientConnectionToITS Client;

            if (string.IsNullOrEmpty(txtActivesId.Text.Trim())) return;

            id = txtActivesId.Text.Trim();
            List = new ArrayList(ClientsList.Values);

            for (int i = 0; i < List.Count; i++)
            {
                Client = List[i] as TClientConnectionToITS;
                if (Client.FEUserType <= 0) continue;
                if (Client.FEUserID != id) continue;
                if (!SSLServer.Connections.ContainsKey(Client.ConnectionID)) continue;

                txtActivesId.Text = "TRADER";
                SSLServer.Connections[Client.ConnectionID].Connected = false;
                break;
            }
        }

        private void btnDisplayClear_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void btnCtrlProcessFile_Click(object sender, EventArgs e)
        {
            //string[] data = File.ReadAllLines(IDayDataPath + txtCtrlFName.Text);

            //TIARequestHeaderRecord IAHdr;
            //TIAErrorMessageRecord IAError;
            //TAllReplyNewRecord[] AllReplyArray;
            //TGlobalHeader GHdr;
            //List<string> list = new List<string>();

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

            //    IAHdr = fs.ReadStruct<TIARequestHeaderRecord>();

            //    list.Add("Header - " + IAHdr.RequestType);
            //    list.Add("-------------------------");
            //    list.Add("AccountType-" + IAHdr.AccountType);
            //    list.Add("ChannelID_BC-" + IAHdr.ChannelID_BC);
            //    list.Add("ChannelID_NC-" + IAHdr.ChannelID_NC);
            //    list.Add("ChannelID_ND-" + IAHdr.ChannelID_ND);
            //    list.Add("ClientType-" + IAHdr.ClientType);
            //    //list.Add("ErrorCode-" + IAHdr.ErrorCode);
            //    list.Add("FELength-" + IAHdr.FELength);
            //    list.Add("FEUserID-" + new string(IAHdr.FEUserID, 0, IAHdr.FELength));
            //    //list.Add("ReplyCnt-" + IAHdr.ReplyCnt);
            //    list.Add("RepqLen-" + IAHdr.RequestLen);
            //    //list.Add("ReplyRecSize-" + IAHdr.ReplyRecSize);
            //    list.Add("ReqType-" + IAHdr.RequestType);
            //    list.Add("SocketNum-" + IAHdr.SocketNum);
            //    list.Add("TraderID-" + new string(IAHdr.TraderID, 0, IAHdr.TraderLength));
            //    list.Add("TraderLength-" + IAHdr.TraderLength);

            //    b=new byte[(GHdr.Size - 62)];
            //    fs.Read(b, 0, b.Length);
            //}
            //fs.Close();
            //File.WriteAllLines(ReportsFilePathName + Fname + ".txt", list);
            //MessageBox.Show("Done.");

            //TIAReplyHeaderRecord IAHdr;
            //TIAErrorMessageRecord IAError;
            //TAllReplyNewRecord[] AllReplyArray;
            //TGlobalHeader GHdr;
            //List<string> list = new List<string>();

            //byte[] b;
            //string Fname = txtCtrlFName.Text;            

            //FileStream fs = new FileStream(IDayDataPath + Fname, FileMode.Open, FileAccess.Read, FileShare.Read);
            //AllReplyArray = new TAllReplyNewRecord[0];
            //while (fs.Position < fs.Length)
            //{
            //    GHdr = fs.ReadStruct<TGlobalHeader>();

            //    list.Add("");
            //    list.Add("");
            //    list.Add("--------Time- "+GHdr.DateTime.ToString("hh:mm:ss:fff")+ ", Flow- ST"+(char)GHdr.SentFrom+"----------------------------------------------------------------------------------------");

            //    IAHdr = fs.ReadStruct<TIAReplyHeaderRecord>();

            //    list.Add("Header - " + IAHdr.ReplyType);
            //    list.Add("-------------------------");
            //    list.Add("AccountType-" + IAHdr.AccountType);
            //    list.Add("ChannelID_BC-" + IAHdr.ChannelID_BC);
            //    list.Add("ChannelID_NC-" + IAHdr.ChannelID_NC);
            //    list.Add("ChannelID_ND-" + IAHdr.ChannelID_ND);
            //    list.Add("ClientType-" + IAHdr.ClientType);
            //    list.Add("ErrorCode-" + IAHdr.ErrorCode);
            //    list.Add("FELength-" + IAHdr.FELength);
            //    list.Add("FEUserID-" + new string(IAHdr.FEUserID, 0, IAHdr.FELength));
            //    list.Add("ReplyCnt-" + IAHdr.ReplyCnt);
            //    list.Add("ReplyLen-" + IAHdr.ReplyLen);
            //    list.Add("ReplyRecSize-" + IAHdr.ReplyRecSize);
            //    list.Add("ReplyType-" + IAHdr.ReplyType);
            //    list.Add("SocketNum-" + IAHdr.SocketNum);
            //    list.Add("TraderID-" + new string(IAHdr.TraderID, 0, IAHdr.TraderLength));
            //    list.Add("TraderLength-" + IAHdr.TraderLength);

            //    if (IAHdr.ErrorCode > 0)
            //    {
            //        IAError = fs.ReadStruct<TIAErrorMessageRecord>();

            //        list.Add("");
            //        list.Add("Error - " + IAHdr.ErrorCode);
            //        list.Add("-------------------------");
            //        list.Add("Errorlength-" + IAError.Errorlength);
            //        list.Add("ErrorStr-" + new string(IAError.ErrorStr).Trim());
            //    }

            //    if (IAHdr.ReplyLen > 0)
            //    {
            //        b = new byte[IAHdr.ReplyLen];
            //        fs.Read(b, 0, IAHdr.ReplyLen);

            //        list.Add("");
            //        list.Add("Body - " + IAHdr.ReplyLen);
            //        list.Add("-------------------------");
            //        list.Add(Encoding.ASCII.GetString(b));
            //        //Array.Resize(ref AllReplyArray, IAHdr.ReplyCnt);

            //        //for (int i = 0; i < IAHdr.ReplyCnt; i++)
            //        //{
            //        //    AllReplyArray[i] = b.ToAllReplyStruct(IAHdr.ReplyType);
            //        //}
            //    }
            //}
            //fs.Close();
            //File.WriteAllLines(ReportsFilePathName + Fname + ".txt", list);
            //MessageBox.Show("Done.");
        }

        private void btnCtrlErrorRpt_Click(object sender, EventArgs e)
        {
            if (SL_ERROR.Count > 0)
            {
                File.WriteAllLines(ReportsFilePathName + "ITS_Error.txt", SL_ERROR);
                SL_ERROR.Clear();
            }
            MessageBox.Show("Done.");
        }
        //-----------

        private void OTSSocket_OnConnected(object sender, nsoftware.IPWorksSSL.IpportsConnectedEventArgs e)
        {
            listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + ":- OTS SOCKET CONNECTED!");
            TIARequestHeaderRecord IAReqHdr;
            TDummyRecord DummyRec;
            List<byte> bToSend;

            if (e.StatusCode != 0)
            {
                Statuslabel_OTSConnection.Image = pictureBoxRed.Image;
                return;
            }

            flAllRepT2TAssigned = false;
            flAllRepBNMapAssigned = false;
            flAllRepBANDerivAssigned = false;
            flAllRepIlliqAssigned = false;

            Statuslabel_OTSConnection.Image = pictureBoxGreen.Image;
            CheckBoxConnectToOfficeServer.Checked = true;

            LocalMachineIP = OTSSocket.LocalHost;
            //On Connection, Send Request 245 with ITS ID. This will help in dup disconnec in msg 9

            bToSend = new List<byte>();

            IAReqHdr.AccountType = 0;
            IAReqHdr.ChannelID_BC = 0;
            IAReqHdr.ChannelID_NC = 0;
            IAReqHdr.ChannelID_ND = 0;
            IAReqHdr.ClientType = 0;
            IAReqHdr.SocketNum = 1;
            IAReqHdr.TraderLength = 3;
            IAReqHdr.TraderID = "ITS".PadRight(14).ToCharArray();
            IAReqHdr.FELength = 3;
            IAReqHdr.FEUserID = "ITS".PadRight(15).ToCharArray();
            IAReqHdr.ClientType = 0;
            IAReqHdr.RequestLen = ConstantUnits.TDummyRecordSize;

            DummyRec.DummyByte = Convert.ToByte(txtSettingsITSID.Text);
            DummyRec.DummyLength = (byte)LocalMachineIP.Length;
            DummyRec.Dummy = LocalMachineIP.PadRight(15).ToCharArray();
            DummyRec.DummyInt = 0;

            IAReqHdr.RequestType = ConstantUnits.REQ_ITS_TO_OTS_CONN;
            bToSend.AddRange(IAReqHdr.ToByteArray());
            bToSend.AddRange(DummyRec.ToByteArray());

            // Request for Illiquid Scrips, Deriv Ban Scrips, Bse Nse Map, T2T Scrips
            //-----------------------------------------------------------------------

            IAReqHdr.RequestType = ConstantUnits.REQ_ITS_TO_OTS_T2T_SCRIPS;
            bToSend.AddRange(IAReqHdr.ToByteArray());
            bToSend.AddRange(DummyRec.ToByteArray());

            IAReqHdr.RequestType = ConstantUnits.REQ_ITS_TO_OTS_BSE_NSE_MAP;
            bToSend.AddRange(IAReqHdr.ToByteArray());
            bToSend.AddRange(DummyRec.ToByteArray());

            IAReqHdr.RequestType = ConstantUnits.REQ_ITS_TO_OTS_BANNED_SCRIPS_DERIV;
            bToSend.AddRange(IAReqHdr.ToByteArray());
            bToSend.AddRange(DummyRec.ToByteArray());

            IAReqHdr.RequestType = ConstantUnits.REQ_ITS_TO_OTS_ILLIQUID_SCRIPS;
            bToSend.AddRange(IAReqHdr.ToByteArray());
            bToSend.AddRange(DummyRec.ToByteArray());

            IAReqHdr.RequestType = 248;
            bToSend.AddRange(IAReqHdr.ToByteArray());
            bToSend.AddRange(DummyRec.ToByteArray());
            //-----------------------------------------------------------------------

            if (OTSSocket.Connected)
                OTSSocket.Send(bToSend.ToArray());
        }

        private void OTSSocket_OnDisconnected(object sender, nsoftware.IPWorksSSL.IpportsDisconnectedEventArgs e)
        {
            listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + ":- OTS SOCKET DISCONNECTED!");
            Statuslabel_OTSConnection.Image = pictureBoxRed.Image;
            CheckBoxConnectToOfficeServer.Checked = false;
        }

        private void OTSSocket_OnDataIn(object sender, nsoftware.IPWorksSSL.IpportsDataInEventArgs e)
        {
            SockBufOTSReply.WriteAndExtend(e.TextB, e.TextB.Length);
            LastRcvdFromOTSTime = DateTime.Now;
            ProcessOTSReply();
        }

        private void ProcessOTSReply()
         {
            int ReplySize;
            byte[] buffToRead,  Compress = null, Decompress, Compress1 = null;
            Commpress = new IBuffer(0);

            TIAReplyHeaderRecord IAHdr;
            TIAErrorMessageRecord IAError;
            TAllReplyNewRecord[] AllReplyArray;
            TClientConnectionToITS Client;
            ICollection<nsoftware.IPWorksSSL.Connection> ValCollection;

            IAError = new TIAErrorMessageRecord();
            AllReplyArray = new TAllReplyNewRecord[0];
            buffToRead = new byte[SockBufOTSReply.BufUsed];
            while (SockBufOTSReply.BufUsed >= ConstantUnits.TIAReplyHeaderRecordSize)
            {
                SockBufOTSReply.Peek(buffToRead, ConstantUnits.TIAReplyHeaderRecordSize);
                IAHdr = buffToRead.ToStructure<TIAReplyHeaderRecord>();

                #region COMMENT for junk

                //06/06/2012 as we are removing bytes to handle buffer accumulation, we need additional checks
                //if IAHdr.ReplyRecSize * IAHdr.ReplyCnt <> IAHdr.ReplyLen then
                //   begin
                //   ListBox1.Items.insert(0,'Bad ReplyLen, may be due to removal 1') ;
                //   With IAHdr do
                //      begin
                //      Str1 := Format('Bad %d,%d,%s,%d,%d,%d,%d,%d',[OkCnt,SocketNum,FEUserID,ReplyType,ReplyLen,ReplyRecSize,ReplyCnt,ErrorCode]) ;
                //      ListBox1.Items.Insert(0,Str1); ;
                //      end ;
                //   break ;
                //   end;

                //if (IAHdr.ReplyCnt > 0) and (IAHdr.ReplyLen <= 0) then
                //   begin
                //   ListBox1.Items.insert(0,'Bad ReplyLen, may be due to removal 2' ) ;
                //   With IAHdr do
                //      begin
                //      Str1 := Format('Bad %d,%d,%s,%d,%d,%d,%d,%d',[OkCnt,SocketNum,FEUserID,ReplyType,ReplyLen,ReplyRecSize,ReplyCnt,ErrorCode]) ;
                //      ListBox1.Items.Insert(0,Str1); ;
                //      end ;
                //   break ;
                //   end;

                //if (IAHdr.SocketNum < 0) or (IAHdr.SocketNum > MaxClientConnections) then
                //   begin
                //    With IAHdr do
                //      begin
                //      Str1 := Format('Bad %d,%d,%s,%d,%d,%d,%d,%d',[OkCnt,SocketNum,FEUserID,ReplyType,ReplyLen,ReplyRecSize,ReplyCnt,ErrorCode]) ;
                //      ListBox1.Items.Insert(0,Str1); ;
                //      end ;
                //  break ;
                //   end;
                //   if OkCnt=1624    then
                //   begin
                //                  showmessage('hello');
                //   end;


                //if Not IsValidCode(IAHdr.ReplyType) then
                //   begin
                //   ListBox1.Items.insert(0,'Bad ReplyCode = ' + inttostr(IAHdr.ReplyType)) ;
                //    With IAHdr do
                //      begin
                //      Str1 := Format('Bad %d,%d,%s,%d,%d,%d,%d,%d',[OkCnt,SocketNum,FEUserID,ReplyType,ReplyLen,ReplyRecSize,ReplyCnt,ErrorCode]) ;
                //      ListBox1.Items.Insert(0,Str1); ;
                //      end ;
                //   break ;
                //   end ;

                #endregion

                ReplySize = ConstantUnits.TIAReplyHeaderRecordSize + IAHdr.ReplyLen;
                if (IAHdr.ErrorCode > 0) ReplySize = ReplySize + ConstantUnits.TIAErrorMessageRecordSize;
                if (SockBufOTSReply.BufUsed < ReplySize)
                {
                    {
                        #region Check if Junk Header
                        bool junkFound = false;
                        if (IAHdr.TraderLength == 1)//Compressed Data
                        {
                            switch (IAHdr.ReplyType)
                            {
                                case 5:
                                case 7:
                                case 8:
                                case 250:
                                case 207:
                                case 141:
                                case 142:
                                    break;
                                default:
                                    junkFound = true;
                                    break;

                            }
                            if (IAHdr.ReplyType == 0)
                                junkFound = true;
                        }
                        else
                        {
                            if (IAHdr.ReplyLen > 0)
                            {

                                if (!(IAHdr.ReplyLen == IAHdr.ReplyCnt * IAHdr.ReplyRecSize))
                                    junkFound = true;
                            }
                            else
                            {
                                if (IAHdr.ReplyType == 0)
                                    junkFound = true;
                                if (IAHdr.ReplyCnt == 0 && IAHdr.ReplyRecSize == 0)
                                {
                                    if ((IAHdr.FELength <= 15) && (IAHdr.TraderLength <= 14))
                                    {
                                        if (IAHdr.TraderLength > 0)
                                        {
                                            if (new string(IAHdr.TraderID).Trim().Length != IAHdr.TraderLength)
                                                junkFound = true;
                                        }
                                        else if (IAHdr.FELength > 0)
                                        {
                                            if (new string(IAHdr.FEUserID).Trim().Length != IAHdr.FELength)
                                                junkFound = true;
                                        }
                                    }
                                    else
                                        junkFound = true;
                                }
                                else
                                    junkFound = true;
                            }

                        }


                        #endregion
                        #region Clear Junk Buffer
                        if (junkFound)
                        {
                            byte[] bytes = new byte[SockBufOTSReply.BufUsed];
                            SockBufOTSReply.Read(bytes, SockBufOTSReply.BufUsed);
                            File.WriteAllBytes($"ITSJunk_{DateTime.Now:ddMMyy_HHmmss}.dat", bytes);
                            File.WriteAllBytes($"ITSJunk_{DateTime.Now:ddMMyy_HHmmss}_H.dat", LastHeader.ToByteArray());

                            SockBufOTSReply.Clear();
                        }
                        #endregion
                        return;
                    }
                }

                //if (SockBufOTSReply.BufUsed < ReplySize) return;
                SockBufOTSReply.Read(buffToRead, ConstantUnits.TIAReplyHeaderRecordSize);
                LastHeader = IAHdr;
                if (Chkbox_WriteOTSRecv.Checked)
                {
                    string TraderId1 = new string(IAHdr.TraderID, 0, IAHdr.TraderLength);
                    if (TraderId1 == txtDisplayClid.Text.Trim())
                    {
                        string str1 = DateTime.Now.ToString("hh:mm:ss") + " - RFO - Rep type: " + IAHdr.ReplyType + ", name:" + TraderId1 + ", Sock:" + IAHdr.SocketNum;
                        listBox1.Items.Insert(0, str1);
                    }
                }

                if (IAHdr.ReplyType == 85)
                    Log2FA(IAHdr.toJsonString());

                if (IAHdr.ErrorCode > 0)
                {
                    SockBufOTSReply.Read(buffToRead, ConstantUnits.TIAErrorMessageRecordSize);
                    IAError = buffToRead.ToStructure<TIAErrorMessageRecord>();

                    if (IAHdr.ReplyType == 85)
                        if (logEnable2FA)
                            Log2FA(IAError.toJsonString());
                }

                if (IAHdr.ReplyCnt > 0)
                {
                    if (IAHdr.ReplyType == 5 && IAHdr.TraderLength == 1)  //Changes done on 27-Mar-2017 - Rohan
                    {
                        try
                        {
                            Compress = new byte[IAHdr.ReplyLen];
                            SockBufOTSReply.Read(Compress, IAHdr.ReplyLen);
                            Decompress = DataCompression(Compress);
                            Commpress.WriteAndExtend(Decompress, Decompress.Length);
                        }
                        catch (Exception E)
                        {
                            SockBufOTSReply.Clear();
                            File.AppendAllText("Decompress_" + DateTime.Now.ToString("ddMMyyyy") + ".txt", DateTime.Now.ToString() + ": " + IAHdr.ReplyType);
                        }
                    }
                    else if (IAHdr.ReplyType == 7 && IAHdr.TraderLength == 1)  //Changes done on 27-Mar-2017 - Rohan
                    {
                        try
                        {
                            Compress = new byte[IAHdr.ReplyLen];
                            SockBufOTSReply.Read(Compress, IAHdr.ReplyLen);
                            Decompress = DataCompression(Compress);
                            Commpress.WriteAndExtend(Decompress, Decompress.Length);
                        }
                        catch (Exception E)
                        {
                            SockBufOTSReply.Clear();
                            File.AppendAllText("Decompress_" + DateTime.Now.ToString("ddMMyyyy") + ".txt", DateTime.Now.ToString() + ": " + IAHdr.ReplyType);
                        }
                    }
                    else if (IAHdr.ReplyType == 8 && IAHdr.TraderLength == 1)  //Changes done on 27-Mar-2017 - Rohan
                    {
                        try
                        {
                            Compress = new byte[IAHdr.ReplyLen];
                            SockBufOTSReply.Read(Compress, IAHdr.ReplyLen);
                            Decompress = DataCompression(Compress);
                            Commpress.WriteAndExtend(Decompress, Decompress.Length);
                        }
                        catch (Exception E)
                        {
                            SockBufOTSReply.Clear();
                            File.AppendAllText("Decompress_" + DateTime.Now.ToString("ddMMyyyy") + ".txt", DateTime.Now.ToString() + ": " + IAHdr.ReplyType);
                        }
                    }
                    else if (IAHdr.ReplyType == 250 && IAHdr.TraderLength == 1)
                    {
                        try
                        {
                            //Compress = new byte[IAHdr.ReplyLen];
                            //if (Compress.Length < IAHdr.ReplyRecSize)
                            //    // Compress = Decompress;
                            //    Compress = new byte[IAHdr.ReplyRecSize];
                            ////Compress1 = new byte[360 - IAHdr.ReplyLen];
                            ////Array.Copy(Compress, 0, Compress1, Compress.Length + 1, IAHdr.ReplyRecSize);
                            //SockBufOTSReply.Read(Compress, IAHdr.ReplyLen);
                            //Decompress = DataCompression(Compress);
                            //Commpress.WriteAndExtend(Decompress, Decompress.Length);


                            Compress = new byte[IAHdr.ReplyLen];
                            SockBufOTSReply.Read(Compress, IAHdr.ReplyLen);
                            Decompress = DataCompression(Compress);
                            Commpress.WriteAndExtend(Decompress, Decompress.Length);

                        }
                        catch (Exception E)
                        {
                            SockBufOTSReply.Clear();
                            File.AppendAllText("Decompress_" + DateTime.Now.ToString("ddMMyyyy") + ".txt", DateTime.Now.ToString() + ": " + IAHdr.ReplyType);
                        }
                    }
                    else if (( IAHdr.ReplyType == 207 ) && IAHdr.TraderLength == 1)  //Changes done on 03-Jun-2017 - Rohan
                    {
                        try
                        {
                            Compress = new byte[IAHdr.ReplyLen];
                            SockBufOTSReply.Read(Compress, IAHdr.ReplyLen);
                            Decompress = DataCompression(Compress);
                            Commpress.WriteAndExtend(Decompress, Decompress.Length);
                        }
                        catch (Exception E)
                        {
                            SockBufOTSReply.Clear();
                            File.AppendAllText("Decompress_" + DateTime.Now.ToString("ddMMyyyy") + ".txt", DateTime.Now.ToString() + ": " + IAHdr.ReplyType);
                        }
                    }
                    else if ((IAHdr.ReplyType == 141 || IAHdr.ReplyType == 142) && IAHdr.TraderLength == 1)  //Changes done on 05-Apr-2018 - Rohan
                    {
                        try
                        {
                            Compress = new byte[IAHdr.ReplyLen];
                            SockBufOTSReply.Read(Compress, IAHdr.ReplyLen);
                            Decompress = DataCompression(Compress);
                            Commpress.WriteAndExtend(Decompress, Decompress.Length);
                        }
                        catch (Exception E)
                        {
                            SockBufOTSReply.Clear();
                            File.AppendAllText("Decompress_" + DateTime.Now.ToString("ddMMyyyy") + ".txt", DateTime.Now.ToString() + ": " + IAHdr.ReplyType);
                        }
                    }
                    else
                    {
                        Array.Resize(ref AllReplyArray, IAHdr.ReplyCnt);
                        for (int i = 0; i < IAHdr.ReplyCnt; i++)
                        {
                            SockBufOTSReply.Read(buffToRead, IAHdr.ReplyRecSize);
                            AllReplyArray[i] = buffToRead.ToAllReplyStruct(IAHdr.ReplyType);
                        }
                    }

                    if (IAHdr.ReplyType == ConstantUnits.REP_ITS_TO_FE_RPT_T2T_SCRIPS ||
                    IAHdr.ReplyType == ConstantUnits.REP_ITS_TO_FE_RPT_BSE_NSE_MAP ||
                    IAHdr.ReplyType == ConstantUnits.REP_ITS_TO_FE_RPT_BANNED_SCRIPS_DERIV ||
                    IAHdr.ReplyType == ConstantUnits.REP_ITS_TO_FE_RPT_ILLIQUID_SCRIPS ||
                    IAHdr.ReplyType == 248)
                    {
                        switch (IAHdr.ReplyType)
                        {
                            case ConstantUnits.REP_ITS_TO_FE_RPT_T2T_SCRIPS:
                                {
                                    AllRepT2T = AllReplyArray;
                                    flAllRepT2TAssigned = true;
                                    listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + " - T2T Assigned Count = " + AllReplyArray.Length);
                                }
                                break;
                            case ConstantUnits.REP_ITS_TO_FE_RPT_BSE_NSE_MAP:
                                {
                                    AllRepBseNseMap = AllReplyArray;
                                    flAllRepBNMapAssigned = true;
                                    listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + " - BseNseMap Assigned Count = " + AllReplyArray.Length);
                                }
                                break;
                            case ConstantUnits.REP_ITS_TO_FE_RPT_BANNED_SCRIPS_DERIV:
                                {
                                    AllRepDerivBanScrip = AllReplyArray;
                                    flAllRepBANDerivAssigned = true;
                                    listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + " - Deriv Banned Assigned Count = " + AllReplyArray.Length);
                                }
                                break;
                            case ConstantUnits.REP_ITS_TO_FE_RPT_ILLIQUID_SCRIPS:
                                {
                                    AllRepIlliquid = AllReplyArray;
                                    flAllRepIlliqAssigned = true;
                                    listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + " - Illiquid Assigned Count = " + AllReplyArray.Length);
                                }
                                break;
                            case 248:
                                {
                                    AllRepSegment = AllReplyArray;
                                    flAllRepSegemntAssigned = true;
                                    listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + " - Segemnt Assigned Count = " + AllReplyArray.Length);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }

                try
                {
                    #region Multiple Send -> 10,11,16,20,21,30,31,32,33,50,51,110,111,120,121,130,131,133,150,151,211,212,221,222,231,232,233,17,171,,18,19,246

                    if (ConstantUnits.ARRAY_REP_ITS_TO_FE_MULTIPLE_SEND.Contains(IAHdr.ReplyType))
                    {

                        //Debug.WriteLine(IAHdr.ReplyType.ToString());
                        SendToMultipleSockets(IAHdr, AllReplyArray, IAError);
                        continue;
                    }

                    #endregion

                    #region Dup Conn -> 9

                    if (IAHdr.ReplyType == ConstantUnits.REP_OTS_TO_ITS_DUPCONN)
                    {
                        HandleDuplicateConnection(IAHdr, AllReplyArray);
                        continue;
                    }

                    #endregion

                    #region Maal Fodo Client List -> 86

                    if (IAHdr.ReplyType == ConstantUnits.REP_OTS_TO_ITS_MFCLIENT_LIST)
                    {
                        MaalFodo(IAHdr, AllReplyArray);
                        continue;
                    }

                    #endregion

                    Client = null;
                    ValCollection = SSLServer.Connections.Values;
                    foreach (nsoftware.IPWorksSSL.Connection TClient in ValCollection)
                    {
                        try
                        {
                            if (!TClient.Connected) continue;
                            Client = ClientsList[TClient.ConnectionId] as TClientConnectionToITS;
                            if (Client == null)
                            {
                                ErrorCnt++;
                                SL_ERROR.Add("RFO,Conid unavailable," + TClient.ConnectionId);
                                //SL_ERROR.Add(DateTime.Now + "RFO,Conid unavailable," + TClient.ConnectionId + "," + new string(IAHdr.TraderID, 0, IAHdr.TraderLength));

                                continue;
                            }

                            if (Client.SockNum == IAHdr.SocketNum) break;
                            else Client = null;
                        }
                        catch (Exception e)
                        {
                            Data.Add(TClient.ConnectionId + "," + new string(IAHdr.FEUserID).Substring(0, IAHdr.FELength) + "," + IAHdr.ReplyType.ToString());
                        }
                    }
                    if (Client == null) continue;

                    #region Individual Send on Socket no, other than 1,2,3,5,6

                    if (!ConstantUnits.ARRAY_REP_ITS_TO_FE_CLIENT_SEND.Contains(IAHdr.ReplyType))
                    {
                        SendToSocket(Client, IAHdr, AllReplyArray, IAError);
                        continue;
                    }

                    #endregion

                    #region Specific client send on Socket num as well as Id, and save 1,3,5,6,85


                    #region CheckingFor_IBTorCTCL

                    if (chk_SendAdminExtReqs.Checked && IAHdr.ErrorCode == 0 && IAHdr.ReplyType == ConstantUnits.REP_OTS_TO_ITS_LOGIN)
                    {
                        string UType = "IBT";

                        if (AllReplyArray[0].LoginReply.AllowNseCash == 1)
                        {
                            if (AllReplyArray[0].LoginReply.ChannelID_NC == NSE_Cash_Internet_UserInfo)  //111111111111100
                                UType = "IBT";
                            else
                                UType = "CTCL";

                        }
                        else
                            if (AllReplyArray[0].LoginReply.AllowNseDeriv == 1)
                            {
                                if (AllReplyArray[0].LoginReply.ChannelID_ND == NSE_Deriv_Internet_UserInfo)  //111111111111100
                                    UType = "IBT";
                                else
                                    UType = "CTCL";
                            }
                            else
                                if (AllReplyArray[0].LoginReply.AllowBseCash == 1)
                                {
                                    if (AllReplyArray[0].LoginReply.ChannelID_BC == BSE_Cash_Internet_UserInfo) //111111111111100
                                        UType = "IBT";
                                    else
                                        UType = "CTCL";
                                }

                        if (UType != cmbServerType.Text)
                        {
                            SendDisconnectionMsgToClient(Client, UType);
                            return;
                        }
                    }

                    #endregion

                    if ((IAHdr.ReplyType == 5 || IAHdr.ReplyType == 7 || IAHdr.ReplyType == 8 || IAHdr.ReplyType == 207 || IAHdr.ReplyType == 250||
                         IAHdr.ReplyType == 141 || IAHdr.ReplyType == 142) && IAHdr.TraderLength == 1)  //Changes done on 27-Mar-2017 - Rohan
                    {
                        SendToClientNew(Client, IAHdr, Compress, IAError);
                    }
                    else
                    {
                        SendToClient(Client, IAHdr, AllReplyArray, IAError);
                    }

                    switch (IAHdr.ReplyType)
                    {
                        #region Login reply. Save TraderId or FEUserID -> 1

                        case ConstantUnits.REP_OTS_TO_ITS_LOGIN: //Unknown User or Non FE Trader                       
                            {
                                Client.Is2FA = false;
                                if (IAHdr.ErrorCode > 0 || AllReplyArray[0].LoginReply.FEUserType == 2) continue;

                                Client.FEUserType = AllReplyArray[0].LoginReply.FEUserType;
                                if (AllReplyArray[0].LoginReply.FEUserType == 1) //Trader
                                    Client.TraderID = new string(AllReplyArray[0].LoginReply.FEUserID, 0, AllReplyArray[0].LoginReply.FEUserIDLength);
                                else
                                    Client.FEUserID = new string(AllReplyArray[0].LoginReply.FEUserID, 0, AllReplyArray[0].LoginReply.FEUserIDLength);


                            }

                            continue;


                        #endregion

                        #region Login reply. Save TraderId or FEUserID -> 3

                        case ConstantUnits.REP_OTS_TO_ITS_LOGIN_NEW: //Unknown User or Non FE Trader
                            {
                                Client.Is2FA = false;
                                if (IAHdr.ErrorCode > 0 || AllReplyArray[0].LoginRepyNew.FEUserType == 2) continue;

                                Client.FEUserType = AllReplyArray[0].LoginRepyNew.FEUserType;
                                if (AllReplyArray[0].LoginRepyNew.FEUserType == 1) //Trader
                                    Client.TraderID = new string(AllReplyArray[0].LoginRepyNew.FEUserID, 0, AllReplyArray[0].LoginRepyNew.FEUserIDLength);
                                else
                                    Client.FEUserID = new string(AllReplyArray[0].LoginRepyNew.FEUserID, 0, AllReplyArray[0].LoginRepyNew.FEUserIDLength);

                                continue;
                            }

                        #endregion

                        #region Login reply. Save TraderId or FEUserID -> 85

                        case ConstantUnits.REP_OTS_TO_ITS_LOGIN_NEW_TwoFactor: //Unknown User or Non FE Trader
                            {
                                Client.Is2FA = true;
                                if (IAHdr.ErrorCode > 0 || AllReplyArray[0].LoginTwoFactorAuthRecord.FEUserType == 2) continue;
                                if (AllReplyArray[0].LoginTwoFactorAuthRecord.Journey == 3 || AllReplyArray[0].LoginTwoFactorAuthRecord.Journey == 4)
                                {
                                    Client.FEUserType = AllReplyArray[0].LoginTwoFactorAuthRecord.FEUserType;
                                    if (AllReplyArray[0].LoginTwoFactorAuthRecord.FEUserType == 1) //Trader
                                        Client.TraderID = new string(AllReplyArray[0].LoginTwoFactorAuthRecord.FEUserID, 0, AllReplyArray[0].LoginTwoFactorAuthRecord.FEUserIDLength);
                                    else
                                        Client.FEUserID = new string(AllReplyArray[0].LoginTwoFactorAuthRecord.FEUserID, 0, AllReplyArray[0].LoginTwoFactorAuthRecord.FEUserIDLength);


                                }
                                continue;
                            }

                        #endregion

                        #region Trader List reply. Add under respective Admin -> 5

                        case ConstantUnits.REP_OTS_TO_ITS_ADD_TRADER:
                            {

                                if (IAHdr.ReplyType == 5 && IAHdr.TraderLength == 1)
                                {
                                    byte[] Data = new byte[200];
                                    int length = IAHdr.ReplyCnt + Reach;

                                  
                                    Reach = Client.ClientArray.Length;

                                    Array.Resize(ref AllReplyArrayNew, IAHdr.ReplyCnt);
                                    Array.Resize(ref Client.ClientArray, Client.ClientArray.Length + IAHdr.ReplyCnt);

                                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    {
                                        Commpress.Read(Data, IAHdr.ReplyRecSize);
                                        AllReplyArrayNew[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);
                                     
                                        Client.ClientArray[Reach] = new string(AllReplyArrayNew[i].TraderListReplyRec.TraderID, 0, AllReplyArrayNew[i].TraderListReplyRec.TDRIDLength);
                                        Reach++;
                                    }

                                    //for (int i = Reach; i < AllReplyArrayNew.Length; i++)
                                    //{

                                    //    AllReplyArrayNew[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);
                                    //    Client.ClientArray[i] = new string(AllReplyArrayNew[i].TraderListReplyRec.TraderID, 0, AllReplyArrayNew[i].TraderListReplyRec.TDRIDLength);
                                    //    //Reach++;
                                    //}                                       
                                    //for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    //{
                                    //    Commpress.Peek(Data, Marshal.SizeOf(new IntellectUtilities.TTraderListReplyRecord()));
                                    //    Client.ClientArray[i] = Data.ToStructure<IntellectUtilities.TTraderListReplyRecord>();

                                    //    Client.ClientArray[i] = new string(AllReplyArray[i].TraderListReplyRec.TraderID, 0, AllReplyArray[i].TraderListReplyRec.TDRIDLength);
                                    //    //Client.ClientArray[i].TraderID = AllReplyArray[i].TraderListReplyRec.TraderID;
                                    //    //Client.ClientArray[i].TraderIDLength = AllReplyArray[i].TraderListReplyRec.TDRIDLength;
                                    //}
                                    Client.ClientCnt = Client.ClientArray.Length;
                                }
                                else
                                {
                                    Reach = Client.ClientArray.Length;
                                    Array.Resize(ref Client.ClientArray, Client.ClientArray.Length + IAHdr.ReplyCnt);
                                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    {
                                        Client.ClientArray[Reach] = new string(AllReplyArray[i].TraderListReplyRec.TraderID, 0, AllReplyArray[i].TraderListReplyRec.TDRIDLength);
                                        //Client.ClientArray[i].TraderID = AllReplyArray[i].TraderListReplyRec.TraderID;
                                        //Client.ClientArray[i].TraderIDLength = AllReplyArray[i].TraderListReplyRec.TDRIDLength;
                                        Reach++;
                                    }
                                    Client.ClientCnt = Client.ClientArray.Length;
                                }
                                continue;
                            }

                        #endregion

                        #region Trader Orders reply. Add under respective Admin -> 7

                        case 7:
                            {

                                if (IAHdr.ReplyType == 7 && IAHdr.TraderLength == 1)
                                {
                                    byte[] Data = new byte[200];
                                    int length = IAHdr.ReplyCnt + Reach;


                                    Reach = Client.ClientArray.Length;

                                    Array.Resize(ref AllReplyArrayNew, IAHdr.ReplyCnt);
                                    Array.Resize(ref Client.ClientArray, Client.ClientArray.Length + IAHdr.ReplyCnt);

                                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    {
                                        Commpress.Read(Data, IAHdr.ReplyRecSize);
                                        AllReplyArrayNew[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);

                                        //Client.ClientArray[Reach] = new string(AllReplyArrayNew[i].OrdersReportDealerReply.TraderID, 0, AllReplyArrayNew[i].TraderListReplyRec.TDRIDLength);
                                        Reach++;
                                    }

                                    //for (int i = Reach; i < AllReplyArrayNew.Length; i++)
                                    //{

                                    //    AllReplyArrayNew[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);
                                    //    Client.ClientArray[i] = new string(AllReplyArrayNew[i].TraderListReplyRec.TraderID, 0, AllReplyArrayNew[i].TraderListReplyRec.TDRIDLength);
                                    //    //Reach++;
                                    //}                                       
                                    //for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    //{
                                    //    Commpress.Peek(Data, Marshal.SizeOf(new IntellectUtilities.TTraderListReplyRecord()));
                                    //    Client.ClientArray[i] = Data.ToStructure<IntellectUtilities.TTraderListReplyRecord>();

                                    //    Client.ClientArray[i] = new string(AllReplyArray[i].TraderListReplyRec.TraderID, 0, AllReplyArray[i].TraderListReplyRec.TDRIDLength);
                                    //    //Client.ClientArray[i].TraderID = AllReplyArray[i].TraderListReplyRec.TraderID;
                                    //    //Client.ClientArray[i].TraderIDLength = AllReplyArray[i].TraderListReplyRec.TDRIDLength;
                                    //}
                                    Client.ClientCnt = Client.ClientArray.Length;
                                }
                                else
                                {
                                    Reach = Client.ClientArray.Length;
                                    Array.Resize(ref Client.ClientArray, Client.ClientArray.Length + IAHdr.ReplyCnt);
                                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    {
                                        Client.ClientArray[Reach] = new string(AllReplyArray[i].TraderListReplyRec.TraderID, 0, AllReplyArray[i].TraderListReplyRec.TDRIDLength);
                                        //Client.ClientArray[i].TraderID = AllReplyArray[i].TraderListReplyRec.TraderID;
                                        //Client.ClientArray[i].TraderIDLength = AllReplyArray[i].TraderListReplyRec.TDRIDLength;
                                        Reach++;
                                    }
                                    Client.ClientCnt = Client.ClientArray.Length;
                                }
                                continue;
                            }

                        #endregion

                        #region Trader Trades reply. Add under respective Admin -> 8

                        case 8:
                            {

                                if (IAHdr.ReplyType == 8 && IAHdr.TraderLength == 1)
                                {
                                    byte[] Data = new byte[200];
                                    int length = IAHdr.ReplyCnt + Reach;


                                    Reach = Client.ClientArray.Length;

                                    Array.Resize(ref AllReplyArrayNew, IAHdr.ReplyCnt);
                                    Array.Resize(ref Client.ClientArray, Client.ClientArray.Length + IAHdr.ReplyCnt);

                                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    {
                                        Commpress.Read(Data, IAHdr.ReplyRecSize);
                                        AllReplyArrayNew[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);

                                        Client.ClientArray[Reach] = new string(AllReplyArrayNew[i].TraderListReplyRec.TraderID, 0, AllReplyArrayNew[i].TraderListReplyRec.TDRIDLength);
                                        Reach++;
                                    }

                                    //for (int i = Reach; i < AllReplyArrayNew.Length; i++)
                                    //{

                                    //    AllReplyArrayNew[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);
                                    //    Client.ClientArray[i] = new string(AllReplyArrayNew[i].TraderListReplyRec.TraderID, 0, AllReplyArrayNew[i].TraderListReplyRec.TDRIDLength);
                                    //    //Reach++;
                                    //}                                       
                                    //for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    //{
                                    //    Commpress.Peek(Data, Marshal.SizeOf(new IntellectUtilities.TTraderListReplyRecord()));
                                    //    Client.ClientArray[i] = Data.ToStructure<IntellectUtilities.TTraderListReplyRecord>();

                                    //    Client.ClientArray[i] = new string(AllReplyArray[i].TraderListReplyRec.TraderID, 0, AllReplyArray[i].TraderListReplyRec.TDRIDLength);
                                    //    //Client.ClientArray[i].TraderID = AllReplyArray[i].TraderListReplyRec.TraderID;
                                    //    //Client.ClientArray[i].TraderIDLength = AllReplyArray[i].TraderListReplyRec.TDRIDLength;
                                    //}
                                    Client.ClientCnt = Client.ClientArray.Length;
                                }
                                else
                                {
                                    Reach = Client.ClientArray.Length;
                                    Array.Resize(ref Client.ClientArray, Client.ClientArray.Length + IAHdr.ReplyCnt);
                                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    {
                                        Client.ClientArray[Reach] = new string(AllReplyArray[i].TraderListReplyRec.TraderID, 0, AllReplyArray[i].TraderListReplyRec.TDRIDLength);
                                        //Client.ClientArray[i].TraderID = AllReplyArray[i].TraderListReplyRec.TraderID;
                                        //Client.ClientArray[i].TraderIDLength = AllReplyArray[i].TraderListReplyRec.TDRIDLength;
                                        Reach++;
                                    }
                                    Client.ClientCnt = Client.ClientArray.Length;
                                }
                                continue;
                            }

                        #endregion
                        #region RMS Margin of all Clients -> 205    //Added done on 27-Mar-2017 - Rohan

                        //case 205:
                        //    {

                        //        if (IAHdr.ReplyType == 205 && IAHdr.TraderLength == 1)
                        //        {
                        //            byte[] Data = new byte[200];
                        //            int length = IAHdr.ReplyCnt + Reach;

                        //            Reach = Client.ClientArrayForMargins.Length;

                        //            Array.Resize(ref AllReplyArrayMgn, IAHdr.ReplyCnt);
                        //            Array.Resize(ref Client.ClientArrayForMargins, Client.ClientArrayForMargins.Length + IAHdr.ReplyCnt);

                        //            for (int i = 0; i < IAHdr.ReplyCnt; i++)
                        //            {
                        //                Commpress.Read(Data, IAHdr.ReplyRecSize);
                        //                AllReplyArrayMgn[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);
                        //                Client.ClientArrayForMargins[Reach] = new string(AllReplyArrayMgn[i].MTMClientsWithAllMarginsReply.FEUserID, 0, AllReplyArrayMgn[i].MTMClientsWithAllMarginsReply.FELength);
                        //                Reach++;
                        //            }

                        //            //Client.ClientCnt = Client.ClientArrayForMargins.Length;
                        //        }
                        //        else
                        //        {
                        //            Reach = Client.ClientArrayForMargins.Length;
                        //            Array.Resize(ref Client.ClientArrayForMargins, Client.ClientArrayForMargins.Length + IAHdr.ReplyCnt);
                        //            for (int i = 0; i < IAHdr.ReplyCnt; i++)
                        //            {
                        //                Client.ClientArrayForMargins[Reach] = new string(AllReplyArray[i].MTMClientsWithAllMarginsReply.FEUserID, 0, AllReplyArray[i].MTMClientsWithAllMarginsReply.FELength);
                        //                Reach++;
                        //            }
                        //            //Client.ClientCnt = Client.ClientArrayForMargins.Length;
                        //        }
                        //        continue;
                        //    }

                        #endregion

                        #region RMS Margin Commodities of all Clients -> 207    //Added done on 27-Mar-2017 - Rohan

                        case 207:
                            {

                                if (IAHdr.ReplyType == 207 && IAHdr.TraderLength == 1)
                                {
                                    byte[] Data = new byte[200];
                                    int length = IAHdr.ReplyCnt + Reach;

                                    Reach = Client.ClientArrayForMcxMargins.Length;

                                    Array.Resize(ref AllReplyArrayMgnMCX, IAHdr.ReplyCnt);
                                    Array.Resize(ref Client.ClientArrayForMcxMargins, Client.ClientArrayForMcxMargins.Length + IAHdr.ReplyCnt);

                                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    {
                                        Commpress.Read(Data, IAHdr.ReplyRecSize);
                                        AllReplyArrayMgnMCX[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);
                                        Client.ClientArrayForMcxMargins[Reach] = new string(AllReplyArrayMgnMCX[i].MTMClientsWithAllMarginsReplyMcx.FEUserID, 0, AllReplyArrayMgnMCX[i].MTMClientsWithAllMarginsReplyMcx.FELength);
                                        Reach++;
                                    }

                                    //Client.ClientCnt = Client.ClientArrayForMcxMargins.Length;
                                }
                                else
                                {
                                    Reach = Client.ClientArrayForMcxMargins.Length;
                                    Array.Resize(ref Client.ClientArrayForMcxMargins, Client.ClientArrayForMcxMargins.Length + IAHdr.ReplyCnt);
                                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    {
                                        Client.ClientArrayForMcxMargins[Reach] = new string(AllReplyArray[i].MTMClientsWithAllMarginsReplyMcx.FEUserID, 0, AllReplyArray[i].MTMClientsWithAllMarginsReplyMcx.FELength);
                                        Reach++;
                                    }
                                    //Client.ClientCnt = Client.ClientArrayForMcxMargins.Length;
                                }
                                continue;
                            }

                        #endregion

                        //case 250:
                        //    {
                        //        //Array.Clear(Client.ClientArrayForMarginsInstrumentWise, 0, Client.ClientArrayForMarginsInstrumentWise.Length);
                        //        Client.ClientArrayForMarginsInstrumentWise = new string[0];//new string(AllReplyArrayNew[0].MTMClientsWithAllMarginsReplyInstrumentWise.FEUserID, 0);//, AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.FELength);
                        //        //Reach = 0;
                        //        if (IAHdr.ReplyType == 250 && IAHdr.TraderLength == 1)
                        //        {
                        //            byte[] Data = new byte[500];
                        //            int length = IAHdr.ReplyCnt + Reach;
                        //            Array.Resize(ref Client.ClientArrayForMarginsInstrumentWise, 0);

                        //            Reach = Client.ClientArrayForMarginsInstrumentWise.Length;

                        //            Array.Resize(ref AllReplyArrayNew, IAHdr.ReplyCnt);
                        //            Array.Resize(ref Client.ClientArrayForMarginsInstrumentWise, Client.ClientArrayForMarginsInstrumentWise.Length + IAHdr.ReplyCnt);

                        //            for (int i = 0; i < IAHdr.ReplyCnt; i++)
                        //            {
                        //                Commpress.Read(Data, IAHdr.ReplyRecSize);
                        //                AllReplyArrayNew[i] = Data.ToAllReplyStruct(IAHdr.ReplyType);
                        //                //string c = "-";
                        //                //var b = string.Join("-", AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.SegmentId);
                        //                //var z = string.Join(AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.FEUserID.ToString(),"-");
                        //                //StringBuilder sb = new StringBuilder(z);
                        //                //sb.Append(Convert.ToChar(AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.FEUserID),0);
                        //                //sb.Append(Convert.ToChar(c), z.Length + 1);
                        //                //sb.Append(Convert.ToChar(AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.SegmentId), z.Length + 2);
                        //                //char[] g = sb.tochara;
                        //                //AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.SegmentId.CopyTo(z, AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.FEUserID.Length);
                        //                //char[] a =AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.FEUserID
                        //                //Client.ClientArrayForMarginsInstrumentWise[i] = new string(g, 0, AllReplyArrayNew[i].MTMClientsWithAllMarginsReplyInstrumentWise.FELength);
                        //                //Client.ClientArrayForMarginsInstrumentWise[Reach] = new string(AllReplyArray[i].MTMClientsWithAllMarginsReplyInstrumentWise.FEUserID, 0, AllReplyArray[i].MTMClientsWithAllMarginsReplyInstrumentWise.FELength);
                        //                //Reach = Client.ClientArrayForMarginsInstrumentWise.Length;
                        //                //Reach++;
                        //            }
                        //            //Client.ClientCnt = Client.ClientArrayForMarginsInstrumentWise.Length;
                        //        }
                        //        else
                        //        {
                        //            Reach = Client.ClientArrayForMarginsInstrumentWise.Length;
                        //            Array.Resize(ref Client.ClientArrayForMarginsInstrumentWise, Client.ClientArrayForMarginsInstrumentWise.Length + IAHdr.ReplyCnt);
                        //            for (int i = 0; i < IAHdr.ReplyCnt; i++)
                        //            {
                        //                //Client.ClientArrayForMarginsInstrumentWise[Reach] = new string(AllReplyArray[i].MTMClientsWithAllMarginsReplyInstrumentWise.FEUserID, 0, AllReplyArray[i].MTMClientsWithAllMarginsReplyInstrumentWise.FELength);
                        //                //Client.ClientArray[i].TraderID = AllReplyArray[i].TraderListReplyRec.TraderID;
                        //                //Client.ClientArray[i].TraderIDLength = AllReplyArray[i].TraderListReplyRec.TDRIDLength;
                        //               // Reach++;
                        //            }
                        //            //Client.ClientCnt = Client.ClientArrayForMarginsInstrumentWise.Length;
                        //        }
                        //        continue;

                        //    }

                        #region Varify Trader reply. -> 6

                        case ConstantUnits.REP_OTS_TO_ITS_VARIFY_TRADER:
                            {
                                int j;
                                //string ID, Id2;
                                //if (AllReplyArray[0].TraderListReplyRec.TdrNameLength > 0 && Client.FEUserType == 15)  //Big Admin
                                //{
                                //    ID = new string(AllReplyArray[0].TraderListReplyRec.TraderID, 0, AllReplyArray[0].TraderListReplyRec.TDRIDLength);
                                //    j = -1;
                                //    for (int i = 0; i < Client.ClientArray.Length; i++)
                                //    {
                                //        Id2 = Client.ClientArray[i];
                                //        if (Id2 != ID) continue;
                                //        j = i;
                                //        break;
                                //    }
                                //    if (j == -1)
                                //    {
                                //        j = Client.ClientArray.Length;
                                //        Array.Resize(ref Client.ClientArray, j + 1);
                                //        Client.ClientArray[j] = new string(AllReplyArray[0].TraderListReplyRec.TraderID, 0, AllReplyArray[0].TraderListReplyRec.TDRIDLength);
                                //    }
                                //}
                                //else
                                //{

                                if (AllReplyArray[0].TraderListReplyRec.TdrNameLength > 0 && Client.FEUserType == 16) //CallCenter ; 1 Client only
                                {
                                    j = Client.ClientArray.Length;
                                    if (j != 1)
                                        Array.Resize(ref Client.ClientArray, 1);
                                    Client.ClientArray[0] = new string(AllReplyArray[0].TraderListReplyRec.TraderID, 0, AllReplyArray[0].TraderListReplyRec.TDRIDLength);
                                }
                                else
                                    if (AllReplyArray[0].TraderListReplyRec.TdrNameLength <= 0 && Client.FEUserType == 16) //CallCenter ; 1 Client only
                                        Client.ClientArray = new string[0];

                                //}
                                Client.ClientCnt = Client.ClientArray.Length;
                                continue;
                            }

                        #endregion
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    SockBufOTSReply.Clear();
                    WriteToErrorLog(IAHdr, AllReplyArray, IAError, e);
                    continue;
                }
            }
        }

        private void SendToMultipleSockets(TIAReplyHeaderRecord IAHdr, TAllReplyNewRecord[] AllReplyArray, TIAErrorMessageRecord IAError)
        {
            string TraderId1;
            TClientConnectionToITS Client;
            ICollection<nsoftware.IPWorksSSL.Connection> ValCollection;

            TraderId1 = new string(IAHdr.TraderID, 0, IAHdr.TraderLength);
            if (Chkbox_WriteActivityToDisplay.Checked)
            {
                if (TraderId1 == txtDisplayClid.Text.Trim())
                {
                    string str1 = DateTime.Now.ToString("hh:mm:ss") + " - STM - Rep type: " + IAHdr.ReplyType + ", name:" + TraderId1 + ", Sock:" + IAHdr.SocketNum;
                    listBox1.Items.Insert(0, str1);
                }
            }

            ValCollection = SSLServer.Connections.Values;
            foreach (nsoftware.IPWorksSSL.Connection TClient in ValCollection)
            {
                try
                {
                    if (!TClient.Connected) continue;
                    Client = ClientsList[TClient.ConnectionId] as TClientConnectionToITS;
                    if (Client == null)
                    {
                        ErrorCnt++;
                        SL_ERROR.Add("STM,Conid unavailable," + TClient.ConnectionId);
                        continue;
                    }

                    if (Client.FEUserType == 0) continue;
                    if (Client.FEUserType == 1)
                    {
                        if (Client.TraderID != TraderId1) continue;

                        IAHdr.SocketNum = Client.SockNum;
                        SendToSocket(Client, IAHdr, AllReplyArray, IAError);
                    } //25 - Type Send ALL CONFIRMATIONS //26-10-2013
                    else if (Client.FEUserType == 25)
                    {
                        IAHdr.SocketNum = Client.SockNum;
                        SendToSocket(Client, IAHdr, AllReplyArray, IAError);
                    }
                    else
                    {
                        for (int i = 0; i < Client.ClientCnt; i++)
                        {
                    
                            if (TraderId1 != Client.ClientArray[i]) continue;

                            IAHdr.SocketNum = Client.SockNum;
                            SendToSocket(Client, IAHdr, AllReplyArray, IAError);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Data.Add(TClient.ConnectionId + "," + new string(IAHdr.FEUserID).Substring(0, IAHdr.FELength) + "," + IAHdr.ReplyType.ToString()); ;
                }
            }

            #region Comment

            //foreach (TClientConnectionToITS Client in ClientsList.Values)
            //{
            //    if (Client.FEUserType == 0) continue;
            //    if (Client.FEUserType == 1)
            //    {
            //        if (Client.TraderID != TraderId1) continue;

            //        IAHdr.SocketNum = Client.SockNum;
            //        SendToSocket(Client, IAHdr, AllReplyArray, IAError);
            //    }
            //    else
            //    {
            //        for (int i = 0; i < Client.ClientCnt; i++)
            //        {
            //            if (TraderId1 != Client.ClientArray[i]) continue;

            //            IAHdr.SocketNum = Client.SockNum;
            //            SendToSocket(Client, IAHdr, AllReplyArray, IAError);
            //            break;
            //        }
            //    }
            //}

            #endregion
        }

        private void SendToSocket(TClientConnectionToITS Client, TIAReplyHeaderRecord IAHdr, TAllReplyNewRecord[] AllReplyArray, TIAErrorMessageRecord IAError)
        {
            List<byte> bToSend = null;

            try
            {
                if (IAHdr.SocketNum <= 0) return;
                if (Client == null) return;
                bToSend = new List<byte>();

                bToSend.AddRange(IAHdr.ToByteArray());
                if (IAHdr.ErrorCode > 0)
                    bToSend.AddRange(IAError.ToByteArray());
                if (IAHdr.ReplyCnt != 0 && IAHdr.ReplyLen != 0)
                    for (int i = 0; i < IAHdr.ReplyCnt; i++)
                        bToSend.AddRange(AllReplyArray[i].ToAllReplyArray(IAHdr.ReplyType));

                if (Chkbox_WriteActivityToDisplay.Checked)
                {
                    if (Client.FEUserID == txtDisplayClid.Text.Trim())
                    {
                        string str1 = DateTime.Now.ToString("hh:mm:ss") + " - STC - Rep type: " + IAHdr.ReplyType + ", name:" + Client.FEUserID + ", Sock:" + IAHdr.SocketNum;
                        listBox1.Items.Insert(0, str1);
                    }
                }

                if (SSLServer.Connections[Client.ConnectionID].Connected)
                {
                    // ---Log --------------------------------------------------------------
                    if (chkSettingsWriteReply.Checked)
                    {
                        List<byte> bToWrite;
                        TGlobalHeader GHdr;

                        GHdr.DateTime = DateTime.Now;
                        GHdr.Size = bToSend.Count;
                        GHdr.SentFrom = (byte)'S';

                        bToWrite = new List<byte>();
                        bToWrite.AddRange(GHdr.ToByteArray());
                        bToWrite.AddRange(bToSend.ToArray());

                        ITS_REP_DatFileStream.Write(bToWrite.ToArray(), 0, bToWrite.Count);
                    }
                    // ---Log --------------------------------------------------------------

                    LastSentToClientTime = DateTime.Now;
                    SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
                    //Debug.WriteLine(IAHdr.ReplyType.ToString() + " Sent");
                }
            }
            catch (nsoftware.IPWorksSSL.IPWorksSSLException ex)
            {
                if (ex.Code == 135) //WOULDBLOCK Error
                {
                    Client.PendingData.AddRange(bToSend);

                    if (!RetrySending.Contains<TClientConnectionToITS>(Client))
                        RetrySending.Add(Client);
                }
            }
        }

        private void HandleDuplicateConnection(TIAReplyHeaderRecord IAHdr, TAllReplyNewRecord[] AllReplyArray)
        {
            string Id;
            int ITSNum;
            List<byte> bToSend;
            TClientConnectionToITS TClient;
            ICollection<nsoftware.IPWorksSSL.Connection> ValCollection;

            if (IAHdr.TraderLength <= 0) return;

            ITSNum = Convert.ToInt32(txtSettingsITSID.Text);
            Id = new string(IAHdr.TraderID, 0, IAHdr.TraderLength);

            ValCollection = SSLServer.Connections.Values;
            foreach (nsoftware.IPWorksSSL.Connection Client in ValCollection)
            {
                if (!Client.Connected) continue;
                TClient = ClientsList[Client.ConnectionId] as TClientConnectionToITS;
                if (TClient == null)
                {
                    ErrorCnt++;
                    SL_ERROR.Add("DUP,Conid unavailable," + Client.ConnectionId);
                    continue;
                }

                if (TClient.FEUserID.ToUpper() != Id) continue;
                if ((ITSNum == AllReplyArray[0].LogoutUserRec.ITS_Num && IAHdr.SocketNum != TClient.SockNum) ||
                    ITSNum != AllReplyArray[0].LogoutUserRec.ITS_Num)
                {
                    IAHdr.SocketNum = TClient.SockNum;

                    bToSend = new List<byte>();
                    bToSend.AddRange(IAHdr.ToByteArray());
                    bToSend.AddRange(AllReplyArray[0].LogoutUserRec.ToByteArray());

                    // ---Log --------------------------------------------------------------
                    if (chkSettingsWriteReply.Checked)
                    {
                        List<byte> bToWrite;
                        TGlobalHeader GHdr;

                        GHdr.DateTime = DateTime.Now;
                        GHdr.Size = bToSend.Count;
                        GHdr.SentFrom = (byte)'D';

                        bToWrite = new List<byte>();
                        bToWrite.AddRange(GHdr.ToByteArray());
                        bToWrite.AddRange(bToSend.ToArray());

                        ITS_REP_DatFileStream.Write(bToWrite.ToArray(), 0, bToWrite.Count);
                    }
                    // ---Log --------------------------------------------------------------

                    LastSentToClientTime = DateTime.Now;
                    Client.DataToSendB = bToSend.ToArray();
                    break;
                }
            }

            #region Comment

            //foreach (TClientConnectionToITS TClient in ClientsList.Values)
            //{                
            //    if (TClient.FEUserID != Id) continue;
            //    if ((ITSNum == AllReplyArray[0].LogoutUserRec.ITS_Num && IAHdr.SocketNum != TClient.SockNum) ||
            //        ITSNum != AllReplyArray[0].LogoutUserRec.ITS_Num)
            //    {
            //        IAHdr.SocketNum = TClient.SockNum;

            //        bToSend = new List<byte>();
            //        bToSend.AddRange(IAHdr.ToByteArray());
            //        bToSend.AddRange(AllReplyArray[0].LogoutUserRec.ToByteArray());

            //        if (SSLServer.Connections[TClient.ConnectionID].Connected)
            //        {
            //            LastSentToClientTime = DateTime.Now;
            //            SSLServer.Connections[TClient.ConnectionID].DataToSendB = bToSend.ToArray();
            //        }
            //        break;
            //    }
            //}

            #endregion
        }

        private void MaalFodo(TIAReplyHeaderRecord IAHdr, TAllReplyNewRecord[] AllReplyArray)
        {
            TIAReplyHeaderRecord IAHdr2;
            TMessageStringRecord MsgRec;
            string MSG, TraderId;

            IAHdr2.ReplyType = ConstantUnits.REP_ITS_TO_FE_MESSAGE;
            IAHdr2.ReplyLen = ConstantUnits.TMessageStringRecordSize;
            IAHdr2.ReplyRecSize = ConstantUnits.TMessageStringRecordSize;
            IAHdr2.ReplyCnt = 1;
            IAHdr2.ErrorCode = 0;
            IAHdr2.ChannelID_NC = 0;
            IAHdr2.ChannelID_ND = 0;
            IAHdr2.ChannelID_BC = 0;
            IAHdr2.SocketNum = 0;
            IAHdr2.AccountType = 0;
            IAHdr2.ClientType = 1;

            for (int i = 0; i < IAHdr.ReplyCnt; i++)
            {
                IAHdr2.TraderID = AllReplyArray[i].MFClientsReportReplyRec.TraderID;
                IAHdr2.TraderLength = AllReplyArray[i].MFClientsReportReplyRec.TraderIDLength;
                IAHdr2.FEUserID = AllReplyArray[i].MFClientsReportReplyRec.AdminID;
                IAHdr2.FELength = AllReplyArray[i].MFClientsReportReplyRec.AdminIDLength;

                TraderId = new string(IAHdr2.TraderID, 0, IAHdr2.TraderLength);

                MSG = "ClientCode "
                    + TraderId
                    + " : Your account is under square up for an amount due of Rs. "
                    + (AllReplyArray[i].MFClientsReportReplyRec.FinalMF).ToString("0.00")
                    + System.Environment.NewLine
                    + " Square up process will start soon and your pending orders, if any, will be cancelled."
                    + System.Environment.NewLine + " Please take necessary action now.";

                MsgRec.MsgStr = MSG.PadRight(250).ToCharArray();
                MsgRec.MsgStrLength = (byte)MSG.Length;

                SendToMultipleSockets2(TraderId, IAHdr2, MsgRec);
            }
        }

        private void SendToMultipleSockets2(string TraderId, TIAReplyHeaderRecord IAHdr2, TMessageStringRecord MsgRec)
        {
            List<byte> bToSend;
            TClientConnectionToITS CIRec;
            ICollection<nsoftware.IPWorksSSL.Connection> ValCollection;

            ValCollection = SSLServer.Connections.Values;
            foreach (nsoftware.IPWorksSSL.Connection TClient in ValCollection)
            {
                if (!TClient.Connected) continue;
                CIRec = ClientsList[TClient.ConnectionId] as TClientConnectionToITS;
                if (CIRec == null)
                {
                    ErrorCnt++;
                    SL_ERROR.Add("STM2,Conid unavailable," + TClient.ConnectionId);
                    continue;
                }

                if (CIRec.FEUserType == 0) continue; //not in use
                if (CIRec.FEUserType != 1) continue;
                if (CIRec.TraderID != TraderId) continue;

                bToSend = new List<byte>();
                bToSend.AddRange(IAHdr2.ToByteArray());
                bToSend.AddRange(MsgRec.ToByteArray());

                TClient.DataToSendB = bToSend.ToArray();
                break;
            }
        }

        private void SendToClient(TClientConnectionToITS Client, TIAReplyHeaderRecord IAHdr2, TAllReplyNewRecord[] AllReplyArray, TIAErrorMessageRecord IAError)
        {
            List<byte> bToSend = null;
            try
            {
                if (IAHdr2.SocketNum <= 0) return;

                string str = new string(IAHdr2.FEUserID, 0, IAHdr2.FELength);
                if (Client.FEUserID != str)
                {
                    BadClientRejectCnt++;
                    listBox1.Items.Insert(0, Client.FEUserID + "****  Return from SendToClient as BadClientReject *****" + str + ", ReplyType =" + IAHdr2.ReplyType); //05.10.15--Yashwant
                    return;
                }

                bToSend = new List<byte>();
                bToSend.AddRange(IAHdr2.ToByteArray());
                if (IAHdr2.ErrorCode > 0)
                    bToSend.AddRange(IAError.ToByteArray());
                if (IAHdr2.ReplyCnt != 0 && IAHdr2.ReplyLen != 0)
                    for (int i = 0; i < IAHdr2.ReplyCnt; i++)
                        bToSend.AddRange(AllReplyArray[i].ToAllReplyArray(IAHdr2.ReplyType));

                if (Chkbox_WriteActivityToDisplay.Checked)
                {
                    if (Client.FEUserID == txtDisplayClid.Text.Trim())
                    {
                        string str1 = DateTime.Now.ToString("hh:mm:ss") + " - STC - Rep type: " + IAHdr2.ReplyType + ", name:" + Client.FEUserID + ", Sock:" + IAHdr2.SocketNum;
                        listBox1.Items.Insert(0, str1);
                    }
                }

                if (SSLServer.Connections[Client.ConnectionID].Connected)
                {
                    // ---Log --------------------------------------------------------------
                    if (chkSettingsWriteReply.Checked)
                    {
                        List<byte> bToWrite;
                        TGlobalHeader GHdr;

                        GHdr.DateTime = DateTime.Now;
                        GHdr.Size = bToSend.Count;
                        GHdr.SentFrom = (byte)'C';

                        bToWrite = new List<byte>();
                        bToWrite.AddRange(GHdr.ToByteArray());
                        bToWrite.AddRange(bToSend.ToArray());

                        ITS_REP_DatFileStream.Write(bToWrite.ToArray(), 0, bToWrite.Count);
                    }
                    // ---Log --------------------------------------------------------------
                    LastSentToClientTime = DateTime.Now;
                    SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
                }
            }
            catch (nsoftware.IPWorksSSL.IPWorksSSLException ex)
            {
                if (ex.Code == 135) //WOULDBLOCK Error
                {
                    Client.PendingData.AddRange(bToSend);

                    if (!RetrySending.Contains<TClientConnectionToITS>(Client))
                        RetrySending.Add(Client);
                }
            }
        }

        private void SendToClientNew(TClientConnectionToITS Client, TIAReplyHeaderRecord IAHdr2, byte[] AllReplyArray, TIAErrorMessageRecord IAError)
        {
            List<byte> bToSend=null;
            //Debug.WriteLine(IAHdr2.ReplyType.ToString());
            try
            {
                if (IAHdr2.SocketNum <= 0) return;

                string str = new string(IAHdr2.FEUserID, 0, IAHdr2.FELength);
                if (Client.FEUserID != str)
                {
                    BadClientRejectCnt++;
                    listBox1.Items.Insert(0, Client.FEUserID + "****  Return from SendToClientNew as BadClientReject *****" + str + ",ReplyType =" + IAHdr2.ReplyType); //05.10.15--Yashwant
                    return;
                }

                bToSend = new List<byte>();
                bToSend.AddRange(IAHdr2.ToByteArray());
                if (IAHdr2.ErrorCode > 0)
                    bToSend.AddRange(IAError.ToByteArray());
                if (IAHdr2.ReplyCnt != 0 && IAHdr2.ReplyLen != 0)
                    //for (int i = 0; i < IAHdr2.ReplyCnt; i++)
                    bToSend.AddRange(AllReplyArray);

                if (Chkbox_WriteActivityToDisplay.Checked)
                {
                    if (Client.FEUserID == txtDisplayClid.Text.Trim())
                    {
                        string str1 = DateTime.Now.ToString("hh:mm:ss") + " - STC - Rep type: " + IAHdr2.ReplyType + ", name:" + Client.FEUserID + ", Sock:" + IAHdr2.SocketNum;
                        listBox1.Items.Insert(0, str1);
                    }
                }

                if (SSLServer.Connections[Client.ConnectionID].Connected)
                {
                    // ---Log --------------------------------------------------------------
                    if (chkSettingsWriteReply.Checked)
                    {
                        List<byte> bToWrite;
                        TGlobalHeader GHdr;

                        GHdr.DateTime = DateTime.Now;
                        GHdr.Size = bToSend.Count;
                        GHdr.SentFrom = (byte)'C';

                        bToWrite = new List<byte>();
                        bToWrite.AddRange(GHdr.ToByteArray());
                        bToWrite.AddRange(bToSend.ToArray());

                        ITS_REP_DatFileStream.Write(bToWrite.ToArray(), 0, bToWrite.Count);
                    }
                    // ---Log --------------------------------------------------------------
                    LastSentToClientTime = DateTime.Now;
                    SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
                }
            }
            catch (nsoftware.IPWorksSSL.IPWorksSSLException ex)
            {
                if (ex.Code == 135) //WOULDBLOCK Error
                {
                    Client.PendingData.AddRange(bToSend);

                    if (!RetrySending.Contains<TClientConnectionToITS>(Client))
                        RetrySending.Add(Client);
                }
            }
        }

        private void OTSSocket_OnError(object sender, nsoftware.IPWorksSSL.IpportsErrorEventArgs e)
        {
            //listBox1.Items.Insert(0, "OTS SOCKET ERROR, " + e.ErrorCode + "," + e.Description);
            ////Added on 28-Mar-2017 - Rohan - To Reconnect on OTS Socket Error
            //CheckBoxConnectToOfficeServer.Checked = false;
            //CheckBoxConnectToOfficeServer_Click(null, null);    //Added on 18-Apr-2017 - Rohan
            //Statuslabel_OTSConnection.Image = pictureBoxRed.Image;
            ////OTSSocket.Disconnect();
            //System.Threading.Thread.Sleep(500);
            //CheckBoxConnectToOfficeServer.Checked = true;
            //CheckBoxConnectToOfficeServer_Click(null, null);
            //listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + ":- OTS SOCKET RECONNECT ATTEMPTED!");
            ////Added on 28-Mar-2017 - Rohan - To Reconnect on OTS Socket Error



            listBox1.Items.Insert(0, "OTS SOCKET ERROR, " + e.ErrorCode + "," + e.Description);
            //Added on 28-Mar-2017 - Rohan - To Reconnect on OTS Socket Error
            CheckBoxConnectToOfficeServer.Checked = false;
            CheckBoxConnectToOfficeServer_Click(null, null);    //Added on 18-Apr-2017 - Rohan
            Statuslabel_OTSConnection.Image = pictureBoxRed.Image;
            OTSSocket.Disconnect();
            
            CheckBoxConnectToOfficeServer.Checked = true;
            CheckBoxConnectToOfficeServer_Click(null, null);
            listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + ":- OTS SOCKET Reconnect Attempt");
            //Added on 28-Mar-2017 - Rohan - To Reconnect on OTS Socket Error
        }
        //-----------

        private void SSLServer_OnConnectionRequest(object sender, nsoftware.IPWorksSSL.IpdaemonsConnectionRequestEventArgs e)
        {
            if (SSLServer.Connections.Count >= MaxConnections)
                e.Accept = false;
        }

        private void SSLServer_OnConnected(object sender, nsoftware.IPWorksSSL.IpdaemonsConnectedEventArgs e)
        {
            try
            {
                ushort Pos;
                string str;
                TClientConnectionToITS Client;

                if (e.StatusCode != 0)
                {
                    listBox1.Items.Insert(0, "Failure to connect. Reason : " + e.Description + "," + e.StatusCode + "," + DateTime.Now + ".\r\n");
                    return;
                }

                str = DateTime.Now.ToString("hh:mm:ss") + " - New conn ConnId = " + e.ConnectionId;
                if (DisconnPosList.Count() > 0)
                {
                    Pos = DisconnPosList[0];
                    DisconnPosList.RemoveAt(0);

                    str += " Reused SocNo = " + Pos;
                }
                else
                {
                    LoginCnt++;
                    Pos = LoginCnt;

                    str += " Soc No = " + Pos;
                }

                if (Chkbox_WriteActivityToDisplay.Checked)
                    listBox1.Items.Insert(0, str);

                Client = new TClientConnectionToITS(Pos, e.ConnectionId, SSLServer.Connections[e.ConnectionId].RemoteHost, LocalMachineIP, 1024, true);
                ClientsList.Add(e.ConnectionId, Client);
            }
            catch (Exception ex)
            {
                Program.ErrorLog(ex);
            }
        }

        private void SSLServer_OnDisconnected(object sender, nsoftware.IPWorksSSL.IpdaemonsDisconnectedEventArgs e)
        {
            TClientConnectionToITS Client;

            if (e.StatusCode != 0) return;

            Client = ClientsList[e.ConnectionId] as TClientConnectionToITS;

            if (Client == null)
            {
                listBox1.Items.Insert(0, "User not found, ConnId = " + e.ConnectionId);
                return;
            }

            if ((chkbox_SendLogoutsToOTS.Checked) && (Client.FEUserID != ""))  //Send Logouts Request To OTS            
                if ((Client.Is2FA != true))
                {
                    SendLogoutMsgToOTS(Client);
                }
                else
                {
                    SendLogoutMsgToOTS_2FA(Client);
                }

            if (Chkbox_WriteActivityToDisplay.Checked)
                listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + " - User Disconnected with ConnId = " + Client.ConnectionID + " Soc No " + Client.SockNum);

            HandleDisconnection(Client);
        }

        private void HandleDisconnection(TClientConnectionToITS Client)
        {
            string str;

            Client.DisconnTime = DateTime.Now;

            str = "\n" + Client.SockNum + ","
                       + Client.ConnectionID + ","
                       + Client.FEUserID + ","
                       + Client.ClientIP + ","
                       + Client.ServerIP + ","
                       + Client.ITS_IP + ","
                       + Client.ConnTime + ","
                       + Client.DisconnTime + ","
                       + Client.Version;

            File.AppendAllText(ReportsFilePathName + SessionFName, str);

            DisconnPosList.Add(Client.SockNum);
            DisconnPosList.Sort();

            ClientsList.Remove(Client.ConnectionID);
        }

        private void SendLogoutMsgToOTS(TClientConnectionToITS Client)
        {
            TIARequestHeaderRecord IAReqHdr;
            TLogoutRequestRecord LogoutRec;
            List<byte> bToSend;

            bToSend = new List<byte>();
            IAReqHdr.AccountType = 0;
            IAReqHdr.ChannelID_BC = 0;
            IAReqHdr.ChannelID_NC = 0;
            IAReqHdr.ChannelID_ND = 0;

            IAReqHdr.SocketNum = Client.SockNum;
            if (Client.TraderID == "")
            {
                IAReqHdr.TraderID = Client.FEUserID.PadRight(14).ToCharArray();
                IAReqHdr.TraderLength = (byte)Client.FEUserID.Length;
            }
            else
            {
                IAReqHdr.TraderID = Client.TraderID.PadRight(14).ToCharArray();
                IAReqHdr.TraderLength = (byte)Client.TraderID.Length;
            }
            IAReqHdr.FEUserID = Client.FEUserID.PadRight(15).ToCharArray();
            IAReqHdr.FELength = (byte)Client.FEUserID.Length;
            IAReqHdr.ClientType = Client.FEUserType;
            IAReqHdr.RequestType = ConstantUnits.REQ_ITS_TO_OTS_CLIENT_DISCONN;
            IAReqHdr.RequestLen = ConstantUnits.TLogoutRequestRecordSize;
            bToSend.AddRange(IAReqHdr.ToByteArray());

            LogoutRec.ClientIP = Client.ClientIP.PadRight(15).ToCharArray();
            LogoutRec.ClientIPLength = (byte)Client.ClientIP.Length;
            LogoutRec.ServerIP = Client.ServerIP.PadRight(15).ToCharArray();
            LogoutRec.ServerIPLength = Client.ServerIPLength;
            LogoutRec.ITS_IP = Client.ITS_IP.PadRight(15).ToCharArray();
            LogoutRec.ITSIPLength = (byte)Client.ITS_IPLength;
            LogoutRec.MachineID = Client.MachineID.PadRight(15).ToCharArray();
            LogoutRec.MachineIDLength = (byte)Client.MachineID.Length;
            bToSend.AddRange(LogoutRec.ToByteArray());

            if (OTSSocket.Connected)
                OTSSocket.Send(bToSend.ToArray());
        }
        private void SendLogoutMsgToOTS_2FA(TClientConnectionToITS Client)
        {
            TIARequestHeaderRecord IAReqHdr;
            TLogoutRequestRecord_2FA LogoutRec_2FA;
            List<byte> bToSend;

            bToSend = new List<byte>();
            IAReqHdr.AccountType = 0;
            IAReqHdr.ChannelID_BC = 0;
            IAReqHdr.ChannelID_NC = 0;
            IAReqHdr.ChannelID_ND = 0;

            IAReqHdr.SocketNum = Client.SockNum;
            if (Client.TraderID == "")
            {
                IAReqHdr.TraderID = Client.FEUserID.PadRight(14).ToCharArray();
                IAReqHdr.TraderLength = (byte)Client.FEUserID.Length;
            }
            else
            {
                IAReqHdr.TraderID = Client.TraderID.PadRight(14).ToCharArray();
                IAReqHdr.TraderLength = (byte)Client.TraderID.Length;
            }
            IAReqHdr.FEUserID = Client.FEUserID.PadRight(15).ToCharArray();
            IAReqHdr.FELength = (byte)Client.FEUserID.Length;
            IAReqHdr.ClientType = Client.FEUserType;
            IAReqHdr.RequestType = ConstantUnits.REQ_ITS_TO_OTS_CLIENT_DISCONN_2FA;
            IAReqHdr.RequestLen = ConstantUnits.TLogoutRequestRecordSize_2FA;
            bToSend.AddRange(IAReqHdr.ToByteArray());

            LogoutRec_2FA.ClientIP = Client.ClientIP.PadRight(15).ToCharArray();
            LogoutRec_2FA.ClientIPLength = (byte)Client.ClientIP.Length;
            LogoutRec_2FA.ServerIP = Client.ServerIP.PadRight(15).ToCharArray();
            LogoutRec_2FA.ServerIPLength = Client.ServerIPLength;
            LogoutRec_2FA.ITS_IP = Client.ITS_IP.PadRight(15).ToCharArray();
            LogoutRec_2FA.ITSIPLength = (byte)Client.ITS_IPLength;
            LogoutRec_2FA.MachineID = Client.MachineID.PadRight(15).ToCharArray();
            LogoutRec_2FA.MachineIDLength = (byte)Client.MachineID.Length;
            bToSend.AddRange(LogoutRec_2FA.ToByteArray());

            if (OTSSocket.Connected)
                OTSSocket.Send(bToSend.ToArray());
        }
        private void SSLServer_OnDataIn(object sender, nsoftware.IPWorksSSL.IpdaemonsDataInEventArgs e)
        {
            try
            {
                TClientConnectionToITS Client;

                Client = ClientsList[e.ConnectionId] as TClientConnectionToITS;
                if (Client == null) return;

                LastRcvdFromClientTime = DateTime.Now;

                Client.Buff.WriteAndExtend(e.TextB, e.TextB.Length);
                ProcessOTSRequest(Client);
            }
            catch (Exception ex)
            {
                Program.ErrorLog(ex);
            }
        }

        private void ProcessOTSRequest(TClientConnectionToITS Client)
         {
            byte[] buffToRead;
            List<byte> bToSend;
            TIAReplyHeaderRecord IAHdr;
            TLoginReplyRecord LoginReplyRec;
            TIAErrorMessageRecord IAError;
            TIARequestHeaderRecord ReqHdr;
            TAllRequestNewRecord AllReq;

            while (Client.Buff.BufUsed >= ConstantUnits.TIARequestHeaderRecordSize)
            {
                buffToRead = new byte[ConstantUnits.TIARequestHeaderRecordSize];
                Client.Buff.Peek(buffToRead, ConstantUnits.TIARequestHeaderRecordSize);
                ReqHdr = buffToRead.ToStructure<TIARequestHeaderRecord>();
                if (Client.Buff.BufUsed < ConstantUnits.TIARequestHeaderRecordSize + ReqHdr.RequestLen) break;
                Client.Buff.Read(buffToRead, ConstantUnits.TIARequestHeaderRecordSize);

                buffToRead = new byte[ReqHdr.RequestLen];
                Client.Buff.Read(buffToRead, ReqHdr.RequestLen);
                //AllReq = buffToRead.ToAllRequestsStruct(ReqHdr.RequestType);

                ReqHdr.SocketNum = Client.SockNum;
                  
                if (ReqHdr.RequestType == ConstantUnits.REQ_FE_TO_ITS_HANDSHAKE)
                {
                    Client.HandShakeRecvdTime = DateTime.Now;
                    SendBackHandshake(Client, ReqHdr);
                    continue;
                }

                if (ReqHdr.RequestType == 85)
                    Log2FA(ReqHdr.toJsonString());

                if ((ReqHdr.RequestType == ConstantUnits.REP_ITS_TO_FE_RPT_T2T_SCRIPS && flAllRepT2TAssigned) ||
                   (ReqHdr.RequestType == ConstantUnits.REP_ITS_TO_FE_RPT_BSE_NSE_MAP && flAllRepBNMapAssigned) ||
                   (ReqHdr.RequestType == ConstantUnits.REP_ITS_TO_FE_RPT_BANNED_SCRIPS_DERIV && flAllRepBANDerivAssigned) ||
                   (ReqHdr.RequestType == ConstantUnits.REP_ITS_TO_FE_RPT_ILLIQUID_SCRIPS && flAllRepIlliqAssigned) ||
                    (ReqHdr.RequestType == ConstantUnits.REP_ITS_TO_FE_TRADERLIST) || (ReqHdr.RequestType == 248))
                {
                    bToSend = new List<byte>();

                    IAHdr.AccountType = 0;
                    IAHdr.ChannelID_BC = 0;
                    IAHdr.ChannelID_NC = 0;
                    IAHdr.ChannelID_ND = 0;
                    IAHdr.SocketNum = Client.SockNum;
                    IAHdr.TraderID = ReqHdr.TraderID;
                    IAHdr.TraderLength = ReqHdr.TraderLength;
                    IAHdr.FEUserID = ReqHdr.FEUserID;
                    IAHdr.FELength = ReqHdr.FELength;
                    IAHdr.ClientType = ReqHdr.ClientType;
                    IAHdr.ReplyType = ReqHdr.RequestType;
                    IAHdr.ErrorCode = 0;

                    switch (ReqHdr.RequestType)
                    {
                        case ConstantUnits.REP_ITS_TO_FE_RPT_T2T_SCRIPS:
                            {
                                IAHdr.ReplyLen = ConstantUnits.TTradeToTradeReplyRecordSize * AllRepT2T.Length;
                                IAHdr.ReplyRecSize = ConstantUnits.TTradeToTradeReplyRecordSize;
                                IAHdr.ReplyCnt = Convert.ToUInt16(AllRepT2T.Length);

                                bToSend.AddRange(IAHdr.ToByteArray());

                                for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    bToSend.AddRange(AllRepT2T[i].TradeToTradeReplyRec.ToByteArray());
                            }
                            break;

                        case ConstantUnits.REP_ITS_TO_FE_RPT_BSE_NSE_MAP:
                            {
                                IAHdr.ReplyLen = ConstantUnits.TBseNseMapRecordRecordSize * AllRepBseNseMap.Length;
                                IAHdr.ReplyRecSize = ConstantUnits.TBseNseMapRecordRecordSize;
                                IAHdr.ReplyCnt = Convert.ToUInt16(AllRepBseNseMap.Length);

                                bToSend.AddRange(IAHdr.ToByteArray());

                                for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    bToSend.AddRange(AllRepBseNseMap[i].BseNseMapReplyRec.ToByteArray());
                            }
                            break;

                        case ConstantUnits.REP_ITS_TO_FE_RPT_BANNED_SCRIPS_DERIV:
                            {
                                IAHdr.ReplyLen = ConstantUnits.TDerivBannedScripReplyRecordSize * AllRepDerivBanScrip.Length;
                                IAHdr.ReplyRecSize = ConstantUnits.TDerivBannedScripReplyRecordSize;
                                IAHdr.ReplyCnt = Convert.ToUInt16(AllRepDerivBanScrip.Length);

                                bToSend.AddRange(IAHdr.ToByteArray());

                                for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    bToSend.AddRange(AllRepDerivBanScrip[i].DerivBannedScripReplyRec.ToByteArray());
                            }
                            break;

                        case ConstantUnits.REP_ITS_TO_FE_RPT_ILLIQUID_SCRIPS:
                            {
                                IAHdr.ReplyLen = ConstantUnits.TIlliquidReplyRecordSize * AllRepIlliquid.Length;
                                IAHdr.ReplyRecSize = ConstantUnits.TIlliquidReplyRecordSize;
                                IAHdr.ReplyCnt = Convert.ToUInt16(AllRepIlliquid.Length);

                                bToSend.AddRange(IAHdr.ToByteArray());

                                for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    bToSend.AddRange(AllRepIlliquid[i].IlliquidReplyRec.ToByteArray());
                            }
                            break;
                        case 248:
                            {
                                IAHdr.ReplyLen = ConstantUnits.TISegmentReplyRecordSize * AllRepSegment.Length;
                                IAHdr.ReplyRecSize = ConstantUnits.TISegmentReplyRecordSize;
                                IAHdr.ReplyCnt = Convert.ToUInt16(AllRepSegment.Length);

                                bToSend.AddRange(IAHdr.ToByteArray());

                                for (int i = 0; i < IAHdr.ReplyCnt; i++)
                                    bToSend.AddRange(AllRepSegment[i].SegmentReply.ToByteArray());
                            }
                            break;
                        case ConstantUnits.REP_ITS_TO_FE_TRADERLIST:
                            {
                                Client.ClientArray = new string[0]; //04.04.2018 PRANOTI
                            }
                            break;

                        default:
                            break;
                    }

                    if (bToSend.Count > 0)
                    {
                        if (Chkbox_WriteActivityToDisplay.Checked)
                        {
                            if (Client.FEUserID == txtDisplayClid.Text.Trim())
                            {
                                string str1 = DateTime.Now.ToString("hh:mm:ss") + " - STC - Rep type: " + IAHdr.ReplyType + ", name:" + Client.FEUserID + ", Sock:" + IAHdr.SocketNum;
                                listBox1.Items.Insert(0, str1);
                            }
                        }

                        LastSentToClientTime = DateTime.Now;
                        SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
                        continue;
                    }
                }

                if (ReqHdr.RequestType == ConstantUnits.REQ_FE_TO_ITS_LOGIN ||
                    ReqHdr.RequestType == ConstantUnits.REQ_FE_TO_ITS_LOGIN_NEW ||
                    ReqHdr.RequestType == ConstantUnits.REQ_FE_TO_ITS_LOGIN_NEW_TwoFactor ||
                    ReqHdr.RequestType == ConstantUnits.REQ_FE_TO_ITS_CHANGE_PASS ||
                    ReqHdr.RequestType == ConstantUnits.REQ_FE_TO_ITS_VARIFY_TRADER)
                {
                    AllReq = buffToRead.ToAllRequestsStruct(ReqHdr.RequestType);
                    switch (ReqHdr.RequestType)
                    {
                        case ConstantUnits.REQ_FE_TO_ITS_LOGIN:
                        case ConstantUnits.REQ_FE_TO_ITS_LOGIN_NEW:
                            {
                                if (chkbox_AllowTradeVersion.Checked)
                                {
                                    //string verLogin = new string(AllReq.LoginRequest.Version, 0, AllReq.LoginRequest.VersionLength);
                                    //string TradeVersion = Txtbox_Trade_Version.Text;
                                    //int val = (verLogin).CompareTo(TradeVersion);

                                    string verLogin = new string(AllReq.LoginRequest.Version, 0, AllReq.LoginRequest.VersionLength).Replace(".", "");
                                    string TradeVersion = Txtbox_Trade_Version.Text.Replace(".", "");
                                    Int64 verLoginInt = Convert.ToInt64(verLogin);
                                    Int64 TradeVersionInt = Convert.ToInt64(TradeVersion);

                                    if (TradeVersionInt > verLoginInt)  //if (val == -1) // left < Right then returns -1
                                    {
                                        bToSend = new List<byte>();
                                        IAHdr = new TIAReplyHeaderRecord();
                                        LoginReplyRec = new TLoginReplyRecord();

                                        //send error message back and disconnect.
                                        IAHdr.SocketNum = Client.SockNum;

                                        if (ReqHdr.TraderLength <= 0)
                                        {
                                            IAHdr.TraderID = ReqHdr.FEUserID;
                                            IAHdr.TraderLength = ReqHdr.FELength;
                                        }
                                        else
                                        {
                                            IAHdr.TraderID = ReqHdr.TraderID;
                                            IAHdr.TraderLength = ReqHdr.TraderLength;
                                        }
                                        IAHdr.FEUserID = ReqHdr.FEUserID;
                                        IAHdr.FELength = ReqHdr.FELength;
                                        IAHdr.ClientType = ReqHdr.ClientType;
                                        IAHdr.ReplyType = ReqHdr.RequestType;
                                        IAHdr.ReplyLen = ConstantUnits.TLoginReplyRecordSize;
                                        IAHdr.ReplyRecSize = ConstantUnits.TLoginReplyRecordSize;
                                        IAHdr.ReplyCnt = 1;
                                        IAHdr.ErrorCode = 10000;
                                        bToSend.AddRange(IAHdr.ToByteArray());

                                        IAError.ErrorStr = Txtbox_OldVerMsg.Text.PadRight(200).ToCharArray();
                                        IAError.Errorlength = (byte)Txtbox_OldVerMsg.Text.TrimEnd().Length;
                                        bToSend.AddRange(IAError.ToByteArray());

                                        LoginReplyRec.BadLoginCnt = 3;
                                        bToSend.AddRange(LoginReplyRec.ToByteArray());

                                        SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
                                        return;
                                    }
                                }
                                //9.6.14
                                if (chk_SendToParticularClient.Checked)
                                {
                                    if (NewPointerCList.Contains(new string(ReqHdr.FEUserID, 0, ReqHdr.FELength)))
                                    {
                                        string verLogin = new string(AllReq.LoginRequest.Version, 0, AllReq.LoginRequest.VersionLength).Replace(".", "");
                                        string TradeVersion = Txtbox_Trade_Version2.Text.Replace(".", "");
                                        Int64 verLoginInt = Convert.ToInt64(verLogin);
                                        Int64 TradeVersionInt = Convert.ToInt64(TradeVersion);

                                        if (TradeVersionInt > verLoginInt)  //if (val == -1) // left < Right then returns -1
                                        {
                                            bToSend = new List<byte>();
                                            IAHdr = new TIAReplyHeaderRecord();
                                            LoginReplyRec = new TLoginReplyRecord();

                                            //send error message back and disconnect.
                                            IAHdr.SocketNum = Client.SockNum;

                                            if (ReqHdr.TraderLength <= 0)
                                            {
                                                IAHdr.TraderID = ReqHdr.FEUserID;
                                                IAHdr.TraderLength = ReqHdr.FELength;
                                            }
                                            else
                                            {
                                                IAHdr.TraderID = ReqHdr.TraderID;
                                                IAHdr.TraderLength = ReqHdr.TraderLength;
                                            }
                                            IAHdr.FEUserID = ReqHdr.FEUserID;
                                            IAHdr.FELength = ReqHdr.FELength;
                                            IAHdr.ClientType = ReqHdr.ClientType;
                                            IAHdr.ReplyType = ReqHdr.RequestType;
                                            IAHdr.ReplyLen = ConstantUnits.TLoginReplyRecordSize;
                                            IAHdr.ReplyRecSize = ConstantUnits.TLoginReplyRecordSize;
                                            IAHdr.ReplyCnt = 1;
                                            IAHdr.ErrorCode = 10000;
                                            bToSend.AddRange(IAHdr.ToByteArray());

                                            IAError.ErrorStr = Txtbox_OldVerMsg.Text.PadRight(200).ToCharArray();
                                            IAError.Errorlength = (byte)Txtbox_OldVerMsg.Text.TrimEnd().Length;
                                            bToSend.AddRange(IAError.ToByteArray());

                                            LoginReplyRec.BadLoginCnt = 3;
                                            bToSend.AddRange(LoginReplyRec.ToByteArray());

                                            SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
                                            return;
                                        }
                                    }
                                }

                                AllReq.LoginRequest.ServerIPLen = (byte)Client.ServerIP.Length;
                                AllReq.LoginRequest.ServerIP = Client.ServerIP.PadRight(15).ToCharArray();
                                AllReq.LoginRequest.ITS_IPLen = (byte)Client.ITS_IP.Length;
                                AllReq.LoginRequest.ITS_IP = Client.ITS_IP.PadRight(15).ToCharArray();

                                Client.ConnTime = DateTime.Now;
                                Client.FEUserID = new string(AllReq.LoginRequest.FEUserID, 0, AllReq.LoginRequest.FEIDLength);
                                Client.FEUserPassword = new string(AllReq.LoginRequest.Password1, 0, AllReq.LoginRequest.Password1Length);
                                Client.FEUserPassword2 = new string(AllReq.LoginRequest.Password2, 0, AllReq.LoginRequest.Password2Length);
                                Client.ServerIP = new string(AllReq.LoginRequest.ServerIP, 0, AllReq.LoginRequest.ServerIPLen);
                                Client.ITS_IP = new string(AllReq.LoginRequest.ITS_IP, 0, AllReq.LoginRequest.ITS_IPLen);
                                Client.ClientIP = new string(AllReq.LoginRequest.ClientIP, 0, AllReq.LoginRequest.ClientIPLen);
                                Client.MachineID = new string(AllReq.LoginRequest.MachineID, 0, AllReq.LoginRequest.MachineIDLen);
                                Client.Version = new string(AllReq.LoginRequest.Version, 0, AllReq.LoginRequest.VersionLength);
                            } break;
                        case ConstantUnits.REQ_FE_TO_ITS_LOGIN_NEW_TwoFactor:
                            {
                                if (logEnable2FA)
                                    Log2FA(AllReq.LoginRequestTwoFactor.toJsonString());
                                if (chkbox_AllowTradeVersion.Checked)
                                {
                                    //string verLogin = new string(AllReq.LoginRequestTwoFactor.Version, 0, AllReq.LoginRequestTwoFactor.VersionLength);
                                    //string TradeVersion = Txtbox_Trade_Version.Text;
                                    //int val = (verLogin).CompareTo(TradeVersion);

                                    string verLogin = new string(AllReq.LoginRequestTwoFactor.Version, 0, AllReq.LoginRequestTwoFactor.VersionLength).Replace(".", "");
                                    string TradeVersion = Txtbox_Trade_Version.Text.Replace(".", "");
                                    Int64 verLoginInt = Convert.ToInt64(verLogin);
                                    //Int64 TradeVersionInt = Convert.ToInt64(TradeVersion);

                                    //if (TradeVersionInt > verLoginInt)  //if (val == -1) // left < Right then returns -1
                                    List<string> AllowedVersion = Txtbox_Trade_Version.Text.Replace(".", "").Split(',').ToList();
                                    if (!AllowedVersion.Contains(verLogin))
                                    {
                                        bToSend = new List<byte>();
                                        IAHdr = new TIAReplyHeaderRecord();
                                        LoginReplyRec = new TLoginReplyRecord();

                                        //send error message back and disconnect.
                                        IAHdr.SocketNum = Client.SockNum;

                                        if (ReqHdr.TraderLength <= 0)
                                        {
                                            IAHdr.TraderID = ReqHdr.FEUserID;
                                            IAHdr.TraderLength = ReqHdr.FELength;
                                        }
                                        else
                                        {
                                            IAHdr.TraderID = ReqHdr.TraderID;
                                            IAHdr.TraderLength = ReqHdr.TraderLength;
                                        }
                                        IAHdr.FEUserID = ReqHdr.FEUserID;
                                        IAHdr.FELength = ReqHdr.FELength;
                                        IAHdr.ClientType = ReqHdr.ClientType;
                                        IAHdr.ReplyType = ReqHdr.RequestType;
                                        IAHdr.ReplyLen = ConstantUnits.TLoginReplyRecordSize;
                                        IAHdr.ReplyRecSize = ConstantUnits.TLoginReplyRecordSize;
                                        IAHdr.ReplyCnt = 1;
                                        IAHdr.ErrorCode = 10000;
                                        bToSend.AddRange(IAHdr.ToByteArray());

                                        IAError.ErrorStr = Txtbox_OldVerMsg.Text.PadRight(200).ToCharArray();
                                        IAError.Errorlength = (byte)Txtbox_OldVerMsg.Text.TrimEnd().Length;
                                        bToSend.AddRange(IAError.ToByteArray());

                                        LoginReplyRec.BadLoginCnt = 3;
                                        bToSend.AddRange(LoginReplyRec.ToByteArray());

                                        SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
                                        return;
                                    }
                                }
                                //9.6.14
                                if (chk_SendToParticularClient.Checked)
                                {
                                    if (NewPointerCList.Contains(new string(ReqHdr.FEUserID, 0, ReqHdr.FELength)))
                                    {
                                        string verLogin = new string(AllReq.LoginRequestTwoFactor.Version, 0, AllReq.LoginRequestTwoFactor.VersionLength).Replace(".", "");
                                        string TradeVersion = Txtbox_Trade_Version2.Text.Replace(".", "");
                                        Int64 verLoginInt = Convert.ToInt64(verLogin);
                                        Int64 TradeVersionInt = Convert.ToInt64(TradeVersion);

                                        //if (TradeVersionInt > verLoginInt)  //if (val == -1) // left < Right then returns -1
                                        if (TradeVersionInt != verLoginInt)
                                        {
                                            bToSend = new List<byte>();
                                            IAHdr = new TIAReplyHeaderRecord();
                                            LoginReplyRec = new TLoginReplyRecord();

                                            //send error message back and disconnect.
                                            IAHdr.SocketNum = Client.SockNum;

                                            if (ReqHdr.TraderLength <= 0)
                                            {
                                                IAHdr.TraderID = ReqHdr.FEUserID;
                                                IAHdr.TraderLength = ReqHdr.FELength;
                                            }
                                            else
                                            {
                                                IAHdr.TraderID = ReqHdr.TraderID;
                                                IAHdr.TraderLength = ReqHdr.TraderLength;
                                            }
                                            IAHdr.FEUserID = ReqHdr.FEUserID;
                                            IAHdr.FELength = ReqHdr.FELength;
                                            IAHdr.ClientType = ReqHdr.ClientType;
                                            IAHdr.ReplyType = ReqHdr.RequestType;
                                            IAHdr.ReplyLen = ConstantUnits.TLoginReplyRecordSize;
                                            IAHdr.ReplyRecSize = ConstantUnits.TLoginReplyRecordSize;
                                            IAHdr.ReplyCnt = 1;
                                            IAHdr.ErrorCode = 10000;
                                            bToSend.AddRange(IAHdr.ToByteArray());

                                            IAError.ErrorStr = Txtbox_OldVerMsg.Text.PadRight(200).ToCharArray();
                                            IAError.Errorlength = (byte)Txtbox_OldVerMsg.Text.TrimEnd().Length;
                                            bToSend.AddRange(IAError.ToByteArray());

                                            LoginReplyRec.BadLoginCnt = 3;
                                            bToSend.AddRange(LoginReplyRec.ToByteArray());

                                            SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
                                            return;
                                        }
                                    }
                                }

                                AllReq.LoginRequestTwoFactor.ServerIPLength = (byte)Client.ServerIP.Length;
                                AllReq.LoginRequestTwoFactor.ServerIP = Client.ServerIP.PadRight(15).ToCharArray();
                                AllReq.LoginRequestTwoFactor.ITS_IPLength = (byte)Client.ITS_IP.Length;
                                AllReq.LoginRequestTwoFactor.ITS_IP = Client.ITS_IP.PadRight(15).ToCharArray();

                                Client.ConnTime = DateTime.Now;
                                Client.FEUserID = new string(AllReq.LoginRequestTwoFactor.FEUserID, 0, AllReq.LoginRequestTwoFactor.FEUserIDLength);
                                Client.FEUserPassword = new string(AllReq.LoginRequestTwoFactor.Password1, 0, AllReq.LoginRequestTwoFactor.Password1Length);
                                //Client.FEUserPassword2 = new string(AllReq.LoginRequestTwoFactor.Password2, 0, AllReq.LoginRequestTwoFactor.Password2Length);
                                Client.ServerIP = new string(AllReq.LoginRequestTwoFactor.ServerIP, 0, AllReq.LoginRequestTwoFactor.ServerIPLength);
                                Client.ITS_IP = new string(AllReq.LoginRequestTwoFactor.ITS_IP, 0, AllReq.LoginRequestTwoFactor.ITS_IPLength);
                                Client.ClientIP = new string(AllReq.LoginRequestTwoFactor.ClientIP, 0, AllReq.LoginRequestTwoFactor.ClientIPLength);
                                Client.MachineID = new string(AllReq.LoginRequestTwoFactor.DeviceID, 0, AllReq.LoginRequestTwoFactor.DeviceIDLength);
                                Client.Version = new string(AllReq.LoginRequestTwoFactor.Version, 0, AllReq.LoginRequestTwoFactor.VersionLength);
                            }
                            break;
                        case ConstantUnits.REQ_FE_TO_ITS_CHANGE_PASS:
                            {
                                AllReq.ChgPswdRequestRec.ServerIPLen = (byte)Client.ServerIP.Length;
                                AllReq.ChgPswdRequestRec.ServerIP = Client.ServerIP.PadRight(15).ToCharArray();
                                AllReq.ChgPswdRequestRec.ITS_IPLen = (byte)Client.ITS_IP.Length;
                                AllReq.ChgPswdRequestRec.ITS_IP = Client.ITS_IP.PadRight(15).ToCharArray();

                            } break;
                        case ConstantUnits.REQ_FE_TO_ITS_VARIFY_TRADER:
                            {
                                AllReq.VerifyTraderRequestRec.ServerIPLength = (byte)Client.ServerIP.Length;
                                AllReq.VerifyTraderRequestRec.ServerIP = Client.ServerIP.PadRight(15).ToCharArray();
                                AllReq.VerifyTraderRequestRec.ITS_IPLength = (byte)Client.ITS_IP.Length;
                                AllReq.VerifyTraderRequestRec.ITS_IP = Client.ITS_IP.PadRight(15).ToCharArray();
                            } break;
                    }
                    buffToRead = AllReq.ToAllRequestsArray(ReqHdr.RequestType);
                }


                if (ReqHdr.RequestType == 246)
                {
                    if (ReqHdr.ChannelID_ND == 501)
                    {
                        AllReq = buffToRead.ToAllRequestsStruct(ReqHdr.RequestType);
                        Client.FEUserType = 46;
                        Client.ClientCnt = 1;
                        Client.ConnTime = DateTime.Now;
                        Client.FEUserID = new string(AllReq.LoginRequest.FEUserID, 0, AllReq.LoginRequest.FEIDLength);
                        Client.FEUserPassword = new string(AllReq.LoginRequest.Password1, 0, AllReq.LoginRequest.Password1Length);
                        Client.FEUserPassword2 = new string(AllReq.LoginRequest.Password2, 0, AllReq.LoginRequest.Password2Length);
                        Client.ServerIP = new string(AllReq.LoginRequest.ServerIP, 0, AllReq.LoginRequest.ServerIPLen);
                        Client.ITS_IP = new string(AllReq.LoginRequest.ITS_IP, 0, AllReq.LoginRequest.ITS_IPLen);
                        Client.ClientIP = new string(AllReq.LoginRequest.ClientIP, 0, AllReq.LoginRequest.ClientIPLen);
                        Client.MachineID = new string(AllReq.LoginRequest.MachineID, 0, AllReq.LoginRequest.MachineIDLen);
                        Client.Version = new string(AllReq.LoginRequest.Version, 0, AllReq.LoginRequest.VersionLength);

                        Array.Resize(ref Client.ClientArray, Client.ClientArray.Length + 1);
                        Client.ClientArray[Client.ClientArray.Length - 1] = "IIS";
                    }
                }

                bToSend = new List<byte>();
                bToSend.AddRange(ReqHdr.ToByteArray());
                bToSend.AddRange(buffToRead);

                if (Chkbox_WriteActivityToDisplay.Checked)
                {
                    if (Client.FEUserID == txtDisplayClid.Text.Trim())
                    {
                        string str = DateTime.Now.ToString("hh:mm:ss") + " - STO - Req ype: " + ReqHdr.RequestType + ", name:" + Client.FEUserID + ", Sock:" + ReqHdr.SocketNum;
                        listBox1.Items.Insert(0, str);
                    }
                }

                if (OTSSocket.Connected)
                {
                    // ---Log --------------------------------------------------------------
                    if (chkSettingsWriteReply.Checked)
                    {
                        List<byte> bToWrite;
                        TGlobalHeader GHdr;

                        GHdr.DateTime = DateTime.Now;
                        GHdr.Size = bToSend.Count;
                        GHdr.SentFrom = (byte)'O';

                        bToWrite = new List<byte>();
                        bToWrite.AddRange(GHdr.ToByteArray());
                        bToWrite.AddRange(bToSend.ToArray());

                        ITS_REQ_DatFileStream.Write(bToWrite.ToArray(), 0, bToWrite.Count);
                    }
                    // ---Log --------------------------------------------------------------

                    LastSentToOTSTime = DateTime.Now;
                    OTSSocket.Send(bToSend.ToArray());
                }
                else
                    UnableToProcessRequest(Client, ReqHdr);
            }
        }

        private void UnableToProcessRequest(TClientConnectionToITS Client, TIARequestHeaderRecord ReqHdr)
        {
            TIAReplyHeaderRecord IAHdr;
            TLinkFailedRecord LinkFailRec;
            List<byte> bToSend;

            bToSend = new List<byte>();
            IAHdr.SocketNum = Client.SockNum;
            IAHdr.TraderID = ReqHdr.TraderID;
            IAHdr.TraderLength = ReqHdr.TraderLength;
            IAHdr.FELength = ReqHdr.FELength;
            IAHdr.FEUserID = ReqHdr.FEUserID;
            IAHdr.ClientType = ReqHdr.ClientType;
            IAHdr.ReplyType = ConstantUnits.REP_ITS_TO_FE_LINK_FAIL;
            IAHdr.ReplyLen = ConstantUnits.TLinkFailedRecordSize;
            IAHdr.ReplyRecSize = (ushort)IAHdr.ReplyLen;
            IAHdr.ReplyCnt = 1;
            IAHdr.ErrorCode = 0;
            IAHdr.AccountType = 0;
            IAHdr.ChannelID_BC = 0;
            IAHdr.ChannelID_NC = 0;
            IAHdr.ChannelID_ND = 0;
            bToSend.AddRange(IAHdr.ToByteArray());

            LinkFailRec.RequestType = ReqHdr.RequestType;
            LinkFailRec.TraderRequestID = 0;
            LinkFailRec.BrokerOrderID = 0;
            LinkFailRec.ErrorLevel = 20;
            LinkFailRec.ErrorMsg = ("ITS to OTS Link is down.").PadRight(100).ToCharArray();
            LinkFailRec.ErrorMsgLength = Convert.ToByte(("ITS to OTS Link is down.").Length);
            #region Comment
            //switch (ReqHdr.RequestType)
            //{
            //    case 10:
            //        {
            //            LinkFailRec.TraderRequestID = AllReq.EquityOrderPlaceRequestRec.TraderRequestID;

            //        } break;
            //    case 20:
            //        {
            //            LinkFailRec.TraderRequestID = AllReq.EquityOrderModifyRequestRec.TraderRequestID;

            //        } break;
            //    case 30:
            //        {
            //            LinkFailRec.TraderRequestID = AllReq.EquityOrderCancelRequestRec.TraderRequestID;

            //        } break;
            //    case 110:
            //        {
            //            LinkFailRec.TraderRequestID = AllReq.DerivOrderPlaceRequestRec.TraderRequestID;

            //        } break;
            //    case 120:
            //        {
            //            LinkFailRec.TraderRequestID = AllReq.DerivOrderModifyRequestRec.TraderRequestID;

            //        } break;
            //    case 130:
            //        {
            //            LinkFailRec.TraderRequestID = AllReq.DerivOrderCancelRequestRec.TraderRequestID;

            //        } break;
            //    case 181:
            //    case 182:
            //    case 183:
            //        {
            //            LinkFailRec.TraderRequestID = AllReq.OptionExerciseRequestRec.TraderRequestID;
            //        } break;
            //    default:
            //        {
            //            listBox1.Items.Insert(0, "In UnableToProcessRequest Default RequestType " + ReqHdr.RequestType.ToString() + " Received.");
            //        }
            //        break;
            //}
            #endregion
            bToSend.AddRange(LinkFailRec.ToByteArray());

            SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
        }

        private void SendBackHandshake(TClientConnectionToITS Client, TIARequestHeaderRecord ReqHdr)
        {
            TIAReplyHeaderRecord IAHdr;
            TDummyRecord DummyRec;
            List<byte> bToSend;

            bToSend = new List<byte>();
            IAHdr.AccountType = 0;
            IAHdr.ChannelID_BC = 0;
            IAHdr.ChannelID_NC = 0;
            IAHdr.ChannelID_ND = 0;
            IAHdr.SocketNum = Client.SockNum;
            IAHdr.TraderID = ReqHdr.TraderID;
            IAHdr.TraderLength = ReqHdr.TraderLength;
            IAHdr.FEUserID = ReqHdr.FEUserID;
            IAHdr.FELength = ReqHdr.FELength;
            IAHdr.ClientType = ReqHdr.ClientType;
            IAHdr.ReplyType = ConstantUnits.REP_ITS_TO_FE_HANDSHAKE;
            IAHdr.ReplyLen = ConstantUnits.TDummyRecordSize;
            IAHdr.ReplyRecSize = Convert.ToUInt16(IAHdr.ReplyLen);
            IAHdr.ReplyCnt = 1;
            IAHdr.ErrorCode = 0;
            bToSend.AddRange(IAHdr.ToByteArray());

            DummyRec.DummyByte = Convert.ToByte(txtSettingsITSID.Text);
            DummyRec.Dummy = ("From ITS").PadRight(15).ToCharArray();
            DummyRec.DummyLength = (byte)8;
            DummyRec.DummyInt = 0;
            bToSend.AddRange(DummyRec.ToByteArray());

            SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
        }

        private void SSLServer_OnSSLStatus(object sender, nsoftware.IPWorksSSL.IpdaemonsSSLStatusEventArgs e)
        {
            if (e.Message == "Remote end signaled end of session.")
                SSLServer.Connections[e.ConnectionId].Connected = false;
        }

        private void SSLServer_OnError(object sender, nsoftware.IPWorksSSL.IpdaemonsErrorEventArgs e)
        {
            //listBox1.Items.Insert(0, "SSL SERVER ERROR, " + e.ErrorCode + "," + e.Description);
            //if (e.ConnectionId != null)
            //    SSLServer.Connections[e.ConnectionId].Connected = false;
            ////Added on 28-Mar-2017 - Rohan - To Reconnect on OTS Socket Error
            //CheckBoxConnectToOfficeServer.Checked = false;
            //CheckBoxConnectToOfficeServer_Click(null, null);
            //Statuslabel_OTSConnection.Image = pictureBoxRed.Image;
            ////OTSSocket.Disconnect();
            //System.Threading.Thread.Sleep(500);
            //CheckBoxConnectToOfficeServer.Checked = true;
            //CheckBoxConnectToOfficeServer_Click(null, null);
            //listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + ":- OTS SOCKET RECONNECT ATTEMPTED!");
            ////Added on 28-Mar-2017 - Rohan - To Reconnect on OTS Socket Error



            listBox1.Items.Insert(0, "SSL SERVER ERROR, " + e.ErrorCode + "," + e.Description);
            if (e.ConnectionId != null)
                SSLServer.Connections[e.ConnectionId].Connected = false;         
        }
        //-----------

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime CurrTime;
            TClientConnectionToITS Client;

            TimerCnt++;
            CurrTime = DateTime.Now;

            StatusLabel_CurrentTime.Text = CurrTime.ToString("hh:mm:ss");
            StatusLabel_rfc.Text = "rfc=" + (CurrTime.Subtract(LastRcvdFromClientTime).Seconds);
            StatusLabel_stc.Text = "stc=" + (CurrTime.Subtract(LastSentToClientTime).Seconds);
            StatusLabel_sto.Text = "sto=" + (CurrTime.Subtract(LastSentToOTSTime).Seconds);
            StatusLabel_rfo.Text = "rfo=" + (CurrTime.Subtract(LastRcvdFromOTSTime).Seconds);

            StatusLabel_SSLConnCount.Text = SSLServer.Connections.Count.ToString();
            StatusLabel_PeakConnCnt.Text = ClientsList.Count.ToString();
            StatusLabel_ErrCnt.Text = ErrorCnt.ToString();
            //StatusLabel_MaxConn.Text = MaxClientConnections.ToString();
            StatusLabel_OTSRecvCnt.Text = SockBufOTSReply.BufUsed.ToString();
            StatusLabel_BadRejectionCount.Text = BadClientRejectCnt.ToString();

            if (TimerCnt % 20 == 0 && (SSLServer.Connections.Count != ClientsList.Count))
            {
                ArrayList CLs = new ArrayList(ClientsList.Values);
                for (int i = 0; i < CLs.Count; i++)
                {
                    Client = CLs[i] as TClientConnectionToITS;
                    if (Client.FEUserType == 0) continue;
                    if ((CurrTime - Client.HandShakeRecvdTime).TotalMinutes < 2) continue;
                    if (SSLServer.Connections.ContainsKey(Client.ConnectionID)) continue;

                    string str = CurrTime.ToString("hh:mm:ss") + " - Killed client :" + Client.ConnectionID + " , " + Client.FEUserID + "," + Client.HandShakeRecvdTime.ToString("hh:mm:ss");
                    listBox1.Items.Insert(0, str);
                    //SSLServer.Disconnect(Client.ConnectionID.ToString());
                    HandleDisconnection(Client);
                }
            }

            if (RetrySending.Count > 0)
            {
                TClientConnectionToITS Cli;
               
                foreach (var item in RetrySending)
                {
                    Cli = (TClientConnectionToITS)item;

                    if (!SSLServer.Connections[Cli.ConnectionID].Connected) continue;

                    try
                    {
                        SSLServer.Send(Cli.ConnectionID.ToString(), Cli.PendingData.ToArray());
                    }
                    catch (nsoftware.IPWorksSSL.IPWorksSSLException ex)
                    {
                        if (ex.Code == 135) //WOULDBLOCK Error
                        {
                            continue;
                        }
                    }
                    Cli.PendingData.Clear();
                    RetrySending.Remove(Cli);
                    if (RetrySending.Count == 0) return;
                }
            }

            if (TimerCnt % 300 == 0)
                MemoryHelper.ClearMemory();

            if ((TimerCnt % 20 == 0) &&
                (!OTSSocket.Connected) &&
                flAutoConnToOTS)
            {
                SockBufOTSReply.Clear();
                OTSSocket.Connect(txtSettingsOTSIP.Text, ConstantUnits.PORT_OTS);
            }
        }

        private void WriteToErrorLog(TIAReplyHeaderRecord IAHdr, TAllReplyNewRecord[] AllReplyArray, TIAErrorMessageRecord IAError, Exception e)
        {
            List<byte> b = new List<byte>();

            b.AddRange(IAHdr.ToByteArray());
            if (IAHdr.ErrorCode > 0)
                b.AddRange(IAError.ToByteArray());
            for (int i = 0; i < IAHdr.ReplyCnt; i++)
                b.AddRange(AllReplyArray[i].ToAllReplyArray(IAHdr.ReplyType));
            ITS_ErrorDatFileStream.Write(b.ToArray(), 0, b.Count);

            Program.ErrorLog(e);
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
        }

        private void CheckBoxListenToClients_CheckedChanged(object sender, EventArgs e)
        {
            AutoStartup = false;
            if (CheckBoxListenToClients.Checked)
            {
                txtMaxClients.Enabled = false;
                MaxConnections = Convert.ToInt32(txtMaxClients.Text);
                //SSLServer.Config("MAXCONNECTIONS = " + MaxConnections);
                SSLServer.Listening = true;
                Statuslabel_ClientsConnection.Image = pictureBoxGreen.Image;
            }
            else
            {
                txtMaxClients.Enabled = true;
                SSLServer.Listening = false;
                Statuslabel_ClientsConnection.Image = pictureBoxRed.Image;
            }
        }

        private void CheckBoxConnectToOfficeServer_CheckedChanged(object sender, EventArgs e)
        {
            AutoStartup = false;
            if (CheckBoxConnectToOfficeServer.Checked)
            {
                //06/06/2012
                //if SockBufOTSReply.GetBufUsed > 0 then
                //   begin
                //   fbname := 'ITS_Buf_' + FormatDateTime('yyyymmdd_hhmmss',now) + '.dat' ;
                //   assignfile(fb,fbname) ;
                //   rewrite(fb) ;
                //   for i := 1 to SockBufOTSReply.GetBufUsed do
                //      begin
                //      SockBufOTSReply.Read(@b,1) ;
                //      Write(fb,b) ;
                //      end;
                //   closefile(fb) ;
                //   end;

                flAutoConnToOTS = true;
                SockBufOTSReply.Clear();
                OTSSocket.Connect(txtSettingsOTSIP.Text, ConstantUnits.PORT_OTS);
            }
            else
            {
                byte[] bytes = new byte[SockBufOTSReply.BufUsed];
                SockBufOTSReply.Read(bytes, SockBufOTSReply.BufUsed);
                File.WriteAllBytes($"ITSJunk_{DateTime.Now:ddMMyy_HHmmss}.dat", bytes);
                File.WriteAllBytes($"ITSJunk_{DateTime.Now:ddMMyy_HHmmss}_H.dat", LastHeader.ToByteArray());
                OTSSocket.Disconnect();
            }
        }

        private void btnChangLog_Click(object sender, EventArgs e)
        {
            if (CL.Visible)
                CL.BringToFront();
            else
                CL.Show(this);
        }

        private void chk_SendToParticularClient_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_SendToParticularClient.Checked)
            {
                ReadClientsListFile();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (File.Exists(InputFile + "ITSMessage.txt"))
            {
                string Result = File.ReadAllText(InputFile + "ITSMessage.txt");
                txtMsg.Text = Result;
            }
        }

        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            List<byte> bToSend;
            TIAReplyHeaderRecord IAHdr2;
            TMessageStringRecord MsgRec;
            TClientConnectionToITS Client;
            ICollection<nsoftware.IPWorksSSL.Connection> ValCollection;

            DialogResult Result = MessageBox.Show("Do you want to send the message now?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (Result == DialogResult.No) return;

            bToSend = new List<byte>();
            IAHdr2.ReplyType = ConstantUnits.REP_ITS_TO_FE_MESSAGE;
            IAHdr2.ReplyLen = ConstantUnits.TMessageStringRecordSize;
            IAHdr2.ReplyRecSize = ConstantUnits.TMessageStringRecordSize;
            IAHdr2.ReplyCnt = 1;
            IAHdr2.ErrorCode = 0;
            IAHdr2.ChannelID_NC = 0;
            IAHdr2.ChannelID_ND = 0;
            IAHdr2.ChannelID_BC = 0;
            IAHdr2.AccountType = 0;
            IAHdr2.ClientType = 1;
            IAHdr2.TraderID = ("ITS").PadRight(14).ToCharArray();
            IAHdr2.TraderLength = 3;
            IAHdr2.FEUserID = ("ITS").PadRight(15).ToCharArray();
            IAHdr2.FELength = 3;
            IAHdr2.SocketNum = 0;
            bToSend.AddRange(IAHdr2.ToByteArray());

            string MSG = txtMsg.Text.Trim();

            MsgRec.MsgStr = MSG.PadRight(250).ToCharArray();
            MsgRec.MsgStrLength = (byte)MSG.Length;
            bToSend.AddRange(MsgRec.ToByteArray());

            //SendToSocket(client,IAHdr2, MsgArr, null);
            ValCollection = SSLServer.Connections.Values;
            foreach (nsoftware.IPWorksSSL.Connection TClient in ValCollection)
            {
                if (!TClient.Connected) continue;
                Client = ClientsList[TClient.ConnectionId] as TClientConnectionToITS;
                if (Client == null)
                {
                    ErrorCnt++;
                    SL_ERROR.Add("SQF MSG,Conid unavailable," + TClient.ConnectionId);
                    continue;
                }

                if (Client.FEUserType != 1) continue;

                LastSentToClientTime = DateTime.Now;
                TClient.DataToSendB = bToSend.ToArray();
            }
        }


        private void SendDisconnectionMsgToClient(TClientConnectionToITS Client, string ClientType)
        {
            List<byte> bToSend;
            TIAReplyHeaderRecord IAHdr2;
            TMessageStringRecord MsgRec;
            string MSG = "";

            bToSend = new List<byte>();
            IAHdr2.ReplyType = ConstantUnits.REP_ITS_TO_FE_MESSAGE;
            IAHdr2.ReplyLen = ConstantUnits.TMessageStringRecordSize;
            IAHdr2.ReplyRecSize = ConstantUnits.TMessageStringRecordSize;
            IAHdr2.ReplyCnt = 1;
            IAHdr2.ErrorCode = 0;
            IAHdr2.ChannelID_NC = 0;
            IAHdr2.ChannelID_ND = 0;
            IAHdr2.ChannelID_BC = 0;
            IAHdr2.AccountType = 0;
            IAHdr2.ClientType = 1;
            IAHdr2.TraderID = ("ITS").PadRight(14).ToCharArray();
            IAHdr2.TraderLength = 3;
            IAHdr2.FEUserID = ("ITS").PadRight(15).ToCharArray();
            IAHdr2.FELength = 3;
            IAHdr2.SocketNum = 0;
            bToSend.AddRange(IAHdr2.ToByteArray());

            if (ClientType == "IBT")
                MSG = "You are NOT permitted to connect to this Server." + "\n" + "Please try another ISP Server.";
            else if (ClientType == "CTCL")
                MSG = "You are NOT permitted to connect to this Server." + "\n" + "Please try another Admin Server.";

            MsgRec.MsgStr = MSG.PadRight(250).ToCharArray();
            MsgRec.MsgStrLength = (byte)MSG.Length;
            bToSend.AddRange(MsgRec.ToByteArray());

            SSLServer.Connections[Client.ConnectionID].DataToSendB = bToSend.ToArray();
            SSLServer.Connections[Client.ConnectionID].Connected = false;
            listBox1.Items.Add(DateTime.Now.ToString("hh:mm:ss") + " " + Client.FEUserID + " Disconnected as not permitted to connect on this server.");

            File.AppendAllText(ReportsFilePathName + "IBTCTCLDissconnections.Txt", DateTime.Now.ToString("hh:mm:ss") + " " + Client.FEUserID + " " + ClientType + "\r\n");
        }

        private void cmbServerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ControlsColorchanging();
        }

        private void ControlsColorchanging()
        {
            if (chk_SendAdminExtReqs.Checked)
            {

                if (cmbServerType.Text == "IBT")
                {
                    this.tabPage2.BackColor = ColorTranslator.FromHtml("240,96,41");
                    this.tabPage3.BackColor = ColorTranslator.FromHtml("240,96,41");
                    this.tabPage4.BackColor = ColorTranslator.FromHtml("240,96,41");
                    this.Setting.BackColor = ColorTranslator.FromHtml("240,96,41");
                    this.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("240,96,41");
                    this.statusStrip1.BackColor = ColorTranslator.FromHtml("240,96,41");
                    this.statusStrip2.BackColor = ColorTranslator.FromHtml("240,96,41");
                    this.BackColor = ColorTranslator.FromHtml("240,96,41");
                    this.ForeColor = Color.White;
                    this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                    this.dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
                    this.btnSettingsSendSqOffMsg.BackColor = Color.LightGray;//.BlanchedAlmond;
                    this.btnSettingsSendSqOffMsg.ForeColor = Color.Black;


                }
                else if (cmbServerType.Text == "CTCL")
                {
                    this.tabPage2.BackColor = Color.CadetBlue;
                    this.tabPage3.BackColor = Color.CadetBlue;
                    this.tabPage4.BackColor = Color.CadetBlue;
                    this.Setting.BackColor = Color.CadetBlue;
                    this.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.CadetBlue;
                    this.statusStrip1.BackColor = Color.CadetBlue;
                    this.statusStrip2.BackColor = Color.CadetBlue;
                    this.BackColor = Color.CadetBlue;
                    this.ForeColor = Color.White;
                    this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                    this.dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
                    this.btnSettingsSendSqOffMsg.BackColor = Color.LightGray;//.BlanchedAlmond;
                    this.btnSettingsSendSqOffMsg.ForeColor = Color.Black;
                }
            }
            else
            {
                this.tabPage2.BackColor = Color.LightGray;
                this.tabPage3.BackColor = Color.LightGray;
                this.tabPage4.BackColor = Color.LightGray;
                this.Setting.BackColor = Color.LightGray;
                this.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
                this.statusStrip1.BackColor = Color.LightGray;
                this.statusStrip2.BackColor = Color.LightGray;
                this.BackColor = Color.LightGray;
                this.ForeColor = Color.Black;
                this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                this.dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
                this.btnSettingsSendSqOffMsg.BackColor = Color.LightGray;//.BlanchedAlmond;
                this.btnSettingsSendSqOffMsg.ForeColor = Color.Black;
            }
        }

        private void chk_SendAdminExtReqs_CheckedChanged(object sender, EventArgs e)
        {
            ControlsColorchanging();
        }

        byte[] DataCompression(byte[] Data)
        {
            MemoryStream ms = new MemoryStream(Data);
            MemoryStream ms2 = new MemoryStream();
            BZip2.Decompress(ms, ms2);
            byte[] CompData = ms2.ToArray();
            return CompData;
        }
    }
}

