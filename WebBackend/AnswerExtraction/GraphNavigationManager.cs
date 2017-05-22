using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Parsing;
using KnowledgeDialog.GraphNavigation;

using KnowledgeDialog.DataCollection.MachineActs;

using KnowledgeDialog.DataCollection;

namespace WebBackend.AnswerExtraction
{
    enum NavigationDialogState { ExpectsEntityLabelHint };

    class GraphNavigationManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        public bool HadInformativeInput { get; private set; }

        public bool CanBeCompleted { get; private set; }

        private readonly Random _rnd = new Random();

        private readonly string[] _entityPhrases;

        private readonly ILinker _linker;

        private readonly NavigationData _data;

        private readonly HashSet<NavigationDialogState> _visitedStates = new HashSet<NavigationDialogState>();

        private EntityNavigationData _currentTopic;

        private NavigationDialogState _currentState;

        internal GraphNavigationManager(NavigationData data, IEnumerable<string> entityPhrases, ILinker linker)
        {
            _data = data;
            _linker = linker;
            _entityPhrases = entityPhrases.ToArray();
        }

        /// </inheritdoc>
        public override ResponseBase Initialize()
        {
            setNewTopic();

            return new WelcomeWithEntityLabelRequestAct(_currentTopic.Phrase);
        }

        private void setNewTopic()
        {
            _currentTopic = getRandomTopic();

            _visitedStates.Clear();
            setState(NavigationDialogState.ExpectsEntityLabelHint);
        }

        /// </inheritdoc>
        public override ResponseBase Input(ParsedUtterance utterance)
        {
            var act = Factory.GetBestDialogAct(utterance);
            switch (_currentState)
            {
                case NavigationDialogState.ExpectsEntityLabelHint:
                    return entityLabelHintInput(utterance, act);

                default:
                    throw new NotImplementedException("Unknown state");
            }
        }

        private ResponseBase entityLabelHintInput(ParsedUtterance utterance, DialogActBase act)
        {
            if (act.IsAffirm)
                return new ContinueAct();

            if (act.IsDontKnow || act.IsNegate)
                return new NotUsefulContinuationAct(startNewFrame());

            if (act.IsAdvice)
            {
                var advice = act as AdviceAct;
                _currentTopic.AddLabelCandidate(advice.Answer.OriginalSentence);
            }

            HadInformativeInput = utterance.Words.Count() > 2;
            CanBeCompleted = true;

            // now we are just collecting simple data without any negotiation
            return new UsefulContinuationAct(startNewFrame());

            var linkedUtterance = _linker.LinkUtterance(utterance.OriginalSentence);
            if (linkedUtterance.Entities.Count() != 0)
                throw new NotImplementedException("maybe noise, relevant entities, etc");


            throw new NotImplementedException("parse entities");
        }

        private ResponseBase startNewFrame()
        {
            var randomStateIterationCount = 10;
            var possibleStates = Enum.GetValues(typeof(NavigationDialogState));
            for (var i = 0; i < randomStateIterationCount + 1; ++i)
            {
                var randomState = (NavigationDialogState)possibleStates.GetValue(_rnd.Next(possibleStates.Length));
                if (setState(randomState))
                    break;

                if (i == randomStateIterationCount)
                    //we covered the topic enough, try different one
                    setNewTopic();
            }

            // provide an output according to current state
            // notice, that the state change is made in advance - (expects label --> we are asking for labele here)
            switch (_currentState)
            {
                case NavigationDialogState.ExpectsEntityLabelHint:
                    return new RequestEntityLabelAct(_currentTopic.Phrase);
            }

            throw new NotImplementedException();
        }

        private EntityNavigationData getRandomTopic()
        {
            var rndIndex = _rnd.Next(_entityPhrases.Length);
            var phrase = _entityPhrases[rndIndex];

            return _data.GetData(phrase);
        }

        private bool setState(NavigationDialogState state)
        {
            _currentState = state;
            return _visitedStates.Add(state);
        }
    }
}
