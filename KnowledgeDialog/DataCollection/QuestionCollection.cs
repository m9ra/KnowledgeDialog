using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.DataCollection
{
    public class QuestionCollection
    {
        /// <summary>
        /// Questions.
        /// </summary>
        private readonly List<string> _questions = new List<string>();

        /// <summary>
        /// Answer ids.
        /// </summary>
        private readonly List<string> _answerId = new List<string>();

        /// <summary>
        /// Mapping between questions and answers.
        /// </summary>
        private readonly Dictionary<string, string> _questionToAnswerId = new Dictionary<string, string>();

        /// <summary>
        /// Generator for item selection.
        /// </summary>
        private readonly Random _rnd = new Random();

        /// <summary>
        /// Lock for question selection.
        /// </summary>
        private readonly object _L_questions = new object();

        public QuestionCollection(List<string> questions, List<string> answerIds)
        {
            _questions.AddRange(questions);

            for (var i = 0; i < questions.Count; ++i)
            {
                var question = questions[i];
                var answerId = answerIds[i];

                _questionToAnswerId.Add(question, answerId);
            }
        }

        public string GetAnswerId(string question)
        {
            return _questionToAnswerId[question];
        }

        internal string GetRandomQuestion()
        {
            lock (_L_questions)
            {
                return _questions[_rnd.Next(_questions.Count)];
            }
        }

    }
}
