using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.Database
{
    public class PresidentLayer : ExplicitLayer
    {
        public static readonly string PresidentNode = "president";

        public static readonly string StateNode = "state";

        public static readonly string ReignsRelation = "reigns in";

        public static readonly string IsMarriedRelation = "is married";

        public static readonly string HasChildRelation = "has child";

        private string _lastPresidentName;

        public PresidentLayer()
        {
            President("Barack Obama")
                .ReignsIn("USA")
                .Wife("Michelle Obama")
                .Children("Malia Obama", "Sasha Obama");

            President("Miloš Zeman")
                .ReignsIn("CZ")
                .Wife("Ivana Zemanová")
                .Children("Kateřina Zemanová");

            President("Andrej Kiska")
                .Wife("Martina Kisková")
                .ReignsIn("SK");

            President("Joachim Gauck")
                .Wife("Gerhild Radtke")
                .ReignsIn("D");

        }

        private PresidentLayer President(string name)
        {
            _lastPresidentName = name;

            AddEdge(N(name), ComposedGraph.IsRelation, N(PresidentNode));

            return this;
        }

        private PresidentLayer ReignsIn(string state)
        {
            AddEdge(N(_lastPresidentName), ReignsRelation, N(state));
            AddEdge(N(state), ComposedGraph.IsRelation, N(StateNode));

            return this;
        }

        internal PresidentLayer Wife(string wifeName)
        {
            AddEdge(N(wifeName), IsMarriedRelation, N(_lastPresidentName));
            AddEdge(N(_lastPresidentName), IsMarriedRelation, N(wifeName));
            return this;
        }

        internal PresidentLayer Children(params string[] names)
        {
            foreach (var name in names)
            {
                AddEdge(N(_lastPresidentName), HasChildRelation, N(name));
            }
            return this;
        }

        private NodeReference N(string name)
        {
            return GraphLayerBase.CreateReference(name.Replace(" ", "_"));
        }

    }
}
