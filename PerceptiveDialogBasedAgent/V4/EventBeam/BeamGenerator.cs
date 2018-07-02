using KnowledgeDialog.Dialog;
using PerceptiveDialogBasedAgent.V2;
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

        /// <summary>
        /// TODO this should not be static
        /// </summary>
        private static readonly Dictionary<string, double> _featureScores = new Dictionary<string, double>();

        internal BeamGenerator()
        {
            //add root node
            _beam.Add(new BeamNode());
        }

        internal Dictionary<ConceptInstance, string> GetPrefixingUnknowns(ConceptInstance instance)
        {
            //TODO all subvalues could be considered here
            var result = new Dictionary<ConceptInstance, string>();

            var activation = GetInstanceActivationRequest(instance);
            if (activation == null || !activation.ActivationPhrases.Any())
                return result;

            var inputs = new List<InputPhraseEvent>();
            foreach (var input in GetPrecedingEvents(getCurrentNode(), activation.ActivationPhrases.First(), true))
            {
                if (IsInputUsed(input))
                    break;

                inputs.Add(input);
            }

            if (inputs.Count > 0)
            {
                inputs.Reverse();
                result[instance] = string.Join(" ", inputs.Select(i => i.Phrase));
            }

            return result;
        }

        internal virtual void Visit(InputPhraseEvent evt)
        {
            // input phrase can create a new instance or point to some old instance
            tryActivateNewInstances();
            tryActivateProperties();

            // input can be unrecognized
            PushSelf();
        }

        internal virtual void Visit(InstanceActivationRequestEvent evt)
        {
            var requests = pushFreeParameterSubstitutionRequests(evt);
            if (!requests.Any())
                //we have got a complete instance
                Push(new InstanceActiveEvent(evt.Instance, canBeReferenced: evt.ActivationPhrases.Any(), evt));
        }

        internal virtual void Visit(InstanceActiveEvent evt)
        {
            handleOnActiveSubstitution(evt);
        }

        internal virtual void Visit(InformationPartEvent evt)
        {
            if (evt.IsFilled)
            {
                var target = new PropertySetTarget(evt.Subject, evt.Property);
                Push(new CloseEvent(evt));
                Push(new PropertySetEvent(target, evt.Value));
            }
            else
            {
                //try to fill by some free active instance
                var completeInstances = GetAvailableActiveInstances();
                tryToFillBy(evt, completeInstances);

                //or keep unfilled
                PushSelf();
            }
        }

        internal virtual void Visit(PropertySetEvent evt)
        {
            ConceptInstance reportedInstance = null;
            if (evt.Target.Instance != null && evt.Target.Property != Concept2.OnSetListener)
                reportedInstance = GetValue(evt.Target.Instance, Concept2.OnSetListener);

            pushCompleteIfPossible(evt.Target.Instance, allowTurnReactivation: false);

            if (reportedInstance != null)
                Push(new InstanceActiveEvent(reportedInstance, false));
        }

        internal virtual void Visit(InstanceReferencedEvent evt)
        {
            //notice that we should not care about multiple activations - because of multiple mentions
            pushCompleteIfPossible(evt.Instance, requireActivationRequest: false);
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

        internal virtual void Visit(InformationReportEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(TurnStartEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(TurnEndEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(FrameEvent evt)
        {
            // nothing to do by default
        }

        internal virtual void Visit(CloseEvent evt)
        {
            // nothing do to do by default
        }

        internal virtual void Visit(OutputEvent evt)
        {
            // nothing do to do by default
        }

        internal virtual void Visit(ExportEvent evt)
        {
            // nothing do to do by default
        }

        internal virtual void Visit(EventBase evt)
        {
            throw new NotSupportedException("Unknown event");
        }


        protected virtual void handleOnActiveSubstitution(InstanceActiveEvent evt)
        {
            // we have a complete instance - lets look where it can be put into
            tryAsSubstitution(evt);
            tryAsPropertyValueActivation(evt);

            //instance can be left without being substituted anywhere
            PushSelf();
        }

        #region Beam Operations

        internal IEnumerable<InputPhraseEvent[]> GetInputActivationHypotheses()
        {
            var phrases = GetAvailableInputPhrases().Take(2).ToArray();

            for (var i = 0; i < phrases.Length; ++i)
            {
                yield return phrases.Take(i + 1).Reverse().ToArray();
            }
        }

        internal IEnumerable<Concept2> GetDefinedConcepts()
        {
            return GetAllEvents<ConceptDefinedEvent>(getCurrentNode()).Select(e => e.Concept);
        }

        protected IEnumerable<InformationPartEvent> GetIncompleteTurnInformationParts()
        {
            return GetFrameEvents<InformationPartEvent>(getCurrentNode(), turnLimited: true).Where(p => !p.IsFilled);
        }

        protected IEnumerable<InstanceActiveEvent> GetAvailableActiveInstances()
        {
            return GetFrameEvents<InstanceActiveEvent>(getCurrentNode(), turnLimited: true);
        }

        protected IEnumerable<InputPhraseEvent> GetAvailableInputPhrases()
        {
            var node = getCurrentNode();
            return GetFrameEvents<InputPhraseEvent>(node, turnLimited: true);
        }

        internal IEnumerable<InstanceActivationRequestEvent> GetTurnActivationRequestedInstances()
        {
            var node = getCurrentNode();
            return GetFrameEvents<InstanceActivationRequestEvent>(node, turnLimited: true);
        }

        internal IEnumerable<ConceptInstance> GetInstances()
        {
            var node = getCurrentNode();

            var instances = new HashSet<ConceptInstance>();
            instances.UnionWith(GetAllEvents<InstanceActivationRequestEvent>(node).Select(i => i.Instance));
            instances.UnionWith(GetAllEvents<InstanceActiveEvent>(node).Select(i => i.Instance));

            return instances;
        }

        internal IEnumerable<InstanceActiveEvent> GetInputActivatedInstances()
        {
            var node = getCurrentNode();
            return GetAllEvents<InstanceActiveEvent>(node).Where(i => i.Request?.ActivationPhrases.Length > 0);
        }

        internal IEnumerable<ConceptInstance> GetValues(ConceptInstance instance, Concept2 property)
        {
            var node = getCurrentNode();

            return GetAllEvents<PropertySetEvent>(node).Where(s => s.Target.Instance == instance && s.Target.Property == property).Select(s => s.SubstitutedValue);
        }

        protected IEnumerable<T> GetPrecedingEvents<T>(BeamNode node, T startingEvt, bool turnLimited)
            where T : EventBase
        {
            var currentNode = node;

            while (currentNode != null)
            {
                if (currentNode.Evt == startingEvt)
                    break;

                currentNode = currentNode.ParentNode;
            }
            currentNode = currentNode.ParentNode;
            while (currentNode != null)
            {
                var evt = currentNode.Evt;
                if (turnLimited && evt is TurnStartEvent)
                    yield break;

                if (evt is T searchedEvent)
                    yield return searchedEvent;

                currentNode = currentNode.ParentNode;
            }
        }

        internal bool IsInputUsed(InputPhraseEvent input)
        {
            return GetAllEvents<InstanceActivationRequestEvent>(getCurrentNode()).Where(r => r.ActivationPhrases.Contains(input)).Any();
        }

        protected IEnumerable<T> GetFrameEvents<T>(BeamNode node, bool turnLimited)
            where T : EventBase
        {
            var closedEvents = new HashSet<T>();
            var closedFrames = new HashSet<FrameEvent>();

            var hasTurnStart = false;
            var currentNode = node;
            while (currentNode != null)
            {
                var evt = currentNode.Evt;
                if (evt is CloseEvent closingEvent)
                {
                    if (closingEvent.ClosedEvent is T closedEvent)
                        closedEvents.Add(closedEvent);

                    if (closingEvent.ClosedEvent is FrameEvent cfEvt)
                        closedFrames.Add(cfEvt);
                }

                if (hasTurnStart && evt is TurnEndEvent)
                    //read all events till end of the previous
                    break;

                if (turnLimited && evt is TurnStartEvent)
                    hasTurnStart = true;

                if (evt is T searchedEvent && !closedEvents.Contains(searchedEvent))
                    yield return searchedEvent;

                if (evt is FrameEvent fEvt && !closedFrames.Contains(fEvt))
                    //we found boundary of an active frame
                    break;

                currentNode = currentNode.ParentNode;
            }
        }

        public IEnumerable<T> GetTurnEvents<T>(int precedingTurns = 0)
            where T : EventBase
        {
            var closedEvents = new HashSet<T>();

            var currentNode = getCurrentNode();
            var currentTurn = 0;
            while (currentNode != null)
            {
                var evt = currentNode.Evt;
                if (evt is CloseEvent closingEvent)
                {
                    if (closingEvent.ClosedEvent is T closedEvent)
                        closedEvents.Add(closedEvent);
                }

                if (evt is TurnStartEvent)
                    currentTurn += 1;

                if (currentTurn == precedingTurns)
                {
                    if (evt is T searchedEvent && !closedEvents.Contains(searchedEvent))
                        yield return searchedEvent;
                }
                else if (currentTurn > precedingTurns)
                {
                    break;
                }

                currentNode = currentNode.ParentNode;
            }
        }

        internal IEnumerable<EventBase> GetTurnEvents(BeamNode node)
        {
            return GetFrameEvents<EventBase>(node, true);
        }

        public bool IsProperty(Concept2 concept)
        {
            return GetAllEvents<PropertySetEvent>(getCurrentNode()).Where(p => p.Target.Property == concept).Any();
        }


        protected IEnumerable<T> GetAllEvents<T>(BeamNode node)
          where T : EventBase
        {
            var closedEvents = new HashSet<T>();

            var currentNode = node;
            while (currentNode != null)
            {
                var evt = currentNode.Evt;
                if (evt is CloseEvent closingEvent)
                {
                    if (closingEvent.ClosedEvent is T closedEvent)
                        closedEvents.Add(closedEvent);
                }


                if (evt is T searchedEvent && !closedEvents.Contains(searchedEvent))
                    yield return searchedEvent;

                currentNode = currentNode.ParentNode;
            }
        }

        public Concept2 DefineConcept(string conceptName)
        {
            var concept = Concept2.From(conceptName);
            return DefineConcept(concept);
        }

        public Concept2 DefineConcept(Concept2 concept)
        {
            PushToAll(new ConceptDefinedEvent(concept));
            return concept;
        }

        public ConceptInstance DefineConceptInstance(string conceptName)
        {
            var concept = DefineConcept(conceptName);
            var instance = new ConceptInstance(concept);

            return instance;
        }

        public ParamDefinedEvent DefineParameter(Concept2 concept, Concept2 parameter, ConceptInstance pattern)
        {
            var result = new ParamDefinedEvent(concept, parameter, pattern);
            PushToAll(result);
            return result;
        }

        public void AddDescription(Concept2 concept, string description)
        {
            PushToAll(new ConceptDescriptionEvent(concept, description));
        }

        public void SetValue(Concept2 concept, Concept2 property, ConceptInstance value)
        {
            PushToAll(new PropertySetEvent(new PropertySetTarget(concept, property), value));
        }

        public void SetValue(ConceptInstance instance, Concept2 property, ConceptInstance value)
        {
            PushToAll(new PropertySetEvent(new PropertySetTarget(instance, property), value));
        }

        public void PushInput(string input)
        {
            PushToAll(new InputPhraseEvent(input));
        }

        public void LimitBeam(int count)
        {
            if (_beam.Count < count)
                //there is nothing to do
                return;

            var bestNodes = GetRankedNodes().Take(count).Select(r => r.Value).ToArray();
            _beam.Clear();
            _beam.UnionWith(bestNodes);

        }

        /// <summary>
        /// Pushes to all beam branches
        /// </summary>
        public void PushToAll(EventBase evt)
        {
            var isRootPush = _layers.Count == 0;
            var pushNodes = isRootPush ? _beam.ToArray() : new[] { getCurrentNode() };

            foreach (var node in pushNodes)
            {
                try
                {
                    if (isRootPush)
                        _layers.Push(new List<BeamNode>() { node });

                    Push(evt);
                }
                finally
                {
                    if (isRootPush)
                        _layers.Clear();
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

        protected virtual bool CanSubstituteSubject(InformationPartEvent information, ConceptInstance subject)
        {
            if (information.Subject != null)
                //overwrites are not allowed
                return false;

            if (information.Value == null)
                //incomplete subject assignments are not allowed
                //TODO is it important to have incomplete assignments enabled ?
                return false;

            if (subject.Concept == information.Value?.Concept || subject.Concept == information.Property)
                //disable self assignments
                return false;

            //TODO check for patterns
            return true;
        }

        protected virtual bool CanSubstituteValue(InformationPartEvent information, ConceptInstance value)
        {
            if (information.Value != null)
                //incomplete subject assignments are not allowed
                //TODO is it important to have incomplete assignments enabled ?
                return false;

            if (value.Concept == information.Subject?.Concept || value.Concept == information.Property)
                //disable self assignments
                return false;

            if (information.Property == Concept2.Property)
                return IsProperty(value.Concept);

            //TODO check for patterns
            return true;
        }

        public static void AddFeatureScore(string feature, double score)
        {
            _featureScores.TryGetValue(feature, out var oldScore);
            _featureScores[feature] = score + oldScore;
        }

        public static double GetScore(TracedScoreEventBase scoreEvt, BeamNode node)
        {
            //TODO recognition learning belongs here
            var featureScore = 0.0;
            foreach (var feature in scoreEvt.GenerateFeatures(node))
            {
                if (_featureScores.TryGetValue(feature, out var score))
                {
                    featureScore += score;
                    break; //TODO think more about feature semantic
                }
            }
            return scoreEvt.GetDefaultScore(node) + featureScore;
        }

        protected double GetScore(TracedScoreEventBase scoreEvt)
        {
            return GetScore(scoreEvt, getCurrentNode());
        }

        protected double GetScore(BeamNode node)
        {
            var accumulator = 0.0;
            var currentNode = node;
            while (currentNode != null)
            {
                if (currentNode.Evt is TracedScoreEventBase scoreEvt)
                    accumulator += GetScore(scoreEvt, node);

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

        protected FrameEvent GetOpenGoal()
        {
            return GetOpenGoal(getCurrentNode());
        }

        protected IEnumerable<ParamDefinedEvent> GetParameterDefinitions(ConceptInstance instance)
        {
            return GetParameterDefinitions(instance, getCurrentNode());
        }

        protected FrameEvent GetOpenGoal(BeamNode node)
        {
            var currentNode = node;
            var closedEvents = new HashSet<EventBase>();
            while (currentNode != null)
            {
                if (currentNode.Evt is CloseEvent closeEvt)
                    closedEvents.Add(closeEvt.ClosedEvent);

                if (currentNode.Evt is FrameEvent goalEvent)
                {
                    if (closedEvents.Contains(goalEvent))
                        //TODO think more about goal closing semantic
                        return null;

                    return goalEvent;
                }

                currentNode = currentNode.ParentNode;
            }

            return null;
        }

        internal IEnumerable<EventBase> GetTurnEvents()
        {
            return GetTurnEvents(getCurrentNode());
        }

        internal IEnumerable<EventBase> GetPreviousTurnEvents()
        {
            return GetPreviousTurnEvents(getCurrentNode());
        }

        internal IEnumerable<EventBase> GetPreviousTurnEvents(BeamNode node)
        {
            var currentNode = node;
            while (currentNode != null)
            {
                if (currentNode.Evt is TurnStartEvent)
                    break;

                currentNode = currentNode.ParentNode;
            }

            return GetTurnEvents(currentNode.ParentNode);
        }


        internal static InputPhraseEvent[] GetSufixPhrases(InputPhraseEvent phrase, int ngramLimitCount, BeamNode node)
        {
            throw new NotImplementedException();
        }

        internal static InputPhraseEvent[] GetPrefixPhrases(InputPhraseEvent phrase, int prefixLength, BeamNode node)
        {
            var result = new List<InputPhraseEvent>();
            var currentNode = node;

            //find node where prefix starts
            while (currentNode != null && currentNode.Evt != phrase)
                currentNode = currentNode.ParentNode;

            while (currentNode != null)
            {
                if (currentNode.Evt is TurnStartEvent)
                    break;

                if (currentNode.Evt is InputPhraseEvent prefixPhrase)
                {
                    result.Add(prefixPhrase);
                    if (result.Count >= prefixLength)
                        break;
                }

                currentNode = currentNode.ParentNode;
            }

            result.Reverse();
            return result.ToArray();
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

        internal InstanceActiveEvent GetTurnInstanceActivation(ConceptInstance instance)
        {
            return GetFrameEvents<InstanceActiveEvent>(getCurrentNode(), turnLimited: true).Where(e => e.Instance == instance).FirstOrDefault();
        }

        internal IEnumerable<InstanceActiveEvent> GetInstanceActivations()
        {
            return GetFrameEvents<InstanceActiveEvent>(getCurrentNode(), turnLimited: false);
        }


        internal InstanceActivationRequestEvent GetInstanceActivationRequest(ConceptInstance instance)
        {
            return GetInstanceActivationRequest(instance, getCurrentNode());
        }

        internal static InstanceActivationRequestEvent GetInstanceActivationRequest(ConceptInstance instance, BeamNode node)
        {
            var currentNode = node;
            while (currentNode != null)
            {
                var activationEvt = currentNode.Evt as InstanceActivationRequestEvent;
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

        internal Dictionary<Concept2, ConceptInstance> GetPropertyValues(ConceptInstance instance, bool includeInheritedProps = true, bool includeInstanceProps = true)
        {
            return GetPropertyValues(instance, getCurrentNode(), includeInheritedProps, includeInstanceProps);
        }

        internal static Dictionary<Concept2, ConceptInstance> GetPropertyValues(ConceptInstance instance, BeamNode node, bool includeInheritedProps = true, bool includeInstanceProps = true)
        {
            var result = new Dictionary<Concept2, ConceptInstance>();

            var currentNode = node;
            while (currentNode != null)
            {
                var propertySetEvt = currentNode.Evt as PropertySetEvent;
                if (((propertySetEvt?.Target.Instance == instance && includeInstanceProps) || (propertySetEvt?.Target.Concept == instance.Concept && includeInheritedProps)) && !result.ContainsKey(propertySetEvt.Target.Property))
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

        private void tryActivateNewInstances()
        {
            var existingInstances = GetTurnActivationRequestedInstances();
            var existingConcepts = new HashSet<Concept2>(existingInstances.Select(e => e.Instance.Concept));
            var concepts = GetDefinedConcepts();
            var inputPhrasesHypotheses = GetInputActivationHypotheses();

            foreach (var inputPhrases in inputPhrasesHypotheses)
            {
                foreach (var concept in concepts)
                {
                    if (existingConcepts.Contains(concept))
                        // prevent concept multi activation
                        continue;

                    var scoreEvent = new InputPhraseScoreEvent(inputPhrases, concept);
                    var score = GetScore(scoreEvent);
                    if (score < Configuration.ConceptActivationThreshold)
                        // performance optimization
                        continue;

                    foreach (var inputPhrase in inputPhrases)
                        Push(new CloseEvent(inputPhrase));

                    Push(scoreEvent);
                    Push(new InstanceActivationRequestEvent(inputPhrases, new ConceptInstance(concept)));
                    Pop();
                    Pop();

                    foreach (var inputPhrase in inputPhrases)
                        Pop();
                }
            }
        }

        private void tryActivateProperties()
        {
            var existingInstances = GetTurnActivationRequestedInstances();
            var existingConcepts = new HashSet<Concept2>(existingInstances.Select(e => e.Instance.Concept));
            var concepts = GetDefinedConcepts();
            var inputPhrasesHypotheses = GetInputActivationHypotheses();

            foreach (var inputPhrases in inputPhrasesHypotheses)
            {
                foreach (var concept in concepts)
                {
                    if (existingConcepts.Contains(concept))
                        // prevent concept multi activation
                        continue;

                    if (!isKnownProperty(concept))
                        //don't spam with meaningless attempts
                        continue;

                    var scoreEvent = new InputPhraseScoreEvent(inputPhrases, concept);
                    var score = GetScore(scoreEvent);
                    if (score < Configuration.ConceptActivationThreshold)
                        // performance optimization
                        continue;

                    foreach (var inputPhrase in inputPhrases)
                        Push(new CloseEvent(inputPhrase));

                    Push(scoreEvent);
                    Push(new InformationPartEvent(null, concept, null));
                    Pop();
                    Pop();

                    foreach (var inputPhrase in inputPhrases)
                        Pop();
                }
            }
        }

        private bool isKnownProperty(Concept2 concept)
        {
            var hierarchy = GetClassHierarchy();
            var hierarchyChain = new HashSet<Concept2>();

            var currentConcept = concept;
            while (currentConcept != null && hierarchyChain.Add(currentConcept))
            {
                hierarchy.TryGetValue(currentConcept, out currentConcept);
            }

            return GetAllEvents<PropertySetEvent>(getCurrentNode()).Where(s => s.Target.Property == Concept2.HasPropertyValue).Any();
        }

        internal Dictionary<Concept2, Concept2> GetClassHierarchy()
        {
            var hierarchy = new Dictionary<Concept2, Concept2>();
            foreach (var propertySet in GetAllEvents<PropertySetEvent>(getCurrentNode()).Where(s => s.Target.Property == Concept2.InstanceOf))
            {
                if (propertySet.Target.Concept == null)
                    continue;

                hierarchy[propertySet.Target.Concept] = propertySet.Target.Concept;
            }
            return hierarchy;
        }

        private void tryAsSubstitution(InstanceActiveEvent evt)
        {
            var distancePenalty = 0;
            foreach (var part in GetIncompleteTurnInformationParts())
            {
                tryToFillBy(part, evt, distancePenalty);

                distancePenalty += 1;
            }
        }

        private void tryAsPropertyValueActivation(InstanceActiveEvent evt)
        {
            var properties = GetPointingProperties(evt.Instance.Concept);
            foreach (var property in properties)
            {
                //try to interpret as an implicit property
                var part = new InformationPartEvent(null, property, null);
                tryFillValueBy(part, evt, 0);
            }
        }

        private IEnumerable<Concept2> GetPointingProperties(Concept2 concept)
        {
            var definedConcepts = GetDefinedConcepts();
            var propertySets = GetAllEvents<PropertySetEvent>(getCurrentNode());
            var result = new HashSet<Concept2>();
            foreach (var propertySet in propertySets)
            {
                if (!definedConcepts.Contains(propertySet.Target.Property))
                    continue;

                if (propertySet.SubstitutedValue.Concept == concept)
                    result.Add(propertySet.Target.Property);
            }
            return result;
        }

        private void tryToFillBy(InformationPartEvent evt, InstanceActiveEvent instanceActiveEvt, int distancePenalty)
        {
            tryFillSubjectBy(evt, instanceActiveEvt, distancePenalty);
            tryFillValueBy(evt, instanceActiveEvt, distancePenalty);
        }

        private void tryToFillBy(InformationPartEvent evt, IEnumerable<InstanceActiveEvent> completeInstances)
        {
            var distancePenalty = 0;

            foreach (var freeInstance in completeInstances)
            {
                tryToFillBy(evt, freeInstance, distancePenalty);
                distancePenalty += 1;
            }
        }

        private void tryFillValueBy(InformationPartEvent evt, InstanceActiveEvent instanceActiveEvt, int distancePenalty)
        {
            if (!CanSubstituteValue(evt, instanceActiveEvt.Instance))
                return;

            var filledEvt = evt.SubstituteValue(instanceActiveEvt.Instance);
            Push(new PropertySetScoreEvent(filledEvt, distancePenalty));
            Push(new CloseEvent(instanceActiveEvt));
            Push(new CloseEvent(evt));
            Push(filledEvt);
            Pop();
            Pop();
            Pop();
            Pop();
        }

        private void tryFillSubjectBy(InformationPartEvent evt, InstanceActiveEvent instanceActiveEvt, int distancePenalty)
        {
            if (!CanSubstituteSubject(evt, instanceActiveEvt.Instance))
                return;

            var filledEvt = evt.SubstituteSubject(instanceActiveEvt.Instance);
            // subject activation is not closed 
            Push(new CloseEvent(evt));
            Push(filledEvt);
            Pop();
            Pop();
        }

        private void pushCompleteIfPossible(ConceptInstance instance, bool allowTurnReactivation = true, bool requireActivationRequest = true)
        {
            var request = GetInstanceActivationRequest(instance);
            if (request == null && requireActivationRequest)
                // only activated instances can fire completition
                return;

            if (!allowTurnReactivation && GetTurnInstanceActivation(instance) != null)
                // we dont want to reactivate instance again
                return;

            var parameters = GetParameterDefinitions(instance);
            foreach (var parameter in parameters)
            {
                var value = GetValue(instance, parameter);
                if (value == null)
                    //instance is not complete yet
                    return;
            }

            var canBeReferenced = !requireActivationRequest || request.ActivationPhrases.Any();
            Push(new InstanceActiveEvent(instance, canBeReferenced, request));
        }

        private IEnumerable<InformationPartEvent> pushFreeParameterSubstitutionRequests(InstanceActivationRequestEvent evt)
        {
            var requests = new List<InformationPartEvent>();
            var targetDefinitions = GetParameterDefinitions(evt.Instance);
            var filteredTargetDefinitions = targetDefinitions.Where(t => GetValue(evt.Instance, t.Property) == null).ToArray();

            foreach (var targetDefinition in filteredTargetDefinitions)
            {
                var request = new InformationPartEvent(evt.Instance, targetDefinition.Property, null);
                requests.Add(request);
                Push(request);
                //no pops because all the parameters will be requested in a serie
            }

            return requests;
        }

        #endregion
    }
}
