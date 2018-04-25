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

        private ConceptInstance[] _processedEvents;

        private MindState _processedState;

        private int _currentEventIndex = 0;

        private PatternBasedGenerator _currentGenerator = null;

        internal EventBasedNLG()
        {
            this
                .ForPattern(Concept2.Output)
                    .Output(simpleOutput)

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
            for (_currentEventIndex = 0; _currentEventIndex < _processedEvents.Length; ++_currentEventIndex)
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
