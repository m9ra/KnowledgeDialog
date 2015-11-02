using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog;
using KnowledgeDialog.Dialog;

using KnowledgeDialog.Dialog.Utterances;
using KnowledgeDialog.Dialog.Responses;

namespace WebBackend.DialogProvider
{
    abstract class WebConsoleBase
    {
        /// <summary>
        /// Lock for QA index
        /// </summary>
        protected static readonly object _L_qa_index = new object();

        private ResponseBase _lastResponse;

        private string _currentHTML;

        private IInputDialogManager _manager;

        protected abstract IInputDialogManager createDialoggManager();

        internal string CurrentHTML
        {
            get
            {
                ensureInitialization();
                return _currentHTML;
            }
        }

        internal ResponseBase LastResponse
        {
            get
            {
                ensureInitialization();
                return _lastResponse;
            }
        }

        internal ResponseBase Input(string utterance)
        {
            ensureInitialization();

            var formattedUtterance = utterance.Trim();
            var parsedUtterance = KnowledgeDialog.Dialog.UtteranceParser.Parse(utterance);
            ResponseBase response;
            lock (_L_qa_index)
            {
                _currentHTML += userTextHTML(formattedUtterance);
                response = _manager.Input(parsedUtterance);
                _lastResponse = response;
                _currentHTML += systemTextHTML(response.ToString());
            }

            return response;
        }

        internal void Close()
        {
            //Closing dialog manager will cause
            //closing output of question answering modul call storage!!!
        }

        private void ensureInitialization()
        {
            if (_lastResponse != null)
                //initialization has been completed
                return;

            _manager = createDialoggManager();
            _lastResponse = _manager.Initialize();
            _currentHTML = systemTextHTML(_lastResponse.ToString());

            if (_manager == null)
                throw new NullReferenceException("_manager");

            if (_lastResponse == null)
                throw new NullReferenceException("_lastResponse");

            if (_currentHTML == null)
                throw new NullReferenceException("_currentHTML");
        }

        private static string systemTextHTML(string text)
        {
            return "<div class='system_text'>" + text + "</div>";
        }

        private static string userTextHTML(string text)
        {
            return "<div class='user_text'>" + text + "</div>";
        }
    }
}
