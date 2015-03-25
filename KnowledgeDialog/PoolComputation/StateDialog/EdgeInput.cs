using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    class EdgeInput
    {
        private readonly string _utterance;
        private readonly EdgeIdentifier _edge;

        public EdgeInput(string utterance)
        {
            if (utterance == null)
                throw new NullReferenceException("utterance");

            _utterance = utterance;
        }

        public EdgeInput(EdgeIdentifier edge)
        {
            if (edge == null)
                throw new NullReferenceException("edge");

            _edge = edge;
        }
        internal IEnumerable<Tuple<Trigger, string, double>> GetScore(StateGraphBuilder state)
        {
            if (_utterance != null)
            {
                return state.UtteranceEdge(_utterance);
            }
            else if (_edge != null)
            {
                return state.DirectEdge(_edge);
            }
            {
                throw new NotImplementedException();
            }
        }
    }
}
