﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Newtonsoft.Json;

namespace WebBackend
{
    public class LogFile
    {
        public readonly string Id;

        public readonly int Size;

        public readonly DateTime Time;

        public readonly string FilePath;

        public readonly string ExperimentId;

        public LogFile(string path, string experimentId)
        {
            FilePath = path;
            Id = Path.GetFileName(path);
            ExperimentId = experimentId;

            Time = File.GetCreationTime(path);

            if (File.Exists(path))
                Size = (int)new FileInfo(path).Length;
        }

        public static IEnumerable<LogFile> Load(string dirPath, string experimentId)
        {
            var result = new List<LogFile>();
            foreach (var file in Directory.GetFiles(dirPath))
            {
                if (file.EndsWith(".json"))
                    result.Add(new LogFile(file,experimentId));
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
                    result.Add(new ActionEntry(data));
                }

            return result;
        }
    }
}