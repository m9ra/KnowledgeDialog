using System;
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
            var path = new StringBuilder();
            path.Append("[");

            if (rule == null)
            {
                path.Append('*');
            }
            else
            {
                path.Append('#');

                foreach (var tuple in rule.Path)
                {
                    var isOut = tuple.Item2;

                    var prefix = isOut ? "--" : "<--";
                    var suffix = isOut ? "-->" : "--";

                    path.Append(prefix);
                    path.Append(tuple.Item1);
                    path.Append(suffix);
                }

                path.Append(rule.EndNode.Data);
            }

            path.Append("]");
            PrintLine(path.ToString(), NodeColor);
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
