using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SimpleQuestions
{
    /// <summary>
    /// Reader for large FreeBase dumps.
    /// </summary>
    abstract class FreeBaseReader
    {
        /// <summary>
        /// File where the triplets are stored.
        /// </summary>
        internal readonly string File;

        protected abstract void ProcessEntry(FreeBaseNode source, FreeBaseEdge edge, FreeBaseNode target);


        internal FreeBaseReader(string file)
        {
            File = file;
        }

        /// <summary>
        /// Reads whole file and process it.
        /// </summary>
        internal void Process()
        {
            var freeBaseStreamReader =
   new StreamReader(File);
            string line;
            while ((line = freeBaseStreamReader.ReadLine()) != null)
            {
                var splits = line.Split('\t');

                var source = new FreeBaseNode(splits[0]);
                var edge = new FreeBaseEdge(splits[1]);
                var target = new FreeBaseNode(splits[2]);

                ProcessEntry(source, edge, target);
            }
        }
    }
}
