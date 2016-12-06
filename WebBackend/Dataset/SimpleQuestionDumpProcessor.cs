using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Dataset
{
    delegate void EntityLineProcessor(string entity, string edge, string[] targets);

    class SimpleQuestionDumpProcessor
    {
        /// <summary>
        /// Path to freebase file.
        /// </summary>
        private readonly string _freebaseDataFile;

        /// <summary>
        /// Strings that are "manually" interned.
        /// </summary>
        private readonly Dictionary<string, string> _internedStrings = new Dictionary<string, string>();

        /// <summary>
        /// Input targets.
        /// </summary>
        private readonly Dictionary<string, HashSet<Tuple<string, string>>> _inTargets = new Dictionary<string, HashSet<Tuple<string, string>>>();

        /// <summary>
        /// Output targets.
        /// </summary>
        private readonly Dictionary<string, Tuple<string, string>[]> _outTargets = new Dictionary<string, Tuple<string, string>[]>();

        /// <summary>
        /// All ids met in the DB.
        /// </summary>
        internal readonly HashSet<string> AllIds = new HashSet<string>();

        /// <summary>
        /// Ids which edges will be searched in the data file.
        /// </summary>
        internal readonly HashSet<string> TargetIds = new HashSet<string>();

        /// <summary>
        /// Ids that have been find during the iteration.
        /// </summary>
        internal readonly HashSet<string> _foundIds = new HashSet<string>();

        internal bool UseInterning = true;  

        internal SimpleQuestionDumpProcessor(string freebaseDataFile)
        {
            _freebaseDataFile = freebaseDataFile;
        }

        internal void AddTargetMid(string mid)
        {
            TargetIds.Add(getId(mid));
        }

        /// <summary>
        /// Runs iteration on the data.
        /// </summary>
        internal void RunIteration()
        {
            var lineIndex = 0;
            using (var file = new StreamReader(_freebaseDataFile))
            {
                while (!file.EndOfStream)
                {
                    ++lineIndex;
                    var line = file.ReadLine();
                    var parts = line.Split('\t');
                    var entity1 = getId(parts[0]);
                    var edge = parts[1];
                    var entities = parts[2].Split(' ');
                    AllIds.Add(entity1);
                    foreach (var entity2Mid in entities)
                    {
                        var entity2 = getId(entity2Mid);
                        AllIds.Add(entity2);

                        if (TargetIds.Contains(entity1))
                        {
                            _foundIds.Add(entity2);
                        }
                        else if (TargetIds.Contains(entity2))
                        {
                            _foundIds.Add(entity1);
                        }
                    }
                }
            }
        }

        internal void LoadInOutBounds()
        {
            iterateLines(_loadInOutBounds);
        }

        private void _loadInOutBounds(string sourceId, string edge, string[] targetIds)
        {

            Tuple<string, string>[] currentOutBounds;
            var outTargets = targetIds.Select(i => Tuple.Create(edge, i));
            if (_outTargets.TryGetValue(sourceId, out currentOutBounds))
            {
                _outTargets[sourceId] = outTargets.Union(currentOutBounds).ToArray();
            }
            else
            {
                _outTargets[sourceId] = outTargets.ToArray();
            }

            foreach (var targetId in targetIds)
            {
                HashSet<Tuple<string, string>> currentInBounds;
                if (!_inTargets.TryGetValue(targetId, out currentInBounds))
                {
                    _inTargets[targetId] = currentInBounds = new HashSet<Tuple<string, string>>();
                }
                currentInBounds.Add(Tuple.Create(edge, sourceId));
            }
        }

        internal void RunConcreteIteration()
        {
            var lineIndex = 0;
            using (var file = new StreamReader(_freebaseDataFile))
            {
                while (!file.EndOfStream)
                {
                    ++lineIndex;
                    var line = file.ReadLine();
                    var parts = line.Split('\t');
                    var entity1 = getId(parts[0]);
                    var edge = parts[1];
                    var entities = parts[2].Split(' ');
                    if (TargetIds.Contains(entity1))
                    {
                        _foundIds.UnionWith(entities);
                    }
                }
            }
        }

        internal void RunAbstractIteration()
        {
            var lineIndex = 0;
            using (var file = new StreamReader(_freebaseDataFile))
            {
                while (!file.EndOfStream)
                {
                    ++lineIndex;
                    var line = file.ReadLine();
                    var parts = line.Split('\t');
                    var entity1 = getId(parts[0]);
                    var edge = parts[1];
                    var entities = parts[2].Split(' ');
                    foreach (var entity2Mid in entities)
                    {
                        var entity2 = getId(entity2Mid);
                        if (TargetIds.Contains(entity2))
                            _foundIds.Add(entity2);
                    }
                }
            }
        }

        internal void RefillTargets()
        {
            TargetIds.UnionWith(_foundIds);
            _foundIds.Clear();
        }


        internal IEnumerable<Tuple<string, string>> GetInTargets(string id)
        {
            HashSet<Tuple<string, string>> result;
            _inTargets.TryGetValue(id, out result);
            return result;
        }

        internal IEnumerable<Tuple<string, string>> GetOutTargets(string id)
        {
            Tuple<string, string>[] result;
            _outTargets.TryGetValue(id, out result);
            return result;
        }

        internal GraphLayerBase GetLayerFromIds(IEnumerable<string> ids)
        {
            var midTable = new HashSet<string>(ids);
            var layer = new ExplicitLayer();

            iterateLines((entityId, edge, targetEntities) =>
            {
                if (!midTable.Contains(entityId))
                    return;

                var entityNode = layer.CreateReference(intern(entityId));
                edge = intern(edge);

                foreach (var targetEntityId in targetEntities)
                {
                    if (!midTable.Contains(targetEntityId))
                        continue;

                    var targetNode = layer.CreateReference(intern(targetEntityId));
                    layer.AddEdge(entityNode, edge, targetNode);
                }
            });

            return layer;
        }

        internal GraphLayerBase GetLayer()
        {
            var layer = new ExplicitLayer();

            iterateLines((entity, edge, targetEntities) =>
            {
                var entityNode = layer.CreateReference(intern(entity));
                edge = intern(edge);

                foreach (var targetEntity in targetEntities)
                {
                    var targetNode = layer.CreateReference(intern(targetEntity));
                    layer.AddEdge(entityNode, edge, targetNode);
                }
            });

            return layer;
        }


        internal void ExportEdges(MysqlFreebaseConnector mysqlConnector)
        {
            iterateLines((entity, edge, targetEntities) =>
            {
                mysqlConnector.WriteEntityEdges(entity, edge, targetEntities);
            });
            mysqlConnector.FlushWrites();
        }

        private void iterateLines(EntityLineProcessor processor)
        {
            var totalSize = new FileInfo(_freebaseDataFile).Length;
            using (var fileStream = File.OpenRead(_freebaseDataFile))
            using (var file = new StreamReader(fileStream))
            {
                var currentLine = 0;
                while (!file.EndOfStream)
                {
                    ++currentLine;
                    if (currentLine % 100000 == 0)
                    {
                        var currentPosition = fileStream.Position;
                        Console.WriteLine("{0:0.00}", 100.0 * currentPosition / totalSize);
                    }


                    var line = file.ReadLine();
                    var parts = line.Split('\t');
                    var entity1 = getId(parts[0]);
                    var edge = processEdge(parts[1]);

                    var targetMids = parts[2].Split(' ');
                    var targetEntities = new string[targetMids.Length];
                    for (var i = 0; i < targetMids.Length; ++i)
                    {
                        targetEntities[i] = getId(intern(targetMids[i]));

                    }
                    processor(intern(entity1), intern(edge), targetEntities);
                }
            }
        }

        private string processEdge(string edgeId)
        {
            if (!edgeId.StartsWith(FreebaseLoader.EdgePrefix))
                throw new NotSupportedException("Edge format unknown: " + edgeId);

            return edgeId.Substring(FreebaseLoader.EdgePrefix.Length);
        }

        private string getId(string mid)
        {
            if (!mid.StartsWith(FreebaseLoader.IdPrefix))
                throw new NotSupportedException("Mid format unknown: " + mid);

            return mid.Substring(FreebaseLoader.IdPrefix.Length);
        }

        private string getMid(string id)
        {
            return FreebaseLoader.IdPrefix + id;
        }

        private string intern(string str)
        {
            if (!UseInterning)
                return str;

            string result;
            if (!_internedStrings.TryGetValue(str, out result))
                _internedStrings[str] = result = str;

            return result;
        }
    }
}
