using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    class DialogState
    {
        public readonly bool IsUserWelcomed;

        public readonly QuestionAnsweringModule QA;

        public readonly ParsedExpression Advice;

        public readonly ParsedExpression Question;

        public readonly ParsedExpression UnknownQuestion;

        public readonly ParsedExpression EquivalenceCandidate;

        public readonly bool? ConfirmValue;

        public bool HasAffirmation { get { return ConfirmValue.HasValue && ConfirmValue.Value; } }

        public bool HasConfirmation { get { return ConfirmValue.HasValue; } }

        public bool HasAdvice { get { return Advice != null; } }

        public bool HasNegation { get { return ConfirmValue.HasValue && !ConfirmValue.Value; } }

        public bool HasNonAnsweredQuestion { get { return Question != null; } }

        public bool HasEquivalenceCandidate { get { return EquivalenceCandidate != null; } }

        public bool HasUnknownQuestion { get { return UnknownQuestion != null; } }

        internal DialogState(QuestionAnsweringModule qa)
        {
            QA = qa;
        }

        private DialogState(
            DialogState previous, bool isUserWelcomed, ParsedExpression advice,
            ParsedExpression question, ParsedExpression unknownQuestion,
            ParsedExpression equivalenceCandidate,
            bool? confirmValue)
        {
            QA = previous.QA;

            IsUserWelcomed = isUserWelcomed;
            Advice = advice;
            Question = question;
            UnknownQuestion = unknownQuestion;
            EquivalenceCandidate = equivalenceCandidate;
            ConfirmValue = confirmValue;
        }

        internal DialogState WithAdvice(ParsedExpression advice)
        {
            return new DialogState(this, IsUserWelcomed, advice, Question, UnknownQuestion, EquivalenceCandidate, ConfirmValue);
        }

        internal DialogState WithConfirm(bool? confirmValue)
        {
            return new DialogState(this, IsUserWelcomed, Advice, Question, UnknownQuestion, EquivalenceCandidate, confirmValue);
        }

        internal DialogState WithUnknownQuestion(ParsedExpression unknownQuestion)
        {
            return new DialogState(this, IsUserWelcomed, Advice, Question, unknownQuestion, EquivalenceCandidate, ConfirmValue);
        }

        internal DialogState WithQuestion(ParsedExpression question)
        {
            return new DialogState(this, IsUserWelcomed, Advice, question, UnknownQuestion, EquivalenceCandidate, ConfirmValue);
        }

        internal DialogState WithWelcomedFlag(bool isUserWelcomed)
        {
            return new DialogState(this, isUserWelcomed, Advice, Question, UnknownQuestion, EquivalenceCandidate, ConfirmValue);
        }

        internal DialogState WithEquivalenceCandidate(ParsedExpression equivalenceCandidate)
        {
            return new DialogState(this, IsUserWelcomed, Advice, Question, UnknownQuestion, equivalenceCandidate, ConfirmValue);
        }
    }
}
