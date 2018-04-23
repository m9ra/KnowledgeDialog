using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Brain
{
    abstract class PlanProvider
    {
        internal abstract MindState Substitute(PointableInstance instance, MindState state);

        internal abstract MindState GenerateQuestion(MindState bestMindState);

        internal abstract IEnumerable<SubstitutionPoint> GenerateSubstitutionPoints(MindState mindState, double providerWeight);
    }
}
