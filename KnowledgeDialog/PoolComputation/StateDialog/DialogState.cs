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

        public readonly bool? ConfirmValue;

        public bool HasAdvice { get { return Advice != null; } }

        public bool HasNegation { get { return ConfirmValue.HasValue && ConfirmValue.Value; } }

        public bool HasNonAnsweredQuestion { get { return Question != null; } }

        public bool HasUnknownQuestion { get { return UnknownQuestion != null; } }

        internal DialogState(QuestionAnsweringModule qa)
        {
            QA = qa;
        }

        private DialogState(DialogState previous, bool isUserWelcomed, ParsedExpression advice, ParsedExpression question, ParsedExpression unknownQuestion, bool? confirmValue)
        {
            QA = previous.QA;

            IsUserWelcomed = isUserWelcomed;
            Advice = advice;
            Question = question;
            UnknownQuestion = unknownQuestion;
            ConfirmValue = confirmValue;
        }

        internal DialogState WithAdvice(ParsedExpression advice)
        {
            return new DialogState(this, IsUserWelcomed, advice, Question, UnknownQuestion, ConfirmValue);
        }

        internal DialogState WithConfirm(bool confirmValue)
        {
            return new DialogState(this, IsUserWelcomed, Advice, Question, UnknownQuestion, ConfirmValue);
        }

        internal DialogState WithUnknownQuestion(ParsedExpression unknownQuestion)
        {
            return new DialogState(this, IsUserWelcomed, Advice, Question, unknownQuestion, ConfirmValue);
        }

        internal DialogState WithQuestion(ParsedExpression question)
        {
            return new DialogState(this, IsUserWelcomed, Advice, question, UnknownQuestion, ConfirmValue);
        }

        internal DialogState WithWelcomedFlag(bool isUserWelcomed)
        {
            return new DialogState(this, isUserWelcomed, Advice, Question, UnknownQuestion, ConfirmValue);
        }
    }
}
