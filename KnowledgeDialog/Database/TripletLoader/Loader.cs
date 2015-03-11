using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace KnowledgeDialog.Database.TripletLoader
{
    class Loader
    {
        private readonly StreamReader _reader;

        public Loader(string path)
        {
            _reader = new StreamReader(path);


        }
    }
}
