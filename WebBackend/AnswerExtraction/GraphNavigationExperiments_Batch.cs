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
        public static void EvaluateLabelRequestInfo()
        {
            var db = Configuration.Db;
            var data = new GraphNavigationDataProvider(Program.Experiments, "graph_navigation");
            var linker = getBlacklistedLinker();

            var retrievedCount = 0;
            var totalCount = 0;

            foreach (var label in data.RequestedLabels)
            {
                totalCount += 1;
                Console.WriteLine("\n" + label);

                var hints = data.GetLabelHints(label);
                var candidateEntities = new HashSet<FreebaseEntry>();
                foreach (var hint in hints)
                {
                    var linkedHint = linker.LinkUtterance(hint);
                    if (linkedHint == null)
                        continue;
                    Console.WriteLine("\t" + linkedHint);
                    foreach (var entityInfo in linkedHint.Entities)
                    {
                        var entity = db.GetEntryFromMid(entityInfo.Mid);
                        //candidateEntities.Add(entity);

                        var targetCount = 0;
                        foreach (var target in entity.Targets)
                        {
                            var targetEntity = db.GetEntryFromId(target.Item2);
                            if (targetEntity != null)
                                candidateEntities.Add(targetEntity);

                            if (targetCount > 1000)
                                break;
                            targetCount += 1;
                        }
                    }
                }

                FreebaseEntry retrievedCandidate = null;
                var isCandidateRetrieved = false;
                foreach (var candidate in candidateEntities)
                {
                    isCandidateRetrieved |= candidate.Aliases.Contains(label);
                    isCandidateRetrieved |= candidate.Label == label;

                    if (isCandidateRetrieved)
                    {
                        retrievedCandidate = candidate;
                        break;
                    }
                }

                if (isCandidateRetrieved)
                {
                    retrievedCount += 1;
                    Console.WriteLine("\t candidate found: {0}", retrievedCandidate);
                }

                Console.WriteLine("\t{0}/{1}", retrievedCount, totalCount);
            }
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
