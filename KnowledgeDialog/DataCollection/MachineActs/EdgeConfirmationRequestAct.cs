using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class EdgeConfirmationRequestAct : MachineActionBase
    {
        private readonly string _label1;

        private readonly string _relationFormat;

        private readonly string _label2;

        public EdgeConfirmationRequestAct(string label1, string relationFormat, string label2)
        {
            _label1 = label1;
            _relationFormat = relationFormat ?? throw new ArgumentNullException("relationFormat");
            _label2 = label2;
        }

        ///<inheritdoc/>
        protected override string initializeMessage()
        {
            var expression = string.Format(_relationFormat, _label1, _label2);

            return string.Format("You mean '{0}' is better?", expression);
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var representation = new ActRepresentation("EdgeConfirmationRequest");

            representation.AddParameter("label1", _label1);
            representation.AddParameter("relationFormat", _relationFormat);
            representation.AddParameter("label2", _label2);
            return representation;
        }
    }
}
