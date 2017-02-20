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
        internal static void BuildFreebaseDB()
        {
            var freebaseWriter = new FreebaseDumpProcessor(Configuration.WholeFreebase_Path);

            var dump = Configuration.GetSimpleQuestionsDump();
            dump.LoadInOutBounds();

            Console.WriteLine("Adding target {0} Ids.", dump.AllIds.Count);
            freebaseWriter.AddTargetMids(dump.AllIds);
            GC.Collect();
            freebaseWriter.WriteDB(Configuration.FreebaseDB_Path, id => dump.GetInTargets(id), id => dump.GetOutTargets(id));
        }

        internal static void BenchmarkFreebaseProviderNodes()
        {
            var db = new FreebaseDbProvider(Configuration.FreebaseDB_Path);
            var test = db.GetEntryFromId("02rwv9s");

            var dump = Configuration.GetSimpleQuestionsDump();
            dump.RunIteration(100000);
            Console.WriteLine("Dump prepared.");
            var ids = dump.AllIds.ToArray();
            Console.WriteLine("Id count " + ids.Length);

            var idRepetitionCount = 1000;

            var rnd = new Random();
            var output = 0;
            for (var sample = 0; sample < 1000; ++sample)
            {
                var start = DateTime.Now;
                for (var i = 0; i < idRepetitionCount; ++i)
                {
                    var rndIndex = rnd.Next(ids.Length);
                    var id = ids[rndIndex];

                    var info = db.GetEntryFromId(id);
                    if (info == null)
                        continue;

                    output += info.Label.Length;
                }
                var duration = DateTime.Now - start;
                Console.WriteLine("Time for entity: {0:0.000}ms", duration.TotalMilliseconds / idRepetitionCount);
            }

            Console.WriteLine(output);
        }
    }
}
