using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace RemoteDesktop.Core
{
	public class RTPSoundStreamPlayer
	{
		/// <summary>
		/// Konstruktor
		/// </summary>
		public RTPSoundStreamPlayer()
		{
			//InitializeComponent();
			Init();
		}

		//Attribute
		RTPReceiver m_Receiver;
		WinSound.Player m_Player;
		List<String> m_Data = new List<string>();
		private Configuration Config = new Configuration();
		//private String ConfigFileName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "config.xml");
		//Graphics GraphicsPanelCurve;
		//Pen PenCurve;
		Byte[] m_BytesToDraw;
		//System.Windows.Forms.Timer m_TimerDrawCurve;
		//System.Windows.Forms.Timer m_TimerDrawProgressBar;
		//System.Windows.Forms.Timer m_TimerDrawMeasurements;
		bool IsDrawCurve = false;
		private WinSound.JitterBuffer m_JitterBuffer = new WinSound.JitterBuffer(null, 20, 0);
		private uint m_JitterBufferLength = 20;
		private WinSound.Stopwatch m_Stopwatch = new WinSound.Stopwatch();
		//private double m_MeasurementTimeOne = 0;
		//private double m_MeasurementTimeTwo = 0;
		//private Queue<double> m_QueueTimeDiffs = new Queue<double>();
		bool m_TimeMeasurementToggler = false;

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
			public String ServerAddress = "192.168.0.11";
			public String SoundDeviceName = "";
			public int ServerPort = 10000;
			public int SamplesPerSecond = 8000;
			public short BitsPerSample = 16;
			public short Channels = 2;
			public Int32 PacketSize = 4096;
			public Int32 BufferCount = 8;
			public uint JitterBuffer = 20;
		}

		/// <summary>
		/// Start
		/// </summary>
		private void Init()
		{
			try
			{
				//WinSoundServer
				m_Player = new WinSound.Player();
				//Comboboxen
				//InitComboboxes();
				//Laden
				//LoadConfig();
				//Noch Nicht verbunden
				//ShowDisconnected();
				//Sonstiges
				//InitGraphics();
			}
			catch (Exception ex)
			{
                //MessageBox.Show(ex.Message, "Fehler beim Initialisieren", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(ex);
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
			//m_JitterBuffer = new WinSound.JitterBuffer(null, (uint)NumericUpDownJitterBuffer.Value, 20);
			m_JitterBuffer = new WinSound.JitterBuffer(null, Config.JitterBuffer, 20);
			m_JitterBuffer.DataAvailable += new WinSound.JitterBuffer.DelegateDataAvailable(OnDataAvailable);

			//ProgressBar anpassen
			//InitProgressBarJitterBuffer();
		}
		/// <summary>
		/// Gibt an ob der Jitter Buffer verwendet werden soll
		/// </summary>
		/// <returns></returns>
		private bool UseJitterBuffer
		{
			get
			{
				if (m_JitterBuffer != null)
				{
					return m_JitterBufferLength >= 2;
				}
				return false;
			}
		}

		/// <summary>
		/// OnDataReceived
		/// </summary>
		/// <param name="strMessage"></param>
        private void OnDataReceived(RTPReceiver rtr, Byte[] bytes)
		{
			try
			{
				//Wenn der Player gestartet wurde
				if (m_Player.Opened && m_Receiver.Connected)
				{
					//RTP Header auslesen
					WinSound.RTPPacket rtp = new WinSound.RTPPacket(bytes);

					////Wenn Anzeige
					//if (IsDrawCurve)
					//{
					//	TimeMeasurement();
					//	m_BytesToDraw = rtp.Data;
					//}

					//Wenn Header korrekt
					if (rtp.Data != null)
					{
						//Wenn JitterBuffer verwendet werden soll
						if (UseJitterBuffer)
						{
							m_JitterBuffer.AddData(rtp);
						}
						else
						{
							//Nach Linear umwandeln
							Byte[] linearBytes = WinSound.Utils.MuLawToLinear(rtp.Data, Config.BitsPerSample, Config.Channels);
							//Abspielen
							m_Player.PlayData(linearBytes, false);
						}
					}
				}
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex);
			}
		}

		/// <summary>
		/// OnDataAvailable
		/// </summary>
		/// <param name="packet"></param>
		private void OnDataAvailable(Object sender, WinSound.RTPPacket rtp)
		{
			//Nach Linear umwandeln
			Byte[] linearBytes = WinSound.Utils.MuLawToLinear(rtp.Data, Config.BitsPerSample, Config.Channels);
			//Abspielen
			m_Player.PlayData(linearBytes, false);
		}

		/// <summary>
		/// OnDisconnected
		/// </summary>
		private void OnDisconnected(string reason)
		{
			try
			{
				this.Invoke(new MethodInvoker(delegate()
				{
					//Player beenden
					m_Player.Close();
					ShowState();
				}));
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex);
			}
		}

        private void togglePlaying()
        {
			//Wenn geöffnet
			if (m_Player.Opened)
			{
				//Wenn JitterBuffer
				if (UseJitterBuffer)
				{
					m_JitterBuffer.Stop();
					//Timer beenden
					//StopTimerDrawProgressBar();
				}

				//MulticastReceiver
				m_Receiver.Disconnect();
				//WinSound Player
				m_Player.Close();
			}
			else
			{
				//if (FormToConfig() == true)
				//{
					//Zeitmessungen zurücksetzen
					//ResetTimeMeasurements();

				//MulticastReceiver
				m_Receiver = new RTPReceiver(Config.PacketSize);
				m_Receiver.DataReceived2 += new RTPReceiver.DelegateDataReceived2(OnDataReceived);
				m_Receiver.Disconnected += new RTPReceiver.DelegateDisconnected(OnDisconnected);
				m_Receiver.Connect(Config.ServerAddress, Config.ServerPort);

				//WinSound Player öffnen
				m_Player.Open(Config.SoundDeviceName, Config.SamplesPerSecond, Config.BitsPerSample, Config.Channels, Config.BufferCount);

				//Wenn JitterBuffer
				if (UseJitterBuffer)
				{
					InitJitterBuffer();
					m_JitterBuffer.Start();
					//StartTimerDrawProgressBar();
				}

				//}
			}
        }

		/// <summary>
		/// TimeMeasurement
		/// </summary>
		//private void TimeMeasurement()
		//{
		//	try
		//	{
		//		//Messen
		//		if (m_MeasurementTimeOne == 0)
		//		{
		//			m_MeasurementTimeOne = m_Stopwatch.ElapsedMilliseconds;
		//		}
		//		else if (m_MeasurementTimeTwo == 0)
		//		{
		//			m_MeasurementTimeTwo = m_Stopwatch.ElapsedMilliseconds;
		//		}
		//		//Wenn Messung komplett
		//		if (m_MeasurementTimeOne != 0 && m_MeasurementTimeTwo != 0)
		//		{
		//			if (m_TimeMeasurementToggler)
		//			{
		//				m_QueueTimeDiffs.Enqueue(m_MeasurementTimeOne - m_MeasurementTimeTwo);
		//				m_MeasurementTimeTwo = 0;
		//			}
		//			else
		//			{
		//				m_QueueTimeDiffs.Enqueue(m_MeasurementTimeTwo - m_MeasurementTimeOne);
		//				m_MeasurementTimeOne = 0;
		//			}
		//			//Nächste Messung vorbereiten
		//			m_TimeMeasurementToggler = !m_TimeMeasurementToggler;
		//		}	
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(String.Format("FormMain.cs | TimeMeasurement() | {0}", ex.Message));
		//	}
		//}

		//private bool FormToConfig()
		//{
		//	try
		//	{
		//		Config.SoundDeviceName = ComboboxSoundDeviceName.SelectedIndex != 0 ? ComboboxSoundDeviceName.SelectedItem.ToString() : "";
		//		Config.ServerAddress = TextBoxMCAddress.Text;
		//		Config.ServerPort = Convert.ToInt32(TextBoxMCPort.Text);
		//		Config.SamplesPerSecond = Convert.ToInt32(ComboboxSamplesPerSecond.SelectedItem.ToString());
		//		Config.BitsPerSample = Convert.ToInt16(ComboboxBitsPerSample.SelectedItem.ToString());
		//		Config.Channels = Convert.ToInt16(ComboboxChannels.SelectedItem.ToString());
		//		Config.PacketSize = (Int32)NumericUpDownPacketSize.Value;
		//		Config.BufferCount = Convert.ToInt32(ComboboxBufferCount.SelectedItem.ToString());
		//		Config.JitterBuffer = Convert.ToUInt32(NumericUpDownJitterBuffer.Value);
		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show(ex.Message, "Fehler bei der Eingabe", MessageBoxButtons.OK, MessageBoxIcon.Error);
		//		return false;
		//	}
		//}

		//private void SaveConfig()
		//{
		//	try
		//	{
		//		FormToConfig();
		//		XmlSerializer ser = new XmlSerializer(typeof(Configuration));
		//		FileStream stream = new FileStream(ConfigFileName, FileMode.Create);
		//		ser.Serialize(stream, this.Config);
		//		stream.Close();
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show(ex.Message);
		//	}
		//}

		//private void LoadConfig()
		//{
		//	try
		//	{
		//		//Wenn die Datei existiert
		//		if (File.Exists(ConfigFileName))
		//		{
		//			XmlSerializer ser = new XmlSerializer(typeof(Configuration));
		//			StreamReader sr = new StreamReader(ConfigFileName);
		//			Config = (Configuration)ser.Deserialize(sr);
		//			sr.Close();
		//		}
		//		//Daten anzeigen
		//		ConfigToForm();
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show(ex.Message);
		//	}
		//}

		/// <summary>
		/// InitProgressBarJitterBuffer
		/// </summary>
		//private void InitProgressBarJitterBuffer()
		//{
		//	//Allgemein
		//	ProgressBarJitterBuffer.Value = 0;

		//	//Wenn der JitterBuffer verwendet werden soll
		//	if (UseJitterBuffer)
		//	{
		//		ProgressBarJitterBuffer.Enabled = true;
		//		ProgressBarJitterBuffer.Maximum = (int)m_JitterBuffer.Maximum;
		//	}
		//	else
		//	{
		//		ProgressBarJitterBuffer.Enabled = false;
		//	}
		//}

		/// <summary>
        /// DrawCurveUlaw
		/// </summary>
		/// <param name="bytes"></param>
		//private void DrawCurveUlaw(Byte[] bytes)
		//{
  //          if (bytes.Length > 0)
  //          {
  //              //Punkte erzeugen
  //              PointF[] points = new PointF[bytes.Length];
  //              for (int i = 0; i < points.Length; i++)
  //              {
  //                  points[i].X = (i + 1) * ((float)PanelCurve.Width / (float)bytes.Length);
  //                  points[i].Y = (PanelCurve.Height - bytes[i] - 10);
  //              }

  //              try
  //              {
  //                  this.Invoke(new MethodInvoker(delegate()
  //                  {
  //                      //Punkte zeichnen
  //                      RectangleF rec = GraphicsPanelCurve.ClipBounds;
  //                      rec.Y += 4194320;
  //                      GraphicsPanelCurve.FillRectangle(Brushes.Black, rec);
  //                      GraphicsPanelCurve.DrawCurve(PenCurve, points);
  //                  }));
  //              }
  //              catch (Exception ex)
  //              {
  //                  System.Diagnostics.Debug.WriteLine(String.Format("FormMain.cs | DrawCurve() | {0}", ex.Message));
  //              }
  //          }
		//}

        /// <summary>
        /// DrawCurveLinear
        /// </summary>
        /// <param name="bytes"></param>
        //private void DrawCurveLinear(Byte[] bytes)
        //{
        //    if (bytes.Length > 0)
        //    {
        //        //Nach Linear umwandeln
        //        int[] linear = WinSound.Utils.MuLawToLinear32(bytes, Config.BitsPerSample, Config.Channels);

        //        //Zeichenskalierungen anbringen
        //        float divisor = Config.BitsPerSample == 8 ? 1 : 400;
        //        float add = Config.BitsPerSample == 8 ? -20 : 150;

        //        //Punkte erzeugen
        //        PointF[] points = new PointF[linear.Length];
        //        for (int i = 0; i < points.Length; i++)
        //        {
        //            points[i].X = (i + 1) * ((float)PanelCurve.Width / (float)linear.Length);
        //            points[i].Y = ((PanelCurve.Height - linear[i]) / divisor) + add;
        //        }

        //        try
        //        {
        //            this.Invoke(new MethodInvoker(delegate()
        //            {
        //                //Punkte zeichnen
        //                RectangleF rec = GraphicsPanelCurve.ClipBounds;
        //                rec.Y += 4194320;
        //                GraphicsPanelCurve.FillRectangle(Brushes.Black, rec);
        //                GraphicsPanelCurve.DrawCurve(PenCurve, points);
        //            }));
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Diagnostics.Debug.WriteLine(String.Format("FormMain.cs | DrawCurve() | {0}", ex.Message));
        //        }
        //    }
        //}

		/// <summary>
		/// OnTimerDrawGraphics
		/// </summary>
		/// <param name="obj"></param>
		//private void OnTimerDrawGraphics(Object obj, EventArgs e)
		//{
		//	try
		//	{
		//		//Curve
		//		if (m_BytesToDraw != null)
		//		{
		//			if (IsDrawCurve)
		//			{
		//				DrawCurveLinear(m_BytesToDraw);
		//				m_BytesToDraw = null;
		//			}
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(ex.Message);
		//	}
		//}

		/// <summary>
		/// OnTimerDrawMeasurements
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="e"></param>
		//private void OnTimerDrawMeasurements(Object obj, EventArgs e)
		//{
		//	try
		//	{
		//		if (IsDrawCurve)
		//		{
		//			//Wenn Messungen vorhanden
		//			if (m_QueueTimeDiffs.Count > 0)
		//			{
		//				//Zeitmessung
		//				DrawTimeMeasurements();
		//			}
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(ex.Message);
		//	}
		//}

		/// <summary>
		/// OnTimerDrawProgressBar
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="e"></param>
		//private void OnTimerDrawProgressBar(Object obj, EventArgs e)
		//{
		//	try
		//	{
		//		//JitterBuffer
		//		ProgressBarJitterBuffer.Value = m_JitterBuffer.Length;
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(ex.Message);
		//	}
		//}

		/// <summary>
		/// StartTimerDrawCurve
		/// </summary>
		//private void StartTimerDrawCurve()
		//{
		//	if (m_TimerDrawCurve == null)
		//	{
		//		//Graphics
		//		m_TimerDrawCurve = new System.Windows.Forms.Timer();
		//		m_TimerDrawCurve.Tick += new EventHandler(OnTimerDrawGraphics);
		//		m_TimerDrawCurve.Interval = 100;
		//		m_TimerDrawCurve.Start();

		//		//Measurements
		//		m_TimerDrawMeasurements = new System.Windows.Forms.Timer();
		//		m_TimerDrawMeasurements.Tick += new EventHandler(OnTimerDrawMeasurements);
		//		m_TimerDrawMeasurements.Interval = 4000;
		//		m_TimerDrawMeasurements.Start();
		//	}
		//}

		/// <summary>
		///StopTimerDrawCurve 
		/// </summary>
		//private void StopTimerDrawCurve()
		//{
		//	if (m_TimerDrawCurve != null)
		//	{
		//		//Graphics
		//		m_TimerDrawCurve.Stop();
		//		m_TimerDrawCurve = null;

		//		//Measurements
		//		m_TimerDrawMeasurements.Stop();
		//		m_TimerDrawMeasurements = null;
		//	}
		//}

		/// <summary>
		/// StartTimerDrawCurve
		/// </summary>
		//private void StartTimerDrawProgressBar()
		//{
		//	if (m_TimerDrawProgressBar == null)
		//	{
		//		m_TimerDrawProgressBar = new System.Windows.Forms.Timer();
		//		m_TimerDrawProgressBar.Tick += new EventHandler(OnTimerDrawProgressBar);
		//		m_TimerDrawProgressBar.Interval = 100;
		//		m_TimerDrawProgressBar.Start();
		//	}
		//}

		/// <summary>
		///StopTimerDrawCurve 
		/// </summary>
		//private void StopTimerDrawProgressBar()
		//{
		//	if (m_TimerDrawProgressBar != null)
		//	{
		//		m_TimerDrawProgressBar.Stop();
		//		m_TimerDrawProgressBar = null;
		//		ProgressBarJitterBuffer.Value = 0;
		//	}
		//}

		/// <summary>
		/// InitGraphics
		/// </summary>
		//private void InitGraphics()
		//{
		//	//GraphicsPanelCurve
		//	GraphicsPanelCurve = PanelCurve.CreateGraphics();
		//	GraphicsPanelCurve.TranslateTransform(0, 0);
		//	PenCurve = new Pen(Color.Green, 2);
		//}

		/// <summary>
		/// InitComboboxes
		/// </summary>
		//private void InitComboboxes()
		//{
		//	ComboboxSoundDeviceName.Items.Clear();
		//	List<String> names = WinSound.WinSound.GetPlaybackNames();

		//	foreach (String name in names.Where(x => x != null))
		//	{
		//		ComboboxSoundDeviceName.Items.Add(name);
		//	}

		//	if (ComboboxSoundDeviceName.Items.Count > 0)
		//	{
		//		ComboboxSoundDeviceName.SelectedIndex = 0;
		//	}
		//	ComboboxSamplesPerSecond.SelectedIndex = 1;
		//	ComboboxBitsPerSample.SelectedIndex = 0;
		//	ComboboxChannels.SelectedIndex = 0;
		//	ComboboxBufferCount.SelectedIndex = 4;
		//}

		/// <summary>
		/// ResetTimeMeasurements
		/// </summary>
		//private void ResetTimeMeasurements()
		//{
		//	m_QueueTimeDiffs.Clear();
		//	m_MeasurementTimeOne = 0;
		//	m_MeasurementTimeTwo = 0;
		//}

		/// <summary>
		/// DrawTimeMeasurements
		/// </summary>
		//private void DrawTimeMeasurements()
		//{
		//	try
		//	{
		//		//Wenn aktiv
		//		if (m_Receiver.Connected)
		//		{
		//			//Wenn Messungen vorhanden
		//			int timeDiffsCount = m_QueueTimeDiffs.Count;
		//			if (m_QueueTimeDiffs.Count > 0)
		//			{
		//				//Testwerte
		//				double averageTimeDiff = 0;
		//				double minTimeDiff = 0;
		//				double maxTimeDiff = 0;
		//				List<double> listTimeDiffs = new List<double>();

		//				//Für jeden Zeitunterschied
		//				for (int i = 0; i < timeDiffsCount; i++)
		//				{
		//					//Differenz ermitteln
		//					double d = m_QueueTimeDiffs.Dequeue();
		//					//Delta ermitteln
		//					double delta = d - 20.0;

		//					//Speichern
		//					listTimeDiffs.Add(d);

		//					//Lokale Höchstwerte berechnen
		//					maxTimeDiff = delta > maxTimeDiff ? delta : maxTimeDiff;
		//					minTimeDiff = delta < minTimeDiff ? delta : minTimeDiff;
		//				}

		//				//Durchschnitte berechnen
		//				averageTimeDiff = listTimeDiffs.Average();
		//				//Differenz berechnen
		//				double averagedeltaDiff = averageTimeDiff - 20.0;
		//				String strAverageDeltaDiff = averagedeltaDiff < 0 ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "- {0:0.0000}", Math.Abs(averagedeltaDiff)) : String.Format("+ {0:0.0000}", Math.Abs(averagedeltaDiff));

		//				//Gesamttext erstellen
		//				String m_Message = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Average of last {0} RTP-Packets: {1:0.0000}ms  {2}ms    Min: {3:0.0000}  Max: {4:0.0000}", timeDiffsCount, averageTimeDiff, strAverageDeltaDiff, minTimeDiff, maxTimeDiff);

		//				//Text zeichnen
		//				RectangleF rec = GraphicsPanelCurve.ClipBounds;
		//				rec.Height = 4194320;
		//				GraphicsPanelCurve.FillRectangle(averageTimeDiff >= 20 ? Brushes.Maroon : Brushes.DarkGreen, rec);
		//				GraphicsPanelCurve.DrawString(m_Message, Font, Brushes.Yellow, new PointF(0, 0));
		//			}
		//		}
		//		else
		//		{
		//			//Messungen resetten
		//			ResetTimeMeasurements();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(String.Format("FormMain.cs | DrawMilliseconds() | {0}", ex.Message));
		//	}
		//}

		/// <summary>
		/// ShowState
		/// </summary>
		//private void ShowState()
		//{
		//	if (m_Player != null && m_Player.Opened)
		//	{
		//		ShowConnected();
		//	}
		//	else
		//	{
		//		ShowDisconnected();
		//	}
		//}

		/// <summary>
		/// ShowConnected
		/// </summary>
		//private void ShowConnected()
		//{
		//	ButtonStart.BackColor = Color.Green;
		//	ComboboxSamplesPerSecond.Enabled = false;
		//	ComboboxSoundDeviceName.Enabled = false;
		//	ComboboxBitsPerSample.Enabled = false;
		//	ComboboxChannels.Enabled = false;
		//	TextBoxMCAddress.Enabled = false;
		//	TextBoxMCPort.Enabled = false;
		//	NumericUpDownPacketSize.Enabled = false;
		//	ComboboxBufferCount.Enabled = false;
		//	NumericUpDownJitterBuffer.Enabled = false;
		//}

		/// <summary>
		///  ShowDisconnected
		/// </summary>
		//private void ShowDisconnected()
		//{
		//	ButtonStart.BackColor = Color.Gray;
		//	ComboboxSamplesPerSecond.Enabled = true;
		//	ComboboxSoundDeviceName.Enabled = true;
		//	ComboboxBitsPerSample.Enabled = true;
		//	ComboboxChannels.Enabled = true;
		//	TextBoxMCAddress.Enabled = true;
		//	TextBoxMCPort.Enabled = true;
		//	NumericUpDownPacketSize.Enabled = true;
		//	ComboboxBufferCount.Enabled = true;
		//	NumericUpDownJitterBuffer.Enabled = true;
		//}

		/// <summary>
		/// FormToConfig
		/// </summary>
		/// <returns></returns>
       

		/// <summary>
		/// ConfigToForm
		/// </summary>
		/// <returns></returns>
		//private bool ConfigToForm()
		//{
		//	try
		//	{
		//		ComboboxSoundDeviceName.SelectedIndex = ComboboxSoundDeviceName.FindString(Config.SoundDeviceName);
		//		TextBoxMCAddress.Text = Config.MulticasAddress;
		//		TextBoxMCPort.Text = Config.MulticastPort.ToString();
		//		ComboboxSamplesPerSecond.SelectedIndex = ComboboxSamplesPerSecond.FindString(Config.SamplesPerSecond.ToString());
		//		ComboboxBitsPerSample.SelectedIndex = ComboboxBitsPerSample.FindString(Config.BitsPerSample.ToString());
		//		ComboboxChannels.SelectedIndex = ComboboxChannels.FindString(Config.Channels.ToString());
		//		NumericUpDownPacketSize.Value = Config.PacketSize;
		//		ComboboxBufferCount.SelectedIndex = ComboboxBufferCount.FindString(Config.BufferCount.ToString());
		//		NumericUpDownJitterBuffer.Value = Config.JitterBuffer;
		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show(ex.Message);
		//		return false;
		//	}
		//}

		/// <summary>
		/// ButtonStart_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//private void ButtonStart_Click(object sender, EventArgs e)
		//{
		//	try
		//	{
		//		//Wenn Eingabe
		//		if (TextBoxMCAddress.Text.Length > 0 && TextBoxMCPort.Text.Length > 0)
		//		{
		//			//Wenn geöffnet
		//			if (m_Player.Opened)
		//			{
		//				//Wenn JitterBuffer
		//				if (UseJitterBuffer)
		//				{
		//					m_JitterBuffer.Stop();
		//					//Timer beenden
		//					StopTimerDrawProgressBar();
		//				}

		//				//MulticastReceiver
		//				m_Receiver.Disconnect();
		//				//WinSound Player
		//				m_Player.Close();
		//			}
		//			else
		//			{
		//				if (FormToConfig() == true)
		//				{
		//					//Zeitmessungen zurücksetzen
		//					ResetTimeMeasurements();

		//					//MulticastReceiver
		//					m_Receiver = new RTPReceiver(Config.PacketSize);
		//					m_Receiver.DataReceived2 += new RTPReceiver.DelegateDataReceived2(OnDataReceived);
		//					m_Receiver.Disconnected += new RTPReceiver.DelegateDisconnected(OnDisconnected);
		//					m_Receiver.Connect(Config.MulticasAddress, Config.MulticastPort);

		//					//WinSound Player öffnen
		//					m_Player.Open(Config.SoundDeviceName, Config.SamplesPerSecond, Config.BitsPerSample, Config.Channels, Config.BufferCount);

		//					//Wenn JitterBuffer
		//					if (UseJitterBuffer)
		//					{
		//						InitJitterBuffer();
		//						m_JitterBuffer.Start();
		//						StartTimerDrawProgressBar();
		//					}
		//				}
		//			}

		//			//Anzeigen
		//			ShowState();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
		//	}
		//}

		/// <summary>
		/// FormMain_FormClosing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
		//{
		//	try
		//	{
		//		if (m_Receiver != null)
		//		{
		//			m_Receiver.Disconnect();

		//			//Events entfernen
		//			m_Receiver.DataReceived2 -= new RTPReceiver.DelegateDataReceived2(OnDataReceived);
		//			m_Receiver.Disconnected -= new RTPReceiver.DelegateDisconnected(OnDisconnected);
		//		}
		//		if (m_Player != null)
		//		{
		//			m_Player.Close();
		//			m_JitterBuffer.Stop();
		//		}
		//		SaveConfig();
		//	}
		//	catch (Exception ex)
		//	{
  //              MessageBox.Show(ex.Message, "Fehler beim Beenden", MessageBoxButtons.OK, MessageBoxIcon.Error);
		//	}
		//}

		/// <summary>
		/// CheckBoxDrawCurve_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//private void CheckBoxDrawCurve_CheckedChanged(object sender, EventArgs e)
		//{
		//	if (CheckBoxDrawCurve.Checked)
		//	{
		//		Height += (PanelCurve.Height + 10);
		//		IsDrawCurve = true;
		//		StartTimerDrawCurve();
		//	}
		//	else
		//	{
		//		Height -= (PanelCurve.Height + 10);
		//		IsDrawCurve = false;
		//		StopTimerDrawCurve();
		//	}
		//	//Zeitmessungen zurücksetzen
		//	ResetTimeMeasurements();
		//}

		/// <summary>
		/// NumericUpDownJitterBuffer_ValueChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//private void NumericUpDownJitterBuffer_ValueChanged(object sender, EventArgs e)
		//{
		//	m_JitterBufferLength = (uint)NumericUpDownJitterBuffer.Value;
		//}
	}
}
