﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using KnowledgeDialog.Knowledge;

using WebBackend.Dataset;

delegate void EntityLineProcessor(string entity, string edge, string[] targets);

namespace WebBackend.AnswerExtraction
{
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

        internal GraphLayerBase GetLayerFrom(IEnumerable<string> mids)
        {
            var midTable = new HashSet<string>(mids);
            var layer = new ExplicitLayer();

            iterateLines((entityId, edge, targetEntities) =>
            {
                var entity = getMid(entityId);
                if (!midTable.Contains(entity))
                    return;

                var entityNode = layer.CreateReference(intern(getMid(entity)));
                edge = intern(edge);

                foreach (var targetEntityId in targetEntities)
                {
                    var targetEntity = getMid(targetEntityId);
                    if (!midTable.Contains(targetEntity))
                        continue;

                    var targetNode = layer.CreateReference(intern(targetEntity));
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
                        targetEntities[i] = getId(targetMids[i]);

                    }
                    processor(entity1, edge, targetEntities);
                }
            }
        }

        private string processEdge(string edgeId)
        {
            if (!edgeId.StartsWith(FreebaseLoader.EdgePrefix))
                throw new NotSupportedException("Edge format unknown: " + edgeId);

            return edgeId.Substring(edgeId.Length);
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
            string result;
            if (!_internedStrings.TryGetValue(str, out result))
                _internedStrings[str] = result = str;

            return result;
        }
    }
}
