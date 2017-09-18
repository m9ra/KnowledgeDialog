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
    class EmptyAgent
    {
        protected readonly MindSet Mind = new MindSet();

        protected readonly Body Body;

        internal EmptyAgent()
        {
            Body = new Body(Mind);

            Mind
                .AddPattern("when", "$something", "do", "$action")
                    .HowToDo("add $action to $something")
                ;
        }

        internal string Input(string data)
        {
            return Body.Input(data);
        }
    }
}
