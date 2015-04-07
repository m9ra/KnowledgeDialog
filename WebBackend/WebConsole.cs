using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation;
using KnowledgeDialog.PoolComputation.StateDialog;

using KnowledgeDialog.Dialog.Utterances;

namespace WebBackend
{
    class WebConsole
    {
        /// <summary>
        /// Lock for QA index
        /// </summary>
        private static readonly object _L_qa_index = new object();

        /// <summary>
        /// Parsers that are used for basic utterance recognition.
        /// </summary>
        private static readonly Func<string, UtteranceBase>[] _utteranceParsers = new Func<string, UtteranceBase>[]{
            AdviceUtterance.TryParse,
            NoUtterance.TryParse,
            AskUtterance.TryParse
        };

        /// <summary>
        /// Mapping of QA modules according to their storages
        /// </summary>
        private static readonly Dictionary<string, QuestionAnsweringModule> _questionAnsweringModules = new Dictionary<string, QuestionAnsweringModule>();

        private readonly StateDialogManager _manager;

        internal string CurrentHTML { get; private set; }

        internal readonly TaskInstance Task;

        internal WebConsole(string storageFullpath, UserTracker tracker)
        {
            var isExperiment = storageFullpath.EndsWith("experiment.dialog");
            if (isExperiment)
                Task = TaskFactory.GetTask(tracker);

            if (storageFullpath == "")
                storageFullpath = null;

            CurrentHTML = systemTextHTML("Hello, how can I help you?");
            _manager = createManager(storageFullpath);
        }

        internal void Input(string utterance)
        {
            var formattedUtterance = utterance.Trim();
            CurrentHTML += userTextHTML(formattedUtterance);

            var parsed = parseUtterance(utterance);
            if (parsed == null)
                return;

            var response = parsed.HandleManager(_manager);
            if (Task != null)
                Task.Register(response);

            CurrentHTML += systemTextHTML(response.ToString());
        }

        private static StateDialogManager createManager(string storageFullPath)
        {
            lock (_L_qa_index)
            {
                QuestionAnsweringModule qa;
                if (storageFullPath == null)
                {
                    qa = createQAModule(null);
                }
                else
                {
                    if (!_questionAnsweringModules.TryGetValue(storageFullPath, out qa))
                        _questionAnsweringModules[storageFullPath] = qa = createQAModule(storageFullPath);
                }
                return new StateDialogManager(new StateContext(qa));
            }
        }

        private static QuestionAnsweringModule createQAModule(string storageFullPath)
        {
            var qa = new QuestionAnsweringModule(DialogWeb.Graph, new CallStorage(storageFullPath));
            return qa;
        }

        private static string systemTextHTML(string text)
        {
            return "<div class='system_text'>" + text + "</div>";
        }

        private static string userTextHTML(string text)
        {
            return "<div class='user_text'>" + text + "</div>";
        }

        private UtteranceBase parseUtterance(string utterance)
        {
            //handle console commands
            switch (utterance)
            {
                case "end":
                case "exit":
                case "esc":
                    return null;
            }

            foreach (var parser in _utteranceParsers)
            {
                var result = parser(utterance);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
