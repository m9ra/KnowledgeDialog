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
            var wikidataWriter = new WikidataDumpProcessor(@"C:\REPOSITORIES\Wikidata-Toolkit\wdtk-examples\dumpfiles\20160510.json.gz");
          
            var freebaseProcessor = new FreebaseDumpProcessor(@"C:\Databases\SimpleQuestions_v2\SimpleQuestions_v2\freebase-subsets\freebase-FB2M.txt");
            freebaseProcessor.RunIteration();

            wikidataWriter.AddTargetMids(freebaseProcessor.AllIds);
            wikidataWriter.WriteDump(@"C:\REPOSITORIES\Wikidata-Toolkit\wdtk-examples\dumpfiles\20160510.freebase.gz");
        }
    }
}
