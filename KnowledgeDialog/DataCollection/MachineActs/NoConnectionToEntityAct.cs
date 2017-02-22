using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

namespace KnowledgeDialog.DataCollection.MachineActs
{
    public class NoConnectionToEntityAct : MachineActionBase
    {
        private readonly EntityInfo _entity;

        public NoConnectionToEntityAct(EntityInfo entity)
        {
            _entity = entity;
        }

        /// <inheritdoc/>
        protected override string initializeMessage()
        {
            return string.Format("I can't see a connection to {0}. Please try to answer with a full sentence.", _entity.BestAliasMatch);
        }

        /// <inheritdoc/>
        protected override ActRepresentation initializeDialogActRepresentation()
        {
            var act = new ActRepresentation("NoConnectionToEntity");
            act.AddParameter("entity", _entity);

            return act;
        }
    }
}
