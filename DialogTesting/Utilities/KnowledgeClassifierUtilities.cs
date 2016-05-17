using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using U = Microsoft.VisualStudio.TestTools.UnitTesting;

using KnowledgeDialog.Knowledge;

namespace DialogTesting.Utilities
{
    static class KnowledgeClassifierUtilities
    {
        public static KnowledgeClassifier<string> Advice(this ComposedGraph graph, string nodeData, string cls)
        {
            return Advice(new KnowledgeClassifier<string>(graph), nodeData, cls);
        }

        public static KnowledgeClassifier<string> Advice(this KnowledgeClassifier<string> classifier, string nodeData, string cls)
        {
            var node = ExplicitLayer.CreateReference(nodeData);
            classifier.Advice(node, cls);
            return classifier;
        }

        public static KnowledgeClassifier<string> Assert(this KnowledgeClassifier<string> classifier, string expectedClass, params string[] nodesData)
        {
            foreach (var nodeData in nodesData)
            {
                var node = classifier.Knowledge.GetNode(nodeData);
                var actualClass = classifier.Classify(node);

                U.Assert.AreEqual(expectedClass, actualClass, "Incorrect classification for '" + nodeData + "'");
            }
            return classifier;
        }
    }
}
