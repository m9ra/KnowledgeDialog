using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Task;
using WebBackend.DialogProvider;

namespace WebBackend.Experiment
{
    class DataCollectionExperiment : ExperimentBase
    {
        /// <summary>
        /// Factories indexed by task id.
        /// </summary>
        private readonly List<TaskFactoryBase> _factories = new List<TaskFactoryBase>();

        /// <summary>
        /// Task indexes relative to factory according to task id.
        /// </summary>
        private readonly List<int> _taskIndexes = new List<int>();

        /// <summary>
        /// Codes that are given for successful task completition.
        /// </summary>
        private readonly List<int> _validationCodes = new List<int>();

        public DataCollectionExperiment(string experimentsRoot, string experimentId, int taskCount, params TaskFactoryBase[] factories)
            : base(experimentsRoot, experimentId)
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

        internal override TaskInstance GetTask(int taskId)
        {
            if (_factories.Count <= taskId)
                //no more tasks is here
                return null;

            var factory = _factories[taskId];
            var factoryRelatedIndex = _taskIndexes[taskId];
            var code = _validationCodes[taskId];

            return factory.CreateInstance(taskId, factoryRelatedIndex, code);
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

        protected override WebConsoleBase createConsole(string databasePath)
        {
            return new DataCollectionWebConsole(databasePath);
        }
    }
}
