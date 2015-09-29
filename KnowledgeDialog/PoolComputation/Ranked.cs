using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation
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
    }
}
