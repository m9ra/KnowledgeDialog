using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Models
{
    delegate IEnumerable<string> OutputGenerator();

    delegate bool ConditionEvaluator();

    class EventBasedNLG
    {
        private readonly List<PatternBasedGenerator> _generators = new List<PatternBasedGenerator>();

        private readonly Random _rnd = new Random();

        private EventBase[] _processedEvents;

        private BeamNode _processedNode;

        private int _currentEventIndex = 0;

        private PatternBasedGenerator _currentGenerator = null;

        internal EventBase CurrentEvent => _processedEvents[_currentEventIndex];

        internal EventBasedNLG()
        {
            this
                .ForEvent<InstanceFoundEvent>()
                    .Output(simpleOutput)

                .ForEvent<UnknownPhraseSubstitutionEvent>()
                    .Output(unknownPhraseSubstitution)

                .ForEvent<SubstitutionRequestEvent>()
                    .Output(needsSubstitution)

                .ForEvent<TooManyInstancesFoundEvent>()
                    .Output(refinement)

                .ForEvent<NoInstanceFoundEvent>()
                    .Output(notFound)

            ;
        }

        private IEnumerable<string> notFound()
        {
            var evt = CurrentEvent as NoInstanceFoundEvent;
            var constraintRepresentation = evt.Criterion.ToPrintable();

            yield return "I don't know anything which is " + constraintRepresentation;
        }

        private IEnumerable<string> simpleOutput()
        {
            var evt = CurrentEvent as InstanceFoundEvent;
            var outputRepresentation = evt.Instance.ToPrintable();

            outputRepresentation += ".";
            if (hasFindAction())
            {
                yield return "I know " + outputRepresentation;
                yield return "I think, you would like " + outputRepresentation;
            }

            yield return "It is " + outputRepresentation;
        }

        private IEnumerable<string> needsSubstitution()
        {
            var evt = CurrentEvent as SubstitutionRequestEvent;
            var value = getPropertyValue(evt.Target.Instance, evt.Target.Property);
            if (value != null)
                //request is not opened
                yield break;

            var questionFormulation = getPropertyQuestion(evt.Target.Property);
            var subject = evt.Target.Instance.Concept.Name;

            yield return questionFormulation + " " + subject;
        }

        private string getPropertyQuestion(Concept2 property)
        {
            if (property == Concept2.Something || property == Concept2.Subject)
            {
                return "What should I";
            }
            else
            {
                return $"What {property.Name} should I";
            }
        }

        private IEnumerable<string> unknownPhraseSubstitution()
        {
            var evt = CurrentEvent as UnknownPhraseSubstitutionEvent;
            var unknownPhrase = evt.UnknownPhrase.InputPhraseEvt.Phrase;

            if (unknownPhrase != null)
                yield return "I don't know phrase " + unknownPhrase + ". What does it mean?";
        }

        private IEnumerable<string> refinement()
        {
            var evt = CurrentEvent as TooManyInstancesFoundEvent;

            var ambiguousConstraint = evt.Criterion;
            var constraintSpecification = getConstraintDescription(ambiguousConstraint);
            constraintSpecification = getPlural(constraintSpecification);

            yield return "I know many " + constraintSpecification + ", which one would you like?";
        }

        private string getConstraintDescription(ConceptInstance conceptInstance)
        {
            if (conceptInstance is null || conceptInstance.Concept != Concept2.Something)
                return conceptInstance.ToPrintable();

            var description = getPropertyValue(conceptInstance, Concept2.InstanceOf)?.ToPrintable();
            if (description == null)
                description = "thing";

            //collect modifiers
            foreach (var property in getProperties(conceptInstance))
            {
                if (property == Concept2.InstanceOf)
                    continue;

                var modifier = getPropertyValue(conceptInstance, property).ToPrintable();
                description = modifier + " " + description;
            }

            return description;
        }

        private ConceptInstance getPropertyValue(ConceptInstance instance, Concept2 property)
        {
            return BeamGenerator.GetValue(instance, property, _processedNode);
        }

        private IEnumerable<Concept2> getProperties(PointableInstance instance)
        {
            throw new NotImplementedException();
        }

        private string getPlural(PointableInstance instance)
        {
            var printable = instance.ToPrintable();
            return getPlural(printable);
        }

        private static string getPlural(string printable)
        {
            var endChar = printable.Last();

            var esLetters = new[] { 's' };
            if (esLetters.Contains(endChar))
                return printable + "es";

            return printable + "s";
        }

        private bool isPropertyValue(PointableInstance instance)
        {
            var conceptInstance = instance as ConceptInstance;
            if (conceptInstance == null)
                return false;

            throw new NotImplementedException();

            return false;
        }

        private bool isProperty(PointableInstance instance)
        {
            var conceptInstance = instance as ConceptInstance;
            if (conceptInstance == null)
                return false;

            throw new NotImplementedException();
        }

        private ConceptInstance getProperty(ConceptInstance instance, Concept2 property)
        {
            return BeamGenerator.GetValue(instance, property, _processedNode);
        }

        private bool evtCondition<T>()
            where T : EventBase
        {
            return _processedEvents[_currentEventIndex] is T;
        }

        private EventBasedNLG Output(OutputGenerator generator)
        {
            _currentGenerator.Generator = generator;
            return this;
        }

        private EventBasedNLG ForEvent<T>()
            where T : EventBase
        {
            _currentGenerator = new PatternBasedGenerator();
            _generators.Add(_currentGenerator);

            _currentGenerator.Condition = () => evtCondition<T>();
            return this;
        }

        internal string GenerateResponse(BeamNode node)
        {
            _processedNode = node;

            _processedEvents = getTurnEvents(node);

            // generate response parts
            var responseParts = new List<string>();
            foreach (var generator in _generators)
            {
                for (_currentEventIndex = 0; _currentEventIndex < _processedEvents.Length; ++_currentEventIndex)
                {
                    if (!generator.Condition())
                        continue;

                    var sample = getSample(generator.Generator);
                    if (sample == null)
                        //sample generation failed - try further
                        continue;

                    responseParts.Add(sample);
                    //TODO offset by the pattern length
                    break;
                }
            }

            if (responseParts.Count == 0)
            {
                return "I don't know what to say.";
            }

            //TODO response part combinations
            //return string.Join(" ", responseParts);
            return responseParts.First();
        }

        private EventBase[] getTurnEvents(BeamNode node)
        {
            var turnEvents = new List<EventBase>();
            var currentNode = node;
            while (currentNode != null)
            {
                if (currentNode.Evt is TurnStartEvent)
                    break;

                turnEvents.Add(currentNode.Evt);
                currentNode = currentNode.ParentNode;
            }
            return turnEvents.ToArray();
        }

        private string getSample(OutputGenerator generator)
        {
            var samples = generator().ToArray();
            if (samples.Length == 0)
                return null;

            var sampleIndex = _rnd.Next(samples.Length);
            return samples[sampleIndex];
        }

        private bool hasFindAction()
        {
            return _processedEvents.Select(e => e as CompleteInstanceEvent).Where(e => e != null).Where(e => e.Instance.Concept == RestaurantDomainBeamGenerator.Find).Any();
        }
    }

    class PatternBasedGenerator
    {
        internal OutputGenerator Generator;

        public ConditionEvaluator Condition;
    }
}
