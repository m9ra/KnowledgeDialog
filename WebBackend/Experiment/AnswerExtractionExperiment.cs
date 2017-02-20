using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Task;
using WebBackend.DialogProvider;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.DataCollection;

using WebBackend.AnswerExtraction;

namespace WebBackend.Experiment
{
    class AnswerExtractionExperiment : ExperimentBase
    {
        private readonly QuestionCollection _questions;

        private readonly ExtractionKnowledge _knowledge;

        public AnswerExtractionExperiment(string experimentsRoot, string experimentId, int taskCount, QuestionCollection questions)
            : base(experimentsRoot, experimentId)
        {
            _questions = questions;
            _knowledge = new ExtractionKnowledge();

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
            return new InformativeTaskInstance(taskId, "Chat with the bot", new NodeReference[0], new NodeReference[0], "question_collection", _validationCodes[taskId], "question_collection.haml");
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
            return new AnswerExtractionWebConsole(databasePath, _questions, _knowledge);
        }
    }
}
