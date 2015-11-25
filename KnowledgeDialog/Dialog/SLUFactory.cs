using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Parsing;

namespace KnowledgeDialog.Dialog
{
    delegate DialogActBase ActCreator(PatternHandler handler);

    public class SLUFactory
    {
        private Dictionary<string, IEnumerable<string>> _groups = new Dictionary<string, IEnumerable<string>>();

        private PatternConfiguration _configuration = new PatternConfiguration();

        private Dictionary<UtterancePattern, ActCreator> _patterns = new Dictionary<UtterancePattern, ActCreator>();

        public SLUFactory()
        {
            RegisterGroup("possesive_pronoun", "his", "her", "its", "their", "our", "yours");
            RegisterGroup("pronoun", "I", "you", "he", "it", "she", "they", "we");
            RegisterGroup("answer_preposition", "for", "of", "on");
            RegisterGroup("yes_word", "yes", "yeah", "y", "ok", "sure");
            RegisterGroup("no_word", "no", "nope", "n");
            RegisterGroup("w_word", "which", "what", "who", "where", "when", "why", "how");
            RegisterGroup("stronging_adjective", "definitely", "absolutely", "pretty");
            RegisterGroup("rude_word", "wtf", "suck", "fuck", "dude", "idiot");
            RegisterGroup("welcome_word", "welcome", "hi", "hello");
            RegisterGroup("bye_word", "bye");


            //yes - no handling
            RegisterPattern(p => new AffirmAct(), "$yes_word");
            RegisterPattern(p => new NegateAct(), "$no_word");
            RegisterPattern(p => new AffirmAct(), "$stronging_adjective $yes_word");
            RegisterPattern(p => new NegateAct(), "$stronging_adjective $no_word");
            RegisterPattern(p => new DontKnowAct(), "dont know", "don't know", "do not know", "no idea");
            RegisterPattern(p => new DontKnowAct(), "I dont know", "I don't know", "I do not know", "I have no idea");

            //chitchat handling
            RegisterPattern(p => new ChitChatAct(ChitChatDomain.Welcome), "$welcome_word");
            RegisterPattern(p => new ChitChatAct(ChitChatDomain.Bye), "$bye_word");
            RegisterPattern(p => new ChitChatAct(ChitChatDomain.Personal), "$w_word are you");
            RegisterPattern(p => new ChitChatAct(ChitChatDomain.Personal), "$w_word is your name");
            RegisterPattern(p => new ChitChatAct(ChitChatDomain.Personal), "$w_word do you do");
            RegisterPattern(p => new ChitChatAct(ChitChatDomain.Rude), "$rude_word");
            RegisterPattern(p => new ChitChatAct(ChitChatDomain.Rude), "I hate you");

            //question - advice parsing
            RegisterPattern(p => new QuestionAct(p[0]), "#1 is #2 $w_word #3");
            RegisterPattern(p => new AdviceAct(p[1]), "$pronoun is #1");
            RegisterPattern(p => new QuestionAct(p[0]), "$w_word #1");
            RegisterPattern(p => new AdviceAct(p[1]), "$possesive_pronoun name is #1");
            RegisterPattern(p => new ExplicitAdviceAct(p[1], p[2]), "#1 is #2");
            RegisterPattern(p => new ExplicitAdviceAct(p[1], p[2]), "correct answer $answer_preposition #1 is #2");
        }

        public IEnumerable<DialogActBase> GetDialogActs(ParsedUtterance utterance)
        {
            var currentStateLayer = new List<PatternState>();
            var finishedStates = new List<PatternState>();

            var currentWordIndex = 0;
            foreach (var word in utterance.Words)
            {
                //we allow pattern begin at every word
                foreach (var pattern in _patterns.Keys)
                {
                    var state = pattern.InitialState(currentWordIndex);
                    currentStateLayer.Add(state);
                }

                var nextStateLayer = new List<PatternState>();
                foreach (var state in currentStateLayer)
                {
                    var nextStates = state.GetNextStates(word, currentWordIndex);
                    foreach (var nextState in nextStates)
                    {
                        if (nextState.IsFinished)
                        {
                            //state has been finished - don't continue expanding from it
                            finishedStates.Add(nextState);
                        }
                        else
                        {
                            //state still needs continuation
                            nextStateLayer.Add(nextState);
                        }
                    }
                }

                currentStateLayer = nextStateLayer;
                currentWordIndex += 1;
            }

            var stateCovers = findStateCovers(currentWordIndex - 1, finishedStates).ToArray();

            throw new NotImplementedException();
        }

        public DialogActBase GetBestDialogAct(ParsedUtterance utterance)
        {
            var currentStateLayer = new List<PatternState>();
            foreach (var pattern in _patterns.Keys)
            {
                var state = pattern.InitialState(0);
                currentStateLayer.Add(state);
            }

            var nextStateLayer = new List<PatternState>();
            var wordIndex = 0;
            foreach (var word in utterance.Words)
            {
                foreach (var state in currentStateLayer)
                {
                    var nextStates = state.GetNextStates(word, wordIndex);
                    nextStateLayer.AddRange(nextStates);
                }
                wordIndex += 1;
                //swap current with next state layer
                var swapLayer = currentStateLayer;
                currentStateLayer = nextStateLayer;
                nextStateLayer = swapLayer;
                nextStateLayer.Clear();

                if (currentStateLayer.Count == 0)
                    //we dont have any matching pattern
                    break;
            }

            currentStateLayer.RemoveAll((p) => !p.IsFinished);

            if (currentStateLayer.Count < 1)
                return new UnrecognizedAct(utterance);

            var bestState = currentStateLayer[0];
            var actFactory = _patterns[bestState.OriginalPattern];
            var handler = new PatternHandler(utterance, bestState);

            return actFactory(handler);
        }

        private void RegisterPattern(ActCreator creator, params string[] patternDefinitions)
        {
            foreach (var patternDefinition in patternDefinitions)
            {
                var pattern = new UtterancePattern(patternDefinition, _configuration);
                _patterns.Add(pattern, creator);
            }
        }

        private void RegisterGroup(string group, params string[] words)
        {
            if (_patterns.Count > 0)
                throw new NotSupportedException("Cannot update groups when patterns are registered");

            _configuration.RegisterGroup(group, words);
        }

        private IEnumerable<IEnumerable<PatternState>> findStateCovers(int endOffset, IEnumerable<PatternState> finishedStates)
        {
            var orderedFinishedStates = finishedStates.OrderByDescending((s) => s.CoveredIndexes.Count());

            //   var coveringStates = orderedFinishedStates.Select(s => s.Parent);

            return findStateCovers(0, endOffset, orderedFinishedStates);
        }

        private IEnumerable<IEnumerable<PatternState>> findStateCovers(int desiredOffset, int endOffset, IEnumerable<PatternState> finishedStates)
        {
            var result = new List<IEnumerable<PatternState>>();
            foreach (var finishedState in finishedStates)
            {
                var lowerIndexes = finishedState.CoveredIndexes.Where(index => index <= desiredOffset);
                var isMatchingState = lowerIndexes.Count() == 1 && lowerIndexes.Contains(desiredOffset);

                var isEndState = finishedState.CoveredIndexes.Contains(endOffset);
                if (!isMatchingState)
                    //determine whether state matches desired offset
                    continue;

                if (isEndState)
                {
                    //end state
                    result.Add(new[] { finishedState });
                    continue;
                }

                var nextOffset = finishedState.CoveredOffset;
                var nextCovers = findStateCovers(nextOffset + 1, endOffset, finishedStates);

                foreach (var nextCover in nextCovers)
                {
                    result.Add(new[] { finishedState }.Concat(nextCover));
                }
            }

            return result;
        }
    }

    class PatternHandler
    {
        Dictionary<int, ParsedUtterance> _sentences = new Dictionary<int, ParsedUtterance>();

        internal PatternHandler(ParsedUtterance utterance, PatternState state)
        {
            var substitutions = state.Substitutions.ToArray();
            var originalPattern = state.OriginalPattern;
            for (var i = 0; i < originalPattern.Length; ++i)
            {
                var sentenceKey = originalPattern.GetSentenceKey(i);
                if (sentenceKey != null)
                {
                    var sentence = ParsedUtterance.From(substitutions[i]);
                    _sentences[int.Parse(sentenceKey)] = sentence;
                }
            }

            _sentences[0] = utterance;
        }

        public ParsedUtterance this[int index]
        {
            get
            {
                return _sentences[index];
            }
        }
    }
}
