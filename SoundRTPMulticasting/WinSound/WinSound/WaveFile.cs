using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WinSound
{
	/// <summary>
	/// WaveFile
	/// </summary>
	public class WaveFile
	{
		/// <summary>
		/// Konstruktor
		/// </summary>
		public WaveFile()
		{

		}

		//Attribute
		public const int WAVE_FORMAT_PCM = 1;

		/// <summary>
		/// WriteNew
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public static void Create(string fileName, uint samplesPerSecond, short bitsPerSample, short channels, Byte[] data)
		{
			//Bestehende Datei löschen
			if (System.IO.File.Exists(fileName))
			{
				System.IO.File.Delete(fileName);
			}

			//Header erstellen
			WaveFileHeader header = CreateNewWaveFileHeader(samplesPerSecond, bitsPerSample, channels, (uint)(data.Length), 44 + data.Length);
			//Header schreiben
			WriteHeader(fileName, header);
			//Daten schreiben
			WriteData(fileName, header.DATAPos, data);
		}
		/// <summary>
		/// AppendData
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public static void AppendData(string fileName, Byte[] data)
		{
			AppendData(fileName, data, false);
		}
		/// <summary>
		/// AppendData
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		/// <param name="forceWriting"></param>
		public static void AppendData(string fileName, Byte[] data, bool forceWriting)
		{
			//Header auslesen
			WaveFileHeader header = ReadHeader(fileName);

			//Wenn Daten vorhanden
			if (header.DATASize > 0 || forceWriting)
			{
				//Daten anfügen
				WriteData(fileName, (int)(header.DATAPos + header.DATASize), data);

				//Header aktualisieren
				header.DATASize += (uint)data.Length;
				header.RiffSize += (uint)data.Length;

				//Header überschreiben
				WriteHeader(fileName, header);
			}
		}
		/// <summary>
		/// Read
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static WaveFileHeader Read(string fileName)
		{
			//Header lesen
			WaveFileHeader header = ReadHeader(fileName);

			//Fertig
			return header;
		}
		/// <summary>
		/// CreateWaveFileHeader
		/// </summary>
		/// <param name="SamplesPerSecond"></param>
		/// <param name="BitsPerSample"></param>
		/// <param name="Channels"></param>
		/// <param name="dataSize"></param>
		/// <returns></returns>
		private static WaveFileHeader CreateNewWaveFileHeader(uint SamplesPerSecond, short BitsPerSample, short Channels, uint dataSize, long fileSize)
		{
			//Header erstellen
			WaveFileHeader Header = new WaveFileHeader();

			//Werte setzen
			Array.Copy("RIFF".ToArray<Char>(), Header.RIFF, 4);
			Header.RiffSize = (uint)(fileSize - 8);
			Array.Copy("WAVE".ToArray<Char>(), Header.RiffFormat, 4);
			Array.Copy("fmt ".ToArray<Char>(), Header.FMT, 4);
			Header.FMTSize = 16;
			Header.AudioFormat = WAVE_FORMAT_PCM;
			Header.Channels = (short)Channels;
			Header.SamplesPerSecond = (uint)SamplesPerSecond;
			Header.BitsPerSample = (short)BitsPerSample;
			Header.BlockAlign = (short)((BitsPerSample * Channels) >> 3);
			Header.BytesPerSecond = (uint)(Header.BlockAlign * Header.SamplesPerSecond);
			Array.Copy("data".ToArray<Char>(), Header.DATA, 4);
			Header.DATASize = dataSize;

			//Fertig
			return Header;
		}
		/// <summary>
		/// ReadHeader
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private static WaveFileHeader ReadHeader(string fileName)
		{
			//Ergebnis
			WaveFileHeader header = new WaveFileHeader();

			//Wenn die Datei existiert
			if (File.Exists(fileName))
			{
				//Datei öffnen
				FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				System.IO.BinaryReader rd = new System.IO.BinaryReader(fs, Encoding.UTF8);

				//Lesen
				if (fs.CanRead)
				{
					//Chunk 1
					header.RIFF = rd.ReadChars(4);
					header.RiffSize = (uint)rd.ReadInt32();
					header.RiffFormat = rd.ReadChars(4);

					//Chunk 2
					header.FMT = rd.ReadChars(4);
					header.FMTSize = (uint)rd.ReadInt32();
					header.FMTPos = fs.Position;
					header.AudioFormat = (short)rd.ReadInt16();
					header.Channels = (short)rd.ReadInt16();
					header.SamplesPerSecond = (uint)rd.ReadInt32();
					header.BytesPerSecond = (uint)rd.ReadInt32();
					header.BlockAlign = (short)rd.ReadInt16();
					header.BitsPerSample = (short)rd.ReadInt16();

					//Zu Beginn von Chunk3 gehen
					fs.Seek(header.FMTPos + header.FMTSize, SeekOrigin.Begin);

					//Chunk 3
					header.DATA = rd.ReadChars(4);
					header.DATASize = (uint)rd.ReadInt32();
					header.DATAPos = (int)fs.Position;

					//Wenn nicht DATA
					if (new String(header.DATA).ToUpper() != "DATA")
					{
						uint DataChunkSize = header.DATASize + 8;
						fs.Seek(DataChunkSize, SeekOrigin.Current);
						header.DATASize = (uint)(fs.Length - header.DATAPos - DataChunkSize);
					}

					//Payload einlesen
					if (header.DATASize <= fs.Length - header.DATAPos)
					{
						header.Payload = rd.ReadBytes((int)header.DATASize);
					}
				}

				//Schliessen
				rd.Close();
				fs.Close();
			}

			//Fertig
			return header;
		}
		/// <summary>
		/// WriteHeader
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="header"></param>
		/// <param name="dataSize"></param>
		public static void WriteHeader(string fileName, WaveFileHeader header)
		{
			//Datei öffnen
			FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			System.IO.BinaryWriter wr = new System.IO.BinaryWriter(fs, Encoding.UTF8);

			//Chunk 1
			wr.Write(header.RIFF);
			wr.Write(Int32ToBytes((int)header.RiffSize));
			wr.Write(header.RiffFormat);

			//Chunk 2
			wr.Write(header.FMT);
			wr.Write(Int32ToBytes((int)header.FMTSize));
			wr.Write(Int16ToBytes(header.AudioFormat));
			wr.Write(Int16ToBytes(header.Channels));
			wr.Write(Int32ToBytes((int)header.SamplesPerSecond));
			wr.Write(Int32ToBytes((int)header.BytesPerSecond));
			wr.Write(Int16ToBytes((short)header.BlockAlign));
			wr.Write(Int16ToBytes((short)header.BitsPerSample));

			//Chunk 3
			wr.Write(header.DATA);
			wr.Write(Int32ToBytes((int)header.DATASize));

			//Datei schliessen
			wr.Close();
			fs.Close();
		}
		/// <summary>
		/// WriteData
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="pos"></param>
		public static void WriteData(string fileName, int pos, Byte[] data)
		{
			//Datei öffnen
			FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			System.IO.BinaryWriter wr = new System.IO.BinaryWriter(fs, Encoding.UTF8);

			//An Schreibposition gehen
			wr.Seek(pos, System.IO.SeekOrigin.Begin);
			//Daten schreiben
			wr.Write(data);
			//Fertig
			wr.Close();
			fs.Close();
		}
		//--------------------------------------------------------------------------------------------
		// BytesToInt32
		//--------------------------------------------------------------------------------------------
		private static int BytesToInt32(ref Byte[] bytes)
		{
			int Int32 = 0;
			Int32 = (Int32 << 8) + bytes[3];
			Int32 = (Int32 << 8) + bytes[2];
			Int32 = (Int32 << 8) + bytes[1];
			Int32 = (Int32 << 8) + bytes[0];
			return Int32;
		}
		//--------------------------------------------------------------------------------------------
		// BytesToInt16
		//--------------------------------------------------------------------------------------------
		private static short BytesToInt16(ref Byte[] bytes)
		{
			short Int16 = 0;
			Int16 = (short)((Int16 << 8) + bytes[1]);
			Int16 = (short)((Int16 << 8) + bytes[0]);
			return Int16;
		}
		//--------------------------------------------------------------------------------------------
		// Int32ToByte
		//--------------------------------------------------------------------------------------------
		private static Byte[] Int32ToBytes(int value)
		{
			Byte[] bytes = new Byte[4];
			bytes[0] = (Byte)(value & 0xFF);
			bytes[1] = (Byte)(value >> 8 & 0xFF);
			bytes[2] = (Byte)(value >> 16 & 0xFF);
			bytes[3] = (Byte)(value >> 24 & 0xFF);
			return bytes;
		}
		//--------------------------------------------------------------------------------------------
		// Int16ToBytes
		//--------------------------------------------------------------------------------------------
		private static Byte[] Int16ToBytes(short value)
		{
			Byte[] bytes = new Byte[2];
			bytes[0] = (Byte)(value & 0xFF);
			bytes[1] = (Byte)(value >> 8 & 0xFF);
			return bytes;
		}
	}
	/// <summary>
	/// WaveFileHeader
	/// </summary>
	public class WaveFileHeader
	{
		/// <summary>
		/// Konstruktor
		/// </summary>
		public WaveFileHeader()
		{

		}

		//Chunk 1
		public Char[] RIFF = new Char[4];
		public uint RiffSize = 8;
		public Char[] RiffFormat = new Char[4];

		//Chunk 2
		public Char[] FMT = new Char[4];
		public uint FMTSize = 16;
		public short AudioFormat;
		public short Channels;
		public uint SamplesPerSecond;
		public uint BytesPerSecond;
		public short BlockAlign;
		public short BitsPerSample;

		//Chunk 3
		public Char[] DATA = new Char[4];
		public uint DATASize;

		//Data
		public Byte[] Payload = new Byte[0];

		//HeaderLength
		public int DATAPos = 44;
		//Position FormatSize
		public long FMTPos = 20;

		/// <summary>
		///Duration 
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public TimeSpan Duration
		{
			get
			{
				int blockAlign = ((BitsPerSample * Channels) >> 3);
				int bytesPerSec = (int)(blockAlign * SamplesPerSecond);
				double value = (double)Payload.Length / (double)bytesPerSec;

				//Fertig
				return new TimeSpan(0, 0, (int)value);
			}
		}
	}
}
