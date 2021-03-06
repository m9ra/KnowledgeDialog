﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;

namespace PerceptiveDialogBasedAgent.V4.Abilities
{
    class ItReferenceResolver : ConceptAbilityBase
    {
        private readonly Concept2 _parameter = Concept2.Subject;

        internal ItReferenceResolver()
            : base("it")
        {
            Description("its");
            Description("one");
        }

        protected override void onInstanceActivated(ConceptInstance instance, BeamGenerator generator)
        {
            var activations = generator.GetInstanceActivations();

            foreach (var relevantInstanceActivation in activations)
            {
                if (!relevantInstanceActivation.CanBeReferenced)
                    continue;

                var relevantInstance = relevantInstanceActivation.Instance;
                if (relevantInstance == instance)
                    //dont reference self
                    continue;

                //try tunnel instances between turns
                generator.Push(new StaticScoreEvent(0.05));
                generator.Push(new InstanceReferencedEvent(relevantInstance));
                generator.Pop();
                generator.Pop();
            }
        }
    }
}
