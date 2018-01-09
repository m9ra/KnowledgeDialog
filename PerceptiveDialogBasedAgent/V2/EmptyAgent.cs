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
            Body.Db.Container
                .Pattern("when $condition then $action")
                    .HowToDo("add $action to trigger $condition")

                .Pattern("no output is available")
                    .IsTrue("output is missing")

                .Pattern("say $something")
                    .HowToDo("print $something")

                .Pattern("say $something and $something2")
                    .HowToDo("print $something joined with $something2")

                .Pattern("user said $something")
                    .IsTrue("history contains $something")

                .Pattern("question was asked")
                    .IsTrue("question slot is filled")

                .Pattern("ask for help")
                    .HowToDo("write database question into question slot and print the question")

                .Pattern("you know $something")
                    .IsTrue("user said $something or $something is defined")
                    
                .Pattern("it")
                    .HowToEvaluate("It", e =>
                    {
                        //TODO this is a simple workaround - allows to point to input only
                        return SemanticItem.Entity(Body.InputHistory.Last());
                    })

                .Pattern("say $something instead of $something2")
                    .HowToDo("use $something instead of $something2 in output")

            ;

            AddPolicy("when user input is received and it is a command then execute it");
            AddPolicy("when output is missing then ask for help");
            AddPolicy("when question was asked and user input can be an answer then fire event answer is provided");
            AddPolicy("when answer is provided then take the advice for question slot");
            //AddPolicy("when answer is provided then write it into answer slot");
            //AddPolicy("when advice is complete then save it");
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
