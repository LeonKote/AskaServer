using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace AskaServer
{
	public class Server
	{
		public static Dictionary<int, Client> Clients { get; } = new Dictionary<int, Client>();
		public static Dictionary<int, Room> Rooms { get; } = new Dictionary<int, Room>();
		public static Dictionary<string, Quiz> Quizzes { get; } = new Dictionary<string, Quiz>();

		private static void Main(string[] args)
		{
			Config.Load();
			Config.SetDefault(new JObject()
			{
				{ "mysqlServer", "" },
				{ "mysqlUser", "" },
				{ "mysqlPassword", "" },
				{ "mysqlDB", "" },
				{ "serviceToken", "" },
				{ "domain", "" },
			});
			Config.Save();

			foreach (string dir in Directory.GetDirectories("quizzes"))
			{
				string id = Path.GetFileName(dir);
				Quizzes.Add(id, JsonConvert.DeserializeObject<Quiz>(File.ReadAllText(dir + "/quiz.json")).Init());
			}

			MySql.ConnectionString = "server=" + Config.GetString("mysqlServer")
				+ ";user=" + Config.GetString("mysqlUser")
				+ ";password=" + Config.GetString("mysqlPassword")
				+ ";database=" + Config.GetString("mysqlDB");

			TcpListener tcpListener = new TcpListener(IPAddress.Any, 8887);
			tcpListener.Start();
			Logger.Log("Server started on port 8887");

			while (true)
			{
				TcpClient tcpClient = tcpListener.AcceptTcpClient();
				Client client = new Client(Utils.RandomGuestId(), tcpClient);
				client.OnConnect();
				Task.Run(() => client.Start());
			}
		}
	}
}