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
      try
      {
        int octet1 = Int32.Parse(ip.Split(new Char[] { '.' }, 4)[0]);
        if ((octet1 >= 224) && (octet1 <= 239))
          return true;
      }
      catch (Exception ex)
      {
        string str = ex.Message;
      }

      return false;
    }
  }
  /// <summary>
  /// Sender
  /// </summary>
  class Sender
  {
    /// <summary>
    /// DoAction
    /// </summary>
    /// <param name="args"></param>
    public void Send(string[] args)
    {
      if ((args.Length < 2) || (args.Length > 3))
        throw new ArgumentException("Parameter(s): <Multicast Addr> <Port> [<TTL>]");

      // Prüfe ob es sich um eine gültige Multicast-Adresse handelt
      if (!MCIPAddress.isValid(args[0]))
        throw new ArgumentException("Valid MC addr: 224.0.0.0 - 239.255.255.255");

      IPAddress destAddr = IPAddress.Parse(args[0]);  // Zieladresse

      int destPort = Int32.Parse(args[1]);    // Zielport

      int TTL;    // Time-to-live für das Datagramm

      if (args.Length == 3)
        TTL = Int32.Parse(args[2]);
      else
        TTL = 1;    // Standard TTL



      Socket sock = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram,
                               ProtocolType.Udp); // Multicast Socket

      // Setze TTL
      sock.SetSocketOption(SocketOptionLevel.IP,
                           SocketOptionName.MulticastTimeToLive,
                           TTL);

      Byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Hallo");

      // Generiere Endpunkt
      IPEndPoint endPoint = new IPEndPoint(destAddr, destPort);

      // Sende den kodierten Artikel als UDP-Paket
      sock.SendTo(bytes, 0, bytes.Length, SocketFlags.None, endPoint);

      sock.Close();
    }
  }
}
