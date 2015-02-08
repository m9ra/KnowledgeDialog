using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    /// <summary>
    /// Pattern based implementation of dialog manager.
    /// </summary>
    public class DialogManager : IDialogManager
    {
        /// <summary>
        /// Context where evaluations are processed.
        /// </summary>
        internal readonly DialogContext Context = new DialogContext();

        /// <summary>
        /// Question that can be negated if any (null otherwise).
        /// </summary>
        private string _lastQuestion;

        /// <summary>
        /// Layer where dialog inputs are kept.
        /// </summary>
        private readonly MultiTurnDialogLayer _dialogLayer = new MultiTurnDialogLayer();

        /// <summary>
        /// Data layers that are used as knowledge base.
        /// </summary>
        private readonly GraphLayerBase[] _dataLayers;

        public DialogManager(params GraphLayerBase[] layers)
        {
            _dataLayers = layers.ToArray();
        }


        /// <summary>
        /// Fill dialog layer with given sentence.
        /// </summary>
        /// <param name="sentence">Sentence that will be used for layer filling.</param>
        /// <param name="dialogLayer">Layer that will be filled.</param>
        public static void FillDialogLayer(MultiTurnDialogLayer dialogLayer, string sentence)
        {
            var words = sentence.Split(' ');
            dialogLayer.ActivateNewSentence();
            foreach (var word in words)
            {
                dialogLayer.AddSentenceWord(word);
            }
        }

        /// <inheritdoc/>
        public ResponseBase Ask(string question)
        {
            _lastQuestion = question;

            var graph = createNewTurnGraph(question);
            Context.NewTurn(graph);

            return Context.CreateActualResponse();
        }

        /// <inheritdoc/>
        public ResponseBase Negate()
        {
            return Context.Negate();
        }

        /// <inheritdoc/>
        public ResponseBase Advise(string question, string answer)
        {
            _lastQuestion = null;

            IEnumerable<NodeReference> inputContextNodes;
            var graph = createNewTurnGraph(question, out inputContextNodes);
            Context.NewTurn(graph);

            var answerResponse = parseAnswer(answer);
            var closestPattern = Context.FindClosestPattern(answerResponse);
            if (closestPattern == null)
            {
                var newPatterns = Context.CreatePatterns(answerResponse, inputContextNodes);
                var patternCount = newPatterns.Count();
                foreach (var newPattern in newPatterns)
                {
                    newPattern.Scale = 1.0 / patternCount;
                    Context.ActivatePattern(newPattern);
                }
            }
            else
            {
                var features = Context.FindCurrentBestFeatures(answerResponse);
                var falsePositives = Context.FindFalsePositives(features, answerResponse);
                if (falsePositives.Any())
                    throw new NotImplementedException("Create new pattern based on new features");
            }

            return Context.AdviceResponse();
        }

        /// <summary>
        /// Update current graph with given sentence.
        /// </summary>
        /// <param name="sentence">Sentence that will be added.</param>
        /// <returns>Updated graph.</returns>
        private ComposedGraph createNewTurnGraph(string sentence)
        {
            IEnumerable<NodeReference> inputContextNodes;
            return createNewTurnGraph(sentence, out inputContextNodes);
        }

        /// <summary>
        /// Update current graph with given sentence.
        /// </summary>
        /// <param name="sentence">Sentence that will be added.</param>
        /// <returns>Updated graph.</returns>
        private ComposedGraph createNewTurnGraph(string sentence, out IEnumerable<NodeReference> inputContextNodes)
        {
            FillDialogLayer(_dialogLayer, sentence);

            //TODO add other layers
            inputContextNodes = _dialogLayer.CurrentInputContextNodes;
            return new ComposedGraph(_dataLayers.Concat(new[] { _dialogLayer }).ToArray());
        }

        /// <summary>
        /// Parse given answer to answer node.
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        private ResponseBase parseAnswer(string answer)
        {
            int count;
            if (int.TryParse(answer, out count))
                return new CountResponse(count);

            return new SimpleResponse(answer);
        }
    }
}
