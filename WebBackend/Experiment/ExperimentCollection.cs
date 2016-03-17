using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Experiment
{
    /// <summary>
    /// Collection of experiments.
    /// </summary>
    class ExperimentCollection
    {
        /// <summary>
        /// Experiments indexed by their ids.
        /// </summary>
        private Dictionary<string, ExperimentBase> _idToExperiment = new Dictionary<string, ExperimentBase>();

        /// <summary>
        /// Root path of the experiment.
        /// </summary>
        private readonly string _experimentsRootPath;

        /// <summary>
        /// Experiments that are available for the collection.
        /// </summary>
        internal IEnumerable<ExperimentBase> Experiments { get { return _idToExperiment.Values; } }

        /// <summary>
        /// Initialize new <see cref="ExperimentCollection"/> with given experiments.
        /// </summary>
        /// <param name="experiments">Experiments that will be encapsulated by current collection.</param>
        internal ExperimentCollection(string experimentsRootPath, params ExperimentBase[] experiments)
        {
            _experimentsRootPath = experimentsRootPath;

            foreach (var experiment in experiments)
            {
                _idToExperiment.Add(experiment.Id, experiment);
            }
        }

        internal ExperimentBase Get(string experimentId)
        {
            if (experimentId == null)
                return null;

            ExperimentBase experiment;
            if (!_idToExperiment.TryGetValue(experimentId, out experiment))
                _idToExperiment[experimentId] = experiment = new NoTaskExperiment(_experimentsRootPath, experimentId);

            return experiment;
        }

    }
}
