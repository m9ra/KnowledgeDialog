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
            var definedConcepts = new HashSet<Concept2>(GetDefinedConcepts());

            foreach (var part in _policyParts)
            {
                var outputs = part.Execute(this, previousTurnEvents, turnEvents, definedConcepts);
                if (outputs.Length > 0)
                {
                    Push(new PolicyTagEvent(part.LastTag));
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

        protected override void handleOnActiveSubstitution(InstanceActiveEvent evt)
        {
            if (!_isSubstitutionDisabled)
                base.handleOnActiveSubstitution(evt);
        }

        internal override void Visit(IncompleteRelationEvent evt)
        {
            if (!_isSubstitutionDisabled)
                base.Visit(evt);
        }
    }
}
