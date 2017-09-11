using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using KnowledgeDialog.DataCollection;

using WebBackend.Dataset;
using WebBackend.AnswerExtraction;

namespace WebBackend
{
    static class Configuration
    {
        /// <summary>
        /// Root path of web application.
        /// </summary>
        public static string RootPath { get; private set; }

        /// <summary>
        /// Path with stored data.
        /// </summary>
        public static string DataPath { get { return Path.Combine(RootPath, "data"); } }

        /// <summary>
        /// Path where experiments are stored.
        /// </summary>
        public static string ExperimentsRootPath { get { return Path.Combine(DataPath, "experiments"); } }

        /// <summary>
        /// Path to omegle experiments
        /// </summary>
        public static string OmegleExperimentsRootPath { get { return Path.Combine(ExperimentsRootPath, "omegle2"); } }

        internal static string SimpleQuestionFB2M_Path { get; private set; }

        internal static string FreebaseDB_Path { get; private set; }

        internal static string WholeFreebase_Path { get; private set; }

        internal static string QuestionDialogsTrain_Path { get; private set; }

        internal static string QuestionDialogsDev_Path { get; private set; }

        internal static string QuestionDialogsTest_Path { get; private set; }

        internal static string SimpleQuestionsTrain_Path { get; private set; }

        private static FreebaseDbProvider _db = null;

        private static ILinker _linker = null;

        private static LinkBasedExtractor _extractor = null;

        internal static QuestionCollection _simpleQuestionsTrain;

        internal static QuestionCollection SimpleQuestionsTrain
        {
            get
            {
                if (_simpleQuestionsTrain == null)
                    _simpleQuestionsTrain = LoadSimpleQuestions(SimpleQuestionsTrain_Path);

                return _simpleQuestionsTrain;
            }
        }

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

        internal static void LoadConfig(string rootPath, string configPath)
        {
            if (RootPath != null)
                throw new NotSupportedException("Config can be load only once");

            RootPath = rootPath;

            var config = File.ReadAllLines(configPath).Where(l => l.Trim() != "").Select(l => l.Split(new[] { ':' }, 2)).ToDictionary(p => p[0].Trim(), p => p[1].Trim());

            SimpleQuestionFB2M_Path = config["SimpleQuestionFB2M_Path"];
            FreebaseDB_Path = config["FreebaseDB_Path"];
            WholeFreebase_Path = config["WholeFreebase_Path"];
            QuestionDialogsTrain_Path = config["QuestionDialogsTrain_Path"];
            QuestionDialogsDev_Path = config["QuestionDialogsDev_Path"];
            QuestionDialogsTest_Path = config["QuestionDialogsTest_Path"];
            SimpleQuestionsTrain_Path = config["SimpleQuestionsTrain_Path"];
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
            var linker = new DiskCachedLinker("../" + storage + ".link", 1, (u, c) => coreLinker.LinkUtterance(u, c), db);
            linker.CacheResult = true;
            return linker;
        }

        /// <summary>
        /// Loads <see cref="QuestionCollection"/> from given question file.
        /// </summary>
        /// <param name="questionFile">The file with questions.</param>
        /// <returns>The created collection.</returns>
        internal static QuestionCollection LoadSimpleQuestions(string questionFile)
        {
            var questionFilePath = Path.Combine(DataPath, questionFile);
            var questionLines = File.ReadAllLines(questionFilePath, Encoding.UTF8);

            var questions = new List<string>();
            var answerIds = new List<string>();
            foreach (var line in questionLines)
            {
                var lineParts = line.Split('\t');
                var question = lineParts[3];
                var answerId = lineParts[2];
                questions.Add(question);
                answerIds.Add(answerId);
            }

            return new QuestionCollection(questions, answerIds);
        }
    }
}
