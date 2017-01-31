using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Parsing
{
    [Serializable]
    public class EntityInfo : IComparable<EntityInfo>
    {
        public readonly string Label;

        public readonly string Mid;

        public readonly string Description;

        public readonly double Score = 0;

        public readonly string BestAliasMatch = null;

        public readonly int InBounds;

        public readonly int OutBounds;

        private readonly double _bestLabelScore = 0;

        public EntityInfo(string mid, string label, int inBounds, int outBounds, string description = null)
        {
            Mid = mid;
            Label = label;
            InBounds = inBounds;
            OutBounds = outBounds;
            Description = description;
        }

        private EntityInfo(string mid, string label, int inBounds, int outBounds, string name, double score, double bestLabelScore, string description)
        {
            Mid = mid;
            BestAliasMatch = name;
            Score = score;
            Label = label;
            InBounds = inBounds;
            OutBounds = outBounds;
            Description = description;
            _bestLabelScore = bestLabelScore;
        }

        public EntityInfo AddScore(string match, double score)
        {
            var isAlias = true;//match.Length < 15;
            if (isAlias && score > _bestLabelScore)
            {
                return new EntityInfo(Mid, Label, InBounds, OutBounds, match, Score + score, score, Description);
            }
            else
            {
                return new EntityInfo(Mid, Label, InBounds, OutBounds, BestAliasMatch, Score + score, _bestLabelScore, Description);
            }
        }

        public EntityInfo SubtractScore(double score)
        {
            return new EntityInfo(Mid, Label, InBounds, OutBounds, BestAliasMatch, Score - score, _bestLabelScore, Description);
        }

        public EntityInfo WithScore(double score)
        {
            return new EntityInfo(Mid, Label, InBounds, OutBounds, BestAliasMatch, score, _bestLabelScore, Description);
        }

        public override bool Equals(object obj)
        {
            var o = obj as EntityInfo;
            if (o == null)
                return false;

            return this.Mid.Equals(o.Mid);
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", Label, Mid);
        }

        public override int GetHashCode()
        {
            return Mid.GetHashCode();
        }

        public int CompareTo(EntityInfo other)
        {
            return Score.CompareTo(other.Score);
        }
    }
}
