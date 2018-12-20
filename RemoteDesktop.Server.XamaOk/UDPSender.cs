using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteDesktop.Server.XamaOK
{

    public class UDPSender : Stream
    {

        public UDPSender(String address, Int32 port, int TTL)
        {
            //Daten übernehmen
            m_Address = address;
            m_Port = port;
            m_TTL = TTL;

            //Initialisieren
            Init();
        }

        //Attribute 
        private Socket m_Socket;
        private IPEndPoint m_EndPoint;
        private static EndPoint m_remote_EndPoint;
        private String m_Address;
        private Int32 m_Port;
        private Int32 m_TTL;
        public bool disconnected = false;
        private bool connected = false;

        private void Init()
        {
            //Zieladresse
            IPAddress destAddr = IPAddress.Parse(m_Address);
            //Multicast Socket
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


            //Generiere Endpoint (local bind)
            m_EndPoint = new IPEndPoint(destAddr, m_Port);
            m_remote_EndPoint = new IPEndPoint(IPAddress.Any, 0);

            m_Socket.Bind(m_EndPoint);

            connected = false;
            // recognize remote app address (block until recieve any message)
            m_Socket.ReceiveFrom(new byte[1024], ref m_remote_EndPoint);
            connected = true;

        }

        // ファイル配信の場合のみ対応. データ配信はデータ位置のみ先頭に戻して継続 (送信先は新たに接続を受けたマシン).
        public void checkNextClient()
        {
            // SocketAsyncEventArgs コンテキスト オブジェクトを作成する。 
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            //socketEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, portNumber);

            // 非同期処理が完了したことを通知するために信号を送るオブジェクト。
            ManualResetEvent clientDone = new ManualResetEvent(false);
            // データを受信するためのバッファーを設定する。 
            socketEventArg.SetBuffer(new Byte[1024], 0, 1024);

            String response = "";
            // Completed イベントのインライン イベント ハンドラー。 
            // 注: メソッドを自己完結させるため、このイベント ハンドラーはインラインで実装される。 
            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
            {
                if (e.SocketError == SocketError.Success)
                {
                    // バッファーからデータを取得する。 
                    response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    response = response.Trim('\0');
                }
                else
                {
                    response = e.SocketError.ToString();
                }
                //m_remote_EndPoint_new = e.RemoteEndPoint;
                disconnected = true;

                clientDone.Set();
            });

            // イベントの状態をシグナルなしに設定し、スレッドのブロックを発生させる。 
            clientDone.Reset();

            //var remote = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
            //socketEventArg.RemoteEndPoint = remote;
            socketEventArg.RemoteEndPoint = m_remote_EndPoint;

            // ソケットを使用して非同期の受信要求を行う。 
            m_Socket.ReceiveFromAsync(socketEventArg);

            // 指定ミリ秒を最大秒数として UI スレッドをブロックする。 
            // この時間内に応答がなければ、処理を先に進める。 
            clientDone.WaitOne(10);
        }

        public override bool CanRead {
            get {
                return connected;
            }
        }

        public override bool CanSeek {
            get {
                return false;
            }
        }

        public override bool CanWrite {
            get {
                return connected;
            }
        }

        public override long Length
        {
            get
            {
                return Int32.MaxValue;
            }
        }

        public override long Position
        {
            get { const long V = 0L; return V; }
            set { }
        }

        public override void Close()
        {
            m_Socket.Close();
        }

        public void SendBytes(Byte[] bytes)
        {
            m_Socket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, m_remote_EndPoint);
        }

        public void SendText(String str)
        {
            this.SendBytes(Encoding.ASCII.GetBytes(str));
        }

        public override void Flush()
        {
            // Do nothing
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return offset;
        }

        public override void SetLength(long value)
        {
            // do nothing
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            SendBytes(buffer);
        }
    }

}

