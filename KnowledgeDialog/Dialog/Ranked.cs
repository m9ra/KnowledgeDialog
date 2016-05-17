using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    public class Ranked<T> : IComparable<Ranked<T>>
    {
        public readonly T Value;

        public readonly double Rank;

        public Ranked(T value, double rank)
        {
            Value = value;
            Rank = rank;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("({0},{1:0.00})", Value, Rank);
        }

        public int CompareTo(Ranked<T> other)
        {
            return other.Rank.CompareTo(Rank);
        }
    }
}
