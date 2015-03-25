using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.ModifiableResponses;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    abstract class StateBase
    {
        public readonly static EdgeIdentifier ForwardEdge = new EdgeIdentifier();

        protected StateContext Context { get; private set; }

        internal ModifiableResponse Execute(StateContext context)
        {
            try
            {
                Context = context;
                EnsureInitialized<Dictionary<string, ResponseStorage>>(() => new Dictionary<string, ResponseStorage>());

                return execute();
            }
            finally
            {
                Context = null;
            }
        }

        protected virtual ModifiableResponse execute()
        {
            //there is nothing to do now
            return null;
        }

        protected ModifiableResponse Response(string defaultUtterance)
        {
            var storages = Get<Dictionary<string, ResponseStorage>>();
            ResponseStorage storage;
            if (!storages.TryGetValue(defaultUtterance, out storage))
                storages[defaultUtterance] = storage = new ResponseStorage(defaultUtterance);

            return new NoContextResponse(storage);
        }

        protected ModifiableResponse Response(string prefix, IEnumerable<NodeReference> nodes, string suffix = "")
        {
            var storageKey = "$" + prefix + "$" + suffix;

            if (prefix.Length > 0) prefix = prefix + " ";
            if (suffix.Length > 0) suffix = " " + suffix;

            Context.EnsureInitialized<SurroundingPattern>(storageKey, () => new SurroundingPattern(Context.Graph, prefix, suffix));
            var pattern = Context.Get<SurroundingPattern>(storageKey);
            return new SurroundedResponse(nodes, pattern);
        }

        protected ModifiableResponse EmitEdge(EdgeIdentifier edge)
        {
            Context.CurrentOutput = new EdgeInput(edge);
            return null;
        }

        protected NodeReference Node(object data)
        {
            return Context.Graph.GetNode(data);
        }

        protected T Get<T>()
        {
            return Context.Get<T>(this.GetType());
        }

        protected T Get<ContextType, T>()
        {
            return Context.Get<T>(typeof(ContextType));
        }

        protected void EnsureInitialized<T>(Func<T> lazyCreator)
        {
            Context.EnsureInitialized<T>(this.GetType(), lazyCreator);
        }

    }
}
