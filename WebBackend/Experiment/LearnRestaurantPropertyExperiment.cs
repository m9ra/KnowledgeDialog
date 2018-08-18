using KnowledgeDialog.Knowledge;
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
        private readonly bool _exportKnowledge;

        private readonly bool _useKnowledge;

        public LearnRestaurantPropertyExperiment(string rootPath, string experimentId, int taskCount, bool exportKnowledge, bool useKnowledge) : base(rootPath, experimentId)
        {
            _exportKnowledge = exportKnowledge;
            _useKnowledge = useKnowledge;

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
            if (taskId % 2 == 0)
            {
                return new RestaurantTaskInstance(taskId, "Find a restaurant.", "learn_restaurant_property", _validationCodes[taskId], "learn_restaurant_property_retrieve.haml");
            }
            else
            {
                return new RestaurantTaskInstance(taskId, "Provide restaurant info.", "learn_restaurant_property", _validationCodes[taskId], "learn_restaurant_property.haml");
            }
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
        protected override WebConsoleBase createConsole(string databasePath, int taskId, SolutionLog log)
        {
            if (taskId % 2 == 0)
            {
                return new PhraseAgentWebConsole(PerceptiveDialogBasedAgent.OutputRecognitionAlgorithm.BombayPresenceOrModerateSearchFallback, Knowledge, _exportKnowledge, _useKnowledge, log);
            }
            else
            {
                return new PhraseAgentWebConsole(PerceptiveDialogBasedAgent.OutputRecognitionAlgorithm.NewBombayProperty, Knowledge, _exportKnowledge, _useKnowledge, log);
            }
        }

    }
}
