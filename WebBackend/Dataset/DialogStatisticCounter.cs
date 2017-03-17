using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Experiment;
using KnowledgeDialog.DataCollection;

namespace WebBackend.Dataset
{
    class DialogStatisticCounter
    {
        private readonly List<ExperimentBase> _experiments = new List<ExperimentBase>();

        internal DialogStatisticCounter(ExperimentCollection collection, string experimentPrefix)
        {
            foreach (var experiment in collection.Experiments)
            {
                if (experiment.Id.StartsWith(experimentPrefix))
                    _experiments.Add(experiment);
            }

            _experiments.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        internal void PrintReferenceOccurence()
        {
            var referenceWords = new[] { "he", "she", "him", "it", "they", "them" };
            foreach (var experiment in _experiments)
            {
                foreach (var logFile in experiment.LoadLogFiles())
                {
                    var lastUtterance = "";
                    var lastWords = new string[0];
                    foreach (var action in logFile.LoadActions())
                    {
                        var normalizedText = action.Text.ToLowerInvariant();
                        var normalizedWords = normalizedText.Split(' ').ToArray();

                        switch (action.Type)
                        {
                            case "T_response":
                                break;
                            case "T_utterance":
                                var difference = lastWords.Except(normalizedWords).ToArray();
                                var refCanceledWords = difference.Except(referenceWords).ToArray();
                                if (refCanceledWords.Length == 0 && difference.Length > 0)
                                {
                                    //we found reference hint
                                    Console.WriteLine(lastUtterance);
                                    Console.WriteLine(normalizedText);
                                    Console.WriteLine();
                                }

                                lastUtterance = normalizedText;
                                lastWords = normalizedWords;
                                break;
                            default:
                                continue;
                        }
                    }
                }
            }
        }
    }
}
