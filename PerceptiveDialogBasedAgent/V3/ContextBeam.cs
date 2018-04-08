using PerceptiveDialogBasedAgent.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    class ContextBeam
    {
        private List<BodyState> _currentStates = new List<BodyState>();

        private readonly Body _body;

        internal ContextBeam(Body body)
        {
            _body = body;
            _currentStates.Add(new BodyState());
        }

        internal void ExtendInput(string word)
        {
            var newStates = new List<BodyState>();
            foreach (var state in _currentStates)
            {
                foreach (var stateWithInput in inputCombinations(state, word))
                {
                    var pointCombinationStates = pointCombinations(stateWithInput).ToArray();
                    foreach (var stateWithPointing in pointCombinationStates)
                    {
                        newStates.Add(stateWithPointing);
                    }
                }
            }
            _currentStates = newStates.OrderByDescending(s => s.Score).Take(16).ToList();
        }

        internal BodyState GetBestState()
        {
            return _currentStates.OrderByDescending(s => _body.ReadoutScore(s)).FirstOrDefault();
        }

        private IEnumerable<BodyState> inputCombinations(BodyState state, string word)
        {
            if (state.LastInputPhrase != null && state.GetConcept(state.LastInputPhrase) == null)
                yield return state.ExpandLastPhrase(word); //expand previous phrase
            yield return state.AddNewPhrase(word); //consider last phrase as closed and add new one
        }

        internal void SetState(BodyState state)
        {
            _currentStates.Clear();
            _currentStates.Add(state);
        }

        private IEnumerable<BodyState> pointCombinations(BodyState state)
        {
            yield return state; //nothing is pointing from last reference
            foreach (var concept in _body.Concepts)
            {
                var lastInputPhrase = state.LastInputPhrase;
                var probability = _body.PointingScore(state, lastInputPhrase, concept);
                var pointingHypotheses = setPointer(state, lastInputPhrase, concept, probability).ToArray();
                foreach (var hypothesis in pointingHypotheses)
                {
                    yield return hypothesis;
                }
            }
        }

        private IEnumerable<BodyState> setPointer(BodyState inputState, InputPhrase phrase, Concept concept, double pointingProbability)
        {
            if (pointingProbability <= 0)
                yield break;

            var rankedConcept = new RankedConcept(concept, pointingProbability);
            var state = inputState.SetPointer(phrase, rankedConcept);
            var context = new BodyContext(rankedConcept, _body, state);
            context.EvaluateActivation(concept);

            yield return context.CurrentState;

            var assignmentInitialState = context.CurrentState;

            //try parameter assignments
            foreach (var requirement in assignmentInitialState.PendingRequirements)
            {
                foreach (var mentionedConcept in assignmentInitialState.RecentMentionedConcepts)
                {
                    if (mentionedConcept == requirement.RequestingConcept.Concept)
                        //don't allow self assignments
                        continue;

                    var newState = assignParameter(assignmentInitialState, requirement, mentionedConcept);
                    if (newState != null)
                        yield return newState;
                }
            }
        }

        private BodyState assignParameter(BodyState initialState, ConceptRequirement requirement, Concept assignedConcept)
        {
            if (!requirement.Domain.Contains(assignedConcept))
                return null;

            var parameterScore = _body.ParameterAssignScore(initialState, requirement, assignedConcept);
            if (parameterScore <= 0)
                return null;

            var assignedState = initialState.AssignParameter(requirement, assignedConcept, parameterScore);
            var context = new BodyContext(requirement.RequestingConcept, _body, assignedState);
            context.EvaluateActivation(requirement.RequestingConcept.Concept);
            return context.CurrentState;
        }
    }
}
