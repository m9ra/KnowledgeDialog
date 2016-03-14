using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog;
using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog.Acts;

using KnowledgeDialog.DataCollection.MachineActs;

namespace KnowledgeDialog.DataCollection
{
    public enum DenotationType { None = -1, Explanation = 0, CorrectAnswer = 1 }

    public class CollectionManager : CollectionManagerBase
    {      
        /// <summary>
        /// Graph that represents system database.
        /// </summary>
        private readonly ComposedGraph _graph;

        #region Dialog state members

        /// <summary>
        /// Determine whether question has been registered from user.
        /// </summary>
        private ParsedUtterance _reqisteredQuestion = null;

        /// <summary>
        /// Determine whether dialog has been closed.
        /// </summary>
        private bool _isDialogClosed = false;

        #endregion

        public CollectionManager(ComposedGraph graph)
        {
            _graph = graph;
        }

        /// <inheritdoc/>
        public override ResponseBase Initialize()
        {
            return new WelcomeAct();
        }

        /// <inheritdoc/>
        public override ResponseBase Input(ParsedUtterance utterance)
        {
            if (_isDialogClosed)
                //dialog has been closed - don't say anything
                return null;

            //input processing
            var utteranceAct = Factory.GetBestDialogAct(utterance);
            var hasNegation = utteranceAct is NegateAct;
            var hasAffirmation = utteranceAct is AffirmAct;
            var isChitChat = utteranceAct is ChitChatAct;
            var questionOnInput = utterance.OriginalSentence.Contains("?") || utteranceAct is QuestionAct;

            //dialog state collection
            var isQuestionRegistered = _reqisteredQuestion != null;
            var isExpectingDenotation = isQuestionRegistered;
            var isExpectingAnswer = isExpectingDenotation && LastDenotationQuestion == DenotationType.CorrectAnswer;
            var isExpectingExplanation = isExpectingDenotation && LastDenotationQuestion == DenotationType.Explanation;
            var nonAskedDenotationType = GetNonaskedDenotationType();

            //dialog handling
            if (isChitChat)
                return HandleChitChat(utteranceAct as ChitChatAct);

            if (isQuestionRegistered && questionOnInput)
            {
                //QUESTION OVERRIDING

                //TODO: how to distinguish between asking different question or explaining by a question?
            }
            else if (!isQuestionRegistered && questionOnInput)
            {
                //FIRST QUESTION REGISTRATION

                //prepare question answering
                _reqisteredQuestion = utterance;
                AskedDenotations.Clear();
            }
            if (!isQuestionRegistered && hasAffirmation)
            {
                //AFFIRMATION AT BEGINING

                return new ContinueAct();
            }
            else if (!isQuestionRegistered && !questionOnInput)
            {
                //UNRECOGNIZED UTTERANCE WHEN QUESTION EXPECTED

                return new DontUnderstandAct();
            }
            else if (isExpectingDenotation && hasAffirmation)
            {
                //USER CONFIRMS WILLINGNESS TO PROVIDE DENOTATION

                return new ContinueAct();
            }
            else if (isExpectingDenotation && hasNegation)
            {
                if (nonAskedDenotationType == DenotationType.None)
                {
                    _isDialogClosed = true;
                    return new IncompleteByeAct();
                }

                return AskAtLeast(nonAskedDenotationType);
            }
            else if (isExpectingAnswer)
            {
                var hasDatabaseEvidence = false;
                //search whether answer contains some entity from knowledge base
                foreach (var word in utterance.Words)
                {
                    if (_graph.HasEvidence(word))
                        hasDatabaseEvidence = true;
                }

                if (!hasDatabaseEvidence)
                    return AskForMissingFact();
            }
            else if (isExpectingExplanation)
            {
                var explanationLength = utterance.Words.Count();
                if (explanationLength < 5)
                    return new TooBriefExplanationAct();

                var diffWords = utterance.Words.Except(_reqisteredQuestion.Words).Except(NonExplainingWords).ToArray();
                if (diffWords.Length < 3)
                    return new UnwantedRephraseDetected();
            }

            //reset last denotation
            LastDenotationQuestion = DenotationType.None;

            //we don't have an answer - try to ask
            if (nonAskedDenotationType != DenotationType.None)
                return Ask(nonAskedDenotationType);

            //we have already asked everything
            _isDialogClosed = true;
            return new ByeAct();
        }

    }
}
