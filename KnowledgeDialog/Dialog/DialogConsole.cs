using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog.Utterances;


namespace KnowledgeDialog.Dialog
{
    public class DialogConsole : IDialogProvider
    {
        /// <summary>
        /// Parsers that are used for basic utterance recognition.
        /// </summary>
        private static readonly Func<string, UtteranceBase>[] _utteranceParsers = new Func<string, UtteranceBase>[]{
            AdviceUtterance.TryParse,
            NoUtterance.TryParse,
            AskUtterance.TryParse
        };

        /// <summary>
        /// Inputs that are simulated as comming from console input.
        /// </summary>
        private readonly Queue<string> _simulatedUtterances = new Queue<string>();

        /// <summary>
        /// Manager that controls provided dialog.
        /// </summary>
        private readonly IDialogManager _manager;

        /// <summary>
        /// Manager that doesn't require utterance parsing.
        /// </summary>
        private readonly IInputDialogManager _inputManager;

        public DialogConsole(object manager)
        {
            _manager = manager as IDialogManager;
            _inputManager = manager as IInputDialogManager;
        }

        /// <summary>
        /// Simulate input from user.
        /// </summary>
        /// <param name="utterances">Simulated utterances.</param>
        public void SimulateInput(params string[] utterances)
        {
            foreach (var utterance in utterances)
            {
                _simulatedUtterances.Enqueue(utterance);
            }
        }

        /// <summary>
        /// Run dialog service (is blocking)
        /// </summary>
        public void Run(bool useDirectInput = false)
        {
            if (_inputManager != null)
            {
                var initializationResponse = _inputManager.Initialize();
                ConsoleServices.PrintOutput(initializationResponse);
            }

            for (; ; )
            {
                var utterance = readUtterance();

                ResponseBase response;
                if (_manager == null)
                {
                    var parsedSentence = Dialog.UtteranceParser.Parse(utterance);
                    response = _inputManager.Input(parsedSentence);
                }
                else
                {
                    var parsedUtterance = parseUtterance(utterance);
                    if (parsedUtterance == null)
                        return;

                    if (useDirectInput)
                    {
                        response = _manager.Ask(utterance);
                    }
                    else
                    {
                        response = parsedUtterance.HandleManager(_manager);
                    }
                }
                ConsoleServices.PrintOutput(response);
            }
        }

        #region Dialog routines

        private string readUtterance()
        {
            ConsoleServices.PrintPrompt();

            string utterance;
            if (_simulatedUtterances.Count > 0)
            {
                utterance = _simulatedUtterances.Dequeue();
                ConsoleServices.PrintLine(utterance, ConsoleServices.ActiveColor);
            }
            else
            {
                utterance = ConsoleServices.ReadLine(ConsoleServices.ActiveColor);
            }

            return utterance;
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

        #endregion

    }
}
