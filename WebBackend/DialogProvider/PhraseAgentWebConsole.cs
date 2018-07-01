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
        private readonly OutputRecognitionAlgorithm _algorithm;

        internal PhraseAgentWebConsole(OutputRecognitionAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        protected override IInputDialogManager createDialogManager()
        {
            return new PhraseAgentManager(_algorithm);
        }
    }
}
