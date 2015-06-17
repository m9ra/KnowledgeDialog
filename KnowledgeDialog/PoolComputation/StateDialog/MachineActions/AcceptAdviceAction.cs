﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class AcceptAdviceAction : MachineActionBase
    {
        private readonly static HashSet<string> _contextIndicators = new HashSet<string>()
        {
            "he","his","him","hisself","himself",
            "she","her","herself",
            "it","its","itself",
            "they","them","themself"            
        };


        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return InputState.HasUnknownQuestion && InputState.HasAdvice;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            var unknownQuestion = InputState.UnknownQuestion;
            var isBasedOnContext = hasContextIndicator(unknownQuestion);
            acceptAdvice(unknownQuestion, InputState.Advice, isBasedOnContext);

            EmitResponse("Thank you for the advice!");
            RemoveAdvice();
            RemoveUnknownQuestion();
        }

        private void acceptAdvice(ParsedExpression question, ParsedExpression advice, bool isBasedOnContext)
        {
            throw new NotImplementedException("Accept the advice");
        }

        private bool hasContextIndicator(ParsedExpression question)
        {
            foreach (var word in question.Words)
            {
                if (_contextIndicators.Contains(word))
                    return true;
            }

            return false;
        }
    }
}