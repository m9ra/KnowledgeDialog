using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.Interpretation
{
    delegate DbConstraint NativePhraseEvaluator(EvaluationContext context);

    delegate DbConstraint NativeActionExecutor(EvaluationContext context);

    class Evaluator
    {
        private readonly Dictionary<string, NativePhraseEvaluator> _evaluators = new Dictionary<string, NativePhraseEvaluator>();

        private readonly Dictionary<string, NativeActionExecutor> _executors = new Dictionary<string, NativeActionExecutor>();

        internal readonly MindSet Mind;

        internal static readonly string HowToEvaluateQ = "How to evaluate @?";

        internal static readonly string HowToDoQ = "How to do @?";

        internal static readonly string NativeEvaluatorPrefix = "%native_evaluator-";

        internal static readonly string NativeExecutorPrefix = "%native_executor-";

        internal Evaluator(MindSet mind)
        {
            Mind = mind;
        }

        internal void AddNativeEvaluator(SemanticPattern pattern, NativePhraseEvaluator evaluator)
        {
            var patternRepresentation = pattern.Representation;
            var evaluatorRepresentation = NativeEvaluatorPrefix + patternRepresentation;
            Mind.AddFact(patternRepresentation, HowToEvaluateQ, NativeEvaluatorPrefix + patternRepresentation);
            _evaluators.Add(evaluatorRepresentation, evaluator);
        }

        public bool IsTrue(DbConstraint constraint)
        {
            //TODO recursive evaluation should be here
            return Mind.Database.Query(constraint).Any();
        }

        public EvaluationResult Evaluate(string phrase, EvaluationContext parentContext = null)
        {
            var nativeEvaluator = getNativeEvaluator(phrase);
            if (nativeEvaluator != null)
            {
                var constraint = nativeEvaluator(parentContext);
                return new EvaluationResult(constraint);
            }

            var match = Mind.Matcher.BestMatch(phrase);
            var element = match.RootElement;

            return Evaluate(element, new EvaluationContext(this, element, parentContext));
        }

        internal EvaluationResult Evaluate(MatchElement element, EvaluationContext parentContext)
        {
            var context = new EvaluationContext(this, element, parentContext);

            var elementRepresentation = element.Pattern.Representation;
            var evaluationDescription = getEvaluationDescription(elementRepresentation, context);
            if (evaluationDescription == null)
            {
                //we can't do better than create explicit entity constraint and generate HowToEvaluateQ.

                //TODO generate Q
                return new EvaluationResult(DbConstraint.Entity(element.Token));
            }

            return Evaluate(evaluationDescription, context);
        }

        private string getEvaluationDescription(string representation, EvaluationContext context)
        {
            var answers = Mind.Database.GetAnswers(representation, HowToEvaluateQ).ToArray();
            if (answers.Length == 0)
                return null;

            if (answers.Length != 1)
                throw new NotImplementedException("What to do when there are multiple evaluations?");

            return answers.FirstOrDefault();
        }

        private NativePhraseEvaluator getNativeEvaluator(string phrase)
        {
            _evaluators.TryGetValue(phrase, out var result);
            return result;
        }
    }
}
