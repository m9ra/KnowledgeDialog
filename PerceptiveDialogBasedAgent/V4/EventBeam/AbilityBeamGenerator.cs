using PerceptiveDialogBasedAgent.V4.Abilities;
using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    internal delegate void BeamExecutionCallback(ConceptInstance action, BeamGenerator generator);

    class AbilityBeamGenerator : BeamGenerator
    {
        private readonly Dictionary<Concept2, BeamExecutionCallback> _conceptCallbacks = new Dictionary<Concept2, BeamExecutionCallback>();

        internal void RegisterAbility(AbilityBase ability)
        {
            ability.Register(this);
        }

        internal void AddCallback(Concept2 concept, BeamExecutionCallback callback)
        {
            _conceptCallbacks.Add(concept, callback);
        }

        internal override void Visit(InstanceActiveEvent evt)
        {
            var completeInstance = evt.Instance;
            _conceptCallbacks.TryGetValue(completeInstance.Concept, out var executor);
            if (executor == null)
            {
                //executor is not defined
                base.Visit(evt);
            }
            else
            {
                //we have got action to execute
                Push(new CloseEvent(evt));
                Push(new StaticScoreEvent(0.05));
                executor(completeInstance, this);
            }
        }
    }
}
