using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace WebBackend
{
    /// <summary>
    /// Writes file with codes that are used for CrowdFlower experiments.
    /// </summary>
    class CrowdFlowerCodeWriter
    {
        /// <summary>
        /// Writer that is used for writing of result file.
        /// </summary>
        private readonly StreamWriter _writer;

        /// <summary>
        /// Webpath of root folder, where experiment will be accessible.
        /// </summary>
        internal static string WebPath = "http://knowledge.packa2.cz/";

        internal CrowdFlowerCodeWriter(string experimentRoot, string experimentId)
        {
            var outputPath = Path.Combine(experimentRoot, "crowdflower_" + experimentId + ".csv");
            _writer = new StreamWriter(outputPath);

            //write csv header
            _writer.WriteLine("task_url,verification_code");
        }

        /// <summary>
        /// Writes entry of task with given taskId.
        /// </summary>
        /// <param name="taskId">Id of task.</param>
        /// <param name="code">Completition code of task.</param>
        internal void Write(int taskId, string code)
        {
            _writer.WriteLine(WebPath + "?taskid=" + taskId + "," + code);
        }

        /// <summary>
        /// Closes the results file.
        /// </summary>
        internal void Close()
        {
            _writer.Close();
        }
    }
}
