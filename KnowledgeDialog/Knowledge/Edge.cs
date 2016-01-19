﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Knowledge
{
    public class Edge
    {
        public readonly string Name;

        public readonly bool IsOutcoming;

        private Edge(string name, bool isOutgoing)
        {
            Name = name;
            IsOutcoming = isOutgoing;
        }

        public static Edge Outcoming(string name)
        {
            return From(name, true);
        }

        public static Edge Incoming(string name)
        {
            return From(name, false);
        }

        public static Edge From(string name, bool isOutgoing)
        {
            return new Edge(name, isOutgoing);
        }

        internal Edge Inverse()
        {
            return Edge.From(Name, !IsOutcoming);
        }
    }
}