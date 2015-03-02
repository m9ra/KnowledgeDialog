using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation
{
    class DialogContext
    {
        internal readonly PoolActionMapping QuestionAnsweringMapping = new PoolActionMapping();

        internal readonly PoolActionMapping AdviceMapping = new PoolActionMapping();

        private readonly List<ModifiableResponse> _responseHistory = new List<ModifiableResponse>();

        private readonly List<string> _utteranceHistory = new List<string>();

        public readonly ContextPool Pool;

        public ComposedGraph Graph { get { return Pool.Graph; } }

        public IEnumerable<ModifiableResponse> ResponseHistory { get { return _responseHistory; } }

        public IEnumerable<string> UtteranceHistory { get { return _utteranceHistory; } }

        internal DialogContext(ComposedGraph graph)
        {
            Pool = new ContextPool(graph);
        }

        internal void RegisterResponse(ModifiableResponse response)
        {
            _responseHistory.Add(response);
        }

        internal void RegisterUtterance(string utterance)
        {
            _utteranceHistory.Add(utterance);
        }
    }
}
