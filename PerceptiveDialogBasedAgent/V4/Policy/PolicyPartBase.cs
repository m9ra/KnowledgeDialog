﻿using PerceptiveDialogBasedAgent.V4.EventBeam;
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
        protected abstract IEnumerable<string> execute(BeamGenerator generator);

        private EventBase[] _previousTurnEvents;

        private HashSet<Concept2> _definedConcepts;

        private EventBase[] _turnEvents;

        private BeamGenerator _generator;

        internal string[] Execute(BeamGenerator generator, EventBase[] previousTurnEvents, EventBase[] turnEvents, HashSet<Concept2> definedConcepts)
        {
            try
            {
                _previousTurnEvents = previousTurnEvents;
                _definedConcepts = definedConcepts;
                _turnEvents = turnEvents;
                _generator = generator;

                return execute(generator).ToArray();
            }
            finally
            {
                _previousTurnEvents = null;
                _definedConcepts = null;
                _turnEvents = null;
                _generator = null;
            }
        }

        protected Evt Get<Evt>(Func<Evt, bool> predicate = null)
            where Evt : EventBase
        {
            return GetMany<Evt>(predicate).FirstOrDefault();
        }

        protected IEnumerable<Evt> GetMany<Evt>(Func<Evt, bool> predicate = null)
          where Evt : EventBase
        {
            for (var i = 0; i < _turnEvents.Length; ++i)
            {
                var e = _turnEvents[_turnEvents.Length - i - 1];
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
                if (predicate(instance))
                    yield return instance;
            }
        }

        internal bool IsDefined(Concept2 concept)
        {
            return _definedConcepts.Contains(concept);
        }

        protected string singular(ConceptInstance instance)
        {
            return instance.ToPrintable();
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
