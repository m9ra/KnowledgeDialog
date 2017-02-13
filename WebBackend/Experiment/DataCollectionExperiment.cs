using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Task;
using WebBackend.DialogProvider;

namespace WebBackend.Experiment
{
    class DataCollectionExperiment : ExperimentBase
    {
        public DataCollectionExperiment(string experimentsRoot, string experimentId, int taskCount, params TaskFactoryBase[] factories)
            : base(experimentsRoot, experimentId)
        {
            ExportExperiment(experimentId, taskCount, factories);
        }
        
        protected override WebConsoleBase createConsole(string databasePath)
        {
            return new DataCollectionWebConsole(databasePath);
        }
    }
}
