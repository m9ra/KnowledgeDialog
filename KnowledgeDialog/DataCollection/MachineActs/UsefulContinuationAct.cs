using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class UsefulContinuationAct : ContinuationActBase
    {
        public UsefulContinuationAct(ResponseBase continuation)
            : base(continuation) { }

        protected override IEnumerable<string> getVariantsFormat()
        {
            return new[]{
                "Great. What about {1}?",
                "Thank you. {0}",
                "Cool, {1}?",
                "Thanks, {1}?",
                "Amazing. {0}",
                "Interesting, {1}?",                
            };
        }
    }
}
