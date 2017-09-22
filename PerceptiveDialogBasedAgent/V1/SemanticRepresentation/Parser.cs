using PerceptiveDialogBasedAgent.V1.Knowledge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V1.SemanticRepresentation
{
    class Parser
    {
        MindSet _mind;

        internal Parser(MindSet mind)
        {
            _mind = mind;
        }

        internal ParsingResult Parse(string utterance)
        {
            throw new NotImplementedException();
        }

        private EntityBox[][] getInitialBoxes(string utterance)
        {
            throw new NotImplementedException();
        }
    }
}
