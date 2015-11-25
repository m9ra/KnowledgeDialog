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
    enum DenotationType { None = -1, Explanation = 0, CorrectAnswer = 1 }

    public class CollectionManager : IInputDialogManager
    {
        /// <summary>
        /// Factory providing SLU parses of output.
        /// </summary>
        private readonly SLUFactory _factory = new SLUFactory();

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
        /// Keep track about denotations that has been asked.
        /// </summary>
        private HashSet<DenotationType> _askedDenotations = new HashSet<DenotationType>();

        /// <summary>
        /// Denotation which has been asked lastly.
        /// </summary>
        private DenotationType _lastDenotationQuestion = DenotationType.None;

        private string[] _nonExplainingWords = new[]
        {
            "of","a","the","whether","what","how","who","which","when","why"
        };

        /// <summary>
        /// Determine whether dialog has been closed.
        /// </summary>
        private bool _isDialogClosed = false;

        #endregion

        public CollectionManager(ComposedGraph graph)
        {
            _graph = graph;
        }

        public ResponseBase Initialize()
        {
            return new WelcomeAct();
        }

        public ResponseBase Input(ParsedUtterance utterance)
        {
            if (_isDialogClosed)
                //dialog has been closed - don't say anything
                return null;

            //input processing
            var utteranceAct = _factory.GetBestDialogAct(utterance);
            var hasNegation = utteranceAct is NegateAct;
            var hasAffirmation = utteranceAct is AffirmAct;
            var isChitChat = utteranceAct is ChitChatAct;
            var questionOnInput = utterance.OriginalSentence.Contains("?") || utteranceAct is QuestionAct;

            //dialog state collection
            var isQuestionRegistered = _reqisteredQuestion != null;
            var isExpectingDenotation = isQuestionRegistered;
            var isExpectingAnswer = isExpectingDenotation && _lastDenotationQuestion == DenotationType.CorrectAnswer;
            var isExpectingExplanation = isExpectingDenotation && _lastDenotationQuestion == DenotationType.Explanation;
            var nonAskedDenotationType = getNonaskedDenotationType();

            //dialog handling
            if (isChitChat)
                return handleChitChat(utteranceAct as ChitChatAct);

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
                _askedDenotations.Clear();
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

                return askAtLeast(nonAskedDenotationType);
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
                    return askForMissingFact();
            }
            else if (isExpectingExplanation)
            {
                var explanationLength = utterance.Words.Count();
                if (explanationLength < 5)
                    return new TooBriefAct();

                var diffWords = utterance.Words.Except(_reqisteredQuestion.Words).Except(_nonExplainingWords).ToArray();
                if (diffWords.Length < 3)
                    return new UnwantedRephraseDetected();
            }

            //reset last denotation
            _lastDenotationQuestion = DenotationType.None;

            //we don't have an answer - try to ask
            if (nonAskedDenotationType != DenotationType.None)
                return ask(nonAskedDenotationType);

            //we have already asked everything
            _isDialogClosed = true;
            return new ByeAct();
        }

        private ResponseBase ask(DenotationType denotationType)
        {
            return ask(denotationType, false);
        }

        private ResponseBase askAtLeast(DenotationType denotationType)
        {
            return ask(denotationType, true);
        }

        private ResponseBase askForMissingFact()
        {
            return new BeMoreSpecificAct();
        }

        private ResponseBase ask(DenotationType denotationType, bool atLeastRequest)
        {
            _askedDenotations.Add(denotationType);
            _lastDenotationQuestion = denotationType;
            switch (denotationType)
            {
                case DenotationType.CorrectAnswer:
                    return new RequestQuestionAsnwerAct(atLeastRequest);

                case DenotationType.Explanation:
                    return new RequestExplanationAct(atLeastRequest);
            }

            throw new NotSupportedException(denotationType.ToString());
        }

        private ResponseBase handleChitChat(ChitChatAct act)
        {
            var domain = act.Domain;
            if (domain == ChitChatDomain.Bye)
            {
                //ending dialog is actually not simple chit chat
                _isDialogClosed = true;
                return new ByeAct();
            }

            return new ChitChatAnswerAct(domain);
        }

        private DenotationType getNonaskedDenotationType()
        {
            foreach (DenotationType denotationType in new[] { DenotationType.Explanation, DenotationType.CorrectAnswer })
            {
                if (!_askedDenotations.Contains(denotationType))
                    //we have denotation that could be asked
                    return denotationType;
            }

            return DenotationType.None;
        }
    }
}
