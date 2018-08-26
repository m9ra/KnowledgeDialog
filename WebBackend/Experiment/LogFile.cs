using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Newtonsoft.Json;
using WebBackend.Dataset;

namespace WebBackend
{
    public class LogFile
    {
        public readonly string Id;

        public readonly int Size;

        public readonly DateTime Time;

        public readonly string FilePath;

        public readonly string ExperimentId;

        public LogFile(string path)
        {
            FilePath = path;
            Id = Path.GetFileName(path);
            Time = File.GetCreationTime(path);

            //find users directory
            var usersDirectory = path;
            while (Path.GetFileNameWithoutExtension(usersDirectory) != "users" && usersDirectory != "")
            {
                usersDirectory = Path.GetDirectoryName(usersDirectory);
            }

            if (usersDirectory == "")
                usersDirectory = Path.Combine(Path.GetDirectoryName(path), "users");

            var experimentDirectory = Path.GetDirectoryName(usersDirectory);
            ExperimentId = Path.GetFileNameWithoutExtension(experimentDirectory);

            if (File.Exists(path))
                Size = (int)new FileInfo(path).Length;
        }

        public static IEnumerable<LogFile> Load(string dirPath)
        {
            var result = new List<LogFile>();
            var fullPath = Path.GetFullPath(dirPath);
            foreach (var file in Directory.GetFiles(fullPath))
            {
                if (file.EndsWith(".json"))
                    result.Add(new LogFile(file));
            }

            result.Sort((a, b) => b.Time.CompareTo(a.Time));
            return result;
        }

        internal IEnumerable<ActionEntry> LoadActions()
        {
            var result = new List<ActionEntry>();
            if (Size == 0)
                return result;

            var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var reader = new StreamReader(fs))
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Trim();
                    if (line == "")
                        continue;

                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(line);
                    result.Add(new ActionEntry(result.Count, data));
                }

            return result;
        }

        internal IEnumerable<TaskDialog> ParseDialogs()
        {
            var actions = LoadActions();
            var dialogs = new List<TaskDialog>();

            var task = getTask(actions);

            var entriesBuffer = new List<ActionEntry>();
            foreach (var action in actions)
            {
                if (action.IsDialogStart && entriesBuffer.Count > 0)
                {
                    var dialog = new TaskDialog(task, this, entriesBuffer);
                    if (dialog.TurnCount > 0)
                        dialogs.Add(dialog);

                    entriesBuffer.Clear();
                }

                entriesBuffer.Add(action);
            }

            var lastdialog = new TaskDialog(task, this, entriesBuffer);
            if (lastdialog.TurnCount > 0)
                dialogs.Add(lastdialog);

            return dialogs;
        }

        private string getTask(IEnumerable<ActionEntry> actions)
        {
            return actions.Where(a => a.Type == "T_task").First().Text;
        }
    }
}
