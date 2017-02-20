using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;
using KnowledgeDialog.Knowledge;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class ExtractionKnowledge
    {
        private Dictionary<string, QuestionInfo> _knowledge = new Dictionary<string, QuestionInfo>();

        internal int QuestionCount { get { return _knowledge.Count; } }

        internal IEnumerable<QuestionInfo> Knowledge { get { return _knowledge.Values; } }

        internal QuestionInfo GetInfo(string question)
        {
            QuestionInfo result;
            _knowledge.TryGetValue(question, out result);

            return result;
        }

        internal void AddQuestion(string question)
        {
            if (_knowledge.ContainsKey(question))
                return;

            _knowledge[question] = new QuestionInfo(UtteranceParser.Parse(question));
        }

        internal void AddAnswerHint(QuestionInfo questionInfo, ParsedUtterance utterance)
        {
            //TODO thread safe
            var newInfo = questionInfo.WithAnswerHint(utterance);
            _knowledge[questionInfo.Utterance.OriginalSentence] = newInfo;
        }

        internal string GetRandomQuestion()
        {
            throw new NotImplementedException();
        }
    }
}
