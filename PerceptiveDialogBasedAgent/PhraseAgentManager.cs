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
        private bool _hadInformativeInput = false;

        public bool HadInformativeInput => _hadInformativeInput;

        public bool CanBeCompleted => true;

        private readonly RestaurantAgent _agent = new RestaurantAgent();

        public override ResponseBase Initialize()
        {
            return new SimpleResponse("Hello, how can I help you?");
        }

        public override ResponseBase Input(ParsedUtterance utterance)
        {
            //Database.DebugTrigger(849);
            var response = _agent.Input(utterance.OriginalSentence);
            var pricerangeSpecifier = _agent.RestaurantSpecifier("pricerange");
            if (pricerangeSpecifier == "expensive")
                _hadInformativeInput = true;

            return new SimpleResponse(response);
        }
    }
}
