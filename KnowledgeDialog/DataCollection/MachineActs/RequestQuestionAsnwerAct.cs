using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class RequestAnswerAct : MachineActionBase
    {
        private readonly bool _isAtLeastRequest;

        internal RequestAnswerAct(bool isAtLeastRequest)
        {
            _isAtLeastRequest = isAtLeastRequest;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return _isAtLeastRequest ?
           "That's OK. Could you please give me the correct answer for the question instead?" :
           "It sounds reasonable, however, I still  cannot find the answer. Could you give me the correct answer for the question please?";
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
