using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog;
using KnowledgeDialog.Dialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation;
using KnowledgeDialog.PoolComputation.StateDialog;

using KnowledgeDialog.Dialog.Utterances;
using KnowledgeDialog.Dialog.Responses;

namespace WebBackend
{
    class WebConsole
    {
        /// <summary>
        /// Lock for QA index
        /// </summary>
        private static readonly object _L_qa_index = new object();

        /// <summary>
        /// Mapping of QA modules according to their storages
        /// </summary>
        private static readonly Dictionary<string, QuestionAnsweringModule> _questionAnsweringModules = new Dictionary<string, QuestionAnsweringModule>();

        private readonly StateDialogManager _manager;

        internal string CurrentHTML { get; private set; }

        internal ResponseBase LastResponse { get; private set; }

        internal WebConsole(string storageFullpath)
        {
            if (storageFullpath == "")
                storageFullpath = null;

            LastResponse = new SimpleResponse("Hello, how can I help you?");
            CurrentHTML = systemTextHTML(LastResponse.ToString());
            _manager = createManager(storageFullpath);
        }

        internal ResponseBase Input(string utterance)
        {
            var formattedUtterance = utterance.Trim();
            CurrentHTML += userTextHTML(formattedUtterance);

            var response = _manager.Input(utterance);
            CurrentHTML += systemTextHTML(response.ToString());

            return response;
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
            var qa = new QuestionAnsweringModule(Program.Graph, new CallStorage(storageFullPath));
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

        internal void Close()
        {
            _manager.Close();
        }
    }
}
