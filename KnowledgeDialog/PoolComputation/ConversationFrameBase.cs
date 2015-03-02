using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.PoolComputation.ModifiableResponses;

using KnowledgeDialog.PoolComputation.Frames;

namespace KnowledgeDialog.PoolComputation
{
    abstract class ConversationFrameBase
    {
        public bool IsComplete { get; protected set; }

        protected abstract ModifiableResponse FrameInitialization();

        protected abstract ModifiableResponse DefaultHandler();

        protected ConversationContext ConversationContext;

        protected string PreviousInput { get { return ConversationContext.PreviousInput; } }

        protected string CurrentInput { get { return ConversationContext.CurrentInput; } }

        private bool _isInitialized = false;

        internal ConversationFrameBase(ConversationContext context)
        {
            ConversationContext = context;
            EnsureInitialized<Dictionary<string, ResponseStorage>>(() => new Dictionary<string, ResponseStorage>());
        }

        public virtual ModifiableResponse Input(string utterance)
        {
            if (utterance != null)
                ConversationContext.ReportInput(utterance);

            if (!_isInitialized)
            {
                var response = FrameInitialization();
                _isInitialized = true;
                return response;
            }
            //TODO mapping routing should be here

            return DefaultHandler();
        }

        protected ModifiableResponse Response(ConversationFrameBase conversationFrame)
        {
            return new FrameResponse(conversationFrame);
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

            ConversationContext.EnsureInitialized<SurroundingPattern>(storageKey, () => new SurroundingPattern(ConversationContext.Graph, prefix, suffix));
            var pattern = ConversationContext.Get<SurroundingPattern>(storageKey);
            return new SurroundedResponse(nodes, pattern);
        }

        protected ModifiableResponse YesNoQuestion(string question, Action yesHandler, Action noHandler)
        {
            return new FrameResponse(new YesNoFrame(ConversationContext, question, yesHandler, noHandler));
        }

        protected void EnsureInitialized<T>(Func<T> lazyCreator)
        {
            ConversationContext.EnsureInitialized<T>(this.GetType(), lazyCreator);
        }

        protected T Get<T>()
        {
            return ConversationContext.Get<T>(this.GetType());
        }

        protected T Get<Context, T>()
        {
            return ConversationContext.Get<T>(typeof(Context));
        }
    }

    class ConversationContext
    {
        public string PreviousInput { get { return _utterances[_utterances.Count - 2]; } }

        public string CurrentInput { get { return _utterances[_utterances.Count - 1]; } }

        internal readonly ComposedGraph Graph;

        internal IEnumerable<object> StoredData { get { return _storage.Values; } }

        private Dictionary<Tuple<object, Type>, object> _storage = new Dictionary<Tuple<object, Type>, object>();

        private List<string> _utterances = new List<string>();

        internal ConversationContext(ComposedGraph graph)
        {
            Graph = graph;
        }

        internal T Get<T>(object owner)
        {
            var key = Tuple.Create(owner, typeof(T));

            return (T)_storage[key];
        }

        internal void EnsureInitialized<T>(object owner, Func<T> lazyCreator)
        {
            var key = Tuple.Create(owner, typeof(T));

            if (_storage.ContainsKey(key))
                //nothing to do
                return;

            _storage[key] = lazyCreator();
        }

        internal void ReportInput(string utterance)
        {
            _utterances.Add(utterance);
        }
    }
}
