using PerceptiveDialogBasedAgent.V1.Interpretation;
using PerceptiveDialogBasedAgent.V1.SemanticRepresentation;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V1
{
    static class Log
    {
        static private readonly bool _enableLogging = true;

        static internal readonly ConsoleColor PolicyColor = ConsoleColor.Cyan;

        static internal readonly ConsoleColor UtteranceColor = ConsoleColor.White;

        static internal readonly ConsoleColor ExecutedCommandColor = ConsoleColor.Red;

        static internal readonly ConsoleColor EvaluatedCommadColor = ConsoleColor.DarkRed;

        static internal readonly ConsoleColor HeadlineColor = ConsoleColor.Green;

        static internal readonly ConsoleColor ItemColor = ConsoleColor.DarkGreen;

        static internal readonly ConsoleColor InfoColor = ConsoleColor.DarkBlue;

        static internal readonly ConsoleColor SensorColor = ConsoleColor.Yellow;

        static internal readonly ConsoleColor ActionColor = ConsoleColor.DarkYellow;

        static internal void Policy(string policyCommand)
        {
            writeln("POLICY: " + policyCommand, PolicyColor);
        }

        static internal void DialogUtterance(string utterance)
        {
            writeln("", InfoColor);
            forceWrite(utterance + "\n", UtteranceColor);
        }

        static internal void Execution(string command, DbConstraint evaluatedCommand)
        {
            writeln("\tEXECUTING: " + command, ExecutedCommandColor);
            writeln("\t\tas: " + prettyCall(evaluatedCommand), EvaluatedCommadColor);
        }

        static internal void List(string name, IEnumerable<object> items)
        {
            writeln("\t{0}", HeadlineColor, name);
            foreach (var item in items)
            {
                writeln("\t\t{0}", ItemColor, item);
            }
        }

        internal static void AddingSensorAction(string sensorTrigger, string action)
        {
            writeln("\tSENSOR: {0}", SensorColor, sensorTrigger);
            writeln("\t\t{0}", ActionColor, action);
        }

        static private string prettyCall(DbConstraint constraint)
        {
            var callName = constraint.PhraseConstraint;
            if (!callName.StartsWith("%"))
                return callName;

            var parameters = new List<string>();
            foreach (var parameter in constraint.SubjectConstraints)
            {
                var value = parameter.Answer.PhraseConstraint;
                if (value == null)
                    value = pretty(parameter.Answer);

                if (value.StartsWith("%"))
                    parameters.Add(prettyCall(parameter.Answer));
                else
                    parameters.Add(value);
            }

            callName = callName.Split(new[] { "_$" }, StringSplitOptions.None)[0];

            return string.Format("{0}({1})", callName, string.Join(", ", parameters));
        }

        static private string pretty(DbConstraint constraint)
        {
            var result = constraint.ToString();

            result = result.Replace("[C]([C]", "[C](");

            return result;
        }

        static private void writeln(string format, ConsoleColor color, params object[] formatArgs)
        {
            write(format + "\n", color, formatArgs);
        }

        static private void write(string format, ConsoleColor color, params object[] formatArgs)
        {
            if (!_enableLogging)
                return;

            forceWrite(format, color, formatArgs);
        }

        static private void forceWrite(string format, ConsoleColor color, params object[] formatArgs)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(format, formatArgs);
            Console.ForegroundColor = previousColor;
        }

        internal static void States(BeamGenerator generator)
        {
            var rankedStates = generator.GetRankedNodes().Reverse().ToArray();

            foreach (var state in rankedStates)
            {
                write($"S: {state.Rank:0.00} > ", Log.HeadlineColor);
                State(state.Value);
                writeln("\n", Log.HeadlineColor);

            }
        }

        internal static void State(BeamNode node)
        {
            var events = new List<EventBase>();
            var currentNode = node;

            while (currentNode != null && currentNode.Evt != null)
            {
                events.Add(currentNode.Evt);
                currentNode = currentNode.ParentNode;
            }

            events.Reverse();
            foreach (var evt in events)
            {
                Log.write(evt.ToString(), Log.ItemColor);
            }
        }
    }
}
