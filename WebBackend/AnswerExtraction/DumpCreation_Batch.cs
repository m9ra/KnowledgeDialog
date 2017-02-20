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

        internal static void FillMySQLNodes()
        {
            var mysqlConnector = new MysqlFreebaseConnector();
            var freebaseWriter = new FreebaseDumpProcessor(Configuration.WholeFreebase_Path);
            var dump = Configuration.GetSimpleQuestionsDump();
            dump.RunIteration();

            freebaseWriter.AddTargetMids(dump.AllIds);
            freebaseWriter.ExportNodes(mysqlConnector);
        }

        internal static void FillMySQLEdges()
        {
            var mysqlConnector = new MysqlFreebaseConnector();
            var dump = Configuration.GetSimpleQuestionsDump();
            dump.UseInterning = false;
            dump.ExportEdges(mysqlConnector);
        }

        internal static void BenchmarkMySQLEdges()
        {
            var mysqlConnector = new MysqlFreebaseConnector();
            var dump = Configuration.GetSimpleQuestionsDump();
            dump.RunIteration(10000);
            Console.WriteLine("Dump prepared.");
            var ids = dump.AllIds.ToArray();
            Console.WriteLine("Id count " + ids.Length);

            var idRepetitionCount = 10;

            var rnd = new Random();
            var output = 0;
            for (var sample = 0; sample < 1000; ++sample)
            {
                var cardinality = 0;
                var start = DateTime.Now;
                for (var i = 0; i < idRepetitionCount; ++i)
                {
                    var rndIndex = rnd.Next(ids.Length);
                    var id = ids[rndIndex];

                    var targets = mysqlConnector.GetTargets(id);
                    foreach (var target in targets)
                    {
                        output += target.Item2.Length;
                        cardinality += 1;
                    }
                }
                var duration = DateTime.Now - start;
                Console.WriteLine("Time for entity: {0:0.000}ms [{1}]", duration.TotalMilliseconds / idRepetitionCount, cardinality);
            }

            Console.WriteLine(output);
        }

        internal static void BenchmarkMySQLNodes()
        {
            var mysqlConnector = new MysqlFreebaseConnector();
            var dump = Configuration.GetSimpleQuestionsDump();
            dump.RunIteration(10000);
            Console.WriteLine("Dump prepared.");
            var ids = dump.AllIds.ToArray();
            Console.WriteLine("Id count " + ids.Length);

            var idRepetitionCount = 10;

            var rnd = new Random();
            var output = 0;
            for (var sample = 0; sample < 1000; ++sample)
            {
                var start = DateTime.Now;
                for (var i = 0; i < idRepetitionCount; ++i)
                {
                    var rndIndex = rnd.Next(ids.Length);
                    var id = ids[rndIndex];

                    var info = mysqlConnector.GetNodeInfo(id);
                    if (info == null)
                        continue;

                    if (info.Item1 != null)
                        output += info.Item1.Length;
                    if (info.Item2 != null)
                        output += info.Item2.Length;
                }
                var duration = DateTime.Now - start;
                Console.WriteLine("Time for entity: {0:0.000}ms", duration.TotalMilliseconds / idRepetitionCount);
            }

            Console.WriteLine(output);
        }
    }
}
