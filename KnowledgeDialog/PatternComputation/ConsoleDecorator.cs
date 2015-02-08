using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.PatternComputation
{
    class ConsoleDecorator : IDialogManager
    {
        private readonly DialogManager _decoratedManager;

        public ConsoleDecorator(DialogManager decoratedManager)
        {
            _decoratedManager = decoratedManager;
        }

        /// <inheritdoc />
        public ResponseBase Ask(string question)
        {
            var context = _decoratedManager.Context;

            var answer = _decoratedManager.Ask(question);

            var scoredResponses = context.GetScoredResponses(context.ActivePatterns);

            ConsoleServices.BeginSection("hypotheses");
            foreach (var scoredResponse in scoredResponses)
            {
                ConsoleServices.Print(scoredResponse);
            }

            ConsoleServices.EndSection();

            return answer;
        }

        /// <inheritdoc />
        public ResponseBase Negate()
        {
            return _decoratedManager.Negate();
        }

        /// <inheritdoc />
        public ResponseBase Advise(string question, string answer)
        {
            var context = _decoratedManager.Context;

            var previousPatterns = context.ActivePatterns.ToArray();
            var response = _decoratedManager.Advise(question, answer);
            var currentPatterns = context.ActivePatterns;

            var newPatterns = currentPatterns.Except(previousPatterns);
            ConsoleServices.BeginSection("new patterns");
            foreach (var pattern in newPatterns)
            {
                ConsoleServices.Print(pattern);
            }
            ConsoleServices.EndSection();

            return response;
        }
    }
}
