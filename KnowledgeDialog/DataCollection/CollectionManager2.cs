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
    public class CollectionManager2 : CollectionManagerBase
    {
        /// <summary>
        /// Actually discussed question.
        /// </summary>
        private ParsedUtterance _actualQuestion = null;

        private readonly QuestionCollection _questions;

        public CollectionManager2(QuestionCollection questions)
        {
            _questions = questions;
            _actualQuestion = getNextQuestion();
        }

        /// <inheritdoc/>
        public override ResponseBase Initialize()
        {
            return new WelcomeAct();
        }

        /// <inheritdoc/>
        public override ResponseBase Input(ParsedUtterance utterance)
        {
            if (IsDialogClosed)
                //dialog has been closed - don't say anything
                return null;

            //input processing
            var utteranceAct = Factory.GetBestDialogAct(utterance);
            var hasNegation = utteranceAct is NegateAct;
            var hasAffirmation = utteranceAct is AffirmAct;
            var isChitChat = utteranceAct is ChitChatAct;
            var questionOnInput = utterance.OriginalSentence.Contains("?") || utteranceAct is QuestionAct;

            //dialog state collection
            var isExpectingDenotation = true;
            var isExpectingAnswer = isExpectingDenotation && LastDenotationQuestion == DenotationType.CorrectAnswer;
            var isExpectingExplanation = isExpectingDenotation && LastDenotationQuestion == DenotationType.Explanation;
            var nonAskedDenotationType = GetNonaskedDenotationType();

            //dialog handling
            if (isChitChat)
                return HandleChitChat(utteranceAct as ChitChatAct);

            if (isExpectingDenotation && hasAffirmation)
            {
                //USER CONFIRMS WILLINGNESS TO PROVIDE DENOTATION

                return new ContinueAct();
            }
            else if (isExpectingDenotation && hasNegation)
            {
                if (nonAskedDenotationType == DenotationType.None)
                {
                    IsDialogClosed = true;
                    _actualQuestion = getNextQuestion();
                    throw new NotImplementedException("Propose new question.");
                }

                return AskAtLeast(nonAskedDenotationType);
            }
            else if (isExpectingAnswer)
            {
                throw new NotImplementedException();
            }
            else if (isExpectingExplanation)
            {
                var explanationLength = utterance.Words.Count();
                if (explanationLength < 5)
                    return new TooBriefAct();

                var diffWords = utterance.Words.Except(_actualQuestion.Words).Except(NonExplainingWords).ToArray();
                if (diffWords.Length < 3)
                    return new UnwantedRephraseDetected();
            }

            //reset last denotation
            LastDenotationQuestion = DenotationType.None;

            //we don't have an answer - try to ask
            if (nonAskedDenotationType != DenotationType.None)
                return Ask(nonAskedDenotationType);

            //we have already asked everything
            IsDialogClosed = true;
            return new ByeAct();
        }

        private ParsedUtterance getNextQuestion()
        {
            return UtteranceParser.Parse(_questions.GetRandomQuestion());
        }
    }
}
