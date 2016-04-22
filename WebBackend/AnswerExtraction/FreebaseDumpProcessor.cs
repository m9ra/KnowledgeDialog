using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class FreebaseDumpProcessor
    {
        /// <summary>
        /// Path to freebase file.
        /// </summary>
        private readonly string _freebaseDataFile;

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

        internal FreebaseDumpProcessor(string freebaseDataFile)
        {
            _freebaseDataFile = freebaseDataFile;
        }

        internal void AddTargetMid(string mid)
        {
            TargetIds.Add(processMid(mid));
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
                    var entity1 = processMid(parts[0]);
                    var edge = parts[1];
                    var entities = parts[2].Split(' ');
                    AllIds.Add(entity1);
                    foreach (var entity2Mid in entities)
                    {
                        var entity2 = processMid(entity2Mid);
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
                    var entity1 = processMid(parts[0]);
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
                    var entity1 = processMid(parts[0]);
                    var edge = parts[1];
                    var entities = parts[2].Split(' ');
                    foreach (var entity2Mid in entities)
                    {
                        var entity2 = processMid(entity2Mid);
                        if(TargetIds.Contains(entity2))
                            _foundIds.Add(entity2);
                    }
                }
            }
        }

        internal void WriteCoverDump(string outputfile)
        {
            using (var output = new StreamWriter(outputfile))
            {
                using (var file = new StreamReader(_freebaseDataFile))
                {
                    while (!file.EndOfStream)
                    {
                        var line = file.ReadLine();
                        var parts = line.Split('\t');
                        var entity1 = processMid(parts[0]);
                        var edge = processEdge(parts[1]);
                        var entity2 = processMid(parts[2]);

                        if (TargetIds.Contains(entity1) && TargetIds.Contains(entity2))
                        {
                            var outputLine = entity1 + ";" + edge + ";" + entity2;
                            output.WriteLine(outputLine);
                        }
                    }
                }
            }
        }

        internal void RefillTargets()
        {
            TargetIds.UnionWith(_foundIds);
            _foundIds.Clear();
        }

        private string processEdge(string edgeId)
        {
            if (!edgeId.StartsWith(FreebaseLoader.EdgePrefix))
                throw new NotSupportedException("Edge format unknown: " + edgeId);

            return edgeId.Substring(edgeId.Length);
        }

        private string processMid(string mid)
        {
            if (!mid.StartsWith(FreebaseLoader.IdPrefix))
                throw new NotSupportedException("Mid format unknown: " + mid);

            return mid.Substring(FreebaseLoader.IdPrefix.Length);
        }
    }
}
