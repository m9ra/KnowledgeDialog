using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ICSharpCode.SharpZipLib;

using System.IO;
using System.IO.Compression;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    delegate void JsonHandler(Dictionary<string, object> obj);

    class WikidataDumpProcessor
    {
        /// <summary>
        /// Path to freebase file.
        /// </summary>
        private readonly string _dumpFile;

        /// <summary>
        /// Writer of gzip output stream.
        /// </summary>
        private StreamWriter _writer;

        /// <summary>
        /// Ids which edges will be searched in the data file.
        /// </summary>
        internal readonly HashSet<string> TargetIds = new HashSet<string>();

        internal WikidataDumpProcessor(string freebaseDataFile)
        {
            _dumpFile = freebaseDataFile;
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
            using(var fileStream = new FileStream(output, FileMode.Create, FileAccess.Write))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
            {
                _writer = new StreamWriter(gzipStream);
                iterateLines(searchIds);
                _writer.Close();
                _writer = null;
            }
        }

        private void searchIds(Dictionary<string, object> entity)
        {
            var properties = entity["claims"] as JObject;
            var freebaseId = getDataValue(properties, "P646");

            if (freebaseId == null)
                //we require freebase id to be present
                return;

            freebaseId = freebaseId.Substring(3, freebaseId.Length - 3);
            if (!TargetIds.Contains(freebaseId))
                return;


            var label = getValue(entity, "labels", "en");
            var description = getValue(entity, "descriptions", "en");
            var aliases = getValues(entity, "aliases", "en");

            var outputLine = freebaseId + "\t" + label + "\t;" + string.Join(";", aliases) + "\t" + description;
            _writer.WriteLine(outputLine);
        }

        private void iterateLines(JsonHandler handler)
        {
            var lineIndex = 0;
            var fileLength = new FileInfo(_dumpFile).Length;

            using (var fileStream = new FileStream(_dumpFile, FileMode.Open, FileAccess.Read))
            {
                using (var fileGzip = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(fileStream))
                {
                    using (var file = new StreamReader(fileGzip))
                    {
                        var openingLine = file.ReadLine(); //skip first line with [
                        if (openingLine != "[")
                            throw new NotSupportedException();

                        var startTime = DateTime.Now;
                        while (!file.EndOfStream)
                        {
                            var currentPosition = fileStream.Position;

                            var line = file.ReadLine();
                            if (line == "]")
                                //last line
                                continue;

                            ++lineIndex;
                            if (lineIndex % 1000 == 0)
                            {
                                var percentage = 100.0 * currentPosition / fileLength;

                                var currentDuration = DateTime.Now - startTime;
                                var expectedDuration = new TimeSpan((long)(currentDuration.Ticks / percentage * 100.0));
                                var remainingTime = expectedDuration - currentDuration;
                                Console.WriteLine("{0:0.00}% remaining time: {1:hh\\:mm\\:ss}", percentage, remainingTime);
                            }

                            if (line.EndsWith(','))
                                line = line.Substring(0, line.Length-1);
                            

                            var entity = JsonConvert.DeserializeObject<Dictionary<string, object>>(line);
                            handler(entity);
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
