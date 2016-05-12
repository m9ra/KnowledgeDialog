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
using KnowledgeDialog.Dialog;

using WebBackend.Task;
using WebBackend.Task.President;

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
        /// Determine whether wikidata will be used or not.
        /// </summary>
        public static bool UseWikidata { get; private set; }

        /// <summary>
        /// Knwoledge graph that is currently used.
        /// </summary>
        public static ComposedGraph Graph { get; private set; }

        /// <summary>
        /// Currently available experiments.
        /// </summary>
        public static ExperimentCollection Experiments { get; private set; }

        /// <summary>
        /// Provider that is used for question providing.
        /// </summary>
        public static QuestionDialogProvider QuestionDialogProvider;

        /// <summary>
        /// Loader for freebase entities
        /// </summary>
        public static FreebaseLoader FreebaseLoader;

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            if (!parseArguments(args))
                //arguments were incorrect
                return;

            //AnswerExtraction.DumpCreation_Batch.DumpQuestions();  

            var simpleQuestions1 = loadSimpleQuestions("questions1.smpq");

            var simpleQuestionsTrain = loadSimpleQuestions("questions_train.smpq");

            Experiments = new ExperimentCollection(ExperimentsRootPath,
                //main experiment where only CrowdFlower's people have access
                new CrowdFlowerExperiment(ExperimentsRootPath, "experiment1", 15, new Task.President.PresidentTaskFactory()),

                //have public experiment with same settings
                new CrowdFlowerExperiment(ExperimentsRootPath, "public_experiment", 15, new Task.President.PresidentTaskFactory()),

                //data collection experiment
                new DataCollectionExperiment(ExperimentsRootPath, "data_collection", 15, new Task.President.PresidentCollectionTaskFactory()),

                //data collection experiment2
                new DataCollectionExperiment(ExperimentsRootPath, "data_collection2", 15, new Task.President.PresidentCollectionTaskFactory()),

                //data collection experiment3
                new DataCollectionExperiment(ExperimentsRootPath, "data_collection3", 15, new Task.President.PresidentCollectionTaskFactory()),

                //data collection experiment4
                new DataCollectionExperiment(ExperimentsRootPath, "data_collection4", 15, new Task.President.PresidentCollectionTaskFactory()),

                //data collection experiment5
                new DataCollectionExperiment(ExperimentsRootPath, "data_collection5", 15, new Task.President.PresidentCollectionTaskFactory()),

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

                new QuestionCollectionExperiment(ExperimentsRootPath, "question_collection_r_10", 100, simpleQuestionsTrain)
                );



            QuestionDialogProvider = new QuestionDialogProvider(Experiments, simpleQuestionsTrain, "question_collection_r_");

            FreebaseLoader = new FreebaseLoader(Path.Combine(DataPath, "freebase"));
            QuestionDialogProvider.Refresh();

            //AnswerExtraction.LuceneIndex_Batch.BuildIndex(FreebaseLoader);
            AnswerExtraction.ExtractionEvaluation_Batch.RunEvaluation(FreebaseLoader);
            return;

            writeQuestionDataset();

            var experiment = Experiments.Get("data_collection5");
            writeDataset(experiment);

            //run server
            runServer(RootPath);
            runConsole();
        }

        /// <summary>
        /// Writes dataset from data collected during given experiment.
        /// </summary>
        /// <param name="experiment">Experiment which data will be written.</param>
        private static void writeDataset(ExperimentBase experiment)
        {
            var writer = new Dataset.DatasetWriter(experiment);
            var trainingSplit = SplitDescription.Ratio(0.3)
                .Add<PresidentOfStateTask>()
                .Add<PresidentChildrenTask>();

            var validationSplit = SplitDescription.Ratio(0.2)
                .Add<PresidentOfStateTask>()
                .Add<PresidentChildrenTask>()
                .Add<StateOfPresidentTask>()
                .Add<DaughterOfTask>()
                ;

            writer.WriteData(".", trainingSplit, validationSplit);
        }

        private static void writeQuestionDataset()
        {
            var provider = Program.QuestionDialogProvider;
            var sluFactory = new SLUFactory();
            provider.Refresh();

            var dialogCount = 0;
            var turnCount = 0;

            var explanationCount = 0;
            var answerCount = 0;
            var noAnswerCount = 0;
            var parahpraseCount = 0;

            var correctCount = 0;
            var incorrectCount = 0;
            var differentChoise = 0;

            var invalid = 0;
            var unwanted = 0;

            var dialogIndexes = new List<int>();
            var questions = new HashSet<string>();
            for (var i = 0; i < provider.DialogCount; ++i)
            {
                var dialog = provider.GetDialog(i);
                var annotation = dialog.Annotation;

                switch (annotation)
                {
                    case "invalid":
                        invalid += 1;
                        break;
                    case "unwanted_answer":
                        unwanted += 1;
                        break;
                }

                if (annotation == null || annotation == "invalid" || annotation == "unwanted_answer")
                    //we dont want this dialog
                    continue;

                switch (annotation)
                {
                    case "correct_answer":
                        correctCount += 1;
                        break;

                    case "different_choice":
                        differentChoise += 1;
                        incorrectCount += 1;
                        break;

                    case "no_answer":
                        noAnswerCount += 1;
                        break;

                    default:
                        incorrectCount += 1;
                        break;
                }

                questions.Add(dialog.Question);

                dialogCount += 1;
                turnCount += dialog.TurnCount;

                explanationCount += dialog.GetExplanationCount(sluFactory);
                parahpraseCount += dialog.GetParaphraseCount(sluFactory);
                answerCount += dialog.GetAnswerCount();


                dialogIndexes.Add(i);
            }

            Console.WriteLine("Dataset statistics");
            w("Dialog count", dialogCount);
            w("Turn count", turnCount);
            w("Explanation count", explanationCount);
            w("Answer count", answerCount);
            w("Noanswer count", noAnswerCount);
            w("Paraphrase count", parahpraseCount);
            w("Correct count", correctCount);
            w("Incorrect count", incorrectCount);
            w("Different choice", differentChoise);
            w("Invalid count", invalid);
            w("Unwanted count", unwanted);
            w("Question count", questions.Count);
            Console.WriteLine();


            var rnd = new Random(12345);
            Shuffle(rnd, dialogIndexes);
            var testRatio = 0.35;
            var trainRatio = 0.5;

            var testCount = (int)(dialogIndexes.Count * testRatio);
            var trainCount = (int)(dialogIndexes.Count * trainRatio);
            var devCount = dialogIndexes.Count - testRatio - trainRatio;

            var testIndexes = dialogIndexes.Take(testCount).ToArray();
            var trainIndexes = dialogIndexes.Skip(testCount).Take(trainCount).ToArray();
            var devIndexes = dialogIndexes.Skip(testCount + trainCount).ToArray();

            if (testIndexes.Intersect(trainIndexes).Any() || trainIndexes.Intersect(devIndexes).Any() || devIndexes.Intersect(testIndexes).Any())
                throw new NotSupportedException("there is an error when spliting dataset");

            writeQuestionDatasetTo("question_dialogs-test.json", testIndexes, provider);
            writeQuestionDatasetTo("question_dialogs-train.json", trainIndexes, provider);
            writeQuestionDatasetTo("question_dialogs-dev.json", devIndexes, provider);

            Console.ReadLine();
        }

        private static void w(string caption, int value)
        {
            Console.WriteLine("\t" + caption + ": " + value);
        }

        private static void writeQuestionDatasetTo(string targetFilePath, int[] dialogsToWrite, Dataset.QuestionDialogProvider provider)
        {
            var turnCount = 0;
            var writer = new QuestionDatasetWriter(targetFilePath);
            foreach (var dialogIndex in dialogsToWrite)
            {
                var dialog = provider.GetDialog(dialogIndex);
                turnCount += dialog.TurnCount;
                writer.Add(dialog);
            }

            writer.Save();

            Console.WriteLine("Statistics for file " + targetFilePath);
            w("Dialog count", dialogsToWrite.Length);
            w("Turn count", turnCount);
            Console.WriteLine();
        }


        public static void Shuffle<T>(Random rnd, IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Parse arguments from commandline.
        /// </summary>
        /// <param name="args">Commandline arguments.</param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        private static bool parseArguments(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Expects path to root folder of web and wikidata DB path");
                return false;
            }

            RootPath = args[0];
            if (!Directory.Exists(RootPath))
                throw new NotSupportedException("Given path doesn't exists. " + Path.GetFullPath(RootPath));

            if (args.Length > 1)
            {
                UseWikidata = true;
                var loader = new KnowledgeDialog.Database.TripletLoader.Loader(args[1]);
                Graph = new ComposedGraph(loader.DataLayer);

                WikidataHelper.PreprocessData(loader, Graph);
            }
            else
            {
                Graph = new ComposedGraph(new KnowledgeDialog.Database.FlatPresidentLayer());
            }

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
