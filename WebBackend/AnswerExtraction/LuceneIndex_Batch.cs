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
            var questionDataset = new QuestionDialogDatasetReader("question_dialogs-train.json");
            var devDataset = new QuestionDialogDatasetReader("question_dialogs-dev.json");

            var dumpLoader = new DumpLoader(@"C:\REPOSITORIES\Wikidata-Toolkit\wdtk-examples\dumpfiles\20160510.freebase.v2.gz");

            var extractor = new AnswerExtraction.Extractor(@"C:\REPOSITORIES\lucene_freebase_v1_index");
            foreach (var id in dumpLoader.Ids)
            {
                var names= dumpLoader.GetNames(id);
                var description = dumpLoader.GetDescription(id);
                extractor.AddEntry(FreebaseLoader.IdPrefix + id, names, description);
            }

            extractor.RebuildFreebaseIndex();

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
