using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace NF
{
    public class TCPClient
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public TCPClient(String server, int port)
        {
            //Daten setzen
            this.m_Server = server;
            this.m_Port = port;

            //Events anhängen
            this.ExceptionAppeared += new DelegateException(this.OnExceptionAppeared);
            this.ClientConnected += new DelegateConnection(this.OnConnected);
            this.ClientDisconnected += new DelegateConnection(this.OnDisconnected);
        }

        //Attribute
        public TcpClient Client;
        NetworkStream m_NetStream;
        byte[] m_ByteBuffer;
        String m_Server;
        int m_Port;
        bool m_AutoConnect = false;
        private System.Threading.Timer m_TimerAutoConnect;
        private int m_AutoConnectInterval = 10;

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0} {1}:{2}", this.GetType(), this.m_Server, this.m_Port);
        }

        /// <summary>
        /// Locker Klasse für AutoConnect
        /// </summary>
        private class Locker_AutoConnectClass
        {
        }
        private Locker_AutoConnectClass Locker_AutoConnect = new Locker_AutoConnectClass();


        //Delegates bzw. Events
        public delegate void DelegateDataReceived(NF.TCPClient client, Byte[] bytes);
        public delegate void DelegateDataSend(NF.TCPClient client, Byte[] bytes);
        public delegate void DelegateDataReceivedComplete(NF.TCPClient client, String message);
        public delegate void DelegateConnection(NF.TCPClient client, string Info);
        public delegate void DelegateException(NF.TCPClient client, Exception ex);
        public event DelegateDataReceived DataReceived;
        public event DelegateDataSend DataSend;
        public event DelegateConnection ClientConnected;
        public event DelegateConnection ClientDisconnected;
        public event DelegateException ExceptionAppeared;

        /// <summary>
        /// Timer für AutoConnect initialisieren
        /// </summary>
        private void InitTimerAutoConnect()
        {
            //Wenn AutoConnect
            if (m_AutoConnect)
            {
                if (m_TimerAutoConnect == null)
                {
                    if (m_AutoConnectInterval > 0)
                    {
                        m_TimerAutoConnect = new System.Threading.Timer(new System.Threading.TimerCallback(OnTimer_AutoConnect), null, m_AutoConnectInterval * 1000, m_AutoConnectInterval * 1000);
                    }
                }
            }
        }
        /// <summary>
        /// Daten senden
        /// </summary>
        /// <param name="Data"></param>
        public void Send(Byte[] data)
        {
            try
            {
                // Sende den kodierten string an den m_Server
                m_NetStream.Write(data, 0, data.Length);

                //Event abschicken
                if (this.DataSend != null)
                {
                    this.DataSend(this, data);
                }
            }
            catch (Exception ex)
            {
                //Exception Event abschicken
                ExceptionAppeared(this, ex);
            }
        }
        /// <summary>
        /// Startet das Lesen 
        /// </summary>
        /// <param name="Data"></param>
        private void StartReading()
        {
            try
            {
                m_ByteBuffer = new byte[1024];
                m_NetStream.BeginRead(m_ByteBuffer, 0, m_ByteBuffer.Length, new AsyncCallback(OnDataReceived), m_NetStream);
            }
            catch (Exception ex)
            {
                //Exception Event abschicken
                ExceptionAppeared(this, ex);
            }
        }
        /// <summary>
        /// Wird aufgerufen wenn Daten erhalten wurden
        /// </summary>
        /// <param name="result"></param>
        private void OnDataReceived(IAsyncResult ar)
        {
            try
            {
                //Netzwerkstream ermitteln
                NetworkStream myNetworkStream = (NetworkStream)ar.AsyncState;

                //Networkstream prüfen
                if (myNetworkStream.CanRead)
                {
                    //Daten lesen
                    int numberOfBytesRead = myNetworkStream.EndRead(ar);

                    //Wenn Daten vorhanden
                    if (numberOfBytesRead > 0)
                    {
                        //Event abschicken
                        if (this.DataReceived != null)
                        {
                            //Nur gelesene Bytes ermitteln
                            Byte[] data = new byte[numberOfBytesRead];
                            System.Array.Copy(m_ByteBuffer, 0, data, 0, numberOfBytesRead);

                            //Abschicken
                            this.DataReceived(this, data);
                        }
                    }
                    else
                    {
                        //Event abschicken
                        if (this.ClientDisconnected != null)
                        {
                            this.ClientDisconnected(this, "FIN");
                        }

                        //Wenn kein AutoConnect
                        if (m_AutoConnect == false)
                        {
                            this.disconnect_intern();
                        }
                        else
                        {
                            this.Disconnect_ButAutoConnect();
                        }

                        //Fertig
                        return;
                    }

                    //Neuer Lesevorgang
                    myNetworkStream.BeginRead(m_ByteBuffer, 0, m_ByteBuffer.Length, new AsyncCallback(OnDataReceived), myNetworkStream);
                }
            }
            catch (Exception ex)
            {
                //Exception Event abschicken
                ExceptionAppeared(this, ex);
            }
        }
        /// <summary>
        /// Neu Verbinden
        /// </summary>
        public void ReConnect()
        {
            //Verbindung beenden
            this.Disconnect();
            //Neue Verbindung starten
            this.Connect();
        }
        /// <summary>
        /// Verbindung aufbauen
        /// </summary>
        public void Connect()
        {
            try
            {
                //Evtl. AutoConnect aktivieren
                InitTimerAutoConnect();

                // Erzeuge neuen Socket der an den m_Server und m_Port gebunden ist
                Client = new TcpClient(this.m_Server, this.m_Port);
                m_NetStream = Client.GetStream();

                //Beginn zu lesen
                this.StartReading();

                //Event abschicken
                ClientConnected(this, String.Format("server: {0} port: {1}", this.m_Server, this.m_Port));
            }
            catch (Exception ex)
            {
                //Weiterleiten
                throw ex;
            }
        }
        /// <summary>
        /// Server des Clients anpingen
        /// </summary>
        public void Ping()
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(m_Server);
            if (reply.Status != IPStatus.Success)
            {
                throw new Exception(String.Format("Server {0} antwortet nicht auf Ping ", m_Server));
            }
        }
        /// <summary>
        /// Server des Clients anpingen. Angabe der maximalen Wartezeit in Millisekungen.
        /// </summary>
        /// <param name="waitTimeout"></param>
        public void Ping(Int32 waitTimeout)
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(m_Server, waitTimeout);
            if (reply.Status != IPStatus.Success)
            {
                throw new Exception(String.Format("Server {0} antwortet nicht auf Ping ", m_Server));
            }
        }
        /// <summary>
        /// Verbindung beenden
        /// </summary>
        public void Disconnect()
        {
            //Verbindung beenden
            disconnect_intern();

            //Wenn nicht schon beendet
            if (m_TimerAutoConnect != null)
            {
                //Nicht mehr wiederverbinden
                m_TimerAutoConnect.Dispose();
                m_TimerAutoConnect = null;
            }

            //Event abschicken
            if (this.ClientDisconnected != null)
            {
                this.ClientDisconnected(this, "Verbindung beendet");
            }
        }
        /// <summary>
        /// Verbindung beenden, aber AutoConnect beibehalten
        /// </summary>
        private void Disconnect_ButAutoConnect()
        {
            //Verbindung beenden
            disconnect_intern();
        }
        /// <summary>
        /// Verbindung beenden (intern)
        /// </summary>
        private void disconnect_intern()
        {
            if (Client != null)
            {
                Client.Close();
            }
            if (m_NetStream != null)
            {
                m_NetStream.Close();
            }
        }
        /// <summary>
        /// Timer der das automatische Verbinden steuert
        /// </summary>
        /// <param name="ob"></param>
        private void OnTimer_AutoConnect(Object ob)
        {
            try
            {
                lock (Locker_AutoConnect)
                {
                    //Wenn gewünscht
                    if (m_AutoConnect)
                    {
                        //Wenn nicht verbunden
                        if (Client == null || Client.Connected == false)
                        {
                            //Erzeuge neuen Socket der an den m_Server und m_Port gebunden ist
                            Client = new TcpClient(this.m_Server, this.m_Port);
                            m_NetStream = Client.GetStream();

                            //Beginn zu lesen
                            this.StartReading();

                            //Event abschicken
                            ClientConnected(this, String.Format("server: {0} port: {1}", this.m_Server, this.m_Port));
                        }
                    }
                    else
                    {
                        if (m_TimerAutoConnect != null)
                        {
                            //Timer beenden
                            m_TimerAutoConnect.Dispose();
                            m_TimerAutoConnect = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Exception Event abschicken
                ExceptionAppeared(this, ex);
            }
        }
        /// <summary>
        /// Wenn eine Exception passiert
        /// </summary>
        /// <param name="ex"></param>
        private void OnExceptionAppeared(NF.TCPClient client, Exception ex)
        {

        }
        /// <summary>
        /// Wenn sich ein Client verbunden hat
        /// </summary>
        /// <param name="client"></param>
        private void OnConnected(NF.TCPClient client, string info)
        {

        }
        /// <summary>
        /// Wenn sich ein Client getrennt hat
        /// </summary>
        /// <param name="client"></param>
        private void OnDisconnected(NF.TCPClient client, string info)
        {

        }
        /// <summary>
        /// Interval für AutoConnect in Sekunden
        /// </summary>
        public Int32 AutoConnectInterval
        {
            get
            {
                return m_AutoConnectInterval;
            }
            set
            {
                m_AutoConnectInterval = value;

                //Prüfen
                if (value > 0)
                {
                    try
                    {
                        //Wenn schnon aktiv
                        if (m_TimerAutoConnect != null)
                        {
                            //Ändern
                            m_TimerAutoConnect.Change(value * 1000, value * 1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionAppeared(this, ex);
                    }
                }
            }
        }
        /// <summary>
        /// Regelt die Automatische Wiederverbindung
        /// </summary>
        /// <returns></returns>
        public bool AutoConnect
        {
            get
            {
                return m_AutoConnect;
            }
            set
            {
                m_AutoConnect = value;

                if (value == true)
                {
                    InitTimerAutoConnect();
                }

            }
        }
        /// <summary>
        /// Gibt an ob der Client versucht sich über AutoConnect zu verbinden
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return m_TimerAutoConnect != null;
            }
        }
        /// <summary>
        /// Gibt an ob der Client verbunden ist. Readonly
        /// </summary>
        /// <returns></returns>
        public bool Connected
        {
            get
            {
                if (this.Client != null)
                {
                    return this.Client.Connected;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
