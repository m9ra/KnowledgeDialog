using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.ModifiableResponses
{
    class FrameResponse : ModifiableResponse
    {
        internal readonly ConversationFrameBase Frame;

        internal FrameResponse(ConversationFrameBase frame)
        {
            Frame = frame;
        }

        public override ResponseBase CreateResponse()
        {
            throw new NotSupportedException("Cannot create response directly");
        }

        public override bool Modify(string modification)
        {
            return false;
        }
    }
}
