using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    public interface IInputDialogManager
    {
        ResponseBase Initialize();

        ResponseBase Input(ParsedExpression utterance);
    }
}
