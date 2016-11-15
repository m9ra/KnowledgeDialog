using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

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

        internal static void RunToyGeneralization()
        {
            var data = new ExplicitLayer();
            var A = data.CreateReference("A");
            var B = data.CreateReference("B");
            var C = data.CreateReference("C");
            var D = data.CreateReference("D");

            var H1 = data.CreateReference("H1");
            var H2 = data.CreateReference("H2");
            var H3 = data.CreateReference("H3");
            var fH1 = data.CreateReference("fH1");
            var fH2 = data.CreateReference("fH2");
            var fH3 = data.CreateReference("fH3");
            var H = data.CreateReference("H");
            var X = data.CreateReference("X");

            var edge1 = "e1";
            var edgeZ1 = "z1";
            var edgeZ2 = "z2";
            var edgeIs = "is";

            data.AddEdge(A, edge1, H1);
            data.AddEdge(B, edge1, H2);
            data.AddEdge(C, edge1, H3);

            data.AddEdge(A, edge1, fH1);
            data.AddEdge(B, edge1, fH2);
            data.AddEdge(C, edge1, fH3);

            data.AddEdge(H1, edgeIs, H);
            data.AddEdge(H2, edgeIs, H);
            data.AddEdge(H3, edgeIs, H);

            data.AddEdge(D, edgeZ1, H1);
            data.AddEdge(D, edgeZ2, H2);

            var graph = new ComposedGraph(data);

            var group = new KnowledgeDialog.Knowledge.Group(graph);
            group.AddNode(A);
            group.AddNode(B);

            var linker = new SingleWordLinker();
            linker.Add(A, B, C, D, H1, H2, H3);

            var generalizer = new PatternGeneralizer(graph, linker.LinkUtterance);

            generalizer.AddExample("Where A lives?", H1);
            generalizer.AddExample("Where B lives?", H2);
            var answer = generalizer.GetAnswer("Where does C live?");
        }

        internal static void DebugInfo(PathSubstitution substitution)
        {
            var extractor = new AnswerExtraction.EntityExtractor(@"C:\REPOSITORIES\lucene_freebase_v1_index");
            extractor.LoadIndex();

            Console.WriteLine("Substitution trace: " + substitution.OriginalTrace.ToString());
            Console.WriteLine("Rank: " + substitution.Rank);
            Console.WriteLine("Substitution node: {0} ({1})", extractor.GetLabel(FreebaseLoader.GetMid(substitution.Substitution.Data)), substitution.Substitution);
            foreach (var node in substitution.OriginalTrace.CurrentNodes.Take(20))
            {
                Console.WriteLine("\t{0} ({1})", extractor.GetLabel(FreebaseLoader.GetMid(node.Data)), node);
            }
        }

        internal static void RunAnswerLoadingTest()
        {
            var trainDataset = new QuestionDialogDatasetReader("question_dialogs-train.json");
            var devDataset = new QuestionDialogDatasetReader("question_dialogs-dev.json");

            var simpleQuestions = new SimpleQuestionDumpProcessor(@"C:\Databases\SimpleQuestions_v2\SimpleQuestions_v2\freebase-subsets\freebase-FB2M.txt");
            var extractor = new AnswerExtraction.EntityExtractor(@"C:\REPOSITORIES\lucene_freebase_v1_index");
            extractor.LoadIndex();

            var trainDialogs = trainDataset.Dialogs.ToArray();
            var linkedUtterancesTrain = cachedLinkedUtterancesTrain(simpleQuestions, extractor, trainDialogs);

            var graph = cachedEntityGraph(simpleQuestions, trainDialogs, linkedUtterancesTrain);

            var linker = new GraphDisambiguatedLinker(extractor, "./verbs.lex");
            var cachedLinker = new CachedLinker(trainDialogs.Select(d => d.Question).ToArray(), linkedUtterancesTrain, linker);
            var generalizer = new PatternGeneralizer(graph, cachedLinker.LinkUtterance);
            var testDialogs = 0;

            //train
            for (var i = 0; i < trainDialogs.Length - testDialogs; ++i)
            {
                var trainDialog = trainDialogs[i];
                var question = trainDialog.Question;
                var answerNodeId = FreebaseLoader.GetId(trainDialog.AnswerMid);
                var answerNode = graph.GetNode(answerNodeId);

                generalizer.AddExample(question, answerNode);
            }

            /**/
            //test
            foreach (var devDialog in trainDialogs)
            {
                writeLine(devDialog.Question);
                writeLine("\t" + cachedLinker.LinkUtterance(devDialog.Question));
                var desiredAnswerLabel = extractor.GetLabel(devDialog.AnswerMid);
                writeLine("\tDesired answer: {0} ({1})", desiredAnswerLabel, devDialog.AnswerMid);
                var answer = generalizer.GetAnswer(devDialog.Question);
                if (answer == null)
                {
                    writeLine("\tNo answer.");
                }
                else
                {
                    var answerLabel = extractor.GetLabel(FreebaseLoader.GetMid(answer.Value.Data));
                    writeLine("\tGeneralizer output: {0} {1}", answerLabel, answer);
                }
                writeLine();
            }
            /**/
            //var result = generalizer.GetAnswer("What is Obama gender?");
            //var result = generalizer.GetAnswer("is mir khasim ali of the male or female gender");
        }

        private static void writeLine(string format = "", params object[] args)
        {
            Console.WriteLine(format, args);
            var writer = new StreamWriter("GoldAnswerBatch.log", true);
            writer.WriteLine(format, args);
            writer.Close();
        }

        internal static void RunEvaluation()
        {
            var trainDataset = new QuestionDialogDatasetReader("question_dialogs-train.json");
            var devDataset = new QuestionDialogDatasetReader("question_dialogs-dev.json");

            var simpleQuestions = new SimpleQuestionDumpProcessor(@"C:\Databases\SimpleQuestions_v2\SimpleQuestions_v2\freebase-subsets\freebase-FB2M.txt");
            var extractor = new AnswerExtraction.EntityExtractor(@"C:\REPOSITORIES\lucene_freebase_v1_index");
            extractor.LoadIndex();

            var trainDialogs = trainDataset.Dialogs.ToArray();
            var linkedUtterances = cachedLinkedUtterancesTrain(simpleQuestions, extractor, trainDialogs);

            var graph = cachedEntityGraph(simpleQuestions, trainDialogs, linkedUtterances);


            printInfo(graph, extractor, "0kpv11", "0drcrdd");


            var qaModule = new ProbabilisticQAModule(graph, new KnowledgeDialog.Database.CallStorage(null));
            for (var i = 0; i < trainDialogs.Length; ++i)
            {
                var dialog = trainDialogs[i];
                var linkedUtterance = linkedUtterances[i];
                var entityUtterance = getEntityUtterance(linkedUtterance);
                var hasDuplicitWords = entityUtterance.Split(' ').Distinct().Count() != entityUtterance.Split(' ').Count();

                if (hasDuplicitWords)
                    //TODO skip utterances with duplicit words for now
                    continue;
                var answerNode = getNode(dialog.AnswerMid, graph);
                qaModule.AdviceAnswer(entityUtterance, false, answerNode);
            }


            qaModule.Optimize(1000);

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
            return ComputationCache.Load("knowledge_all_train3", 1, () =>
             {
                 var trainEntities = getQAEntities(trainDialogs, linkedUtterances);
                 //var layer = simpleQuestions.GetLayerFromIds(trainEntities);

                 foreach (var entityId in trainEntities)
                 {
                     simpleQuestions.AddTargetMid(FreebaseLoader.GetMid(entityId));
                 }
                 simpleQuestions.RunIteration();
                 var layer = simpleQuestions.GetLayerFromIds(simpleQuestions.AllIds);
                 var graph = new ComposedGraph(layer);
                 return graph;
             });
        }

        private static LinkedUtterance[] cachedLinkedUtterancesTrain(SimpleQuestionDumpProcessor simpleQuestions, EntityExtractor extractor, QuestionDialog[] trainDialogs)
        {
            var linkedUtterances = ComputationCache.Load("linked_all_train", 1, () =>
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
