using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    abstract class MachineActionBase
    {
        protected DialogState InputState { get; private set; }

        protected bool ActionIsPending { get { throw new NotImplementedException(); } }

        protected abstract bool CouldApply();

        protected abstract void Apply();

        internal DialogState TryApply(DialogState state)
        {
            throw new NotImplementedException();
        }

        #region Machine action utilities

        protected void EmitResponse(string message)
        {
            throw new NotImplementedException();
        }

        protected void SetWelcomedFlag(bool value)
        {
            throw new NotImplementedException();
        }

        protected void MarkActionAsPending()
        {
            throw new NotImplementedException();
        }

        protected void MarkQuestionAnswered()
        {
            throw new NotImplementedException();
        }

        protected void ForwardControl()
        {
            throw new NotImplementedException();
        }

        protected void RemoveQuestion()
        {
            throw new NotImplementedException();
        }

        protected void RemoveUnknownQuestion()
        {
            throw new NotImplementedException();
        }

        protected void RemoveNegation()
        {
            throw new NotImplementedException();
        }
        
        protected void RemoveAdvice()
        {
            throw new NotImplementedException();
        }

        protected void SetEquivalenceHypothesis(ParsedExpression parsedSentence, ParsedExpression equivalenceCandidate)
        {
            throw new NotImplementedException();
        }

        protected void SetQuestionAsUnknown()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
