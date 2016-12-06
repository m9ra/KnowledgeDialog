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

            var dump = Configuration.GetSimpleQuestionsDump();
            dump.RunIteration();

            freebaseWriter.AddTargetMids(dump.AllIds);
            freebaseWriter.WriteDump(Configuration.FreebaseDump_Path);
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
    }
}
