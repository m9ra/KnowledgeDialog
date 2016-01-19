using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleQuestions.FreeBaseReaders;

namespace SimpleQuestions
{
    class Program
    {
        static void Main(string[] args)
        {
            Statistics_Experiment(args[0], args[1]);
        }

        static void Statistics_Experiment(string freebaseFile, string questionFile)
        {
            var questionReader = new QuestionFileReader(questionFile);
            var questionNodes = new HashSet<FreeBaseNode>();
            var questionEdges = new HashSet<FreeBaseEdge>();
            foreach (var question in questionReader.GetEntries())
            {
                questionNodes.Add(question.SupportFact);
                questionNodes.Add(question.Answer);
                questionEdges.Add(question.Edge);
            }

            Console.WriteLine("Node count {0}", questionNodes.Count);
            Console.WriteLine("Edge count {0}", questionEdges.Count);

            var tripletSelector = new TripletSelector(freebaseFile);
            tripletSelector.AddNodesToSelect(questionNodes);
            tripletSelector.AddEdgesToSelect(questionEdges);

            tripletSelector.ProgressReporter += count => Console.WriteLine("\ttriplets processed: {0}\t{1}/{2}", count, tripletSelector.SelectedNodeCount, tripletSelector.SelectedEdgeCount);
            tripletSelector.Iterate();

            Console.WriteLine("Selected node count {0}", tripletSelector.SelectedNodeCount);
            Console.WriteLine("Selected edge count {0}", tripletSelector.SelectedEdgeCount);

            Console.ReadKey();

        }
    }
}
