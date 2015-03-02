using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PoolComputation.ModifiableResponses;

namespace KnowledgeDialog.PoolComputation.Frames
{
    class QuestionAnsweringFrame : ConversationFrameBase
    {
        public static readonly int MaximumUserReport = 2;

        public static readonly int MaximumWidth = 100;

        protected ContextPool Pool { get { return _context.Pool; } }

        private readonly string NoResults = "I have no matching data";

        private readonly DialogContext _context;

        private string _lastQuestion;

        public QuestionAnsweringFrame(ConversationContext conversationContext, DialogContext context)
            : base(conversationContext)
        {
            _context = context;
            EnsureInitialized<PoolActionMapping>(() => new PoolActionMapping());
        }

        protected override ModifiableResponse FrameInitialization()
        {
            return DefaultHandler();
        }

        protected override ModifiableResponse DefaultHandler()
        {
            var utterance = CurrentInput;
            if (utterance.StartsWith("it is", StringComparison.InvariantCultureIgnoreCase))
            {
                return Response(new QueryAdviceFrame(ConversationContext, _lastQuestion, _context));
            }

            var hypotheses = Get<PoolActionMapping>().GetActions(utterance, Pool);
            var bestHypothesis = hypotheses.FirstOrDefault();

            if (bestHypothesis == null)
            {
                return Response(new QueryAdviceFrame(ConversationContext, utterance, _context));
            }


            _lastQuestion = utterance;

            Pool.SetSubstitutions(bestHypothesis.Substitutions);
            foreach (var action in bestHypothesis.Actions)
            {
                action.Run(Pool);
            }

            if (Pool.ActiveCount <= MaximumUserReport)
            {
                if (!Pool.HasActive)
                {
                    return Response(NoResults);
                }
                else
                {
                    return Response("It is", Pool.ActiveNodes);
                }
            }
            else
            {
                throw new NotImplementedException("Find criterion");
            }
        }

        protected ModifiableResponse acceptAdvice()
        {
            throw new NotImplementedException();
        }
    }
}
