using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Dataset
{
    class TaskDialog
    {
        public readonly string Task;

        public readonly IEnumerable<ActionEntry> Entries;

        public int TurnCount => Entries.Where(e => e.Type == "T_utterance").Count();

        public int SuccessCode => Entries.Where(e => e.Type == "T_completition" && e.Data.ContainsKey("success_code")).Select(e => (int)(long)e.Data["success_code"]).FirstOrDefault();

        public DateTime Start => Entries.First().Time;

        public DateTime End => Entries.Last().Time;

        public readonly string ExperimentId;

        public readonly string Id;

        public readonly string LogFilePath;

        internal TaskDialog(string task, LogFile log, IEnumerable<ActionEntry> entries)
        {
            ExperimentId = log.ExperimentId;
            Task = task;
            Entries = entries.ToArray();
            LogFilePath = log.FilePath;

            var startTime = Start.Ticks;
            Id = Path.GetFileName(LogFilePath) + "." + startTime;
        }
    }
}
