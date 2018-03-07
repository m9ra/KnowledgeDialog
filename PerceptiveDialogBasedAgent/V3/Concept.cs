using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V3
{
    class Concept
    {
        public readonly string Name;

        internal readonly BodyAction Action;

        private readonly List<string> _descriptions = new List<string>();

        internal IEnumerable<string> Descriptions => _descriptions;

        public Concept(string name, BodyAction action)
        {
            Name = name;
            Action = action;
        }

        public void AddDescription(string description)
        {
            _descriptions.Add(description);
        }

        /// </inheritdoc>
        public override string ToString()
        {
            return "'" + Name + "' D: " + _descriptions.Count;
        }
    }
}
