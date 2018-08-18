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
using WebBackend.Dataset;
using KnowledgeDialog.Knowledge;

namespace WebBackend.AnswerExtraction
{
    enum NavigationDialogState { ExpectsEntityLabelHint, ExpectsEdgeHint };

    class GraphNavigationManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        public bool HadInformativeInput { get; private set; }

        public bool CanBeCompleted { get; private set; }

        public int SuccessCode { get; private set; }

        private static readonly string _edgeTag = "[edge]";

        private static readonly string _label1Tag = "[label1]";

        private static readonly string _label2Tag = "[label2]";

        private readonly Random _rnd = new Random();

        private readonly string[] _entityPhrases;

        private readonly string[] _edges;

        private static readonly List<Tuple<FreebaseEntry, Edge, FreebaseEntry>[]> _edgeRepresentants = new List<Tuple<FreebaseEntry, Edge, FreebaseEntry>[]>();

        private readonly ILinker _linker;

        private readonly NavigationData _data;

        private readonly HashSet<NavigationDialogState> _visitedStates = new HashSet<NavigationDialogState>();

        private EntityNavigationData _currentEntityTopic;

        private EdgeNavigationData _currentEdgeTopic;

        private Tuple<FreebaseEntry, Edge, FreebaseEntry> _currentEdgeRepresentant;

        private string _currentEdgeHypothesis;

        private string _initialEdgeHypothesis;

        private NavigationDialogState _currentState;

        internal GraphNavigationManager(NavigationData data, IEnumerable<string> entityPhrases, ILinker linker)
        {
            _data = data;
            _linker = linker;
            _entityPhrases = entityPhrases.ToArray();
            _edges = new string[]
            {
                "/location/country/form_of_government",
                "/biology/organism_classification/higher_classification",
                "/people/person/profession",
                "/education/field_of_study/students_majoring",
                "/sports/school_sports_team/school",
                "/music/genre/albums",
                "/medicine/disease/causes",
                "/tv/tv_program/regular_cast",
                "/people/person/nationality",
                "/film/film_genre/films_in_this_genre",
                "/music/artist/genre",
                "/business/business_operation/industry",
                "/common/topic/notable_types",
                "/location/location/contains",
            };

            sampleEdgeRepresentants(20);
        }

        /// </inheritdoc>
        public override ResponseBase Initialize()
        {
            setNewTopic();

            switch (_currentState)
            {
                case NavigationDialogState.ExpectsEdgeHint:
                    parseCurrentEdgeRepresentant(out string label1, out string requestedRelation, out string edgeHypothesisFormat, out string label2);
                    return new WelcomeWithEdgeRequestAct(label1, requestedRelation, edgeHypothesisFormat, label2);

                case NavigationDialogState.ExpectsEntityLabelHint:
                    return new WelcomeWithEntityLabelRequestAct(_currentEntityTopic.Phrase);
            }

            throw new NotImplementedException();
        }

        private void setNewTopic()
        {
            _visitedStates.Clear();
            setRandomTopic();
        }

        /// </inheritdoc>
        public override ResponseBase Input(ParsedUtterance utterance)
        {
            HadInformativeInput = false;
            CanBeCompleted = false;

            var act = Factory.GetBestDialogAct(utterance);
            switch (_currentState)
            {
                case NavigationDialogState.ExpectsEntityLabelHint:
                    return entityLabelHintInput(utterance, act);

                case NavigationDialogState.ExpectsEdgeHint:
                    return edgeHintInput(utterance, act);

                default:
                    throw new NotImplementedException("Unknown state");
            }
        }

        private ResponseBase edgeHintInput(ParsedUtterance utterance, DialogActBase act)
        {
            if (act.IsDontKnow)
                return new NotUsefulContinuationAct(startNewFrame());

            var edgeHint = utterance.OriginalSentence;
            var intextFormat = getIntextFormat(edgeHint);

            if (act.IsNegate && intextFormat == null)
                return new RequestWordingAct();

            if (act.IsAffirm)
            {
                //force users to really output something
                if (_initialEdgeHypothesis != _currentEdgeHypothesis)
                {
                    HadInformativeInput = true;
                    CanBeCompleted = true;
                    _currentEdgeTopic.AddExpression(_currentEdgeHypothesis);
                    return new UsefulContinuationAct(startNewFrame());
                }
                else
                {
                    return new NotUsefulContinuationAct(startNewFrame());
                }
            }


            if (intextFormat == null)
            {
                var taggedInput = tagEdgeHint(edgeHint);
                if (getWords(taggedInput).Length < 2)
                    return new DontUnderstandAct();

                if (!taggedInput.Contains(_label1Tag))
                    return new NoConnectionToEntityAct(entityInfo(_currentEdgeRepresentant.Item1));

                if (!taggedInput.Contains(_label2Tag))
                    return new NoConnectionToEntityAct(entityInfo(_currentEdgeRepresentant.Item3));
            }

            if (intextFormat.Replace(_label1Tag, "").Replace(_label2Tag, "").Trim() == "")
                return new NotUsefulContinuationAct(startNewFrame());

            _currentEdgeHypothesis = intextFormat;
            parseCurrentEdgeRepresentant(out string label1, out string requestedRelation, out string edgeHypothesis, out string label2);
            return new EdgeConfirmationRequestAct(label1, intextFormat, label2);
        }

        private string getIntextFormat(string edgeHint)
        {
            var taggedHint = tagEdgeHint(edgeHint);
            if (!taggedHint.Contains(_label1Tag) || !taggedHint.Contains(_label2Tag))
                return null;

            var tag1 = _label1Tag;
            var tag2 = _label2Tag;

            var pos1 = taggedHint.IndexOf(tag1);
            var pos2 = taggedHint.IndexOf(tag2);

            if (pos1 > pos2)
            {
                var tmp = pos1;
                pos1 = pos2;
                pos2 = tmp;

                tag1 = _label2Tag;
                tag2 = _label1Tag;
            }

            pos2 += _label2Tag.Length;
            var intext = taggedHint.Substring(pos1, pos2 - pos1);
            var intextFormat = intext.Replace(_label1Tag, "{0}").Replace(_label2Tag, "{1}");

            return intextFormat;
        }

        private string tagEdgeHint(string edgeHint)
        {
            edgeHint = edgeHint.ToLowerInvariant();
            parseCurrentEdgeRepresentant(out string label1, out string requestedRelation, out string questionedEdge, out string label2);
            label1 = label1.ToLowerInvariant();
            label2 = label2.ToLowerInvariant();
            questionedEdge = questionedEdge.ToLowerInvariant();

            //var taggedHint = tagWithContext(edgeHint, questionedEdge, _edgeTag, canResolveCoreference: false);
            var taggedHint = edgeHint;
            taggedHint = tagWithContext(taggedHint, label1, _label1Tag, canResolveCoreference: true);
            taggedHint = tagWithContext(taggedHint, label2, _label2Tag, canResolveCoreference: true);
            return taggedHint;
        }

        private string tagWithContext(string edgeHint, string context, string tag, bool canResolveCoreference)
        {
            //first try the exact match
            if (replaceFirstOccurrence(ref edgeHint, context, tag))
                return edgeHint;

            //next try word groups matching
            var contextWords = getWords(context);
            for (var n = contextWords.Length - 1; n > 0; --n)
            {
                foreach (var ngram in getNgrams(context, n))
                {
                    if (!UtteranceParser.IsInformativeWord(ngram))
                        continue;

                    var edgeHintCopy = edgeHint;
                    if (replaceFirstOccurrence(ref edgeHintCopy, ngram, tag)
                        && !edgeHintCopy.Contains(ngram)
                        )
                        return edgeHintCopy;
                }
            }

            return edgeHint;
        }

        private bool replaceFirstOccurrence(ref string text, string value, string replacement)
        {
            var pos = text.IndexOf(value);
            if (pos < 0)
            {
                return false;
            }
            text = text.Substring(0, pos) + replacement + text.Substring(pos + value.Length);
            return true;
        }

        private string[] getWords(string text)
        {
            return text.Split(' ');
        }

        private string[] getNgrams(string text, int n)
        {
            var words = getWords(text);

            var result = new List<string>();
            for (var i = 0; i <= words.Length - n; ++i)
            {
                var ngram = new StringBuilder();
                for (var j = 0; j < n; ++j)
                {
                    if (ngram.Length > 0)
                        ngram.Append(' ');

                    ngram.Append(words[i + j]);
                }

                result.Add(ngram.ToString());
            }

            return result.ToArray();
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
                _currentEntityTopic.AddLabelCandidate(advice.Answer.OriginalSentence);
            }

            HadInformativeInput = utterance.Words.Count() > 2;
            CanBeCompleted = true;

            // now we are just collecting simple data without any negotiation
            return new UsefulContinuationAct(startNewFrame());

            /*var linkedUtterance = _linker.LinkUtterance(utterance.OriginalSentence);
            if (linkedUtterance.Entities.Count() != 0)
                throw new NotImplementedException("maybe noise, relevant entities, etc");


            throw new NotImplementedException("parse entities");*/
        }

        private ResponseBase startNewFrame()
        {
            /*
            var randomStateIterationCount = 10;
            var possibleStates = Enum.GetValues(typeof(NavigationDialogState));
            for (var i = 0; i < randomStateIterationCount + 1; ++i)
            {
                //var randomState = (NavigationDialogState)possibleStates.GetValue(_rnd.Next(possibleStates.Length));
                var randomState = NavigationDialogState.ExpectsEdgeHint;
                if (setState(randomState))
                    break;

                if (i == randomStateIterationCount)
                    //we covered the topic enough, try different one
                    setNewTopic();
            }*/

            setNewTopic();

            // provide an output according to current state
            // notice, that the state change is made in advance - (expects label --> we are asking for labele here)
            switch (_currentState)
            {
                case NavigationDialogState.ExpectsEntityLabelHint:
                    return new RequestEntityLabelAct(_currentEntityTopic.Phrase);
                case NavigationDialogState.ExpectsEdgeHint:
                    parseCurrentEdgeRepresentant(out string label1, out string requestedRelation, out string edgeHypothesisFormat, out string label2);
                    return new EdgeRequestAct(label1, requestedRelation, edgeHypothesisFormat, label2);
            }

            throw new NotImplementedException();
        }

        private void setRandomTopic()
        {
            _currentEntityTopic = null;
            _currentEdgeTopic = null;
            _currentEdgeHypothesis = null;
            _initialEdgeHypothesis = null;

            //TODO select random state
            _currentState = NavigationDialogState.ExpectsEdgeHint;

            switch (_currentState)
            {
                case NavigationDialogState.ExpectsEntityLabelHint:
                    _currentEntityTopic = getRandomEntityTopic();
                    return;

                case NavigationDialogState.ExpectsEdgeHint:
                    findRandomEdgeTopic(out _currentEdgeTopic, out _currentEdgeHypothesis);
                    _initialEdgeHypothesis = _currentEdgeHypothesis;
                    _currentEdgeRepresentant = getRandomEdgeRepresentant(_currentEdgeTopic.Edge);
                    return;
            }
        }

        private Tuple<FreebaseEntry, Edge, FreebaseEntry> getRandomEdgeRepresentant(string edge)
        {
            var edgeIndex = Array.IndexOf(_edges, edge);
            var representants = _edgeRepresentants[edgeIndex];

            var representantIndex = _rnd.Next(representants.Length);
            var representant = representants[representantIndex];

            return representant;
        }

        private void findRandomEdgeTopic(out EdgeNavigationData edgeTopic, out string edgeHypothesis)
        {
            var rndIndex = _rnd.Next(_edges.Length);
            var edge = _edges[rndIndex];

            edgeTopic = _data.GetEdgeData(edge);

            var hypotheses = edgeTopic.ExpressionVotes.ToArray();
            var totalVoteCount = hypotheses.Select(t => Math.Abs(t.Item2)).Sum() + 1;

            //TODO format edge to be readable
            edgeHypothesis = formatFreebaseEdge(edgeTopic.Edge);
            var repetitionCount = hypotheses.Length * 5;
            for (var i = 0; i < repetitionCount; ++i)
            {
                var hypothesisSample = hypotheses[i % hypotheses.Length];
                var threshold = 1.0 * Math.Max(1, hypothesisSample.Item2) / totalVoteCount;

                if (_rnd.NextDouble() >= threshold)
                    continue;

                edgeHypothesis = hypothesisSample.Item1;
            }
        }

        private string formatFreebaseEdge(string edge)
        {
            var formattedEdge = edge.Replace('/', ' ').Replace('_', ' ');

            var resultWords = new List<string>();
            foreach (var word in getWords(formattedEdge).Reverse())
                if (!resultWords.Contains(word))
                    resultWords.Add(word);

            var words = resultWords.Take(3).Reverse();
            formattedEdge = string.Join(" ", words);

            return "{0}, " + formattedEdge + ", {1}";
        }

        private EntityNavigationData getRandomEntityTopic()
        {
            var rndIndex = _rnd.Next(_entityPhrases.Length);
            var phrase = _entityPhrases[rndIndex];

            return _data.GetEntityData(phrase);
        }

        private bool setState(NavigationDialogState state)
        {
            _currentState = state;
            return _visitedStates.Add(state);
        }

        private void sampleEdgeRepresentants(int perEdgeSampleCount)
        {
            if (_edgeRepresentants.Count != 0)
                //representants are set already
                return;

            for (var i = 0; i < _edges.Length; ++i)
            {
                var edge = _edges[i];
                var representants = getRepresentants(edge, perEdgeSampleCount);
                _edgeRepresentants.Add(representants.ToArray());
            }
        }

        private IEnumerable<Tuple<FreebaseEntry, Edge, FreebaseEntry>> getRepresentants(string edge, int count)
        {
            var db = _linker.GetDb();

            var visitedIds = new HashSet<string>();
            foreach (var name in _entityPhrases)
            {
                foreach (var pointer in db.GetScoredDocs(name))
                {
                    var entry = db.GetEntry(pointer);
                    visitedIds.Add(entry.Id);
                }
            }

            var result = new List<Tuple<FreebaseEntry, Edge, FreebaseEntry>>();

            var depth = 0;
            var worklist = new Queue<string>(visitedIds);
            worklist.Enqueue(null);
            while (worklist.Count > 0)
            {
                var currentId = worklist.Dequeue();
                if (currentId == null)
                {
                    ++depth;
                    continue;
                }

                var entry = db.GetEntryFromId(currentId);
                if (!isGoodSample(entry))
                    continue;

                foreach (var target in entry.Targets)
                {
                    if (target.Item1.Name == edge)
                    {
                        if (visitedIds.Add(target.Item2))
                        {
                            var targetEntity = db.GetEntryFromId(target.Item2);

                            worklist.Enqueue(target.Item2);


                            var isOut = target.Item1.IsOutcoming;
                            var from = isOut ? entry : targetEntity;
                            var to = isOut ? targetEntity : entry;

                            if (!isGoodSample(targetEntity))
                                continue;

                            result.Add(Tuple.Create(from, target.Item1, to));
                            if (result.Count > count)
                                return result;
                        }
                    }
                    else if (depth < 2 && worklist.Count < 1000)
                    {
                        if (visitedIds.Add(target.Item2))
                            worklist.Enqueue(target.Item2);
                    }
                }
            }

            return result;
        }

        private bool isGoodSample(FreebaseEntry entry)
        {
            return entry != null && entry.Label != null && getWords(entry.Label).Length < 4;
        }

        private void parseCurrentEdgeRepresentant(out string label1, out string requestedRelation, out string edgeHypothesis, out string label2)
        {
            label1 = _currentEdgeRepresentant.Item1.Label;
            label2 = _currentEdgeRepresentant.Item3.Label;
            requestedRelation = _currentEdgeRepresentant.Item2.ToString();
            edgeHypothesis = _currentEdgeHypothesis;
        }


        private EntityInfo entityInfo(FreebaseEntry entity)
        {
            var db = _linker.GetDb();

            var entityInfo = db.GetEntityInfoFromMid(FreebaseDbProvider.GetMid(entity.Id));
            return entityInfo;
        }
    }
}
