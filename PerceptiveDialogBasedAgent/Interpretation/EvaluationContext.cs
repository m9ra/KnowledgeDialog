using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.Interpretation
{
    class EvaluationContext
    {
        private readonly Evaluator _evaluator;

        private readonly MatchElement _evaluatedElement;

        private readonly EvaluationContext _parentContext;

        internal DbConstraint Fetch => _evaluator.Mind.Database.Fetch;

        internal EvaluationContext(Evaluator evaluator)
            : this(evaluator, null, null) { }

        internal EvaluationContext(Evaluator evaluator, MatchElement evaluatedElement, EvaluationContext parentContext)
        {
            _evaluator = evaluator;

            _evaluatedElement = evaluatedElement;
            _parentContext = parentContext;
        }

        internal string Substitute(string phrase)
        {
            return _evaluatedElement.Substitute(phrase);
        }

        internal DbConstraint this[string variableName]
        {
            get
            {
                return Evaluate(variableName, Evaluator.HowToEvaluateQ);
            }
        }

        internal bool IsTrue(string variableName)
        {
            var variableSubstitution = getVariableSubstitution(variableName);
            return _evaluator.IsTrue(variableSubstitution.Token);
        }

        internal DbConstraint Evaluate(string variableName, string question)
        {
            var variableSubstitution = getVariableSubstitution(variableName);
            var result = _evaluator.Evaluate(variableSubstitution, question, this);

            return result.Constraint;
        }

        private MatchElement getVariableSubstitution(string variableName)
        {
            var variable = "$" + variableName;
            var variableSubstitution = _evaluatedElement.GetSubstitution(variable);
            return variableSubstitution;
        }

        internal DbConstraint Raw(string variableName)
        {
            var variable = "$" + variableName;
            var variableSubstitution = _evaluatedElement.GetSubstitution(variable);
            var result = DbConstraint.Entity(variableSubstitution.Token);
            return result;
        }


        internal DbConstraint AnswerWhere(DbConstraint subject, string question)
        {
            throw new NotImplementedException();
        }

        internal DbConstraint Constraint(DbConstraint fetch, string v, DbConstraint dbConstraint)
        {
            throw new NotImplementedException();
        }
    }
}
