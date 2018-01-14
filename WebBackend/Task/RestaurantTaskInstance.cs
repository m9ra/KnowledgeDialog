using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Task
{
    class RestaurantTaskInstance : TaskInstance
    {

        /// <summary>
        /// Determine whether task has been completed.
        /// </summary>
        private bool _isComplete = false;

        /// <inheritdoc/>
        public override bool IsComplete { get { return _isComplete; } }

        internal RestaurantTaskInstance(int id, string taskFormat, string key, int validationCodeKey, string experimentHAML = "experiment.haml")
            : base(id, taskFormat, new NodeReference[0], new NodeReference[0], key, validationCodeKey, experimentHAML)
        {
        }

        internal void Register(IInformativeFeedbackProvider provider)
        {
            throw new NotImplementedException();
        }
    }
}