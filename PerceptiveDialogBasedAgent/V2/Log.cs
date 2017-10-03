using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class Log
    {
        private static readonly bool _enableLogging = true;

        internal static readonly ConsoleColor PolicyColor = ConsoleColor.Cyan;

        internal static readonly ConsoleColor UtteranceColor = ConsoleColor.White;

        internal static readonly ConsoleColor ExecutedCommandColor = ConsoleColor.Red;

        internal static readonly ConsoleColor EvaluatedCommadColor = ConsoleColor.DarkRed;

        internal static readonly ConsoleColor HeadlineColor = ConsoleColor.Green;

        internal static readonly ConsoleColor ItemColor = ConsoleColor.DarkGreen;

        internal static readonly ConsoleColor InfoColor = ConsoleColor.DarkBlue;

        internal static readonly ConsoleColor SensorColor = ConsoleColor.Yellow;

        internal static readonly ConsoleColor ActionColor = ConsoleColor.DarkYellow;

        internal static void Policy(string policyCommand)
        {
            writeln("POLICY: " + policyCommand, PolicyColor);
        }

        internal static void DialogUtterance(string utterance)
        {
            writeln("", InfoColor);
            forceWrite(utterance + "\n", UtteranceColor);
        }

        internal static void Questions(IEnumerable<SemanticItem> questions)
        {
            if (!questions.Any())
                return;

            writeln("\tDATABASE QUESTIONS", HeadlineColor);
            foreach (var question in questions)
            {
                writeln("\t\t{0}", ItemColor, question);
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

            forceWrite(format, color, formatArgs);
        }

        private static void forceWrite(string format, ConsoleColor color, params object[] formatArgs)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(format, formatArgs);
            Console.ForegroundColor = previousColor;
        }

    }
}
