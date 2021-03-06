﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class AssignUnknownProperty : ConceptAbilityBase
    {
        internal AssignUnknownProperty() : base(Concept2.AssignUnknownProperty.Name, fireConceptDefinedEvt: false)
        {
            AddParameter(Concept2.Subject);
            AddParameter(Concept2.Target);
        }

        internal static ConceptInstance Create(ConceptInstance instanceToRefine, string unknown, BeamGenerator generator)
        {
            var assignUnknownProperty = new ConceptInstance(Concept2.AssignUnknownProperty);
            var unknownPropertyCandidate = new ConceptInstance(Concept2.From(unknown));
            generator.SetValue(assignUnknownProperty, Concept2.Subject, unknownPropertyCandidate);
            generator.SetValue(assignUnknownProperty, Concept2.Target, instanceToRefine);
            return assignUnknownProperty;
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var subject = generator.GetValue(instance, Concept2.Subject);
            var target = generator.GetValue(instance, Concept2.Target);

            var relevantProperties = getRelevantProperties(target, generator);
            if (!relevantProperties.Any())
            {
                generator.Push(new StaticScoreEvent(-0.5));
                return;
            }

            var disambiguation = new ConceptInstance(Concept2.PropertyValueDisambiguation);
            generator.SetValue(disambiguation, Concept2.Unknown, subject);
            generator.SetValue(disambiguation, Concept2.Target, target);
            foreach (var pair in relevantProperties)
            {
                var propertyInstance = new ConceptInstance(pair.Key);
                foreach (var value in pair.Value)
                {
                    var valueInstance = new ConceptInstance(value);
                    generator.SetValue(valueInstance, Concept2.Property, propertyInstance);
                    generator.SetValue(disambiguation, Concept2.Subject, valueInstance);
                }
            }

            generator.Push(new InstanceActivationRequestEvent(disambiguation));
        }

        private Dictionary<Concept2, HashSet<Concept2>> getRelevantProperties(ConceptInstance targetInstance, BeamGenerator generator)
        {
            var properties = new Dictionary<Concept2, HashSet<Concept2>>();
            var definedConcepts = new HashSet<Concept2>(generator.GetDefinedConcepts());
            foreach (var conceptCandidate in definedConcepts)
            {
                var conceptCandidateInstance = new ConceptInstance(conceptCandidate);

                var instanceOf = generator.GetValue(conceptCandidateInstance, Concept2.InstanceOf);
                if (conceptCandidate != targetInstance.Concept && instanceOf?.Concept != targetInstance.Concept)
                    continue;

                var instanceProperties = generator.GetPropertyValues(conceptCandidateInstance);
                foreach (var instanceProperty in instanceProperties)
                {
                    if (!definedConcepts.Contains(instanceProperty.Value.Concept))
                        continue;

                    if (!definedConcepts.Contains(instanceProperty.Key))
                        continue;

                    if (!properties.TryGetValue(instanceProperty.Key, out var propertyValues))
                        properties[instanceProperty.Key] = propertyValues = new HashSet<Concept2>();

                    propertyValues.Add(instanceProperty.Value.Concept);
                }
            }

            return properties;
        }
    }
}
