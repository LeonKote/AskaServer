using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AskaServer
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Client : ClientSocket
	{
		[JsonProperty]
		public int Id { get; set; }
		public bool IsAuthed { get; set; }
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public string Image { get; set; }
		public Room Room { get; set; }

		public Client(int id, TcpClient tcpClient) : base(tcpClient)
		{
			Id = id;
		}

		protected override void OnMessage(string message)
		{
			Logger.Log((string.IsNullOrEmpty(Name) ? GetIP() : Name) + ": " + message);

			JObject request;

			try
			{
				request = JObject.Parse(message);
			}
			catch (JsonReaderException)
			{
				return;
			}

			if (!request.HasValues) return;

			JToken? jToken;
			switch (request.Properties().First().Name)
			{
				case "auth":
					if (IsAuthed) return;
					AuthHandler.Execute(this, request);
					break;
				case "searchQuiz":
					if (!IsAuthed) return;

					jToken = request["searchQuiz"];
					if (jToken == null || jToken.Type != JTokenType.String) return;

					Send("searchQuiz", JArray.FromObject(Server.Quizzes.Select(x => x.Value).Where(x => !x.IsHidden && x.Name.Contains((string)jToken, StringComparison.OrdinalIgnoreCase)).Take(10), Utils.JsonSerializer));
					break;
				case "create":
					if (!IsAuthed || Room != null) return;

					jToken = request["create"];
					if (jToken == null || jToken.Type != JTokenType.String || !Server.Quizzes.ContainsKey((string)jToken)) return;

					int id = Utils.RandomRoomId();
					Room room = new Room(id, Server.Quizzes[(string)jToken], this);
					Server.Rooms.Add(id, room);

					Logger.Log($"{Name} created new room #{id} {Server.Quizzes[(string)jToken].Name}");
					break;
				case "join":
					if (!IsAuthed) return;

					jToken = request["join"];
					if (jToken == null || jToken.Type != JTokenType.Integer || !Server.Rooms.ContainsKey((int)jToken)) return;

					Server.Rooms[(int)jToken].OnClientJoin(this);
					break;
				case "leave":
					if (Room == null) return;
					Room.OnClientLeave(this);
					break;
				case "start":
					if (Room == null) return;
					Room.OnGameStart(this);
					break;
				case "answer":
					if (Room == null) return;

					jToken = request["answer"];
					if (jToken == null || jToken.Type != JTokenType.Integer) return;

					Room.OnClientAnswer(this, (int)jToken);
					break;
				case "name":
					if (!IsAuthed) return;
					NameHandler.Execute(this, request);
					break;
				case "avatar":
					if (Id < 0) return;
					AvatarHandler.Execute(this, request);
					break;
				case "userQuiz":
					if (Id < 0) return;
					UserQuizHandler.Execute(this, request);
					break;
			}
		}

		public override void OnConnect()
		{
			Server.Clients.Add(Id, this);
		}

		public override void OnDisconnect()
		{
			Room?.OnClientLeave(this);

			Server.Clients.Remove(Id);
			
			if (!IsAuthed) return;

			Logger.Log($"{Name} has left the server!");
		}
	}
}
