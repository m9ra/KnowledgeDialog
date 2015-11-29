using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    class RequestQuestionAsnwerAct : MachineActionBase
    {
        private readonly bool _isAtLeastRequest;

        internal RequestQuestionAsnwerAct(bool isAtLeastRequest)
        {
            _isAtLeastRequest = isAtLeastRequest;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return _isAtLeastRequest ?
           "That's ok, can you give me the correct answer for your question instead?" :
           "It sounds reasonable, however, I still can't find the answer. Can you give me the correct answer for your question please?";
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation = new ActRepresentation("RequestAnswer");
            representation.AddParameter("at_least", _isAtLeastRequest);

            return representation;
        }
    }
}
