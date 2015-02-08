using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.PatternComputation.Actions;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    class PathFeature
    {
        /// <summary>
        /// Path defining the reference.
        /// </summary>
        public readonly KnowledgePath Path;

        /// <summary>
        /// Containing group.
        /// </summary>
        public readonly KnowledgeGroup ContainingGroup;

        internal PathFeature(KnowledgePath path, KnowledgeGroup containingGroup)
        {
            Path = path;
            ContainingGroup = containingGroup;
        }
    }
}
