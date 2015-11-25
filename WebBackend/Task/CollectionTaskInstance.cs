using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.DataCollection.MachineActs;

namespace WebBackend.Task
{
    class CollectionTaskInstance : TaskInstance
    {
        private readonly HashSet<string> _requiredEntityAliases;

        private bool _containsEntity = false;

        private bool _isDialogEnded = false;

        public override bool IsComplete { get { return _containsEntity && _isDialogEnded; } }

        internal CollectionTaskInstance(string taskFormat, IEnumerable<NodeReference> substitutions, IEnumerable<NodeReference> requiredEntityAliases, string key, int validationCodeKey) :
            base(taskFormat, substitutions, new NodeReference[0], key, validationCodeKey)
        {
            _requiredEntityAliases = new HashSet<string>(requiredEntityAliases.Select(e => e.Data));
        }

        internal override void Register(string utterance, ResponseBase response)
        {
            var parsedUtterance = UtteranceParser.Parse(utterance);
            foreach (var word in parsedUtterance.Words)
            {
                if (_requiredEntityAliases.Contains(word))
                    _containsEntity = true;
            }

            if (response is ByeAct)
                _isDialogEnded = true;
        }
    }
}
