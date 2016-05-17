using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    class InputProcessor : IActVisitor
    {
        protected readonly DialogState InputState;

        internal DialogState Output { get; private set; }

        internal InputProcessor(DialogState inputState)
        {
            InputState = inputState;
            Output = inputState;
        }

        public void Visit(AdviceAct adviceAct)
        {
            SetAdvice(adviceAct.Answer);
        }

        public void Visit(AffirmAct confirmAct)
        {
            SetConfirm(Confirmation.Affirm);
        }

        public void Visit(ThinkAct thinkAct)
        {
            throw new NotSupportedException("Think act is not supported");
        }

        public void Visit(NegateAct negateAct)
        {
            SetConfirm(Confirmation.Negate);
        }

        public void Visit(ExplicitAdviceAct explicitAdviceAct)
        {
            SetUnknownQuestion(explicitAdviceAct.Question);
            SetAdvice(explicitAdviceAct.Answer);
        }

        public void Visit(QuestionAct questionAct)
        {
            SetQuestion(questionAct.Question);
        }

        public void Visit(UnrecognizedAct unrecognizedAct)
        {
            if (InputState.ExpectsAnswer && unrecognizedAct.Utterance.Words.Count()==1)
            {
                //we got single entity so it is probably an advice
                SetAdvice(unrecognizedAct.Utterance);
                return;
            }

            SetQuestion(unrecognizedAct.Utterance);
        }

        public void Visit(ChitChatAct chitChat)
        {
            throw new NotImplementedException();
        }

        public void Visit(DontKnowAct dontKnow)
        {
            SetConfirm(Confirmation.DontKnow);
        }

        #region State handling

        protected void SetAdvice(ParsedUtterance advice)
        {
            Output = Output.WithAdvice(advice);
        }

        protected void SetConfirm(Confirmation confirmValue)
        {
            Output = Output.WithConfirm(confirmValue);
        }

        protected void SetUnknownQuestion(ParsedUtterance question)
        {
            Output = Output.WithUnknownQuestion(question);
        }

        protected void SetQuestion(ParsedUtterance question)
        {
            Output = Output.WithQuestion(question);
        }
        #endregion
    }
}
