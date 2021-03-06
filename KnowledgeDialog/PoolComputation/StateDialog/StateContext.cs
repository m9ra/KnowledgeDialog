﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    public class StateContext
    {

        internal readonly HeuristicQAModule QuestionAnsweringModule;

        internal readonly ComposedGraph Graph;

        internal ContextPool Pool { get { return QuestionAnsweringModule.Pool; } }

        internal EdgeInput CurrentOutput;

        internal EdgeInput PreviousOutput { get; private set; }

        internal string Input { get; private set; }

        internal IEnumerable<string> Substitutions { get { return _substitutions; } }

        private readonly Dictionary<StateProperty2, string> _storedValues = new Dictionary<StateProperty2, string>();

        private readonly Dictionary<Tuple<object, Type>, object> _storage = new Dictionary<Tuple<object, Type>, object>();

        private readonly List<string> _substitutions = new List<string>();

        internal readonly CallStorage CallStorage;

        public int MaximumUserReport = 3;

        public StateContext(ComposedGraph composedGraph, string serializationPath = null)
            : this(new HeuristicQAModule(composedGraph, new CallStorage(serializationPath)))
        {
        }

        public StateContext(HeuristicQAModule module)
        {
            QuestionAnsweringModule = module;
            CallStorage = module.Storage;
            Graph = module.Graph;
        }

        internal void StartTurn(string utterance)
        {
            Input = utterance;
        }

        internal void SetValue(StateProperty2 property, string p)
        {
            if (property != null)
                _storedValues[property] = p;
        }

        internal void Remove(StateProperty2 property)
        {
            _storedValues.Remove(property);
        }

        internal string Get(StateProperty2 property)
        {
            string result;
            _storedValues.TryGetValue(property, out result);

            return result;
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


        internal void EdgeReset()
        {
            _substitutions.Clear();

            PreviousOutput = CurrentOutput;
            CurrentOutput = null;
        }

        internal void AddSubstitution(string substitution)
        {
            _substitutions.Add(substitution);
        }

        internal bool IsTrue(StateProperty2 property)
        {
            return Get(property) == StateProperty2.TrueValue;
        }

        internal bool IsSet(StateProperty2 property)
        {
            return _storedValues.ContainsKey(property);
        }

        internal void Close()
        {
            CallStorage.Close();
        }
    }
}
