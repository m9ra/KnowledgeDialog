using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation
{
    class ProcessingContext
    {
        private readonly List<ResponseBase> _responses = new List<ResponseBase>();

        internal bool NeedNextMachineAction { get; private set; }

        internal IEnumerable<ResponseBase> Responses { get { return _responses; } }

        internal void ForwardControl()
        {
            NeedNextMachineAction = true;
        }

        internal void Emit(ResponseBase response)
        {
            _responses.Add(response);
        }
    }
}
