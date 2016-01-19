using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuestions.FreeBaseReaders
{
    class TripletSelector : FreeBaseReader
    {
        private readonly HashSet<FreeBaseNode> _nodesToSelect = new HashSet<FreeBaseNode>();

        private readonly HashSet<FreeBaseEdge> _edgesToSelect = new HashSet<FreeBaseEdge>();

        private readonly List<FreeBaseTriplet> _selectedTriplets = new List<FreeBaseTriplet>();

        internal int SelectedNodeCount { get { return _nodesToSelect.Count; } }

        internal int SelectedEdgeCount { get { return _edgesToSelect.Count; } }

        internal TripletSelector(string file)
            : base(file)
        {
        }

        /// <inheritdoc/>
        protected override void ProcessEntry(FreeBaseNode source, FreeBaseEdge edge, FreeBaseNode target)
        {
            if (_nodesToSelect.Contains(source) && _nodesToSelect.Contains(target))
            {
                //add new edge between the nodes
                selectTriplet(source, edge, target);
                return;
            }

            if (_nodesToSelect.Contains(source) && _edgesToSelect.Contains(edge))
            {
                //new node is connected with known edge
                selectTriplet(source, edge, target);
                _nodesToSelect.Add(target);
                return;
            }

            if (_edgesToSelect.Contains(edge) && _nodesToSelect.Contains(target))
            {
                //connection from new node with known edge
                selectTriplet(source, edge, target);
                _nodesToSelect.Add(source);
                return;
            }

            //triplet is not selected
        }

        internal void AddNodesToSelect(IEnumerable<FreeBaseNode> nodes)
        {
            _nodesToSelect.UnionWith(nodes);
        }

        internal void AddEdgesToSelect(IEnumerable<FreeBaseEdge> edges)
        {
            _edgesToSelect.UnionWith(edges);
        }

        private void selectTriplet(FreeBaseNode source, FreeBaseEdge edge, FreeBaseNode target)
        {
            var triplet = new FreeBaseTriplet(source, edge, target);
            _selectedTriplets.Add(triplet);
        }
    }
}
