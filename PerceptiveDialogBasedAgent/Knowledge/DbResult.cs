using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.Knowledge
{
    class DbResult
    {
        internal readonly DbConstraint Constraint;

        internal readonly string Substitution;

        internal DbResult(DbConstraint constraint, string substitution)
        {
            Constraint = constraint;
            Substitution = substitution;
        }
    }
}
