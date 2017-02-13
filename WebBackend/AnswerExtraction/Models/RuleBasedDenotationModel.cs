using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.DataCollection.MachineActs;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction.Models
{
    class RuleBasedDenotationModel : IModel
    {
        private readonly ExtractionKnowledge _knowledge;

        /// <summary>
        /// How many times more evidence has the annotation over other hypothesis to be called correct.
        /// </summary>
        private readonly double _sureFactor = 2.0;

        /// <summary>
        /// How many evidence we need at least for a denotation to be correct.
        /// </summary>
        private readonly int _minimalHintCount = 2;

        /// <summary>
        /// Extractor that is used for answer extraction.
        /// </summary>
        private readonly LinkBasedExtractor _extractor;

        internal RuleBasedDenotationModel(ExtractionKnowledge knowledge)
        {
            _knowledge = knowledge;
        }

        public IEnumerable<MachineActionBase> PoseQuestions(QuestionInfo question)
        {
            var hintsStatistics = getHintsStatistics(question);

            //simple threshold for beiing sure enough about the correct answer
            var topEvidence = hintsStatistics.Values.OrderByDescending(v => v).FirstOrDefault();
            var remainingEvidence = hintsStatistics.Values.Sum();

            if (topEvidence > _minimalHintCount && _sureFactor * remainingEvidence > topEvidence)
                //we are sure about denotation of the question
                return new MachineActionBase[0];

            return new MachineActionBase[]{
                new DirectAnswerHintQuestionAct(question.Utterance)
            };
        }

        public void ParseResponse(DialogContext context, ParsedUtterance response)
        {
            throw new NotImplementedException("parse answer hint");
        }

        private Dictionary<FreebaseEntity, int> getHintsStatistics(QuestionInfo question)
        {
            var statistics = new Dictionary<FreebaseEntity, int>();
            foreach (var answerHint in question.AnswerHints)
            {
                int count;
                var entity = extractAnswer(question, answerHint);
                statistics.TryGetValue(entity, out count);
                count += 1;
                statistics[entity] = count;
            }

            return statistics;
        }

        private FreebaseEntity extractAnswer(QuestionInfo question, string answerHint)
        {
            throw new NotImplementedException();
        }
    }
}
