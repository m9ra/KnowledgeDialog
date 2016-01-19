using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SimpleQuestions
{
    delegate void ProggressReporter(int processedCount);

    class FreeBaseTriplet
    {
        internal readonly FreeBaseNode Source;

        internal readonly FreeBaseEdge Edge;

        internal readonly FreeBaseNode Target;

        internal FreeBaseTriplet(FreeBaseNode source, FreeBaseEdge edge, FreeBaseNode target)
        {
            Source = source;
            Edge = edge;
            Target = target;
        }
    }

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

        private int _tripletProcessed = 0;

        internal int ProcessedCountThreshold = 100000;

        internal event ProggressReporter ProgressReporter;

        internal FreeBaseReader(string file)
        {
            File = file;
        }

        /// <summary>
        /// Reads whole file and process it.
        /// </summary>
        internal void Iterate()
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
                ++_tripletProcessed;

                if (_tripletProcessed % ProcessedCountThreshold == 0)
                    if (ProgressReporter != null)
                        ProgressReporter(_tripletProcessed);
            }
        }
    }
}
