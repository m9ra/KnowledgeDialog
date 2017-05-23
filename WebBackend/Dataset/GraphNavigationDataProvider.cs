using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBackend.Experiment;

namespace WebBackend.Dataset
{
    class GraphNavigationDataProvider
    {
        private readonly List<ExperimentBase> _experiments = new List<ExperimentBase>();

        private readonly List<Tuple<string, string>> _labelRequestResponses = new List<Tuple<string, string>>();

        internal IEnumerable<Tuple<string, string>> LabelRequestResponses => _labelRequestResponses;

        internal IEnumerable<string> RequestedLabels => _labelRequestResponses.Select(t => t.Item1).Distinct().ToArray();

        internal GraphNavigationDataProvider(ExperimentCollection collection, string experimentPrefix)
        {
            foreach (var experiment in collection.Experiments)
            {
                if (experiment.Id.StartsWith(experimentPrefix))
                    _experiments.Add(experiment);
            }

            _experiments.Sort((a, b) => a.Id.CompareTo(b.Id));

            load();
        }

        internal IEnumerable<string> GetLabelHints(string label)
        {
            var result = from response in _labelRequestResponses where response.Item1 == label select response.Item2;
            return result.ToArray();
        }

        private void load()
        {
            _labelRequestResponses.Clear();
            var logFiles = new List<LogFile>();
            foreach (var experiment in _experiments)
            {
                foreach (var logFile in experiment.LoadLogFiles())
                {
                    logFiles.Add(logFile);
                }
            }

            foreach (var logFile in logFiles)
            {
                var actions = logFile.LoadActions().ToArray();
                string currentLabelRequested = null;
                foreach (var action in actions)
                {
                    if (action.IsReset)
                        currentLabelRequested = null;

                    switch (action.Type)
                    {
                        case "T_response":

                            var welcomeLabelRequest = action.ParseAct("WelcomeWithEntityLabelRequest(phrase='", "'");
                            var continuedLabelRequest = action.ParseAct("RequestEntityLabel(phrase='", "'");
                            var labelRequest = welcomeLabelRequest == null ? continuedLabelRequest : welcomeLabelRequest;

                            if (labelRequest == null)
                                break;

                            if (currentLabelRequested != null)
                                throw new NotImplementedException("Unprocessed label request");

                            currentLabelRequested = labelRequest;
                            break;
                        case "T_utterance":
                            if (currentLabelRequested == null)
                                throw new NotImplementedException();

                            var hint = action.Text;
                            _labelRequestResponses.Add(Tuple.Create(currentLabelRequested, hint));

                            currentLabelRequested = null;
                            break;
                    }
                }
            }
        }
    }
}
