using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KnowledgeDialog.Dialog.Parsing;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class PopularityMaximizationLinker : ILinker
    {
        private readonly UtteranceLinker _linker;

        internal int Nbest = 1;

        internal PopularityMaximizationLinker(FreebaseDbProvider db, string verbsLexicon = null)
        {
            _linker = new UtteranceLinker(db, verbsLexicon);
        }

        public LinkedUtterance LinkUtterance(string utterance, IEnumerable<EntityInfo> context = null)
        {
            return _linker.LinkUtterance(utterance, Nbest).FirstOrDefault();
        }
    }
}
