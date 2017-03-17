using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    static class Statistics_Batch
    {
        internal static void CountReferences()
        {
            var counter = new DialogStatisticCounter(Program.Experiments, "answer_extraction");

            counter.PrintReferenceOccurence();

            Console.WriteLine("Press any key....");
            Console.ReadKey();
        }
    }
}
