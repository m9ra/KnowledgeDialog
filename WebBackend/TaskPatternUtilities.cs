using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.TaskPatterns;

namespace WebBackend
{
    static class TaskPatternUtilities
    {
        public static readonly string CheckAndLearn = " If not, try to teach it to the system and ask it again.";
        public static string[] StateSubstitutions
        {
            get
            {
                if (Program.UseWikidata)
                    return new[]{
                        "United states of America",
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
    }
}
