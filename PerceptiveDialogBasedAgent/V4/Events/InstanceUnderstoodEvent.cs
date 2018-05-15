using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    internal class InstanceUnderstoodEvent : EventBase
    {
        public InstanceActivationEvent InstanceActivationEvent;

        public InstanceUnderstoodEvent(InstanceActivationEvent instanceActivationEvent)
        {
            InstanceActivationEvent = instanceActivationEvent;
        }

        internal override void Accept(BeamGenerator g)
        {
            g.Visit(this);
        }

        public override string ToString()
        {
            return $"[understood: {InstanceActivationEvent.Instance.Concept.Name}]";
        }
    }
}