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
            var freebaseWriter = new FreebaseDumpProcessor(Configuration.WholeFreebase_Path);

            var freebaseProcessor = Configuration.GetSimpleQuestionsDump();
            freebaseProcessor.RunIteration();

            freebaseWriter.AddTargetMids(freebaseProcessor.AllIds);
            freebaseWriter.WriteDump(Configuration.FreebaseDump_Path);
        }
    }
}
