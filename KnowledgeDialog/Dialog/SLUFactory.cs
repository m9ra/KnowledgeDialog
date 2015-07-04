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
            RegisterGroup("rude_word", "wtf", "suck", "fuck");
            RegisterGroup("welcome_word","welcome", "hi", "hello");
            RegisterGroup("bye_word", "bye");


            //yes - no handling
            RegisterPattern("$yes_word", (p) => new AffirmAct());
            RegisterPattern("$no_word", (p) => new NegateAct());
            RegisterPattern("$stronging_adjective $yes_word", (p) => new AffirmAct());
            RegisterPattern("$stronging_adjective $no_word", (p) => new NegateAct());

            //chitchat handling
            RegisterPattern("$welcome_word", (p) => new ChitChatAct(ChitChatDomain.Welcome));
            RegisterPattern("$bye_word", (p) => new ChitChatAct(ChitChatDomain.Bye));
            RegisterPattern("$w_word are you", (p) => new ChitChatAct(ChitChatDomain.Personal));
            RegisterPattern("$rude_word", (p) => new ChitChatAct(ChitChatDomain.Rude));

            //question - advice parsing
            RegisterPattern("#1 is #2 $w_word #3", (p) => new QuestionAct(p[0]));
            RegisterPattern("$pronoun is #1", (p) => new AdviceAct(p[1]));
            RegisterPattern("$w_word #1", (p) => new QuestionAct(p[0]));
            RegisterPattern("$possesive_pronoun name is #1", (p) => new AdviceAct(p[1]));
            RegisterPattern("#1 is #2", (p) => new ExplicitAdviceAct(p[1], p[2]));
            RegisterPattern("correct answer $answer_preposition #1 is #2", (p) => new ExplicitAdviceAct(p[1], p[2]));
        }

        public DialogActBase GetDialogAct(ParsedExpression utterance)
        {
            var currentStateLayer = new List<PatternState>();
            foreach (var pattern in _patterns.Keys)
            {
                var state = pattern.InitialState();
                currentStateLayer.Add(state);
            }

            var nextStateLayer = new List<PatternState>();
            foreach (var word in utterance.Words)
            {
                foreach (var state in currentStateLayer)
                {
                    var nextStates = state.GetNextStates(word);
                    nextStateLayer.AddRange(nextStates);
                }

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

        private void RegisterPattern(string patternDefinition, ActCreator creator)
        {
            var pattern = new UtterancePattern(patternDefinition, _configuration);
            _patterns.Add(pattern, creator);
        }

        private void RegisterGroup(string group, params string[] words)
        {
            if (_patterns.Count > 0)
                throw new NotSupportedException("Cannot update groups when patterns are registered");

            _configuration.RegisterGroup(group, words);
        }
    }

    class PatternHandler
    {
        Dictionary<int, ParsedExpression> _sentences = new Dictionary<int, ParsedExpression>();

        internal PatternHandler(ParsedExpression utterance, PatternState state)
        {
            var substitutions = state.Substitutions.ToArray();
            var originalPattern = state.OriginalPattern;
            for (var i = 0; i < originalPattern.Length; ++i)
            {
                var sentenceKey = originalPattern.GetSentenceKey(i);
                if (sentenceKey != null)
                {
                    var sentence = ParsedExpression.From(substitutions[i]);
                    _sentences[int.Parse(sentenceKey)] = sentence;
                }
            }

            _sentences[0] = utterance;
        }

        public ParsedExpression this[int index]
        {
            get
            {
                return _sentences[index];
            }
        }
    }
}
