using PerceptiveDialogBasedAgent.V4.Brain;
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

        private readonly Body _body;

        private ConceptInstance[] _processedEvents;

        private MindState _processedState;

        private int _currentEventIndex = 0;

        private PatternBasedGenerator _currentGenerator = null;

        internal EventBasedNLG(Body body)
        {
            _body = body;

            this
                .ForPattern(Concept2.Output)
                    .Output(simpleOutput)

                .ForPattern(Concept2.NeedsRefinement)
                    .Output(refinement)

                .ForPattern(Concept2.NotFoundEvent)
                    .Output(notFound)
            ;
        }

        private IEnumerable<string> notFound()
        {
            var constraint = getEventProperty(Concept2.Subject);
            var constraintRepresentation = constraint.ToPrintable();

            yield return "I don't know anything which is " + constraintRepresentation;
        }

        private IEnumerable<string> simpleOutput()
        {
            var outputRepresentation = getEventProperty(Concept2.Subject).ToPrintable();

            yield return "I know " + outputRepresentation;
            yield return "I think, you want " + outputRepresentation;
        }

        private IEnumerable<string> refinement()
        {
            var ambiguousConstraint = getEventProperty(Concept2.Target);
            var constraintSpecification = getConstraintDescription(ambiguousConstraint);
            constraintSpecification = getPlural(constraintSpecification);

            yield return "I know many " + constraintSpecification + ", could you be more specific?";
        }

        private string getConstraintDescription(PointableInstance instance)
        {
            var conceptInstance = instance as ConceptInstance;
            if (conceptInstance is null || conceptInstance.Concept != Concept2.Something)
                return instance.ToPrintable();

            var description = getPropertyValue(conceptInstance, Concept2.InstanceOf)?.ToPrintable();
            if (description == null)
                description = "thing";

            //collect modifiers
            foreach (var property in _processedState.GetProperties(instance))
            {
                if (property == Concept2.InstanceOf)
                    continue;

                var modifier = getPropertyValue(instance, property).ToPrintable();
                description = modifier + " " + description;
            }

            return description;
        }

        private PointableInstance getPropertyValue(PointableInstance instance, Concept2 property)
        {
            return _processedState.GetPropertyValue(instance, property);
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

            foreach (var concept in _body.Concepts)
            {
                foreach (var property in concept.Properties)
                {
                    var value = concept.GetPropertyValue(property);
                    if (value == instance)
                        return true;

                    var conceptValue = value as ConceptInstance;
                    if (conceptValue != null && conceptValue.Concept == concept)
                        return true;
                }
            }

            return false;
        }

        private bool isProperty(PointableInstance instance)
        {
            var conceptInstance = instance as ConceptInstance;
            if (conceptInstance == null)
                return false;

            foreach (var concept in _body.Concepts)
            {
                foreach (var property in concept.Properties)
                {
                    if (property == conceptInstance.Concept)
                        return true;
                }
            }

            return false;
        }

        private PointableInstance getEventProperty(Concept2 property)
        {
            var evt = _processedEvents[_currentEventIndex];
            return _processedState.GetPropertyValue(evt, property);
        }

        private bool evtPatternCondition(Concept2[] evtPattern)
        {
            if (_currentEventIndex + evtPattern.Length > _processedEvents.Length)
                return false;

            for (var i = 0; i < evtPattern.Length; ++i)
            {
                var pattern = evtPattern[i];
                var processedEvent = _processedEvents[_currentEventIndex + i];

                if (processedEvent.Concept != pattern)
                    return false;
            }

            return true;
        }

        private EventBasedNLG Output(OutputGenerator generator)
        {
            _currentGenerator.Generator = generator;
            return this;
        }

        private EventBasedNLG ForPattern(params Concept2[] evtPattern)
        {
            _currentGenerator = new PatternBasedGenerator();
            _generators.Add(_currentGenerator);

            _currentGenerator.Condition = () => evtPatternCondition(evtPattern);
            return this;
        }

        internal string GenerateResponse(MindState state)
        {
            _processedState = state;
            _processedEvents = state.Events.ToArray();
            var responseParts = new List<string>();

            // find where turn started
            for (_currentEventIndex = _processedEvents.Length - 1; _currentEventIndex >= 0; _currentEventIndex--)
                if (_processedEvents[_currentEventIndex].Concept == Concept2.NewTurn)
                    break;

            // generate response parts
            for (; _currentEventIndex < _processedEvents.Length; ++_currentEventIndex)
            {
                foreach (var generator in _generators)
                {
                    if (!generator.Condition())
                        continue;

                    responseParts.Add(getSample(generator.Generator));
                    //TODO offset by the pattern length
                    break;
                }
            }

            if (responseParts.Count == 0)
            {
                return "I don't know what to say.";
            }

            //TODO response part combinations
            return string.Join(" ", responseParts);
        }

        private string getSample(OutputGenerator generator)
        {
            var samples = generator().ToArray();

            var sampleIndex = _rnd.Next(samples.Length);
            return samples[sampleIndex];
        }
    }

    class PatternBasedGenerator
    {
        internal OutputGenerator Generator;

        public ConditionEvaluator Condition;
    }
}
