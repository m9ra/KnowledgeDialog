using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected override string initializeDialogActRepresentation()
        {
            if (_isAtLeastRequest)
            {
                return "RequestAnswer(at_least='true')";
            }
            else
            {
                return "RequestAnswer(at_least='false')";
            }
        }
    }
}
