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
            var freebaseWriter = new FreebaseDumpProcessor(@"./freebase.zip");
          
           /* var freebaseProcessor = new SimpleQuestionDumpProcessor(@"./freebase-FB2M.txt");
            freebaseProcessor.RunIteration();

            freebaseWriter.AddTargetMids(freebaseProcessor.AllIds);*/
            freebaseWriter.WriteDump(@"./20160510.freebase.v2.gz");
        }
    }
}
