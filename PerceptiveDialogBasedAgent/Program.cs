﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            V4.Experiments.RefinementRestaurantSearch();

            Console.WriteLine();
            Console.WriteLine("Press any key to leave");
            Console.ReadKey();
        }
    }
}
