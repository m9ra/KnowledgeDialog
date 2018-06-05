using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class ComposedPolicyBeamGenerator : AbilityBeamGenerator
    {
        private readonly Random _rnd = new Random();

        private readonly List<PolicyPartBase> _policyParts = new List<PolicyPartBase>();

        private bool _isSubstitutionDisabled = false;

        internal void AddPolicyPart(PolicyPartBase policyPart)
        {
            _policyParts.Insert(0, policyPart);
        }

        private void runPolicy()
        {
            var previousTurnEvents = GetPreviousTurnEvents().ToArray();
            var turnEvents = GetTurnEvents().ToArray();

            foreach (var part in _policyParts)
            {
                var outputs = part.Execute(this, previousTurnEvents, turnEvents);
                if (outputs.Length > 0)
                {
                    pushRandomOutput(outputs);
                    break;
                }
            }
        }

        private void pushRandomOutput(string[] outputs)
        {
            var outputIndex = _rnd.Next(outputs.Length);
            Push(new OutputEvent(outputs[outputIndex]));
        }

        internal override void Visit(TurnEndEvent evt)
        {
            base.Visit(evt);

            _isSubstitutionDisabled = true; //this prevents answering requests/actions before the question was presented to user
            runPolicy();
            _isSubstitutionDisabled = false;
        }

        internal override void Visit(SubstitutionRequestEvent evt)
        {
            if (!_isSubstitutionDisabled)
                base.Visit(evt);
        }
    }
}
