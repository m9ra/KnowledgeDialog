using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    /// <summary>
    /// Represents value of confirmation.
    /// </summary>
    internal enum Confirmation { None, Affirm, Negate, DontKnow };

    /// <summary>
    /// Represents a state of a dialog. It is immutable.
    /// </summary>
    class DialogState
    {
        /// <summary>
        /// Determine that user has been welcomed.
        /// </summary>
        public bool IsUserWelcomed { get { return getValue(_isUserWelcomed); } }

        /// <summary>
        /// Question answering module that is used by the dialog manager.
        /// </summary>
        public HeuristicQAModule QA { get { return getValue(_qa); } }

        /// <summary>
        /// Available advice.
        /// </summary>
        public ParsedUtterance Advice { get { return getValue(_advice); } }

        /// <summary>
        /// Available question.
        /// </summary>
        public ParsedUtterance Question { get { return getValue(_question); } }

        /// <summary>
        /// Available unknown question.
        /// </summary>
        public ParsedUtterance UnknownQuestion { get { return getValue(_unknownQuestion); } }

        /// <summary>
        /// Possible candidate for equivalence.
        /// </summary>
        public ParsedUtterance EquivalenceCandidate { get { return getValue(_equivalenceCandidate); } }

        /// <summary>
        /// Determine whether question on difference word has been asked.
        /// </summary>
        public bool DifferenceWordQuestioned { get { return getValue(_differenceWordQuestioned); } }

        /// <summary>
        /// Determine whether answer is expected. (Is used for better parsing)
        /// </summary>
        public bool ExpectsAnswer { get { return getValue(_expectsAnswer); } }

        /// <summary>
        /// Determine whether yes, no or don't know has been answered.
        /// </summary>
        public Confirmation ConfirmValue { get { return getValue(_confirmValue); } }

        /// <summary>
        /// Determine whether affirmation is available.
        /// </summary>
        public bool HasAffirmation { get { return ConfirmValue == Confirmation.Affirm; } }

        /// <summary>
        /// Determine whether negation is availble.
        /// </summary>
        public bool HasNegation { get { return ConfirmValue == Confirmation.Negate; } }

        /// <summary>
        /// Determine whether any value for confirmation is available.
        /// </summary>
        public bool HasConfirmation { get { return ConfirmValue != Confirmation.None; } }

        /// <summary>
        /// Determine whether advice is available.
        /// </summary>
        public bool HasAdvice { get { return Advice != null; } }

        /// <summary>
        /// Determine whether there is a question which is not answered yet.
        /// </summary>
        public bool HasNonAnsweredQuestion { get { return Question != null; } }

        /// <summary>
        /// Determine whether candidate for equivalence is available.
        /// </summary>
        public bool HasEquivalenceCandidate { get { return EquivalenceCandidate != null; } }

        /// <summary>
        /// Determine whether unknown question is available.
        /// </summary>
        public bool HasUnknownQuestion { get { return UnknownQuestion != null; } }

        #region Property definitions

        private static readonly StateProperty<bool> _isUserWelcomed;

        private static readonly StateProperty<HeuristicQAModule> _qa;

        private static readonly StateProperty<ParsedUtterance> _advice;

        private static readonly StateProperty<ParsedUtterance> _question;

        private static readonly StateProperty<ParsedUtterance> _unknownQuestion;

        private static readonly StateProperty<ParsedUtterance> _equivalenceCandidate;

        private static readonly StateProperty<bool> _differenceWordQuestioned;

        private static readonly StateProperty<bool> _expectsAnswer;

        private static readonly StateProperty<Confirmation> _confirmValue;

        #endregion

        /// <summary>
        /// Storage for property values.
        /// </summary>
        private readonly Dictionary<object, object> _propertyToValue;

        static DialogState()
        {
            initialize(ref _isUserWelcomed, "IsUserWelcomed");
            initialize(ref _qa, "QA");
            initialize(ref _advice, "Advice");
            initialize(ref _question, "Question");
            initialize(ref _unknownQuestion, "UnknownQuestion");
            initialize(ref _equivalenceCandidate, "EquivalenceCandidate");
            initialize(ref _differenceWordQuestioned, "DifferenceWordQuestioned");
            initialize(ref _expectsAnswer, "ExpectsAnswer");
            initialize(ref _confirmValue, "ConfirmValue");
        }

        internal DialogState(HeuristicQAModule qa)
        {
            _propertyToValue = new Dictionary<object, object>();
            _qa.SetValue(_propertyToValue, qa);
        }

        private DialogState(
            Dictionary<object, object> propertyToValue)
        {
            _propertyToValue = propertyToValue;
        }

        #region State manipulation services

        /// <summary>
        /// Creates new state with given advice.
        /// </summary>
        /// <param name="advice">Advice for new state.</param>
        /// <returns>The new state.</returns>
        internal DialogState WithAdvice(ParsedUtterance advice)
        {
            return newStateWithValue(_advice, advice);
        }

        /// <summary>
        /// Creates new state with given confirm value.
        /// </summary>
        /// <param name="confirmValue">Confirmation value for new state.</param>
        /// <returns>The new state.</returns>
        internal DialogState WithConfirm(Confirmation confirmValue)
        {
            return newStateWithValue(_confirmValue, confirmValue);
        }

        /// <summary>
        /// Creates new state with given unknown question.
        /// </summary>
        /// <param name="unknownQuestion">Unknown question for new state.</param>
        /// <returns>The new state.</returns>
        internal DialogState WithUnknownQuestion(ParsedUtterance unknownQuestion)
        {
            return newStateWithValue(_unknownQuestion, unknownQuestion);
        }

        /// <summary>
        /// Creates new state with given question.
        /// </summary>
        /// <param name="question">Question for new state.</param>
        /// <returns>The new state.</returns>
        internal DialogState WithQuestion(ParsedUtterance question)
        {
            return newStateWithValue(_question, question);
        }

        /// <summary>
        /// Creates new state with given welcome flag.
        /// </summary>
        /// <param name="isUserWelcomed">Flag for new state.</param>
        /// <returns>The new state.</returns>
        internal DialogState WithWelcomedFlag(bool isUserWelcomed)
        {
            return newStateWithValue(_isUserWelcomed, isUserWelcomed);
        }

        /// <summary>
        /// Creates new state with given equivalence candidate.
        /// </summary>
        /// <param name="equivalenceCandidate">Equivalence candidate for new state.</param>
        /// <returns>The new state.</returns>
        internal DialogState WithEquivalenceCandidate(ParsedUtterance equivalenceCandidate)
        {
            return newStateWithValue(_equivalenceCandidate, equivalenceCandidate);
        }

        /// <summary>
        /// Creates new state with given difference word questioned flag.
        /// </summary>
        /// <param name="differenceWordQuestioned">Flag for new state.</param>
        /// <returns>The new state.</returns>
        internal DialogState WithDifferenceWordQuestion(bool differenceWordQuestioned)
        {
            return newStateWithValue(_differenceWordQuestioned, differenceWordQuestioned);
        }

        /// <summary>
        /// Creates new state with given expect answer flag.
        /// </summary>
        /// <param name="isExpected">Flag for new state.</param>
        /// <returns>The new state.</returns>
        internal DialogState SetExpectAnswer(bool isExpected)
        {
            return newStateWithValue(_expectsAnswer, isExpected);
        }

        #endregion

        #region Property handling utilities

        /// <summary>
        /// Creates a new state which is based on current state and has given value for given property.
        /// </summary>
        /// <typeparam name="PropertyType">Type of value represented by property.</typeparam>
        /// <param name="property">Property which value will be set.</param>
        /// <param name="value">Value which will be set to the property.</param>
        /// <returns>The new dialog state.</returns>
        private DialogState newStateWithValue<PropertyType>(StateProperty<PropertyType> property, PropertyType value)
        {
            var propertyToValueCopy = new Dictionary<object, object>(_propertyToValue);

            property.SetValue(propertyToValueCopy, value);

            return new DialogState(propertyToValueCopy);
        }

        /// <summary>
        /// Gets value that is stored by given property.
        /// </summary>
        /// <typeparam name="PropertyType">Type of value represented by property.</typeparam>
        /// <param name="property">Property which value is requested.</param>
        /// <returns>Value of the property if available <c>default</c> otherwise.</returns>
        private PropertyType getValue<PropertyType>(StateProperty<PropertyType> property)
        {
            return property.GetValue(_propertyToValue);
        }

        /// <summary>
        /// Initialize given property reference with new property of given name.
        /// </summary>
        /// <typeparam name="PropertyType">Type of value represented by property.</typeparam>
        /// <param name="property">Initialized property.</param>
        /// <param name="propertyName">Name of initialized property.</param>
        private static void initialize<PropertyType>(ref StateProperty<PropertyType> property, string propertyName)
        {
            if (property != null)
                throw new NotSupportedException("Cannot initialize same property twice");

            property = new StateProperty<PropertyType>(propertyName);
        }

        #endregion
    }
}
