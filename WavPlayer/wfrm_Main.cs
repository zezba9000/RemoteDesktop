using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Threading;

using System.Net.Sockets;

using LumiSoft.Net.UDP;
using LumiSoft.Net.Codec;
using LumiSoft.Media.Wave;

namespace WavPlayer
{
    /// <summary>
    /// Application main window.
    /// </summary>
    public class wfrm_Main : Form
    {
        private Label         mt_InDevices        = null;
        private ComboBox      m_pInDevices        = null;
        private Label         mt_OutDevices       = null;
        private ComboBox      m_pOutDevices       = null;
        private Label         mt_Codec            = null;
        private ComboBox      m_pCodec            = null;
        private GroupBox      m_pSeparator1       = null;
        private Label         mt_LocalEndPoint    = null;
        private ComboBox      m_pLoacalIP         = null;
        private NumericUpDown m_pLocalPort        = null;
        private Button        m_pToggleRun        = null;
        private CheckBox      m_pRecord           = null;
        private Label         mt_RecordFile       = null;
        private TextBox       m_pRecordFile       = null;
        private Button        m_pRecordFileBrowse = null;
        private GroupBox      m_pSeparator2       = null;
        private Label         mt_RemoteEP         = null;
        private TextBox       m_pRemoteIP         = null;
        private NumericUpDown m_pRemotePort       = null;
        private Label         mt_Microphone       = null;
        private Button        m_pToggleMic        = null;
        private Label         mt_SendTestSound    = null;
        private Button        m_pSendTestSound    = null;
        private Button        m_pPlayTestSound    = null;
        private GroupBox      m_pSeparator3       = null;
        private Label         mt_PacketsReceived  = null;
        private Label         m_pPacketsReceived  = null;
        private Label         mt_BytesReceived    = null;
        private Label         m_pBytesReceived    = null;
        private Label         mt_PacketsSent      = null;
        private Label         m_pPacketsSent      = null;
        private Label         mt_BytesSent        = null;
        private Label         m_pBytesSent        = null;

        private bool                       m_IsRunning     = false;
        private bool                       m_IsSendingMic  = false;
        private bool                       m_IsSendingTest = false;
        private UdpServer                  m_pUdpServer    = null;
        private WaveIn                     m_pWaveIn       = null;
        private WaveOut                    m_pWaveOut      = null;
        private int                        m_Codec         = 0;
        private FileStream                 m_pRecordStream = null;
        private IPEndPoint                 m_pTargetEP     = null;
        private string                     m_PlayFile      = "";
        private System.Windows.Forms.Timer m_pTimer        = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public wfrm_Main()
        {
            InitUI();

            LoadWaveDevices();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(400,400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Wave demo";
            this.FormClosed += new FormClosedEventHandler(wfrm_Main_FormClosed);

            mt_InDevices = new Label();
            mt_InDevices.Size = new Size(100,20);
            mt_InDevices.Location = new Point(0,20);
            mt_InDevices.TextAlign = ContentAlignment.MiddleRight;
            mt_InDevices.Text = "Input device:";
            
            m_pInDevices = new ComboBox();
            m_pInDevices.Size = new Size(275,20);
            m_pInDevices.Location = new Point(100,20);
            m_pInDevices.DropDownStyle = ComboBoxStyle.DropDownList;

            mt_OutDevices = new Label();
            mt_OutDevices.Size = new Size(100,20);
            mt_OutDevices.Location = new Point(0,45);
            mt_OutDevices.TextAlign = ContentAlignment.MiddleRight;
            mt_OutDevices.Text = "Output device:";

            m_pOutDevices = new ComboBox();
            m_pOutDevices.Size = new Size(275,20);
            m_pOutDevices.Location = new Point(100,45);
            m_pOutDevices.DropDownStyle = ComboBoxStyle.DropDownList;

            mt_Codec = new Label();
            mt_Codec.Size = new Size(100,20);
            mt_Codec.Location = new Point(0,70);
            mt_Codec.TextAlign = ContentAlignment.MiddleRight;
            mt_Codec.Text = "Codec:";

            m_pCodec = new ComboBox();
            m_pCodec.Size = new Size(100,20);
            m_pCodec.Location = new Point(100,70);
            m_pCodec.DropDownStyle = ComboBoxStyle.DropDownList;
            m_pCodec.Items.Add("G711 a-law");
            m_pCodec.Items.Add("G711 u-law");
            m_pCodec.SelectedIndex = 0;

            m_pSeparator1 = new GroupBox();
            m_pSeparator1.Size = new Size(395,3);
            m_pSeparator1.Location = new Point(2,100);

            mt_LocalEndPoint = new Label();
            mt_LocalEndPoint.Size = new Size(100,20);
            mt_LocalEndPoint.Location = new Point(0,110);
            mt_LocalEndPoint.TextAlign = ContentAlignment.MiddleRight;
            mt_LocalEndPoint.Text = "Local end point:";

            m_pLoacalIP = new ComboBox();
            m_pLoacalIP.Size = new Size(135,20);
            m_pLoacalIP.Location = new Point(100,110);
            m_pLoacalIP.DropDownStyle = ComboBoxStyle.DropDownList;
            m_pLoacalIP.Items.Add("127.0.0.1");
            foreach(IPAddress ip in System.Net.Dns.GetHostAddresses("")){
                m_pLoacalIP.Items.Add(ip.ToString());
            }
            m_pLoacalIP.SelectedIndex = 0;
                        
            m_pLocalPort = new NumericUpDown();
            m_pLocalPort.Size = new Size(60,20);
            m_pLocalPort.Location = new Point(240,110);
            m_pLocalPort.Minimum = 1;
            m_pLocalPort.Maximum = 99999;
            m_pLocalPort.Value = 11000;

            m_pToggleRun = new Button();
            m_pToggleRun.Size = new Size(70,20);
            m_pToggleRun.Location = new Point(305,110);
            m_pToggleRun.Text = "Start";
            m_pToggleRun.Click += new EventHandler(m_pToggleRun_Click);

            m_pRecord = new CheckBox();
            m_pRecord.Size = new Size(200,20);
            m_pRecord.Location = new Point(100,140);
            m_pRecord.Text = "Record incoming audio";
            m_pRecord.CheckedChanged += new EventHandler(m_pRecord_CheckedChanged);

            mt_RecordFile = new Label();
            mt_RecordFile.Size = new Size(100,20);
            mt_RecordFile.Location = new Point(0,160);
            mt_RecordFile.TextAlign = ContentAlignment.MiddleRight;
            mt_RecordFile.Text = "File Name:";

            m_pRecordFile = new TextBox();
            m_pRecordFile.Size = new Size(240,20);
            m_pRecordFile.Location = new Point(100,160);
            m_pRecordFile.Enabled = false;

            m_pRecordFileBrowse = new Button();
            m_pRecordFileBrowse.Size = new Size(30,20);
            m_pRecordFileBrowse.Location = new Point(345,160);
            m_pRecordFileBrowse.Text = "...";
            m_pRecordFileBrowse.Enabled = false;
            m_pRecordFileBrowse.Click += new EventHandler(m_pRecordFileBrowse_Click);
                        
            m_pSeparator2 = new GroupBox();
            m_pSeparator2.Size = new Size(395,3);
            m_pSeparator2.Location = new Point(2,190);

            mt_RemoteEP = new Label();
            mt_RemoteEP.Size = new Size(100,20);
            mt_RemoteEP.Location = new Point(0,200);
            mt_RemoteEP.TextAlign = ContentAlignment.MiddleRight;
            mt_RemoteEP.Text = "Target IP/Port:";

            m_pRemoteIP = new TextBox();
            m_pRemoteIP.Size = new Size(200,20);
            m_pRemoteIP.Location = new Point(100,200);
            m_pRemoteIP.Enabled = false;

            m_pRemotePort = new NumericUpDown();
            m_pRemotePort.Size = new Size(70,20);
            m_pRemotePort.Location = new Point(305,200);
            m_pRemotePort.Minimum = 1;
            m_pRemotePort.Maximum = 99999;
            m_pRemotePort.Value = 11000;
            m_pRemotePort.Enabled = false;

            mt_Microphone = new Label();
            mt_Microphone.Size = new Size(100,20);
            mt_Microphone.Location = new Point(0,230);
            mt_Microphone.TextAlign = ContentAlignment.MiddleRight;
            mt_Microphone.Text = "Microphone:";

            m_pToggleMic = new Button();
            m_pToggleMic.Size = new Size(70,20);
            m_pToggleMic.Location = new Point(100,230);
            m_pToggleMic.Text = "Start";
            m_pToggleMic.Enabled = false;
            m_pToggleMic.Click += new EventHandler(m_pToggleMic_Click);

            mt_SendTestSound = new Label();
            mt_SendTestSound.Size = new Size(100,20);
            mt_SendTestSound.Location = new Point(0,255);
            mt_SendTestSound.TextAlign = ContentAlignment.MiddleRight;
            mt_SendTestSound.Text = "Test sound:";

            m_pSendTestSound = new Button();
            m_pSendTestSound.Size = new Size(70,20);
            m_pSendTestSound.Location = new Point(100,255);
            m_pSendTestSound.Text = "Start";
            m_pSendTestSound.Enabled = false;
            m_pSendTestSound.Click += new EventHandler(m_pSendTestSound_Click);

            m_pPlayTestSound = new Button();
            m_pPlayTestSound.Size = new Size(70,20);
            m_pPlayTestSound.Location = new Point(180,255);
            m_pPlayTestSound.Text = "Play";
            m_pPlayTestSound.Enabled = false;
            m_pPlayTestSound.Click += new EventHandler(m_pPlayTestSound_Click);

            m_pSeparator3 = new GroupBox();
            m_pSeparator3.Size = new Size(395,3);
            m_pSeparator3.Location = new Point(2,285);

            mt_PacketsReceived = new Label();
            mt_PacketsReceived.Size = new Size(100,20);
            mt_PacketsReceived.Location = new Point(0,300);
            mt_PacketsReceived.TextAlign = ContentAlignment.MiddleRight;
            mt_PacketsReceived.Text = "Packets received:";

            m_pPacketsReceived = new Label();
            m_pPacketsReceived.Size = new Size(100,20);
            m_pPacketsReceived.Location = new Point(100,300);
            m_pPacketsReceived.TextAlign = ContentAlignment.MiddleLeft;
            m_pPacketsReceived.Text = "0";

            mt_BytesReceived = new Label();
            mt_BytesReceived.Size = new Size(100,20);
            mt_BytesReceived.Location = new Point(0,320);
            mt_BytesReceived.TextAlign = ContentAlignment.MiddleRight;
            mt_BytesReceived.Text = "Bytes received:";

            m_pBytesReceived = new Label();
            m_pBytesReceived.Size = new Size(100,20);
            m_pBytesReceived.Location = new Point(100,320);
            m_pBytesReceived.TextAlign = ContentAlignment.MiddleLeft;
            m_pBytesReceived.Text = "0";

            mt_PacketsSent = new Label();
            mt_PacketsSent.Size = new Size(100,20);
            mt_PacketsSent.Location = new Point(0,340);
            mt_PacketsSent.TextAlign = ContentAlignment.MiddleRight;
            mt_PacketsSent.Text = "Packets sent:";

            m_pPacketsSent = new Label();
            m_pPacketsSent.Size = new Size(100,20);
            m_pPacketsSent.Location = new Point(100,340);
            m_pPacketsSent.TextAlign = ContentAlignment.MiddleLeft;
            m_pPacketsSent.Text = "0";

            mt_BytesSent = new Label();
            mt_BytesSent.Size = new Size(100,20);
            mt_BytesSent.Location = new Point(0,360);
            mt_BytesSent.TextAlign = ContentAlignment.MiddleRight;
            mt_BytesSent.Text = "Bytes sent:";

            m_pBytesSent = new Label();
            m_pBytesSent.Size = new Size(100,20);
            m_pBytesSent.Location = new Point(100,360);
            m_pBytesSent.TextAlign = ContentAlignment.MiddleLeft;
            m_pBytesSent.Text = "0";

            this.Controls.Add(mt_InDevices);
            this.Controls.Add(m_pInDevices);
            this.Controls.Add(mt_OutDevices);
            this.Controls.Add(m_pOutDevices);
            this.Controls.Add(mt_Codec);
            this.Controls.Add(m_pCodec);
            this.Controls.Add(m_pSeparator1);
            this.Controls.Add(m_pLoacalIP);
            this.Controls.Add(mt_LocalEndPoint);
            this.Controls.Add(m_pLocalPort);
            this.Controls.Add(m_pToggleRun);
            this.Controls.Add(m_pRecord);
            this.Controls.Add(mt_RecordFile);
            this.Controls.Add(m_pRecordFile);
            this.Controls.Add(m_pRecordFileBrowse);
            this.Controls.Add(m_pSeparator2);
            this.Controls.Add(mt_RemoteEP);
            this.Controls.Add(m_pRemoteIP);
            this.Controls.Add(m_pRemotePort);
            this.Controls.Add(mt_Microphone);
            this.Controls.Add(m_pToggleMic);
            this.Controls.Add(mt_SendTestSound);
            this.Controls.Add(m_pSendTestSound);
            this.Controls.Add(m_pPlayTestSound);
            this.Controls.Add(m_pSeparator3);
            this.Controls.Add(mt_PacketsReceived);
            this.Controls.Add(m_pPacketsReceived);
            this.Controls.Add(mt_BytesReceived);
            this.Controls.Add(m_pBytesReceived);
            this.Controls.Add(mt_PacketsSent);
            this.Controls.Add(m_pPacketsSent);
            this.Controls.Add(mt_BytesSent);
            this.Controls.Add(m_pBytesSent);
        }
                                                                                                               
        #endregion


        #region Events Handling

        #region method m_pToggleRun_Click

        private void m_pToggleRun_Click(object sender,EventArgs e)
        {
            if(m_IsRunning){
                m_IsRunning = false;
                m_IsSendingTest = false;

                m_pUdpServer.Dispose();
                m_pUdpServer = null;

                m_pWaveOut.Dispose();
                m_pWaveOut = null;

                if(m_pRecordStream != null){
                    m_pRecordStream.Dispose();
                    m_pRecordStream = null;
                }

                m_pTimer.Dispose();
                m_pTimer = null;

                m_pInDevices.Enabled = true;
                m_pOutDevices.Enabled = true;
                m_pCodec.Enabled = true;
                m_pToggleRun.Text = "Start";
                m_pRecord.Enabled = true;
                m_pRecordFile.Enabled = true;
                m_pRecordFileBrowse.Enabled = true;
                m_pRemoteIP.Enabled = false;
                m_pRemotePort.Enabled = false;
                m_pToggleMic.Text = "Start";
                m_pToggleMic.Enabled = false;
                m_pSendTestSound.Enabled = false;
                m_pSendTestSound.Text = "Start";
                m_pPlayTestSound.Enabled = false;
                m_pPlayTestSound.Text = "Play";
            }
            else{
                if(m_pOutDevices.SelectedIndex == -1){
                    MessageBox.Show(this,"Please select output device !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }
                if(m_pRecord.Checked && m_pRecordFile.Text == ""){
                    MessageBox.Show(this,"Please specify record file !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }

                if(m_pRecord.Checked){
                    m_pRecordStream = File.Create(m_pRecordFile.Text);
                }

                m_IsRunning = true;

                m_Codec = m_pCodec.SelectedIndex;

                m_pWaveOut = new WaveOut(WaveOut.Devices[m_pOutDevices.SelectedIndex],8000,16,1);

                m_pUdpServer = new UdpServer();
                m_pUdpServer.Bindings = new IPEndPoint[]{new IPEndPoint(IPAddress.Parse(m_pLoacalIP.Text),(int)m_pLocalPort.Value)};
                m_pUdpServer.PacketReceived += new PacketReceivedHandler(m_pUdpServer_PacketReceived);
                m_pUdpServer.Start();

                m_pTimer = new System.Windows.Forms.Timer();
                m_pTimer.Interval = 1000;
                m_pTimer.Tick += new EventHandler(m_pTimer_Tick);
                m_pTimer.Enabled = true;

                m_pInDevices.Enabled = false;
                m_pOutDevices.Enabled = false;
                m_pCodec.Enabled = false;
                m_pToggleRun.Text = "Stop";
                m_pRecord.Enabled = false;
                m_pRecordFile.Enabled = false;
                m_pRecordFileBrowse.Enabled = false;
                m_pRemoteIP.Enabled = true;
                m_pRemotePort.Enabled = true;
                m_pToggleMic.Enabled = true;
                m_pSendTestSound.Enabled = true;
                m_pSendTestSound.Text = "Start";
                m_pPlayTestSound.Enabled = true;                
                m_pPlayTestSound.Text = "Play";
            }
        }
                                
        #endregion

        #region method m_pRecord_CheckedChanged

        private void m_pRecord_CheckedChanged(object sender,EventArgs e)
        {
            if(m_pRecord.Checked){
                m_pRecordFile.Enabled = true;
                m_pRecordFileBrowse.Enabled = true;
            }
            else{
                m_pRecordFile.Enabled = false;
                m_pRecordFileBrowse.Enabled = false;
            }
        }

        #endregion

        #region method m_pRecordFileBrowse_Click

        private void m_pRecordFileBrowse_Click(object sender,EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "record.raw";
            if(dlg.ShowDialog(this) == DialogResult.OK){
                m_pRecordFile.Text = dlg.FileName;
            }
        }

        #endregion

        #region method m_pToggleMic_Click

        private void m_pToggleMic_Click(object sender,EventArgs e)
        {
            if(m_IsSendingMic){
                m_IsSendingMic = false;

                m_pWaveIn.Dispose();
                m_pWaveIn = null;

                OnAudioStopped();
            }
            else{
                if(m_pInDevices.SelectedIndex == -1){
                    MessageBox.Show(this,"Please select input device !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }
                try{
                    m_pTargetEP = new IPEndPoint(IPAddress.Parse(m_pRemoteIP.Text),(int)m_pRemotePort.Value);
                }
                catch{
                    MessageBox.Show(this,"Invalid target IP address or port !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }

                m_IsSendingMic = true;

                m_pWaveIn = new WaveIn(WaveIn.Devices[m_pInDevices.SelectedIndex],8000,16,1,400);
                m_pWaveIn.BufferFull += new BufferFullHandler(m_pWaveIn_BufferFull);
                m_pWaveIn.Start();

                m_pToggleMic.Text = "Stop";
                m_pSendTestSound.Enabled = false;
            }
        }
                
        #endregion

        #region method m_pSendTestSound_Click

        private void m_pSendTestSound_Click(object sender,EventArgs e)
        {
            if(m_IsSendingTest){
                m_IsSendingTest = false;

                OnAudioStopped();
            }
            else{
                try{
                    m_pTargetEP = new IPEndPoint(IPAddress.Parse(m_pRemoteIP.Text),(int)m_pRemotePort.Value);
                }
                catch{
                    MessageBox.Show(this,"Invalid target IP address or port !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }                

                OpenFileDialog dlg = new OpenFileDialog();
                dlg.InitialDirectory = Application.StartupPath + "\\audio";
                if(dlg.ShowDialog(null) == DialogResult.OK){
                    m_PlayFile = dlg.FileName;

                    m_pSendTestSound.Text = "Stop";
                    m_pToggleMic.Enabled = false;
                    m_pPlayTestSound.Enabled = false;

                    m_IsSendingTest = true;

                    Thread tr = new Thread(new ThreadStart(this.SendTest));
                    tr.Start();
                }
            }
        }

        #endregion

        #region method m_pPlayTestSound_Click

        private void m_pPlayTestSound_Click(object sender,EventArgs e)
        {
            if(m_IsSendingTest){
                m_IsSendingTest = false;

                OnAudioStopped();
            }
            else{
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.InitialDirectory = Application.StartupPath + "\\audio";
                if(dlg.ShowDialog(null) == DialogResult.OK){
                    m_PlayFile = dlg.FileName;

                    m_IsSendingTest = true;

                    m_pToggleMic.Enabled = false;
                    m_pSendTestSound.Enabled = false;
                    m_pPlayTestSound.Text = "Stop";

                    Thread tr = new Thread(new ThreadStart(this.PlayTestAudio));
                    tr.Start();
                }
            }
        }

        #endregion

        #region method m_pUdpServer_PacketReceived

        /// <summary>
        /// This method is called when we got UDP packet. 
        /// </summary>
        /// <param name="e">Event data.</param>
        private void m_pUdpServer_PacketReceived(UdpPacket_eArgs e)
        {
            // Decompress data.
            byte[] decodedData = null;
            if(m_Codec == 0){
                decodedData = G711.Decode_aLaw(e.Data,0,e.Data.Length);
            }
            else if(m_Codec == 1){
                decodedData = G711.Decode_uLaw(e.Data,0,e.Data.Length);
            }

            // We just play received packet.
            m_pWaveOut.Play(decodedData,0,decodedData.Length);

            // Record if recoring enabled.
            if(m_pRecordStream != null){
                m_pRecordStream.Write(decodedData,0,decodedData.Length);
            }
        }

        #endregion

        #region method m_pWaveIn_BufferFull

        /// <summary>
        /// This method is called when recording buffer is full and we need to process it.
        /// </summary>
        /// <param name="buffer">Recorded data.</param>
        private void m_pWaveIn_BufferFull(byte[] buffer)
        {
            // Compress data. 
            byte[] encodedData = null;
            if(m_Codec == 0){
                encodedData = G711.Encode_aLaw(buffer,0,buffer.Length);
            }
            else if(m_Codec == 1){
                encodedData = G711.Encode_uLaw(buffer,0,buffer.Length);
            }

            // We just sent buffer to target end point.
            m_pUdpServer.SendPacket(encodedData,0,encodedData.Length,m_pTargetEP);
        }

        #endregion

        #region method wfrm_Main_FormClosed

        /// <summary>
        /// This method is called when this form is closed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void wfrm_Main_FormClosed(object sender,FormClosedEventArgs e)
        {
            if(m_pUdpServer != null){
                m_pUdpServer.Dispose();
                m_pUdpServer = null;
            }
            if(m_pWaveIn != null){
                m_pWaveIn.Dispose();
                m_pWaveIn = null;
            }
            if(m_pWaveOut != null){
                m_pWaveOut.Dispose();
                m_pWaveOut = null;
            }
            if(m_pRecordStream != null){
                m_pRecordStream.Dispose();
                m_pRecordStream = null;
            }
            if(m_pTimer != null){
                m_pTimer.Dispose();
                m_pTimer = null;
            }
        }

        #endregion

        #region method m_pTimer_Tick

        /// <summary>
        /// This method is called when we need to refresh UI statistics data.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimer_Tick(object sender,EventArgs e)
        {
            m_pPacketsReceived.Text = m_pUdpServer.PacketsReceived.ToString();
            m_pBytesReceived.Text   = m_pUdpServer.BytesReceived.ToString();
            m_pPacketsSent.Text     = m_pUdpServer.PacketsSent.ToString();
            m_pBytesSent.Text       = m_pUdpServer.BytesSent.ToString();
        }

        #endregion

        #endregion


        #region method SendTest

        /// <summary>
        /// Sends test sound to target end point.
        /// </summary>
        private void SendTest()
        {
            try{                
                using(FileStream fs = File.OpenRead(m_PlayFile)){
                    byte[] buffer       = new byte[400];
                    int    readedCount  = fs.Read(buffer,0,buffer.Length);
                    long   lastSendTime = DateTime.Now.Ticks;
                    while(m_IsSendingTest && readedCount > 0){
                        // Compress data.
                        byte[] encodedData = null;
                        if(m_Codec == 0){
                            encodedData = G711.Encode_aLaw(buffer,0,buffer.Length);
                        }
                        else if(m_Codec == 1){
                            encodedData = G711.Encode_uLaw(buffer,0,buffer.Length);
                        }

                        // Send and read next.
                        m_pUdpServer.SendPacket(encodedData,0,encodedData.Length,m_pTargetEP);
                        readedCount = fs.Read(buffer,0,buffer.Length);

                        Thread.Sleep(25);

                        lastSendTime = DateTime.Now.Ticks;
                    }
                }

                if(m_IsRunning){
                    this.Invoke(new MethodInvoker(this.OnAudioStopped));
                }
            }
            catch(Exception x){
                MessageBox.Show(null,"Error: " + x.ToString(),"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        #endregion

        #region method PlayTestAudio

        /// <summary>
        /// Plays test audio.
        /// </summary>
        private void PlayTestAudio()
        {
            try{           
                using(FileStream fs = File.OpenRead(m_PlayFile)){
                    byte[] buffer       = new byte[400];
                    int    readedCount  = fs.Read(buffer,0,buffer.Length);
                    long   lastSendTime = DateTime.Now.Ticks;
                    while(m_IsSendingTest && readedCount > 0){
                        // Send and read next.
                        m_pWaveOut.Play(buffer,0,readedCount);
                        readedCount = fs.Read(buffer,0,buffer.Length);

                        Thread.Sleep(25);

                        lastSendTime = DateTime.Now.Ticks;
                    }                    
                }

                if(m_IsRunning){
                    this.Invoke(new MethodInvoker(this.OnAudioStopped));
                }
            }
            catch(Exception x){
                MessageBox.Show(null,"Error: " + x.ToString(),"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        #endregion

        #region method OnAudioStopped

        /// <summary>
        /// This method is called when audio sending/playing completes.
        /// NOTE: This method must be called from UI thread.
        /// </summary>
        private void OnAudioStopped()
        {
             m_pToggleMic.Enabled = true;
             m_pSendTestSound.Enabled = true;
             m_pSendTestSound.Text = "Start";
             m_pPlayTestSound.Enabled = true;
             m_pPlayTestSound.Text = "Play";

             m_IsSendingTest = false;
        }

        #endregion


        #region method LoadWaveDevices

        /// <summary>
        /// Loads available wave input and output devices to UI.
        /// </summary>
        private void LoadWaveDevices()
        {
            // Load input devices.
            m_pInDevices.Items.Clear();
            foreach(WavInDevice device in WaveIn.Devices){
                m_pInDevices.Items.Add(device.Name);
            }
            if(m_pInDevices.Items.Count > 0){
                m_pInDevices.SelectedIndex = 0;
            }

            // Load output devices.
            m_pOutDevices.Items.Clear();
            foreach(WavOutDevice device in WaveOut.Devices){
                m_pOutDevices.Items.Add(device.Name);
            }
            if(m_pOutDevices.Items.Count > 0){
                m_pOutDevices.SelectedIndex = 0;
            }
        }

        #endregion

    }
}
