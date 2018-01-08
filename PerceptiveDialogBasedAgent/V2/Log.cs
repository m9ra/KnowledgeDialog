using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class Log
    {

        internal static readonly bool PrintDatabaseInfo = false;

        internal static readonly ConsoleColor PolicyColor = ConsoleColor.Cyan;

        internal static readonly ConsoleColor UtteranceColor = ConsoleColor.White;

        internal static readonly ConsoleColor ExecutedCommandColor = ConsoleColor.Red;

        internal static readonly ConsoleColor EvaluatedCommadColor = ConsoleColor.DarkRed;

        internal static readonly ConsoleColor HeadlineColor = ConsoleColor.Green;

        internal static readonly ConsoleColor ItemColor = ConsoleColor.DarkGreen;

        internal static readonly ConsoleColor InfoColor = ConsoleColor.DarkBlue;

        internal static readonly ConsoleColor SensorColor = ConsoleColor.Yellow;

        internal static readonly ConsoleColor ActionColor = ConsoleColor.DarkYellow;

        private static int _currentIndentation = 0;

        private static readonly bool _enableLogging = true;



        internal static void Policy(string policyCommand)
        {
            writeln("POLICY: " + policyCommand, PolicyColor);
        }

        internal static void QueryPush(SemanticItem item)
        {
            if (!PrintDatabaseInfo)
                return;

            writeln("QUERY: " + item.ToString(), HeadlineColor);
            _currentIndentation += 1;
        }

        internal static void QueryPop(IEnumerable<SemanticItem> result)
        {
            if (!PrintDatabaseInfo)
                return;

            foreach (var item in result)
            {
                writeln(item.ToString(), ItemColor);
            }

            _currentIndentation -= 1;
            writeln("RESULT: " + result.Count(), HeadlineColor);
        }

        internal static void DialogUtterance(string utterance)
        {
            writeln("", InfoColor);
            rawWrite(utterance + "\n", UtteranceColor);
        }

        internal static void EventHandler(string eventDescription)
        {
            writeln("\tEVENT: " + eventDescription, SensorColor);
        }

        internal static void NewFact(SemanticItem newFact)
        {
            writeln("\tNEW FACT: " + newFact, SensorColor);
        }

        internal static void Questions(IEnumerable<SemanticItem> questions)
        {
            if (!questions.Any())
                return;

            writeln("\tDATABASE QUESTIONS", HeadlineColor);

            var questionCounts = new Dictionary<string, int>();
            foreach (var question in questions)
            {
                var questionStr = question.ReadableRepresentation();
                questionCounts.TryGetValue(questionStr, out var count);
                questionCounts[questionStr] = count + 1;
            }

            //ensure each question is printed only once with count information
            foreach (var question in questions)
            {
                var questionStr = question.ReadableRepresentation();
                if (!questionCounts.ContainsKey(questionStr))
                    continue;

                var count = questionCounts[questionStr];
                questionCounts.Remove(questionStr);

                writeln("\t\t{0} x{1} Id: {2}", ItemColor, questionStr, count, question.Id);
            }
        }

        internal static void Result(IEnumerable<SemanticItem> result)
        {
            writeln("\nRESULT", HeadlineColor);
            foreach (var fact in result)
            {
                var factStr = fact.ReadableRepresentation();
                writeln("\t{0} Id: {1}", ItemColor, factStr, fact.Id);
            }
        }

        internal static void Dump(Database database)
        {
            writeln("\nDATABASE DUMP", HeadlineColor);
            writeln("\tFACTS", HeadlineColor);

            var facts = database.GetData();
            foreach (var fact in facts)
            {
                var factStr = fact.ReadableRepresentation();
                writeln("\t\t{0} Id: {1}", ItemColor, factStr, fact.Id);
            }
        }

        internal static void SensorAdd(string condition, string action)
        {
            writeln("\tSENSOR: {0}", SensorColor, condition);
            writeln("\t\t{0}", ActionColor, action);
        }

        private static void writeln(string format, ConsoleColor color, params object[] formatArgs)
        {
            write(format + "\n", color, formatArgs);
        }

        private static void write(string format, ConsoleColor color, params object[] formatArgs)
        {
            if (!_enableLogging)
                return;

            var prefix = "".PadLeft(_currentIndentation * 4, ' ');

            rawWrite(prefix + format, color, formatArgs);
        }

        private static void rawWrite(string format, ConsoleColor color, params object[] formatArgs)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(format, formatArgs);
            Console.ForegroundColor = previousColor;
        }
    }
}
