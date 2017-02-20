using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;

using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.DataCollection;

using WebBackend.Dataset;
using WebBackend.Experiment;

namespace WebBackend
{
    class Program
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
        /// Currently available experiments.
        /// </summary>
        public static ExperimentCollection Experiments { get; private set; }

        /// <summary>
        /// Provider that is used for question providing.
        /// </summary>
        public static QuestionDialogProvider QuestionDialogProvider;

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            if (!parseArguments(args))
                //arguments were incorrect
                return;

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            //AnswerExtraction.ExtractionEvaluation_Batch.RunLinkingExperiment();
            //AnswerExtraction.ExtractionEvaluation_Batch.ExportAnswerExtractionData();
            //AnswerExtraction.ExtractionEvaluation_Batch.RunLinkedAnswerExtractionExperiment();
            //AnswerExtraction.LuceneIndex_Batch.BuildIndex();
            //AnswerExtraction.DumpCreation_Batch.BenchmarkFreebaseProviderNodes();
            //AnswerExtraction.DumpCreation_Batch.BuildFreebaseDB();
            //AnswerExtraction.DumpCreation_Batch.BenchmarkMySQLEdges(); 
            //AnswerExtraction.DumpCreation_Batch.FillMySQLEdges();
            //AnswerExtraction.DumpCreation_Batch.DumpQuestions();
            //GeneralizationQA.GoldenAnswer_Batch.RunAnswerGeneralizationDev();
            //GeneralizationQA.GoldenAnswer_Batch.RunGraphMIExperiment();
            //return;

            var db = Configuration.Db;
            var linker = Configuration.GetCachedLinker(db, "answer_extraction");
            var extractor = new AnswerExtraction.LinkBasedExtractor(linker, db);

            var simpleQuestions1 = loadSimpleQuestions("questions1.smpq");
            var simpleQuestionsTrain = loadSimpleQuestions("questions_train.smpq");
            var extensionQuestions = loadExtensionQuestions("questions_train.smpq");

            Experiments = new ExperimentCollection(ExperimentsRootPath,

                //question collection experiment 
                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection", 15, simpleQuestions1),

                //question collection experiment 
                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection2", 50, simpleQuestionsTrain),


                //full operation question collection experiment 
                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_1", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_2", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_3", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_4", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_5", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_6", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_7", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_8", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_9", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_10", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(ExperimentsRootPath, "qdd_extension_r_1", 100, simpleQuestionsTrain),
                new QuestionCollectionExperiment(ExperimentsRootPath, "qdd_extension_r_2", 100, simpleQuestionsTrain),
                new QuestionCollectionExperiment(ExperimentsRootPath, "qdd_extension_r_3", 100, extensionQuestions),
                new QuestionCollectionExperiment(ExperimentsRootPath, "qdd_extension_r_4", 100, extensionQuestions),
                new QuestionCollectionExperiment(ExperimentsRootPath, "qdd_extension_r_5", 100, extensionQuestions),
                new AnswerExtractionExperiment(ExperimentsRootPath, "answer_extraction", 100, simpleQuestionsTrain, extractor)
                );

            QuestionDialogProvider = new QuestionDialogProvider(Experiments, simpleQuestionsTrain, "qdd_extension_r_");

            //writeQuestionDataset();

            //run server
            runServer(RootPath);
            runConsole();
        }

        /// <summary>
        /// Parse arguments from commandline.
        /// </summary>
        /// <param name="args">Commandline arguments.</param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        private static bool parseArguments(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Expects path to root folder of web");
                return false;
            }

            RootPath = args[0];
            if (!Directory.Exists(RootPath))
                throw new NotSupportedException("Given path doesn't exists. " + Path.GetFullPath(RootPath));


            return true;
        }

        /// <summary>
        /// Loads <see cref="QuestionCollection"/> from given question file.
        /// </summary>
        /// <param name="questionFile">The file with questions.</param>
        /// <returns>The created collection.</returns>
        private static QuestionCollection loadSimpleQuestions(string questionFile)
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

        private static QuestionCollection loadExtensionQuestions(string questionFile)
        {
            var allQuestions = loadSimpleQuestions(questionFile);

            var answerIds = new List<string>();
            var questions = new List<string>();

            fillWithHintedQuestions(Configuration.GetQuestionDialogsTrain(), questions);
            fillWithHintedQuestions(Configuration.GetQuestionDialogsDev(), questions);
            fillWithHintedQuestions(Configuration.GetQuestionDialogsTest(), questions);

            foreach (var question in questions)
            {
                answerIds.Add(allQuestions.GetAnswerId(question));
            }

            return new QuestionCollection(questions, answerIds);
        }

        private static void fillWithHintedQuestions(QuestionDialogDatasetReader qdd, List<string> questions)
        {
            foreach (var dialog in qdd.Dialogs)
            {
                if (dialog.HasCorrectAnswer)
                    questions.Add(dialog.Question);
            }
        }

        #region Server utilities

        /// <summary>
        /// Run server providing web on given path.
        /// </summary>
        /// <param name="wwwPath">Root path of provided web.</param>
        private static void runServer(string wwwPath)
        {
            var webApp = new DialogWeb(wwwPath);

            ServerEnvironment.AddApplication(webApp);
            var server = ServerEnvironment.Start(4000);
        }

        /// <summary>
        /// Run console that allows to control the server.
        /// </summary>
        private static void runConsole()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { OnExit(); };
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();

                switch (keyInfo.KeyChar)
                {
                    case 't':
                        Log.TraceDisabled = !Log.TraceDisabled;
                        break;
                    case 'n':
                        Log.NoticeDisabled = !Log.NoticeDisabled;
                        break;
                }

            } while (keyInfo.Key != ConsoleKey.Escape);

            Environment.Exit(0);
        }

        /// <summary>
        /// Handler that is called on server exit.
        /// </summary>
        private static void OnExit()
        {
        }

        #endregion
    }
}
