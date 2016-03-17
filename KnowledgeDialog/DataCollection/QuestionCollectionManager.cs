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
    public class QuestionCollectionManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        /// <summary>
        /// Words that has been detected for question rephrases.
        /// </summary>
        private HashSet<string> _questionRephraseWords = new HashSet<string>();

        /// <summary>
        /// Pool with selected questions.
        /// </summary>
        private readonly QuestionCollection _questions;

        /// <summary>
        /// Actually discussed question.
        /// </summary>
        private ParsedUtterance _actualQuestion = null;

        /// <summary>
        /// Determine whether collection is in rephrase request phase.
        /// </summary>
        private bool _isRephrasePhase = false;

        /// <summary>
        /// Determine whether last input was informative.
        /// </summary>
        public bool HadInformativeInput { get; private set; }

        /// <summary>
        /// Determine whether task based on data collection can be completed.
        /// </summary>
        public bool CanBeCompleted { get; private set; }

        public QuestionCollectionManager(QuestionCollection questions)
        {
            _questions = questions;
            _actualQuestion = getNextQuestion();
            _isRephrasePhase = true;
        }

        /// <inheritdoc/>
        public override ResponseBase Initialize()
        {
            return new WelcomeWithRephraseRequestAct(_actualQuestion);
        }

        /// <inheritdoc/>
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

            //dialog state collection
            var isExpectingDenotation = !_isRephrasePhase;
            var isExpectingAnswer = isExpectingDenotation && LastDenotationQuestion == DenotationType.CorrectAnswer;
            var isExpectingExplanation = isExpectingDenotation && LastDenotationQuestion == DenotationType.Explanation;
            var nonAskedDenotationType = GetNonaskedDenotationType();

            //dialog handling
            if (isChitChat)
                return HandleChitChat(utteranceAct as ChitChatAct);

            if (_isRephrasePhase && (hasNegation || isDontKnow))
            {
                //USER DONT WANT TO GIVE THE REPHRASE
                _isRephrasePhase = false;
                isExpectingDenotation = true;
            }

            if (hasAffirmation)
            {
                //USER CONFIRMS WILLINGNESS TO PROVIDE INFORMATION

                return new ContinueAct();
            }
            else if (_isRephrasePhase)
            {
                var rephraseInformativeWords = getInformativeWords(utterance);
                var questionInformativeWords = getInformativeWords(_actualQuestion);

                if (rephraseInformativeWords.Count() < questionInformativeWords.Count() - 1)
                    //rephrase is too short
                    return new TooBriefRephraseAct();

                if (Enumerable.SequenceEqual(lower(utterance.Words), lower(_actualQuestion.Words)))
                    //rephrase is exactly same to the question
                    return new TooBriefRephraseAct();

                _questionRephraseWords.UnionWith(lower(utterance.Words));
                _isRephrasePhase = false;
                HadInformativeInput = true;
            }
            else if (isExpectingDenotation && (hasNegation || isDontKnow))
            {
                if (nonAskedDenotationType == DenotationType.None)
                {
                    return proposeNewQuestion(true);
                }

                return AskAtLeast(nonAskedDenotationType);
            }
            else if (isExpectingAnswer)
            {
                if (utterance.Words.Count() < 3)
                    return new TooBriefAnswerAct();

                var answerInformativeWords = getInformativeWords(utterance);
                var questionInformativeWords = getInformativeWords(_actualQuestion);

                var questionBinding = answerInformativeWords.Intersect(questionInformativeWords.Union(_questionRephraseWords));

                if (questionBinding.Count() < 1)
                    return new TooBriefAnswerAct();

                //ACTUALLY WE DONT HAVE VALIDATION FOR THIS
                HadInformativeInput = true;
            }
            else if (isExpectingExplanation)
            {
                var explanationLength = utterance.Words.Count();
                if (explanationLength < 5)
                    return new TooBriefExplanationAct();

                var utteranceInformativeWords = getInformativeWords(utterance);
                var questionInformativeWords = getInformativeWords(_actualQuestion);
                var newInformativeWords = utteranceInformativeWords.Except(questionInformativeWords).Except(_questionRephraseWords);

                if (newInformativeWords.Count() < 3)
                    return new UnwantedRephraseDetected();
            }

            LastDenotationQuestion = DenotationType.None;
            if (nonAskedDenotationType != DenotationType.None)
            {
                HadInformativeInput = true;
                //we don't have an answer - try to ask
                return Ask(nonAskedDenotationType);
            }

            //we have already asked everything
            return proposeNewQuestion(false);
        }

        private ParsedUtterance getNextQuestion()
        {
            return UtteranceParser.Parse(_questions.GetRandomQuestion());
        }

        private ResponseBase proposeNewQuestion(bool atLeast)
        {
            _actualQuestion = getNextQuestion();
            _isRephrasePhase = true;
            _questionRephraseWords.Clear();

            CanBeCompleted = true;
            AskedDenotations.Clear();
            LastDenotationQuestion = DenotationType.None;

            return new RephraseQuestionProposeAct(_actualQuestion, atLeast);
        }

        private IEnumerable<string> getInformativeWords(ParsedUtterance utterance)
        {
            return lower(utterance.Words.Except(NonInformativeWords).Distinct());
        }

        private IEnumerable<string> lower(IEnumerable<string> words)
        {
            return words.Select(w => w.ToLowerInvariant()).ToArray();
        }
    }
}
