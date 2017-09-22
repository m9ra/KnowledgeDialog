using PerceptiveDialogBasedAgent.V1.Interpretation;
using PerceptiveDialogBasedAgent.V1.Knowledge;
using PerceptiveDialogBasedAgent.V1.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V1
{
    class EmptyAgent
    {
        protected readonly MindSet Mind = new MindSet();

        protected readonly Body Body;

        internal EmptyAgent()
        {
            Body = new Body(Mind);

            Mind
                .AddPattern("when", "$something", "then", "$action")
                    .HowToDo("add $action to $something")

                .AddPattern("say", "$something")
                    .HowToDo("print $something")

                .AddPattern("do", "$something")
                    .HowToDo(c => Mind.Evaluator.Evaluate(c["something"].PhraseConstraint, Evaluator.HowToDoQ).Constraint)

                .AddPattern("user", "input")
                    .HowToEvaluate("input from user")

                .AddPattern("$something", "is", "defined")
                    .HowToEvaluate(c => isDefined(c["something"]))
                    .IsTrue(c => isDefined(c["something"]))

                .AddPattern("action", "for", "$something")
                    .HowToEvaluate(c => DbConstraint.Entity(null).ExtendBySubject(c["something"], Evaluator.HowToDoQ))

                .AddPattern("if", "$something", "then", "$something2")
                    .HowToDo(c =>
                    {
                        var isConditionTrue = c.IsTrue("something");
                        if (isConditionTrue)
                        {
                            var result = c.Evaluate("something2", Evaluator.HowToDoQ);
                            return result;
                        }

                        return DbConstraint.Entity("nothing");
                    })

                .AddPattern("ask", "$something")
                    .HowToDo("first write topic $something second set topic pointer to a missing entity third formulate a topic question")

                ;


            Body
                .SensorAction("before input processing",
                    "if action for user input is defined then do user input"  //without this, nothing gets executed
                )

                .Policy("when missing output then ask a database question")
                ;
        }

        internal string Input(string data)
        {
            var result = Body.Input(data);
            return result;
        }

        private DbConstraint isDefined(DbConstraint something)
        {
            //TODO with Db variables this would work easily
            var constraints = something.AnswerConstraints.ToArray();
            if (constraints.Length != 1)
                throw new NotImplementedException();

            var definition = constraints[0].Subject.PhraseConstraint;
            var definitionViewpoint = constraints[0].Question;
            var evaluation = Mind.Evaluator.Evaluate(definition, definitionViewpoint);

            var result = evaluation.Constraint.PhraseConstraint != definition ? "yes" : "no";
            return new DbConstraint(result);
        }
    }
}
