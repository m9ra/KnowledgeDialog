using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V1.SemanticRepresentation
{
    class EntityBox
    {
        private readonly Dictionary<EntityBox, EntityBox> _relations = new Dictionary<EntityBox, EntityBox>();
    }
}
