using PerceptiveDialogBasedAgent.Interpretation;
using PerceptiveDialogBasedAgent.Knowledge;
using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent
{
    class MindSet
    {
        /// <summary>
        /// Database where knowledge is stored.
        /// </summary>
        internal readonly Database Database;

        /// <summary>
        /// Matcher for string representation creation.
        /// </summary>
        internal readonly PatternMatcher Matcher;

        internal readonly Evaluator Evaluator;

        private SemanticPattern _lastPattern;

        internal MindSet()
        {
            Database = new Database();
            Matcher = new PatternMatcher();
            Evaluator = new Evaluator(this);
        }

        internal MindSet AddPattern(params string[] patternParts)
        {
            var pattern = SemanticPattern.Parse(patternParts);
            Matcher.AddPattern(pattern);

            _lastPattern = pattern;

            return this;
        }

        internal MindSet AddFact(string subject, string question, string answer)
        {
            Database.AddFact(subject, question, answer);
            return this;
        }

        internal MindSet Semantic(NativePhraseEvaluator nativeEvaluator)
        {
            Evaluator.AddNativeEvaluator(_lastPattern, nativeEvaluator);
            return this;
        }

    }
}
