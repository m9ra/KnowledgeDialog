using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Runtime.InteropServices;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation;

using KnowledgeDialog.RuleQuestions;


namespace KnowledgeDialog
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        static void MainParsing(string[] args)
        {
            var utterance = "yes, Obama is president of which state?";
            var parsedUtterance = UtteranceParser.Parse(utterance);

            var factory = new SLUFactory();
            var bestAct = factory.GetBestDialogAct(parsedUtterance);
            var acts = factory.GetDialogActs(parsedUtterance);
            foreach (var act in acts)
            {
                Console.WriteLine(act);
            }
        }

        static void Main(string[] args)
        {
            System.Console.SetBufferSize(240, 10000);   // make sure buffer is bigger than window
            System.Console.SetWindowSize(240, 54);   //set window size to almost full screen

            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3


            var parse = UtteranceParser.Parse("name of wife of Barack Obama president is Michelle Obama");
            //MultipleAdvice(args[0]);
            //ExplicitStateDialog(args[0]);
            //ProbabilisticMappingQATest(args[0]);
            RuleQuestionTest(args[0]);
        }

        private static void RuleQuestionTest(string dbPath)
        {
            var loader = loadDB(dbPath);
            var graph = new ComposedGraph(loader.DataLayer);
            var generator = new StructuredInterpretationGenerator(graph);

            var denotation1 = graph.GetNode("Barack Obama");
            var q1 = "Who is United States of America president?";

            var denotation2 = graph.GetNode("Vladimir Putin");
            var q2 = "Who is Russia president?";

            var q3 = "Who is Czech republic president?";
            var denotation3 = graph.GetNode("Miloš Zeman");

            generator.AdviceAnswer(q1, denotation1);
            generator.AdviceAnswer(q2, denotation2);
            generator.Optimize(5000);

            var interpretations = new List<Ranked<StructuredInterpretation>>();
            foreach (var evidence in generator.GetEvidences(q3))
            {
                foreach (var interpretation in evidence.AvailableRankedInterpretations)
                {
                    interpretations.Add(interpretation);
                }
            }

            interpretations.Sort((a, b) => a.Rank.CompareTo(b.Rank));
            foreach (var interpretation in interpretations)
            {
                var answer = generator.Evaluate(q3, interpretation.Value);
                ConsoleServices.Print(interpretation);
                ConsoleServices.Print(answer);
                ConsoleServices.PrintEmptyLine();
            }

            var qGenerator = new QuestionGenerator(generator);
            //var questions = Generator.FindDistinguishingNodeQuestions();
            throw new NotImplementedException();
        }

        private static void ProbabilisticMappingQATest(string dbPath)
        {
            var loader = loadDB(dbPath);
            var graph = new ComposedGraph(loader.DataLayer);
            var qa = new PoolComputation.ProbabilisticQA.ProbabilisticQAModule(graph, new CallStorage(null));


            var denotation1 = graph.GetNode("Barack Obama");
            var q1 = "Who is United States of America president?";

            var denotation2 = graph.GetNode("Vladimir Putin");
            var q2 = "Who is Russia president?";

            var q3 = "Who is Czech republic president?";
            var denotation3 = graph.GetNode("Miloš Zeman");

            qa.AdviceAnswer(q1, false, denotation1);
            qa.AdviceAnswer(q2, false, denotation2);
            qa.Optimize(100);


            var pool = new ContextPool(graph);
            //   var a1 = qa.GetAnswer(q1, pool).ToArray();
            var a3 = qa.GetAnswer(q3, pool).ToArray();
            var repl_a1 = qa.GetAnswer(q1, pool).ToArray();
            throw new NotImplementedException();
        }

        private static void MappingQATest(string dbPath)
        {
            var loader = loadDB(dbPath);
            var graph = new ComposedGraph(loader.DataLayer);
            var qa = new PoolComputation.MappedQA.MappedQAModule(graph, new CallStorage(null));

            qa.AdviceAnswer("Who is United States of America president?", false, graph.GetNode("Barack Obama"));
            qa.Optimize();
        }

        private static void ExplicitStateDialog(string dbPath)
        {
            var loader = loadDB(dbPath);
            var qa = new PoolComputation.HeuristicQAModule(new ComposedGraph(loader.DataLayer), new CallStorage(null));
            var manager = new PoolComputation.ExplicitStateDialogManager(qa);
            var provider = new DialogConsole(manager);

            provider.SimulateInput(
                "François Hollande is president in which state ?",
                "It is France",
                "Barack Obama is president of which state ?",
                "yes",
                "dont know"
            );

            provider.Run();
        }


        private static void MultipleAdvice(string dbPath)
        {
            var loader = loadDB(dbPath);

            var manager = new PoolComputation.StateDialogManager(null, loader.DataLayer);
            var provider = new DialogConsole(manager);

            provider.SimulateInput(
                "François Hollande is president in which state ?",
                "France",
                "France",
                "François Hollande is president in which state ?"
                );

            provider.Run();
        }

        private static Database.TripletLoader.Loader loadDB(string dbPath)
        {
            var loader = new Database.TripletLoader.Loader(dbPath);
            var graph = new ComposedGraph(loader.DataLayer);
            WikidataHelper.PreprocessData(loader, graph);
            return loader;
        }

        private static void InconsistencyDBTesting(string dbPath)
        {
            var loader = new Database.TripletLoader.Loader(dbPath);
            var graph = new ComposedGraph(loader.DataLayer);
            WikidataHelper.PreprocessData(loader, graph);

            var manager = new PoolComputation.StateDialogManager(null, loader.DataLayer);
            var provider = new DialogConsole(manager);

            provider.SimulateInput(
                "name of Czech Republic president",
                "it is Miloš Zeman",
                "name of Russia president",
                "it is Vladimir Putin",
                "name of United States of America president",
                "it is Barack Obama",
                "name of United States of America president",
                "name of Czech Republic president"
                );

            provider.Run();
        }

        private static void PersistentInformationConsole()
        {
            var manager = new PoolComputation.StateDialogManager("test.json", new FlatPresidentLayer());
            var provider = new DialogConsole(manager);

            provider.Run();
        }

        private static void EquivalenceLearning()
        {
            var manager = new PoolComputation.StateDialogManager(null, new FlatPresidentLayer());
            var provider = new DialogConsole(manager);

            /*  provider.SimulateInput(
              "president of USA?",
              "it is Barack Obama",
              "wife of president of USA?",
              "no",
              "no",
              "it is Michelle Obama"
              );*/

            provider.SimulateInput(
          "president of USA?",
          "it is Barack Obama",
          "wife of president of USA?",
          "no",
          "no",
          "it is Michelle Obama",
          "wife of president of SK?"
          );

            provider.Run();
        }

        private static void MultipleNodesDistinguishing()
        {
            var manager = new PoolComputation.StateDialogManager(null, new FlatPresidentLayer());
            var provider = new DialogConsole(manager);

            provider.SimulateInput(
            "president of USA?",
            "it is Barack Obama",
            "president of USA?",
            "president of CZ?",
            "it is Miloš Zeman"
            );

            provider.Run();
        }

        private static void ExternalDB()
        {
            var manager = new PoolComputation.StateDialogManager(null, new FlatPresidentLayer());
            var provider = new DialogConsole(manager);

            provider.SimulateInput(
            "president of USA?",
            "it is blabla",
            "president of USA?",
            "it is Barack Obama",
            "president of USA?",
            "president of CZ?",
            "his wife?",
            "yes",
            "it is Ivana Zemanová",
            "president of USA?",
            "his wife?"
            );

            provider.Run();
        }

        private static void StateBasedManager()
        {
            var manager = new PoolComputation.StateDialogManager(null, new Database.PresidentLayer());
            var provider = new DialogConsole(manager);

            provider.SimulateInput(
            "president of USA?",
            "it is Barack_Obama",
            "president of CZ?",
            "name of his wife?",
            "yes",
            "it is Ivana_Zemanová",
            "president of D?",
            "his wife?"
            );

            provider.Run();
        }

        private static void KnowledgeClassifier()
        {
            var dataLayer = new PresidentLayer();
            var graph = new ComposedGraph(dataLayer);

            var node1 = graph.GetNode("Barack_Obama");
            var node2 = graph.GetNode("Miloš_Zeman");
            var node3 = graph.GetNode("Michelle_Obama");

            var log = new MultiTraceLog(new[] { node1, node2, node3 }, graph);

            var classifier = new KnowledgeClassifier<string>(graph);
            classifier.Advice(node1, "president");
            classifier.Advice(node2, "president");
            classifier.Advice(node3, "wife");

            var node4 = graph.GetNode("Ivana_Zemanová");
            var node5 = graph.GetNode("Andrej_Kiska");

            var test1 = classifier.Classify(node4);
            var test2 = classifier.Classify(node5);
        }


        private static void debugDialog1(DialogConsole provider)
        {
            provider.SimulateInput(
                "how many children Barack_Obama has ?",
                "how many children Barack_Obama has is 2",
                "how many children Barack_Obama has ?",
                "no",
                "how many children Barack_Obama has ?",
                "how many children Miloš_Zeman has ?"
                );
        }

        private static void demoDialog1(DialogConsole provider)
        {
            provider.SimulateInput(
                "president of USA?",
                "president of CZ?",
                "president of USA is Barack_Obama",
                "president of USA?",
                "president of CZ?",
                "wife of USA president?",
                "wife of USA president is Michelle_Obama",
                "wife of CZ president?",
                "no",
                "wife of CZ president?"
                );
        }

        private static void contextDialog(DialogConsole provider)
        {
            provider.SimulateInput(
                "president of USA is Barack_Obama",
                "president of USA?",
                "last is Barack_Obama",
                "president of CZ?",
                "last?"
                );
        }
    }
}
