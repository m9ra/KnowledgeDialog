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
            SetConfirm(true);
        }

        public void Visit(ThinkAct thinkAct)
        {
            throw new NotSupportedException("Think act is not supported");
        }

        public void Visit(NegateAct negateAct)
        {
            SetConfirm(false);
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
            SetQuestion(unrecognizedAct.Utterance);
        }

        public void Visit(ChitChatAct chitChat)
        {
            throw new NotImplementedException();
        }

        public void Visit(DontKnowAct dontKnow)
        {
            throw new NotImplementedException();
        }
        
        #region State handling

        protected void SetAdvice(ParsedExpression advice)
        {
            Output = Output.WithAdvice(advice);
        }

        protected void SetConfirm(bool confirmValue)
        {
            Output = Output.WithConfirm(confirmValue);
        }

        protected void SetUnknownQuestion(ParsedExpression question)
        {
            Output = Output.WithUnknownQuestion(question);
        }

        protected void SetQuestion(ParsedExpression question)
        {
            Output = Output.WithQuestion(question);
        }
        #endregion
    }
}
