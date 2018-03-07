using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3.Models
{
    class HandcraftedStrategy : StrategyModelBase
    {
        private readonly Body _body;

        internal HandcraftedStrategy(Body body)
        {
            _body = body;
        }

        internal override BodyState InputProcessingBypass(BodyState state, string input)
        {
            var targetConcept = state.GetValue("targetConcept");
            if (targetConcept == null)
                return null;

            _body
                .Concept(targetConcept, null)
                    .Description(input);

            var newConcept = _body.GetConcept(targetConcept);

            state = state.AddNewPhrase(input);
            state = state
                .SetPointer(state.LastInputPhrase, new RankedConcept(newConcept, 1.0))
                .SetValue("targetConcept", null)
                .SetValue("output", "Ok, thank you.");

            return state;
        }

        internal override BodyState AfterReadout(BodyState state)
        {
            //remove output after readout
            var newState = state.SetValue("output", null);
            return newState;
        }

        internal override BodyState NoOutput(BodyState state)
        {
            var unknownInput = state.Input.Reverse().Take(10).Where(i => state.GetConcept(i) == null).First();
            if (unknownInput != null)
            {
                var questionState = state
                    .SetValue("output", "What is '" + unknownInput + "'?")
                    .SetValue("targetConcept", unknownInput.ToString());

                return questionState;
            }

            return state;
        }
    }
}
