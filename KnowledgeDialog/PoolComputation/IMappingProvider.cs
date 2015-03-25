using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation
{
    interface IMappingProvider
    {
        void DesiredScore(object _index, double p);
    }
}
