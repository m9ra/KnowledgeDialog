using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuestions
{
    class SimpleQuestionEntry
    {
        public readonly FreeBaseNode SupportFact;

        public readonly FreeBaseEdge Edge;

        public readonly FreeBaseNode Answer;

        public readonly string Question;

        internal SimpleQuestionEntry(string entryLine)
        {
            var tokens = entryLine.Split('\t');

            SupportFact=new FreeBaseNode(tokens[0]);
            Edge = new FreeBaseEdge(tokens[1]);
            Answer = new FreeBaseNode(tokens[2]);
            Question = tokens[3];
        }
    }
}
