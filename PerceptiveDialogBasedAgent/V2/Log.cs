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
            Writeln("POLICY: " + policyCommand, PolicyColor);
        }

        internal static void QueryPush(SemanticItem item)
        {
            if (!PrintDatabaseInfo)
                return;

            Writeln("QUERY: " + item.ToString(), HeadlineColor);
            _currentIndentation += 1;
        }

        internal static void QueryPop(IEnumerable<SemanticItem> result)
        {
            if (!PrintDatabaseInfo)
                return;

            foreach (var item in result)
            {
                Writeln(item.ToString(), ItemColor);
            }

            _currentIndentation -= 1;
            Writeln("RESULT: " + result.Count(), HeadlineColor);
        }

        internal static void Indent()
        {
            _currentIndentation += 1;
        }

        internal static void Dedent()
        {
            _currentIndentation -= 1;
        }

        internal static void DialogUtterance(string utterance)
        {
            Writeln("", InfoColor);
            rawWrite(utterance + "\n", UtteranceColor);
        }

        internal static void EventHandler(string eventDescription)
        {
            Writeln("\tEVENT: " + eventDescription, SensorColor);
        }

        internal static void NewFact(SemanticItem newFact)
        {
            Writeln("\tNEW FACT: " + newFact, SensorColor);
        }

        internal static void Questions(IEnumerable<SemanticItem> questions)
        {
            if (!questions.Any())
                return;

            Writeln("\tDATABASE QUESTIONS", HeadlineColor);

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

                Writeln("\t\t{0} x{1} Id: {2}", ItemColor, questionStr, count, question.Id);
            }
        }

        internal static void Result(IEnumerable<SemanticItem> result)
        {
            Writeln("\nRESULT", HeadlineColor);
            foreach (var fact in result)
            {
                var factStr = fact.ReadableRepresentation();
                Writeln("\t{0} Id: {1}", ItemColor, factStr, fact.Id);
            }
        }

        internal static void Dump(Database database)
        {
            Writeln("\nDATABASE DUMP", HeadlineColor);
            Writeln("\tFACTS", HeadlineColor);

            var facts = database.GetData();
            foreach (var fact in facts)
            {
                var factStr = fact.ReadableRepresentation();
                Writeln("\t\t{0} Id: {1}", ItemColor, factStr, fact.Id);
            }
        }

        internal static void SensorAdd(string condition, string action)
        {
            Writeln("\tSENSOR: {0}", SensorColor, condition);
            Writeln("\t\t{0}", ActionColor, action);
        }

        internal static void Writeln(string format = "", ConsoleColor color = ConsoleColor.Gray, params object[] formatArgs)
        {
            Write(format + "\n", color, formatArgs);
        }

        internal static void Write(string format, ConsoleColor color = ConsoleColor.Gray, params object[] formatArgs)
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
