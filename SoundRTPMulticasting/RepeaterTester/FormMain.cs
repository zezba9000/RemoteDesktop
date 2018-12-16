using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinSoundTester
{
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
		WinSound.Repeater repeaterOne = new WinSound.Repeater();

		/// <summary>
		/// Init
		/// </summary>
		private void Init()
		{
			try
			{
				InitComboboxWaveIn();
				InitComboboxWaveOut();
				InitComoboxSamplesPerSecond();
				InitComboboxBitsPerSample();
				InitComboboxChannels();
				InitComboboxBufferCount();
				InitComboboxBufferSize();

				repeaterOne.RepeaterStopped += new WinSound.Repeater.DelegateStopped(OnRepeaterStopped);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// InitComboboxWaveIn
		/// </summary>
		private void InitComboboxWaveIn()
		{
			ComboboxWaveIn.DataSource = null;
			ComboboxWaveIn.DataSource = WinSound.WinSound.GetRecordingNames();
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
		/// InitComboboxBufferCount
		/// </summary>
		private void InitComboboxBufferCount()
		{
			ComboboxBufferCount.Items.Clear();
			ComboboxBufferCount.Items.Add("1");
			ComboboxBufferCount.Items.Add("2");
			ComboboxBufferCount.Items.Add("4");
			ComboboxBufferCount.Items.Add("6");
			ComboboxBufferCount.Items.Add("8");
			ComboboxBufferCount.Items.Add("12");
			ComboboxBufferCount.Items.Add("16");
			ComboboxBufferCount.Items.Add("24");
			ComboboxBufferCount.Items.Add("64");

			ComboboxBufferCount.SelectedIndex = 4;
		}
		/// <summary>
		/// InitComboboxBufferSize
		/// </summary>
		private void InitComboboxBufferSize()
		{
			ComboboxBufferSize.Items.Clear();
			ComboboxBufferSize.Items.Add("100");
			ComboboxBufferSize.Items.Add("250");
			ComboboxBufferSize.Items.Add("500");
			ComboboxBufferSize.Items.Add("1024");
            ComboboxBufferSize.Items.Add("2048");
			ComboboxBufferSize.Items.Add("4096");
			ComboboxBufferSize.Items.Add("8192");
			ComboboxBufferSize.Items.Add("16000");

			ComboboxBufferSize.SelectedIndex = 5;
		}
		/// <summary>
		/// OnRepeaterStopped
		/// </summary>
		private void OnRepeaterStopped()
		{
			try
			{
				this.Invoke(new MethodInvoker(delegate()
				{
					ComboboxWaveIn.Enabled = true;
					ComboboxWaveOut.Enabled = true;
					ComboboxSamplesPerSecond.Enabled = true;
					ComboboxBitsPerSample.Enabled = true;
					ComboboxChannels.Enabled = true;
					ComboboxBufferCount.Enabled = true;
					ComboboxBufferSize.Enabled = true;
					ButtonStartRepeater.BackColor = SystemColors.Control;
				}));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			}
		}
		/// <summary>
		/// ButtonStartRepeater_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonStartRepeater_Click(object sender, EventArgs e)
		{
			try
			{
				//for (int i = 0; i < 10; i++)
				{
					if (repeaterOne.Started)
					{
						repeaterOne.Stop();
					}
					else
					{
						//Daten ermitteln
						int samplesPerSecond = Convert.ToInt32(ComboboxSamplesPerSecond.SelectedItem);
						int bitsPerSample = Convert.ToInt32(ComboboxBitsPerSample.SelectedItem);
						int channels = Convert.ToInt32(ComboboxChannels.SelectedItem);
						int bufferCount = Convert.ToInt32(ComboboxBufferCount.SelectedItem);
						int bufferSize = Convert.ToInt32(ComboboxBufferSize.SelectedItem);

						repeaterOne.Start(ComboboxWaveIn.SelectedItem.ToString(), ComboboxWaveOut.SelectedItem.ToString(), samplesPerSecond, bitsPerSample, channels, bufferCount, bufferSize);
						ComboboxWaveIn.Enabled = false;
						ComboboxWaveOut.Enabled = false;
						ComboboxSamplesPerSecond.Enabled = false;
						ComboboxBitsPerSample.Enabled = false;
						ComboboxChannels.Enabled = false;
						ComboboxBufferCount.Enabled = false;
						ComboboxBufferSize.Enabled = false;
						ButtonStartRepeater.BackColor = Color.DarkGreen;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// FormMain_FormClosing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			repeaterOne.RepeaterStopped -= new WinSound.Repeater.DelegateStopped(OnRepeaterStopped);
			repeaterOne.Stop();
		}
		/// <summary>
		/// CheckBoxMute_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckBoxMute_CheckedChanged(object sender, EventArgs e)
		{
			repeaterOne.Mute = CheckBoxMute.Checked;

			if (CheckBoxMute.Checked)
			{
				CheckBoxMute.ForeColor = Color.Red;
			}
			else
			{
				CheckBoxMute.ForeColor = Color.DimGray;
			}
		}
	}
}
