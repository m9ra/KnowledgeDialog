using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class Body
    {
        internal readonly Mind Mind = new Mind();

        public string Input(string utterance)
        {
            Log.DialogUtterance("U: " + utterance);
            throw new NotImplementedException();
            var result = "notimplemented";
            Log.DialogUtterance("S: " + utterance);
        }
    }
}
