using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Database;
using KnowledgeDialog.PatternComputation;

namespace KnowledgeDialog
{
    class Program
    {
        static void Main(string[] args)
        {
            PoolDialog();
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

        private static void PatternDialog()
        {
            var manager = new PatternComputation.DialogManager(new Database.PresidentLayer());
            var decoratedManager = new PatternComputation.ConsoleDecorator(manager);
            var provider = new DialogConsole(decoratedManager);

            //demoMatching();
            //demoDialog1(provider);
            contextDialog(provider);
            //debugDialog1(provider);

            provider.Run();
        }

        private static void PoolDialog()
        {
            var manager = new PoolComputation.DialogManager(new Database.PresidentLayer());
            var provider = new DialogConsole(new PoolComputation.ConsoleDecorator(manager));

            provider.SimulateInput(
             "president of USA?",
             "it is Barack_Obama",
             "president of USA?",
             "you should say his name i_s Barack_Obama",
             "president of USA?",
             "wife of president in USA?",
             "it is Michelle_Obama",
             "wife of president in CZ?",
             "you should say her name i_s Ivana_Zemanová",
             "wife of president in D?",
             "you should say her name i_s Gerhild_Radtke?",
             "president of SK?",
             "you should say his name i_s Andrej_Kiska",
             "wife of president in SK?",
             "president of CZ?"


             );

            provider.Run();
        }

        private static void demoMatching()
        {
            var dataLayer = new PresidentLayer();
            var dialogLayer = new MultiTurnDialogLayer();
            var graph = new ComposedGraph(dataLayer, dialogLayer);

            var node1 = ComposedGraph.Active;
            var node2 = "USA";

            DialogManager.FillDialogLayer(dialogLayer, "president of USA");

            var paths = new[] { graph.GetPaths(graph.GetNode(node1), graph.GetNode(node2), 100, 100).First() };
            var group = new KnowledgeGroup(paths);

            DialogManager.FillDialogLayer(dialogLayer, "president2 of USA");

            var evaluation = new PatternComputation.PartialMatching.PartialEvaluation(group, graph);
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
