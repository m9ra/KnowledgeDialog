using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

namespace KnowledgeDialog.Database
{
    public class FlatPresidentLayer : ExplicitLayer
    {
        private string _lastPresidentName;

        public readonly static string ExPresidentNode = "expresident";

        public FlatPresidentLayer()
        {
            President("Barack Obama")
                .ReignsIn("USA")
                .Wife("Michelle Obama")
                .Children("Malia Obama", "Sasha Obama");

            President("George Bush")
                .ReignsIn("USA")
                .Wife("Laura Bush")
                .IsExPresident();

            President("Miloš Zeman")
                .ReignsIn("CZ")
                .Wife("Ivana Zemanová")
                .Children("Kateřina Zemanová");

            President("Václav Klaus")
                .ReignsIn("CZ")
                .Wife("Livia Klausová")
                .IsExPresident();
            

            President("Andrej Kiska")
                .Wife("Martina Kisková")
                .ReignsIn("SK");

            President("Joachim Gauck")
                .Wife("Gerhild Radtke")
                .ReignsIn("D");
        }

        private FlatPresidentLayer IsExPresident()
        {
            AddEdge(N(_lastPresidentName), ComposedGraph.IsRelation, N(ExPresidentNode));

            return this;
        }

        private FlatPresidentLayer President(string name)
        {
            _lastPresidentName = name;

            AddEdge(N(name), ComposedGraph.IsRelation, N(PresidentLayer.PresidentNode));

            return this;
        }

        private FlatPresidentLayer ReignsIn(string state)
        {
            AddEdge(N(_lastPresidentName), PresidentLayer.ReignsRelation, N(state));
            AddEdge(N(state), ComposedGraph.IsRelation, N(PresidentLayer.StateNode));

            return this;
        }

        internal FlatPresidentLayer Wife(string wifeName)
        {
            AddEdge(N(wifeName), PresidentLayer.IsMarriedRelation, N(_lastPresidentName));
            AddEdge(N(_lastPresidentName), PresidentLayer.IsMarriedRelation, N(wifeName));
            return this;
        }

        internal FlatPresidentLayer Children(params string[] names)
        {
            foreach (var name in names)
            {
                AddEdge(N(_lastPresidentName), PresidentLayer.HasChildRelation, N(name));
            }
            return this;
        }

        private NodeReference N(string name)
        {
            SentenceParser.RegisterEntity(name);
            return GraphLayerBase.CreateReference(name);
        }

    }
}
