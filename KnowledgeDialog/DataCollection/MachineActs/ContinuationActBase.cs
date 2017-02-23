using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public abstract class ContinuationActBase : MachineActionBase
    {
        protected abstract IEnumerable<string> getVariantsFormat();

        private readonly ResponseBase _continuation;

        private readonly object _L_rnd = new object();

        private readonly Random _rnd = new Random();

        protected ContinuationActBase(ResponseBase continuation)
        {
            _continuation = continuation;
        }

        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            var variants = getVariantsFormat().ToArray();
            lock (_L_rnd)
            {
                var nextIndex = _rnd.Next(variants.Length);

                var continuationMessage = _continuation.ToString().Trim();
                var continuationMessageInner = char.ToLowerInvariant(continuationMessage[0]) + continuationMessage.Substring(1, continuationMessage.Length - 2);
                return string.Format(variants[nextIndex], continuationMessage, continuationMessageInner);
            }
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var typeName = this.GetType().Name;
            if (!typeName.EndsWith("Act"))
                throw new FormatException("Act name does not follow naming conventions");

            var act = new ActRepresentation(typeName.Substring(0, typeName.Length - 3));
            act.AddParameter("_continuation", _continuation);
            return act;
        }
    }
}
