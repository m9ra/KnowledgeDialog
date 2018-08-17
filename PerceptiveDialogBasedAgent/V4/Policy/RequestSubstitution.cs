using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    class RequestSubstitution : PolicyPartBase
    {
        protected override IEnumerable<string> execute(BeamGenerator generator)
        {
            var request = Get<InformationPartEvent>(p => !p.IsFilled);
            if (request == null || request.Subject == null)
                yield break;

            var instanceConcept = request.Subject?.Concept;
            if (instanceConcept == Concept2.What)
            {
                var subject = generator.GetValue(request.Subject, Concept2.Subject);
                var property = generator.GetValue(request.Subject, Concept2.Property);

                generator.Push(request);


                if (subject == null && property != null)
                {
                    yield return $"{singular(property)} of what ?";
                }
                else if (property == null && subject != null)
                {
                    yield return $"What are you interested in about {singular(subject)} ?";
                }
                else
                {
                    yield return "What are you interested in?";

                }

            }
            else
            {
                generator.Push(request);
                yield return "What should I " + singular(request.Subject) + " ?";
            }
        }
    }
}
