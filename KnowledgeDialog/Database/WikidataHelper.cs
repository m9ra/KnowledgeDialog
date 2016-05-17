using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Database.TripletLoader;

namespace KnowledgeDialog.Database
{
    public static class WikidataHelper
    {
        static readonly string PresidentNode = "Q30461";

        static readonly string NullNode = "null";

        static readonly string HeldPositionEdge = "P39";

        static readonly string TemporaryMainEdge = "main";

        static readonly string EndEdge = "P582";

        public static void PreprocessData(Loader loader, ComposedGraph graph)
        {
            foreach (var node in loader.Nodes)
            {
                var positions = graph.OutcommingTargets(node, HeldPositionEdge).ToArray();
                foreach (var position in positions)
                {
                    if (IsPresidentPosition(position, graph))
                    {
                        //repair form of president position (fill end=null, if there is no end specified)
                        RepairPresidentPosition(node, position, loader.DataLayer, graph);
                    }
                    else
                    {
                        //keep only president positions
                        RemovePosition(node, position, loader.DataLayer);
                    }
                }
            }
        }

        private static bool IsPresidentPosition(NodeReference positionRoot, ComposedGraph graph)
        {
            positionRoot = getPropertyNode(positionRoot, graph);

            if (positionRoot == null)
                return false;

            var president = graph.GetNode(PresidentNode);

            if (president.Equals(positionRoot))
                return true;

            var paths = graph.GetPaths(president, positionRoot, 1, 100).ToArray();
            return paths.Length > 0;
        }

        private static void RemovePosition(NodeReference node, NodeReference positionRoot, ExplicitLayer layer)
        {
            //remove node-->PositionEdge-->positionRoot
            //it does not matter if positionRoot is temporary or not
            layer.RemoveEdge(node, HeldPositionEdge, positionRoot);
        }

        private static void RepairPresidentPosition(NodeReference node, NodeReference positionRoot, ExplicitLayer layer, ComposedGraph graph)
        {
            if(!IsTemporaryNode(positionRoot))
                //only temporary nodes will be repaired
                return;

            var endNode = graph.OutcommingTargets(positionRoot, EndEdge).FirstOrDefault();
            if (endNode != null)
                //nothing to repair
                return;

            layer.AddEdge(positionRoot,EndEdge,graph.GetNode(NullNode));
        }

        private static bool IsTemporaryNode(NodeReference node)
        {
            return node.Data.StartsWith("$");
        }

        private static NodeReference getPropertyNode(NodeReference positionRoot, ComposedGraph graph)
        {
            var isTemporary = IsTemporaryNode(positionRoot);
            if (isTemporary)
                positionRoot = graph.OutcommingTargets(positionRoot, TemporaryMainEdge).FirstOrDefault();

            return positionRoot;
        }

    }
}
