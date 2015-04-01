using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;
using KnowledgeDialog.PoolComputation;

using KnowledgeDialog.Dialog.Utterances;

namespace WebBackend
{
    class WebConsole
    {
        /// <summary>
        /// Parsers that are used for basic utterance recognition.
        /// </summary>
        private static readonly Func<string, UtteranceBase>[] _utteranceParsers = new Func<string, UtteranceBase>[]{
            AdviceUtterance.TryParse,
            NoUtterance.TryParse,
            AskUtterance.TryParse
        };

        private readonly StateDialogManager _manager;

        internal string CurrentHTML { get; private set; }

        internal WebConsole(string storageFullPath)
        {
            CurrentHTML = systemTextHTML("Hello, how can I help you?");

            _manager = new StateDialogManager(storageFullPath, new FlatPresidentLayer());
        }

        internal void Input(string utterance)
        {
            var formattedUtterance = utterance.Trim();
            CurrentHTML += userTextHTML(formattedUtterance);

            var parsed = parseUtterance(utterance);
            if (parsed == null)
                return;

            var response = parsed.HandleManager(_manager);
            CurrentHTML += systemTextHTML(response.ToString());
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
