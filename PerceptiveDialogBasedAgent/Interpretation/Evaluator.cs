using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.Interpretation
{
    delegate DbConstraint NativePhraseEvaluator(EvaluationContext context);

    class Evaluator
    {
        private readonly Dictionary<string, NativePhraseEvaluator> _evaluators = new Dictionary<string, NativePhraseEvaluator>();

        internal readonly MindSet Mind;

        internal static readonly string HowToEvaluateQ = "How to evaluate @?";

        internal static readonly string HowToDoQ = "How to do @?";

        internal static readonly string IsItTrueQ = "Is @ true?";

        internal Evaluator(MindSet mind)
        {
            Mind = mind;
        }

        internal void AddNativeEvaluator(SemanticPattern pattern, string evaluatedQuestion, NativePhraseEvaluator evaluator)
        {
            var patternRepresentation = pattern.Representation;
            var evaluatorRepresentation = getNativeEvaluatorId(patternRepresentation, evaluatedQuestion);

            Mind.AddFact(patternRepresentation, evaluatedQuestion, evaluatorRepresentation);
            _evaluators.Add(evaluatorRepresentation, evaluator);
        }

        public bool IsTrue(DbConstraint constraint)
        {
            var isKnownFact = Mind.Database.Query(constraint).Any();
            if (isKnownFact)
                return true;

            if (constraint.AnswerConstraints.Any() || constraint.SubjectConstraints.Any())
                return false;

            var result = Evaluate(constraint.PhraseConstraint, IsItTrueQ);

            return result.Constraint.PhraseConstraint == "true";
        }

        public EvaluationResult Evaluate(string phrase, string question, EvaluationContext parentContext = null)
        {
            var nativeEvaluator = getNativeEvaluator(phrase, question);
            if (nativeEvaluator != null)
            {
                var constraint = nativeEvaluator(parentContext);
                return new EvaluationResult(constraint, new DbConstraint[0]);
            }

            var match = Mind.Matcher.BestMatch(phrase);
            var element = match.RootElement;

            return Evaluate(element, question, new EvaluationContext(this, element, parentContext));
        }

        internal EvaluationResult Evaluate(MatchElement element, string question, EvaluationContext parentContext)
        {
            var context = new EvaluationContext(this, element, parentContext);

            var elementRepresentation = element.Pattern.Representation;
            var evaluationDescription = getEvaluationDescription(elementRepresentation, question, context);
            if (evaluationDescription == null)
            {
                //we can't do better than create explicit entity constraint and generate HowToEvaluateQ.
                return new EvaluationResult(DbConstraint.Entity(element.Token), new DbConstraint(new ConstraintEntry(DbConstraint.Entity(element.Token), question, null)));
            }

            return Evaluate(evaluationDescription, question, context);
        }

        private string getEvaluationDescription(string representation, string question, EvaluationContext context)
        {
            var answers = Mind.Database.GetAnswers(representation, question).ToArray();
            if (answers.Length == 0)
                return null;

            if (answers.Length != 1)
                throw new NotImplementedException("What to do when there are multiple evaluations?");

            var answer = answers.FirstOrDefault();
            if (!_evaluators.ContainsKey(answer))
                // dont change evaluator ids
                answer = context.Substitute(answer);

            return answer;
        }

        private NativePhraseEvaluator getNativeEvaluator(string phrase, string question)
        {
            _evaluators.TryGetValue(phrase, out var result);
            return result;
        }

        private string getNativeEvaluatorId(string phrase, string question)
        {
            return phrase + "_" + question;
        }
    }
}
