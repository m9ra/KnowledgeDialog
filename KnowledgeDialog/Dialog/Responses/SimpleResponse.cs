using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Responses
{
    class SimpleResponse : ResponseBase
    {
        public readonly string ResponseText;

        public SimpleResponse(string responseText)
        {
            ResponseText = responseText;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ResponseText;

        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var o = obj as SimpleResponse;
            if (o == null)
                return false;
            return o.ResponseText == ResponseText;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ResponseText.GetHashCode();
        }
    }
}
