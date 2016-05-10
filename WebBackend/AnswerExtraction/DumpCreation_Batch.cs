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
            var dumpProcessor = new WikidataDumpProcessor(@"C:\REPOSITORIES\Wikidata-Toolkit\wdtk-examples\dumpfiles\20160510.json.gz");
            var questionDataset = new QuestionDialogDatasetReader("question_dialogs-train.json");
            var devDataset = new QuestionDialogDatasetReader("question_dialogs-dev.json");

            foreach (var dialog in questionDataset.Dialogs)
            {
                dumpProcessor.AddTargetMid(dialog.AnswerMid);
            }

            dumpProcessor.WriteDump();
        }
    }
}
