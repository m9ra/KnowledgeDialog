using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class EmptyAgent
    {
        /// <summary>
        /// Agents body.
        /// </summary>
        internal readonly Body Body = new Body();

        internal EmptyAgent()
        {            
            Body
                .Pattern("$action1 and $action2")
                    .HowToDo("not implemented")

                .Pattern("no output is available")
                    .IsTrue("output is missing")

                .Pattern("ask for help")
                    .HowToDo("write database question into question slot and print the question")
            ;


            AddPolicy("when user input is a command execute it");
            AddPolicy("when no output is available ask for help");
            AddPolicy("when answer is provided write it into answer slot");
            AddPolicy("when advice is complete save it");            
        }

        public string Input(string utternace)
        {
            return Body.Input(utternace);
        }

        protected void AddPolicy(string utterance)
        {
            Body.PolicyInput(utterance);
        }
    }
}
