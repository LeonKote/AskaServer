using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AskaServer
{
	public static class UserQuizHandler
	{
		private static readonly Regex idRegex = new Regex("^[a-f0-9]{3,18}$");
		private static readonly Regex nameRegex = new Regex("^[A-Za-zА-Яа-я0-9_ ]{3,64}$");

		private static readonly JpegEncoder jpegEncoder = new JpegEncoder()
		{
			Quality = 30
		};

		public static void Execute(Client client, JObject request)
		{
			JToken? jToken = request["userQuiz"];
			if (jToken == null || jToken.Type != JTokenType.Object) return;

			Quiz? quiz = jToken.ToObject<Quiz>();
			if (quiz == null) return;

			if (idRegex.IsMatch(quiz.Id))
			{
				client.Send("userQuiz", new JObject { { "error", "wrongId" } });
				return;
			}

			if (nameRegex.IsMatch(quiz.Name))
			{
				client.Send("userQuiz", new JObject { { "error", "wrongName" } });
				return;
			}

			if (Server.Quizzes.ContainsKey(quiz.Id))
			{
				client.Send("userQuiz", new JObject { { "error", "idAlreadyExists" } });
				return;
			}

			quiz.Init();
			quiz.AuthorId = client.Id;

			Image image;

			try
			{
				image = Image.Load(Convert.FromBase64String(quiz.Image));
			}
			catch
			{
				client.Send("userQuiz", new JObject { { "error", "wrongQuizImage" } });
				return;
			}

			Utils.ResizeImage(image, 512);
			image.SaveAsJpeg($"quizIcons/{quiz.Id}.jpg", jpegEncoder);
			quiz.Image = Config.GetString("domain") + $"quizzes/{quiz.Id}.jpg";
			image.Dispose();

			foreach (QuizQuestion question in quiz.Questions)
			{
				try
				{
					image = Image.Load(Convert.FromBase64String(question.Image));
				}
				catch
				{
					client.Send("userQuiz", new JObject
					{
						{ "error", "wrongQuestionImage" },
						{ "questionId", question.Id }
					});
					return;
				}

				Utils.ResizeImage(image, 512);
				image.SaveAsJpeg($"quizzes/{quiz.Id}/{question.Id}.jpg", jpegEncoder);
				question.Image = null;
				image.Dispose();
			}

			File.WriteAllText($"quizzes/{quiz.Id}/quiz.json", JsonConvert.SerializeObject(quiz, Utils.JsonSerializerSettings));

			Server.Quizzes.Add(quiz.Id, quiz);

			client.Send("userQuiz", true);

			Logger.Log($"{client.Name} posted new quiz named {quiz.Name}");
		}
	}
}
