using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace RemoteDesktop.Core
{
	///// <summary>
	///// MCIPAddress Class
	///// </summary>
	//public class MCIPAddress
	//{
	//	// Überprüfe ob es sich um eine gültige IPv4 Multicast-Adresse handelt
	//	public static bool isValid(string ip)
	//	{
	//		try
	//		{
	//			int octet1 = Int32.Parse(ip.Split(new Char[] { '.' }, 4)[0]);
	//			if ((octet1 >= 224) && (octet1 <= 239))
	//				return true;
	//		}
	//		catch (Exception ex)
	//		{
	//			string str = ex.Message;
	//		}

	//		return false;
	//	}
	//}

	/// <summary>
	/// Multicast Receiver Klasse
	/// </summary>
	public class RTPReceiver
	{
		/// <summary>
		/// Konstruktor
		/// </summary>
		public RTPReceiver(int bufferSize)
		{
			bytes = new Byte[bufferSize];
		}

		//Attribute
		Socket m_Socket;
		IPAddress m_Address;
		Int32 m_Port;
		EndPoint m_EndPoint;
		Byte[] bytes;
		bool IsConnected = false;
		Object Locker = new Object();
        public int Port = 0;

		//Delegates bzw. Events
		public delegate void DelegateDataReceived2(RTPReceiver mc, Byte[] bytes);
		public delegate void DelegateDisconnected(string Reason);
		public delegate void DelegateExceptionAppeared(Exception ex);
        public event DelegateDataReceived2 DataReceived2;
		public event DelegateDisconnected Disconnected;

        /// <summary>
        /// Address
        /// </summary>
        public string Address
        {
            get
            {
                return m_Address.ToString();
            }
        }
		/// <summary>
		/// Connected
		/// </summary>
		public bool Connected
		{
			get
			{
				return IsConnected;
			}
		}
		/// <summary>
		/// Connect
		/// </summary>
		/// <param name="strAdress"></param>
		/// <param name="port"></param>
		public void Connect(string strAddress, int port)
		{
			//Zieladresse und Port setzen
			m_Address = IPAddress.Parse(strAddress);
			m_Port = port;
			//Socket erstellen
			m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); // Multicast Socket
			//Adresse wiederverwenden
			m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			//Generiere Endpunkt
			m_EndPoint = new IPEndPoint(IPAddress.Any, m_Port);
			m_Socket.Bind(m_EndPoint);
			//Mitgliedschaft in der Multicast Gruppe bekannt geben
			m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(m_Address, IPAddress.Any));
			//Verbunden
			IsConnected = true;
			//Beginnen zu lesen
			this.DoRead();
		}
		/// <summary>
		/// Startet das Lesen 
		/// </summary>
		/// <param name="Data"></param>
		private void DoRead()
		{
			try
			{
				m_Socket.BeginReceive(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), m_Socket);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}
		/// <summary>
		/// OnDataReceived
		/// </summary>
		/// <param name="ar"></param>
		private void OnDataReceived(IAsyncResult ar)
		{
			try
			{
				//So lange wie verbunden
                if (IsConnected)
                {
                    //Anzahl gelesener Bytes
                    int read = m_Socket.EndReceive(ar);

                    //Wenn Daten vorhanden
                    if (read > 0)
                    {
                        //Wenn das Event verwendet wird
                        if (this.DataReceived2 != null)
                        {
                            //Wenn gelesene Bytes und Datengrösse übereinstimmen
                            if (read == bytes.Length)
                            {
                                //Event abschicken
                                this.DataReceived2(this, bytes);
                            }
                            else
                            {
                                //Nur den gelesenen Teil übermitteln
                                Byte[] readed = new Byte[read];
                                Array.Copy(bytes, readed, read);

                                //Event abschicken
                                this.DataReceived2(this, readed);
                            }
                        }

                        //Weiterlesen
                        m_Socket.BeginReceive(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
                    }
                    else
                    {
                        //Verbindung beendet
                        m_Socket.Close();
                    }
                }
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(String.Format("MulticastReceiver.cs | OnDataReceived() | {0}", ex.Message));
				//Verbindung beenden
				Disconnect();
			}
		}
		/// <summary>
		/// Disconnect
		/// </summary>
		public void Disconnect()
		{
			if (IsConnected)
			{
				//Nicht mehr verbunden
				IsConnected = false;
				//Kündige Mitgliedschaft in der Multicast Gruppe
				m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(m_Address, IPAddress.Any));
				//Verbindung beenden
				m_Socket.Close();

				//Event abschicken
				if (this.Disconnected != null)
				{
					this.Disconnected("Connection has been finished");
				}
			}
		}
	}
}
