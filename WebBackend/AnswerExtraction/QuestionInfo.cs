using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace WebBackend.AnswerExtraction
{
    class QuestionInfo
    {
        private readonly List<string> _answerHints = new List<string>();

        private readonly List<string> _typeHints = new List<string>();

        internal readonly ParsedUtterance Utterance;

        internal readonly IEnumerable<string> AnswerHints;
         
    }
}
