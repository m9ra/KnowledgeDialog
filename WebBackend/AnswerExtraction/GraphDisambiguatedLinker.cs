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


        internal GraphDisambiguatedLinker(FreebaseDbProvider db, string verbsLexicon, bool useDisambiguation = true)
            : base(db, verbsLexicon)
        {

        }

        protected override IEnumerable<EntityInfo> disambiguateTo(IEnumerable<EntityInfo> entities, int entityHypothesisCount)
        {
            entities = base.disambiguateTo(entities, 5);

            if (_useDisambiguation)
            {
                var orderedEntities = entities.OrderByDescending(e =>
                {
                    var entry = Db.GetEntryFromId(e.Mid);
                    var count = entry.Targets.Count();
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
