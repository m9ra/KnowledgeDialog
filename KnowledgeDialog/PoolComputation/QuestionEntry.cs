using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class QuestionEntry
    {
        private readonly Dictionary<NodeReference, AnswerReport> _answerCounts = new Dictionary<NodeReference, AnswerReport>();

        internal readonly string Question;

        internal readonly ParsedSentence ParsedQuestion;

        internal readonly NodesEnumeration QuestionNodes;

        internal bool HasAnswer { get { return _answerCounts.Count > 0; } }

        internal bool IsContextFree
        {
            get
            {
                var maxReport = getMaxReport();
                return maxReport.ContextFreeCounts >= maxReport.ContextBasedCounts;
            }
        }

        internal NodeReference CorrectAnswer { get { return getMaxReport().Answer; } }

        internal QuestionEntry(string question, ComposedGraph graph)
        {
            Question = question;
            ParsedQuestion = SentenceParser.Parse(question);
            QuestionNodes = findeQuestionNodes(graph);
        }

        private AnswerReport getMaxReport()
        {
            AnswerReport maxReport = null;
            var maxCount = int.MinValue;

            foreach (var report in _answerCounts.Values)
            {
                var count = report.ContextBasedCounts + report.ContextFreeCounts;
                if(count>maxCount){
                    maxCount = count;
                    maxReport = report;
                }
            }

            return maxReport;
        }

        private NodesEnumeration findeQuestionNodes(ComposedGraph graph)
        {
            var nodes = new List<NodeReference>();
            foreach (var word in ParsedQuestion.Words)
            {
                if (graph.HasEvidence(word))
                    nodes.Add(graph.GetNode(word));
            }
            return new NodesEnumeration(nodes);
        }

        internal void RegisterAnswer(bool isBasedOnContext, NodeReference answer)
        {
            AnswerReport report;
            if (!_answerCounts.TryGetValue(answer, out report))
                _answerCounts[answer] = report = new AnswerReport(this, answer);

            report.ReportOccurence(isBasedOnContext);
        }
    }
}
