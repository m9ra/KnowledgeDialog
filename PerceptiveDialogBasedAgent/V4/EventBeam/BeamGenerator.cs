using KnowledgeDialog.Dialog;
using PerceptiveDialogBasedAgent.V4.Brain;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.EventBeam
{
    class BeamGenerator
    {
        private readonly HashSet<BeamNode> _beam = new HashSet<BeamNode>();

        private BeamNode _currentNode = null;


        internal BeamGenerator()
        {
            //add root node
            _beam.Add(new BeamNode());
        }

        internal virtual void Visit(InputPhraseEvent evt)
        {
            //try to activate known concepts
            foreach (var concept in GetConcepts())
            {
                var scoreEvent = new InputPhraseScoreEvent(evt, concept);
                var score = GetScore(scoreEvent);
                if (score < Configuration.ConceptActivationThreshold)
                    continue;

                Push(new InstanceActivationEvent(evt, new ConceptInstance(concept)));
                Push(scoreEvent);
                Pop();
                Pop();
            }

            //try to treat it as an unknown phrase
            Push(new UnknownPhraseEvent(evt));
            Pop();

            //TODO try to reactivate previous mentions
        }

        internal virtual void Visit(InstanceActivationEvent evt)
        {
            var targetDefinitions = GetTargetDefinitions(evt.Instance);
            foreach (var targetDefinition in targetDefinitions)
            {
                Push(new SubstitutionRequestEvent(evt.Instance, targetDefinition));
                //no pops because all the parameters will be requested in a serie
            }

            if (targetDefinitions.Any())
                // no more processing for incomplete instances
                return;

            //we have got a complete instance
            Push(new CompleteInstanceEvent(evt));
        }

        internal virtual void Visit(CompleteInstanceEvent evt)
        {
            // we have a complete instance - lets look where it can be put into
            foreach (var request in GetFreeSubstitutionRequests())
            {
                if (!IsSatisfiedBy(request, evt.Instance))
                    //substitution is not possible
                    continue;

                var scoreEvt = new DistanceScoreEvt(request, evt);
                Push(new SubstitutionEvent(request, evt));
                Push(scoreEvt);
                Pop();
                Pop();
            }

            //instance can be left without being substituted anywhere
            PushSelf();
        }

        internal virtual void Visit(UnknownPhraseEvent evt)
        {
            //try to substitute unknown phrase to some parameter
            var substitutionRequests = GetFreeSubstitutionRequests();
            foreach (var request in substitutionRequests)
            {
                var scoreEvt = new DistanceScoreEvt(request, evt);
                Push(new UnknownPhraseSubstitutionEvent(request, evt));
                Push(scoreEvt);
                Pop();
                Pop();
            }

            PushSelf();
        }

        internal virtual void Visit(SubstitutionRequestEvent evt)
        {
            //try to substitute by some free active instance
            var completeInstances = GetFreeCompleteInstances();
            foreach (var freeInstance in completeInstances)
            {
                if (!IsSatisfiedBy(evt, freeInstance.Instance))
                    continue;

                var scoreEvt = new DistanceScoreEvt(evt, freeInstance);
                Push(new SubstitutionEvent(evt, freeInstance));
                Push(scoreEvt);
                Pop();
                Pop();
            }

            //try to substitute by unknown phrases
            var unknownPhrases = GetFreeUnknownPhrases();
            foreach (var unknownPhrase in unknownPhrases)
            {
                var scoreEvt = new DistanceScoreEvt(evt, unknownPhrase);
                Push(new UnknownPhraseSubstitutionEvent(evt, unknownPhrase));
                Push(scoreEvt);
                Pop();
                Pop();
            }

            PushSelf();
        }

        internal virtual void Visit(SubstitutionEvent evt)
        {
            //TODO create instance available event according to open requests
        }

        internal virtual void Visit(UnknownPhraseSubstitutionEvent evt)
        {
            // nothing to do by default - instance available is generated after unkonwn phrase is confirmed
        }

        internal virtual void Visit(ConceptDefinedEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(ConceptDescriptionEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(TargetDefinedEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(TracedScoreEventBase evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(EventBase evt)
        {
            throw new NotSupportedException("Unknown event");
        }

        #region Beam Operations

        /// <summary>
        /// Pushes to all beam branches
        /// </summary>
        public void PushToAll(EventBase evt)
        {
            foreach (var node in _beam.ToArray())
            {
                try
                {
                    _currentNode = node;
                    Push(evt);
                }
                finally
                {
                    _currentNode = null;
                }
            }
        }

        /// <summary>
        /// Pushes to currently active branch
        /// </summary>
        /// <param name="evt"></param>
        public void Push(EventBase evt)
        {
            if (_currentNode == null)
                throw new InvalidOperationException("Cannot push in this state");

            var newNode = new BeamNode(_currentNode, evt);
            _beam.Remove(_currentNode);
            _beam.Add(newNode);
            _currentNode = newNode;

            evt.Accept(this);
        }

        public void PushSelf()
        {
            var newNode = new BeamNode(_currentNode.ParentNode, _currentNode.Evt);
            _beam.Remove(_currentNode);
            _beam.Add(newNode);
        }

        public void Pop()
        {
            _currentNode = _currentNode.ParentNode;
        }

        protected bool IsSatisfiedBy(SubstitutionRequestEvent request, ConceptInstance instance)
        {
            //TODO check for patterns
            return true;
        }

        protected double GetScore(TracedScoreEventBase scoreEvt)
        {
            //TODO recognition learning belongs here
            return scoreEvt.GetDefaultScore();
        }

        protected double GetScore(BeamNode node)
        {
            var accumulator = 0.0;
            var currentNode = node;
            while (currentNode != null)
            {
                var scoreEvt = currentNode.Evt as TracedScoreEventBase;
                if (scoreEvt != null)
                    accumulator += GetScore(scoreEvt);

                currentNode = currentNode.ParentNode;
            }

            return accumulator;
        }

        protected IEnumerable<Concept2> GetConcepts()
        {
            if (_currentNode == null)
                throw new InvalidOperationException();

            return GetConcepts(_currentNode);
        }

        protected IEnumerable<TargetDefinedEvent> GetTargetDefinitions(ConceptInstance instance)
        {
            if (_currentNode == null)
                throw new InvalidOperationException();

            return GetTargetDefinitions(instance, _currentNode);
        }

        protected IEnumerable<SubstitutionRequestEvent> GetFreeSubstitutionRequests()
        {
            return GetFreeSubstitutionRequests(_currentNode);
        }

        protected IEnumerable<UnknownPhraseEvent> GetFreeUnknownPhrases()
        {
            return GetFreeUnknownPhrases(_currentNode);
        }

        protected IEnumerable<CompleteInstanceEvent> GetFreeCompleteInstances()
        {
            return GetFreeCompleteInstances(_currentNode);
        }

        protected IEnumerable<CompleteInstanceEvent> GetFreeCompleteInstances(BeamNode node)
        {
            var result = new List<CompleteInstanceEvent>();
            var substitutedInstances = new HashSet<ConceptInstance>();

            var currentNode = node;
            while (currentNode != null)
            {
                var substitutionEvt = currentNode.Evt as SubstitutionEvent;
                if (substitutionEvt != null)
                    substitutedInstances.Add(substitutionEvt.Instance);

                var activationEvt = currentNode.Evt as CompleteInstanceEvent;
                if (activationEvt != null && !substitutedInstances.Contains(activationEvt.Instance))
                    result.Add(activationEvt);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        protected IEnumerable<UnknownPhraseEvent> GetFreeUnknownPhrases(BeamNode node)
        {
            var result = new List<UnknownPhraseEvent>();
            var usedPhrases = new HashSet<UnknownPhraseEvent>();

            var currentNode = node;
            while (currentNode != null)
            {
                var substitutionEvt = currentNode.Evt as UnknownPhraseSubstitutionEvent;
                if (substitutionEvt != null)
                    usedPhrases.Add(substitutionEvt.UnknownPhrase);

                var unknownPhraseEvt = currentNode.Evt as UnknownPhraseEvent;
                if (unknownPhraseEvt != null && !usedPhrases.Contains(unknownPhraseEvt))
                    result.Add(unknownPhraseEvt);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        protected IEnumerable<SubstitutionRequestEvent> GetFreeSubstitutionRequests(BeamNode node)
        {
            var result = new List<SubstitutionRequestEvent>();
            var substitutedRequests = new HashSet<SubstitutionRequestEvent>();

            var currentNode = node;
            while (currentNode != null)
            {
                var substitutionEvt = currentNode.Evt as SubstitutionEvent;
                if (substitutionEvt != null)
                    substitutedRequests.Add(substitutionEvt.SubstitutionRequest);

                var unknownPhraseSubstitution = currentNode.Evt as UnknownPhraseSubstitutionEvent;
                if (unknownPhraseSubstitution != null)
                    substitutedRequests.Add(unknownPhraseSubstitution.SubstitutionRequest);

                var requestEvt = currentNode.Evt as SubstitutionRequestEvent;
                if (requestEvt != null && !substitutedRequests.Contains(requestEvt))
                    result.Add(requestEvt);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        internal IEnumerable<Concept2> GetConcepts(BeamNode node)
        {
            var result = new List<Concept2>();
            var currentNode = node;
            while (currentNode != null)
            {
                var conceptDefinedEvt = currentNode.Evt as ConceptDefinedEvent;
                if (conceptDefinedEvt != null)
                    result.Add(conceptDefinedEvt.Concept);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        protected IEnumerable<TargetDefinedEvent> GetTargetDefinitions(ConceptInstance instance, BeamNode node)
        {
            var result = new List<TargetDefinedEvent>();
            var currentNode = node;
            while (currentNode != null)
            {
                var tagetDefinedEvt = currentNode.Evt as TargetDefinedEvent;
                if (tagetDefinedEvt?.Concept == instance.Concept)
                    result.Add(tagetDefinedEvt);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        internal IEnumerable<string> GetDescriptions(Concept2 concept, BeamNode node)
        {
            var result = new List<string>();
            var currentNode = node;
            while (currentNode != null)
            {
                var descriptionEvt = currentNode.Evt as ConceptDescriptionEvent;
                if (descriptionEvt?.Concept == concept)
                    result.Add(descriptionEvt.Description);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        internal BeamNode GetBestNode()
        {
            var bestScore = double.NegativeInfinity;
            BeamNode bestNode = null;
            foreach (var node in _beam)
            {
                var score = GetScore(node);
                if (bestNode == null || score > bestScore)
                {
                    bestScore = score;
                    bestNode = node;
                }
            }

            return bestNode;
        }

        internal Ranked<BeamNode>[] GetRankedNodes()
        {
            var result = new List<Ranked<BeamNode>>();
            foreach (var node in _beam)
            {
                var score = GetScore(node);

                result.Add(new Ranked<BeamNode>(node, score));
            }

            return result.OrderByDescending(r => r.Rank).ToArray();
        }

        #endregion
    }
}
