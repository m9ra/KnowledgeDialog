using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    /// <summary>
    /// Report about advice of question answer.
    /// </summary>
    class AnswerReport
    {
        /// <summary>
        /// Question that has been answered by Answer.
        /// </summary>
        public readonly QuestionEntry Question;

        /// <summary>
        /// Answer for Question.
        /// </summary>
        public readonly NodeReference Answer;

        /// <summary>
        /// How many times was answer context free
        /// </summary>
        public int ContextFreeCounts {get;private set;}

        /// <summary>
        /// How many times was answer based on context
        /// </summary>
        public int ContextBasedCounts{get;private set;}

        internal AnswerReport(QuestionEntry question, NodeReference answer)
        {
            Question = question;
            Answer = answer;
        }

        internal void ReportOccurence(bool isContextBased){
            if(isContextBased)
                ContextBasedCounts+=1;
            else
                ContextFreeCounts+=1;
        }
    }
}
