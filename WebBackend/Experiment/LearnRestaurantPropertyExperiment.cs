using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBackend.DialogProvider;
using WebBackend.Task;

namespace WebBackend.Experiment
{
    class LearnRestaurantPropertyExperiment : ExperimentBase
    {
        public LearnRestaurantPropertyExperiment(string rootPath, string experimentId, int taskCount) : base(rootPath, experimentId)
        {
            var writer = new CrowdFlowerCodeWriter(ExperimentRootPath, experimentId);

            //generate all tasks
            for (var taskIndex = 0; taskIndex < taskCount; ++taskIndex)
            {
                add(taskIndex, writer);
            }

            writer.Close();
        } 
        
        ///<inheritdoc/>
        internal override TaskInstance GetTask(int taskId)
        {
            return new RestaurantTaskInstance(taskId, "Provide restaurant info.", "learn_restaurant_property", _validationCodes[taskId], "learn_restaurant_property.haml");
        }

        /// <summary>
        /// Adds factory with given taskIndex. Task is written by writer.
        /// </summary>
        /// <param name="taskId">Task index relative to factory.</param>
        /// <param name="writer">Writer where task will be written.</param>
        private void add(int taskId, CrowdFlowerCodeWriter writer)
        {
            _validationCodes.Add(new Random(taskId).Next(1000, 9999));

            var task = GetTask(taskId);
            writer.Write(task);
        }

        ///<inheritdoc/>
        protected override WebConsoleBase createConsole(string databasePath)
        {
            return new PhraseAgentWebConsole();
        }
    }
}
