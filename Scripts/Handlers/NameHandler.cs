using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AskaServer
{
	public static class NameHandler
	{
		public static readonly Regex NameRegex = new Regex("^[A-Za-zА-Яа-я0-9_]{3,18}$");

		public static void Execute(Client client, JObject request)
		{
			JToken? jToken = request["name"];
			if (jToken == null || jToken.Type != JTokenType.String) return;

			if (!NameRegex.IsMatch((string)jToken))
			{
				client.Send("name", new JObject { { "error", "wrongName" } });
				return;
			}

			MySql mysql = new MySql();
			mysql.Update($"UPDATE `users` SET `username` = '{(string)jToken}' WHERE `users`.`id` = {client.Id}");
			mysql.Close();

			Logger.Log($"{client.Name} changed name to {(string)jToken}");

			client.Name = (string)jToken;
			client.Send("name", true);
		}
	}
}
