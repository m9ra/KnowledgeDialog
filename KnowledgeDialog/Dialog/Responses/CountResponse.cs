using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.Dialog.Responses
{
    class CountResponse : ResponseBase
    {
        public readonly int Count;

        public CountResponse(int count)
        {
            Count = count;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Count.ToString();

        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var o = obj as CountResponse;
            if (o == null)
                return false;
            return o.Count == Count;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Count.GetHashCode();
        }
    }
}
