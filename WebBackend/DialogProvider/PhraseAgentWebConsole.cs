using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;
using PerceptiveDialogBasedAgent;

namespace WebBackend.DialogProvider
{
    class PhraseAgentWebConsole : WebConsoleBase
    {
        private readonly OutputRecognitionAlgorithm _algorithm;

        private readonly VoteContainer<object> _knowledge;

        private readonly bool _useKnowledge;

        private readonly bool _exportKnowledge;

        internal PhraseAgentWebConsole(OutputRecognitionAlgorithm algorithm, VoteContainer<object> knowledge, bool exportKnowledge, bool useKnowledge)
        {
            _algorithm = algorithm;
            _knowledge = knowledge;

            _useKnowledge = useKnowledge;
            _exportKnowledge = exportKnowledge;
        }

        protected override IInputDialogManager createDialogManager()
        {
            var manager = new PhraseAgentManager(_algorithm, _knowledge, _exportKnowledge, _useKnowledge);

            return manager;
        }
    }
}
