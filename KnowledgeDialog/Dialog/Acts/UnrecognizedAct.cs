﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Acts
{
    class UnrecognizedAct : DialogActBase
    {
        public readonly ParsedUtterance Utterance;

        public UnrecognizedAct(ParsedUtterance utterance)
        {
            Utterance = utterance;
        }

        internal override void Visit(IActVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "Unrecognized(utterance='" + Utterance + "')";
        }
    }
}
