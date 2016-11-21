using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Dataset
{
    class FreebaseEntry
    {
        public readonly string Id;

        public readonly string Label;

        public readonly string Description;

        public readonly IEnumerable<string> Aliases;

        public readonly IEnumerable<Tuple<Edge, string>> Targets;

        internal FreebaseEntry(string id, string label, string description, IEnumerable<string> aliases, IEnumerable<Tuple<Edge, string>> targets)
        {
            Id = id;
            Label = label;
            Description = description;
            Aliases = aliases.ToArray();
            Targets = targets.ToArray();
        }
    }
}
