using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog;
using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Responses;

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
        /// Mapping of denotation type to questions.
        /// </summary>
        private Dictionary<DenotationType, string> _questionDefinitions = new Dictionary<DenotationType, string> { 
            {DenotationType.Explanation,"I don’t know meaning of your question. Can you explain it to me?"},
            {DenotationType.CorrectAnswer,"I’m still not getting the idea. Can you give me the correct answer for your question?"}
        };

        private Dictionary<DenotationType, string> _atLeastQuestionDefinitions = new Dictionary<DenotationType, string>
        {
             {DenotationType.Explanation,"No problem, can you explain the question in detail instead?"},
            {DenotationType.CorrectAnswer,"That's ok, can you give me the correct answer for your question instead?"}
        };


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
        /// Determine whether dialog has been closed.
        /// </summary>
        private bool _isDialogClosed = false;

        #endregion

        public ResponseBase Initialize()
        {
            return new SimpleResponse("Hello, how can I help you?");
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
            var nonAskedDenotationType = getNonaskedDenotationType();

            //dialog handling
            if (isChitChat)
                return handleChitChat(utteranceAct as ChitChatAct);

            if (isQuestionRegistered && questionOnInput)
            {
                //QUESTION OVERRIDING

                //user is asking for another question?
                _reqisteredQuestion = utterance;

                //we don't know anything about denotations for new question
                _askedDenotations.Clear();
            }
            else if (!isQuestionRegistered && questionOnInput)
            {
                //FIRST QUESTION REGISTRATION

                //prepare question answering
                _reqisteredQuestion = utterance;
                _askedDenotations.Clear();
            }
            else if (!isQuestionRegistered && !questionOnInput)
            {
                //UNRECOGNIZED UTTERANCE WHEN QUESTION EXPECTED

                return new SimpleResponse("I'm sorry, but I can't understand you. Can you ask me by different words?");
            }
            else if (isExpectingDenotation && hasAffirmation)
            {
                //USER CONFIRMS WILLINGNESS TO PROVIDE DENOTATION

                return new SimpleResponse("That's great, let tell it to me.");
            }
            else if (isExpectingDenotation && hasNegation)
            {
                if (nonAskedDenotationType == DenotationType.None)
                {
                    _isDialogClosed = true;
                    return new SimpleResponse("Ok. Thank you anyway. Bye.");
                }

                return askAtLeast(nonAskedDenotationType);
            }

            //we don't have an answer - try to ask
            if (nonAskedDenotationType != DenotationType.None)
                return ask(nonAskedDenotationType);

            //we have already asked everything
            _isDialogClosed = true;
            return new SimpleResponse("Thank you for your help, bye.");
        }

        private ResponseBase ask(DenotationType denotationType)
        {
            _askedDenotations.Add(denotationType);
            return new SimpleResponse(_questionDefinitions[denotationType]);
        }

        private ResponseBase askAtLeast(DenotationType denotationType)
        {
            _askedDenotations.Add(denotationType);
            return new SimpleResponse(_atLeastQuestionDefinitions[denotationType]);
        }

        private ResponseBase handleChitChat(ChitChatAct act)
        {
            switch (act.Domain)
            {
                case ChitChatDomain.Welcome:
                    return new SimpleResponse("Can you ask me some question please?");

                case ChitChatDomain.Bye:
                    _isDialogClosed = true;
                    return new SimpleResponse("Bye.");

                case ChitChatDomain.Polite:
                case ChitChatDomain.Personal:
                    return new SimpleResponse("I can't talk about my personality, lets return to the question.");

                case ChitChatDomain.Rude:
                    return new SimpleResponse("I'm sorry for disappointing you, unfortunatelly we should return to the question.");
            }

            return new SimpleResponse("I'm sorry, but I don't understand");
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
