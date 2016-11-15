using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class LuceneIndex_Batch
    {
        internal static void BuildIndex()
        {
            var trainDataset = Configuration.GetQuestionDialogsTrain();
            var devDataset = Configuration.GetQuestionDialogsDev();

            var simpleQuestionsProcessor = Configuration.GetSimpleQuestionsDump();
            simpleQuestionsProcessor.LoadInOutBounds();
            GC.Collect();

            var dumpLoader = new DumpLoader(Configuration.FreebaseDump_Path);
            var extractor = new FreebaseDbProvider(Configuration.LuceneIndex_Path);
            extractor.StartFreebaseIndexRebuild();
            foreach (var id in dumpLoader.Ids)
            {
                var names = dumpLoader.GetNames(id);
                var description = dumpLoader.GetDescription(id);
                var inBounds = simpleQuestionsProcessor.GetInBounds(id);
                var outBounds = simpleQuestionsProcessor.GetOutBounds(id);
                extractor.AddEntry(FreebaseLoader.IdPrefix + id, names, description, inBounds, outBounds);
            }
            extractor.FinishFreebaseIndexRebuild();
            extractor.LoadIndex();

            var totalCount = 0;
            var includedCount = 0;
            foreach (var dialog in devDataset.Dialogs)
            {
                if (!dialog.HasCorrectAnswer)
                    continue;

                var label = dumpLoader.GetLabel(dialog.AnswerMid);
                if (label != null)
                    includedCount += 1;
                else
                    Console.WriteLine(dialog.AnswerMid);

                totalCount += 1;
            }

            Console.WriteLine("Included questions {0:0.00}", 100.0 * includedCount / totalCount);
        }
    }
}
