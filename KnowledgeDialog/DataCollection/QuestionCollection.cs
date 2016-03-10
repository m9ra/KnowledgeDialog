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
        /// Generator for item selection.
        /// </summary>
        private readonly Random _rnd = new Random();

        /// <summary>
        /// Lock for question selection.
        /// </summary>
        private readonly object _L_questions = new object();

        public QuestionCollection(IEnumerable<string> questions)
        {
            _questions.AddRange(questions);
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
