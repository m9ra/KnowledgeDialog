using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

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
            "No problem, could you please explain in detail what is the question really asking for?" :
            "This seems like an interesting question. Could you explain in detail what it is really asking for?";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation = new ActRepresentation("RequestExplanation");
            representation.AddParameter("at_least", _isAtLeastRequest);

            return representation;
        }
    }
}
