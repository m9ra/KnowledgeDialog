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
        private readonly List<ParsedUtterance> _answerHints = new List<ParsedUtterance>();

        private readonly List<ParsedUtterance> _typeHints = new List<ParsedUtterance>();

        internal readonly ParsedUtterance Utterance;

        internal IEnumerable<ParsedUtterance> AnswerHints { get { return _answerHints; } }

        internal QuestionInfo(ParsedUtterance utterance)
        {
            Utterance = utterance;
        }

        private QuestionInfo(ParsedUtterance utterance, IEnumerable<ParsedUtterance> answerHints, IEnumerable<ParsedUtterance> typeHints)
        {
            Utterance = utterance;
            _answerHints.AddRange(answerHints);
            _typeHints.AddRange(typeHints);
        }

        internal QuestionInfo WithAnswerHint(ParsedUtterance answerHint)
        {
            return new QuestionInfo(answerHint, _answerHints.Concat(new [] { answerHint }), _typeHints);
        }
    }
}
