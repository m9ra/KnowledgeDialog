using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.DataCollection;

using WebBackend.Task;
using WebBackend.DialogProvider;

namespace WebBackend.Experiment
{
    class QuestionCollectionExperiment : ExperimentBase
    {
        /// <summary>
        /// Codes that are given for successful task completition.
        /// </summary>
        private readonly List<int> _validationCodes = new List<int>();

        private readonly QuestionCollection _questions;

        public QuestionCollectionExperiment(string experimentsRoot, string experimentId, int taskCount, QuestionCollection questions)
            : base(experimentsRoot, experimentId)
        {
            _questions = questions;
            var writer = new CrowdFlowerCodeWriter(ExperimentRootPath, experimentId);

            //generate all tasks
            for (var taskIndex = 0; taskIndex < taskCount; ++taskIndex)
            {
                add(taskIndex, writer);
            }

            writer.Close();
        }

        ///<inheritdoc/>
        internal override TaskInstance GetTask(int taskIndex)
        {
            return new InformativeTaskInstance("Chat with the bot", new NodeReference[0], new NodeReference[0], "question_collection", _validationCodes[taskIndex], "question_collection.haml");
        }

        /// <summary>
        /// Adds factory with given taskIndex. Task is written by writer.
        /// </summary>
        /// <param name="taskIndex">Task index relative to factory.</param>
        /// <param name="writer">Writer where task will be written.</param>
        private void add(int taskIndex, CrowdFlowerCodeWriter writer)
        {
            _validationCodes.Add(new Random(taskIndex).Next(1000, 9999));

            writer.Write(taskIndex, _validationCodes.Last().ToString());
        }

        ///<inheritdoc/>
        protected override WebConsoleBase createConsole(string databasePath)
        {
            return new QuestionCollectionWebConsole(databasePath, _questions);
        }
    }
}
