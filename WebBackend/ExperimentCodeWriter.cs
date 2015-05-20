using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace WebBackend
{
    class ExperimentCodeWriter
    {
        private readonly StreamWriter _writer;

        internal static string WebPath = "http://knowledge.packa2.cz/" + ExperimentData.ExperimentId;

        internal ExperimentCodeWriter(string path)
        {
            _writer = new StreamWriter(path);

            _writer.WriteLine("task_url,verification_code");
        }

        internal void Write(string experimentId, int taskId)
        {
            var code = ExperimentData.GetSuccessCode(experimentId, taskId);
            _writer.WriteLine(WebPath + "?taskid=" + taskId + "," + code);
        }

        internal void Close()
        {
            _writer.Close();
        }
    }
}
