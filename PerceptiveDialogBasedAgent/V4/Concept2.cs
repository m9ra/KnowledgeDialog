using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Concept2
    {
        public readonly string Name;

        internal readonly BodyAction2 Action;

        private readonly List<string> _descriptions = new List<string>();

        internal IEnumerable<string> Descriptions => _descriptions;

        public Concept2(string name, BodyAction2 action)
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
