using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend
{
    class PresidentInfo
    {
        private readonly List<string> _children = new List<string>();

        public string Name { get; private set; }

        public string WifeName { get; private set; }

        public string State { get; private set; }

        public IEnumerable<string> Children { get { return _children; } }

        internal PresidentInfo(string name)
        {
            Name = name;
        }

        internal static PresidentInfo Create(string name)
        {
            return new PresidentInfo(name);
        }

        internal PresidentInfo Wife(string wifeName)
        {
            this.WifeName = wifeName;
            return this;
        }

        internal PresidentInfo SetState(string state)
        {
            this.State = state;
            return this;
        }

        internal PresidentInfo Child(string childName)
        {
            _children.Add(childName);
            return this;
        }
    }
}
