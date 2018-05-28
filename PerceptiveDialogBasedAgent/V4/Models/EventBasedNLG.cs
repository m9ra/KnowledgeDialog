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

        private HashSet<EventBase> _completedEvents = new HashSet<EventBase>();

        private BeamNode _processedNode;

        private int _currentEventIndex = 0;

        private PatternBasedGenerator _currentGenerator = null;

        internal EventBase CurrentEvent => _processedEvents[_currentEventIndex];

        internal EventBasedNLG()
        {
            this
                .ForEvent<InstanceFoundEvent>()
                    .Output(simpleOutput)

                .ForEvent<ConfirmationAcceptedEvent>()
                    .Output(confirmationAccepted)

                .ForEvent<SubstitutionConfirmationRequestEvent>()
                    .Output(substitutionConfirmationRequest)

                .ForEvent<UnknownPhraseSubstitutionEvent>()
                    .Output(unknownPhraseSubstitution)

                .ForEvent<SubstitutionRequestEvent>()
                    .Output(needsSubstitution)

                .ForEvent<PhraseStillNotKnownEvent>()
                    .Output(stillNotKnown)

                .ForEvent<InstanceUnderstoodEvent>()
                    .Output(instanceUnderstood)

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

        private IEnumerable<string> confirmationAccepted()
        {
            var continuation = generateResponse(); //continuation is required

            yield return "Ok. " + continuation;
            yield return "I see. " + continuation;
            yield return "Nice. " + continuation;
            yield return "Interesting. " + continuation;
        }

        private IEnumerable<string> substitutionConfirmationRequest()
        {
            var evt = CurrentEvent as SubstitutionConfirmationRequestEvent;

            var target = evt.SubstitutionRequest.Target;
            var classDescriptor = target.Property == Concept2.Subject ? getPropertyValue(target.Instance, Concept2.Target).Concept.Name : evt.UnknownPhrase.InputPhraseEvt.Phrase;
            var childDescriptor = target.Property == Concept2.Subject ? evt.UnknownPhrase.InputPhraseEvt.Phrase : getPropertyValue(target.Instance, Concept2.Subject)?.Concept.Name;

            //TODO make it more general
            yield return $"You think {childDescriptor} is {target.Instance.Concept.Name} {classDescriptor} ?";
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

            if (subject == "what")
            {
                yield return "What are you asking for?";
            }
            else if (subject == Concept2.InstanceOf.Name)
            {
                var target = getProperty(evt.Target.Instance, Concept2.Target);
                if (target != null)
                {
                    yield return "What is " + target.ToPrintable() + " ?";
                }
                else if (subject != null)
                {
                    yield return "What class is " + subject + " ?";
                }
                else
                {
                    yield return "What is instance of what ?";
                }
            }
            else
            {
                yield return questionFormulation + " " + subject;
            }
        }


        private IEnumerable<string> stillNotKnown()
        {
            var evt = CurrentEvent as PhraseStillNotKnownEvent;
            var unknownPhrase = evt.UnknownPhraseSubstitutionEvent.UnknownPhrase.InputPhraseEvt.Phrase;

            if (unknownPhrase != null)
                yield return $"It seems to be complicated, I don't know {evt.UnknownPhraseEvent.InputPhraseEvt.Phrase} either. So, what does {unknownPhrase} mean?";
        }

        private IEnumerable<string> instanceUnderstood()
        {
            var evt = CurrentEvent as InstanceUnderstoodEvent;
            var concept = evt.InstanceActivationEvent.Instance.Concept;
            var phrase = concept.Name;

            if (new[] { Concept2.Yes, Concept2.No, Concept2.DontKnow}.Contains(concept))
                //filter too general answers
                yield break;

            yield return $"I know what {phrase} is. But I don't know what should I do?";
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
            _completedEvents = new HashSet<EventBase>();

            return generateResponse();
        }

        private string generateResponse()
        {
            // generate response parts
            var responseParts = new List<string>();
            foreach (var generator in _generators)
            {
                for (_currentEventIndex = 0; _currentEventIndex < _processedEvents.Length; ++_currentEventIndex)
                {
                    var eventToProcess = _processedEvents[_currentEventIndex];
                    if (_completedEvents.Contains(eventToProcess))
                        continue;

                    if (!generator.Condition())
                        continue;

                    _completedEvents.Add(eventToProcess);

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
                return "I don't know what to say next.";
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
            return _processedEvents.Select(e => e as CompleteInstanceEvent).Where(e => e != null).Where(e => e.Instance.Concept.Name == "find").Any();
        }
    }

    class PatternBasedGenerator
    {
        internal OutputGenerator Generator;

        public ConditionEvaluator Condition;
    }
}
