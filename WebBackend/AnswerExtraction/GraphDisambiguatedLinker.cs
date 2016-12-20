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
        private List<ExplicitLayer> _disambiguationLayers = new List<ExplicitLayer>();

        private bool _useDisambiguation = false;

        private Dictionary<string, EntityInfo> _context = new Dictionary<string, EntityInfo>();

        internal GraphDisambiguatedLinker(FreebaseDbProvider db, string verbsLexicon, bool useGraphDisambiguation = true)
            : base(db, verbsLexicon)
        {
            _useDisambiguation = useGraphDisambiguation;
        }

        internal LinkedUtterance LinkUtterance(string utterance, IEnumerable<EntityInfo> context = null)
        {
            _context.Clear();
            if (context != null)
            {
                foreach (var entity in context)
                {
                    _context[Db.GetFreebaseId(entity.Mid)] = entity;
                }
            }

            var linkedUtterance = base.LinkUtterance(utterance, 20).First();
            if (!_useDisambiguation)
                return linkedUtterance;

            var entityClusters = new List<EntityInfo[]>();
            foreach (var part in linkedUtterance.Parts)
            {
                if (part.Entities.Any())
                {
                    entityClusters.Add(part.Entities.ToArray());
                }
            }

            var disambiguatedClusters = disambiguateClusters(entityClusters);
            var entityQueue = new Queue<EntityInfo>(disambiguatedClusters);
            var disambiguatedParts = new List<LinkedUtterancePart>();
            foreach (var part in linkedUtterance.Parts)
            {
                if (part.Entities.Any())
                {
                    disambiguatedParts.Add(LinkedUtterancePart.Entity(part.Token, new[] { entityQueue.Dequeue() }));
                }
                else
                {
                    disambiguatedParts.Add(part);
                }
            }

            return new LinkedUtterance(disambiguatedParts);
        }

        internal override IEnumerable<LinkedUtterance> LinkUtterance(string utterance, int entityHypCount)
        {
            throw new NotSupportedException();
        }

        private EntityInfo[] disambiguateClusters(IEnumerable<EntityInfo[]> entityClusters)
        {
            //we are trying to find largest best scored entity component


            var clusterArray = pruneClusters(entityClusters);
            var entityMap = new Dictionary<string, HashSet<EntityInfo>>();
            var componentSizes = new Dictionary<EntityInfo, int>();

            var backIds = new HashSet<string>();

            var currentSelection = new int[clusterArray.Length];
            var bestSelection = currentSelection.ToArray();
            var bestScore = evaluateSelection(bestSelection, clusterArray);

            while (currentSelection != null)
            {
                updateToNextSelection(ref currentSelection, clusterArray);
                var currentScore = evaluateSelection(currentSelection, clusterArray);
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestSelection = currentSelection.ToArray();
                }
            }

            var result = new List<EntityInfo>();
            for (var i = 0; i < bestSelection.Length; ++i)
            {
                result.Add(clusterArray[i][bestSelection[i]]);
            }
            return result.ToArray();
        }

        private EntityInfo[][] pruneClusters(IEnumerable<EntityInfo[]> clusters)
        {
            var clustersArray = clusters.ToArray();

            var maxIterationLimit = 2000;
            while (getIterationCount(clustersArray) > maxIterationLimit)
            {
                shrinkClusters(clustersArray);
            }

            return clustersArray;
        }

        private void shrinkClusters(EntityInfo[][] clusters)
        {
            var maxClusterSize = clusters.Select(c => c.Length).Max();
            for (var i = 0; i < clusters.Length; ++i)
            {
                if (clusters[i].Length < maxClusterSize)
                    continue;

                clusters[i] = pruneEntities(clusters[i], maxClusterSize - 1).ToArray();
                return;
            }
        }

        private long getIterationCount(EntityInfo[][] clusters)
        {
            var totalIterationCount = 1L;
            foreach (var cluster in clusters)
            {
                totalIterationCount *= cluster.Length;
            }

            return totalIterationCount;
        }

        private void updateToNextSelection(ref int[] selection, EntityInfo[][] entities)
        {
            for (var i = 0; i < selection.Length; ++i)
            {
                selection[i] += 1;
                if (selection[i] < entities[i].Length)
                    //update was successful
                    return;

                //overflow - shift furhter
                selection[i] = 0;
            }

            //we are at the end
            selection = null;
            return;
        }

        private double evaluateSelection(int[] selection, EntityInfo[][] entities)
        {
            if (selection == null)
                return 0;

            //sum of components where components are calculated from multiplication
            var components = getComponents(selection, entities);
            var accumulator = 0.0;
            foreach (var component in components)
            {
                var componentScale = component.Count() * 100000;
                var componentScore = 1.0 + componentScale;
                foreach (var entity in component)
                {
                    componentScore *= entity.Score;
                }
                accumulator += componentScore;
            }
            return accumulator;
        }

        private IEnumerable<IEnumerable<EntityInfo>> getComponents(int[] selection, EntityInfo[][] entities)
        {
            var entityIndex = new Dictionary<string, EntityInfo>();
            for (var i = 0; i < selection.Length; ++i)
            {
                var entity = entities[i][selection[i]];
                entityIndex[Db.GetFreebaseId(entity.Mid)] = entity;
            }

            var components = new List<IEnumerable<EntityInfo>>();
            while (entityIndex.Count > 0)
            {
                //find a single component
                var component = new List<EntityInfo>();
                components.Add(component);
                var componentSeedId = entityIndex.Keys.First();
                var componentSeed = entityIndex[componentSeedId];

                var entitiesToProcess = new Queue<EntityInfo>();
                entitiesToProcess.Enqueue(componentSeed);
                entityIndex.Remove(componentSeedId);
                if (_context.ContainsKey(componentSeedId))
                    //compensate for beiing in context itself
                    component.Add(componentSeed);


                //construct component from the seed
                while (entitiesToProcess.Count > 0)
                {
                    var entity = entitiesToProcess.Dequeue();
                    component.Add(entity);

                    foreach (var target in Db.GetEntryFromId(Db.GetFreebaseId(entity.Mid)).Targets)
                    {
                        if (_context.ContainsKey(target.Item2))
                        {
                            var contextEntity = _context[target.Item2];
                            if (!component.Contains(contextEntity))
                                component.Add(contextEntity);
                        }

                        if (entityIndex.ContainsKey(target.Item2))
                        {
                            //component was enlarged
                            var targetEntity = entityIndex[target.Item2];
                            entityIndex.Remove(target.Item2);
                            entitiesToProcess.Enqueue(targetEntity);
                        }
                    }
                }

            }

            return components;
        }

        protected override IEnumerable<EntityInfo> pruneEntities(IEnumerable<EntityInfo> entities, int entityHypothesisCount)
        {
            if (_useDisambiguation)
            {
                var contextMatchFactor = 10000;
                entities = base.pruneEntities(entities, entityHypothesisCount * 2);
                var orderedEntities = entities.OrderByDescending(e =>
                {
                    var entry = Db.GetEntryFromId(e.Mid);
                    var accumulator = e.Score;
                    foreach (var target in entry.Targets)
                    {
                        if (_context.ContainsKey(target.Item2))
                            accumulator += contextMatchFactor;
                    }

                    return accumulator;
                });

                return orderedEntities.Take(entityHypothesisCount);
            }
            else
            {
                return base.pruneEntities(entities, entityHypothesisCount);
            }
        }
    }
}
