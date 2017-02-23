using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class NotUnderstandableContinuationAct : ContinuationActBase
    {
        public NotUnderstandableContinuationAct(ResponseBase continuation)
            : base(continuation) { }

        protected override IEnumerable<string> getVariantsFormat()
        {
            return new[]{
                "{0}",
                "I don't know. What about {1}?",
                "I'm not sure. What about {1}?",
                "Well, {1}?",
            };
        }
    }
}
