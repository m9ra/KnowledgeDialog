using PerceptiveDialogBasedAgent.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    abstract class PointingModelBase
    {
        internal abstract IEnumerable<RankedPointing> GenerateMappings(BodyState2 state);

        internal abstract IEnumerable<RankedPointing> GetForwardings(ConceptInstance forwardedConcept, BodyState2 state);

        internal abstract BodyState2 AddSubstitution(BodyState2 state, ConceptInstance container, Concept2 parameter, ConceptInstance value);

        internal abstract string StateReaction(BodyState2 state, out BodyState2 finalState);

        internal abstract void OnConceptChange();

        protected void LogState(BodyState2 state)
        {
            Log.Indent();
            foreach (var input in state.InputPhrases)
            {
                Log.Writeln(input.ToString(), Log.SensorColor);
                Log.Indent();
                var inputTarget = GetTargetRepresentation(input, state);

                Log.Writeln(inputTarget, Log.ItemColor);
                Log.Dedent();
            }
            Log.Dedent();
            Log.Writeln();
        }

        protected string GetTargetRepresentation(PointableInstance source, BodyState2 state)
        {
            var rankedPointing = state.GetRankedPointing(source);
            if (rankedPointing == null)
                return "unknown";

            var strRepresentation = rankedPointing.Target.ToString() + $"~{rankedPointing.Rank:0.00}";
            var forwardedPointing = state.GetRankedPointing(rankedPointing.Target);
            if (forwardedPointing != null)
                strRepresentation += " --> " + GetTargetRepresentation(rankedPointing.Target, state);

            return strRepresentation;
        }
    }
}
