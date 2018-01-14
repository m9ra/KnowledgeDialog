using KnowledgeDialog.DataCollection;
using KnowledgeDialog.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PerceptiveDialogBasedAgent.V2;
using KnowledgeDialog.Dialog.Responses;

namespace PerceptiveDialogBasedAgent
{
    public class PhraseAgentManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        public bool HadInformativeInput => throw new NotImplementedException();

        public bool CanBeCompleted => throw new NotImplementedException();

        private readonly RestaurantAgent _agent = new RestaurantAgent();

        public override ResponseBase Initialize()
        {
            return new SimpleResponse("Hello, how can I help you?");
        }

        public override ResponseBase Input(ParsedUtterance utterance)
        {
            var response = _agent.Input(utterance.OriginalSentence);
            return new SimpleResponse(response);
        }
    }
}
