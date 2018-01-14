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


            InitializeExperiments();

            //AnswerExtraction.Omegle_Batch.ObserveDialogCollection();
            //AnswerExtraction.GraphNavigationExperiments_Batch.PrintEdgeVotesInfo();
            //AnswerExtraction.GraphNavigationExperiments_Batch.EvaluateLabelRequestInfo();
            //AnswerExtraction.ExtractionEvaluation_Batch.RunLinkingExperiment();
            //AnswerExtraction.ExtractionEvaluation_Batch.ExportAnswerExtractionData();
            //AnswerExtraction.Statistics_Batch.CountReferences();
            //AnswerExtraction.ExtractionEvaluation_Batch.RunLinkedAnswerExtractionExperiment();
            //AnswerExtraction.SigdialPaperExperiments_Batch.EdgeMaximizationLinking();
            //AnswerExtraction.SigdialPaperExperiments_Batch.PopularityMaximizationLinking();
            //AnswerExtraction.SigdialPaperExperiments_Batch.BasicCancelation();
            //AnswerExtraction.SigdialPaperExperiments_Batch.DatasetStatistics();
            //AnswerExtraction.GraphNavigationExperiments_Batch.ListUnknownEntityWordsQDD();
            //AnswerExtraction.SigdialPaperExperiments_Batch.BasicCancelation_WithEnumDetection();
            //AnswerExtraction.SigdialPaperExperiments_Batch.BasicCancelation_WithEnumAndNgrams();
            //AnswerExtraction.LuceneIndex_Batch.BuildIndex();
            //AnswerExtraction.DumpCreation_Batch.BenchmarkFreebaseProviderNodes();
            //AnswerExtraction.DumpCreation_Batch.BuildFreebaseDB();
            //AnswerExtraction.DumpCreation_Batch.BenchmarkMySQLEdges(); 
            //AnswerExtraction.DumpCreation_Batch.FillMySQLEdges();
            //AnswerExtraction.DumpCreation_Batch.DumpQuestions();
            //GeneralizationQA.GoldenAnswer_Batch.RunAnswerGeneralizationDev();
            //GeneralizationQA.GoldenAnswer_Batch.RunGraphMIExperiment();
            RunWebInterface();
            //Console.ReadLine();
        }

        private static void InitializeExperiments()
        {
            var simpleQuestions1 = Configuration.LoadSimpleQuestions("questions1.smpq");
            var simpleQuestionsTrain = Configuration.SimpleQuestionsTrain;
            var extensionQuestions = loadExtensionQuestions(Configuration.SimpleQuestionsTrain_Path);

            var experimentsRootPath = Configuration.ExperimentsRootPath;

            Experiments = new ExperimentCollection(experimentsRootPath,

           /*     //question collection experiment 
                new QuestionCollectionExperiment(experimentsRootPath, "question_collection", 15, simpleQuestions1),

                //question collection experiment 
                new QuestionCollectionExperiment(experimentsRootPath, "question_collection2", 50, simpleQuestionsTrain),


                //full operation question collection experiment 
                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_1", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_2", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_3", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_4", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_5", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_6", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_7", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_8", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_9", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "question_collection_r_10", 100, simpleQuestionsTrain),

                new QuestionCollectionExperiment(experimentsRootPath, "qdd_extension_r_1", 100, simpleQuestionsTrain),
                new QuestionCollectionExperiment(experimentsRootPath, "qdd_extension_r_2", 100, simpleQuestionsTrain),
                new QuestionCollectionExperiment(experimentsRootPath, "qdd_extension_r_3", 100, extensionQuestions),
                new QuestionCollectionExperiment(experimentsRootPath, "qdd_extension_r_4", 100, extensionQuestions),
                new QuestionCollectionExperiment(experimentsRootPath, "qdd_extension_r_5", 100, extensionQuestions),
                new AnswerExtractionExperiment(experimentsRootPath, "answer_extraction", 100, simpleQuestionsTrain, Configuration.AnswerExtractor),
                new GraphNavigationExperiment(experimentsRootPath, "graph_navigation", 100, Configuration.GetQuestionDialogsTrain()),
                new GraphNavigationExperiment(experimentsRootPath, "edge_requests", 100, Configuration.GetQuestionDialogsTrain()),*/
                new PhraseRestaurantExperiment(experimentsRootPath, "phrase_restaurant", 100)
                );

            QuestionDialogProvider = new QuestionDialogProvider(Experiments, simpleQuestionsTrain, "qdd_extension_r_");
        }

        private static void RunWebInterface()
        {
            //run server
            runServer(Configuration.RootPath);
            runConsole();
        }

        /// <summary>
        /// Parse arguments from commandline.
        /// </summary>
        /// <param name="args">Commandline arguments.</param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        private static bool parseArguments(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Expects path to root folder of web and path to configuration file.");
                return false;
            }

            var rootPath = args[0];
            var configPath = args[1];
            if (!Directory.Exists(rootPath))
                throw new NotSupportedException("Given directory path doesn't exists. " + Path.GetFullPath(rootPath));

            if (!File.Exists(configPath))
                throw new NotSupportedException("Given file path doesn't exists. " + Path.GetFullPath(configPath));

            Configuration.LoadConfig(rootPath, configPath);
            return true;
        }

        private static QuestionCollection loadExtensionQuestions(string questionFile)
        {
            var allQuestions = Configuration.LoadSimpleQuestions(questionFile);

            var answerIds = new List<string>();
            var questions = new List<string>();

            fillWithHintedQuestions(Configuration.GetQuestionDialogsTrain(), questions);
            fillWithHintedQuestions(Configuration.GetQuestionDialogsDev(), questions);
            fillWithHintedQuestions(Configuration.GetQuestionDialogsTest(), questions);

            foreach (var question in questions)
            {
                answerIds.Add(allQuestions.GetAnswerMid(question));
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
