using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class EvaluationLogEntry
    {
        public readonly string Input;

        public readonly string Question;

        public readonly IEnumerable<SemanticItem> Result;

        internal EvaluationLogEntry(string input, string question, IEnumerable<SemanticItem> result)
        {
            Input = input;
            Question = question;
            Result = result.ToArray();
        }
    }
}
