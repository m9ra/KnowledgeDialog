using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public class AffirmAct : DialogActBase
    {
        private readonly string _expression;

        internal AffirmAct()
        {

        }

        internal AffirmAct(string expression)
        {
            _expression = expression;
        }

        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            if (_expression == null)
                return "Affirm()";

            return string.Format("Affirm({0})", _expression);
        }

        /// <inheritdoc/>
        public override ActRepresentation GetDialogAct()
        {
            var act = new ActRepresentation("Affirm");
            if (_expression != null)
                act.AddParameter("expression", _expression);

            return act;
        }
    }
}
