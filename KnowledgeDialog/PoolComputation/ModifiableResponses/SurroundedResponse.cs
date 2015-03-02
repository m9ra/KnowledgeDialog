using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PoolComputation.ModifiableResponses
{
    class SurroundedResponse : ModifiableResponse
    {
        private readonly IEnumerable<NodeReference> _reportedNodes;

        private readonly SurroundingPatternPart _prefix;

        private readonly SurroundingPatternPart _suffix;

        public SurroundedResponse(IEnumerable<NodeReference> reportedNodes, SurroundingPattern pattern)
        {
            _reportedNodes = reportedNodes.ToArray();
            _prefix = pattern.Prefix;
            _suffix = pattern.Suffix;
        }

        public override ResponseBase CreateResponse()
        {
            var prefix = _prefix.GetPattern(_reportedNodes);
            var suffix = _suffix.GetPattern(_reportedNodes);

            var nodesString = stringifyNodes();
            return new SimpleResponse(prefix + nodesString + suffix);
        }

        private string stringifyNodes()
        {
            var nodesString = string.Join(" and ", _reportedNodes.Select(n => n.Data));
            return nodesString;
        }

        public override bool Modify(string modification)
        {
            var nodesString = stringifyNodes();
            var reportedIndex = modification.IndexOf(nodesString);
            if (reportedIndex < 0)
                //TODO: we cannot modify reported part now
                return false;

            var modificationPrefix = modification.Substring(0, reportedIndex);
            var modificationSuffix = modification.Substring(reportedIndex + nodesString.Length);

            _prefix.SetPattern(_reportedNodes, modificationPrefix);
            _suffix.SetPattern(_reportedNodes, modificationSuffix);
            return true;
        }
    }

    class SurroundingPattern
    {
        internal readonly SurroundingPatternPart Prefix;

        internal readonly SurroundingPatternPart Suffix;

        internal SurroundingPattern(ComposedGraph graph, string defaultPrefix, string defaultSuffix)
        {
            Prefix = new SurroundingPatternPart(graph, defaultPrefix);
            Suffix = new SurroundingPatternPart(graph, defaultSuffix);
        }
    }

    class SurroundingPatternPart
    {
        internal readonly KnowledgeClassifier<string> SingleNodeClassifier;

        internal readonly KnowledgeClassifier<string> MultipleNodesClassifier;

        private string _singleDefaultValue;

        private string _multiDefaultValue;

        internal SurroundingPatternPart(ComposedGraph graph, string defaultValue)
        {
            SingleNodeClassifier = new KnowledgeClassifier<string>(graph);
            MultipleNodesClassifier = new KnowledgeClassifier<string>(graph);

            _singleDefaultValue = defaultValue;
            _multiDefaultValue = defaultValue;
        }

        public string GetPattern(IEnumerable<NodeReference> nodes)
        {
            switch (nodes.Count())
            {
                case 0:
                    throw new NotImplementedException();

                case 1:
                    if (_singleDefaultValue != null)
                        return _singleDefaultValue;
                    return SingleNodeClassifier.Classify(nodes.First());

                default:
                    if (_multiDefaultValue != null)
                        return _multiDefaultValue;

                    //TODO other nodes can be also taken into consideration 
                    return MultipleNodesClassifier.Classify(nodes.First());
            }
        }

        public void SetPattern(IEnumerable<NodeReference> nodes, string pattern)
        {
            //reset default value, because we have advice
            

            switch (nodes.Count())
            {
                case 0:
                    throw new NotImplementedException();

                case 1:
                    _singleDefaultValue = null;
                    SingleNodeClassifier.Advice(nodes.First(), pattern);
                    break;

                default:
                    //TODO other nodes can be also taken into consideration 
                    _multiDefaultValue = null;
                    MultipleNodesClassifier.Advice(nodes.First(), pattern);
                    break;
            }
        }
    }
}
