using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Responses
{
    class MultiResponse : ResponseBase
    {
        public readonly IEnumerable<ResponseBase> Responses;

        public MultiResponse(IEnumerable<ResponseBase> responses)
        {
            Responses = responses.ToArray();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = string.Join(" ",Responses);
            return text;
        }
    }
}
