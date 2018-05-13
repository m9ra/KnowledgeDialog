using KnowledgeDialog.Dialog;
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
        private readonly Stack<List<BeamNode>> _layers = new Stack<List<BeamNode>>();

        private readonly HashSet<BeamNode> _beam = new HashSet<BeamNode>();

        internal BeamGenerator()
        {
            //add root node
            _beam.Add(new BeamNode());
        }

        internal virtual void Visit(InputPhraseEvent evt)
        {
            //try to activate known concepts
            tryActivateKnownConcepts(evt);

            //try to treat it as an unknown phrase
            tryActivateUnknownPhrase(evt);
        }

        internal virtual void Visit(InstanceActivationEvent evt)
        {
            var requests = pushFreeParameterSubstitutionRequests(evt);
            if (!requests.Any())
                //we have got a complete instance
                Push(new CompleteInstanceEvent(evt.Instance));
        }

        internal virtual void Visit(CompleteInstanceEvent evt)
        {
            // we have a complete instance - lets look where it can be put into
            tryAsSubstitution(evt);

            //instance can be left without being substituted anywhere
            PushSelf();
        }

        internal virtual void Visit(UnknownPhraseEvent evt)
        {
            //try to substitute unknown phrase to some targets
            tryAsSubstitution(evt);

            PushSelf();
        }

        internal virtual void Visit(SubstitutionRequestEvent evt)
        {
            //try to substitute by some free active instance
            var completeInstances = GetFreeCompleteInstances();
            trySubstituteBy(evt, completeInstances);

            //try to substitute by unknown phrases
            var unknownPhrases = GetFreeUnknownPhrases();
            trySubstituteBy(evt, unknownPhrases);

            PushSelf();
        }

        internal virtual void Visit(PropertySetEvent evt)
        {
            tryPushComplete(evt.Target.Instance);
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

        internal virtual void Visit(ParamDefinedEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(TracedScoreEventBase evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(InstanceFoundEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(TooManyInstancesFoundEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(NoInstanceFoundEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(NewTurnEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(EventBase evt)
        {
            throw new NotSupportedException("Unknown event");
        }

        #region Beam Operations

        public void SetProperty(Concept2 concept, Concept2 property, ConceptInstance value)
        {
            PushToAll(new PropertySetEvent(new PropertySetTarget(concept, property), value));
        }

        public void PushInput(string input)
        {
            PushToAll(new InputPhraseEvent(input));
        }

        /// <summary>
        /// Pushes to all beam branches
        /// </summary>
        public void PushToAll(EventBase evt)
        {
            foreach (var node in _beam.ToArray())
            {
                try
                {
                    _layers.Push(new List<BeamNode>() { node });
                    Push(evt);
                }
                finally
                {
                    _layers.Pop();
                }
            }
        }

        /// <summary>
        /// Pushes to currently active branch
        /// </summary>
        /// <param name="evt"></param>
        public void Push(EventBase evt)
        {
            var currentLayer = _layers.Peek();

            var newLayer = new List<BeamNode>();
            foreach (var pushNode in currentLayer)
            {
                var oldBeam = new HashSet<BeamNode>(_beam);
                var newNode = new BeamNode(pushNode, evt);
                _beam.Remove(pushNode);
                _beam.Add(newNode);

                try
                {
                    _layers.Push(new List<BeamNode>() { newNode });
                    evt.Accept(this);
                }
                finally
                {
                    newLayer.AddRange(_beam.Except(oldBeam));

                    //auto pop layers so Pops within accept works consistently
                    //the popped layers are not interesting - we care only about the top ones
                    while (_layers.Peek() != currentLayer)
                        _layers.Pop();
                }
            }

            _layers.Push(newLayer);
        }

        public void PushSelf()
        {
            var currentNode = getCurrentNode();
            var newNode = new BeamNode(currentNode.ParentNode, currentNode.Evt);
            _beam.Remove(currentNode);
            _beam.Add(newNode);
        }

        public void Pop()
        {
            _layers.Pop();
        }

        protected bool IsSatisfiedBy(SubstitutionRequestEvent request, ConceptInstance instance)
        {
            if (request.Target.Instance == instance)
                //disable self assignments
                return false;

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
                if (currentNode.Evt is TracedScoreEventBase scoreEvt)
                    accumulator += GetScore(scoreEvt);

                currentNode = currentNode.ParentNode;
            }

            return accumulator;
        }

        protected BeamNode getCurrentNode()
        {
            var layer = _layers.Peek();
            if (layer.Count != 1)
                throw new NotImplementedException();

            return layer.First();
        }

        protected IEnumerable<Concept2> GetConcepts()
        {
            return GetConcepts(getCurrentNode());
        }

        protected IEnumerable<ParamDefinedEvent> GetParameterDefinitions(ConceptInstance instance)
        {
            return GetParameterDefinitions(instance, getCurrentNode());
        }

        protected IEnumerable<SubstitutionRequestEvent> GetFreeSubstitutionRequests()
        {
            return GetFreeSubstitutionRequests(getCurrentNode());
        }

        protected IEnumerable<UnknownPhraseEvent> GetFreeUnknownPhrases()
        {
            return GetFreeUnknownPhrases(getCurrentNode());
        }

        protected IEnumerable<CompleteInstanceEvent> GetFreeCompleteInstances()
        {
            return GetFreeCompleteInstances(getCurrentNode());
        }

        protected IEnumerable<CompleteInstanceEvent> GetFreeCompleteInstances(BeamNode node)
        {
            var result = new List<CompleteInstanceEvent>();
            var substitutedInstances = new HashSet<ConceptInstance>();

            var currentNode = node;
            while (currentNode != null)
            {
                if (currentNode.Evt is PropertySetEvent propertySetEvt)
                    substitutedInstances.Add(propertySetEvt.SubstitutedValue);

                if (currentNode.Evt is CompleteInstanceEvent activationEvt && !substitutedInstances.Contains(activationEvt.Instance))
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
                if (currentNode.Evt is UnknownPhraseSubstitutionEvent substitutionEvt)
                    usedPhrases.Add(substitutionEvt.UnknownPhrase);

                if (currentNode.Evt is UnknownPhraseEvent unknownPhraseEvt && !usedPhrases.Contains(unknownPhraseEvt))
                    result.Add(unknownPhraseEvt);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        protected IEnumerable<SubstitutionRequestEvent> GetFreeSubstitutionRequests(BeamNode node)
        {
            var result = new List<SubstitutionRequestEvent>();
            var substitutedTargets = new HashSet<PropertySetTarget>();

            var currentNode = node;
            while (currentNode != null)
            {
                if (currentNode.Evt is PropertySetEvent propertySetEvt)
                    substitutedTargets.Add(propertySetEvt.Target);

                if (currentNode.Evt is UnknownPhraseSubstitutionEvent unknownPhraseSubstitution)
                    substitutedTargets.Add(unknownPhraseSubstitution.SubstitutionRequest.Target);

                if (currentNode.Evt is SubstitutionRequestEvent requestEvt && !substitutedTargets.Contains(requestEvt.Target))
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
                if (currentNode.Evt is ConceptDefinedEvent conceptDefinedEvt)
                    result.Add(conceptDefinedEvt.Concept);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        protected IEnumerable<ParamDefinedEvent> GetParameterDefinitions(ConceptInstance instance, BeamNode node)
        {
            var result = new List<ParamDefinedEvent>();
            var currentNode = node;
            while (currentNode != null)
            {
                var tagetDefinedEvt = currentNode.Evt as ParamDefinedEvent;
                if (tagetDefinedEvt?.Concept == instance.Concept)
                    result.Add(tagetDefinedEvt);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        internal InstanceActivationEvent GetInstanceActivation(ConceptInstance instance)
        {
            return GetInstanceActivation(instance, getCurrentNode());
        }

        internal static InstanceActivationEvent GetInstanceActivation(ConceptInstance instance, BeamNode node)
        {
            var currentNode = node;
            while (currentNode != null)
            {
                var activationEvt = currentNode.Evt as InstanceActivationEvent;
                if (activationEvt?.Instance == instance)
                    return activationEvt;

                currentNode = currentNode.ParentNode;
            }

            return null;
        }

        internal ConceptInstance GetValue(ConceptInstance instance, ParamDefinedEvent parameter)
        {
            return GetValue(instance, parameter, getCurrentNode());
        }

        internal ConceptInstance GetValue(ConceptInstance instance, Concept2 property)
        {
            return GetValue(instance, property, getCurrentNode());
        }


        internal static ConceptInstance GetValue(ConceptInstance instance, ParamDefinedEvent parameter, BeamNode node)
        {
            var currentNode = node;
            while (currentNode != null)
            {
                var substitutionEvent = currentNode.Evt as PropertySetEvent;
                var propertyMatch = substitutionEvent?.Target.Property == parameter.Property || parameter.Property == Concept2.Something;

                if (substitutionEvent?.Target.Instance == instance && propertyMatch)
                    //TODO!!!!!!!! Check which parameter was assigned
                    return substitutionEvent.SubstitutedValue;

                currentNode = currentNode.ParentNode;
            }

            return null;
        }

        internal static ConceptInstance GetValue(ConceptInstance instance, Concept2 property, BeamNode node)
        {
            var currentNode = node;
            while (currentNode != null)
            {
                var propertySetEvt = currentNode.Evt as PropertySetEvent;
                if ((propertySetEvt?.Target.Instance == instance || propertySetEvt?.Target.Concept == instance.Concept) && propertySetEvt.Target.Property == property)
                    return propertySetEvt.SubstitutedValue;

                currentNode = currentNode.ParentNode;
            }

            return null;
        }

        internal Dictionary<Concept2, ConceptInstance> GetPropertyValues(ConceptInstance instance)
        {
            return GetPropertyValues(instance, getCurrentNode());
        }

        internal static Dictionary<Concept2, ConceptInstance> GetPropertyValues(ConceptInstance instance, BeamNode node)
        {
            var result = new Dictionary<Concept2, ConceptInstance>();

            var currentNode = node;
            while (currentNode != null)
            {
                var propertySetEvt = currentNode.Evt as PropertySetEvent;
                if ((propertySetEvt?.Target.Instance == instance || propertySetEvt?.Target.Concept == instance.Concept) && !result.ContainsKey(propertySetEvt.Target.Property))
                    //reflect only last value (the freshest one)
                    result.Add(propertySetEvt.Target.Property, propertySetEvt.SubstitutedValue);

                currentNode = currentNode.ParentNode;
            }

            return result;
        }

        internal static IEnumerable<string> GetDescriptions(Concept2 concept, BeamNode node)
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

        #region Policy implementation

        private void tryActivateKnownConcepts(InputPhraseEvent evt)
        {
            foreach (var concept in GetConcepts())
            {
                var scoreEvent = new InputPhraseScoreEvent(evt, concept);
                var score = GetScore(scoreEvent);
                if (score < Configuration.ConceptActivationThreshold)
                    continue;

                Push(scoreEvent);
                Push(new InstanceActivationEvent(evt, new ConceptInstance(concept)));
                Pop();
                Pop();
            }
        }

        private void tryActivateUnknownPhrase(InputPhraseEvent evt)
        {
            Push(new UnknownPhraseEvent(evt));
            Pop();
        }

        private void tryAsSubstitution(CompleteInstanceEvent evt)
        {
            foreach (var request in GetFreeSubstitutionRequests())
            {
                if (!IsSatisfiedBy(request, evt.Instance))
                    //substitution is not possible
                    continue;

                var scoreEvt = new DistanceScoreEvt(request, evt);
                Push(scoreEvt);
                Push(new PropertySetEvent(request, evt));
                Pop();
                Pop();
            }
        }

        private void tryAsSubstitution(UnknownPhraseEvent evt)
        {
            var substitutionRequests = GetFreeSubstitutionRequests();
            foreach (var request in substitutionRequests)
            {
                var scoreEvt = new DistanceScoreEvt(request, evt);
                Push(scoreEvt);
                Push(new UnknownPhraseSubstitutionEvent(request, evt));
                Pop();
                Pop();
            }
        }

        private void trySubstituteBy(SubstitutionRequestEvent evt, IEnumerable<UnknownPhraseEvent> unknownPhrases)
        {
            foreach (var unknownPhrase in unknownPhrases)
            {
                var scoreEvt = new DistanceScoreEvt(evt, unknownPhrase);
                Push(scoreEvt);
                Push(new UnknownPhraseSubstitutionEvent(evt, unknownPhrase));
                Pop();
                Pop();
            }
        }

        private void trySubstituteBy(SubstitutionRequestEvent evt, IEnumerable<CompleteInstanceEvent> completeInstances)
        {
            foreach (var freeInstance in completeInstances)
            {
                if (!IsSatisfiedBy(evt, freeInstance.Instance))
                    continue;

                var scoreEvt = new DistanceScoreEvt(evt, freeInstance);
                Push(scoreEvt);
                Push(new PropertySetEvent(evt, freeInstance));
                Pop();
                Pop();
            }
        }

        private void tryPushComplete(ConceptInstance instance)
        {
            var activation = GetInstanceActivation(instance);
            if (activation == null)
                //only activated instances can fire completition
                return;

            var parameters = GetParameterDefinitions(instance);
            foreach (var parameter in parameters)
            {
                var value = GetValue(instance, parameter);
                if (value == null)
                    //instance is not complete yet
                    return;
            }

            Push(new CompleteInstanceEvent(instance));
        }

        private IEnumerable<SubstitutionRequestEvent> pushFreeParameterSubstitutionRequests(InstanceActivationEvent evt)
        {
            var requests = new List<SubstitutionRequestEvent>();
            var targetDefinitions = GetParameterDefinitions(evt.Instance);
            foreach (var targetDefinition in targetDefinitions)
            {
                var request = new SubstitutionRequestEvent(evt.Instance, targetDefinition);
                requests.Add(request);
                Push(request);
                //no pops because all the parameters will be requested in a serie
            }

            return requests;
        }

        #endregion
    }
}
