using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlayerTester
{
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
		WinSound.Player playerOne = new WinSound.Player();
        WinSound.Recorder recorderOne = new WinSound.Recorder();
        List<Byte> RecordedBytes = new List<Byte>();
        String FileName = "";
        int SamplesPerSecond = 8000;
        int BitsPerSample = 16;
        int Channels = 2;
        bool Append = false;


		/// <summary>
		/// Init
		/// </summary>
		private void Init()
		{
			try
			{
				InitComboboxWaveOut();
                InitComboboxWaveIn();
				InitComoboxSamplesPerSecond();
				InitComboboxBitsPerSample();
				InitComboboxChannels();

				//Events
				playerOne.PlayerClosed += new WinSound.Player.DelegateStopped(OnPlayerClosed);
				playerOne.PlayerStopped += new WinSound.Player.DelegateStopped(OnPlayerStopped);
                recorderOne.DataRecorded += new WinSound.Recorder.DelegateDataRecorded(OnDataRecorded);
                recorderOne.RecordingStopped += new WinSound.Recorder.DelegateStopped(OnRecordingStopped);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		/// <summary>
		///InitComboboxWaveOut
		/// </summary>
		private void InitComboboxWaveOut()
		{
			ComboboxWaveOut.DataSource = null;
			ComboboxWaveOut.DataSource = WinSound.WinSound.GetPlaybackNames();
		}
        /// <summary>
        ///InitComboboxWaveIn
        /// </summary>
        private void InitComboboxWaveIn()
        {
            ComboboxWaveIn.DataSource = null;
            ComboboxWaveIn.DataSource = WinSound.WinSound.GetRecordingNames();
        }
		/// <summary>
		/// InitComoboxSamplesPerSecond
		/// </summary>
		private void InitComoboxSamplesPerSecond()
		{
			ComboboxSamplesPerSecond.Items.Clear();
			ComboboxSamplesPerSecond.Items.Add("5000");
			ComboboxSamplesPerSecond.Items.Add("8000");
			ComboboxSamplesPerSecond.Items.Add("11025");
			ComboboxSamplesPerSecond.Items.Add("22050");
			ComboboxSamplesPerSecond.Items.Add("44100");
			ComboboxSamplesPerSecond.Items.Add("96000");

			ComboboxSamplesPerSecond.SelectedIndex = 4;
		}
		/// <summary>
		/// InitComoboxSamplesPerSecond
		/// </summary>
		private void InitComboboxBitsPerSample()
		{
			ComboboxBitsPerSample.Items.Clear();
			ComboboxBitsPerSample.Items.Add("8");
			ComboboxBitsPerSample.Items.Add("16");

			ComboboxBitsPerSample.SelectedIndex = 1;
		}

		/// <summary>
		/// InitComboboxChannels
		/// </summary>
		private void InitComboboxChannels()
		{
			ComboboxChannels.Items.Clear();
			ComboboxChannels.Items.Add("1");
			ComboboxChannels.Items.Add("2");

			ComboboxChannels.SelectedIndex = 1;
		}
        /// <summary>
        /// StartRecord
        /// </summary>
        private void StartRecord()
        {
            try
            {
                if (TextBoxFileName.Text.Length > 0)
                {
                    //Wenn nicht schon gestartet
                    if (recorderOne.Started == false)
                    {
                        //Werte ermitteln
                        SamplesPerSecond = Convert.ToInt32(ComboboxSamplesPerSecond.Text);
                        BitsPerSample = Convert.ToInt32(ComboboxBitsPerSample.Text);
                        Channels = Convert.ToInt32(ComboboxChannels.Text);
                        string waveIn = ComboboxWaveIn.SelectedItem.ToString();
                        FileName = TextBoxFileName.Text;
                        Append = CheckBoxAppend.Checked;

                        //Optimale Buffergrösse bestimmen
                        int bufferSize = GetBufferSizeBySamplesPerSecond(SamplesPerSecond);
                        
                        //Aufnehmen
                        recorderOne.Start(waveIn, SamplesPerSecond, BitsPerSample, Channels, 8, bufferSize);
                        ShowRecording(true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("FormMain.cs | StartRecord() | {0}", ex.Message));
            }
        }
        /// <summary>
        /// GetBufferSizeBySamplesPerSecond
        /// </summary>
        /// <param name="samplesPerSecond"></param>
        /// <returns></returns>
        private int GetBufferSizeBySamplesPerSecond(int samplesPerSecond)
        {
            //Ergebnis
            int bufferSize = 2048;

            if (samplesPerSecond >= 5000)
            {
                bufferSize =  512;
            }
            if (samplesPerSecond >= 8000)
            {
                bufferSize = 1024;
            }
            if (samplesPerSecond >= 11025)
            {
                bufferSize = 1024;
            }
            if (samplesPerSecond >= 22050)
            {
                bufferSize = 2048;
            }
            if (samplesPerSecond >= 44100)
            {
                bufferSize = 4096;
            }
            if (samplesPerSecond >= 96000)
            {
                bufferSize = 8192;
            }

            //Fertig
            return bufferSize;
        }
        /// <summary>
        /// StopRecord
        /// </summary>
        private void StopRecord()
        {
            try
            {
                if (recorderOne.Started)
                {
                    recorderOne.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("FormMain.cs | StopRecord() | {0}", ex.Message));
            }
        }
        /// <summary>
        /// ShowRecording
        /// </summary>
        /// <param name="recording"></param>
        private void ShowRecording(bool recording)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                //Direkt
                ComboboxWaveOut.Enabled = !recording;
                ComboboxWaveIn.Enabled = !recording;
                ComboboxSamplesPerSecond.Enabled = !recording;
                ComboboxBitsPerSample.Enabled = !recording;
                ComboboxChannels.Enabled = !recording;
                TextBoxFileName.Enabled = !recording;
                ButtonOpenFileDialog.Enabled = !recording;
                ButtonPlay.Enabled = !recording;
                ButtonPause.Enabled = !recording;
                CheckBoxAppend.Enabled = !recording;

                if (recording)
                {
                    ButtonRecord.BackColor = Color.DarkRed;
                }
                else
                {
                    ButtonRecord.BackColor = SystemColors.Control;
                }
            }));
        }
		/// <summary>
		/// OnRepeateClosed
		/// </summary>
		private void OnPlayerClosed()
		{
			try
			{
				this.Invoke(new MethodInvoker(delegate()
				{
					ComboboxWaveOut.Enabled = true;
                    ComboboxWaveIn.Enabled = true;
					ComboboxSamplesPerSecond.Enabled = true;
					ComboboxBitsPerSample.Enabled = true;
					ComboboxChannels.Enabled = true;
					ButtonPause.Enabled = false;
					ButtonOpenFileDialog.Enabled = true;
					TextBoxFileName.Enabled = true;
					ButtonPlay.BackColor = SystemColors.Control;
                    ButtonPlay.Text = "Play";
                    CheckBoxAppend.Enabled = true;
                    ButtonRecord.Enabled = true;
				}));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			}
		}
		/// <summary>
		/// OnPlayerStopped
		/// </summary>
		private void OnPlayerStopped()
		{
            try
            {
                //Player schliessen
                playerOne.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
		}
        /// <summary>
        /// On_DataRecorded
        /// </summary>
        /// <param name="data"></param>
        private void OnDataRecorded(Byte[] data)
        {
            try
            {
               RecordedBytes.AddRange(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("FormMain.cs | On_DataRecorded() | {0}", ex.Message));
            }
        }
        /// <summary>
        /// On_RecordingStopped
        /// </summary>
        private void OnRecordingStopped()
        {
            try
            {
                //Bei gültigem Dateiname
                if (FileName.Length > 0)
                {
                    //Aufgenommene Daten speichern
                    if (RecordedBytes.Count > 0)
                    {
                        if (Append)
                        {
                            //Daten anfügen
                            WinSound.WaveFile.AppendData(FileName, RecordedBytes.ToArray());
                        }
                        else
                        {
                            //WaveFile neu schreiben
                            WinSound.WaveFile.Create(FileName, (uint)SamplesPerSecond, (short)BitsPerSample, (short)Channels, RecordedBytes.ToArray());
                        }

                        //Daten für nächste Aufnahme leeren
                        RecordedBytes.Clear();
                    }
                }

                //Anzeigen
                ShowRecording(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("FormMain.cs | On_RecordingStopped() | {0}", ex.Message));
                RecordedBytes.Clear();
            }
        }
		/// <summary>
		/// ShowPlaying
		/// </summary>
		private void ShowPlaying()
		{
            ComboboxWaveOut.Enabled = false;
            ComboboxWaveIn.Enabled = false;
            ComboboxSamplesPerSecond.Enabled = false;
            ComboboxBitsPerSample.Enabled = false;
            ComboboxChannels.Enabled = false;
            ButtonOpenFileDialog.Enabled = true;
            TextBoxFileName.Enabled = true;
            ButtonPlay.BackColor = Color.DarkGreen;
			ButtonPause.Enabled = true;
			TextBoxFileName.Enabled = false;
			ButtonOpenFileDialog.Enabled = false;
            ButtonPlay.Text = "Stop";
            CheckBoxAppend.Enabled = false;
            ButtonRecord.Enabled = false;
			Application.DoEvents();
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
		/// ButtonOpen_Click 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonOpen_Click(object sender, EventArgs e)
		{
            try
            {
                if (playerOne.Opened == false)
                {
                    //Wenn eine gültige Datei
                    String fileName = TextBoxFileName.Text;
                    if (System.IO.File.Exists(fileName))
                    {
                        //Abspielen
                        if (playerOne.PlayFile(fileName, ComboboxWaveOut.Text))
                        {
                            ShowPlaying();
                            return;
                        }
                        else
                        {
                            //Datei nicht vorhanden oder fehlerhaft
                            ButtonPlay.BackColor = Color.Red;
                        }
                    }
                }
                else
                {
                    playerOne.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
           
		}
		/// <summary>
		/// ButtonPause_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonPause_Click(object sender, EventArgs e)
		{
			try
			{
				if (playerOne.Paused)
				{
					bool ok = playerOne.EndPause();
				}
				else
				{
					bool ok = playerOne.StartPause();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
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
				playerOne.PlayerClosed -= new WinSound.Player.DelegateStopped(OnPlayerClosed);
				playerOne.PlayerStopped -= new WinSound.Player.DelegateStopped(OnPlayerStopped);
                recorderOne.DataRecorded -= new WinSound.Recorder.DelegateDataRecorded(OnDataRecorded);
                recorderOne.RecordingStopped -= new WinSound.Recorder.DelegateStopped(OnRecordingStopped);
				bool hrPlayer = playerOne.Close();
                bool hrRecorder = recorderOne.Stop();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			}
		}
        /// <summary>
        /// ButtonRecord_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRecord_Click(object sender, EventArgs e)
        {

            if (recorderOne.Started)
            {
                StopRecord();
            }
            else
            {
                StartRecord();
            }
        }
	}
}
