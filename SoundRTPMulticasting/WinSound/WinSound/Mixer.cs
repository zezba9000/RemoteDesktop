using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinSound
{
	/// <summary>
	/// Mixer
	/// </summary>
	public class Mixer
	{
		/// <summary>
		/// MixBytes
		/// </summary>
		/// <param name="?"></param>
		/// <param name="maxListCount"></param>
		/// <param name="BitsPerSample"></param>
		/// <returns></returns>
		public static List<Byte> MixBytes(List<List<Byte>> listList, int BitsPerSample)
		{
			//Ergebnis
			List<Int32> list16 = new List<Int32>();
			List<Int32> list16Abs = new List<Int32>();
			int maximum = 0;

			//Fertig
			return MixBytes_Intern(listList, BitsPerSample, out list16, out list16Abs, out maximum);
		}
		/// <summary>
		/// MixBytes 
		/// </summary>
		/// <param name="listList"></param>
		/// <param name="BitsPerSample"></param>
		/// <param name="listLinear"></param>
		/// <returns></returns>
		public static List<Byte> MixBytes(List<List<Byte>> listList, int BitsPerSample, out List<Int32> listLinear, out List<Int32> listLinearAbs, out int maximum)
		{
			//Fertig
			return MixBytes_Intern(listList, BitsPerSample, out listLinear, out listLinearAbs, out maximum);
		}
		/// <summary>
		/// MixBytes_Intern
		/// </summary>
		/// <param name="listList"></param>
		/// <param name="BitsPerSample"></param>
		/// <param name="listLinear"></param>
		/// <returns></returns>
		private static List<Byte> MixBytes_Intern(List<List<Byte>> listList, int BitsPerSample, out List<Int32> listLinear, out List<Int32> listLinearAbs, out int maximum)
		{

			//Defaultwert setzen
			listLinear = new List<Int32>();
			listLinearAbs = new List<Int32>();
			maximum = 0;

			//Maximale Anzahl Bytes zum Mischen ermitteln
			int maxBytesCount = 0;
			foreach (List<Byte> l in listList)
			{
				if (l.Count > maxBytesCount)
				{
					maxBytesCount = l.Count;
				}
			}

			//Wenn Daten vorhanden
			if (listList.Count > 0 && maxBytesCount > 0)
			{

				//Je nach BitsPerSample
				switch (BitsPerSample)
				{
					//8
					case 8:
						return MixBytes_8Bit(listList, maxBytesCount, out listLinear, out listLinearAbs, out maximum);

					//16
					case 16:
						return MixBytes_16Bit(listList, maxBytesCount, out listLinear, out listLinearAbs, out maximum);
				}
			}

			//Fehler
			return new List<Byte>();
		}
		/// <summary>
		/// MixBytes_16Bit
		/// </summary>
		/// <param name="listList"></param>
		/// <returns></returns>
		private static List<Byte> MixBytes_16Bit(List<List<Byte>> listList, int maxBytesCount, out List<Int32> listLinear, out List<Int32> listLinearAbs, out int maximum)
		{
			//Ergebnis
			maximum = 0;

			//Array mit linearen und Byte Werten erstellen 
			int linearCount = maxBytesCount / 2;
			Int32[] bytesLinear = new Int32[linearCount];
			Int32[] bytesLinearAbs = new Int32[linearCount];
			Byte[] bytesRaw = new Byte[maxBytesCount];

			//Für jede ByteListe
			for (int v = 0; v < listList.Count; v++)
			{
				//In Array umwandeln
				Byte[] bytes = listList[v].ToArray();

				//Für jeden 16Bit Wert
				for (int i = 0, a = 0; i < linearCount; i++, a += 2)
				{
					//Wenn Werte zum Mischen vorhanden
					if (i < bytes.Length && a < bytes.Length - 1)
					{
						//Wert ermitteln
						Int16 value16 = BitConverter.ToInt16(bytes, a);
						int value32 = bytesLinear[i] + value16;

						//Wert addieren	(Überläufe abfangen)
						if (value32 < Int16.MinValue)
						{
							value32 = Int16.MinValue;
						}
						else if (value32 > Int16.MaxValue)
						{
							value32 = Int16.MaxValue;
						}

						//Werte setzen
						bytesLinear[i] = value32;
						bytesLinearAbs[i] = Math.Abs(value32);
						Int16 mixed16 = Convert.ToInt16(value32);
						Array.Copy(BitConverter.GetBytes(mixed16), 0, bytesRaw, a, 2);

						//Maximum berechnen
						if (value32 > maximum)
						{
							maximum = value32;
						}
					}
					else
					{
						//Stumm lassen
					}
				}
			}

			//Out Ergebnis
			listLinear = new List<int>(bytesLinear);
			listLinearAbs = new List<int>(bytesLinearAbs);

			//Fertig
			return new List<Byte>(bytesRaw);
		}
		/// <summary>
		/// MixBytes_8Bit
		/// </summary>
		/// <param name="listList"></param>
		/// <param name="maxBytesCount"></param>
		/// <param name="listLinear"></param>
		/// <param name="listLinearAbs"></param>
		/// <param name="maximum"></param>
		/// <returns></returns>
		private static List<Byte> MixBytes_8Bit(List<List<Byte>> listList, int maxBytesCount, out List<Int32> listLinear, out List<Int32> listLinearAbs, out int maximum)
		{
			//Ergebnis
			maximum = 0;

			//Array mit linearen und Byte Werten erstellen 
			int linearCount = maxBytesCount;
			Int32[] bytesLinear = new Int32[linearCount];
			Byte[] bytesRaw = new Byte[maxBytesCount];

			//Für jede ByteListe
			for (int v = 0; v < listList.Count; v++)
			{
				//In Array umwandeln
				Byte[] bytes = listList[v].ToArray();

				//Für jeden 8 Bit Wert
				for (int i = 0; i < linearCount; i++)
				{
					//Wenn Werte zum Mischen vorhanden
					if (i < bytes.Length)
					{
						//Wert ermitteln
						Byte value8 = bytes[i];
						int value32 = bytesLinear[i] + value8;

						//Wert addieren	(Überläufe abfangen)
						if (value32 < Byte.MinValue)
						{
							value32 = Byte.MinValue;
						}
						else if (value32 > Byte.MaxValue)
						{
							value32 = Byte.MaxValue;
						}

						//Werte setzen
						bytesLinear[i] = value32;
						bytesRaw[i] = BitConverter.GetBytes(value32)[0];

						//Maximum berechnen
						if (value32 > maximum)
						{
							maximum = value32;
						}
					}
					else
					{
						//Stumm lassen
					}
				}
			}

			//Out Ergebnisse
			listLinear = new List<int>(bytesLinear);
			listLinearAbs = new List<int>(bytesLinear);

			//Fertig
			return new List<Byte>(bytesRaw);
		}
		/// <summary>
		/// SubsctractBytes_16Bit
		/// </summary>
		/// <param name="listList"></param>
		/// <param name="maxBytesCount"></param>
		/// <returns></returns>
		public static List<Byte> SubsctractBytes_16Bit(List<Byte> listSource, List<Byte> listToSubstract)
		{
			//Ergebnis
			List<Byte> list = new List<byte>(listSource.Count);

			//Array mit linearen Werten erstellen (16Bit)
			int value16Count = listSource.Count / 2;
			List<Int16> list16Mixed = new List<Int16>(new Int16[value16Count]);

			//In Array umwandeln
			Byte[] bytesSource = listSource.ToArray();
			Byte[] bytesSubstract = listToSubstract.ToArray();

			//Für jeden 16Bit Wert
			for (int i = 0, a = 0; i < value16Count; i++, a += 2)
			{
				//Wenn Werte vorhanden
				if (i < bytesSource.Length && a < bytesSource.Length - 1)
				{
					//Werte ermitteln
					Int16 value16Source = BitConverter.ToInt16(bytesSource, a);
					Int16 value16Substract = BitConverter.ToInt16(bytesSubstract, a);
					int value32 = value16Source - value16Substract;

					//Wert addieren	(Überläufe abfangen)
					if (value32 < Int16.MinValue)
					{
						value32 = Int16.MinValue;
					}
					else if (value32 > Int16.MaxValue)
					{
						value32 = Int16.MaxValue;
					}

					//Wert setzen
					list16Mixed[i] = Convert.ToInt16(value32);
				}
			}

			//Für jeden Wert
			foreach (Int16 v16 in list16Mixed)
			{
				//Integer nach Bytes umwandeln
				Byte[] bytes = BitConverter.GetBytes(v16);
				list.AddRange(bytes);

			}

			//Fertig
			return list;
		}
	}
}
