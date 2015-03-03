using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.PoolComputation.PoolActions;

using KnowledgeDialog.PoolComputation.Frames;
using KnowledgeDialog.PoolComputation.ModifiableResponses;

namespace KnowledgeDialog.PoolComputation
{
    public class DialogManager : IDialogManager
    {
        internal readonly DialogContext Context;

        internal readonly ConversationContext ConversationContext;

        private readonly ComposedGraph _graph;

        private readonly List<string> _lastWindowSentences = new List<string>();

        private readonly List<ModifiableResponse> _responses = new List<ModifiableResponse>();

        private readonly Stack<ConversationFrameBase> _framesStack = new Stack<ConversationFrameBase>();

        public DialogManager(params GraphLayerBase[] layers)
        {
            _graph = new ComposedGraph(layers);
            Context = new DialogContext(_graph);
            ConversationContext = new ConversationContext(_graph);
            _framesStack.Push(new Frames.QuestionAnsweringFrame(ConversationContext, Context));
        }

        #region Frame stack control

        ConversationFrameBase tryGetManagerFrame(string utterance)
        {
            var prefix = "you should say";
            if (utterance!=null && utterance.Contains(prefix))
                return new ChangeResponseFrame(ConversationContext, _responses.Last());

            return null;
        }

        ConversationFrameBase getCurrentFrame(string utterance)
        {
            var managerFrame = tryGetManagerFrame(utterance);
            if (managerFrame != null)
                //default manager frame is available
                _framesStack.Push(managerFrame);

            while (_framesStack.Peek().IsComplete)
            {
                _framesStack.Pop();
            }

            return _framesStack.Peek();
        }

        ResponseBase getResponse(string utterance)
        {
            ResponseBase response = null;
            do
            {
                var frame = getCurrentFrame(utterance);
                var modifiableResponse = frame.Input(utterance);
                if (modifiableResponse == null)
                {
                    if (frame.IsComplete)
                        continue;
                    else
                        throw new NotSupportedException("Cannot accept null response");
                }



                //utterance can be processed only once
                utterance = null;
                if (modifiableResponse is FrameResponse)
                {
                    var frameResponse = modifiableResponse as FrameResponse;
                    _framesStack.Push(frameResponse.Frame);
                }
                else
                {
                    _responses.Add(modifiableResponse);
                    response = modifiableResponse.CreateResponse();
                }
            } while (response == null);
            return response;
        }

        #endregion

        #region Utterance routing

        public ResponseBase Ask(string question)
        {
            return getResponse(question);
        }

        public ResponseBase Negate()
        {
            throw new NotImplementedException();
        }

        public ResponseBase Advise(string question, string answer)
        {
            //TODO this is hack
            var originalUtterance = question + " is " + answer;

            return getResponse(originalUtterance);
        }

        #endregion
    }
}
