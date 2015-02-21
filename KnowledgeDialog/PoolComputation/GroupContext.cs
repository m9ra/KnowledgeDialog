using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation
{
    class GroupContext
    {
        public readonly ActionGroup Group;

        public readonly int Offset;

        internal GroupContext(ActionGroup group, int offset)
        {
            Group = group;
            Offset = offset;
        }

        public override bool Equals(object obj)
        {
            var o = obj as GroupContext;
            if (o == null)
                return false;

            return Group == o.Group && Offset == o.Offset;
        }

        public override int GetHashCode()
        {
            return Group.GetHashCode() + Offset;
        }
    }
}
