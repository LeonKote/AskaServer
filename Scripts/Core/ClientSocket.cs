using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AskaServer
{
	public class ClientSocket
	{
		private TcpClient tcpClient;
		private StreamReader streamReader;
		private StreamWriter streamWriter;

		public ClientSocket(TcpClient tcpClient)
		{
			this.tcpClient = tcpClient;
			this.streamReader = new StreamReader(tcpClient.GetStream());
			this.streamWriter = new StreamWriter(tcpClient.GetStream());
			this.streamWriter.AutoFlush = true;
		}

		public async void Start()
		{
			try
			{
				while (true)
				{
					string? str = await streamReader.ReadLineAsync();

					if (str == null)
					{
						OnDisconnect();
						break;
					}

					OnMessage(str);
				}
			}
			catch (IOException)
			{
				OnDisconnect();
			}
		}

		protected virtual void OnMessage(string message)
		{

		}

		public virtual void OnConnect()
		{

		}

		public virtual void OnDisconnect()
		{

		}

		public string GetIP()
		{
			return ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
		}

		public void Send(string key, JToken? value)
		{
			try
			{
				streamWriter.WriteLine(new JObject { { key, value } }.ToString(Formatting.None));
			}
			catch (IOException)
			{

			}
		}
	}
}
