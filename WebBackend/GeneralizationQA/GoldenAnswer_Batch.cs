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
            var db = Configuration.GetFreebaseDbProvider();

            Console.WriteLine("Substitution trace: " + substitution.OriginalTrace.ToString());
            Console.WriteLine("Rank: " + substitution.Rank);
            Console.WriteLine("Substitution node: {0} ({1})", db.GetLabel(FreebaseLoader.GetMid(substitution.Substitution.Data)), substitution.Substitution);
            foreach (var node in substitution.OriginalTrace.CurrentNodes.Take(20))
            {
                Console.WriteLine("\t{0} ({1})", db.GetLabel(FreebaseLoader.GetMid(node.Data)), node);
            }
        }

        internal static void RunAnswerGeneralizationDev()
        {
            var trainDataset = Configuration.GetQuestionDialogsTrain();
            var devDataset = Configuration.GetQuestionDialogsDev();

            var simpleQuestions = Configuration.GetSimpleQuestionsDump();
            var db = Configuration.GetFreebaseDbProvider();

            var trainDialogs = trainDataset.Dialogs.ToArray();
            var linkedUtterancesTrain = cachedLinkedUtterancesTrain(simpleQuestions, db, trainDialogs);

            //var graph = cachedEntityGraph(simpleQuestions, trainDialogs, linkedUtterancesTrain);

            var graph = new ComposedGraph(new FreebaseGraphLayer(db));

            var linker = new GraphDisambiguatedLinker(db, "./verbs.lex");
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

            /*/
            //evaluation on dev set
            foreach (var devDialog in trainDialogs)
            {
                writeLine(devDialog.Question);
                writeLine("\t" + cachedLinker.LinkUtterance(devDialog.Question));
                var desiredAnswerLabel = db.GetLabel(devDialog.AnswerMid);
                writeLine("\tDesired answer: {0} ({1})", desiredAnswerLabel, devDialog.AnswerMid);
                var answer = generalizer.GetAnswer(devDialog.Question);
                if (answer == null)
                {
                    writeLine("\tNo answer.");
                }
                else
                {
                    var answerLabel = db.GetLabel(FreebaseLoader.GetMid(answer.Value.Data));
                    writeLine("\tGeneralizer output: {0} {1}", answerLabel, answer);
                }
                writeLine();
            }
            /**/
            var result = generalizer.GetAnswer("What county is ovens auditorium in");
            //var result = generalizer.GetAnswer("What is Obama gender?");
            //var result = generalizer.GetAnswer("is mir khasim ali of the male or female gender");
        }


        private static void logWriteLine(string format = "", params object[] args)
        {
            Console.WriteLine(format, args);
            var writer = new StreamWriter("GoldAnswerBatch.log", true);
            writer.WriteLine(format, args);
            writer.Close();
        }

        internal static void RunEvaluation()
        {
            var trainDataset = Configuration.GetQuestionDialogsTrain();
            var devDataset = Configuration.GetQuestionDialogsDev();

            var simpleQuestions = Configuration.GetSimpleQuestionsDump();
            var db = Configuration.GetFreebaseDbProvider();

            var trainDialogs = trainDataset.Dialogs.ToArray();
            var linkedUtterances = cachedLinkedUtterancesTrain(simpleQuestions, db, trainDialogs);

            var graph = cachedEntityGraph(simpleQuestions, trainDialogs, linkedUtterances);


            printInfo(graph, db, "0kpv11", "0drcrdd");


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

        internal static void RunGraphMIExperiment()
        {
            var trainDataset = Configuration.GetQuestionDialogsTrain();
            var devDataset = Configuration.GetQuestionDialogsDev();

            var db = Configuration.GetFreebaseDbProvider();
            var graph = new ComposedGraph(new FreebaseGraphLayer(db));

            var trainDialogs = trainDataset.Dialogs.ToArray();
            var simpleQuestions = Configuration.GetSimpleQuestionsDump();
            var linkedUtterances = cachedLinkedUtterancesTrain(simpleQuestions, db, trainDialogs);
            var linkedUtterancesTrain = cachedLinkedUtterancesTrain(simpleQuestions, db, trainDialogs);
            var linker = new GraphDisambiguatedLinker(db, "./verbs.lex");
            var cachedLinker = new CachedLinker(trainDialogs.Select(d => d.Question).ToArray(), linkedUtterancesTrain, linker);

            var totalNgramCounts = new Dictionary<string, int>();
            var totalEdgeCounts = new Dictionary<Edge, int>();
            var ngramEdgeCounts = new Dictionary<Tuple<string, Edge>, int>();
            foreach (var dialog in trainDataset.Dialogs)
            {
                var questionNgrams = getQuestionNgrams(dialog, 4, cachedLinker);
                var linkedQuestion = cachedLinker.LinkUtterance(dialog.Question);

                Console.WriteLine(dialog.Question);
                var answerNode = graph.GetNode(db.GetFreebaseId(dialog.AnswerMid));
                var targets = graph.GetNeighbours(answerNode, 100);

                var questionEntities = linkedQuestion.Parts.SelectMany(p => p.Entities.Select(e => db.GetFreebaseId(e.Mid))).ToArray();
                var edges = new HashSet<Edge>();
                foreach (var target in targets)
                {
                    var edge = target.Item1;
                    var targetId = target.Item2.Data;
                    if (!edges.Add(edge))
                        continue;

                    if (!questionEntities.Contains(targetId))
                        continue;

                    foreach (var rawNgram in questionNgrams)
                    {
                        if (!rawNgram.Contains(targetId))
                            continue;

                        var ngram = rawNgram.Replace(targetId, "$");

                        int count;
                        var key = Tuple.Create(ngram, edge);
                        ngramEdgeCounts.TryGetValue(key, out count);
                        ngramEdgeCounts[key] = count + 1;

                        totalNgramCounts.TryGetValue(ngram, out count);
                        totalNgramCounts[ngram] = count + 1;

                        totalEdgeCounts.TryGetValue(edge, out count);
                        totalEdgeCounts[edge] = count + 1;
                    }
                }
            }

            var orderedCounts = ngramEdgeCounts.OrderBy(p => getPmi(p.Key, totalNgramCounts, totalEdgeCounts, ngramEdgeCounts));
            foreach (var pair in orderedCounts)
            {
                logWriteLine("{0} -> [{1},{2},{3}] {4:0.00}", pair.Key, pair.Value, totalNgramCounts[pair.Key.Item1], totalEdgeCounts[pair.Key.Item2], getPmi(pair.Key, totalNgramCounts, totalEdgeCounts, ngramEdgeCounts));
            }
        }

        private static double getPmi(Tuple<string, Edge> evt, Dictionary<string, int> ngrams, Dictionary<Edge, int> edges, Dictionary<Tuple<string, Edge>, int> events)
        {
            var eventSum = events.Values.Sum();
            var edgesSum = edges.Values.Sum();
            var ngramsSum = ngrams.Values.Sum();

            var pEdge = 1.0 * edges[evt.Item2] / edgesSum;
            var pEvt = 1.0 * events[evt] / eventSum;
            var pNgram = 1.0 * ngrams[evt.Item1] / ngramsSum;

            return Math.Log(pEvt / pNgram / pEdge);
        }

        private static IEnumerable<string> getQuestionNgrams(QuestionDialog dialog, int n, CachedLinker linker)
        {
            var result = new HashSet<string>();
            for (var i = 2; i <= n; ++i)
            {

                var question = dialog.Question;
                //result.UnionWith(getNgrams(question, n));

                var linkedQuestion = linker.LinkUtterance(question);
                result.UnionWith(linkedQuestion.GetNgrams(i));
                /*
                foreach (var explanation in dialog.ExplanationTurns)
                {
                    result.UnionWith(getNgrams(explanation.InputChat, i));
                }*/
            }

            return result;
        }


        private static IEnumerable<string> getNgrams(string utterance, int n)
        {
            var words = utterance.ToLowerInvariant().Replace(",", " , ").Replace("?", " ? ").Replace(".", " . ").Replace("\"", " \" ").Replace("!", " ! ").Split(' ').ToArray();

            var result = new List<string>();
            for (var i = 0; i < words.Length; ++i)
            {
                var ngram = new StringBuilder();
                for (var j = 0; j < n; ++j)
                {
                    var index = i + j;
                    var word = index >= words.Length ? "</s>" : words[index];
                    if (ngram.Length > 0)
                        ngram.Append(",");

                    ngram.Append(word);
                }

                result.Add(ngram.ToString());
            }
            return result;
        }

        private static void printInfo(ComposedGraph graph, FreebaseDbProvider db, params string[] ids)
        {
            foreach (var id in ids)
            {
                var mid = FreebaseLoader.GetMid(id);

                var label = db.GetLabel(mid);
                var description = db.GetDescription(mid);

                Console.WriteLine(id + " " + label);
                Console.WriteLine("\t" + description);
                Console.WriteLine();
            }
        }

        private static ComposedGraph cachedEntityGraph(SimpleQuestionDumpProcessor simpleQuestions, QuestionDialog[] trainDialogs, LinkedUtterance[] linkedUtterances)
        {
            return ComputationCache.Load("knowledge_all_train", 1, () =>
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

        private static LinkedUtterance[] cachedLinkedUtterancesTrain(SimpleQuestionDumpProcessor simpleQuestions, FreebaseDbProvider db, QuestionDialog[] trainDialogs)
        {
            var linkedUtterances = ComputationCache.Load("linked_all_train", 1, () =>
            {
                var linker = new GraphDisambiguatedLinker(db, "./verbs.lex");

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
