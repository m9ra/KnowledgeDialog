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
            var provider = new DialogConsole(manager);

            provider.SimulateInput(
             "president of USA?",
             "it is Barack_Obama",
             "president of CZ?",
             "president",
             "of USA"
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
