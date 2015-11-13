using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class RequestExplanationAct : MachineActionBase
    {
        private readonly bool _isAtLeastRequest;

        internal RequestExplanationAct(bool isAtLeastRequest)
        {
            _isAtLeastRequest = isAtLeastRequest;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return _isAtLeastRequest ?
            "No problem, can you explain the question in detail instead?" :
            "It seems like an interesting question. However, I would like you explain it to me in detail please.";
        }

        /// <inheritdoc/>
        protected override string initializeDialogActRepresentation()
        {
            if (_isAtLeastRequest)
            {
                return "RequestExplanation(at_least='true')";
            }
            else
            {
                return "RequestExplanation(at_least='false')";
            }
        }
    }
}
