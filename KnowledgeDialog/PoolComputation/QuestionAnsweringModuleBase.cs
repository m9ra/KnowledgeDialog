using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    public abstract class QuestionAnsweringModuleBase
    {
        private readonly object _L_input = new object();

        private readonly CallSerializer _adviceAnswer;

        private readonly CallSerializer _repairAnswer;

        private readonly CallSerializer _setEquivalencies;

        private readonly CallSerializer _negate;

        internal readonly CallStorage Storage;

        internal readonly ContextPool Pool;

        internal ComposedGraph Graph { get { return Pool.Graph; } }

        internal QuestionAnsweringModuleBase(ComposedGraph graph, CallStorage storage)
        {
            Storage = storage;
            Pool = new ContextPool(graph);

            _adviceAnswer = storage.RegisterCall("AdviceAnswer", c =>
            {
                _AdviceAnswer(c.String("question"), c.Bool("isBasedOnContext"), c.Node("correctAnswerNode", Graph), c.Nodes("context", Graph));
            });

            _repairAnswer = storage.RegisterCall("RepairAnswer", c =>
            {
                _RepairAnswer(c.String("question"), c.Node("suggestedAnswer", Graph), c.Nodes("context", Graph));
            });

            _setEquivalencies = storage.RegisterCall("SetEquivalence", c =>
            {
                SetEquivalence(c.String("patternQuestion"), c.String("queriedQuestion"), c.Bool("isEquivalent"));
            });

            _negate = storage.RegisterCall("Negate", c =>
            {
                Negate(c.String("question"));
            });
        }

        #region Template methods

        protected abstract bool adviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context);

        protected abstract void repairAnswer(string question, NodeReference suggestedAnswer, IEnumerable<NodeReference> context);

        protected abstract void setEquivalence(string patternQuestion, string queriedQuestion, bool isEquivalent);

        protected abstract void negate(string question);

        #endregion

        #region Input methods

        public bool AdviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode)
        {
            lock (_L_input)
            {
                return _AdviceAnswer(question, isBasedOnContext, correctAnswerNode, Pool.ActiveNodes);
            }
        }

        private bool _AdviceAnswer(string question, bool isBasedOnContext, NodeReference correctAnswerNode, IEnumerable<NodeReference> context)
        {
            _adviceAnswer.ReportParameter("question", question);
            _adviceAnswer.ReportParameter("isBasedOnContext", isBasedOnContext);
            _adviceAnswer.ReportParameter("correctAnswerNode", correctAnswerNode);
            _adviceAnswer.ReportParameter("context", context);
            _adviceAnswer.SaveReport();

            return adviceAnswer(question, isBasedOnContext, correctAnswerNode, context);
        }

        public void RepairAnswer(string question, NodeReference suggestedAnswer)
        {
            lock (_L_input)
            {
                _RepairAnswer(question, suggestedAnswer, Pool.ActiveNodes);
            }
        }

        public void _RepairAnswer(string question, NodeReference suggestedAnswer, IEnumerable<NodeReference> context)
        {
            _repairAnswer.ReportParameter("question", question);
            _repairAnswer.ReportParameter("suggestedAnswer", suggestedAnswer);
            _repairAnswer.ReportParameter("context", context);
            _repairAnswer.SaveReport();

            repairAnswer(question, suggestedAnswer, context);
        }

        public void SetEquivalence(string patternQuestion, string queriedQuestion, bool isEquivalent)
        {
            lock (_L_input)
            {
                _setEquivalencies.ReportParameter("patternQuestion", patternQuestion);
                _setEquivalencies.ReportParameter("queriedQuestion", queriedQuestion);
                _setEquivalencies.ReportParameter("isEquivalent", isEquivalent);
                _setEquivalencies.SaveReport();

                setEquivalence(patternQuestion, queriedQuestion, isEquivalent);
            }
        }



        public void Negate(string question)
        {
            lock (_L_input)
            {

                _negate.ReportParameter("question", question);
                _negate.SaveReport();

                negate(question);
            }
        }


        #endregion
    }
}
