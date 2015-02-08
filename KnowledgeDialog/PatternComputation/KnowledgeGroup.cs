using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    public class KnowledgeGroup
    {
        private readonly List<PathFeature> _features = new List<PathFeature>();

        /// <summary>
        /// Features that are contained in group.
        /// </summary>
        internal IEnumerable<PathFeature> Features { get { return _features; } }


        internal KnowledgeGroup(IEnumerable<KnowledgePath> groupPaths)
        {
            foreach (var path in groupPaths)
            {
                _features.Add(new PathFeature(path, this));
            }
        }
    }
}
