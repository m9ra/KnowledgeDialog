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

            RegisterPattern(p => new DontKnowAct(), "i dont know", "i don't know", "i do not know", "i have no idea");

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
            if (HasImplicitDontKnow(utterance))
                return new DontKnowAct();

            if (HasImplicitNegation(utterance))
                return new NegateAct();


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

        public static bool HasImplicitNegation(ParsedUtterance utterance)
        {
            return HasImplicitNegation(utterance.OriginalSentence);
        }

        public static bool HasImplicitDontKnow(ParsedUtterance utterance)
        {
            return HasImplicitDontKnow(utterance.OriginalSentence);
        }

        public static bool HasImplicitNegation(string utterance)
        {
            return
                containsExpression(utterance, "no it") ||
                containsExpression(utterance, "cant") ||
                containsExpression(utterance, "can't") ||
                containsExpression(utterance, "cannot") ||
                containsExpression(utterance, "can not") ||
                containsExpression(utterance, "i dont") ||
                containsExpression(utterance, "i do not") ||
                containsExpression(utterance, "i don't") ||
                containsExpression(utterance, "donot") ||
                containsExpression(utterance, "sorry") ||
                containsExpression(utterance, "i would need") ||
                containsExpression(utterance, "i would have") ||
                containsExpression(utterance, "i must") ||
                containsExpression(utterance, "i need") ||
                containsExpression(utterance, "pointless") ||
                containsExpression(utterance, "impossible") ||
                containsExpression(utterance, "check wikipedia") ||
                containsExpression(utterance, "check google") ||
                containsExpression(utterance, "google it") ||
                (containsExpression(utterance, " no ") && containsExpression(utterance, "correct")) ||
                (containsExpression(utterance, " no ") && containsExpression(utterance, "answer")) ||
                (containsExpression(utterance, " i ") && containsExpression(utterance, "never"))  ||
                (containsExpression(utterance, " i ") && containsExpression(utterance, "unable")) ||
                (containsExpression(utterance, " i'") && containsExpression(utterance, "never")) ||
                (containsExpression(utterance, " ive") && containsExpression(utterance, "never")) ||
                (containsExpression(utterance, "need") && containsExpression(utterance, "search")) ||
                (containsExpression(utterance, "google") && containsExpression(utterance, "search")) ||
                (containsExpression(utterance, "bing") && containsExpression(utterance, "search")) ||
                (containsExpression(utterance, "web") && containsExpression(utterance, "search")) ||
                
                startsWith(utterance, "not ") ||
                startsWith(utterance, "no ") ||
                startsWith(utterance, "not,") ||
                startsWith(utterance, "no,")
                ;
        }

        public static bool HasImplicitDontKnow(string utterance)
        {
            return
                containsExpression(utterance, "no idea") ||
                containsExpression(utterance, "guess") ||
                containsExpression(utterance, "i haven't") ||
                containsExpression(utterance, "i havent") ||
                containsExpression(utterance, "i have not") ||
                containsExpression(utterance, "if i knew") ||
                containsExpression(utterance, "dont know") ||
                containsExpression(utterance, "don't know") ||
                containsExpression(utterance, "understand") ||
                containsExpression(utterance, "hard to say") ||
                containsExpression(utterance, "i have never") ||
                containsExpression(utterance, "i has never") ||
                containsExpression(utterance, "i was not") ||
                containsExpression(utterance, "i wasnt") ||
                containsExpression(utterance, "i wasn't") ||
                containsExpression(utterance, "not sure") ||
                containsExpression(utterance, "unsure") ||
                containsExpression(utterance, "just look") ||
                containsExpression(utterance, "has look") ||
                containsExpression(utterance, "has to look") ||
                containsExpression(utterance, "i do not") ||
                containsExpression(utterance, "i did not")

                ;
        }

        private static bool containsExpression(string utterance, string expression)
        {
            var sanitizedUtterance = " " + utterance.ToLowerInvariant() + " ";
            return sanitizedUtterance.Contains(expression);
        }

        private static bool startsWith(string utterance, string expression)
        {
            return utterance.StartsWith(expression, StringComparison.InvariantCultureIgnoreCase);
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
