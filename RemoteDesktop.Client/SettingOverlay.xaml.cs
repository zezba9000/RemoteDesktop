using RemoteDesktop.Core;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace RemoteDesktop.Client
{
	namespace XML
	{
		public class CustomSocketAddress
		{
			[XmlAttribute("Enabled")] public bool enabled = false;
			[XmlText] public string address;
		}

		[XmlRoot]
		public class Settings
		{
			[XmlElement("ImageBit")] public int imageBit = 16;
			[XmlElement("ImageScale")] public float imageScale = .75f;
			[XmlElement("CompressImageFrames")] public bool compressImageFrames = true;
			[XmlElement("CustomSocketAddress")] public CustomSocketAddress customSocketAddress = new CustomSocketAddress();
			[XmlElement("TargetFPS")] public int targetFPS = 10;
		}
	}

	/// <summary>
	/// Interaction logic for SettingOverlay.xaml
	/// </summary>
	public partial class SettingOverlay : UserControl
	{
		public delegate void ApplyCallbackMethod();
		public event ApplyCallbackMethod ApplyCallback;

		private string filePath;
		public XML.Settings settings;

		public SettingOverlay()
		{
			InitializeComponent();
			
			filePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Settings.xml");
			if (File.Exists(filePath))
			{
				try
				{
					using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
					{
						var xml = new XmlSerializer(typeof(XML.Settings));
						settings = (XML.Settings)xml.Deserialize(stream);
					}
				}
				catch (Exception e)
				{
					DebugLog.LogError("Failed to load settings file: " + e.Message);
					settings = new XML.Settings();
				}
			}
			else
			{
				settings = new XML.Settings();
			}
		}

		public void SaveSettings()
		{
			try
			{
				using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					var xml = new XmlSerializer(typeof(XML.Settings));
					xml.Serialize(stream, settings);
				}
			}
			catch (Exception e)
			{
				DebugLog.LogError("Failed to save settings file: " + e.Message);
			}
		}

		public void Show()
		{
			imageBitComboBox.SelectedIndex = settings.imageBit == 24 ? 1 : 0;

			switch (settings.imageScale)
			{
				case 1.0f: imageScaleComboBox.SelectedIndex = 0; break;
				case .75f: imageScaleComboBox.SelectedIndex = 1; break;
				case .5f: imageScaleComboBox.SelectedIndex = 2; break;
				case .25f: imageScaleComboBox.SelectedIndex = 3; break;
			}

			switch (settings.targetFPS)
			{
				case 5: targetFPSComboBox.SelectedIndex = 0; break;
				case 10: targetFPSComboBox.SelectedIndex = 1; break;
				case 15: targetFPSComboBox.SelectedIndex = 2; break;
				case 30: targetFPSComboBox.SelectedIndex = 3; break;
				case 60: targetFPSComboBox.SelectedIndex = 4; break;
			}

			compressCheckBox.IsChecked = settings.compressImageFrames;
			customAddressCheckBox.IsChecked = settings.customSocketAddress.enabled;

			Visibility = Visibility.Visible;
		}

		private void applyButton_Click(object sender, RoutedEventArgs e)
		{
			settings.imageBit = imageBitComboBox.SelectedIndex == 0 ? 16 : 24;

			switch (imageScaleComboBox.SelectedIndex)
			{
				case 0: settings.imageScale = 1.0f; break;
				case 1: settings.imageScale = .75f; break;
				case 2: settings.imageScale = .5f; break;
				case 3: settings.imageScale = .25f; break;
			}

			switch (targetFPSComboBox.SelectedIndex)
			{
				case 0: settings.targetFPS = 5; break;
				case 1: settings.targetFPS = 10; break;
				case 2: settings.targetFPS = 15; break;
				case 3: settings.targetFPS = 30; break;
				case 4: settings.targetFPS = 60; break;
			}

			settings.compressImageFrames = compressCheckBox.IsChecked == true;
			settings.customSocketAddress.enabled = customAddressCheckBox.IsChecked == true;	

			Visibility = Visibility.Hidden;
			if (ApplyCallback != null) ApplyCallback();
		}
	}
}
