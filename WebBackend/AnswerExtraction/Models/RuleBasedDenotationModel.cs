using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.DataCollection;
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

            if (_knowledge == null)
                throw new NullReferenceException();
        }

        public IEnumerable<ResponseBase> PoseQuestions(QuestionInfo question)
        {
            var hintsStatistics = getHintsStatistics(question);

            //simple threshold for beiing sure enough about the correct answer
            var topEvidence = hintsStatistics.Values.OrderByDescending(v => v).FirstOrDefault();
            var remainingEvidence = hintsStatistics.Values.Sum();

            if (topEvidence > _minimalHintCount && _sureFactor * remainingEvidence > topEvidence)
                //we are sure about denotation of the question
                return new ResponseBase[0];

            return new ResponseBase[]{
                new DirectAnswerHintQuestionAct(question.Utterance)
            };
        }

        public void UpdateContext(DialogContext context)
        {
            var nextOutput = createNextOutput(context);
            context.RegisterNextOutput(nextOutput);
        }

        private MachineActionBase createNextOutput(DialogContext context)
        {
            //input processing
            var utteranceAct = context.BestUserInputAct;

            if (context.IsDontKnow || context.HasNegation)
            {
                // USER DOES NOT KNOW THE ANSWER
                context.CompletitionStatus = CompletitionStatus.NotUseful;
                return null;
            }
            else if (context.HasAffirmation)
            {
                // USER IS SUGGESTING THAT HE KNOWS THE ANSWER
                return new ContinueAct();
            }
            else if (context.IsQuestionOnInput)
            {
                // USER IS ASKING A QUESTION
                throw new NotImplementedException();
            }
            else
            {
                // USER PROVIDED ANSWER
                var utterance = context.UserInput;
                if (utterance.Words.Count() < 3)
                    return new TooBriefAnswerAct();

                var answerInformativeWords = QuestionCollectionManager.GetInformativeWords(utterance);
                var questionInformativeWords = QuestionCollectionManager.GetInformativeWords(context.Topic.Utterance);

                var questionBinding = answerInformativeWords.Intersect(questionInformativeWords);

                if (questionBinding.Count() < 1)
                    return new TooBriefAnswerAct();

                context.HadInformativeInput = true;

                _knowledge.AddAnswerHint(context.Topic, utterance);
                context.CompletitionStatus = CompletitionStatus.Useful;
                return null;
            }
        }

        private Dictionary<FreebaseEntity, int> getHintsStatistics(QuestionInfo question)
        {
            var statistics = new Dictionary<FreebaseEntity, int>();
            foreach (var answerHint in question.AnswerHints)
            {
                int count;
                var entity = extractAnswer(question, answerHint.OriginalSentence);
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
