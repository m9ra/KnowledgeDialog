using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using WebBackend.DialogProvider;

namespace WebBackend.Experiment
{
    /// <summary>
    /// Representation of the experiment.
    /// </summary>
    abstract class ExperimentBase
    {
        /// <summary>
        /// Relative path from experiment root to users data.
        /// </summary>
        public readonly static string RelativeUserPath = "users";

        /// <summary>
        /// Id of the experiment.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Path where all files of experiment will be stored.
        /// </summary>
        public readonly string ExperimentRootPath;

        /// <summary>
        /// Path where user based files of experiment will be stored.
        /// </summary>
        public readonly string ExperimentUserPath;

        /// <summary>
        /// Initialize new instance of <see cref="ExperimentBase"/>.
        /// </summary>
        /// <param name="id">Id of the experiment.</param>
        /// <param name="rootPath">Root path where experiment data will be placed.</param>
        protected ExperimentBase(string rootPath, string id)
        {
            Id = id;
            ExperimentRootPath = Path.Combine(rootPath, id);
            ExperimentUserPath = Path.Combine(ExperimentRootPath, RelativeUserPath);

            //prepare file structure
            Directory.CreateDirectory(ExperimentRootPath);
            Directory.CreateDirectory(ExperimentUserPath);
        }

        abstract protected WebConsoleBase createConsole(string databasePath);

        abstract internal TaskInstance GetTask(int taskId);
        
        internal WebConsoleBase CreateConsoleWithDatabase(string databaseIdentifier)
        {
            var databasePath = GetDatabasePath(databaseIdentifier);
            return createConsole(databasePath);
        }

        internal string GetDatabasePath(string database)
        {
            return Path.Combine(ExperimentRootPath, Id + "." + database);
        }

        internal string GetLogPath(string userId, int taskId)
        {
            return Path.Combine(ExperimentUserPath, string.Format("{1}-{0}.json", userId, taskId));
        }

        internal string GetFeedbackPath()
        {
            return Path.Combine(ExperimentRootPath, "feedback.json");
        }

        internal IEnumerable<LogFile> LoadLogFiles()
        {
            return LogFile.Load(ExperimentUserPath);
        }
    }
}
