using System;
using System.Collections.Generic;

using System.Net;
using System.Net.PeerToPeer;
using System.Net.Sockets;

namespace RemoteDesktop.Core
{
	public enum NetworkTypes
	{
		Server,
		Client
	}

	public class NetworkHost
	{
		public List<IPEndPoint> endpoints;
		public string name;

		public NetworkHost(string name)
		{
			this.name = name;
			endpoints = new List<IPEndPoint>();
		}

		public override string ToString()
		{
			return name;
		}
	}

	public class NetworkDiscovery : IDisposable
	{
		private NetworkTypes type;

		// server
		private PeerName peerName;
		private PeerNameRegistration peerNameRegistration;

		// client
		private PeerNameResolver peerNameResolver;

		public NetworkDiscovery(NetworkTypes type)
		{
			this.type = type;
		}

		public void Register(string name, int port)
		{
			if (type != NetworkTypes.Server) throw new Exception("Only allowed for server!");

			peerName = new PeerName(name, PeerNameType.Unsecured);
			peerNameRegistration = new PeerNameRegistration();
			peerNameRegistration.PeerName = peerName;
			peerNameRegistration.Comment = Dns.GetHostName();
			peerNameRegistration.Port = port;
			peerNameRegistration.Start();
		}

		public List<NetworkHost> Find(string name)
		{
			if (type != NetworkTypes.Client) throw new Exception("Only allowed for client!");

			var hosts = new List<NetworkHost>();
			peerNameResolver = new PeerNameResolver();
			peerName = new PeerName(name, PeerNameType.Unsecured);
			var results = peerNameResolver.Resolve(peerName);
			foreach (var record in results)
			{
				var host = new NetworkHost(record.Comment);
				foreach (var endpoint in record.EndPointCollection)
				{
					if (endpoint.AddressFamily == AddressFamily.InterNetwork)
					{
						Console.WriteLine(string.Format("Found EndPoint {0}:{1}", endpoint.Address, endpoint.Port));
						host.endpoints.Add(endpoint);
					}
				}

				if (host.endpoints.Count != 0) hosts.Add(host);
			}

			return hosts;
		}

		public void Dispose()
		{
			if (peerNameRegistration != null)
			{
				peerNameRegistration.Stop();
				peerNameRegistration = null;
			}
		}
	}
}
