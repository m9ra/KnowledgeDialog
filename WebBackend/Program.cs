using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;

using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

using WebBackend.Experiment;

namespace WebBackend
{
    class Program
    {
        /// <summary>
        /// Root path of web application.
        /// </summary>
        public static string RootPath { get; private set; }

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
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            if (!parseArguments(args))
                //arguments were incorrect
                return;

            Experiments = new ExperimentCollection(ExperimentsRootPath,
                //main experiment where only CrowdFlower's people have access
                new CrowdFlowerExperiment(ExperimentsRootPath, "experiment1", 15, new Task.President.PresidentTaskFactory()),

                //have public experiment with same settings
                new CrowdFlowerExperiment(ExperimentsRootPath, "public_experiment", 15, new Task.President.PresidentTaskFactory()),

                //data collection experiment
                new DataCollectionExperiment(ExperimentsRootPath, "data_collection", 15, new Task.President.PresidentCollectionTaskFactory())
                );

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
