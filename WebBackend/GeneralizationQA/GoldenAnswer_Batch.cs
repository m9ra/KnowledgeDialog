using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Dataset;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation;
using KnowledgeDialog.PoolComputation.ProbabilisticQA;

using WebBackend.AnswerExtraction;

namespace WebBackend.GeneralizationQA
{
    class GoldenAnswer_Batch
    {
        internal static void RunEvaluation()
        {
            var trainDataset = new QuestionDialogDatasetReader("question_dialogs-train.json");
            var devDataset = new QuestionDialogDatasetReader("question_dialogs-dev.json");

            var simpleQuestions = new SimpleQuestionDumpProcessor(@"C:\REPOSITORIES\Wikidata-Toolkit\wdtk-examples\dumpfiles\freebase-FB2M.txt");
            var layer = simpleQuestions.GetLayer();
            var graph = new ComposedGraph(layer);

            var qaModule = new ProbabilisticQAModule(graph, new KnowledgeDialog.Database.CallStorage(null));
            foreach (var dialog in trainDataset.Dialogs)
            {
                var answerNode = getNode(dialog.AnswerMid, graph);
                qaModule.AdviceAnswer(dialog.Question, false, answerNode);
            }


            qaModule.Optimize(100);

            var correctAnswers = 0;
            var totalDialogs = 0;
            foreach (var dialog in devDataset.Dialogs)
            {
                ++totalDialogs;

                //TODO add interactive learning
                var answerNode = getNode(dialog.AnswerMid, graph);
                var question = dialog.Question;

                var pool = new ContextPool(graph);
                var resultAnswer = qaModule.GetRankedAnswer(question, pool);
                if (resultAnswer != null && resultAnswer.Value.Equals(answerNode))
                    ++correctAnswers;
            }

            Console.WriteLine("Precision: {0:0.00}%", 100.0 * correctAnswers / totalDialogs);
        }

        static NodeReference getNode(string freebaseMid, ComposedGraph graph)
        {
            if (!freebaseMid.StartsWith(FreebaseLoader.IdPrefix))
                throw new NotSupportedException("Invalid id.");

            var id = freebaseMid.Substring(FreebaseLoader.IdPrefix.Length);
            return graph.GetNode(id);
        }
    }
}
