using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    abstract class MachineActionBase
    {
        private DialogState _newState;

        private ProcessingContext _context;

        protected DialogState InputState { get; private set; }

        protected abstract bool CouldApply();

        protected abstract void Apply();

        internal bool CanBeApplied(DialogState state)
        {
            try
            {
                InputState = state;
                return CouldApply();
            }
            finally
            {
                InputState = null;
            }
        }

        internal DialogState ApplyOn(DialogState state, ProcessingContext context)
        {
            try
            {
                //initialize state
                InputState = state;
                _newState = InputState;

                //initialize context
                _context = context;

                //apply machine action
                Apply();
                return _newState;
            }
            finally
            {
                InputState = null;
                _newState = null;
            }
        }

        #region Machine action utilities

        protected void EmitResponse(string message)
        {
            _context.Emit(new SimpleResponse(message));
        }

        protected void SetWelcomedFlag(bool value)
        {
            _newState = _newState.WithWelcomedFlag(value);
        }

        protected void ForwardControl()
        {
            _context.ForwardControl();
        }

        protected void RemoveQuestion()
        {
            _newState = _newState.WithQuestion(null);
        }

        protected void RemoveUnknownQuestion()
        {
            _newState = _newState.WithUnknownQuestion(null);
        }

        protected void RemoveConfirmation()
        {
            _newState = _newState.WithConfirm(Confirmation.None);
        }

        protected void RemoveAdvice()
        {
            _newState = _newState.WithAdvice(null);
        }

        protected void SetExpectAnswer(bool expectAnswer) {
            _newState = _newState.SetExpectAnswer(expectAnswer);
        }

        protected void RemoveEquivalenceCandidate()
        {
            _newState = _newState.WithEquivalenceCandidate(null);
        }

        protected void SetEquivalenceCandidate(ParsedUtterance equivalenceCandidate)
        {
            _newState = _newState.WithEquivalenceCandidate(equivalenceCandidate);
        }

        protected void SetQuestionAsUnknown()
        {
            var question = _newState.Question;
            _newState = _newState.WithQuestion(null).WithUnknownQuestion(question);
        }

        protected void SetDifferenceWordQuestion(bool differenceWordQuestioned)
        {
            _newState = _newState.WithDifferenceWordQuestion(differenceWordQuestioned);
        }

        #endregion
    }
}
