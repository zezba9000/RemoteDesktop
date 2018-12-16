using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

namespace MulticastStreamerXama
{

      public class UDPSender
      {

        public UDPSender(String address, Int32 port, int TTL)
        {
          //Daten übernehmen
          m_Address = address;
          m_Port = port;
          m_TTL = TTL;

          //Initialisieren
          Init();
        }

        //Attribute 
        private Socket m_Socket;
        private IPEndPoint m_EndPoint;
        private EndPoint m_remote_EndPoint;
        private String m_Address;
        private Int32 m_Port;
        private Int32 m_TTL;

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="args"></param>
        private void Init()
        {
          //Zieladresse
          IPAddress destAddr = IPAddress.Parse(m_Address);
          //Multicast Socket
          m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

          //if (!isBroadcast)
          //{
          //  //Setze TTL
          //    m_Socket.SetSocketOption(SocketOptionLevel.IP,SocketOptionName.MulticastTimeToLive, m_TTL);
          //}
          //else
          //{
          //    //DEBUG: try change to Multicast to Broadcast for communicate 3rd party Andoid reciever app
          //    m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 16);
          //    m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
          //}

          
          //Generiere Endpoint (local bind)
          m_EndPoint = new IPEndPoint(destAddr, m_Port);

          // recognize remote app address (block until recieve any message)
          m_Socket.ReceiveFrom(new byte[1024], ref m_remote_EndPoint);
        }

        /// <summary>
        /// Close
        /// </summary>
        public void Close()
        {
          m_Socket.Close();
        }
        /// <summary>
        /// Bytes versenden
        /// </summary>
        /// <param name="args"></param>
        public void SendBytes(Byte[] bytes)
        {
          m_Socket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, m_EndPoint);
        }
        /// <summary>
        /// Text versenden
        /// </summary>
        /// <param name="str"></param>
        public void SendText(String str)
        {
          this.SendBytes(Encoding.ASCII.GetBytes(str));
        }
      }

    /// <summary>
    /// FormMain
    /// </summary>
    public partial class FormMain : Form
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            Init();
        }

        //Attribute
        private UDPSender m_UDPSender;
        private WinSound.Recorder m_Recorder = new WinSound.Recorder();
        private Configuration Config = new Configuration();
        private String ConfigFileName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "config.xml");
        private bool m_IsFormMain = true;
        private System.Windows.Forms.Timer m_TimerProgressBarFile = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer m_TimerProgressBarJitterBuffer = new System.Windows.Forms.Timer();
        private int m_CurrentRTPBufferPos = 0;
        private Byte[] m_FilePayloadBuffer;
        private int m_RTPPartsLength = 0;
        Byte[] m_PartByte;
        private bool m_IsTimerStreamRunning = false;
        private uint m_Milliseconds = 20;
        private WinSound.EventTimer m_TimerStream = new WinSound.EventTimer();
        private bool m_Loop = false;
        private WinSound.JitterBuffer m_JitterBuffer;
        private uint m_RecorderFactor = 4;
        private uint m_JitterBufferCount = 20;
        private long m_SequenceNumber = 4596;
        private long m_TimeStamp = 0;
        private int m_Version = 2;
        private bool m_Padding = false;
        private bool m_Extension = false;
        private int m_CSRCCount = 0;
        private bool m_Marker = false;
        private int m_PayloadType = 0;
        private uint m_SourceId = 0;
        WinSound.WaveFileHeader m_FileHeader = new WinSound.WaveFileHeader();

        /// <summary>
        /// Init
        /// </summary>
        private void Init()
        {
            try
            {
                InitComboboxes();
                LoadConfig();
                InitRecorder();
                InitTimerShowProgressBarFile();
                InitTimerStream();
                InitJitterBuffer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Init()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// InitJitterBuffer
        /// </summary>
        private void InitJitterBuffer()
        {
            //Wenn vorhanden
            if (m_JitterBuffer != null)
            {
                m_JitterBuffer.DataAvailable -= new WinSound.JitterBuffer.DelegateDataAvailable(OnDataAvailable);
            }

            //Neu erstellen
            m_JitterBuffer = new WinSound.JitterBuffer(null, m_JitterBufferCount, m_Milliseconds);
            m_JitterBuffer.DataAvailable += new WinSound.JitterBuffer.DelegateDataAvailable(OnDataAvailable);
        }
        /// <summary>
        /// InitTimerStream
        /// </summary>
        private void InitTimerStream()
        {
            m_TimerStream.TimerTick += new WinSound.EventTimer.DelegateTimerTick(OnTimerStream);
        }
        /// <summary>
        /// StartTimerStream
        /// </summary>
        private void StartTimerStream()
        {
            //WaveFile Header
            m_FileHeader = WinSound.WaveFile.Read(Config.FileName);
            //Buffer erzeugen
            FillRTPBufferWithPayloadData(m_FileHeader);
            //Bytes für die einzelnen Datenpakete
            m_PartByte = new Byte[m_RTPPartsLength];
            //Buffer Position
            m_CurrentRTPBufferPos = 0;

            //ProgressBar
            ProgressBarFile.Invoke(new MethodInvoker(delegate()
            {
                ProgressBarFile.Value = 0;
                ProgressBarFile.Maximum = m_FilePayloadBuffer.Length;
            }));

            //Timer starten
            m_TimerStream.Start(m_Milliseconds, 0);
            m_IsTimerStreamRunning = m_TimerStream.IsRunning;
        }
        /// <summary>
        /// StopTimerStream
        /// </summary>
        private void StopTimerStream()
        {
            if (m_TimerStream.IsRunning)
            {
                //QueueTimer beenden
                m_TimerStream.Stop();

                //Variablen setzen
                m_IsTimerStreamRunning = m_TimerStream.IsRunning;
                m_UDPSender.Close();
                m_UDPSender = null;
                m_CurrentRTPBufferPos = 0;
                OnFileStreamingEnd();
            }
        }
        /// <summary>
        /// OnTimerStream
        /// </summary>
        /// <param name="lpParameter"></param>
        /// <param name="TimerOrWaitFired"></param>
        private void OnTimerStream()
        {
            try
            {
                //Wenn noch aktiv
                if (m_IsTimerStreamRunning)
                {
                    if ((m_CurrentRTPBufferPos + m_RTPPartsLength) <= m_FilePayloadBuffer.Length)
                    {
                        //Bytes senden
                        Array.Copy(m_FilePayloadBuffer, m_CurrentRTPBufferPos, m_PartByte, 0, m_RTPPartsLength);
                        m_CurrentRTPBufferPos += m_RTPPartsLength;
                        WinSound.RTPPacket rtp = ToRTPPacket(m_PartByte, m_FileHeader.BitsPerSample, m_FileHeader.Channels);
                        m_UDPSender.SendBytes(rtp.ToBytes());
                    }
                    else
                    {
                        //Rest-Bytes senden
                        int rest = m_FilePayloadBuffer.Length - m_CurrentRTPBufferPos;
                        Byte[] restBytes = new Byte[m_PartByte.Length];
                        Array.Copy(m_FilePayloadBuffer, m_CurrentRTPBufferPos, restBytes, 0, rest);
                        WinSound.RTPPacket rtp = ToRTPPacket(restBytes, m_FileHeader.BitsPerSample, m_FileHeader.Channels);
                        m_UDPSender.SendBytes(rtp.ToBytes());

                        if (m_Loop == false)
                        {
                            //QueueTimer beenden
                            StopTimerStream();
                        }
                        else
                        {
                            //Von vorne beginnen
                            m_CurrentRTPBufferPos = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StopTimerStream();
            }
        }
        /// <summary>
        /// InitTimerShowProgressBarFile
        /// </summary>
        private void InitTimerShowProgressBarFile()
        {
            m_TimerProgressBarFile = new System.Windows.Forms.Timer();
            m_TimerProgressBarFile.Interval = 60;
            m_TimerProgressBarFile.Tick += new EventHandler(OnTimerProgressBarFile);
        }
        /// <summary>
        /// OnTimerProgressBarFile
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private void OnTimerProgressBarFile(Object obj, EventArgs e)
        {
            try
            {
                if (m_IsTimerStreamRunning)
                {
                    ProgressBarFile.Value = Math.Min(m_CurrentRTPBufferPos, ProgressBarFile.Maximum);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("FormMain.cs | OnTimerProgressBarFile() | {0}", ex.Message));
                m_TimerProgressBarFile.Stop();
            }
        }
        /// <summary>
        /// InitRecorder
        /// </summary>
        private void InitRecorder()
        {
            m_Recorder.DataRecorded += new WinSound.Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard);
            m_Recorder.RecordingStopped += new WinSound.Recorder.DelegateStopped(OnRecordingStopped);
        }
        /// <summary>
        /// InitComboboxes
        /// </summary>
        private void InitComboboxes()
        {
            ComboboxSoundDeviceName.Items.Clear();
            List<String> names = WinSound.WinSound.GetRecordingNames();

            foreach (String name in names.Where(x => x != null))
            {
                ComboboxSoundDeviceName.Items.Add(name);
            }

            if (ComboboxSoundDeviceName.Items.Count > 0)
            {
                ComboboxSoundDeviceName.SelectedIndex = 0;
            }
            ComboboxSamplesPerSecond.SelectedIndex = 1;
            ComboboxBitsPerSample.SelectedIndex = 0;
            ComboboxChannels.SelectedIndex = 0;
            ComboboxBufferCount.SelectedIndex = 4;
        }
        /// <summary>
        /// Config
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// Config
            /// </summary>
            public Configuration()
            {

            }

            //Attribute
            public String localAddress = "";
            public String SoundDeviceName = "";
            public int localPort = 0;
            public int SamplesPerSecond = 8000;
            public short BitsPerSample = 16;
            public short Channels = 2;
            public Int32 BufferCount = 8;
            public String FileName = "";
            public bool Loop = false;
            public bool UseJitterBuffer = true;
        }
        //----------------------------------------------------------------
        //Daten schreiben
        //----------------------------------------------------------------
        private void SaveConfig()
        {
            try
            {
                FormToConfig();
                XmlSerializer ser = new XmlSerializer(typeof(Configuration));
                FileStream stream = new FileStream(ConfigFileName, FileMode.Create);
                ser.Serialize(stream, this.Config);
                stream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //----------------------------------------------------------------
        //Daten lesen
        //----------------------------------------------------------------
        private void LoadConfig()
        {
            try
            {
                //Wenn die Datei existiert
                if (File.Exists(ConfigFileName))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Configuration));
                    StreamReader sr = new StreamReader(ConfigFileName);
                    Config = (Configuration)ser.Deserialize(sr);
                    sr.Close();
                }

                //Daten anzeigen
                ConfigToForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// FormToConfig
        /// </summary>
        /// <returns></returns>
        private bool FormToConfig()
        {
            try
            {
                Config.SoundDeviceName = ComboboxSoundDeviceName.SelectedItem != null ? ComboboxSoundDeviceName.SelectedItem.ToString() : "";
                Config.localAddress = TextBoxMulticastAddress.Text;
                Config.localPort = Convert.ToInt32(TextBoxMulticastPort.Text);
                Config.SamplesPerSecond = Convert.ToInt32(ComboboxSamplesPerSecond.SelectedItem.ToString());
                Config.BitsPerSample = Convert.ToInt16(ComboboxBitsPerSample.SelectedItem.ToString());
                Config.Channels = Convert.ToInt16(ComboboxChannels.SelectedItem.ToString());
                Config.BufferCount = Convert.ToInt32(ComboboxBufferCount.SelectedItem.ToString());
                Config.FileName = TextBoxFileName.Text;
                Config.Loop = CheckBoxLoop.Checked;
                Config.UseJitterBuffer = CheckBoxJitterBuffer.Checked;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler bei der Eingabe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        /// <summary>
        /// ConfigToForm
        /// </summary>
        /// <returns></returns>
        private bool ConfigToForm()
        {
            try
            {
                ComboboxSoundDeviceName.SelectedIndex = ComboboxSoundDeviceName.FindString(Config.SoundDeviceName);
                TextBoxMulticastAddress.Text = Config.localAddress;
                TextBoxMulticastPort.Text = Config.localPort.ToString();
                ComboboxSamplesPerSecond.SelectedIndex = ComboboxSamplesPerSecond.FindString(Config.SamplesPerSecond.ToString());
                ComboboxBitsPerSample.SelectedIndex = ComboboxBitsPerSample.FindString(Config.BitsPerSample.ToString());
                ComboboxChannels.SelectedIndex = ComboboxChannels.FindString(Config.Channels.ToString());
                ComboboxBufferCount.SelectedIndex = ComboboxBufferCount.FindString(Config.BufferCount.ToString());
                TextBoxFileName.Text = Config.FileName;
                CheckBoxLoop.Checked = Config.Loop;
                CheckBoxJitterBuffer.Checked = Config.UseJitterBuffer;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// StartRecording
        /// </summary>
        private void StartRecording()
        {
            try
            {
                //Buffer Grösse je nach JitterBuffer berechnen
                int bufferSize = 0;
                if (Config.UseJitterBuffer)
                {
                    bufferSize = WinSound.Utils.GetBytesPerInterval((uint)Config.SamplesPerSecond, Config.BitsPerSample, Config.Channels) * (int)m_RecorderFactor;
                }
                else
                {
                    bufferSize = WinSound.Utils.GetBytesPerInterval((uint)Config.SamplesPerSecond, Config.BitsPerSample, Config.Channels);
                }

                if (bufferSize > 0)
                {
                    if (m_Recorder.Start(Config.SoundDeviceName, Config.SamplesPerSecond, Config.BitsPerSample, Config.Channels, Config.BufferCount, bufferSize))
                    {
                        ShowStarted_StreamSound();

                        //Wenn JitterBuffer
                        if (Config.UseJitterBuffer)
                        {
                            m_JitterBuffer.Start();
                            m_TimerProgressBarJitterBuffer.Start();
                        }
                    }
                    else
                    {
                        ShowError();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("BufferSize must be greater than 0. BufferSize: {0}", bufferSize));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// OnFileStreamingEnd
        /// </summary>
        private void OnFileStreamingEnd()
        {
            //Wenn Formular noch aktiv
            if (m_IsFormMain)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    ShowStopped_StreamFile();
                }));
            }
        }
        /// <summary>
        /// ToRTPData
        /// </summary>
        /// <param name="linearData"></param>
        /// <param name="bitsPerSample"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        private Byte[] ToRTPData(Byte[] data, int bitsPerSample, int channels)
        {
            //Neues RTP Packet erstellen
            WinSound.RTPPacket rtp = ToRTPPacket(data, bitsPerSample, channels);
            //RTPHeader in Bytes erstellen
            Byte[] rtpBytes = rtp.ToBytes();
            //Fertig
            return rtpBytes;
        }
        /// <summary>
        /// ToRTPPacket
        /// </summary>
        /// <param name="linearData"></param>
        /// <param name="bitsPerSample"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        private WinSound.RTPPacket ToRTPPacket(Byte[] linearData, int bitsPerSample, int channels)
        {
            //Daten Nach MuLaw umwandeln
            Byte[] mulaws = WinSound.Utils.LinearToMulaw(linearData, bitsPerSample, channels);

            //Neues RTP Packet erstellen
            WinSound.RTPPacket rtp = new WinSound.RTPPacket();

            //Werte übernehmen
            rtp.Data = mulaws;
            rtp.SourceId = m_SourceId;
            rtp.CSRCCount = m_CSRCCount;
            rtp.Extension = m_Extension;
            rtp.HeaderLength = WinSound.RTPPacket.MinHeaderLength;
            rtp.Marker = m_Marker;
            rtp.Padding = m_Padding;
            rtp.PayloadType = m_PayloadType;
            rtp.Version = m_Version;

            //RTP Header aktualisieren
            try
            {
                rtp.SequenceNumber = Convert.ToUInt16(m_SequenceNumber);
                m_SequenceNumber++;
            }
            catch (Exception)
            {
                m_SequenceNumber = 0;
            }
            try
            {
                rtp.Timestamp = Convert.ToUInt32(m_TimeStamp);
                m_TimeStamp += mulaws.Length;
            }
            catch (Exception)
            {
                m_TimeStamp = 0;
            }

            //Fertig
            return rtp;
        }
        /// <summary>
        /// StartStreamingFile
        /// </summary>
        private void StartStreamingFile()
        {
            //Wenn Datei vorhanden
            if (File.Exists(Config.FileName))
            {
                if (m_IsTimerStreamRunning == false)
                {
                    //Header auslesen
                    WinSound.WaveFileHeader header = WinSound.WaveFile.Read(Config.FileName);

                    //MulticastSender starten
                    m_UDPSender = new UDPSender(Config.localAddress, Config.localPort, 10);

                    //QueueTimer starten
                    m_TimerProgressBarFile.Start();
                    StartTimerStream();
                }
            }
        }
        /// <summary>
        /// StopStreamingFile
        /// </summary>
        private void StopStreamingFile()
        {
            m_IsTimerStreamRunning = false;
            //QueueTimer beenden
            m_TimerProgressBarFile.Stop();
            StopTimerStream();
        }
        /// <summary>
        /// FillRTPBufferWithPayloadData
        /// </summary>
        /// <param name="header"></param>
        private void FillRTPBufferWithPayloadData(WinSound.WaveFileHeader header)
        {
            m_RTPPartsLength = WinSound.Utils.GetBytesPerInterval(header.SamplesPerSecond, header.BitsPerSample, header.Channels);
            m_FilePayloadBuffer = header.Payload;
        }
        /// <summary>
        /// OnDataReceivedFromSoundcard
        /// </summary>
        /// <param name="linearData"></param>
        private void OnDataReceivedFromSoundcard(Byte[] data)
        {
            try
            {
                lock (this)
                {
                    if (m_UDPSender != null)
                    {
                        //Wenn Form noch aktiv
                        if (m_IsFormMain)
                        {
                            //Wenn JitterBuffer
                            if (Config.UseJitterBuffer)
                            {
                                //Sounddaten in kleinere Einzelteile zerlegen
                                int bytesPerInterval = WinSound.Utils.GetBytesPerInterval((uint)Config.SamplesPerSecond, Config.BitsPerSample, Config.Channels);
                                int count = data.Length / bytesPerInterval;
                                int currentPos = 0;
                                for (int i = 0; i < count; i++)
                                {
                                    //Teilstück in RTP Packet umwandeln
                                    Byte[] partBytes = new Byte[bytesPerInterval];
                                    Array.Copy(data, currentPos, partBytes, 0, bytesPerInterval);
                                    currentPos += bytesPerInterval;
                                    WinSound.RTPPacket rtp = ToRTPPacket(partBytes, Config.BitsPerSample, Config.Channels);
                                    //In Buffer legen
                                    m_JitterBuffer.AddData(rtp);
                                }
                            }
                            else
                            {
                                //Alles in RTP Packet umwandeln
                                Byte[] rtp = ToRTPData(data, Config.BitsPerSample, Config.Channels);
                                //Absenden
                                m_UDPSender.SendBytes(rtp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// OnDataAvailable
        /// </summary>
        /// <param name="packet"></param>
        private void OnDataAvailable(Object sender, WinSound.RTPPacket rtp)
        {
            try
            {
                if (m_UDPSender != null)
                {
                    if (m_IsFormMain)
                    {
                        //RTP Packet in Bytes umwandeln
                        Byte[] rtpBytes = rtp.ToBytes();
                        //Absenden
                        m_UDPSender.SendBytes(rtpBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// OnRecordingStopped
        /// </summary>
        private void OnRecordingStopped()
        {
            try
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    ShowStopped_StreamSound();

                    //Wenn JitterBuffer
                    if (Config.UseJitterBuffer)
                    {
                        m_TimerProgressBarJitterBuffer.Stop();
                        m_JitterBuffer.Stop();
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// ShowStarted_StreamSound
        /// </summary>
        private void ShowStarted_StreamSound()
        {
            ButtonStart.BackColor = Color.DarkGreen;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType() == typeof(GroupBox))
                {
                    foreach (Control ctrlNext in ctrl.Controls)
                    {
                        if (ctrlNext.GetType() == typeof(ComboBox))
                        {
                            ctrlNext.Enabled = false;
                        }
                        else if (ctrlNext.GetType() == typeof(NumericUpDown))
                        {
                            ctrlNext.Enabled = false;
                        }
                        else if (ctrlNext.GetType() == typeof(TextBox))
                        {
                            ctrlNext.Enabled = false;
                        }
                    }
                }
            }

            //File Controls
            ButtonStreamFile.Enabled = false;
            ListBoxFile.Enabled = false;
            ButtonOpenFileDialog.Enabled = false;
            CheckBoxLoop.Enabled = false;
            CheckBoxJitterBuffer.Enabled = false;
        }
        /// <summary>
        /// ShowStarted_StreamFile
        /// </summary>
        private void ShowStarted_StreamFile()
        {
            ButtonStreamFile.BackColor = Color.DarkGreen;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType() == typeof(GroupBox))
                {
                    foreach (Control ctrlNext in ctrl.Controls)
                    {
                        if (ctrlNext.GetType() == typeof(ComboBox))
                        {
                            ctrlNext.Enabled = false;
                        }
                        else if (ctrlNext.GetType() == typeof(NumericUpDown))
                        {
                            ctrlNext.Enabled = false;
                        }
                        else if (ctrlNext.GetType() == typeof(TextBox))
                        {
                            ctrlNext.Enabled = false;
                        }
                    }
                }
            }

            //File Controls
            ButtonStart.Enabled = false;
            ButtonOpenFileDialog.Enabled = false;
            CheckBoxLoop.Enabled = false;
            CheckBoxJitterBuffer.Enabled = false;
        }
        /// <summary>
        /// ShowStopped_StreamSound
        /// </summary>
        private void ShowStopped_StreamSound()
        {
            ButtonStart.BackColor = SystemColors.Control;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType() == typeof(GroupBox))
                {
                    foreach (Control ctrlNext in ctrl.Controls)
                    {
                        if (ctrlNext.GetType() == typeof(ComboBox))
                        {
                            ctrlNext.Enabled = true;
                        }
                        else if (ctrlNext.GetType() == typeof(NumericUpDown))
                        {
                            ctrlNext.Enabled = true;
                        }
                        else if (ctrlNext.GetType() == typeof(TextBox))
                        {
                            ctrlNext.Enabled = true;
                        }
                    }
                }
            }

            //File Controls
            ButtonStreamFile.Enabled = true;
            ButtonOpenFileDialog.Enabled = true;
            ListBoxFile.Enabled = true;
            CheckBoxLoop.Enabled = true;
            CheckBoxJitterBuffer.Enabled = true;
        }
        /// <summary>
        /// ShowStopped_StreamFile
        /// </summary>
        private void ShowStopped_StreamFile()
        {
            ButtonStreamFile.BackColor = SystemColors.Control;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType() == typeof(GroupBox))
                {
                    foreach (Control ctrlNext in ctrl.Controls)
                    {
                        if (ctrlNext.GetType() == typeof(ComboBox))
                        {
                            ctrlNext.Enabled = true;
                        }
                        else if (ctrlNext.GetType() == typeof(NumericUpDown))
                        {
                            ctrlNext.Enabled = true;
                        }
                        else if (ctrlNext.GetType() == typeof(TextBox))
                        {
                            ctrlNext.Enabled = true;
                        }
                    }
                }
            }

            ButtonStart.Enabled = true;
            ButtonOpenFileDialog.Enabled = true;
            ProgressBarFile.Value = 0;
            CheckBoxLoop.Enabled = true;
            CheckBoxJitterBuffer.Enabled = true;
        }
        /// <summary>
        /// ShowError
        /// </summary>
        private void ShowError()
        {
            ButtonStart.BackColor = Color.Red;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType() == typeof(GroupBox))
                {
                    foreach (Control ctrlNext in ctrl.Controls)
                    {
                        if (ctrlNext.GetType() == typeof(ComboBox))
                        {
                            ctrlNext.Enabled = true;
                        }
                        else if (ctrlNext.GetType() == typeof(NumericUpDown))
                        {
                            ctrlNext.Enabled = true;
                        }
                        else if (ctrlNext.GetType() == typeof(TextBox))
                        {
                            ctrlNext.Enabled = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// ShowFileInfoInListBox
        /// </summary>
        private void ShowFileInfoInListBox()
        {
            try
            {
                if (File.Exists(TextBoxFileName.Text))
                {
                    WinSound.WaveFileHeader header = WinSound.WaveFile.Read(TextBoxFileName.Text);
                    if (header.Payload.Length > 0)
                    {
                        //Grösse berechnen
                        double fileSizeInMB = (double)header.DATASize / 1024.0 / 1024.0;
                        //Dauer berechnen
                        TimeSpan ts = header.Duration;

                        ListBoxFile.Items.Clear();
                        ListBoxFile.Items.Add(String.Format("Size: {0:0.0} MB   Duration: {1:0.0.0}", fileSizeInMB, ts));
                        ListBoxFile.Items.Add("");
                        ListBoxFile.Items.Add(String.Format("SamplesPerSecond: {0}", header.SamplesPerSecond));
                        ListBoxFile.Items.Add(String.Format("BitsPerSample: {0}", header.BitsPerSample));
                        ListBoxFile.Items.Add(String.Format("Channels: {0}", header.Channels));
                    }
                }
                else
                {
                    ListBoxFile.Items.Clear();
                    ListBoxFile.Items.Add("Select a valid Wav-File");
                }
            }
            catch (Exception ex)
            {
                ListBoxFile.Items.Clear();
                ListBoxFile.Items.Add(String.Format("File Error: {0}", ex.Message));
            }
        }
        /// <summary>
        /// FormMain_FormClosing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //Form ist geschlossen
                m_IsFormMain = false;

                //Wenn FileStreaming
                if (m_IsTimerStreamRunning)
                {
                    //Beenden
                    StopStreamingFile();
                }

                if (m_UDPSender != null)
                {
                    m_UDPSender.Close();
                }

                m_Recorder.DataRecorded -= new WinSound.Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard);
                m_Recorder.RecordingStopped -= new WinSound.Recorder.DelegateStopped(OnRecordingStopped);
                m_Recorder.Stop();
                SaveConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        ///  ButtonStart_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            try
            {
                //Daten ermitteln
                FormToConfig();

                if (m_Recorder.Started == false)
                {
                    //Starten
                    m_UDPSender = new UDPSender(Config.localAddress, Config.localPort, 10);
                    StartRecording();
                    ShowStarted_StreamSound();
                }
                else
                {
                    //Schliessen
                    m_UDPSender.Close();
                    m_UDPSender = null;
                    m_Recorder.Stop();

                    //Wenn JitterBuffer
                    if (Config.UseJitterBuffer)
                    {
                        m_TimerProgressBarJitterBuffer.Stop();
                        m_JitterBuffer.Stop();
                    }

                    //Warten bis Aufnahme beendet
                    while (m_Recorder.Started)
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// ButtonOpenFileDialog_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOpenFileDialog_Click(object sender, EventArgs e)
        {
            if (OpenFileDialogMain.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxFileName.Text = OpenFileDialogMain.FileName;
            }
        }
        /// <summary>
        /// ButtonStreamFile_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStreamFile_Click(object sender, EventArgs e)
        {
            try
            {
                //Daten ermitteln
                FormToConfig();

                //Wenn die Datei existiert
                if (File.Exists(Config.FileName))
                {
                    if (m_IsTimerStreamRunning == false)
                    {
                        //Starten
                        StartStreamingFile();
                    }
                    else
                    {
                        //Schliessen
                        StopStreamingFile();
                    }

                    //Je nach Zustand
                    if (m_IsTimerStreamRunning)
                    {
                        ShowStarted_StreamFile();
                    }
                    else
                    {
                        ShowStopped_StreamFile();
                    }
                }
                else
                {
                    MessageBox.Show("File not found", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// TextBoxFileName_TextChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxFileName_TextChanged(object sender, EventArgs e)
        {
            Config.FileName = TextBoxFileName.Text;
            ShowFileInfoInListBox();
        }
        /// <summary>
        /// CheckBoxStreamFile_CheckedChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxStreamFile_CheckedChanged(object sender, EventArgs e)
        {
            //QueueTimer starten oder beenden
            m_TimerProgressBarFile.Enabled = CheckBoxStreamFile.Checked;

            //Ansicht ändern
            if (CheckBoxStreamFile.Checked)
            {
                this.Height = 400;

                //Wenn File im Stream
                if (m_IsTimerStreamRunning)
                {
                    ProgressBarFile.Value = m_CurrentRTPBufferPos;
                }
            }
            else
            {
                this.Height = 245;
            }
        }
        /// <summary>
        /// CheckBoxLoop_CheckedChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxLoop_CheckedChanged(object sender, EventArgs e)
        {
            m_Loop = CheckBoxLoop.Checked;
        }
        /// <summary>
        /// CheckBoxJitterBuffer_CheckedChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxJitterBuffer_CheckedChanged(object sender, EventArgs e)
        {
            Config.UseJitterBuffer = CheckBoxJitterBuffer.Checked;
        }
    }
}
