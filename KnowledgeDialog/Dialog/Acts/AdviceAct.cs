﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    class AdviceAct : DialogActBase
    {
        public readonly ParsedExpression Answer;

        public AdviceAct(ParsedExpression advice)
        {
            Answer = advice;
        }

        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "Advice(answer='" + Answer + "')";
        }
    }
}
