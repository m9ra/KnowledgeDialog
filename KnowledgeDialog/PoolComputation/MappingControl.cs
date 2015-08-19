using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class MappingControl<T>
    {
        public readonly string Substitution;

        public readonly double Score;

        public readonly T Value;

        public readonly ParsedUtterance ParsedSentence;

        private readonly IMappingProvider<T> _provider;

        public MappingControl(string substitution, double score, IMappingProvider<T> provider, T value, ParsedUtterance originalSentence)
        {
            Substitution = substitution;
            Score = score;
            Value = value;
            ParsedSentence = originalSentence;

            _provider = provider;
        }

        internal void Suggest(bool isCorrect)
        {
            //Now we don't take advantage from these suggestions
        }

        internal MappingControl<NewT> ChangeValue<NewT>(NewT value)
        {
            return new MappingControl<NewT>(Substitution, Score, null, value, ParsedSentence);
        }
    }
}
