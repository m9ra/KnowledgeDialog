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
        }

        internal void Write(string experimentId, int taskId)
        {
            var code = ExperimentData.GetSuccessCode(experimentId, taskId);
            _writer.Write(WebPath + "?taskid=" + taskId + ";" + code + Environment.NewLine);
        }

        internal void Close()
        {
            _writer.Close();
        }
    }
}
