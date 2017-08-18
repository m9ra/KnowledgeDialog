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

        internal EvaluationResult(DbConstraint constraint)
        {
            Constraint = constraint;
        }

        public override string ToString()
        {
            return "[Evaluation]" + Constraint.ToString();
        }
    }
}
