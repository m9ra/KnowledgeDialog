using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.DataCollection;

namespace WebBackend.DialogProvider
{
    class QuestionCollectionWebConsole : WebConsoleBase
    {
        private readonly string _databasePath;

        private readonly QuestionCollection _questions;

        internal QuestionCollectionWebConsole(string databasePath, QuestionCollection questions)
        {
            _databasePath = databasePath;
            _questions = questions;
        }

        protected override IInputDialogManager createDialogManager()
        {
            return new QuestionCollectionManager(_questions);
        }
    }
}
