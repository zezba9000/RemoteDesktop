using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktop.Core
{
	public static class DebugLog
	{
		private static void Write(string message)
		{
			Console.WriteLine(message);
		}

		public static void Log(string message)
		{
			Write(message);
		}

		public static void LogWarning(string message)
		{
			Write("WARNING: " + message);
		}

		public static void LogError(string message)
		{
			Write("ERROR: " + message);
		}
	}
}
