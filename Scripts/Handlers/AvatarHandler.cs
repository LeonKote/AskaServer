using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskaServer
{
	public static class AvatarHandler
	{
		public static void Execute(Client client, JObject request)
		{
			JToken? jToken = request["avatar"];
			if (jToken == null || jToken.Type != JTokenType.String) return;

			Image image;

			try
			{
				image = Image.Load(Convert.FromBase64String((string)jToken));
			}
			catch
			{
				client.Send("avatar", new JObject { { "error", "wrongImage" } });
				return;
			}

			Utils.ResizeImage(image, 100);
			image.SaveAsJpeg($"profiles/{client.Id}.jpg");
			client.Send("avatar", true);
			image.Dispose();
		}
	}
}
