using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent
{
    static class Experiments
    {
        internal static void DbTests()
        {
            var mind = new MindSet();

            mind.AddFact("dog", "how @ is defined?", "animal with four legs");
            mind.AddFact("dog", "what does @ eat?", "meat");

            var constraint = new DbConstraint(
                ConstraintEntry.AnswerWhere("dog", "how @ is defined?")
                );

            foreach (var entity in mind.Database.Query(constraint))
            {
                Console.WriteLine(entity.Substitution);
            }
        }

        internal static void ParsingTests()
        {
            var mind = new MindSet();
            mind
             .AddPattern("a", "$something")
                 .Semantic(c => c["something"])

             .AddPattern("what", "is", "$what")
                 .Semantic(c => c.AnswerWhere(c["something"], "how @ is defined?"))

             .AddPattern("$something1", "and", "$something2")
                 .Semantic(c => c["something1"].Join(c["something2"]))
             ;

            var matches = mind.Matcher.Match("what is a pilot");
            foreach (var match in matches)
            {
                Console.WriteLine(match);
            }
        }

        internal static void AdviceTests()
        {
            var mind = new MindSet();
            mind
                .AddPattern("$something", "can", "have", "$something2")
                    .Semantic(c =>
                        c["something"].ExtendByAnswer("what does @ have?", c["something2"])
                    )

                .AddPattern("every", "$something")
                    .Semantic(c =>
                        DbConstraint.Entity(null).ExtendByAnswer("what @ is?", c["something"])
                    )

                 .AddPattern("blue", "$something")

                ;

            mind.AddFact("dog", "what color @ has?", "black");
            mind.AddFact("whale", "what color @ has?", "blue");
            mind.AddFact("whale", "where @ lives?", "sea");
            mind.AddFact("dog", "what @ is?", "mammal");
            mind.AddFact("whale", "what @ is?", "mammal");

            var evaluation = mind.Evaluator.Evaluate("every blue mammal");
            wl(evaluation.ToString());

            foreach (var value in mind.Database.Query(evaluation.Constraint))
            {
                Console.WriteLine("Result: " + value.Substitution);
            }

            //what is sky?
            //the stuff above
            //what is a blue color?
            //its an example of a color

            wl("S: What is The Great Britain?");
            wl("U: It is a country.");

            wl("S: What is a country?");
            wl("U: Its a location with a culture, people and so on.");
            wl("U: USA is a country.");
            wl("U: Country is a word.");
            wl("....");
            //writes 
            //  - how is defined? A location
            //  - which extra props. it has? culture
            //  - which extra props. it has? people

            wl("S: What culture a country has?");
            wl("U: It depends on a concrete country.");

            wl("S: So do you know some country?");
            wl("U: Sure, e.g. germany with their strict culture.");
        }

        internal static void ThingsToThinkAbout()
        {
            //intention
            Console.WriteLine("What is a pilot?");
            Console.WriteLine("It is a man which drives aeroplanes");

            Console.WriteLine("Which words that you know ends with a letter 'a' ?");

            Console.WriteLine("All mens you know.");
            Console.WriteLine("What about them?");
            Console.WriteLine("Find their surnames.");
            Console.WriteLine("I did.");
            Console.WriteLine("List them all here.");
        }

        private static void wl(string text)
        {
            Console.WriteLine(text);
        }
    }
}
