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

        public Statistics(string experimentRoot)
        {
            var experimentId = Path.GetFileNameWithoutExtension(experimentRoot);
            LogFiles = LogFile.Load(Path.Combine(experimentRoot, ExperimentBase.RelativeUserPath), experimentId);

            foreach (var file in LogFiles)
            {                
                var currentDialogTurnCount = 0;
                foreach (var action in file.LoadActions())
                {
                    if (action.Type == "T_reset")
                    {
                        if (currentDialogTurnCount > 0)
                            //skip empty dialogs
                            ++DialogCount;

                        currentDialogTurnCount = 0;
                    }
                    else if (action.Type == "T_utterance")
                    {
                        ++TurnCount;
                        ++currentDialogTurnCount;
                    }
                }

                if(currentDialogTurnCount>0)
                    ++DialogCount;
            }

            var advice = new LogFile(Path.Combine(experimentRoot, experimentId + ".dialog"));
            foreach (var action in advice.LoadActions())
            {
                if (action.Type == "T_advice")
                {
                    ++QuestionAnswerPairs;
                }
                else if (action.Type == "T_equivalence")
                {
                    if ((bool)action.Data["isEquivalent"])
                        ++PositiveEquivalencies;
                    else
                        ++NegativeEquivalencies;
                }
            }
        }
    }
}
