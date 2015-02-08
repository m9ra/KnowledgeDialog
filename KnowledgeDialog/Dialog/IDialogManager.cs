using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.Dialog
{
    public interface IDialogManager
    {
        ResponseBase Ask(string question);

        ResponseBase Negate();

        ResponseBase Advise(string question, string answer);
    }
}
