using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace NF
{
  /// <summary>
  /// MCIPAddress Class
  /// </summary>
  public class MCIPAddress
  {
    // Überprüfe ob es sich um eine gültige IPv4 Multicast-Adresse handelt
    public static bool isValid(string ip)
    {
        //try
        //{
        //  int octet1 = Int32.Parse(ip.Split(new Char[] { '.' }, 4)[0]);
        //  if ((octet1 >= 224) && (octet1 <= 239))
        //    return true;
        //}
        //catch (Exception ex)
        //{
        //  string str = ex.Message;
        //}

        //return false;
        return true;
    }
  }
  /// <summary>
  /// Sender
  /// </summary>
  public class MulticastSender
  {

    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <param name="TTL"></param>
    public MulticastSender(String address, Int32 port, int TTL)
    {
      //Prüfe ob es sich um eine gültige Multicast-Adresse handelt
      if (!MCIPAddress.isValid(address))
        throw new ArgumentException("Valid MC addr: 224.0.0.0 - 239.255.255.255");


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
    private String m_Address;
    private Int32 m_Port;
    private Int32 m_TTL;

    /// <summary>
    /// Init
    /// </summary>
    /// <param name="args"></param>
    private void Init()
    {
      //Zieladresse
      IPAddress destAddr = IPAddress.Parse(m_Address);
      //Multicast Socket
      m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); 
      //Setze TTL
      //m_Socket.SetSocketOption(SocketOptionLevel.IP,SocketOptionName.MulticastTimeToLive, m_TTL);

      // DEBUG: try change to Multicast to Broadcast for communicate 3rd party Andoid reciever app
      m_Socket.SetSocketOption(SocketOptionLevel.IP,SocketOptionName.IpTimeToLive, 16);
      m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

      // Generiere Endpunkt
      m_EndPoint = new IPEndPoint(destAddr, m_Port);
    }
    /// <summary>
    /// Close
    /// </summary>
    public void Close()
    {
      m_Socket.Close();
    }
    /// <summary>
    /// Bytes versenden
    /// </summary>
    /// <param name="args"></param>
    public void SendBytes(Byte[] bytes)
    {
      m_Socket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, m_EndPoint);
    }
    /// <summary>
    /// Text versenden
    /// </summary>
    /// <param name="str"></param>
    public void SendText(String str)
    {
      this.SendBytes(Encoding.ASCII.GetBytes(str));
    }
  }
}
