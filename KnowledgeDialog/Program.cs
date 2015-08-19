using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Database;

namespace KnowledgeDialog
{
    class Program
    {
        static void Main(string[] args)
        {
            var parse = UtteranceParser.Parse("name of wife of Barack Obama president is Michelle Obama");
            //MultipleAdvice(args[0]);
            //ExplicitStateDialog(args[0]);
            MappingQATest(args[0]);
        }

        private static void MappingQATest(string dbPath)
        {
            var loader = loadDB(dbPath);
            var graph = new ComposedGraph(loader.DataLayer);
            var qa = new PoolComputation.MappedQA.MappedQAModule(graph, new CallStorage(null));

            qa.AdviceAnswer("Who is United States of America president?", false, graph.GetNode("Barack Obama"));
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
