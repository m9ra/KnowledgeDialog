using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.MappedQA.PoolRules
{
    abstract class RuleBitBase
    {
        private string _notationCache;

        internal string Notation
        {
            get
            {
                if (_notationCache == null)
                    _notationCache = getNotation();

                return _notationCache;
            }
        }

        /// <summary>
        /// Template method for getting bit notation.
        /// </summary>
        /// <returns>The bit notation.</returns>
        protected abstract string getNotation();

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Notation.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            var o = obj as RuleBitBase;
            if (o == null)
                return false;

            return Notation.Equals(o.Notation);
        }

        public override string ToString()
        {
            return Notation;
        }
    }
}
