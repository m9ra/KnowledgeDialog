using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Dataset;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

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

            var simpleQuestions = new SimpleQuestionDumpProcessor(@"C:\Databases\SimpleQuestions_v2\SimpleQuestions_v2\freebase-subsets\freebase-FB2M.txt");
            var extractor = new AnswerExtraction.EntityExtractor(@"C:\REPOSITORIES\lucene_freebase_v1_index");
            extractor.LoadIndex();

            var trainDialogs = trainDataset.Dialogs.Take(1).ToArray();
            var linkedUtterances = cachedLinkedUtterances(simpleQuestions, extractor, trainDialogs);

            var graph = cachedEntityGraph(simpleQuestions, trainDialogs, linkedUtterances);


            printInfo(graph, extractor, "0kpv11", "0drcrdd");


            var qaModule = new ProbabilisticQAModule(graph, new KnowledgeDialog.Database.CallStorage(null));
            for (var i = 0; i < trainDialogs.Length; ++i)
            {
                var dialog = trainDialogs[i];
                var linkedUtterance = linkedUtterances[i];
                var entityUtterance = getEntityUtterance(linkedUtterance);
                var answerNode = getNode(dialog.AnswerMid, graph);
                qaModule.AdviceAnswer(entityUtterance, false, answerNode);
            }


            qaModule.Optimize(100000);

            var correctAnswers = 0;
            var totalDialogs = 0;
            for (var i = 0; i < trainDialogs.Length; ++i)
            {
                var dialog = trainDialogs[i];
                var linkedUtterance = linkedUtterances[i];
                var entityUtterance = getEntityUtterance(linkedUtterance);
                ++totalDialogs;

                //TODO add interactive learning
                var answerNode = getNode(dialog.AnswerMid, graph);

                var pool = new ContextPool(graph);
                var resultAnswer = qaModule.GetRankedAnswer(entityUtterance, pool);
                var predictedNode = resultAnswer.Value.FirstOrDefault();
                if (predictedNode != null && predictedNode.Equals(answerNode))
                    ++correctAnswers;
                Console.WriteLine("Desired: " + answerNode.ToString());
                Console.WriteLine(linkedUtterance);
                Console.WriteLine();
            }

            Console.WriteLine("Precision: {0:0.00}%", 100.0 * correctAnswers /
totalDialogs);
            Console.ReadKey();
        }

        private static void printInfo(ComposedGraph graph, EntityExtractor extractor, params string[] ids)
        {
            foreach (var id in ids)
            {
                var mid = FreebaseLoader.GetMid(id);

                var label = extractor.GetLabel(mid);
                var description = extractor.GetDescription(mid);

                Console.WriteLine(id + " " + label);
                Console.WriteLine("\t" + description);
                Console.WriteLine();
            }
        }

        private static ComposedGraph cachedEntityGraph(SimpleQuestionDumpProcessor simpleQuestions, QuestionDialog[] trainDialogs, LinkedUtterance[] linkedUtterances)
        {
            return ComputationCache.Load("knowledge_20_train", 1, () =>
             {
                 var trainEntities = getQAEntities(trainDialogs, linkedUtterances);

                 var layer = simpleQuestions.GetLayerFromIds(trainEntities);
                 var graph = new ComposedGraph(layer);
                 return graph;
             });
        }

        private static LinkedUtterance[] cachedLinkedUtterances(SimpleQuestionDumpProcessor simpleQuestions, EntityExtractor extractor, QuestionDialog[] trainDialogs)
        {
            var linkedUtterances = ComputationCache.Load("linked_20_train", 1, () =>
            {
                var linker = new GraphDisambiguatedLinker(extractor, "./verbs.lex");
                linker.RegisterDisambiguationEntities(trainDialogs.Select(d => d.Question));
                linker.LoadDisambiguationEntities(simpleQuestions);

                var linkedUtterancesList = new List<LinkedUtterance>();
                foreach (var dialog in trainDialogs)
                {
                    var linkedUtterance = linker.LinkUtterance(dialog.Question, 1).First();
                    linkedUtterancesList.Add(linkedUtterance);
                }
                return linkedUtterancesList;
            }).ToArray();
            return linkedUtterances;
        }

        private static IEnumerable<string> getQAEntities(QuestionDialog[] trainDialogs, LinkedUtterance[] utterances)
        {
            var result = new List<string>();
            foreach (var dialog in trainDialogs)
            {
                result.Add(FreebaseLoader.GetId(dialog.AnswerMid));
            }

            foreach (var utterance in utterances)
            {
                result.AddRange(utterance.Parts.SelectMany(p => p.Entities).Select(e => FreebaseLoader.GetId(e.Mid)));
            }

            return result;
        }

        static NodeReference getNode(string freebaseMid, ComposedGraph graph)
        {
            var id = FreebaseLoader.GetId(freebaseMid);
            return graph.GetNode(id);
        }

        static string getEntityUtterance(LinkedUtterance linkedUtterance)
        {
            var result = new List<string>();
            foreach (var part in linkedUtterance.Parts)
            {
                if (part.Entities.Any())
                    result.Add(FreebaseLoader.GetId(part.Entities.First().Mid));
                else
                    result.Add(part.Token);
            }

            return string.Join(" ", result);
        }
    }
}
