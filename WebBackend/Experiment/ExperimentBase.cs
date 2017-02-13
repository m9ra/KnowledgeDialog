using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using WebBackend.Task;
using WebBackend.DialogProvider;

namespace WebBackend.Experiment
{
    /// <summary>
    /// Representation of the experiment.
    /// </summary>
    abstract class ExperimentBase
    {

        /// <summary>
        /// Factories indexed by task id.
        /// </summary>
        protected readonly List<TaskFactoryBase> _factories = new List<TaskFactoryBase>();

        /// <summary>
        /// Task indexes relative to factory according to task id.
        /// </summary>
        protected readonly List<int> _taskIndexes = new List<int>();

        /// <summary>
        /// Codes that are given for successful task completition.
        /// </summary>
        protected readonly List<int> _validationCodes = new List<int>();

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

        protected void ExportExperiment(string experimentId, int taskCount, TaskFactoryBase[] factories)
        {
            var writer = new CrowdFlowerCodeWriter(ExperimentRootPath, experimentId);

            //generate all tasks
            while (_taskIndexes.Count < taskCount)
            {
                foreach (var factory in factories)
                {
                    for (var taskIndex = 0; taskIndex < factory.GetTaskCount(); ++taskIndex)
                    {
                        if (_taskIndexes.Count >= taskCount)
                            break;

                        add(factory, taskIndex, writer);
                    }
                }
            }

            writer.Close();
        }

        /// <summary>
        /// Adds factory with given taskIndex. Task is written by writer.
        /// </summary>
        /// <param name="factory">Factory of tasks.</param>
        /// <param name="taskIndex">Task index relative to factory.</param>
        /// <param name="writer">Writer where task will be written.</param>
        private void add(TaskFactoryBase factory, int taskIndex, CrowdFlowerCodeWriter writer)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var taskId = _taskIndexes.Count;

            _factories.Add(factory);
            _taskIndexes.Add(taskIndex);
            _validationCodes.Add(new Random(taskId).Next(1000, 9999));

            var task = GetTask(taskId);
            writer.Write(task);
        }

        internal virtual TaskInstance GetTask(int taskId)
        {
            if (_factories.Count <= taskId)
                //no more tasks is here
                return null;

            var factory = _factories[taskId];
            var factoryRelatedIndex = _taskIndexes[taskId];
            var code = _validationCodes[taskId];

            return factory.CreateInstance(taskId, factoryRelatedIndex, code);
        }

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
