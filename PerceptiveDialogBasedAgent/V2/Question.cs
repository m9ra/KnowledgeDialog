using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    public static class Question
    {
        public readonly static string Entity = "what is it?";

        public static readonly string WhatItSpecifies = "what does $@ specify ?";

        public readonly static string WhatShouldAgentDoNow = "what should agent do now ?";

        public readonly static string HowToConvertItToNumber = "how to convert $@ to number ?";

        public readonly static string HowToDo = "how to do $@ ?";

        public readonly static string HowToSimplify = "how to simplify $@ ?";

        public readonly static string IsItTrue = "is $@ true ?";

        public readonly static string CanItBeAnswer = "can $@ be answer ?";

        public readonly static string HowToEvaluate = "what does $@ mean ?";

        public readonly static string HowToParaphrase= "how to paraphrase $@ ?";
    }
}
