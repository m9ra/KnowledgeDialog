using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.QuestionAnswering;

using KnowledgeDialog.Database;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.ProbabilisticQA
{
    class ProbabilisticQAManager : IQuestionAnswerManager
    {
        private readonly ProbabilisticQAModule _module;

        private readonly ContextPool _pool;

        internal ProbabilisticQAManager(ComposedGraph graph, CallStorage storage)
        {
            _pool = new ContextPool(graph);
            _module = new ProbabilisticQAModule(_pool.Graph, storage);

        }

        /// <inheritdoc/>
        public void InitializeNewDialog()
        {
            _pool.ClearAccumulator();
        }

        /// <inheritdoc/>
        public QuestionAnswerReceiveResult ReceiveQuestionPart(IEnumerable<TurnLog> questionTurns)
        {
            var answer = _module.GetRankedAnswer(questionTurns.Last().Text, _pool);
            if (answer.Rank < 0.8)
                return QuestionAnswerReceiveResult.HintNeeded(answer.Rank);

            return QuestionAnswerReceiveResult.From(answer);
        }

        /// <inheritdoc/>
        public QuestionAnswerReceiveResult ReceiveExplanationPart(IEnumerable<TurnLog> explanationTurns)
        {
            //we now cannot utilize explanations
            return QuestionAnswerReceiveResult.HintNeeded(0.0);
        }

        /// <inheritdoc/>
        public QuestionAnswerReceiveResult ReceiveAnswerPart(IEnumerable<TurnLog> answerTurns)
        {
            var lastTurn = answerTurns.Last();
            var parsedUtterance = UtteranceParser.Parse(lastTurn.Text);
            foreach (var word in parsedUtterance.Words)
            {
                if (_pool.Graph.HasEvidence(word))
                {
                    var answer = new[] { _pool.Graph.GetNode(word) };
                    return QuestionAnswerReceiveResult.From(new Ranked<IEnumerable<NodeReference>>(answer, 1.0));
                }
            }
            throw new NotImplementedException("Parse answer");
        }
    }
}
