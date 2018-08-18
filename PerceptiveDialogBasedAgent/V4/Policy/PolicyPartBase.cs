using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Policy
{
    abstract class PolicyPartBase
    {
        protected ConceptInstance TagInstance => _tagInstance;

        internal ConceptInstance LastTag { get; private set; }

        protected abstract IEnumerable<string> execute(BeamGenerator generator);

        private EventBase[] _previousTurnEvents;

        private HashSet<Concept2> _definedConcepts;

        private EventBase[] _turnEvents;

        private BeamGenerator _generator;

        private ConceptInstance _tagInstance;

        internal string[] Execute(BeamGenerator generator, EventBase[] previousTurnEvents, EventBase[] turnEvents, HashSet<Concept2> definedConcepts)
        {
            try
            {
                _previousTurnEvents = previousTurnEvents;
                _definedConcepts = definedConcepts;
                _turnEvents = turnEvents;
                _generator = generator;
                _tagInstance = new ConceptInstance(AsConcept(this.GetType()));
                return execute(generator).ToArray();
            }
            finally
            {
                _previousTurnEvents = null;
                _definedConcepts = null;
                _turnEvents = null;
                _generator = null;

                LastTag = _tagInstance;
                _tagInstance = null;
            }
        }

        protected Concept2 AsConcept<T>()
        {
            return AsConcept(typeof(T));
        }

        protected Concept2 AsConcept(Type policy)
        {
            return Concept2.From(policy.Name);
        }

        protected Evt Get<Evt>(Func<Evt, bool> predicate = null, bool searchInsideTurnOnly = true)
            where Evt : EventBase
        {
            return GetMany(predicate, searchInsideTurnOnly).FirstOrDefault();
        }

        protected IEnumerable<Evt> GetMany<Evt>(Func<Evt, bool> predicate = null, bool searchInsideTurnOnly = true)
          where Evt : EventBase
        {
            var needTurnStart = searchInsideTurnOnly;
            for (var i = 0; i < _turnEvents.Length; ++i)
            {
                var e = _turnEvents[_turnEvents.Length - i - 1];

                if (e is TurnStartEvent)
                    needTurnStart = false;

                if (needTurnStart)
                    continue;

                if (e is Evt evt)
                    if (predicate == null || predicate(evt))
                        yield return evt;
            }
        }

        protected Evt Find<Evt>(Func<Evt, bool> predicate = null, int precedingTurns = 0)
            where Evt : EventBase
        {
            var events = _generator.GetTurnEvents<Evt>(precedingTurns);
            if (predicate != null)
                events = events.Where(predicate);

            return events.FirstOrDefault();
        }

        protected IEnumerable<ConceptInstance> FindTurnInstances(Func<ConceptInstance, bool> predicate = null)
        {
            var instances = _generator.GetInputActivatedInstances();
            foreach (var instance in instances.Select(i => i.Instance).Distinct())
            {
                if (predicate == null || predicate(instance))
                    yield return instance;
            }
        }

        protected IEnumerable<ConceptInstance> FindDeactivatedTurnInstances(Func<ConceptInstance, bool> predicate = null)
        {
            var instances = _generator.GetTurnLimitedDeactivatedInputActivatedInstances();
            foreach (var instance in instances.Select(i => i.Instance).Distinct())
            {
                if (predicate == null || predicate(instance))
                    yield return instance;
            }
        }

        internal bool IsDefined(Concept2 concept)
        {
            return _definedConcepts.Contains(concept);
        }

        protected bool PreviousPolicy<T>()
    where T : PolicyPartBase
        {
            return PreviousPolicy<T>(out _);
        }

        protected bool PreviousPolicy<T>(out PolicyTagEvent tag)
            where T : PolicyPartBase
        {
            var policyConcept = AsConcept<T>();

            tag = _turnEvents.Where(e => e is PolicyTagEvent tagEvt && tagEvt.Tag.Concept == policyConcept).FirstOrDefault() as PolicyTagEvent;

            return tag != null;
        }

        protected IEnumerable<string> GetUnknownPhrases(BeamGenerator generator)
        {
            var allInputPhrases = GetMany<InputPhraseEvent>();

            var currentBuffer = new List<InputPhraseEvent>();
            foreach (var inputPhrase in allInputPhrases)
            {
                var phrase = inputPhrase;
                var isPhraseDisconnected = currentBuffer.Count > 0 && phrase.InputId - currentBuffer.Last().InputId != 1;

                if (generator.IsInputUsed(phrase) || isDelimiter(inputPhrase) || isPhraseDisconnected)
                {
                    if (currentBuffer.Count > 0)
                        yield return composeUnknownPhrase(currentBuffer);

                    currentBuffer.Clear();
                    continue;
                }
                currentBuffer.Add(phrase);
            }

            if (currentBuffer.Count > 0)
                yield return composeUnknownPhrase(currentBuffer);
        }

        private bool isDelimiter(InputPhraseEvent inputPhrase)
        {
            var phrase = inputPhrase.Phrase;
            return new[] { "is", "can", "will", "in", "from", "out", "of", "i", "am", "iam", "i'am", "where", "what", "mean" }.Contains(phrase);
        }

        private string composeUnknownPhrase(IEnumerable<InputPhraseEvent> currentBuffer)
        {
            return string.Join(" ", currentBuffer.Select(i => i.Phrase));
        }

        protected string singular(ConceptInstance instance)
        {
            return instance.ToPrintable();
        }

        protected string singularWithProperty(ConceptInstance instance)
        {
            var properties = _generator.GetPropertyValues(instance, includeInheritedProps: false);
            foreach (var property in properties.Keys.ToArray())
            {
                var value = properties[property];
                if (value.Concept == Concept2.Something)
                    continue;

                //properties.Remove(property);
            }

            if (properties.Count == 0)
                return instance.ToPrintable();

            var reportedProperty = properties.Keys.First();
            var reportedValue = properties[reportedProperty];
            var propertyName = reportedProperty.Name;

            if (reportedValue.Concept == Concept2.Something)
                return instance.ToPrintable() + " " + propertyName;
            else
                return instance.ToPrintable() + " " + propertyName + " is " + reportedValue.ToPrintable();
        }

        protected string singular(Concept2 concept)
        {
            return concept.Name;
        }

        protected string plural(ConceptInstance instance)
        {
            return singular(instance) + "s";
        }

        protected string plural(Concept2 concept)
        {
            return singular(concept) + "s";
        }
    }
}
