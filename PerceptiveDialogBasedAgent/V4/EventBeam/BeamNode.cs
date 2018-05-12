using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class BeamNode
    {
        internal readonly BeamNode ParentNode;

        internal readonly EventBase Evt;

        public BeamNode()
        {
        }

        public BeamNode(BeamNode parentNode, EventBase evt)
        {
            ParentNode = parentNode;
            Evt = evt;
        }
    }
}
