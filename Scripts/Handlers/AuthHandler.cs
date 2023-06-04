using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AskaServer
{
	public static class AuthHandler
	{
		public static readonly Regex tokenRegex = new Regex("^[a-f0-9]{64}$");

		public static void Execute(Client client, JObject request)
		{
			JToken? jToken = request["auth"];
			if (jToken == null || jToken.Type != JTokenType.Object) return;

			request = (JObject)jToken;

			jToken = request["type"];
			if (jToken == null || jToken.Type != JTokenType.String) return;

			string type = (string)jToken;

			if (type == "service")
			{
				jToken = request["token"];
				if (jToken == null || jToken.Type != JTokenType.String || (string)jToken != Config.GetString("serviceToken")) return;

				int sessionId = (int)request["sessionId"];
				if (!Server.Clients.ContainsKey(sessionId)) return;

				client = Server.Clients[sessionId];

				JObject profile = (JObject)request["profile"];

				int id = (int)profile["id"];
				string name = (string)profile["name"];
				string image = Config.GetString("domain") + $"profiles/{id}.jpg";

				if (Server.Clients.ContainsKey(id))
				{
					client.Send("auth", new JObject { { "error", "alreadyLogged" } });
					return;
				}

				client.Id = id;
				client.IsAuthed = true;
				client.Name = name;
				client.Image = image;
				client.Send("auth", new JObject
				{
					{ "type", "success" },
					{ "token", request["userToken"] },
					{ "profile", Utils.Serialize(client) }
				});
				Logger.Log($"{client.GetIP()} authed as {client.Name} using vk");
			}
			else if (type == "token")
			{
				jToken = request["token"];
				if (jToken == null || jToken.Type != JTokenType.String || !tokenRegex.IsMatch((string)jToken)) return;

				MySql mysql = new MySql();
				MySqlDataReader reader = mysql.Select($"SELECT * FROM `tokens` WHERE `token` LIKE '{(string)jToken}'");

				if (reader.Read())
				{
					reader = mysql.Select($"SELECT * FROM `users` WHERE `id` = {reader.GetInt32(1)}");
					reader.Read();

					int id = reader.GetInt32(0);
					string name = reader.GetString(2);
					string image = Config.GetString("domain") + $"profiles/{id}.jpg";

					if (Server.Clients.ContainsKey(id))
					{
						client.Send("auth", new JObject { { "error", "alreadyLogged" } });
						return;
					}

					mysql.Update($"UPDATE `tokens` SET `expires` = '{DateTime.Now.AddSeconds(2592000).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `tokens`.`token` = '{(string)jToken}'");

					client.Id = id;
					client.IsAuthed = true;
					client.Name = name;
					client.Image = image;
					client.Send("auth", new JObject
					{
						{ "type", "success" },
						{ "profile", Utils.Serialize(client) }
					});
					Logger.Log($"{client.GetIP()} authed as {client.Name} using token");
				}

				mysql.Close();
			}
			else if (type == "vk")
			{
				int authToken = Utils.RandomId();
				MySql mysql = new MySql();
				mysql.Update($"INSERT INTO `authtokens` (`authToken`, `sessionId`, `expires`) VALUES ('{authToken}', '{client.Id}', '{DateTime.Now.AddSeconds(60).ToString("yyyy-MM-dd HH:mm:ss")}')");
				mysql.Close();
				client.Send("auth", new JObject
				{
					{ "type", "url" },
					{ "url", "https://oauth.vk.com/authorize?client_id=7691413&display=page&redirect_uri=https://rtflegion.ru/auth.php&response_type=code&v=5.131&state=" + authToken }
				});
			}
			else if (type == "tg")
			{

			}
			else if (type == "name")
			{
				jToken = request["name"];
				if (jToken == null || jToken.Type != JTokenType.String) return;

				if (!NameHandler.NameRegex.IsMatch((string)jToken))
				{
					client.Send("auth", new JObject { { "error", "wrongName" } });
					return;
				}

				client.IsAuthed = true;
				client.Name = (string)jToken;
				client.Send("auth", new JObject
				{
					{ "type", "success" },
					{ "profile", Utils.Serialize(client) }
				});
				Logger.Log($"{client.GetIP()} authed as {client.Name} anonymously");
			}
		}
	}
}
