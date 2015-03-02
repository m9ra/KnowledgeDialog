using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation
{
    class ConsoleDecorator : IDialogManager
    {
        public ResponseBase Ask(string question)
        {
            throw new NotImplementedException();
        }

        public ResponseBase Negate()
        {
            throw new NotImplementedException();
        }

        public ResponseBase Advise(string question, string answer)
        {
            throw new NotImplementedException();
        }
    }
}
