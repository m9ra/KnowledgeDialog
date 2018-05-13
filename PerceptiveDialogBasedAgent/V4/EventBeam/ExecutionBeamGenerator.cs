using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    internal delegate void BeamExecutionCallback(ConceptInstance action, ExecutionBeamGenerator generator);

    class ExecutionBeamGenerator : BeamGenerator
    {
        private readonly Dictionary<Concept2, BeamExecutionCallback> _conceptCallbacks = new Dictionary<Concept2, BeamExecutionCallback>();

        internal ExecutionBeamGenerator()
        {
            PushToAll(new ConceptDefinedEvent(Concept2.ActionToExecute));
            PushToAll(new ParamDefinedEvent(Concept2.ActionToExecute, Concept2.Subject, new ConceptInstance(Concept2.Something)));
            PushToAll(new InstanceActivationEvent(null, new ConceptInstance(Concept2.ActionToExecute)));
        }

        internal void AddCallback(Concept2 concept, BeamExecutionCallback callback)
        {
            _conceptCallbacks.Add(concept, callback);
        }

        internal override void Visit(CompleteInstanceEvent evt)
        {
            if (evt.Instance.Concept != Concept2.ActionToExecute)
            {
                base.Visit(evt);
                return;
            }

            //we have got action to execute
            var value = GetValue(evt.Instance, Concept2.Subject);
            _conceptCallbacks.TryGetValue(value.Concept, out var executor);

            if (executor == null)
            {
                //executor is not defined
                base.Visit(evt);
                return;
            }

            Push(new StaticScoreEvent(0.05));
            executor(value, this);
        }
    }
}
