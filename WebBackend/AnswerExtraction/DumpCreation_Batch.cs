using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    /// <summary>
    /// Batch script for freebase dump creation.
    /// </summary>
    class DumpCreation_Batch
    {
        internal static void DumpQuestions()
        {
            var dumpProcessor = new FreebaseDumpProcessor(@"C:\Databases\SimpleQuestions_v2\SimpleQuestions_v2\freebase-subsets\freebase-FB2M.txt");
            var questionDataset = new QuestionDialogDatasetReader("question_dialogs-train.json");
            var devDataset = new QuestionDialogDatasetReader("question_dialogs-dev.json");

            foreach (var dialog in questionDataset.Dialogs)
            {
                dumpProcessor.AddTargetMid(dialog.AnswerMid);
            }

          //  dumpProcessor.RunAbstractIteration();
            dumpProcessor.RefillTargets();
          //  dumpProcessor.RunConcreteIteration();
            dumpProcessor.RunIteration();
            dumpProcessor.RefillTargets();
            var contained = 0;
            foreach (var dialog in devDataset.Dialogs)
            {
                if (dumpProcessor.AllIds.Contains(dialog.AnswerMid.Substring(FreebaseLoader.IdPrefix.Length)))
                    contained += 1;
            }

            Console.WriteLine("{0:0.00}", 100.0 * contained / devDataset.Dialogs.Count());
            //  dumpProcessor.RunIteration();
            //  dumpProcessor.WriteCoverDump("cover_dump.trp");
        }
    }
}
