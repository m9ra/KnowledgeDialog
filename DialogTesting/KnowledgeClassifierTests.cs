
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PatternComputation;

using DialogTesting.Utilities;


namespace DialogTesting
{
    [TestClass]
    public class KnowledgeClassifierTests
    {
        /// <summary>
        /// Tests that turn nodes are connected to knowledge in the graph.
        /// </summary>
        [TestMethod]
        public void BasicClassification()
        {
            Graphs.Alphabet
                .Advice("A", "capital letter")
                .Advice("B", "capital letter")
                .Advice("c", "small letter")

                .Assert("capital letter", "A", "B", "C", "D")
                .Assert("small letter", "a", "b", "c", "d");
        }

        private static void edge(string node1,string edgeName, string node2)
        {
            var dataLayer = new PresidentLayer();
            var dialogLayer = new MultiTurnDialogLayer();

            DialogManager.FillDialogLayer(dialogLayer, "president of USA");

            var graph = new ComposedGraph(dataLayer, dialogLayer);
            var paths = graph.GetPaths(graph.GetNode(node1), graph.GetNode(node2), 100, 100).ToArray();

            if (paths.Length == 0)
                throw new InternalTestFailureException("Missing path from " + node1 + " to " + node2);
        }
    }
}
