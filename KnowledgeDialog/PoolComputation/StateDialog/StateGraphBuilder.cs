using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    class StateGraphBuilder
    {
        internal readonly TriggerGroup Triggers;

        internal Trigger DefaultTrigger { get; private set; }

        internal readonly StateBase State;

        private readonly Dictionary<UtteranceMapping<ActionBlock>, Trigger> _externalTriggers = new Dictionary<UtteranceMapping<ActionBlock>, Trigger>();

        private readonly Dictionary<EdgeIdentifier, Trigger> _directEdges = new Dictionary<EdgeIdentifier, Trigger>();

        internal StateGraphBuilder(StateBase state, ComposedGraph graph)
        {
            Triggers = new TriggerGroup(graph);
            State = state;
        }

        internal ModifiableResponse ExecuteState(StateContext context)
        {
            return State.Execute(context);
        }

        internal StateGraphBuilder Default(StateGraphBuilder nextState, StateProperty inputProperty = null)
        {
            DefaultTrigger = new Trigger(nextState, (c) => c.SetValue(inputProperty, c.Input));
            return this;
        }

        internal StateGraphBuilder DefaultForward(StateGraphBuilder nextState, StateProperty inputProperty = null)
        {
            DefaultTrigger = new Trigger(nextState, (c) =>
            {
                c.SetValue(inputProperty, c.Input);
                c.CurrentOutput = c.PreviousOutput;
            });
            return this;
        }

        internal StateGraphBuilder YesNoEdge(StateGraphBuilder nextState, StateProperty property)
        {
            addEdge("yes", nextState, (c) => c.SetValue(property, StateProperty.TrueValue));
            addEdge("of course", nextState, (c) => c.SetValue(property, StateProperty.TrueValue));
            addEdge("yeah", nextState, (c) => c.SetValue(property, StateProperty.TrueValue));
            addEdge("y", nextState, (c) => c.SetValue(property, StateProperty.TrueValue));
            addEdge("no", nextState, (c) => c.SetValue(property, StateProperty.FalseValue));
            addEdge("not", nextState, (c) => c.SetValue(property, StateProperty.FalseValue));
            addEdge("none", nextState, (c) => c.SetValue(property, StateProperty.FalseValue));
            addEdge("n", nextState, (c) => c.SetValue(property, StateProperty.FalseValue));

            return this;
        }

        internal StateGraphBuilder HasMatch(UtteranceMapping<ActionBlock> group, StateGraphBuilder nextState, StateProperty inputProperty = null)
        {
            var trigger = new Trigger(nextState, (c) => c.SetValue(inputProperty, c.Input));
            _externalTriggers.Add(group, trigger);
            return this;
        }

        internal StateGraphBuilder IsEdge(StateGraphBuilder stateGraphBuilder, StateProperty stateProperty)
        {
            this
                .Edge("*", stateGraphBuilder, stateProperty)
                .Edge("his name is *", stateGraphBuilder, stateProperty)
                .Edge("her name is *", stateGraphBuilder, stateProperty)
                .Edge("he is *", stateGraphBuilder, stateProperty)
                .Edge("she is *", stateGraphBuilder, stateProperty)
                .Edge("it is *", stateGraphBuilder, stateProperty)
                .Edge("correct answer is *", stateGraphBuilder, stateProperty)
                .Edge("* is the correct answer", stateGraphBuilder, stateProperty);

            return this;
        }

        internal StateGraphBuilder Edge(string pattern, StateGraphBuilder nextState, params StateProperty[] properties)
        {
            addEdge(pattern, nextState, (c) => setProperties(c, properties));
            return this;
        }

        internal StateGraphBuilder Edge(EdgeIdentifier edge, StateGraphBuilder nextState)
        {
            var trigger = new Trigger(nextState, null);
            _directEdges.Add(edge, trigger);
            return this;
        }

        internal StateGraphBuilder TakeEdgesFrom(StateGraphBuilder state)
        {
            if (state.DefaultTrigger != null)
                DefaultTrigger = state.DefaultTrigger;

            Triggers.FillFrom(state.Triggers);
            foreach (var directEdge in state._directEdges)
            {
                _directEdges[directEdge.Key] = directEdge.Value;
            }

            return this;
        }

        internal IEnumerable<MappingControl<Trigger>> UtteranceEdge(string utterance)
        {
            var parsedUtterance = SentenceParser.Parse(utterance);

            foreach (var triggerPair in _externalTriggers)
            {
                var bestHypothesis = triggerPair.Key.BestMap(parsedUtterance);
                if (bestHypothesis != null && bestHypothesis.Score > 0.5)
                {
                    return new[]{
                        bestHypothesis.ChangeValue(triggerPair.Value)
                    };
                }
            }

            return Triggers.FindMapping(parsedUtterance);
        }

        internal IEnumerable<MappingControl<Trigger>> DirectEdge(EdgeIdentifier edgeId)
        {
            Trigger trigger;
            if (!_directEdges.TryGetValue(edgeId, out trigger))
                yield break;

            yield return new MappingControl<Trigger>(null, 1.0, null, trigger, null);
        }

        private void addEdge(string pattern, StateGraphBuilder nextState, TriggerAction action)
        {
            var trigger = new Trigger(nextState, action);

            Triggers.SetMapping(pattern, trigger);
        }

        private void setProperties(StateContext context, StateProperty[] properties)
        {
            var index = 0;
            foreach (var value in context.Substitutions)
            {
                if (index >= properties.Length)
                    //no properties to be set
                    return;

                context.SetValue(properties[index], value);

                ++index;
            }
        }
    }
}
