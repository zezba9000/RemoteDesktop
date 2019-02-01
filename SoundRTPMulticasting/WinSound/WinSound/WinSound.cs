using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WinSound
{
	/// <summary>
	/// LockerClass
	/// </summary>
	class LockerClass
	{

	}
	/// <summary>
	/// WinSound
	/// </summary>
	public class WinSound
	{
		/// <summary>
		/// Konstruktor
		/// </summary>
		public WinSound()
		{

		}

		/// <summary>
		/// Alle Abspielgeräte anzeigen
		/// </summary>
		/// <returns></returns>
		public static List<String> GetPlaybackNames()
		{
			//Ergebnis
			List<String> list = new List<string>();
			Win32.WAVEOUTCAPS waveOutCap = new Win32.WAVEOUTCAPS();

			//Anzahl Devices
			uint num = Win32.waveOutGetNumDevs();
			for (int i = 0; i < num; i++)
			{
				uint hr = Win32.waveOutGetDevCaps(i, ref waveOutCap, Marshal.SizeOf(typeof(Win32.WAVEOUTCAPS)));
				if (hr == (int)Win32.HRESULT.S_OK)
				{
					list.Add(waveOutCap.szPname);
				}
			}

			//Fertig
			return list;
		}
		/// <summary>
		/// Alle Aufnahmegeräte anzeigen
		/// </summary>
		/// <returns></returns>
		public static List<String> GetRecordingNames()
		{
			//Ergebnis
			List<String> list = new List<string>();
			Win32.WAVEINCAPS waveInCap = new Win32.WAVEINCAPS();

			//Anzahl Devices
			uint num = Win32.waveInGetNumDevs();
			for (int i = 0; i < num; i++)
			{
				uint hr = Win32.waveInGetDevCaps(i, ref waveInCap, Marshal.SizeOf(typeof(Win32.WAVEINCAPS)));
				if (hr == (int)Win32.HRESULT.S_OK)
				{
					list.Add(waveInCap.szPname);
				}
			}

			//Fertig
			return list;
		}
		/// <summary>
		/// GetWaveInDeviceIdByName
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static int GetWaveInDeviceIdByName(string name)
		{
			//Anzahl Devices
			uint num = Win32.waveInGetNumDevs();

			//WaveIn Struktur
			Win32.WAVEINCAPS caps = new Win32.WAVEINCAPS();
			for (int i = 0; i < num; i++)
			{
				Win32.HRESULT hr = (Win32.HRESULT)Win32.waveInGetDevCaps(i, ref caps, Marshal.SizeOf(typeof(Win32.WAVEINCAPS)));
				if (hr == Win32.HRESULT.S_OK)
				{
					if (caps.szPname == name)
					{
						//Gefunden
						return i;
					}
				}
			}

			//Nicht gefunden
			return Win32.WAVE_MAPPER;
		}
		/// <summary>
		/// GetWaveOutDeviceIdByName
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static int GetWaveOutDeviceIdByName(string name)
		{
			//Anzahl Devices
			uint num = Win32.waveOutGetNumDevs();

			//WaveIn Struktur
			Win32.WAVEOUTCAPS caps = new Win32.WAVEOUTCAPS();
			for (int i = 0; i < num; i++)
			{
				Win32.HRESULT hr = (Win32.HRESULT)Win32.waveOutGetDevCaps(i, ref caps, Marshal.SizeOf(typeof(Win32.WAVEOUTCAPS)));
				if (hr == Win32.HRESULT.S_OK)
				{
					if (caps.szPname == name)
					{
						//Gefunden
						return i;
					}
				}
			}

			//Nicht gefunden
			return Win32.WAVE_MAPPER;
		}
		/// <summary>
		/// FlagToString
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public static String FlagToString(Win32.WaveHdrFlags flag)
		{
			StringBuilder sb = new StringBuilder();

			if ((flag & Win32.WaveHdrFlags.WHDR_PREPARED) > 0) sb.Append("PREPARED ");
			if ((flag & Win32.WaveHdrFlags.WHDR_BEGINLOOP) > 0) sb.Append("BEGINLOOP ");
			if ((flag & Win32.WaveHdrFlags.WHDR_ENDLOOP) > 0) sb.Append("ENDLOOP ");
			if ((flag & Win32.WaveHdrFlags.WHDR_INQUEUE) > 0) sb.Append("INQUEUE ");
			if ((flag & Win32.WaveHdrFlags.WHDR_DONE) > 0) sb.Append("DONE ");

			return sb.ToString();
		}
	}
}
