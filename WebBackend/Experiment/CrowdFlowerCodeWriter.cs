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
        /// ID of written experiment.
        /// </summary>
        private readonly string _experimentId;

        /// <summary>
        /// Webpath of root folder, where experiment will be accessible.
        /// </summary>
        internal static string WebPath = "http://knowledge.packa2.cz/";

        internal CrowdFlowerCodeWriter(string experimentRoot, string experimentId)
        {
            _experimentId = experimentId;

            var outputPath = Path.Combine(experimentRoot, "crowdflower_" + experimentId + ".csv");
            _writer = new StreamWriter(outputPath);

            //write csv header
            _writer.WriteLine("task_url,experiment_id,taskid,key");
        }

        /// <summary>
        /// Writes entry of given task.
        /// </summary>
        /// <param name="task">The task.</param>
        internal void Write(TaskInstance task)
        {
            _writer.WriteLine(WebPath + _experimentId + "?taskid=" + task.Id + "," + _experimentId + "," + task.Id + "," + task.ValidationCodeKey);
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
