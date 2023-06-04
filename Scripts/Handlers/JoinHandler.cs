using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskaServer
{
	public static class JoinHandler
	{
		private static readonly Dictionary<int, string> easterEggQuizzes = new Dictionary<int, string>()
		{
			{ 250252, "easteregg" }
		};

		public static void Execute(Client client, JObject request)
		{
			JToken jToken = request["join"];
			if (jToken == null || jToken.Type != JTokenType.Integer) return;

			int code = (int)jToken;

			if (easterEggQuizzes.ContainsKey(code))
			{
				if (!Server.Rooms.ContainsKey(code))
				{
					Quiz quiz = Server.Quizzes[easterEggQuizzes[code]];
					Room room = new Room(code, quiz, client);
					Server.Rooms.Add(code, room);

					Logger.Log($"{client.Name} created new room #{code} {quiz.Name}");
					return;
				}
				else if (Server.Rooms[code].IsStarted)
				{
					client.Send("join", new JObject() { { "error", "unknownErr" } });
					return;
				}

				Server.Rooms[code].OnClientJoin(client);
				return;
			}

			if (!Server.Rooms.ContainsKey(code))
			{
				client.Send("join", new JObject() { { "error", "doesntExistsErr" } });
				return;
			}
			else if (Server.Rooms[code].IsStarted)
			{
				client.Send("join", new JObject() { { "error", "startedErr" } });
				return;
			}

			Server.Rooms[code].OnClientJoin(client);
		}
	}
}
