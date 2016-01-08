using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    class Ranked<T>
    {
        internal readonly T Value;

        internal readonly double Rank;

        internal Ranked(T value, double rank)
        {
            Value = value;
            Rank = rank;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("({0},{1:0.00})", Value, Rank);
        }
    }
}
