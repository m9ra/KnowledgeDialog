using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.QuestionAnswering
{
    public class TurnLog
    {
        /// <summary>
        /// Time when turn appeared within a dialog system.
        /// </summary>
        public readonly DateTime Time;

        /// <summary>
        /// Text of the turn.
        /// </summary>
        public readonly string Text;
    }
}
