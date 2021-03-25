using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class PropertyValueDisambiguation : ConceptAbilityBase
    {
        internal PropertyValueDisambiguation() : base(Concept2.PropertyValueDisambiguation.Name, fireConceptDefinedEvt: false)
        {
            AddParameter(Concept2.Subject);
            AddParameter(Concept2.Unknown);
            AddParameter(Concept2.Answer);
            AddParameter(Concept2.Target);
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var subjects = generator.GetValues(instance, Concept2.Subject).ToArray();

            var answer = generator.GetValue(instance, Concept2.Answer);
            var unknown = generator.GetValue(instance, Concept2.Unknown);
            var target = generator.GetValue(instance, Concept2.Target);

            var directSubjects = filterDirectSubjects(subjects, answer);
            var indirectSubjects = filterIndirectSubjects(subjects, answer, generator);

            if (answer.Concept == Concept2.Nothing)
            {
                // user does not like any option 
                return;
            }

            if (directSubjects.Count() == 1)
            {
                pushSetSubject(target, directSubjects.First(), generator, unknown);
                return;
            }

            if (indirectSubjects.Count() == 1)
            {
                pushSetSubject(target, indirectSubjects.First(), generator, unknown);
                return;
            }

            if (indirectSubjects.Count() == 0)
            {
                generator.Push(new InstanceOutputEvent(new ConceptInstance(Concept2.DisambiguationFailed)));
                return;
            }

            if (indirectSubjects.Count() < subjects.Count())
            {
                //create new disambiguation
                generator.Push(new StaticScoreEvent(0.1));
                pushNewDisambiguation(indirectSubjects, unknown, target, generator);
                return;
            }

            if (indirectSubjects.Count() == subjects.Count())
            {
                var knowledgeConfirmation = new ConceptInstance(Concept2.DisambiguatedKnowledgeConfirmed);
                generator.SetValue(knowledgeConfirmation, Concept2.Subject, answer);
                generator.SetValue(knowledgeConfirmation, Concept2.Target, instance);
                generator.Push(new StaticScoreEvent(0.05));
                generator.Push(new InstanceOutputEvent(knowledgeConfirmation));
                return;
            }

            //disambiguation was not helpful
            generator.Push(new StaticScoreEvent(-1.0));
        }

        private void pushSetSubject(ConceptInstance target, ConceptInstance resolvedSubject, BeamGenerator generator, ConceptInstance unknown)
        {
            RememberConceptDescription.Activate(resolvedSubject.Concept, unknown.Concept.Name, generator);

            var relevantProperty = generator.GetValue(resolvedSubject, Concept2.Property);
            generator.Push(new StaticScoreEvent(0.1));
            generator.SetValue(target, relevantProperty.Concept, resolvedSubject);
        }

        private void pushNewDisambiguation(IEnumerable<ConceptInstance> indirectSubjects, ConceptInstance unknown, ConceptInstance target, BeamGenerator generator)
        {
            var disambiguation = new ConceptInstance(Concept2.PropertyValueDisambiguation);
            generator.SetValue(disambiguation, Concept2.Unknown, unknown);
            generator.SetValue(disambiguation, Concept2.Target, target);
            foreach (var subject in indirectSubjects)
            {
                generator.SetValue(disambiguation, Concept2.Subject, subject);
            }

            generator.Push(new InstanceActivationRequestEvent(disambiguation));
        }

        private IEnumerable<ConceptInstance> filterIndirectSubjects(ConceptInstance[] subjects, ConceptInstance answer, BeamGenerator generator)
        {
            var result = new List<ConceptInstance>();
            foreach (var subject in subjects)
            {
                var propertyInfo = generator.GetValue(subject, Concept2.Property);
                if (propertyInfo.Concept == answer.Concept)
                    result.Add(subject);
            }

            return result;
        }

        private IEnumerable<ConceptInstance> filterDirectSubjects(ConceptInstance[] subjects, ConceptInstance answer)
        {
            return subjects.Where(s => s.Concept == answer.Concept).ToArray();
        }
    }
}
