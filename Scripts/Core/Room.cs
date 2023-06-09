using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskaServer
{
	public class Room
	{
		private Quiz quiz;
		private Client host;
		private Game game;
		private bool isStarted;

		public int Id { get; set; }
		public bool IsStarted { get { return isStarted; } }

		public Dictionary<int, Client> Clients { get; } = new Dictionary<int, Client>();

		public Room(int id, Quiz quiz, Client client)
		{
			this.Id = id;
			this.quiz = quiz;
			this.host = client;
			this.game = new Game(this, quiz);

			OnClientJoin(client);
		}

		public void OnClientJoin(Client client)
		{
			client.Room = this;

			Broadcast("clientJoin", Utils.Serialize(client));

			Clients.Add(client.Id, client);

			client.Send("roomJoin", new JObject()
			{
				{ "code", Id },
				{ "quiz", JObject.FromObject(quiz, Utils.JsonSerializer) },
				{ "clients", JArray.FromObject(Clients.Values, Utils.JsonSerializer) }
			});
		}

		public void OnClientLeave(Client client)
		{
			client.Room = null;

			if (Clients.Count == 1)
			{
				Server.Rooms.Remove(Id);
				Logger.Log($"Room #{Id} was destroyed");
			}

			Clients.Remove(client.Id);

			Broadcast("clientLeave", client.Id);
		}

		public void OnGameStart(Client client)
		{
			if (client != host || game.IsStarted) return;

			isStarted = true;
			Task.Run(() => game.Start());
		}

		public void OnClientAnswer(Client client, int id)
		{
			game.OnAnswer(client, id);
		}

		public void Broadcast(string key, JToken? value = null)
		{
			foreach (Client client in Clients.Values)
			{
				client.Send(key, value);
			}
		}
	}
}
