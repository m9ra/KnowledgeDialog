using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Dataset;

namespace WebBackend
{
    static class Configuration
    {
        internal static readonly string LuceneIndex_Path = @"C:/REPOSITORIES/lucene_freebase_v3_index";

        internal static readonly string SimpleQuestionFB2M_Path = @"C:/Databases/SimpleQuestions_v2/SimpleQuestions_v2/freebase-subsets/freebase-FB2M.txt";

        internal static readonly string FreebaseDump_Path = @"C:/REPOSITORIES/Wikidata-Toolkit/wdtk-examples/dumpfiles/20160510.freebase.v3.gz";

        internal static readonly string FreebaseDB_Path = @"C:/REPOSITORIES/freebase.db";

        internal static readonly string WholeFreebase_Path = @"C:/REPOSITORIES/freebase.zip";

        internal static readonly string QuestionDialogsTrain_Path = @"./question_dialogs-train.json";

        internal static readonly string QuestionDialogsDev_Path = @"./question_dialogs-dev.json";

        internal static readonly string QuestionDialogsTest_Path = @"./question_dialogs-test.json";

        internal static QuestionDialogDatasetReader GetQuestionDialogsTrain()
        {
            return new QuestionDialogDatasetReader(QuestionDialogsTrain_Path);
        }

        internal static QuestionDialogDatasetReader GetQuestionDialogsDev()
        {
            return new QuestionDialogDatasetReader(QuestionDialogsDev_Path);
        }

        internal static QuestionDialogDatasetReader GetQuestionDialogsTest()
        {
            return new QuestionDialogDatasetReader(QuestionDialogsTest_Path);
        }

        internal static SimpleQuestionDumpProcessor GetSimpleQuestionsDump()
        {
            return new SimpleQuestionDumpProcessor(SimpleQuestionFB2M_Path);
        }

        internal static FreebaseDbProvider GetFreebaseDbProvider()
        {
            return new FreebaseDbProvider(LuceneIndex_Path);
        }
    }
}
