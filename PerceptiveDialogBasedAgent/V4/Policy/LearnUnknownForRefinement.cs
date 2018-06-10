using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class LearnUnknownForRefinement : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var evt = Get<InformationReportEvent>();
            if (evt?.Instance.Concept != Concept2.NeedsRefinement)
                yield break;

            var instanceToRefine = generator.GetValue(evt.Instance, Concept2.Subject);
            var activationTarget = generator.GetValue(evt.Instance, Concept2.Target);

            var prefixesForRefine = generator.GetPrefixingUnknowns(instanceToRefine);

            if (!prefixesForRefine.TryGetValue(instanceToRefine, out var propertyUnknown))
                //nothing to ask for
                yield break;

            /* 
              //TODO feature based questions
            
              //ACTION SEQUENCE
              //Do you want X which is UNKNOWN ?    * Prompt
              //-yes: what property UNKNOWN is ?    *   yes -> expect (properties)
              //-- expect property                  *
              //-no: So which X you want ?          *   no -> ?
              //--standard refinement               *

              yield return $"I know many {plural(instanceToRefine)} which one would you like?";*/

            var prompt = new ConceptInstance(Concept2.Prompt);

            var assignUnknownProperty = new ConceptInstance(Concept2.AssignUnknownProperty);
            var unknownPropertyCandidate = new ConceptInstance(Concept2.From(propertyUnknown));
            generator.SetValue(assignUnknownProperty, Concept2.Subject, unknownPropertyCandidate);
            generator.SetValue(assignUnknownProperty, Concept2.Target, instanceToRefine);

            generator.SetValue(prompt, Concept2.Yes, assignUnknownProperty);
            //TODO refine event should be retriggred
            generator.SetValue(prompt, Concept2.No, instanceToRefine);
            generator.Push(new InstanceActivationRequestEvent(null, prompt));
            yield return $"So, you would like to find {plural(instanceToRefine)} which are {propertyUnknown}?";
        }
    }
}
