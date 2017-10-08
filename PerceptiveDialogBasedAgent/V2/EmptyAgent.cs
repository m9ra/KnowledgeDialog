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
            Body
                .Pattern("when $condition then $action")
                    .HowToDo("add $action to trigger $condition")

                .Pattern("$action1 and $action2")
                    .HowToDo("not implemented")

                .Pattern("no output is available")
                    .IsTrue("output is missing")

                .Pattern("say $something")
                    .HowToDo("print $something")

                .Pattern("ask for help")
                    .HowToDo("write database question into question slot and print the question")

                .Pattern("$something is a command")
                    .IsTrue("$something has how to do question specified")

                 .Pattern("it")
                    .HowToEvaluate("It", e =>
                    {
                        //TODO this is simple workaround - allows to point to input only
                        return SemanticItem.Entity(Body.InputHistory.Last());
                    })

                .Pattern("execute $something")
                    .HowToDo("Execute", e =>
                    {
                        var something = e.EvaluateOne("$something");
                        return something;
                    })

                .Pattern("$something has $question question specified")
                    .IsTrue("IsQuestionSpecified", e =>
                    {
                        var something = e.EvaluateOne("$something");
                        var question = e.GetSubstitution("$question");
                        var normalizedQuestion = question + " $@ ?";
                        var queryItem = SemanticItem.AnswerQuery(normalizedQuestion, Constraints.WithInput(something.Answer));
                        var result = Body.Db.Query(queryItem);
                        var answer = result.Any() ? Database.YesA : Database.NoA;

                        return SemanticItem.Entity(answer);
                    })
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
