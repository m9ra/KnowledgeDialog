using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.DataCollection;

using WebBackend.AnswerExtraction;
using KnowledgeDialog.GraphNavigation;

namespace WebBackend.DialogProvider
{
    class GraphNavigationWebConsole : WebConsoleBase
    {
        private readonly string _databasePath;

        private readonly IEnumerable<string> _phrases;

        private readonly NavigationData _data;

        private readonly ILinker _linker;

        internal GraphNavigationWebConsole(string databasePath, IEnumerable<string> phrases, NavigationData data, ILinker linker)
        {
            _databasePath = databasePath;
            _phrases = phrases;
            _data = data;
            _linker = linker;
        }

        /// <inheritdoc/>
        protected override IInputDialogManager createDialogManager()
        {
            return new GraphNavigationManager(_data, _phrases, _linker);
        }
    }
}
