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


using KnowledgeDialog.Dialog.Responses;

namespace WebBackend.DialogProvider
{
    class HeuristicManagerWebConsole : WebConsoleBase
    {
        /// <summary>
        /// Mapping of QA modules according to their storages.
        /// </summary>
        private static readonly Dictionary<string, HeuristicQAModule> _questionAnsweringModules = new Dictionary<string, HeuristicQAModule>();


        private readonly string _storageFullpath;

        internal HeuristicManagerWebConsole(string storageFullpath)
        {
            if (storageFullpath == "")
                storageFullpath = null;

            _storageFullpath = storageFullpath;
        }

        protected override IInputDialogManager createDialoggManager()
        {
            return createManager(_storageFullpath);
        }

        private static StateDialogManager createManager(string storageFullPath)
        {
            lock (_L_qa_index)
            {
                HeuristicQAModule qa;
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

        private static HeuristicQAModule createQAModule(string storageFullPath)
        {
            var qa = new HeuristicQAModule(Program.Graph, new CallStorage(storageFullPath));
            return qa;
        }
    }
}
