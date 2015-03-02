using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.ModifiableResponses;

namespace KnowledgeDialog.PoolComputation.Frames
{
    class ChangeResponseFrame : ConversationFrameBase
    {
        private readonly string FormConfirmation = "I will be better next time!";

        private readonly string FormIncapability = "I'm sorry but I can't learn this form of response.";

        private readonly string DontUnderstand = "I cant understand you. I have expected advice in form: You should say ...";

        private readonly ModifiableResponse _changedResponse;

        internal ChangeResponseFrame(ConversationContext context, ModifiableResponse changedResponse)
            : base(context)
        {
            _changedResponse = changedResponse;
        }

        protected override ModifiableResponse FrameInitialization()
        {
            //TODO make it more robust
            var action = "you should say ";
            IsComplete = true;
            if (CurrentInput.StartsWith(action, StringComparison.OrdinalIgnoreCase))
            {
                var correctForm = CurrentInput.Substring(action.Length);
                if (_changedResponse.Modify(correctForm))
                {
                    return Response(FormConfirmation);
                }
                else
                {
                    return Response(FormIncapability);
                }
            }
            else
            {
                return Response(DontUnderstand);
            }
        }

        protected override ModifiableResponse DefaultHandler()
        {
            throw new NotImplementedException();
        }
    }
}
