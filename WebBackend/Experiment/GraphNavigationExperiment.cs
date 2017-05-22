using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using WebBackend.Task;
using WebBackend.DialogProvider;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.DataCollection;

using WebBackend.AnswerExtraction;
using KnowledgeDialog.GraphNavigation;
using WebBackend.Dataset;

namespace WebBackend.Experiment
{
    class GraphNavigationExperiment : ExperimentBase
    {
        internal readonly int RequiredInformativeTurnCount = 3;

        private readonly IEnumerable<string> _phrases;

        private readonly NavigationData _data;

        private readonly ILinker _linker;

        private readonly FreebaseDbProvider _db;

        public GraphNavigationExperiment(string experimentsRoot, string experimentId, int taskCount, QuestionDialogDatasetReader seedDialogs)
            : base(experimentsRoot, experimentId)
        {
            _db = Configuration.Db;
            var phrases = loadPhrases(seedDialogs, _db);
            _phrases = phrases.ToArray();
            var navigationDataPath = Path.Combine(ExperimentRootPath, "navigation_data.nvd");
            _data = new NavigationData(navigationDataPath);

            _linker = createLinker(_phrases);

            var writer = new CrowdFlowerCodeWriter(ExperimentRootPath, experimentId);

            //generate all tasks
            for (var taskIndex = 0; taskIndex < taskCount; ++taskIndex)
            {
                add(taskIndex, writer);
            }

            writer.Close();
        }

        private ILinker createLinker(IEnumerable<string> excludedPhrases)
        {
            var coreLinker = new GraphDisambiguatedLinker(_db, "./verbs.lex", useGraphDisambiguation: true);
            coreLinker.SetBlacklistLabels(excludedPhrases);

            var linker = new DiskCachedLinker(ExperimentRootPath + "/experiment_linker.link", 1, (u, c) => coreLinker.LinkUtterance(u, c), _db);
            linker.CacheResult = true;
            return linker;
        }

        private IEnumerable<string> loadPhrases(QuestionDialogDatasetReader seedDialogs, FreebaseDbProvider db)
        {
            var entities = seedDialogs.Dialogs.Select(d => db.GetEntryFromMid(d.AnswerMid)).Where(e => e != null).ToArray();

            var phrases = new List<string>();
            foreach (var entity in entities)
            {
                if (entity.Aliases.Count() < 2)
                    continue;

                var alias = entity.Aliases.First();
                if (!meetsPhraseRequirements(alias))
                    continue;

                phrases.Add(entity.Aliases.First());
            }

            return phrases.Distinct().ToArray();
        }

        private bool meetsPhraseRequirements(string alias)
        {
            foreach (var ch in alias)
            {
                if (!char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch))
                    return false;

                if (ch > 'z')
                    return false;
            }
            return true;
        }

        ///<inheritdoc/>
        internal override TaskInstance GetTask(int taskId)
        {
            return new InformativeTaskInstance(taskId, "Chat with the bot", new NodeReference[0], new NodeReference[0], "graph_navigation", _validationCodes[taskId], RequiredInformativeTurnCount, "graph_navigation.haml");
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
            return new GraphNavigationWebConsole(databasePath, _phrases, _data, _linker);
        }
    }
}
