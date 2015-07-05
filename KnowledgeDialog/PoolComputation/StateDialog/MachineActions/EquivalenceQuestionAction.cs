﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class EquivalenceQuestionAction : MachineActionBase
    {
        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return InputState.HasNonAnsweredQuestion && InputState.HasEquivalenceCandidate && !InputState.HasConfirmation;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            EmitResponse("Is your question same as '" + InputState.EquivalenceCandidate + "'?");
        }
    }
}