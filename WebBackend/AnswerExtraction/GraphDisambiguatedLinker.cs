using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Parsing;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class GraphDisambiguatedLinker : UtteranceLinker
    {
        List<ExplicitLayer> _disambiguationLayers = new List<ExplicitLayer>();

        private bool _useDisambiguation = false;

        private HashSet<string> _collectedDisambiguationIds = new HashSet<string>();

        private ComposedGraph _disambiguationGraph;

        internal GraphDisambiguatedLinker(EntityExtractor extractor, string verbsLexicon)
            : base(extractor, verbsLexicon)
        {

        }

        internal void RegisterDisambiguationEntities(IEnumerable<string> utterances)
        {
            foreach (var utterance in utterances)
            {
                LinkUtterance(utterance, 0);
            }
        }

        internal void LoadDisambiguationEntities(SimpleQuestionDumpProcessor processor)
        {
            var layer = processor.GetLayerFromIds(_collectedDisambiguationIds);
            _disambiguationGraph = new ComposedGraph(layer);
            _useDisambiguation = true;
        }

        protected override IEnumerable<EntityInfo> disambiguateTo(IEnumerable<EntityInfo> entities, int entityHypothesisCount)
        {
            entities = base.disambiguateTo(entities, 5);

            if (_useDisambiguation)
            {
                var orderedEntities = entities.OrderByDescending(e =>
                {
                    var entityNode = _disambiguationGraph.GetNode(e.Mid);
                    var neighbours = _disambiguationGraph.GetNeighbours(entityNode, 1000);
                    var count = neighbours.Count();

                    return count;
                });

                return orderedEntities.Take(entityHypothesisCount);
            }
            else
            {
                _collectedDisambiguationIds.UnionWith(entities.Select(e => FreebaseLoader.GetId(e.Mid)));
                return base.disambiguateTo(entities, entityHypothesisCount);
            }
        }

    }
}
