using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PatternComputation;


namespace DialogTesting
{
    [TestClass]
    public class DialogLayerTests
    {
        /// <summary>
        /// Tests that turn nodes are connected to knowledge in the graph.
        /// </summary>
        [TestMethod]
        public void TurnConnectionTest()
        {
            var active = ComposedGraph.Active;
            var obama = "Barack_Obama";
            var usa = "USA";

            checkPathExistence(active, usa);
            checkPathExistence(obama, usa);
            checkPathExistence(active, obama);
        }

        private static void checkPathExistence(string node1, string node2)
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
