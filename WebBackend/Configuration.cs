using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Dataset;
using WebBackend.AnswerExtraction;

namespace WebBackend
{
    static class Configuration
    {
        internal static readonly string SimpleQuestionFB2M_Path = @"C:/DATABASES/SimpleQuestions_v2/SimpleQuestions_v2/freebase-subsets/freebase-FB2M.txt";

        internal static readonly string FreebaseDB_Path = @"C:/DATABASES/Freebase/freebase.db";

        internal static readonly string WholeFreebase_Path = @"C:/DATABASES/Freebase/freebase.zip";

        internal static readonly string QuestionDialogsTrain_Path = @"./question_dialogs-train.json";

        internal static readonly string QuestionDialogsDev_Path = @"./question_dialogs-dev.json";

        internal static readonly string QuestionDialogsTest_Path = @"./question_dialogs-test.json";

        private static FreebaseDbProvider _db = null;

        private static ILinker _linker = null;

        private static LinkBasedExtractor _extractor = null;

        internal static FreebaseDbProvider Db
        {
            get
            {
                if (_db == null)
                    _db = new FreebaseDbProvider(FreebaseDB_Path);

                return _db;
            }
        }

        internal static ILinker Linker
        {
            get
            {
                if (_linker == null)
                    _linker = Configuration.CreateCachedLinker(Db, "linker");

                return _linker;
            }
        }

        internal static LinkBasedExtractor AnswerExtractor
        {
            get
            {
                if (_extractor == null)
                    _extractor = new AnswerExtraction.LinkBasedExtractor(Linker, Db);

                return _extractor;
            }
        }

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

        internal static DiskCachedLinker CreateCachedLinker(FreebaseDbProvider db, string storage)
        {
            var coreLinker = new GraphDisambiguatedLinker(db, "./verbs.lex", useGraphDisambiguation: true);
            var linker = new DiskCachedLinker("../" + storage + ".link", 1, (u, c) => coreLinker.LinkUtterance(u, c));
            linker.CacheResult = true;
            return linker;
        }
    }
}
