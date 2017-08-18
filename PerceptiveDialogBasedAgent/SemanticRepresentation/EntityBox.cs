using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.SemanticRepresentation
{
    class EntityBox
    {
        private readonly Dictionary<EntityBox, EntityBox> _relations = new Dictionary<EntityBox, EntityBox>();
    }
}
