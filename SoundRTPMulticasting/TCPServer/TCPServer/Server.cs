using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Collections;

namespace NF
{
    /// <summary>
    /// TCPServer
    /// </summary>
    public class TCPServer
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public TCPServer()
        {

        }

        //Attribute
        private IPEndPoint m_endpoint;
        private TcpListener m_tcpip;
        private Thread m_ThreadMainServer;
        private ListenerState m_State;


        //Die Liste der laufenden TCPServer-Threads
        private List<ServerThread> m_threads = new List<ServerThread>();

        //Delegates
        public delegate void DelegateClientConnected(ServerThread st);
        public delegate void DelegateClientDisconnected(ServerThread st, string info);
        public delegate void DelegateDataReceived(ServerThread st, Byte[] data);

        //Events
        public event DelegateClientConnected ClientConnected;
        public event DelegateClientDisconnected ClientDisconnected;
        public event DelegateDataReceived DataReceived;

        /// <summary>
        /// TCPServer Stati
        /// </summary>
        public enum ListenerState
        {
            None,
            Started,
            Stopped,
            Error
        };
        /// <summary>
        /// Alle Aktuellen Clients des Servers
        /// </summary>
        public List<ServerThread> Clients
        {
            get
            {
                return m_threads;
            }
        }
        /// <summary>
        /// Connected
        /// </summary>
        public ListenerState State
        {
            get
            {
                return m_State;
            }
        }
        /// <summary>
        /// Gibt den inneren TcpListener des Servers zurück
        /// </summary>
        public TcpListener Listener
        {
            get
            {
                return this.m_tcpip;
            }
        }
        /// <summary>
        /// Starten des Servers
        /// </summary>
        public void Start(string strIPAdress, int Port)
        {
            //Endpoint und Listener bestimmen
            m_endpoint = new IPEndPoint(IPAddress.Parse(strIPAdress), Port);
            m_tcpip = new TcpListener(m_endpoint);

            if (m_tcpip == null) return;

            try
            {
                m_tcpip.Start();

                // Haupt-TCPServer-Thread initialisieren und starten
                m_ThreadMainServer = new Thread(new ThreadStart(Run));
                m_ThreadMainServer.Start();

                //State setzen
                this.m_State = ListenerState.Started;
            }
            catch (Exception ex)
            {
                //Beenden
                m_tcpip.Stop();
                this.m_State = ListenerState.Error;

                //Exception werfen
                throw ex;
            }
        }
        /// <summary>
        /// Run
        /// </summary>
        private void Run()
        {
            while (true)
            {
                //Wartet auf eingehenden Verbindungswunsch
                TcpClient client = m_tcpip.AcceptTcpClient();
                //Initialisiert und startet einen TCPServer-Thread
                //und fügt ihn zur Liste der TCPServer-Threads hinzu
                ServerThread st = new ServerThread(client);

                //Events hinzufügen
                st.DataReceived += new ServerThread.DelegateDataReceived(OnDataReceived);
                st.ClientDisconnected += new ServerThread.DelegateClientDisconnected(OnClientDisconnected);

                //Weitere Arbeiten
                OnClientConnected(st);

                try
                {
                    //Beginnen zu lesen
                    client.Client.BeginReceive(st.ReadBuffer, 0, st.ReadBuffer.Length, SocketFlags.None, st.Receive, client.Client);
                }
                catch (Exception ex)
                {
                    //Verbindung fehlerhaft
                    Console.WriteLine(ex.Message);
                }
            }
        }
        /// <summary>
        /// Nachricht an alle verbundenen Clients senden. Gibt die Anzahl der vorhandenen Clients zurück
        /// </summary>
        /// <param name="Message"></param>
        public int Send(Byte[] data)
        {
            //Für jede Verbindung
					  List<ServerThread> list = new List<ServerThread>(m_threads);
            foreach (ServerThread sv in list)
            {
                try
                {
                    //Senden
                    if (data.Length > 0)
                    {
                        sv.Send(data);
                    }
                }
                catch (Exception)
                {

                }
            }
            //Anzahl zurückgeben
            return m_threads.Count;
        }
        /// <summary>
        /// Wird ausgeführt wenn Daten angekommen sind
        /// </summary>
        /// <param name="Data"></param>
        private void OnDataReceived(ServerThread st, Byte[] data)
        {
            //Event abschicken bzw. weiterleiten
            if (DataReceived != null)
            {
                DataReceived(st, data);
            }
        }
        /// <summary>
        /// Wird aufgerufen wenn sich ein Client beendet
        /// </summary>
        /// <param name="st"></param>
        private void OnClientDisconnected(ServerThread st, string info)
        {
            //Aus Liste entfernen
            m_threads.Remove(st);

            //Event abschicken bzw. weiterleiten
            if (ClientDisconnected != null)
            {
                ClientDisconnected(st, info);
            }
        }
        /// <summary>
        /// Wird aufgerufen wenn sich ein Client verbindet
        /// </summary>
        /// <param name="st"></param>
        private void OnClientConnected(ServerThread st)
        {
            //Wenn nicht vorhanden
            if (!m_threads.Contains(st))
            {
                //Zur Liste der Clients hinzufügen
                m_threads.Add(st);
            }

            //Event abschicken bzw. weiterleiten
            if (ClientConnected != null)
            {
                ClientConnected(st);
            }
        }
        /// <summary>
        /// Beenden des Servers
        /// </summary>
        public void Stop()
        {
            try
            {
                if (m_ThreadMainServer != null)
                {
                    // Haupt-TCPServer-Thread stoppen
                    m_ThreadMainServer.Abort();
                    System.Threading.Thread.Sleep(100);
                }

                // Alle TCPServer-Threads stoppen
                for (IEnumerator en = m_threads.GetEnumerator(); en.MoveNext(); )
                {
                    //Nächsten TCPServer-Thread holen
                    ServerThread st = (ServerThread)en.Current;
                    //und stoppen
                    st.Stop();

                    //Event abschicken
                    if (ClientDisconnected != null)
                    {
                        ClientDisconnected(st, "Verbindung wurde beendet");
                    }
                }

                if (m_tcpip != null)
                {
                    //Listener stoppen
                    m_tcpip.Stop();
                    m_tcpip.Server.Close();
                }

                //Liste leeren
                m_threads.Clear();
                //Status vermerken
                this.m_State = ListenerState.Stopped;

            }
            catch (Exception)
            {
                this.m_State = ListenerState.Error;
            }
        }
    }

    /// <summary>
    /// ServerThread eines Servers
    /// </summary>
    public class ServerThread
    {
        // Stop-Flag
        private bool m_IsStopped = false;
        // Die Verbindung zum Client
        private TcpClient m_Connection = null;
        //Lesepuffer
        public byte[] ReadBuffer = new byte[1024];
        //Mute
        public bool IsMute = false;
        //Name
        public String Name = "";

        public delegate void DelegateDataReceived(ServerThread st, Byte[] data);
        public event DelegateDataReceived DataReceived;
        public delegate void DelegateClientDisconnected(ServerThread sv, string info);
        public event DelegateClientDisconnected ClientDisconnected;

        /// <summary>
        /// Inneren Client
        /// </summary>
        public TcpClient Client
        {
            get
            {
                return m_Connection;
            }
        }
        /// <summary>
        /// Verbindung ist beendet
        /// </summary>
        public bool IsStopped
        {
            get
            {
                return m_IsStopped;
            }
        }
        // Speichert die Verbindung zum Client und startet den Thread
        public ServerThread(TcpClient connection)
        {
            // Speichert die Verbindung zu Client,
            // um sie später schließen zu können
            this.m_Connection = connection;
        }
        /// <summary>
        /// Nachrichten lesen
        /// </summary>
        /// <param name="ar"></param>
        public void Receive(IAsyncResult ar)
        {
            try
            {
                //Wenn nicht mehr verbunden
                if (this.m_Connection.Client.Connected == false)
                {
                    return;
                }

                if (ar.IsCompleted)
                {
                    //Lesen
                    int bytesRead = m_Connection.Client.EndReceive(ar);

                    //Wenn Daten vorhanden
                    if (bytesRead > 0)
                    {
                        //Nur gelesene Bytes ermitteln
                        Byte[] data = new byte[bytesRead];
                        System.Array.Copy(ReadBuffer, 0, data, 0, bytesRead);

                        //Event abschicken
                        DataReceived(this, data);
                        //Weiter lesen
                        m_Connection.Client.BeginReceive(ReadBuffer, 0, ReadBuffer.Length, SocketFlags.None, Receive, m_Connection.Client);
                    }
                    else
                    {
                        //Verbindung getrennt
                        HandleDisconnection("Verbindung wurde beendet");
                    }
                }
            }
            catch (Exception ex)
            {
                //Verbindung getrennt
                HandleDisconnection(ex.Message);
            }
        }
        /// <summary>
        /// Alles nötige bei einem Verbindungsabbruch unternehmen
        /// </summary>
        public void HandleDisconnection(string reason)
        {
            //Clientverbindung ist beendet
            m_IsStopped = true;

            //Event abschicken
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, reason);
            }
        }
        /// <summary>
        /// Senden von Nachrichten
        /// </summary>
        /// <param name="strMessage"></param>
        public void Send(Byte[] data)
        {
            try
            {
                //Wenn die Verbindung noch besteht
                if (this.m_IsStopped == false)
                {
                    //Hole den Stream für's schreiben
                    NetworkStream ns = this.m_Connection.GetStream();

                    lock (ns)
                    {
                        // Sende den kodierten string an den TCPServer
                        ns.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                //Verbindung schliessen
                this.m_Connection.Close();
                //Verbindung beenden
                this.m_IsStopped = true;

                //Event abschicken
                if (ClientDisconnected != null)
                {
                    ClientDisconnected(this, ex.Message);
                }

                //Exception weiterschicken
                throw ex;
            }
        }
        /// <summary>
        /// Thread anhalten
        /// </summary>
        public void Stop()
        {
            //Wenn ein Client noch verbunden ist
            if (m_Connection.Client.Connected == true)
            {
                //Verbindung beenden
                m_Connection.Client.Disconnect(false);
            }

            this.m_IsStopped = true;
        }
    }
}
