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

        internal MindSet AddFact(string subject, string question, string answer, string durability = null)
        {
            Database.AddFact(subject, question, answer, durability);
            return this;
        }

        internal MindSet HowToEvaluate(NativePhraseEvaluator nativeEvaluator)
        {
            Evaluator.AddNativeEvaluator(_lastPattern, Evaluator.HowToEvaluateQ, nativeEvaluator);
            return this;
        }
        
        internal MindSet IsTrue(NativePhraseEvaluator nativeEvaluator)
        {
            Evaluator.AddNativeEvaluator(_lastPattern, Evaluator.IsItTrueQ, nativeEvaluator);
            return this;
        }

        internal MindSet HowToDo(NativePhraseEvaluator nativeEvaluator)
        {
            Evaluator.AddNativeEvaluator(_lastPattern, Evaluator.HowToDoQ, nativeEvaluator);
            return this;
        }

        internal MindSet IsTrue(string evaluationDescription)
        {
            Database.AddFact(_lastPattern.Representation, Evaluator.IsItTrueQ, evaluationDescription);
            return this;
        }

        internal MindSet HowToEvaluate(string semanticDescription)
        {
            Database.AddFact(_lastPattern.Representation, Evaluator.HowToEvaluateQ, semanticDescription);
            return this;
        }

        internal MindSet HowToDo(string howToDoDescription)
        {
            Database.AddFact(_lastPattern.Representation, Evaluator.HowToDoQ, howToDoDescription);
            return this;
        }
    }
}
