using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.ModifiableResponses;

namespace KnowledgeDialog.PoolComputation
{
    class ConsoleDecorator : IDialogManager
    {
        private readonly DialogManager _dialogManager;

        public ConsoleDecorator(DialogManager dialogManager)
        {
            _dialogManager = dialogManager;
        }

        public ResponseBase Ask(string question)
        {
            var preRoots = getRootRules(_dialogManager).ToArray();

            var response = _dialogManager.Ask(question);

            var postRoots = getRootRules(_dialogManager).ToArray();

            var diffRoots = postRoots.Except(preRoots);
            if (diffRoots.Any())
            {
                ConsoleServices.BeginSection("Classification rules");
                foreach (var diffRoot in diffRoots)
                {
                    ConsoleServices.Print(diffRoot);
                }
                ConsoleServices.EndSection();
            }

            return response;
        }

        public ResponseBase Negate()
        {
            return _dialogManager.Negate();
        }

        public ResponseBase Advise(string question, string answer)
        {
            return _dialogManager.Advise(question, answer);
        }

        private IEnumerable<KnowledgeRule> getRootRules(DialogManager manager)
        {
            foreach (var data in manager.ConversationContext.StoredData)
            {
                var storagePattern = data as SurroundingPattern;
                if (storagePattern != null)
                {
                    yield return storagePattern.Prefix.SingleNodeClassifier.Root;
                }
            }
        }
    }
}
