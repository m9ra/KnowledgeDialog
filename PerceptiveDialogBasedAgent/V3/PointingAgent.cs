using PerceptiveDialogBasedAgent.V3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    class PointingAgent
    {
        internal readonly Body Body = new Body(new SimpleMatchModel());

        private readonly StrategyModelBase _strategy;

        internal PointingAgent()
        {
            _strategy = new HandcraftedStrategy(Body);

            Body
                .Concept("yes", _nativeValue)
                    .Description("positive answer to a question")

                .Concept("no", _nativeValue)
                    .Description("negative answer to a question")

                .Concept("print", _print)
                    .Description("it is an action")
                    .Description("alias to say")
                ;

        }

        internal string Input(string utterance)
        {
            V2.Log.DialogUtterance("U: " + utterance);

            var bypassedState = _strategy.InputProcessingBypass(Body.GetBestState(), utterance);
            if (bypassedState == null)
            {
                var words = utterance.Split(' ');
                foreach (var word in words)
                {
                    Body.AddInput(word);
                }
            }
            else
            {
                Body.SetState(bypassedState);
            }

            var state = Body.GetBestState();
            if (state.GetValue("output") == null)
                state = _strategy.NoOutput(state);

            var outputValue = state.GetValue("output");

            V2.Log.DialogUtterance("S: " + outputValue);

            state = _strategy.AfterReadout(state);
            Body.SetState(state);

            return outputValue;
        }

        private void _print(BodyContext context)
        {
            if (!context.RequireParameter("What should be printed?", out var subject))
                return;

            context.SetValue("output", subject.Name);
        }

        private void _databaseSearch(BodyContext context)
        {
            if (!context.RequireParameter("Which database should I search in?", out var database, context.Databases))
                return;

            var allCriterions = context.GetCriterions(database);
            if (!context.RequireMultiParameter("Which criterions should be used for the database search?", out var selectedCriterions, allCriterions))
                return;

            throw new NotImplementedException("Add the real search as a callback to context");
        }

        private void _nativeValue(BodyContext context)
        {
            //native values does not need to process context anyhow
        }
    }
}
