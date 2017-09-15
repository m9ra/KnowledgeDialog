using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent
{
    class Sensor : BodyPart
    {
        public readonly string Condition;

        public readonly List<string> AttachedActions = new List<string>();
    }



    class Body
    {
        private readonly MindSet _mind;

        private readonly List<Sensor> _sensors = new List<Sensor>();

        private List<string> _inputHistory = new List<string>();

        private List<string> _outputHistory = new List<string>();

        private List<string> _outputCandidates = new List<string>();


        internal Body(MindSet mind)
        {
            _mind = mind;
        }

        internal string Input(string utterance)
        {
            _inputHistory.Add(utterance);
            runSensors("before input processing");

            runPolicy();

            runSensors("before output printing");

            var output = _outputCandidates.LastOrDefault();
            _outputHistory.Add(output);
            return output;
        }

        private void runSensors(string trigger)
        {

        }
    }
}
