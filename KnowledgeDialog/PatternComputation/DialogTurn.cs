using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    class DialogTurn
    {
        /// <summary>
        /// Determine that turn has been closed.
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// Graph snapshot that was available for current turn.
        /// </summary>
        public readonly ComposedGraph Graph;

        public DialogTurn(ComposedGraph graph)
        {
            Graph = graph.CreateSnapshot();
        }


        /// <summary>
        /// Close turn. It prevents any further changes.
        /// </summary>
        internal void Close()
        {
            IsClosed = true;
        }

    }
}
