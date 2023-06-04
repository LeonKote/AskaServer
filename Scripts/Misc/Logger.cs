using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskaServer
{
	public static class Logger
	{
		public static void Log(string message)
		{
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
		}
	}
}
