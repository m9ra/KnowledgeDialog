using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace WebBackend.Task
{
    class InformativeTaskInstance : TaskInstance
    {
        /// <summary>
        /// How many informative utterances has been registered.
        /// </summary>
        private int _informativeTurnCount = 0;

        /// <summary>
        /// Determine whether task has been completed.
        /// </summary>
        private bool _isComplete = false;

        /// <inheritdoc/>
        public override bool IsComplete { get { return _isComplete; } }

        internal InformativeTaskInstance(string taskFormat, IEnumerable<NodeReference> substitutions, IEnumerable<NodeReference> expectedAnswers, string key, int validationCodeKey, string experimentHAML = "experiment.haml")
            : base(taskFormat, substitutions, expectedAnswers, key, validationCodeKey, experimentHAML)
        {
        }

        /// <inheritdoc/>
        internal override void Register(string utterance, ResponseBase response)
        {
            base.Register(utterance, response);
        }

        internal void Register(IInformativeFeedbackProvider provider)
        {
            if (provider.HadInformativeInput)
                ++_informativeTurnCount;

            if (_informativeTurnCount > 5 && provider.CanBeCompleted)
                _isComplete = true;
        }
    }
}
