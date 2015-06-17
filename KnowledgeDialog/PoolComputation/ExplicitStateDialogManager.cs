using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;

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
        private DialogState _currentState = new DialogState();

        private List<MachineActionBase> _machineActions = new List<MachineActionBase>();

        internal ExplicitStateDialogManager()
        {
            Add<QuestionAnsweringAction>();
            Add<NoAdviceApologizeAction>();
            Add<AcceptAdviceAction>();
            Add<RequestAdviceAction>();
            Add<WelcomeAction>();
            Add<HowCanIHelpYouAction>();
        }

        public ResponseBase Input(ParsedExpression utterance)
        {
            var inputAct = _factory.GetDialogAct(utterance);
            _currentState = applyInput(inputAct, _currentState);

            throw new NotImplementedException();
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
