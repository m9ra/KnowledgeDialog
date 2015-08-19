using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    class InterpretationsFactory
    {
        private Dialog.ParsedUtterance parsedQuestion;
        private bool isBasedOnContext;
        private Knowledge.NodeReference correctAnswerNode;
        private IEnumerable<Knowledge.NodeReference> context;

        public InterpretationsFactory(Dialog.ParsedUtterance parsedQuestion, bool isBasedOnContext, Knowledge.NodeReference correctAnswerNode, IEnumerable<Knowledge.NodeReference> context)
        {
            // TODO: Complete member initialization
            this.parsedQuestion = parsedQuestion;
            this.isBasedOnContext = isBasedOnContext;
            this.correctAnswerNode = correctAnswerNode;
            this.context = context;
        }
    }
}
