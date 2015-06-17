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

        public readonly ParsedExpression Advice;

        public readonly ParsedExpression Question;

        public readonly ParsedExpression UnknownQuestion;

        public readonly bool? ConfirmValue;

        public bool HasAdvice { get { return Advice != null; } }

        public bool HasNegation { get { return ConfirmValue.HasValue && ConfirmValue.Value; } }

        public bool HasNonAnsweredQuestion { get { throw new NotImplementedException(); } }
        
        public bool HasUnknownQuestion { get { return UnknownQuestion != null; } }

        internal DialogState()
        {
        }

        private DialogState(bool isUserWelcomed, ParsedExpression advice, ParsedExpression question, ParsedExpression unknownQuestion, bool? confirmValue)
        {
            IsUserWelcomed = isUserWelcomed;
            Advice = advice;
            Question = question;
            UnknownQuestion = unknownQuestion;
            ConfirmValue = confirmValue;
        }

        internal DialogState WithAdvice(ParsedExpression advice)
        {
            return new DialogState(IsUserWelcomed, advice, Question, UnknownQuestion, ConfirmValue);
        }

        internal DialogState WithConfirm(bool confirmValue)
        {
            return new DialogState(IsUserWelcomed, Advice, Question, UnknownQuestion, ConfirmValue);
        }

        internal DialogState WithUnknownQuestion(ParsedExpression unknownQuestion)
        {
            return new DialogState(IsUserWelcomed, Advice, Question, unknownQuestion, ConfirmValue);
        }

        internal DialogState WithQuestion(ParsedExpression question)
        {
            return new DialogState(IsUserWelcomed, Advice, question, UnknownQuestion, ConfirmValue);
        }
    }
}
