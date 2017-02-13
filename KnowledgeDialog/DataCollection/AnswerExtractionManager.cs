using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Parsing;

using KnowledgeDialog.DataCollection.MachineActs;

namespace KnowledgeDialog.DataCollection
{
    public class AnswerExtractionManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        /// <summary>
        /// Determine whether manager expects new answer.
        /// </summary>
        private bool _expectsAnswer;

        /// <summary>
        /// Pool with selected questions.
        /// </summary>
        private readonly QuestionCollection _questions;

        /// <summary>
        /// Actually discussed question.
        /// </summary>
        private ParsedUtterance _actualQuestion = null;

        /// <summary>
        /// Determine whether last input was informative.
        /// </summary>
        public bool HadInformativeInput { get; private set; }

        /// <summary>
        /// Determine whether task based on data collection can be completed.
        /// </summary>
        public bool CanBeCompleted { get; private set; }


        public AnswerExtractionManager(QuestionCollection questions)
        {
            _questions = questions;
            _actualQuestion = getNextQuestion();
        }

        /// </inheritdoc>
        public override ResponseBase Initialize()
        {
            _expectsAnswer = true;
            return new WelcomeWithAnswerRequestAct(_actualQuestion);
        }

        /// </inheritdoc>
        public override ResponseBase Input(ParsedUtterance utterance)
        {
            CanBeCompleted = false;
            HadInformativeInput = false;
            if (IsDialogClosed)
                //dialog has been closed - don't say anything
                return null;

            //input processing
            var utteranceAct = Factory.GetBestDialogAct(utterance);

            var hasNegation = utteranceAct is NegateAct;
            var hasAffirmation = utteranceAct is AffirmAct;
            var isChitChat = utteranceAct is ChitChatAct;
            var isDontKnow = utteranceAct is DontKnowAct;
            var questionOnInput = utterance.OriginalSentence.Contains("?") || utteranceAct is QuestionAct;

            if (_expectsAnswer && (isDontKnow || hasNegation))
            {
                // USER DOES NOT KNOW THE ANSWER
                _actualQuestion = getNextQuestion();
                return Ask(DenotationType.CorrectAnswer);
            }
            else if (_expectsAnswer && hasAffirmation)
            {
                // USER IS SUGGESTING THAT HE KNOWS THE ANSWER
                return new ContinueAct();
            }
            else if (_expectsAnswer && questionOnInput)
            {
                // USER IS ASKING A QUESTION
                throw new NotImplementedException();
            }
            else if (_expectsAnswer)
            {
                // USER PROVIDED ANSWER
                return analyzeAnswer(utterance);
            }
            else if (!_expectsAnswer)
            {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        private ResponseBase analyzeAnswer(ParsedUtterance utterance)
        {
            throw new NotImplementedException();
        }

        private ParsedUtterance getNextQuestion()
        {
            return UtteranceParser.Parse(_questions.GetRandomQuestion());
        }
    }
}
