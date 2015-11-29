﻿using System;
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
            "No problem, can you explain the question in detail instead?" :
            "It seems like an interesting question. However, I would like you explain it to me in detail please.";
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
