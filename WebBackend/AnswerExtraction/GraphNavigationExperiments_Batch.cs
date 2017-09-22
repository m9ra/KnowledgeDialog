using KnowledgeDialog.GraphNavigation;
using KnowledgeDialog.Knowledge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBackend.Dataset;
using WebBackend.Experiment;

namespace WebBackend.AnswerExtraction
{
    static class GraphNavigationExperiments_Batch
    {
        public static void PrintEdgeVotesInfo()
        {
            var data = new NavigationData("../cf_data.nvd");

            foreach (var edge in data.EdgeData)
            {
                Console.WriteLine(edge.Edge);
                foreach (var vote in edge.ExpressionVotes.OrderByDescending(v => v.Item2))
                {
                    Console.WriteLine("\t{0} > {1}", vote.Item2, vote.Item1);
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("END");
            Console.ReadLine();
        }

        public static void EvaluateLabelRequestInfo()
        {
            var db = Configuration.Db;
            var data = new GraphNavigationDataProvider(Program.Experiments, "graph_navigation");
            var linker = getBlacklistedLinker();

            var retrievedCount = 0;
            var totalCount = 0;
            var edgeWordCounts = new Dictionary<string, int>();

            foreach (var label in data.RequestedLabels)
            {
                totalCount += 1;
                // Console.WriteLine("\n" + label);

                var hints = data.GetLabelHints(label);
                var candidateEntities = new List<Tuple<FreebaseEntry, Edge, FreebaseEntry>>();
                foreach (var hint in hints)
                {
                    var linkedHint = linker.LinkUtterance(hint);
                    if (linkedHint == null)
                        continue;

                    //Console.WriteLine("\t" + linkedHint);
                    foreach (var entityInfo in linkedHint.Entities)
                    {
                        var entity = db.GetEntryFromMid(entityInfo.Mid);
                        //candidateEntities.Add(Tuple.Create(entity, Edge.From(".self.", true), entity));
                        //break;

                        var targetCount = 0;
                        foreach (var target in entity.Targets)
                        {
                            var targetEntity = db.GetEntryFromId(target.Item2);
                            if (targetEntity != null)
                                candidateEntities.Add(Tuple.Create(entity, target.Item1, targetEntity));

                            if (targetCount > 1000)
                                break;
                            targetCount += 1;
                        }
                    }
                }

                Tuple<FreebaseEntry, Edge, FreebaseEntry> retrievedCandidate = null;
                var isCandidateRetrieved = false;
                foreach (var candidateTarget in candidateEntities)
                {
                    var candidate = candidateTarget.Item3;

                    isCandidateRetrieved |= candidate.Aliases.Contains(label);
                    isCandidateRetrieved |= candidate.Label == label;

                    if (isCandidateRetrieved)
                    {
                        retrievedCandidate = candidateTarget;
                        break;
                    }
                }

                if (isCandidateRetrieved)
                {
                    retrievedCount += 1;
                    Console.WriteLine("\t candidate found in {3}: {0} {1} {2}", retrievedCandidate.Item1, retrievedCandidate.Item2, retrievedCandidate.Item3, candidateEntities.Count);

                    foreach (var hint in hints)
                    {
                        countHintNgrams(hint, retrievedCandidate, edgeWordCounts);
                    }
                }
                //Console.WriteLine("\t{0}/{1}", retrievedCount, totalCount);
            }

            /* return;
             foreach (var pair in edgeWordCounts.OrderBy(p => p.Value))
             {
                 Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
             }*/
        }

        private static void countHintNgrams(string hint, Tuple<FreebaseEntry, Edge, FreebaseEntry> target, Dictionary<string, int> counts)
        {
            var words = hint.ToLowerInvariant().Split(' ');
            foreach (var word in words)
            {
                var key = getEdgeKey(word, target);
                counts.TryGetValue(key, out int count);
                counts[key] = count + 1;
            }
        }

        private static string getEdgeKey(string ngram, Tuple<FreebaseEntry, Edge, FreebaseEntry> target)
        {
            return ngram + " " + target.Item2;
        }

        public static void ListUnknownEntityWordsQDD()
        {
            var train = Configuration.GetQuestionDialogsTrain();
            var linker = Configuration.Linker;
            var aliasModel = new Models.IdfAliasDetectionModel();
            foreach (var dialog in train.Dialogs)
            {
                var answerHintTurn = dialog.AnswerTurns.LastOrDefault();
                if (answerHintTurn == null)
                    continue;

                var answerHint = answerHintTurn.InputChat;
                var linkedQuestion = linker.LinkUtterance(dialog.Question);
                var linkedAnswerHint = linker.LinkUtterance(answerHint, linkedQuestion.Entities);

                if (linkedAnswerHint == null)
                    continue;

                aliasModel.Accept(linkedAnswerHint);
            }

            foreach (var phraseCount in aliasModel.NonEntityPhrasesInverseCounts.OrderBy(p => p.Value))
            {
                Console.WriteLine("{0}: {1}", phraseCount.Key, phraseCount.Value);
            }
        }

        private static ILinker getBlacklistedLinker()
        {
            var phrases = GraphNavigationExperiment.LoadPhrases(Configuration.GetQuestionDialogsTrain(), Configuration.Db);

            var coreLinker = new GraphDisambiguatedLinker(Configuration.Db, "./verbs.lex", useGraphDisambiguation: true);
            coreLinker.SetBlacklistLabels(phrases);

            var linker = new DiskCachedLinker("graph_navigation_blacklisted_linker.link", 1, (u, c) => coreLinker.LinkUtterance(u, c), coreLinker.Db);

            return linker;
        }
    }
}
