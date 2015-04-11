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

            while (!_reader.EndOfStream)
            {
                var line = _reader.ReadLine();

                var parts = line.Split(';');
                var source = String.Intern(parts[0]);
                var edge = String.Intern(parts[1]);
                var target = String.Intern(parts[2]);

                var sourceNode = GraphLayerBase.CreateReference(source);
                var targetNode = GraphLayerBase.CreateReference(target);
                DataLayer.AddEdge(sourceNode, edge, targetNode);

                SentenceParser.RegisterEntity(target);
            }
        }
    }
}
