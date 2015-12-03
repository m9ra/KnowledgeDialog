using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public abstract class MachineActionBase : ResponseBase
    {
        /// <summary>
        /// Message that will be printed to user when machine action 
        /// is given to output.
        /// </summary>
        private string _message;

        /// <summary>
        /// Semantic representation of the machine act.
        /// </summary>
        private ActRepresentation _dialogActRepresentation;

        /// <summary>
        /// Initialization for act message which will be printed to user.
        /// </summary>
        /// <returns>The message.</returns>
        protected abstract string initializeMessage();

        /// <summary>
        /// Initialization for semantic dialog act representation.
        /// </summary>
        /// <returns>The representation.</returns>
        protected abstract ActRepresentation initializeDialogActRepresentation();

        /// <inheritdoc/>
        public override string GetDialogActRepresentation()
        {
            if (_dialogActRepresentation == null)
                _dialogActRepresentation = initializeDialogActRepresentation();

            return _dialogActRepresentation.ToFunctionalRepresentation();
        }

        /// <inheritdoc/>
        public override string GetDialogActJsonRepresentation()
        {
            if (_dialogActRepresentation == null)
                _dialogActRepresentation = initializeDialogActRepresentation();

            return _dialogActRepresentation.ToJsonRepresentation();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (_message == null)
                _message = initializeMessage();

            return _message;
        }
    }
}
