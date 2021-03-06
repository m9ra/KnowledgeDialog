﻿using System;
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

                .Pattern("ask $something")
                    .HowToDo("print $something joined with ?")

                .Pattern("say $something and $something2")
                    .HowToDo("print $something joined with $something2")

                .Pattern("user said $something")
                    .IsTrue("history contains $something")

                .Pattern("you know $something")
                    .IsTrue("user said $something or $something is defined")

                .Pattern(
                    "i think $something",
                    "a $something",
                    "an $something",
                    "maybe $something",
                    "probably $something",
                    "it is $something"
                    )
                    .HowToSimplify("$something")

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
            AddPolicy("when answer is provided then accept the advice");
            AddPolicy("when the last command failed and advice was received then repeat the last command");
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
