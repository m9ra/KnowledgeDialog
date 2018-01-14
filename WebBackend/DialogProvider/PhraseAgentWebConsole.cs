using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KnowledgeDialog.Dialog;

using PerceptiveDialogBasedAgent;

namespace WebBackend.DialogProvider
{
    class PhraseAgentWebConsole : WebConsoleBase
    {
        protected override IInputDialogManager createDialogManager()
        {
            return new PhraseAgentManager();
        }
    }
}
