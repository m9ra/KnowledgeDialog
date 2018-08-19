using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebBackend.Experiment
{
    public class Statistics
    {
        public IEnumerable<LogFile> LogFiles { get; private set; }

        public int TurnCount { get; private set; }

        public int DialogCount { get; private set; }

        public int QuestionAnswerPairs { get; private set; }

        public int PositiveEquivalencies { get; private set; }

        public int NegativeEquivalencies { get; private set; }

        public int NegativeOneCodes { get; private set; }

        public int PositiveOneCodes { get; private set; }

        public int PositiveTwoCodes { get; private set; }

        public Statistics(string experimentRoot)
        {
            var experimentId = Path.GetFileNameWithoutExtension(experimentRoot);
            LogFiles = LogFile.Load(Path.Combine(experimentRoot, ExperimentBase.RelativeUserPath));

            foreach (var file in LogFiles)
            {
                foreach (var dialogue in file.ParseDialogs())
                {
                    ++DialogCount;
                    TurnCount += dialogue.TurnCount;

                    if (dialogue.SuccessCode == 1)
                        PositiveOneCodes += 1;

                    if (dialogue.SuccessCode == 2)
                        PositiveTwoCodes += 1;

                    if (dialogue.SuccessCode == -1)
                        NegativeOneCodes += 1;
                }
            }
        }
    }
}
