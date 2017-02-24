using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Parsing;
using KnowledgeDialog.Knowledge;
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

        private readonly Random _rnd = new Random();

        internal RuleBasedDenotationModel(ExtractionKnowledge knowledge, LinkBasedExtractor extractor)
        {
            _knowledge = knowledge;
            _extractor = extractor;

            if (_knowledge == null)
                throw new NullReferenceException("knowledge");

            if (_extractor == null)
                throw new NullReferenceException("extractor");
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
                context.CompletitionStatus = CompletitionStatus.NotUnderstandable;
                return null;
            }
            else
            {
                // USER PROVIDED ANSWER
                var utterance = context.UserInput;
                if (utterance.Words.Count() <=1)
                    return new TooBriefAnswerAct();

                var question = context.Topic.Utterance.OriginalSentence;
                var answerHint = utterance.OriginalSentence;

                var questionEntities = getEntities(question);
                var answerHintEntities = getEntities(answerHint, question);

                var questionEntityNames = getEntityNames(questionEntities).ToArray();
                var answerHintEntityNames = getEntityNames(answerHintEntities).ToArray();

                var questionBinding = questionEntities.Intersect(answerHintEntities).ToArray();
                var questionBindingNames = questionEntityNames.Intersect(answerHintEntityNames).ToArray();

                if (questionBindingNames.Count() < 1)
                    return new NoConnectionToEntityAct(selectNoConnectionEntity(questionEntities));

                context.HadInformativeInput = true;

                _knowledge.AddAnswerHint(context.Topic, utterance);
                context.CompletitionStatus = CompletitionStatus.Useful;
                return null;
            }
        }

        private EntityInfo selectNoConnectionEntity(EntityInfo[] entities)
        {
            var entityIndex = _rnd.Next(entities.Length);
            return entities[entityIndex];
        }

        private IEnumerable<string> getEntityNames(IEnumerable<EntityInfo> entities)
        {
            return entities.SelectMany(e => _extractor.Db.GetNamesFromMid(e.Mid));
        }

        private EntityInfo[] getEntities(string utterance, string contextUtterance = null)
        {
            var linked = link(utterance, contextUtterance);
            if (linked == null)
                return Enumerable.Empty<EntityInfo>().ToArray();

            Console.WriteLine(linked);
            return linked.Entities.ToArray();
        }

        private LinkedUtterance link(string utterance, string contextUtterance = null)
        {
            var linker = _extractor.Linker;

            var contextEntities = new List<EntityInfo>();
            if (contextUtterance != null)
            {
                var linkedContext = linker.LinkUtterance(contextUtterance);
                if (linkedContext != null)
                    contextEntities.AddRange(linkedContext.Entities);
            }

            return _extractor.Linker.LinkUtterance(utterance, contextEntities);
        }

        private Dictionary<EntityInfo, int> getHintsStatistics(QuestionInfo question)
        {
            var statistics = new Dictionary<EntityInfo, int>();
            foreach (var answerHint in question.AnswerHints)
            {
                int count;
                var entity = extractAnswer(question, answerHint.OriginalSentence);
                if (entity == null)
                    continue;

                statistics.TryGetValue(entity, out count);
                count += 1;
                statistics[entity] = count;
            }

            return statistics;
        }

        private EntityInfo extractAnswer(QuestionInfo question, string answerHint)
        {
            return _extractor.ExtractAnswerEntity(question.Utterance.OriginalSentence, answerHint).FirstOrDefault();
        }
    }
}
