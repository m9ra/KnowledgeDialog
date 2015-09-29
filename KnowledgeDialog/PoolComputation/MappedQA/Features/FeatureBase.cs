using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    abstract class FeatureBase
    {
        abstract protected int getHashCode();

        abstract protected bool equals(FeatureBase featureBase);

        public override int GetHashCode()
        {
            return getHashCode();
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            var f = obj as FeatureBase;
            if (f == null)
                return false;

            return equals(f);
        }
    }

    class FeatureInstance
    {
        internal readonly ParsedUtterance Origin;

        internal readonly FeatureBase Feature;

        internal readonly IEnumerable<int> CoveredPositions;

        internal int MaxPosition { get { return Origin.Words.Count() - 1; } }

        internal FeatureInstance(ParsedUtterance origin, FeatureBase feature, params int[] coveredPositions)
        {
            Origin = origin;
            Feature = feature;
            CoveredPositions = coveredPositions.ToArray();
        }
    }
}
