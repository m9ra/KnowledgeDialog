using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class MappingControl
    {
        public readonly string Substitution;

        public readonly double Score;

        private readonly object _index;

        private readonly IMappingProvider _provider;

        public MappingControl(string substitution, double score, object index, IMappingProvider provider)
        {
            Substitution = substitution;
            Score = score;
            _provider = provider;

            _index = index;
        }

        internal void Suggest(bool isCorrect)
        {
            if (isCorrect)
            {
                _provider.DesiredScore(_index, 1.0);
            }
            else
            {
                _provider.DesiredScore(_index, Score * 0.5);
            }
        }
    }
}
