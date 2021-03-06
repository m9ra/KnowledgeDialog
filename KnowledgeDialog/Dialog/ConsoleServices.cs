﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog.Responses;

namespace KnowledgeDialog.Dialog
{
    static class ConsoleServices
    {
        internal static readonly ConsoleColor SectionBoundaries = ConsoleColor.Cyan;

        internal static readonly ConsoleColor PromptColor = ConsoleColor.Gray;

        internal static readonly ConsoleColor ActiveColor = ConsoleColor.White;

        internal static readonly ConsoleColor NodeColor = ConsoleColor.Yellow;

        internal static readonly ConsoleColor OperatorColor = ConsoleColor.Red;

        internal static readonly ConsoleColor CaptionColor = ConsoleColor.Green;

        internal static readonly ConsoleColor InfoColor = ConsoleColor.Gray;

        internal static readonly ConsoleColor ConfidenceColor = ConsoleColor.DarkCyan;

        internal static readonly ConsoleColor PatternHead = ConsoleColor.DarkYellow;

        internal static readonly Stack<string> _sectionNames = new Stack<string>();

        private static bool _needIndent = true;

        private static int _indentationLevel = 0;


        /// <summary>
        /// Print formatted hypothesis.
        /// </summary>
        /// <param name="hypothesis"></param>
        internal static void Print(Tuple<ResponseBase, double> hypothesis)
        {
            Print(hypothesis.Item1.ToString(), NodeColor);
            Print(": ", OperatorColor);
            PrintLine(hypothesis.Item2, ConfidenceColor);
        }


        internal static void Print(IEnumerable<Edge> path)
        {
            var buffer = new StringBuilder();
            FillWithPath(buffer, path);

            PrintLine(buffer.ToString(), NodeColor);
        }

        /// <summary>
        /// Begin new section with given name.
        /// </summary>
        /// <param name="sectionName">Name of section</param>
        internal static void BeginSection(string sectionName)
        {
            PrintLine("".PadLeft(4, '=') + sectionName + "".PadLeft(4, '='), SectionBoundaries);
            ++_indentationLevel;

            _sectionNames.Push(sectionName);
        }

        /// <summary>
        /// End previous section.
        /// </summary>
        internal static void EndSection()
        {
            --_indentationLevel;
            var borderLength = _sectionNames.Pop().Length + 8;
            PrintLine("".PadLeft(borderLength, '='), SectionBoundaries);
        }

        internal static void PrintOutput(ResponseBase response)
        {
            Print("response< ", PromptColor);
            PrintLine(response.ToString() + "\n", ActiveColor);
        }

        internal static void Print(KnowledgePath path)
        {
            PrintLine(path.ToString(), NodeColor);
        }

        internal static void Print(KnowledgeRule rule)
        {
            var pathBuffer = new StringBuilder();
            pathBuffer.Append("[");

            if (rule == null)
            {
                pathBuffer.Append('*');
            }
            else
            {
                pathBuffer.Append('#');

                var path = rule.Path;
                FillWithPath(pathBuffer, path);

                pathBuffer.Append(rule.EndNode.Data);
            }

            pathBuffer.Append("]");
            PrintLine(pathBuffer.ToString(), NodeColor);
        }

        internal static void FillWithPath(StringBuilder pathBuffer, IEnumerable<Edge> path)
        {
            foreach (var edge in path)
            {
                var isOut = edge.IsOutcoming;

                var prefix = isOut ? "--" : "<--";
                var suffix = isOut ? "-->" : "--";

                pathBuffer.Append(prefix);
                pathBuffer.Append(edge.Name);
                pathBuffer.Append(suffix);
            }
        }

        internal static void PrintPrompt()
        {
            Print("utterance> ", PromptColor);
        }

        internal static void PrintLine(object data, ConsoleColor color)
        {
            Print(data + Environment.NewLine, color);
            _needIndent = true;
        }

        internal static void Print(object data, ConsoleColor color)
        {
            var lastColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            if (_needIndent)
            {
                _needIndent = false;
                Console.Write("".PadLeft(_indentationLevel * 2) + data);
            }
            else
            {
                Console.Write(data);
            }
            Console.ForegroundColor = lastColor;
        }

        internal static void Print(string caption, IEnumerable<PoolComputation.MappedQA.PoolRules.PoolRuleBase> rules)
        {
            PrintLine(caption, ActiveColor);
            Indent(1);
            PrintLine(string.Join(" | ", rules), NodeColor);
            Indent(-1);
        }

        internal static void Print(Ranked<RuleQuestions.StructuredInterpretation> interpretation)
        {
            Print("STRUCTURED INTERPRETATION", CaptionColor);
            PrintLine(" rank: " + interpretation.Rank, ActiveColor);
            PrintLine(interpretation.Value.FeatureKey, ActiveColor);
            Indent(1);

            PrintLine("GENERAL: ", OperatorColor);
            Indent(1);
            foreach (var constraint in interpretation.Value.GeneralConstraints)
                PrintLine(constraint, NodeColor);
            Indent(-1);

            PrintLine("DISAMBIGUATION: ", OperatorColor);
            Indent(1);
            foreach (var constraint in interpretation.Value.DisambiguationConstraints)
                PrintLine(constraint, NodeColor);
            Indent(-1);

            Indent(-1);
        }

        internal static void Print(IEnumerable<NodeReference> nodes)
        {
            PrintLine(string.Join(", ", nodes), InfoColor);
        }

        internal static void Print(string caption, IEnumerable<PoolComputation.MappedQA.PoolRules.PoolRuleBase> rules, HashSet<NodeReference> nodes)
        {
            PrintLine(caption, CaptionColor);
            Indent(1);
            PrintLine(string.Join(" | ", rules), NodeColor);
            PrintLine(string.Join(", ", nodes), InfoColor);
            Indent(-1);
        }

        internal static void PrintEmptyLine()
        {
            Console.WriteLine();
        }

        internal static string ReadLine(ConsoleColor color)
        {
            var lastColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            var result = Console.ReadLine();
            Console.ForegroundColor = lastColor;

            return result;
        }

        internal static void Indent(int change)
        {
            _indentationLevel += change;
        }




    }
}
