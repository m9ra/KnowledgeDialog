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
                .Pattern("when $condition then $action")
                    .HowToDo("add $action to trigger $condition")

                .Pattern("execute $something")
                    .HowToDo("not implemented")

                .Pattern("$action1 and $action2")
                    .HowToDo("not implemented")

                .Pattern("no output is available")
                    .IsTrue("output is missing")

                .Pattern("ask for help")
                    .HowToDo("write database question into question slot and print the question")
            ;


            AddPolicy("when user input is a command then execute it");
            AddPolicy("when no output is available then ask for help");
            AddPolicy("when answer is provided then write it into answer slot");
            AddPolicy("when advice is complete then save it");            
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
