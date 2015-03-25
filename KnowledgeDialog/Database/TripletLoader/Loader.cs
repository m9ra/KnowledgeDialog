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
    class Loader
    {
        private readonly StreamReader _reader;

        internal readonly ExplicitLayer DataLayer;

        public Loader(string path)
        {
            _reader = new StreamReader(path);
            DataLayer = new ExplicitLayer();

            var limit = 4300000;

            while (!_reader.EndOfStream)
            {
                var line = _reader.ReadLine();

                var parts = line.Split(';');
                var source = parts[0];
                var edge = parts[1];
                var target = parts[2];

                var sourceNode = GraphLayerBase.CreateReference(source);
                var targetNode = GraphLayerBase.CreateReference(target);
                DataLayer.AddEdge(sourceNode, edge, targetNode);

                SentenceParser.RegisterEntity(target);

                --limit;
                if (limit < 0)
                    break;
            }
        }
    }
}
