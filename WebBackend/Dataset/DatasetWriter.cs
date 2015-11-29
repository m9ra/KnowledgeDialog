using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;    

namespace WebBackend.Dataset
{
    class DatasetWriter
    {
        /// <summary>
        /// Folder where source files are stored.
        /// </summary>
        internal readonly string SourceFolder;

        internal DatasetWriter(string sourceFolder)
        {
            SourceFolder = sourceFolder;
        }

        /// <summary>
        /// Writes dataset files into the target folder.
        /// </summary>
        /// <param name="targetFolder">Target for dataset files.</param>
        internal void WriteData(string targetFolder)
        {
            foreach (var file in Directory.GetFiles(targetFolder, "*.json"))
            {
                throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }
}
