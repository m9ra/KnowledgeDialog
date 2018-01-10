using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2.Modules
{
    class CommandControlModule : BodyModuleBase
    {
        private readonly Body _body;

        private string _lastFailedCommand;

        internal CommandControlModule(Body body)
        {
            _body = body;
        }

        internal void ReportCurrentCommandFail()
        {
            if (_lastFailedCommand != null)
                return;

            _lastFailedCommand = _body.InputHistory.Last();
        }

        protected override void initializeAbilities()
        {
            this
            .AddAbility("repeat the last command")
                .Call((Action)_ability_repeatLastCommand)

            .AddAnswer("the last command failed", Question.IsItTrue)
                .Call(_value_lastCommandFailed)
            ;
        }

        private void _ability_repeatLastCommand()
        {
            _body.ClearOutput();
            _body.ExecuteCommand(_lastFailedCommand);
        }

        private SemanticItem _value_lastCommandFailed()
        {
            return _lastFailedCommand == null ? SemanticItem.No : SemanticItem.Yes;
        }
    }
}
