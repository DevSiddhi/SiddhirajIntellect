namespace ITS
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.Statuslabel_OTSConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this.Statuslabel_ClientsConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_SSLConnCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_PeakConnCnt = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_ErrCnt = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_OTSRecvCnt = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_BadRejectionCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel_CurrentTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_rfc = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_stc = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_sto = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel_rfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Setting = new System.Windows.Forms.TabPage();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.cmbServerType = new System.Windows.Forms.ComboBox();
            this.chk_SendAdminExtReqs = new System.Windows.Forms.CheckBox();
            this.Txtbox_Trade_Version2 = new System.Windows.Forms.TextBox();
            this.chk_SendToParticularClient = new System.Windows.Forms.CheckBox();
            this.chkSettingsWriteReply = new System.Windows.Forms.CheckBox();
            this.btnSettingsSendSqOffMsg = new System.Windows.Forms.Button();
            this.pictureBoxRed = new System.Windows.Forms.PictureBox();
            this.pictureBoxGreen = new System.Windows.Forms.PictureBox();
            this.txtMaxClients = new System.Windows.Forms.TextBox();
            this.Lbl_MaxClients = new System.Windows.Forms.Label();
            this.Txtbox_Trade_Version = new System.Windows.Forms.TextBox();
            this.chkbox_SendLogoutsToOTS = new System.Windows.Forms.CheckBox();
            this.Txtbox_OldVerMsg = new System.Windows.Forms.TextBox();
            this.Lbl_OldVerMsg = new System.Windows.Forms.Label();
            this.txtSettingsOTSIP = new System.Windows.Forms.TextBox();
            this.txtSettingsITSID = new System.Windows.Forms.TextBox();
            this.chkbox_AllowTradeVersion = new System.Windows.Forms.CheckBox();
            this.CheckBoxListenToClients = new System.Windows.Forms.CheckBox();
            this.CheckBoxConnectToOfficeServer = new System.Windows.Forms.CheckBox();
            this.Lbl_OTS_IP = new System.Windows.Forms.Label();
            this.lbl_ITS_ID = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtDisplayClid = new System.Windows.Forms.TextBox();
            this.Btn_Save = new System.Windows.Forms.Button();
            this.Chkbox_WriteOTSRecv = new System.Windows.Forms.CheckBox();
            this.Chkbox_WriteActivityToDisplay = new System.Windows.Forms.CheckBox();
            this.btnDisplayClear = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnActivesDisconn = new System.Windows.Forms.Button();
            this.txtActivesId = new System.Windows.Forms.TextBox();
            this.btnActivesUpdate = new System.Windows.Forms.Button();
            this.txtActivitiesFileName = new System.Windows.Forms.TextBox();
            this.btnActivitiesSave2File = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSendMsg = new System.Windows.Forms.Button();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.btnChangLog = new System.Windows.Forms.Button();
            this.btnCtrlErrorRpt = new System.Windows.Forms.Button();
            this.txtCtrlFName = new System.Windows.Forms.TextBox();
            this.btnCtrlProcessFile = new System.Windows.Forms.Button();
            this.SSLServer = new nsoftware.IPWorksSSL.Ipdaemons(this.components);
            this.OTSSocket = new nsoftware.IPWorksSSL.Ipports(this.components);
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.listLogs = new System.Windows.Forms.ListBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMulticastID = new System.Windows.Forms.TextBox();
            this.txtMulticastIP = new System.Windows.Forms.TextBox();
            this.label54 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.txtMulticastPort = new System.Windows.Forms.TextBox();
            this.lblMastersStatus = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.Setting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGreen)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel4.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Statuslabel_OTSConnection
            // 
            this.Statuslabel_OTSConnection.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.Statuslabel_OTSConnection.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.Statuslabel_OTSConnection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Statuslabel_OTSConnection.Image = ((System.Drawing.Image)(resources.GetObject("Statuslabel_OTSConnection.Image")));
            this.Statuslabel_OTSConnection.Name = "Statuslabel_OTSConnection";
            this.Statuslabel_OTSConnection.Size = new System.Drawing.Size(20, 20);
            this.Statuslabel_OTSConnection.Text = "toolStripStatusLabel16";
            // 
            // Statuslabel_ClientsConnection
            // 
            this.Statuslabel_ClientsConnection.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.Statuslabel_ClientsConnection.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.Statuslabel_ClientsConnection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Statuslabel_ClientsConnection.Image = ((System.Drawing.Image)(resources.GetObject("Statuslabel_ClientsConnection.Image")));
            this.Statuslabel_ClientsConnection.Name = "Statuslabel_ClientsConnection";
            this.Statuslabel_ClientsConnection.Size = new System.Drawing.Size(20, 20);
            this.Statuslabel_ClientsConnection.Text = "toolStripStatusLabel16";
            // 
            // StatusLabel_SSLConnCount
            // 
            this.StatusLabel_SSLConnCount.AutoSize = false;
            this.StatusLabel_SSLConnCount.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_SSLConnCount.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.StatusLabel_SSLConnCount.Name = "StatusLabel_SSLConnCount";
            this.StatusLabel_SSLConnCount.Size = new System.Drawing.Size(55, 20);
            this.StatusLabel_SSLConnCount.Text = "0";
            // 
            // StatusLabel_PeakConnCnt
            // 
            this.StatusLabel_PeakConnCnt.AutoSize = false;
            this.StatusLabel_PeakConnCnt.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_PeakConnCnt.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.StatusLabel_PeakConnCnt.Name = "StatusLabel_PeakConnCnt";
            this.StatusLabel_PeakConnCnt.Size = new System.Drawing.Size(55, 20);
            this.StatusLabel_PeakConnCnt.Text = "0";
            // 
            // StatusLabel_ErrCnt
            // 
            this.StatusLabel_ErrCnt.AutoSize = false;
            this.StatusLabel_ErrCnt.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_ErrCnt.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.StatusLabel_ErrCnt.Name = "StatusLabel_ErrCnt";
            this.StatusLabel_ErrCnt.Size = new System.Drawing.Size(55, 20);
            this.StatusLabel_ErrCnt.Text = "0";
            // 
            // StatusLabel_OTSRecvCnt
            // 
            this.StatusLabel_OTSRecvCnt.AutoSize = false;
            this.StatusLabel_OTSRecvCnt.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_OTSRecvCnt.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.StatusLabel_OTSRecvCnt.Name = "StatusLabel_OTSRecvCnt";
            this.StatusLabel_OTSRecvCnt.Size = new System.Drawing.Size(100, 20);
            this.StatusLabel_OTSRecvCnt.Text = "0";
            // 
            // StatusLabel_BadRejectionCount
            // 
            this.StatusLabel_BadRejectionCount.AutoSize = false;
            this.StatusLabel_BadRejectionCount.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_BadRejectionCount.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.StatusLabel_BadRejectionCount.Name = "StatusLabel_BadRejectionCount";
            this.StatusLabel_BadRejectionCount.Size = new System.Drawing.Size(55, 20);
            this.StatusLabel_BadRejectionCount.Text = "0";
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Statuslabel_OTSConnection,
            this.Statuslabel_ClientsConnection,
            this.StatusLabel_SSLConnCount,
            this.StatusLabel_PeakConnCnt,
            this.StatusLabel_ErrCnt,
            this.StatusLabel_OTSRecvCnt,
            this.StatusLabel_BadRejectionCount});
            this.statusStrip1.Location = new System.Drawing.Point(0, 238);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(382, 25);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel_CurrentTime
            // 
            this.StatusLabel_CurrentTime.AutoSize = false;
            this.StatusLabel_CurrentTime.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_CurrentTime.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.StatusLabel_CurrentTime.Name = "StatusLabel_CurrentTime";
            this.StatusLabel_CurrentTime.Size = new System.Drawing.Size(72, 20);
            this.StatusLabel_CurrentTime.Text = "0";
            // 
            // StatusLabel_rfc
            // 
            this.StatusLabel_rfc.AutoSize = false;
            this.StatusLabel_rfc.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_rfc.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.StatusLabel_rfc.Name = "StatusLabel_rfc";
            this.StatusLabel_rfc.Size = new System.Drawing.Size(72, 20);
            this.StatusLabel_rfc.Text = "0";
            // 
            // StatusLabel_stc
            // 
            this.StatusLabel_stc.AutoSize = false;
            this.StatusLabel_stc.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_stc.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.StatusLabel_stc.Name = "StatusLabel_stc";
            this.StatusLabel_stc.Size = new System.Drawing.Size(72, 20);
            this.StatusLabel_stc.Text = "0";
            // 
            // StatusLabel_sto
            // 
            this.StatusLabel_sto.AutoSize = false;
            this.StatusLabel_sto.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_sto.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.StatusLabel_sto.Name = "StatusLabel_sto";
            this.StatusLabel_sto.Size = new System.Drawing.Size(72, 20);
            this.StatusLabel_sto.Text = "0";
            // 
            // StatusLabel_rfo
            // 
            this.StatusLabel_rfo.AutoSize = false;
            this.StatusLabel_rfo.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StatusLabel_rfo.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.StatusLabel_rfo.Name = "StatusLabel_rfo";
            this.StatusLabel_rfo.Size = new System.Drawing.Size(72, 20);
            this.StatusLabel_rfo.Text = "0";
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel_CurrentTime,
            this.StatusLabel_rfc,
            this.StatusLabel_stc,
            this.StatusLabel_sto,
            this.StatusLabel_rfo});
            this.statusStrip2.Location = new System.Drawing.Point(0, 213);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(382, 25);
            this.statusStrip2.TabIndex = 6;
            this.statusStrip2.Text = "statusStrip2";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Setting);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(382, 213);
            this.tabControl1.TabIndex = 7;
            // 
            // Setting
            // 
            this.Setting.Controls.Add(this.lblMastersStatus);
            this.Setting.Controls.Add(this.checkBox2);
            this.Setting.Controls.Add(this.cmbServerType);
            this.Setting.Controls.Add(this.chk_SendAdminExtReqs);
            this.Setting.Controls.Add(this.Txtbox_Trade_Version2);
            this.Setting.Controls.Add(this.chk_SendToParticularClient);
            this.Setting.Controls.Add(this.chkSettingsWriteReply);
            this.Setting.Controls.Add(this.btnSettingsSendSqOffMsg);
            this.Setting.Controls.Add(this.pictureBoxRed);
            this.Setting.Controls.Add(this.pictureBoxGreen);
            this.Setting.Controls.Add(this.txtMaxClients);
            this.Setting.Controls.Add(this.Lbl_MaxClients);
            this.Setting.Controls.Add(this.Txtbox_Trade_Version);
            this.Setting.Controls.Add(this.chkbox_SendLogoutsToOTS);
            this.Setting.Controls.Add(this.Txtbox_OldVerMsg);
            this.Setting.Controls.Add(this.Lbl_OldVerMsg);
            this.Setting.Controls.Add(this.txtSettingsOTSIP);
            this.Setting.Controls.Add(this.txtSettingsITSID);
            this.Setting.Controls.Add(this.chkbox_AllowTradeVersion);
            this.Setting.Controls.Add(this.CheckBoxListenToClients);
            this.Setting.Controls.Add(this.CheckBoxConnectToOfficeServer);
            this.Setting.Controls.Add(this.Lbl_OTS_IP);
            this.Setting.Controls.Add(this.lbl_ITS_ID);
            this.Setting.Location = new System.Drawing.Point(4, 22);
            this.Setting.Name = "Setting";
            this.Setting.Padding = new System.Windows.Forms.Padding(3);
            this.Setting.Size = new System.Drawing.Size(374, 187);
            this.Setting.TabIndex = 0;
            this.Setting.Text = "Settings";
            this.Setting.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(287, 109);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(66, 17);
            this.checkBox2.TabIndex = 51;
            this.checkBox2.Text = "Log 2FA";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // cmbServerType
            // 
            this.cmbServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbServerType.FormattingEnabled = true;
            this.cmbServerType.Items.AddRange(new object[] {
            "IBT",
            "CTCL"});
            this.cmbServerType.Location = new System.Drawing.Point(128, 104);
            this.cmbServerType.Name = "cmbServerType";
            this.cmbServerType.Size = new System.Drawing.Size(61, 21);
            this.cmbServerType.TabIndex = 49;
            this.cmbServerType.SelectedIndexChanged += new System.EventHandler(this.cmbServerType_SelectedIndexChanged);
            // 
            // chk_SendAdminExtReqs
            // 
            this.chk_SendAdminExtReqs.AutoSize = true;
            this.chk_SendAdminExtReqs.Location = new System.Drawing.Point(106, 107);
            this.chk_SendAdminExtReqs.Name = "chk_SendAdminExtReqs";
            this.chk_SendAdminExtReqs.Size = new System.Drawing.Size(15, 14);
            this.chk_SendAdminExtReqs.TabIndex = 48;
            this.chk_SendAdminExtReqs.UseVisualStyleBackColor = true;
            this.chk_SendAdminExtReqs.CheckedChanged += new System.EventHandler(this.chk_SendAdminExtReqs_CheckedChanged);
            // 
            // Txtbox_Trade_Version2
            // 
            this.Txtbox_Trade_Version2.Location = new System.Drawing.Point(298, 83);
            this.Txtbox_Trade_Version2.Name = "Txtbox_Trade_Version2";
            this.Txtbox_Trade_Version2.Size = new System.Drawing.Size(55, 20);
            this.Txtbox_Trade_Version2.TabIndex = 47;
            this.Txtbox_Trade_Version2.Text = "2.0.5.175";
            // 
            // chk_SendToParticularClient
            // 
            this.chk_SendToParticularClient.AutoSize = true;
            this.chk_SendToParticularClient.Location = new System.Drawing.Point(106, 85);
            this.chk_SendToParticularClient.Name = "chk_SendToParticularClient";
            this.chk_SendToParticularClient.Size = new System.Drawing.Size(190, 17);
            this.chk_SendToParticularClient.TabIndex = 46;
            this.chk_SendToParticularClient.Text = "Send To Particular Clients Using>=";
            this.chk_SendToParticularClient.UseVisualStyleBackColor = true;
            this.chk_SendToParticularClient.CheckedChanged += new System.EventHandler(this.chk_SendToParticularClient_CheckedChanged);
            // 
            // chkSettingsWriteReply
            // 
            this.chkSettingsWriteReply.AutoSize = true;
            this.chkSettingsWriteReply.Location = new System.Drawing.Point(213, 160);
            this.chkSettingsWriteReply.Name = "chkSettingsWriteReply";
            this.chkSettingsWriteReply.Size = new System.Drawing.Size(124, 17);
            this.chkSettingsWriteReply.TabIndex = 8;
            this.chkSettingsWriteReply.Text = "Write req/reply to file";
            this.chkSettingsWriteReply.UseVisualStyleBackColor = true;
            // 
            // btnSettingsSendSqOffMsg
            // 
            this.btnSettingsSendSqOffMsg.Location = new System.Drawing.Point(10, 157);
            this.btnSettingsSendSqOffMsg.Name = "btnSettingsSendSqOffMsg";
            this.btnSettingsSendSqOffMsg.Size = new System.Drawing.Size(179, 23);
            this.btnSettingsSendSqOffMsg.TabIndex = 5;
            this.btnSettingsSendSqOffMsg.Text = "Send SquareOff Naked Pos Msg";
            this.btnSettingsSendSqOffMsg.UseVisualStyleBackColor = true;
            this.btnSettingsSendSqOffMsg.Click += new System.EventHandler(this.btnSettingsSendSqOffMsg_Click);
            // 
            // pictureBoxRed
            // 
            this.pictureBoxRed.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxRed.Image")));
            this.pictureBoxRed.Location = new System.Drawing.Point(248, -14);
            this.pictureBoxRed.Name = "pictureBoxRed";
            this.pictureBoxRed.Size = new System.Drawing.Size(52, 38);
            this.pictureBoxRed.TabIndex = 44;
            this.pictureBoxRed.TabStop = false;
            this.pictureBoxRed.Visible = false;
            // 
            // pictureBoxGreen
            // 
            this.pictureBoxGreen.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxGreen.Image")));
            this.pictureBoxGreen.Location = new System.Drawing.Point(319, -14);
            this.pictureBoxGreen.Name = "pictureBoxGreen";
            this.pictureBoxGreen.Size = new System.Drawing.Size(52, 38);
            this.pictureBoxGreen.TabIndex = 45;
            this.pictureBoxGreen.TabStop = false;
            this.pictureBoxGreen.Visible = false;
            // 
            // txtMaxClients
            // 
            this.txtMaxClients.Location = new System.Drawing.Point(248, 24);
            this.txtMaxClients.Name = "txtMaxClients";
            this.txtMaxClients.Size = new System.Drawing.Size(51, 20);
            this.txtMaxClients.TabIndex = 43;
            this.txtMaxClients.Text = "1200";
            // 
            // Lbl_MaxClients
            // 
            this.Lbl_MaxClients.AutoSize = true;
            this.Lbl_MaxClients.Location = new System.Drawing.Point(300, 27);
            this.Lbl_MaxClients.Name = "Lbl_MaxClients";
            this.Lbl_MaxClients.Size = new System.Drawing.Size(61, 13);
            this.Lbl_MaxClients.TabIndex = 42;
            this.Lbl_MaxClients.Text = "Max Clients";
            // 
            // Txtbox_Trade_Version
            // 
            this.Txtbox_Trade_Version.Location = new System.Drawing.Point(247, 48);
            this.Txtbox_Trade_Version.Name = "Txtbox_Trade_Version";
            this.Txtbox_Trade_Version.Size = new System.Drawing.Size(92, 20);
            this.Txtbox_Trade_Version.TabIndex = 41;
            this.Txtbox_Trade_Version.Text = "1.0.0.1";
            // 
            // chkbox_SendLogoutsToOTS
            // 
            this.chkbox_SendLogoutsToOTS.AutoSize = true;
            this.chkbox_SendLogoutsToOTS.Location = new System.Drawing.Point(106, 66);
            this.chkbox_SendLogoutsToOTS.Name = "chkbox_SendLogoutsToOTS";
            this.chkbox_SendLogoutsToOTS.Size = new System.Drawing.Size(133, 17);
            this.chkbox_SendLogoutsToOTS.TabIndex = 40;
            this.chkbox_SendLogoutsToOTS.Text = "Send Logouts To OTS";
            this.chkbox_SendLogoutsToOTS.UseVisualStyleBackColor = true;
            // 
            // Txtbox_OldVerMsg
            // 
            this.Txtbox_OldVerMsg.Location = new System.Drawing.Point(84, 131);
            this.Txtbox_OldVerMsg.Name = "Txtbox_OldVerMsg";
            this.Txtbox_OldVerMsg.Size = new System.Drawing.Size(278, 20);
            this.Txtbox_OldVerMsg.TabIndex = 39;
            this.Txtbox_OldVerMsg.Text = "Old Version.Please Download New Version from http://www.ventura1.com/Download/CNe" +
    "wPointer/auto/NewPointer.zip";
            // 
            // Lbl_OldVerMsg
            // 
            this.Lbl_OldVerMsg.AutoSize = true;
            this.Lbl_OldVerMsg.Location = new System.Drawing.Point(11, 133);
            this.Lbl_OldVerMsg.Name = "Lbl_OldVerMsg";
            this.Lbl_OldVerMsg.Size = new System.Drawing.Size(71, 13);
            this.Lbl_OldVerMsg.TabIndex = 38;
            this.Lbl_OldVerMsg.Text = "Old Ver Msg:-";
            // 
            // txtSettingsOTSIP
            // 
            this.txtSettingsOTSIP.Location = new System.Drawing.Point(9, 55);
            this.txtSettingsOTSIP.Name = "txtSettingsOTSIP";
            this.txtSettingsOTSIP.Size = new System.Drawing.Size(87, 20);
            this.txtSettingsOTSIP.TabIndex = 37;
            this.txtSettingsOTSIP.Text = "113.30.138.244";
            // 
            // txtSettingsITSID
            // 
            this.txtSettingsITSID.Location = new System.Drawing.Point(55, 12);
            this.txtSettingsITSID.Name = "txtSettingsITSID";
            this.txtSettingsITSID.Size = new System.Drawing.Size(33, 20);
            this.txtSettingsITSID.TabIndex = 36;
            this.txtSettingsITSID.Text = "1";
            // 
            // chkbox_AllowTradeVersion
            // 
            this.chkbox_AllowTradeVersion.AutoSize = true;
            this.chkbox_AllowTradeVersion.Location = new System.Drawing.Point(106, 49);
            this.chkbox_AllowTradeVersion.Name = "chkbox_AllowTradeVersion";
            this.chkbox_AllowTradeVersion.Size = new System.Drawing.Size(141, 17);
            this.chkbox_AllowTradeVersion.TabIndex = 35;
            this.chkbox_AllowTradeVersion.Text = "Allow Trader Version  >=";
            this.chkbox_AllowTradeVersion.UseVisualStyleBackColor = true;
            // 
            // CheckBoxListenToClients
            // 
            this.CheckBoxListenToClients.AutoSize = true;
            this.CheckBoxListenToClients.Location = new System.Drawing.Point(106, 27);
            this.CheckBoxListenToClients.Name = "CheckBoxListenToClients";
            this.CheckBoxListenToClients.Size = new System.Drawing.Size(104, 17);
            this.CheckBoxListenToClients.TabIndex = 34;
            this.CheckBoxListenToClients.Text = "Listen To Clients";
            this.CheckBoxListenToClients.UseVisualStyleBackColor = true;
            this.CheckBoxListenToClients.CheckedChanged += new System.EventHandler(this.CheckBoxListenToClients_CheckedChanged);
            this.CheckBoxListenToClients.Click += new System.EventHandler(this.CheckBoxListenToClients_Click);
            // 
            // CheckBoxConnectToOfficeServer
            // 
            this.CheckBoxConnectToOfficeServer.AutoSize = true;
            this.CheckBoxConnectToOfficeServer.Location = new System.Drawing.Point(106, 10);
            this.CheckBoxConnectToOfficeServer.Name = "CheckBoxConnectToOfficeServer";
            this.CheckBoxConnectToOfficeServer.Size = new System.Drawing.Size(147, 17);
            this.CheckBoxConnectToOfficeServer.TabIndex = 33;
            this.CheckBoxConnectToOfficeServer.Text = "Connect To Office Server";
            this.CheckBoxConnectToOfficeServer.UseVisualStyleBackColor = true;
            this.CheckBoxConnectToOfficeServer.CheckedChanged += new System.EventHandler(this.CheckBoxConnectToOfficeServer_CheckedChanged);
            this.CheckBoxConnectToOfficeServer.Click += new System.EventHandler(this.CheckBoxConnectToOfficeServer_Click);
            // 
            // Lbl_OTS_IP
            // 
            this.Lbl_OTS_IP.AutoSize = true;
            this.Lbl_OTS_IP.Location = new System.Drawing.Point(7, 39);
            this.Lbl_OTS_IP.Name = "Lbl_OTS_IP";
            this.Lbl_OTS_IP.Size = new System.Drawing.Size(42, 13);
            this.Lbl_OTS_IP.TabIndex = 32;
            this.Lbl_OTS_IP.Text = "OTS IP";
            // 
            // lbl_ITS_ID
            // 
            this.lbl_ITS_ID.AutoSize = true;
            this.lbl_ITS_ID.Location = new System.Drawing.Point(8, 15);
            this.lbl_ITS_ID.Name = "lbl_ITS_ID";
            this.lbl_ITS_ID.Size = new System.Drawing.Size(44, 13);
            this.lbl_ITS_ID.TabIndex = 31;
            this.lbl_ITS_ID.Text = "ITS ID:-";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(374, 187);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Display";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listBox1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(368, 181);
            this.panel1.TabIndex = 0;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(0, 30);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(368, 151);
            this.listBox1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.txtDisplayClid);
            this.panel2.Controls.Add(this.Btn_Save);
            this.panel2.Controls.Add(this.Chkbox_WriteOTSRecv);
            this.panel2.Controls.Add(this.Chkbox_WriteActivityToDisplay);
            this.panel2.Controls.Add(this.btnDisplayClear);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(368, 30);
            this.panel2.TabIndex = 0;
            // 
            // txtDisplayClid
            // 
            this.txtDisplayClid.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtDisplayClid.Location = new System.Drawing.Point(122, 6);
            this.txtDisplayClid.Name = "txtDisplayClid";
            this.txtDisplayClid.Size = new System.Drawing.Size(100, 20);
            this.txtDisplayClid.TabIndex = 8;
            this.txtDisplayClid.Text = "TRADER";
            this.txtDisplayClid.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Btn_Save
            // 
            this.Btn_Save.Location = new System.Drawing.Point(307, 5);
            this.Btn_Save.Name = "Btn_Save";
            this.Btn_Save.Size = new System.Drawing.Size(44, 21);
            this.Btn_Save.TabIndex = 7;
            this.Btn_Save.Text = "Save";
            this.Btn_Save.UseVisualStyleBackColor = true;
            // 
            // Chkbox_WriteOTSRecv
            // 
            this.Chkbox_WriteOTSRecv.AutoSize = true;
            this.Chkbox_WriteOTSRecv.Location = new System.Drawing.Point(228, 7);
            this.Chkbox_WriteOTSRecv.Name = "Chkbox_WriteOTSRecv";
            this.Chkbox_WriteOTSRecv.Size = new System.Drawing.Size(77, 17);
            this.Chkbox_WriteOTSRecv.TabIndex = 6;
            this.Chkbox_WriteOTSRecv.Text = "OTS Recv";
            this.Chkbox_WriteOTSRecv.UseVisualStyleBackColor = true;
            // 
            // Chkbox_WriteActivityToDisplay
            // 
            this.Chkbox_WriteActivityToDisplay.AutoSize = true;
            this.Chkbox_WriteActivityToDisplay.Location = new System.Drawing.Point(63, 8);
            this.Chkbox_WriteActivityToDisplay.Name = "Chkbox_WriteActivityToDisplay";
            this.Chkbox_WriteActivityToDisplay.Size = new System.Drawing.Size(60, 17);
            this.Chkbox_WriteActivityToDisplay.TabIndex = 5;
            this.Chkbox_WriteActivityToDisplay.Text = "Activity";
            this.Chkbox_WriteActivityToDisplay.UseVisualStyleBackColor = true;
            // 
            // btnDisplayClear
            // 
            this.btnDisplayClear.Location = new System.Drawing.Point(9, 5);
            this.btnDisplayClear.Name = "btnDisplayClear";
            this.btnDisplayClear.Size = new System.Drawing.Size(47, 21);
            this.btnDisplayClear.TabIndex = 4;
            this.btnDisplayClear.Text = "Clear";
            this.btnDisplayClear.UseVisualStyleBackColor = true;
            this.btnDisplayClear.Click += new System.EventHandler(this.btnDisplayClear_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.panel3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(374, 187);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Actives";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dataGridView1);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(374, 187);
            this.panel3.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 30);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(374, 157);
            this.dataGridView1.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnActivesDisconn);
            this.panel4.Controls.Add(this.txtActivesId);
            this.panel4.Controls.Add(this.btnActivesUpdate);
            this.panel4.Controls.Add(this.txtActivitiesFileName);
            this.panel4.Controls.Add(this.btnActivitiesSave2File);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(374, 30);
            this.panel4.TabIndex = 0;
            // 
            // btnActivesDisconn
            // 
            this.btnActivesDisconn.Location = new System.Drawing.Point(257, 4);
            this.btnActivesDisconn.Name = "btnActivesDisconn";
            this.btnActivesDisconn.Size = new System.Drawing.Size(50, 23);
            this.btnActivesDisconn.TabIndex = 6;
            this.btnActivesDisconn.Text = "Discon";
            this.btnActivesDisconn.UseVisualStyleBackColor = true;
            this.btnActivesDisconn.Click += new System.EventHandler(this.btnActivesDisconn_Click);
            // 
            // txtActivesId
            // 
            this.txtActivesId.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtActivesId.Location = new System.Drawing.Point(172, 5);
            this.txtActivesId.Name = "txtActivesId";
            this.txtActivesId.Size = new System.Drawing.Size(80, 20);
            this.txtActivesId.TabIndex = 5;
            this.txtActivesId.Text = "TRADER";
            this.txtActivesId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnActivesUpdate
            // 
            this.btnActivesUpdate.Location = new System.Drawing.Point(315, 4);
            this.btnActivesUpdate.Name = "btnActivesUpdate";
            this.btnActivesUpdate.Size = new System.Drawing.Size(50, 23);
            this.btnActivesUpdate.TabIndex = 4;
            this.btnActivesUpdate.Text = "Update";
            this.btnActivesUpdate.UseVisualStyleBackColor = true;
            this.btnActivesUpdate.Click += new System.EventHandler(this.btnActivesUpdate_Click);
            // 
            // txtActivitiesFileName
            // 
            this.txtActivitiesFileName.Location = new System.Drawing.Point(3, 6);
            this.txtActivitiesFileName.Name = "txtActivitiesFileName";
            this.txtActivitiesFileName.Size = new System.Drawing.Size(94, 20);
            this.txtActivitiesFileName.TabIndex = 2;
            this.txtActivitiesFileName.Text = "ITS_ConnList.Csv";
            // 
            // btnActivitiesSave2File
            // 
            this.btnActivitiesSave2File.Location = new System.Drawing.Point(101, 4);
            this.btnActivitiesSave2File.Name = "btnActivitiesSave2File";
            this.btnActivitiesSave2File.Size = new System.Drawing.Size(46, 23);
            this.btnActivitiesSave2File.TabIndex = 3;
            this.btnActivitiesSave2File.Text = "Save";
            this.btnActivitiesSave2File.UseVisualStyleBackColor = true;
            this.btnActivitiesSave2File.Click += new System.EventHandler(this.btnActivitiesSave2File_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.button1);
            this.tabPage4.Controls.Add(this.btnSendMsg);
            this.tabPage4.Controls.Add(this.txtMsg);
            this.tabPage4.Controls.Add(this.btnChangLog);
            this.tabPage4.Controls.Add(this.btnCtrlErrorRpt);
            this.tabPage4.Controls.Add(this.txtCtrlFName);
            this.tabPage4.Controls.Add(this.btnCtrlProcessFile);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(374, 187);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Control";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(8, 142);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(92, 23);
            this.button1.TabIndex = 12;
            this.button1.Text = "Read Message";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSendMsg
            // 
            this.btnSendMsg.Location = new System.Drawing.Point(117, 142);
            this.btnSendMsg.Name = "btnSendMsg";
            this.btnSendMsg.Size = new System.Drawing.Size(164, 23);
            this.btnSendMsg.TabIndex = 11;
            this.btnSendMsg.Text = "Send Message To All";
            this.btnSendMsg.UseVisualStyleBackColor = true;
            this.btnSendMsg.Click += new System.EventHandler(this.btnSendMsg_Click);
            // 
            // txtMsg
            // 
            this.txtMsg.Location = new System.Drawing.Point(8, 41);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.Size = new System.Drawing.Size(273, 95);
            this.txtMsg.TabIndex = 10;
            // 
            // btnChangLog
            // 
            this.btnChangLog.Location = new System.Drawing.Point(327, 125);
            this.btnChangLog.Name = "btnChangLog";
            this.btnChangLog.Size = new System.Drawing.Size(34, 23);
            this.btnChangLog.TabIndex = 9;
            this.btnChangLog.Text = "CL";
            this.btnChangLog.UseVisualStyleBackColor = true;
            this.btnChangLog.Click += new System.EventHandler(this.btnChangLog_Click);
            // 
            // btnCtrlErrorRpt
            // 
            this.btnCtrlErrorRpt.Location = new System.Drawing.Point(286, 8);
            this.btnCtrlErrorRpt.Name = "btnCtrlErrorRpt";
            this.btnCtrlErrorRpt.Size = new System.Drawing.Size(75, 23);
            this.btnCtrlErrorRpt.TabIndex = 2;
            this.btnCtrlErrorRpt.Text = "Error Rpt";
            this.btnCtrlErrorRpt.UseVisualStyleBackColor = true;
            this.btnCtrlErrorRpt.Click += new System.EventHandler(this.btnCtrlErrorRpt_Click);
            // 
            // txtCtrlFName
            // 
            this.txtCtrlFName.Location = new System.Drawing.Point(89, 10);
            this.txtCtrlFName.Name = "txtCtrlFName";
            this.txtCtrlFName.Size = new System.Drawing.Size(100, 20);
            this.txtCtrlFName.TabIndex = 1;
            // 
            // btnCtrlProcessFile
            // 
            this.btnCtrlProcessFile.Location = new System.Drawing.Point(8, 8);
            this.btnCtrlProcessFile.Name = "btnCtrlProcessFile";
            this.btnCtrlProcessFile.Size = new System.Drawing.Size(75, 23);
            this.btnCtrlProcessFile.TabIndex = 0;
            this.btnCtrlProcessFile.Text = "ProcessFile";
            this.btnCtrlProcessFile.UseVisualStyleBackColor = true;
            this.btnCtrlProcessFile.Click += new System.EventHandler(this.btnCtrlProcessFile_Click);
            // 
            // SSLServer
            // 
            this.SSLServer.About = "IP*Works! SSL V9 [Build 4611]";
            this.SSLServer.SSLStartMode = nsoftware.IPWorksSSL.IpdaemonsSSLStartModes.sslNone;
            this.SSLServer.OnConnected += new nsoftware.IPWorksSSL.Ipdaemons.OnConnectedHandler(this.SSLServer_OnConnected);
            this.SSLServer.OnConnectionRequest += new nsoftware.IPWorksSSL.Ipdaemons.OnConnectionRequestHandler(this.SSLServer_OnConnectionRequest);
            this.SSLServer.OnDataIn += new nsoftware.IPWorksSSL.Ipdaemons.OnDataInHandler(this.SSLServer_OnDataIn);
            this.SSLServer.OnDisconnected += new nsoftware.IPWorksSSL.Ipdaemons.OnDisconnectedHandler(this.SSLServer_OnDisconnected);
            this.SSLServer.OnError += new nsoftware.IPWorksSSL.Ipdaemons.OnErrorHandler(this.SSLServer_OnError);
            this.SSLServer.OnSSLStatus += new nsoftware.IPWorksSSL.Ipdaemons.OnSSLStatusHandler(this.SSLServer_OnSSLStatus);
            // 
            // OTSSocket
            // 
            this.OTSSocket.About = "IP*Works! SSL V9 [Build 4611]";
            this.OTSSocket.SSLStartMode = nsoftware.IPWorksSSL.IpportsSSLStartModes.sslNone;
            this.OTSSocket.OnConnected += new nsoftware.IPWorksSSL.Ipports.OnConnectedHandler(this.OTSSocket_OnConnected);
            this.OTSSocket.OnDataIn += new nsoftware.IPWorksSSL.Ipports.OnDataInHandler(this.OTSSocket_OnDataIn);
            this.OTSSocket.OnDisconnected += new nsoftware.IPWorksSSL.Ipports.OnDisconnectedHandler(this.OTSSocket_OnDisconnected);
            this.OTSSocket.OnError += new nsoftware.IPWorksSSL.Ipports.OnErrorHandler(this.OTSSocket_OnError);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.listLogs);
            this.tabPage1.Controls.Add(this.groupBox8);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(374, 187);
            this.tabPage1.TabIndex = 4;
            this.tabPage1.Text = "Handler";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // listLogs
            // 
            this.listLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listLogs.FormattingEnabled = true;
            this.listLogs.Location = new System.Drawing.Point(7, 74);
            this.listLogs.Name = "listLogs";
            this.listLogs.Size = new System.Drawing.Size(360, 108);
            this.listLogs.TabIndex = 94;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.label9);
            this.groupBox8.Controls.Add(this.txtMulticastID);
            this.groupBox8.Controls.Add(this.txtMulticastIP);
            this.groupBox8.Controls.Add(this.label54);
            this.groupBox8.Controls.Add(this.label55);
            this.groupBox8.Controls.Add(this.txtMulticastPort);
            this.groupBox8.Location = new System.Drawing.Point(7, 4);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(346, 64);
            this.groupBox8.TabIndex = 93;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Intellect Handler";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(192, 15);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 13);
            this.label9.TabIndex = 48;
            this.label9.Text = "ID :";
            // 
            // txtMulticastID
            // 
            this.txtMulticastID.Location = new System.Drawing.Point(195, 32);
            this.txtMulticastID.Name = "txtMulticastID";
            this.txtMulticastID.Size = new System.Drawing.Size(39, 20);
            this.txtMulticastID.TabIndex = 49;
            this.txtMulticastID.Text = "1";
            // 
            // txtMulticastIP
            // 
            this.txtMulticastIP.Location = new System.Drawing.Point(10, 32);
            this.txtMulticastIP.Name = "txtMulticastIP";
            this.txtMulticastIP.Size = new System.Drawing.Size(109, 20);
            this.txtMulticastIP.TabIndex = 45;
            this.txtMulticastIP.Text = "224.0.0.10";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(6, 16);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(42, 13);
            this.label54.TabIndex = 44;
            this.label54.Text = "Group :";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(122, 15);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(32, 13);
            this.label55.TabIndex = 46;
            this.label55.Text = "Port :";
            // 
            // txtMulticastPort
            // 
            this.txtMulticastPort.Location = new System.Drawing.Point(125, 32);
            this.txtMulticastPort.Name = "txtMulticastPort";
            this.txtMulticastPort.Size = new System.Drawing.Size(39, 20);
            this.txtMulticastPort.TabIndex = 47;
            this.txtMulticastPort.Text = "7585";
            // 
            // lblMastersStatus
            // 
            this.lblMastersStatus.Location = new System.Drawing.Point(3, 104);
            this.lblMastersStatus.Name = "lblMastersStatus";
            this.lblMastersStatus.Size = new System.Drawing.Size(93, 17);
            this.lblMastersStatus.TabIndex = 93;
            this.lblMastersStatus.Text = "label17";
            this.lblMastersStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblMastersStatus.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 263);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip2);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "ITS_";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.Setting.ResumeLayout(false);
            this.Setting.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGreen)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private nsoftware.IPWorksSSL.Ipdaemons SSLServer;
        private nsoftware.IPWorksSSL.Ipports OTSSocket;
        private System.Windows.Forms.ToolStripStatusLabel Statuslabel_OTSConnection;
        private System.Windows.Forms.ToolStripStatusLabel Statuslabel_ClientsConnection;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_SSLConnCount;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_PeakConnCnt;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_ErrCnt;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_OTSRecvCnt;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_BadRejectionCount;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_CurrentTime;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_rfc;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_stc;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_sto;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_rfo;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Setting;
        private System.Windows.Forms.Button btnSettingsSendSqOffMsg;
        private System.Windows.Forms.PictureBox pictureBoxRed;
        private System.Windows.Forms.TextBox txtMaxClients;
        private System.Windows.Forms.Label Lbl_MaxClients;
        private System.Windows.Forms.TextBox Txtbox_Trade_Version;
        private System.Windows.Forms.CheckBox chkbox_SendLogoutsToOTS;
        private System.Windows.Forms.TextBox Txtbox_OldVerMsg;
        private System.Windows.Forms.Label Lbl_OldVerMsg;
        private System.Windows.Forms.TextBox txtSettingsOTSIP;
        private System.Windows.Forms.TextBox txtSettingsITSID;
        private System.Windows.Forms.CheckBox chkbox_AllowTradeVersion;
        private System.Windows.Forms.CheckBox CheckBoxListenToClients;
        private System.Windows.Forms.CheckBox CheckBoxConnectToOfficeServer;
        private System.Windows.Forms.Label Lbl_OTS_IP;
        private System.Windows.Forms.Label lbl_ITS_ID;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button Btn_Save;
        private System.Windows.Forms.CheckBox Chkbox_WriteOTSRecv;
        private System.Windows.Forms.CheckBox Chkbox_WriteActivityToDisplay;
        private System.Windows.Forms.Button btnDisplayClear;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnActivesUpdate;
        private System.Windows.Forms.TextBox txtActivitiesFileName;
        private System.Windows.Forms.Button btnActivitiesSave2File;
        private System.Windows.Forms.TextBox txtDisplayClid;
        private System.Windows.Forms.Button btnActivesDisconn;
        private System.Windows.Forms.TextBox txtActivesId;
        private System.Windows.Forms.CheckBox chkSettingsWriteReply;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button btnCtrlProcessFile;
        private System.Windows.Forms.TextBox txtCtrlFName;
        private System.Windows.Forms.Button btnCtrlErrorRpt;
        private System.Windows.Forms.Button btnChangLog;
        private System.Windows.Forms.CheckBox chk_SendToParticularClient;
        private System.Windows.Forms.TextBox Txtbox_Trade_Version2;
        private System.Windows.Forms.CheckBox chk_SendAdminExtReqs;
        private System.Windows.Forms.Button btnSendMsg;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cmbServerType;
        private System.Windows.Forms.PictureBox pictureBoxGreen;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ListBox listLogs;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtMulticastID;
        private System.Windows.Forms.TextBox txtMulticastIP;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.TextBox txtMulticastPort;
        private System.Windows.Forms.Label lblMastersStatus;
    }
}

