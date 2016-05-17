using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation
{
    abstract class ModifiableResponse
    {
        public abstract ResponseBase CreateResponse();

        /// <summary>
        /// Modify system, so the next response will has form of the given modification.
        /// </summary>
        /// <param name="modification">Modification of current response.</param>
        public abstract bool Modify(string modification);
    }
}
