using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace DialogTesting.Utilities
{
    class Graphs
    {
        #region Graph constants

        public static readonly string CharNode = "char";

        public static readonly string UpperCaseNode = "uppercase";

        public static readonly string LowerCaseNode = "lowercase";

        public static readonly string IsRelation = ComposedGraph.IsRelation;

        #endregion


        public static readonly ComposedGraph Alphabet = Graph
            .ReverseMultiEdge(CharNode, IsRelation, "A", "B", "C", "D", "a", "b", "c", "d")
            .ReverseMultiEdge(UpperCaseNode, IsRelation, "A", "B", "C", "D")
            .ReverseMultiEdge(LowerCaseNode, IsRelation, "a", "b", "c", "d")
            ;

        public static readonly ComposedGraph Names = Graph
            .ReverseMultiEdge("Human", "is_man", "Pavel", "Pepa", "Ondra", "David")
            .ReverseMultiEdge("Human", "is_woman", "Zuzana", "Jitka", "Nikola", "Tereza")
            ;

        public static readonly ComposedGraph Alphabet2 = Graph
            .E("A", "alias", "A_p")
            .E("B", "alias", "B_p")
            .E("C", "alias", "C_p")
            .E("D", "alias", "D_p")
            .ReverseMultiEdge(CharNode, IsRelation, "A", "B", "C", "D", "a", "b", "c", "d")
            ;

        #region Graph building utilities

        private static Graphs Graph { get { return new Graphs(); } }

        private readonly ExplicitLayer _layer = new ExplicitLayer();

        private Graphs()
        {
        }

        private Graphs E(string node1, string edgeName, string node2)
        {
            _layer.AddEdge(
                GraphLayerBase.CreateReference(node1),
                edgeName,
                GraphLayerBase.CreateReference(node2)
                );
            return this;
        }

        private Graphs ReverseMultiEdge(string node2, string edgeName, params string[] nodes1)
        {
            foreach (var node1 in nodes1)
            {
                E(node1, edgeName, node2);
            }

            return this;
        }


        private ComposedGraph build()
        {
            return new ComposedGraph(new[] { _layer });
        }

        public static implicit operator ComposedGraph(Graphs builder)
        {
            return builder.build();
        }

        #endregion
    }
}
