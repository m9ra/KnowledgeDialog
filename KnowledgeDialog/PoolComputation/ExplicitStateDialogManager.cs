using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.StateDialog;
using KnowledgeDialog.PoolComputation.StateDialog.MachineActions;

namespace KnowledgeDialog.PoolComputation
{
    public class ExplicitStateDialogManager : IInputDialogManager
    {
        /// <summary>
        /// Factory providing SLU parses of output.
        /// </summary>
        private readonly SLUFactory _factory = new SLUFactory();

        /// <summary>
        /// Current state of the dialog.
        /// </summary>
        private DialogState _currentState;

        private bool _isInitialized = false;

        private List<MachineActionBase> _machineActions = new List<MachineActionBase>();

        internal ExplicitStateDialogManager(HeuristicQAModule qa)
        {
            _currentState = new DialogState(qa);

            Add<AskEquivalenceDifferenceAction>();
            Add<AcceptEquivalenceAdvice>();            
            Add<EquivalenceQuestionAction>();
            Add<QuestionAnsweringAction>();
            Add<NoAdviceApologizeAction>();
            Add<AcceptAdviceAction>();
            Add<RequestAdviceAction>();
            Add<WelcomeAction>();
            Add<HowCanIHelpYouAction>();
        }

        public ResponseBase Initialize()
        {
            if (_isInitialized)
                throw new InvalidOperationException("Cannot initialize twice");

            //there is no input before initialization
            _isInitialized = true;
            return applyAction();
        }

        public ResponseBase Input(ParsedUtterance utterance)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Cannot take input before initialization");

            var inputAct = _factory.GetBestDialogAct(utterance);
            _currentState = applyInput(inputAct, _currentState);
            //reset expect answer flag after each input.
            _currentState = _currentState.SetExpectAnswer(false);

            return applyAction();
        }

        private ResponseBase applyAction()
        {
            var appliedActions = new HashSet<MachineActionBase>();
            var responses = new List<ResponseBase>();

            for (var i = 0; i < _machineActions.Count; ++i)
            {
                var action = _machineActions[i];

                if (appliedActions.Contains(action))
                    //prevent infinite loop
                    //for now, don't allow same action to process multiple times
                    //TODO better would be checking that same state has been reached
                    continue;

                if (action.CanBeApplied(_currentState))
                {
                    //we have machine action which can made the transition
                    var context = new ProcessingContext();
                    var newState = action.ApplyOn(_currentState, context);
                    appliedActions.Add(action);
                    _currentState = newState;

                    responses.AddRange(context.Responses);

                    if (context.NeedNextMachineAction)
                    {
                        //state needs further processing
                        //therefore we need to scan applicability of machine actions again
                        i = -1;
                        continue;
                    }
                    else
                    {
                        //no more machine action is required
                        break;
                    }
                }
            }

            return new MultiResponse(responses);
        }

        /// <summary>
        /// Apply given input on given state.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="state"></param>
        /// <returns>State after input application.</returns>
        private DialogState applyInput(DialogActBase input, DialogState state)
        {
            var processor = new InputProcessor(state);
            input.Visit(processor);

            return processor.Output;
        }

        private void Add<MachineAction>()
            where MachineAction : MachineActionBase, new()
        {
            var action = new MachineAction();
            _machineActions.Add(action);
        }
    }
}
