using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.IO;
using System.IO.Compression;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    delegate void TripletHandler(string freebaseId, string edge, string value);

    class FreebaseEntity
    {
        internal readonly string FreebaseId;

        internal readonly List<string> Aliases = new List<string>();

        internal string Label;

        internal string Description;

        internal FreebaseEntity(string freebaseId)
        {
            FreebaseId = freebaseId;
        }
    }

    class FreebaseDumpProcessor
    {
        public static readonly string RdfIdPrefix = "<http://rdf.freebase.com/ns/m.";

        /// <summary>
        /// Path to freebase file.
        /// </summary>
        private readonly string _dumpFile;

        /// <summary>
        /// Writer of gzip output stream.
        /// </summary>
        private DumpWriter _writer;

        /// <summary>
        /// Entities to write.
        /// </summary>
        private Dictionary<string, FreebaseEntity> _entitiesToWrite;

        /// <summary>
        /// Ids which edges will be searched in the data file.
        /// </summary>
        internal readonly HashSet<string> TargetIds = new HashSet<string>();

        internal FreebaseDumpProcessor(string freebaseDumpFile)
        {
            _dumpFile = freebaseDumpFile;
        }

        internal void AddTargetMid(string mid)
        {
            TargetIds.Add(processMid(mid));
        }

        /// <summary>
        /// Runs iteration on the data.
        /// </summary>
        internal void WriteDump(string output)
        {
            iterateLines(writeTargetIds);

            _writer = new DumpWriter(output);
            foreach (var value in _entitiesToWrite.Values)
            {
                _writer.Write(value.FreebaseId, value.Label, value.Aliases, value.Description);
            }
            _writer.Close();
            _writer = null;
        }

        private void writeTargetIds(string freebaseId, string edge, string value)
        {
            if (!freebaseId.StartsWith(RdfIdPrefix))
                return;

            var id = freebaseId.Substring(RdfIdPrefix.Length, freebaseId.Length - RdfIdPrefix.Length - 1);
            if (!TargetIds.Contains(freebaseId))
                return;

            FreebaseEntity entity;
            if (!_entitiesToWrite.TryGetValue(id, out entity))
                _entitiesToWrite[id] = entity = new FreebaseEntity(id);

            switch (edge)
            {
                case "TODO":
                    throw new NotImplementedException();
                default:
                    return;
            }
        }

        private void iterateLines(TripletHandler handler)
        {
            var lineIndex = 0;
            var fileLength = new FileInfo(_dumpFile).Length;
            var startTime = DateTime.Now;

            //152470320
            using (var fileStream = new FileStream(_dumpFile, FileMode.Open, FileAccess.Read))
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    using (var stream = entry.Open())
                    {
                        var file = new StreamReader(stream);
                        while (!file.EndOfStream)
                        {
                            var currentPosition = fileStream.Position;
                            var line = file.ReadLine();

                            ++lineIndex;
                            if (lineIndex % 50000 == 0)
                            {
                                var percentage = 100.0 * currentPosition / fileLength;

                                var currentDuration = DateTime.Now - startTime;
                                var expectedDuration = new TimeSpan((long)(currentDuration.Ticks / percentage * 100.0));
                                var remainingTime = expectedDuration - currentDuration;
                                Console.WriteLine("{0:0.00}% remaining time: {1:hh\\:mm\\:ss}", percentage, remainingTime);
                            }

                            var parts = line.Split('\t');
                            if (parts[3] != ".")
                                throw new NotImplementedException();

                            handler(parts[0], parts[1], parts[2]);
                        }
                    }
                }
            }
        }

        private string getValue(Dictionary<string, object> entity, string containerId, string valueId)
        {
            var container = entity[containerId] as JObject;
            if (container == null)
                return null;

            var valueContainer = container.GetValue(valueId) as JObject;
            if (valueContainer == null)
                return null;

            var value = valueContainer.GetValue("value");
            if (value == null)
                return null;

            return value.ToString();
        }

        private IEnumerable<string> getValues(Dictionary<string, object> entity, string containerId, string valueId)
        {
            var container = entity[containerId] as JObject;
            if (container == null)
                return Enumerable.Empty<string>();

            var valuesContainer = container.GetValue(valueId) as JArray;
            if (valuesContainer == null)
                return Enumerable.Empty<string>();

            var result = new List<string>();
            foreach (JObject valueContainer in valuesContainer)
            {
                var value = valueContainer.GetValue("value");
                result.Add(value.ToString());
            }
            return result;
        }

        private string getDataValue(JObject propertyContainer, string propertyId)
        {
            var propertyArray = propertyContainer.GetValue(propertyId) as JArray;
            if (propertyArray == null)
                return null;

            var propertyObject = propertyArray.First as JObject;
            var propertyMainsnak = propertyObject.GetValue("mainsnak") as JObject;

            var datavalue = propertyMainsnak.GetValue("datavalue") as JObject;
            if (datavalue == null)
                return null;

            var valueObject = datavalue.GetValue("value");
            if (valueObject == null)
                return null;

            var value = valueObject.ToString();
            return value;
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

        internal void AddTargetMids(IEnumerable<string> ids)
        {
            TargetIds.UnionWith(ids);
        }
    }
}
