using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.DataCollection;

namespace WebBackend.DialogProvider
{
    class DataCollectionWebConsole : WebConsoleBase
    {
        private readonly string _databasePath;

        internal DataCollectionWebConsole(string databasePath)
        {
            _databasePath = databasePath;
        }

        protected override IInputDialogManager createDialoggManager()
        {
            return new CollectionManager(Program.Graph);
        }
    }
}
