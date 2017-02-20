using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Parsing;

using KnowledgeDialog.DataCollection.MachineActs;

using KnowledgeDialog.Knowledge;

namespace WebBackend.AnswerExtraction
{
    enum ResponseParsingResult { Helpful, NotHelpful, NotUnderstandable };

    interface IModel
    {
        /// <summary>
        /// Poses questions about information needed for a given question.
        /// </summary>
        IEnumerable<ResponseBase> PoseQuestions(QuestionInfo question);

        /// <summary>
        /// Parse an informative response provided for a context.
        /// </summary>
        void UpdateContext(DialogContext context);
    }
}
