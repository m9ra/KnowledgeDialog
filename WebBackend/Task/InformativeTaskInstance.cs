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
        /// How many informative turns are required for taking the task as complete.
        /// </summary>
        private readonly int _requiredInformativeTurnCount;

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

        internal InformativeTaskInstance(int id, string taskFormat, IEnumerable<NodeReference> substitutions, IEnumerable<NodeReference> expectedAnswers, string key, int validationCodeKey, int requiredInformativeTurnCount, string experimentHAML = "experiment.haml")
            : base(id, taskFormat, substitutions, expectedAnswers, key, validationCodeKey, experimentHAML)
        {
            _requiredInformativeTurnCount = requiredInformativeTurnCount;
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

            if (_informativeTurnCount >= _requiredInformativeTurnCount && provider.CanBeCompleted)
                _isComplete = true;

            SuccessCode = provider.SuccessCode;
        }
    }
}
