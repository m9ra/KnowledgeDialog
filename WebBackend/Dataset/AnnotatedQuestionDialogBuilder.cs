using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.DataCollection;

namespace WebBackend.Dataset
{
    class AnnotatedQuestionDialogBuilder
    {
        internal bool HasValidDialog { get { return _explanationTurns.Count > 0 && _answerTurns.Count > 0 && _question != null; } }

        private readonly List<AnnotatedQuestionActionEntry> _explanationTurns = new List<AnnotatedQuestionActionEntry>();

        private readonly List<AnnotatedQuestionActionEntry> _answerTurns = new List<AnnotatedQuestionActionEntry>();

        private readonly QuestionCollection _questions;

        private readonly AnnotatedQuestionLogFile _log;

        private string _question;

        private string _answerId;

        private bool _isAnswerPhase;

        private AnnotatedQuestionDialogBuilder(AnnotatedQuestionLogFile log, QuestionCollection questions)
        {
            _questions = questions;
            _log = log;
        }

        internal static IEnumerable<AnnotatedQuestionDialog> ParseDialogs(AnnotatedQuestionLogFile log, QuestionCollection questions)
        {
            var actions = log.LoadActions().ToArray();
            var validDialogs = new List<AnnotatedQuestionDialog>();

            //fill builders with dialog data
            AnnotatedQuestionDialogBuilder currentBuilder = null;
            foreach (var action in actions)
            {
                if (action.IsDialogStart && currentBuilder != null)
                {
                    if (currentBuilder.HasValidDialog)
                        validDialogs.Add(currentBuilder.Build());

                    currentBuilder = null;
                }

                if (currentBuilder == null)
                    currentBuilder = new AnnotatedQuestionDialogBuilder(log, questions);

                currentBuilder.Register(action);
            }

            return validDialogs;
        }

        private AnnotatedQuestionDialog Build()
        {
            if (!HasValidDialog)
                throw new NotSupportedException("Cannot create invalid dialog");

            var answerNames = Configuration.Db.GetNames(_answerId);
            return new AnnotatedQuestionDialog(_log, _question, _answerId, answerNames, _explanationTurns, _answerTurns);
        }

        private void Register(AnnotatedQuestionActionEntry action)
        {
            if (action.IsDialogStart)
            {
                _question = action.ParseQuestion();
                _answerId = _questions.GetAnswerId(_question);
            }

            if (!action.IsRegularTurn)
                //we want regular turns only
                return;

            if (action.Act != null && action.Act.StartsWith("RequestAnswer"))
                _isAnswerPhase = true;

            if (_isAnswerPhase)
            {
                _answerTurns.Add(action);
            }
            else
            {
                _explanationTurns.Add(action);
            }
        }
    }
}
