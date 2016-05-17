using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

using KnowledgeDialog.PoolComputation.MappedQA.PoolRules;

namespace KnowledgeDialog.PoolComputation.MappedQA.Features
{
    abstract class FeatureBase
    {
        abstract protected int getHashCode();

        abstract protected bool equals(FeatureBase featureBase);

        abstract protected string toString();

        abstract protected double probability(RulePart part);

        abstract protected void setMapping(FeatureInstance featureInstance, NodeMapping mapping);

        internal protected double Probability(RulePart part)
        {
            return probability(part);
        }

        internal void SetMapping(FeatureInstance featureInstance, NodeMapping mapping)
        {
            setMapping(featureInstance, mapping);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return getHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            var f = obj as FeatureBase;
            if (f == null)
                return false;

            return equals(f);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return toString();
        }
    }

    class FeatureInstance
    {
        internal readonly ParsedUtterance Origin;

        internal readonly FeatureBase Feature;

        internal readonly IEnumerable<int> CoveredPositions;

        internal int MaxOriginPosition { get { return Origin.Words.Count() - 1; } }

        internal FeatureInstance(ParsedUtterance origin, FeatureBase feature, params int[] coveredPositions)
        {
            Origin = origin;
            Feature = feature;
            CoveredPositions = coveredPositions.ToArray();
        }

        internal void SetMapping(NodeMapping mapping)
        {
            Feature.SetMapping(this, mapping);
        }
    }
}
