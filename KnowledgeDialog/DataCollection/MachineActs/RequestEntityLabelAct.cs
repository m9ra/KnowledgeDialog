using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class RequestEntityLabelAct : MachineActionBase
    {
        /// <summary>
        /// Question presented with the weolcome.
        /// </summary>
        private readonly string _phrase;

        public RequestEntityLabelAct(string phrase)
        {
            _phrase = phrase;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            return string.Format("What is '{0}' ?", _phrase);
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation = new ActRepresentation("RequestEntityLabel");

            representation.AddParameter("phrase", _phrase);
            return representation;
        }
    }
}
