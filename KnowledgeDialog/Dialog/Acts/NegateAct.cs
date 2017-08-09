using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    public class NegateAct : DialogActBase
    {
        readonly string _expression;

        internal NegateAct()
        {

        }

        internal NegateAct(string expression)
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
                return "Negate()";

            return string.Format("Negate({0})", _expression);
        }

        /// <inheritdoc/>
        public override ActRepresentation GetDialogAct()
        {
            var act = new ActRepresentation("Negate");
            if (_expression != null)
                act.AddParameter("expression", _expression);

            return act;
        }
    }
}
