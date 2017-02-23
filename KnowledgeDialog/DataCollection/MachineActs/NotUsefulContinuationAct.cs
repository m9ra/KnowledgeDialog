using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class NotUsefulContinuationAct : ContinuationActBase
    {
        public NotUsefulContinuationAct(ResponseBase continuation)
            : base(continuation) { }

        protected override IEnumerable<string> getVariantsFormat()
        {
            return new[]{
                "{0}",
                "Well. What about {1}?",
                "Nevermind, {1}?",
                "No problem, {1}?",
                "Another question, {1}?",
                "Another thing, {1}?",
                "Next, {1}?",                
            };
        }
    }
}
