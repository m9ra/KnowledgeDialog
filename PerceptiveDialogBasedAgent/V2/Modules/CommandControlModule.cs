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

        private readonly ModuleField<string> _inputLiteralField;

        private readonly ModuleField<string> _evaluationField;

        internal CommandControlModule(Body body)
        {
            _body = body;
            _inputLiteralField = newField<string>("InputLiteral", _param_inputLiteral);
            _evaluationField = newField<string>("Evaluation", _param_evaluation);
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
                .CallAction(_ability_repeatLastCommand)

            .AddAnswer("the last command failed", Question.IsItTrue)
                .Call(_value_lastCommandFailed)

            .AddAnswer("user said $something", Question.IsItTrue)
                .Param(_evaluationField, "$something")
                .Param(_inputLiteralField, "$something")
                .Call(_value_userSaidSomething)
            ;
        }

        private SemanticItem _param_evaluation(ModuleContext context)
        {
            var evaluation = context.GetAnswer(Question.HowToEvaluate, _body.InputHistory.Last());

            return evaluation;
        }

        private SemanticItem _param_inputLiteral(ModuleContext context)
        {
            return SemanticItem.Entity(context.Input);
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

        private SemanticItem _value_userSaidSomething(string userInputEvaluation, string something)
        {
            return SemanticItem.Convert(userInputEvaluation.Contains(something));
        }
    }
}
