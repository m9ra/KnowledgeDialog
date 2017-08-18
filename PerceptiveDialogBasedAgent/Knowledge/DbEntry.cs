using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.Knowledge
{
    class DbEntry
    {
        /// <summary>
        /// Object which information is reresented here.
        /// </summary>
        internal readonly string Subject;

        /// <summary>
        /// Question which leads from Object1 to Object2.
        /// </summary>
        internal readonly string Question;

        /// <summary>
        /// Piece of information for Object1.
        /// </summary>
        internal readonly string Answer;

        internal DbEntry(string subject, string question, string answer)
        {
            Subject = subject;
            Question = question;
            Answer = answer;
        }
    }
}
