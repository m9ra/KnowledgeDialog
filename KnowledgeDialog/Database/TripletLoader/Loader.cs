using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.Database.TripletLoader
{
    public class Loader
    {
        private readonly StreamReader _reader;

        private readonly Dictionary<string, string> _myInterning = new Dictionary<string, string>();

        private readonly Dictionary<string, IList<string>> _neighbours = new Dictionary<string, IList<string>>();

        public readonly ExplicitLayer DataLayer;

        public Loader(string path)
        {
            _reader = new StreamReader(path, Encoding.UTF8);
            DataLayer = new ExplicitLayer();

            Console.WriteLine("Counting lines: " + path);
            var lineCount = 0;
            var edges = new HashSet<string>();
            var myInterning = new Dictionary<string, string>();
            while (!_reader.EndOfStream)
            {
                var line = _reader.ReadLine();
                ++lineCount;
            }

            _reader.BaseStream.Seek(0, SeekOrigin.Begin);

            Console.WriteLine("Loading graph(" + lineCount + "): " + path);
            var lastPercent = 0;
            var lineIndex = 0;

            while (!_reader.EndOfStream)
            {
                var line = _reader.ReadLine();

                var parts = split(line);
                var source = intern(parts[0]);
                var edge = intern(parts[1]);
                var target = intern(parts[2]);

                IList<string> neighbours;
                if (!_neighbours.TryGetValue(source, out neighbours))
                    _neighbours[source] = neighbours = new List<string>();

                neighbours.Add(target);

                var sourceNode = GraphLayerBase.CreateReference(source);
                var targetNode = GraphLayerBase.CreateReference(target);
                DataLayer.AddEdge(sourceNode, edge, targetNode);
                SentenceParser.RegisterEntity(target);

                ++lineIndex;
                var currentPercent = 100 * lineIndex / lineCount;
                if (currentPercent != lastPercent)
                {
                    GC.Collect();
                    Console.WriteLine(currentPercent);
                    lastPercent = currentPercent;
                }
            }

            var maxLen = 0;
            KeyValuePair<string, IList<string>> max = new KeyValuePair<string,IList<string>>();
            foreach (var pair in _neighbours)
            {
                if (pair.Value.Count > maxLen)
                {
                    maxLen = pair.Value.Count;
                    max = pair;
                }
            }

            Console.WriteLine(max.Key + " " + maxLen);
        }

        private string[] split(string str)
        {
            string[] result = new string[3];
            var lastIndex = 0;
            for (var i = 2; i < str.Length; ++i)
            {
                var ch = str[i];
                if (ch == ';')
                {
                    if (lastIndex == 0)
                    {
                        result[0] = intern(str.Substring(0, i));
                        lastIndex = i + 1;
                    }
                    else
                    {
                        result[1] = intern(str.Substring(lastIndex, i - lastIndex));
                        result[2] = intern(str.Substring(i + 1));
                        break;
                    }
                }
            }

            return result;
        }

        private string intern(string str)
        {
            string result;
            if (!_myInterning.TryGetValue(str, out result))
                _myInterning[str] = result = str;

            return result;
        }
    }
}
