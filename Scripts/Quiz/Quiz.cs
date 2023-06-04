using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskaServer
{
	public class Quiz
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }
		public int QuestionsCount { get; set; }
		public int AuthorId { get; set; }
		public bool IsHidden { internal get; set; }
		public QuizQuestion[] Questions { internal get; set; }

		public Quiz Init()
		{
			Image = Config.GetString("domain") + $"quizzes/{Id}.jpg";

			for (int i = 0; i < Questions.Length; i++)
			{
				Questions[i].Id = i + 1;
				Questions[i].CalculateTimings();
			}
			return this;
		}
	}
}
