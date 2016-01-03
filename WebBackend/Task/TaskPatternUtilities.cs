using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Task;

namespace WebBackend
{
    internal delegate string SubstitutionSelector(PresidentInfo info);

    internal delegate IEnumerable<string> CorrectAnswerMultiSelector(PresidentInfo info);

    internal delegate string CorrectAnswerSingleSelector(PresidentInfo info);

    static class TaskPatternUtilities
    {
        private static bool _enableCheckAndLearn = true;

        public static string CheckAndLearn { get { return _enableCheckAndLearn ? " If not, try to teach it to the system and <b>ask it again</b>." : ""; } }

        public static readonly string WikiLabelEdge = "en.label";

        public static readonly string WikiAliasEdge = "en.alias";

        public static readonly PresidentInfo[] Presidents = new[]
        {
            PresidentInfo.Create("Barack Obama")
                .SetState("United States of America")
                .Wife("Michelle Obama")
                .Child("Malia Obama")
                .Child("Sasha Obama")
            ,

            PresidentInfo.Create("François Hollande")
                .SetState("France")
                .Child("Julien Hollande")
                .Child("Flora Hollande")
                .Child("Thomas Hollande")
                .Child("Clémence Hollande")
            ,

            PresidentInfo.Create("Joachim Gauck")
                .SetState("Germany")
            ,

            PresidentInfo.Create("Vladimir Putin")
                .SetState("Russia")
            ,
        };


        public static void FillPresidentTask(TaskPatternBase target, SubstitutionSelector substitutionSelector, CorrectAnswerMultiSelector correctAnswerSelector)
        {
            if (!Program.UseWikidata)
                throw new NotImplementedException();

            foreach (var info in Presidents)
            {
                var substitution = substitutionSelector(info);
                var correctAnswer = correctAnswerSelector(info).Where(s => s != null).ToArray();

                if (substitution == null || correctAnswer.Length == 0)
                    //there is nothing to add
                    continue;

                correctAnswer = expandAliases(correctAnswer);

                target.AddTaskSubstitution(substitution, correctAnswer);
            }
        }

        public static void DisableCheckAndLearn()
        {
            _enableCheckAndLearn = false;
        }

        public static void FillPresidentTask(TaskPatternBase target, SubstitutionSelector substitutionSelector, CorrectAnswerSingleSelector correctAnswerSelector)
        {
            FillPresidentTask(target, substitutionSelector, president => new[] { correctAnswerSelector(president) });
        }

        public static string[] StateSubstitutions
        {
            get
            {
                if (Program.UseWikidata)
                    return new[]{
                        "United States of America",
                        "France",
                        "Germany",
                        "Russia"
                    };
                else
                    return new[]{
                    "United states of America",
                    "USA",
                    "D",
                    "CZ",
                    "SK",
                };
            }
        }

        public static string[] PresidentSubstitutions
        {
            get
            {
                if (Program.UseWikidata)
                    return new[]{
                        "Barack Obama",
                        "François Hollande",
                        "Joachim Gauck",
                        "Vladimir Putin"
                    };
                else
                    return new[]{
                        "Barack Obama",
                        "Miloš Zeman",
                        "Andrej Kiska",
                        "Joachim Gauck"
                    };
            }
        }

        public static IEnumerable<Tuple<string, bool>> ReignsInFromStatePath
        {
            get
            {
                if (Program.UseWikidata)
                    return new[]{
                            Tuple.Create("en.label",false),
                            Tuple.Create("P27",false),
                            Tuple.Create("en.label",true)
                        };
                else
                    return new[]{
                        Tuple.Create("reigns in",false)
                    };
            }
        }

        public static IEnumerable<Tuple<string, bool>> ReignsInFromPresidentPath
        {
            get
            {
                if (Program.UseWikidata)
                    return new[]{
                        Tuple.Create("en.label",false),
                        Tuple.Create("P27",true),
                        Tuple.Create("en.label",true)
                    };
                else
                    return new[]{
                        Tuple.Create("reigns in",true)
                    };
            }
        }

        public static IEnumerable<Tuple<string, bool>> WifeOfPresidentFromStatePath
        {
            get
            {
                if (Program.UseWikidata)
                    return new[]{
                        Tuple.Create("en.label",false),
                        Tuple.Create("P27",false),
                        Tuple.Create("P26",true),
                        Tuple.Create("en.label",true)
                    };
                else
                    return new[]{
                        Tuple.Create("reigns in",false),
                        Tuple.Create("is married",true)
                    };
            }
        }

        public static IEnumerable<Tuple<string, bool>> PresidentChildFromState
        {
            get
            {
                if (Program.UseWikidata)
                    return new[]{
                        Tuple.Create("en.label",false),
                        Tuple.Create("P27",false),
                        Tuple.Create("P40.main",true),
                        Tuple.Create("en.label",true)
                    };
                else
                    return new[]{
                        Tuple.Create("reigns in",false),
                        Tuple.Create("has child",true)
                    };
            }
        }

        private static string[] expandAliases(string[] nodeData)
        {
            var graph = Program.Graph;

            var result = new HashSet<string>();
            var aliasEdges = new[] { WikiLabelEdge, WikiAliasEdge };

            foreach (var data in nodeData)
            {
                var startingNodes = new[] { graph.GetNode(data) };

                foreach (var outgoingEdge in aliasEdges)
                {
                    foreach (var incommingEdge in aliasEdges)
                    {
                        var targetNodes = graph.GetForwardTargets(startingNodes, new[]{
                            Tuple.Create(outgoingEdge,false),
                            Tuple.Create(incommingEdge,true)
                        }).ToArray();

                        var targets = from node in targetNodes select node.Data;
                        result.UnionWith(targets);
                    }
                }
            }

            foreach (var node in nodeData)
            {
                if (!result.Contains(node))
                    throw new NotSupportedException("Non-reversible alias found " + node);
            }

            return result.ToArray();
        }
    }
}
