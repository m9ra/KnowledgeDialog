using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Parsing;

using KnowledgeDialog.DataCollection.MachineActs;

using KnowledgeDialog.DataCollection;

namespace WebBackend.AnswerExtraction
{
    public class AnswerExtractionManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        /// <summary>
        /// Pool with selected questions.
        /// </summary>
        private readonly QuestionCollection _questions;

        private readonly ExtractionKnowledge _knowledge;

        private readonly LinkBasedExtractor _extractor;

        private readonly HashSet<string> _discussedTopics = new HashSet<string>();

        private readonly HashSet<ResponseBase> _discussedQuestions = new HashSet<ResponseBase>();

        private readonly Random _rnd = new Random();

        /// <summary>
        /// Context which is currently used for dialog. Contains actual topic...
        /// </summary>
        private DialogContext _actualContext;

        /// <summary>
        /// Determine whether last input was informative.
        /// </summary>
        public bool HadInformativeInput { get; private set; }

        /// <summary>
        /// Determine whether task based on data collection can be completed.
        /// </summary>
        public bool CanBeCompleted { get; private set; }
        public int SuccessCode { get; private set; }

        internal AnswerExtractionManager(QuestionCollection questions, ExtractionKnowledge knowledge, LinkBasedExtractor extractor)
        {
            _questions = questions;
            _knowledge = knowledge;
            _extractor = extractor;
        }

        /// </inheritdoc>
        public override ResponseBase Initialize()
        {
            _actualContext = createNewContext();

            return formulateOutput(_actualContext.NextMachineOutput);
        }

        private ResponseBase formulateOutput(ResponseBase response)
        {
            //TODO resolve context etc.
            return response;
        }

        /// </inheritdoc>
        public override ResponseBase Input(ParsedUtterance utterance)
        {
            //we require enough informative turns only
            CanBeCompleted = true;
            HadInformativeInput = false;

            if (IsDialogClosed)
                //dialog has been closed - don't say anything
                return null;

            if (_actualContext == null)
                throw new NotImplementedException();

            if (_actualContext.IsComplete)
                _actualContext = createNewContext();

            processContext(_actualContext, utterance);
            HadInformativeInput = _actualContext.HadInformativeInput;

            if (_actualContext.IsComplete)
            {
                _actualContext = createNewContext();
            }
            return _actualContext.NextMachineOutput;
        }

        private DialogContext createNewContext()
        {
            QuestionInfo topic;
            ResponseBase topicQuestion = null;

            if (_actualContext != null)
                //try to stay with topic
                topic = _knowledge.GetInfo(_actualContext.Topic.Utterance.OriginalSentence);
            else
                topic = selectTopic();

            var model = new Models.RuleBasedDenotationModel(_knowledge, _extractor);
            while (topicQuestion == null)
            {
                topicQuestion = selectQuestion(topic, model);
                if (topicQuestion == null)
                    topic = selectTopic();
            }

            _discussedTopics.Add(topic.Utterance.OriginalSentence);
            _discussedQuestions.Add(topicQuestion);
            var context = new DialogContext(topic, topicQuestion, model, Factory);
            context.RegisterNextOutput(decorateTopicQuestion(topicQuestion));
            return context;
        }

        private ResponseBase decorateTopicQuestion(ResponseBase topicQuestion)
        {
            var lastStatus = _actualContext == null ? CompletitionStatus.None : _actualContext.CompletitionStatus;
            var isSampeTopic = _actualContext == null ? false : _actualContext.Topic.Equals(topicQuestion);
            switch (lastStatus)
            {
                case CompletitionStatus.None:
                    return topicQuestion;

                case CompletitionStatus.NotUseful:
                    return new NotUsefulContinuationAct(topicQuestion);

                case CompletitionStatus.Useful:
                    return new UsefulContinuationAct(topicQuestion);

                case CompletitionStatus.NotUnderstandable:
                    return new NotUnderstandableContinuationAct(topicQuestion);

                default:
                    throw new NotImplementedException();
            }
        }

        private QuestionInfo selectTopic()
        {
            var newQuestionProbability = Math.Max(0.1, (100 - _knowledge.QuestionCount) / 100.0);
            string topicCandidate;

            while (true)
            {
                if (_rnd.NextDouble() < newQuestionProbability)
                {
                    do
                    {
                        topicCandidate = _questions.GetRandomQuestion();
                    } while (!isGoodQuestion(topicCandidate));

                    _extractor.Linker.LinkUtterance(topicCandidate); //cache linked information
                    _knowledge.AddQuestion(topicCandidate);
                }
                else
                {
                    topicCandidate = _knowledge.GetRandomQuestion();
                }

                if (!_discussedTopics.Contains(topicCandidate))
                    break;
            }

            return _knowledge.GetInfo(topicCandidate);
        }

        private bool isGoodQuestion(string question)
        {
            var hasCapitalLetter = char.IsUpper(question[0]);
            var hasQuestionMark = question.Last() == '?';
            var hasSimpleCoding = question.All(ch => (int)ch < 255);

            return hasCapitalLetter && hasQuestionMark && hasSimpleCoding;
        }

        private ResponseBase selectQuestion(QuestionInfo topic, IModel model)
        {
            if (topic == null)
                return null;

            return model.PoseQuestions(topic).Except(_discussedQuestions).FirstOrDefault();
        }

        private void processContext(DialogContext context, ParsedUtterance input)
        {
            context.RegisterInput(input);
            context.HandlingModel.UpdateContext(context);
        }
    }
}
