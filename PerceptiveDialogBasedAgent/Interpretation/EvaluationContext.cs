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

        public DbConstraint this[string variableName]
        {
            get
            {
                var variable = "$" + variableName;
                var variableSubstitution = _evaluatedElement.GetSubstitution(variable);
                var result = _evaluator.Evaluate(variableSubstitution, this);

                return result.Constraint;
            }
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
