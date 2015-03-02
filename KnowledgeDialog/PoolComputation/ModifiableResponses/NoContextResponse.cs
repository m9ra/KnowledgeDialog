using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.PoolComputation.ModifiableResponses
{
    class NoContextResponse : ModifiableResponse
    {
        private readonly ResponseStorage _storage;

        public NoContextResponse(ResponseStorage storage)
        {
            _storage = storage;
        }

        public override ResponseBase CreateResponse()
        {
            return new SimpleResponse(_storage.Utterance);
        }

        public override bool Modify(string modification)
        {
            _storage.Utterance = modification;
            return true;
        }
    }

    class ResponseStorage {
        
        internal string Utterance;

        internal ResponseStorage(string initialUtterance)
        {
            Utterance = initialUtterance;
        }
    }
}
