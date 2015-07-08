using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Experiment
{
    class NoTaskExperiment : ExperimentBase
    {
        internal NoTaskExperiment(string rootPath, string id)
            : base(rootPath, id)
        {

        }

        /// <inheritdoc/>
        internal override TaskInstance GetTask(int taskId)
        {
            //this experiment doesn't have any task
            return null;
        }
    }
}
