using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerceptiveDialogBasedAgent.SemanticRepresentation;

namespace PerceptiveDialogBasedAgent.Interpretation
{
    class EvaluationResult
    {
        public readonly DbConstraint Constraint;

        public readonly IEnumerable<DbConstraint> UnknownConstraints;

        internal EvaluationResult(DbConstraint constraint, params DbConstraint[] unknownConstraint)
        {
            Constraint = constraint;
            UnknownConstraints = unknownConstraint;
        }

        public override string ToString()
        {
            return "[Evaluation]" + Constraint.ToString();
        }
    }
}
