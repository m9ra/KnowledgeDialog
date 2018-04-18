using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    static class Configuration
    {
        public static readonly double ConceptActivationThreshold = 0.2;

        public static readonly double ForwardingActivationThreshold = 0.2;

        public static readonly double ParameterSubstitutionScore = 0.1;

        public static readonly int StateBeamLimit = 200;

        public static readonly int MaxPhraseWordCount = 3;
    }
}
