using PerceptiveDialogBasedAgent.Interpretation;
using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent
{
    internal delegate DbConstraint NativeCallWrapper(DbConstraint input);

    class RestaurantAgent : EmptyAgent
    {
        internal RestaurantAgent()
        {
            Mind
                .AddPattern("a", "$something")
                    .HowToEvaluate(c => c["something"])

                .AddPattern("an", "$something")
                    .HowToEvaluate(c => c["something"])

                .AddPattern("if", "$something", "then", "$something2")
                    .HowToDo(c =>
                    {
                        var isConditionTrue = c.IsTrue("something");
                        if (isConditionTrue)
                        {
                            return c["something2"];
                        }

                        return DbConstraint.Entity("nothing");
                    })

                .AddPattern("$someone", "said", "$something")
                    .HowToEvaluate(c => c["someone"].ExtendByAnswer("What @ said?", c["something"]))

                .AddPattern("user", "specified", "$something")
                    .IsTrue("criterions on $something exists")

                .AddPattern("$something", "exists")
                    .IsTrue(c => Body.IfDbContains(c["something"]))

                .AddPattern("criterions", "on", "$something")
                    .HowToEvaluate(c => findUserCriterions(c["something"]))

                .AddPattern("you", "know", "$something")
                    .IsTrue(c => Body.IfDbContains(c["something"]))

                .AddPattern("which", "$something")
                    .HowToEvaluate(c =>
                        new DbConstraint().ExtendByAnswer("What is @?", c["something"])
                    )

                .AddPattern("$something", "user", "wants")
                    .HowToEvaluate(c =>
                        c["something"].ExtendBySubject(DbConstraint.Entity("user"), "What @ wants?")
                    )


                .AddPattern("remember", "$something")
                    .HowToDo(c =>
                    {
                        var fact = c["something"];
                        if (fact.PhraseConstraint != "nothing")
                        {
                            var answerEntry = fact.SubjectConstraints.First();
                            Mind.Database.AddFact(fact.PhraseConstraint, answerEntry.Question, answerEntry.Answer.PhraseConstraint);
                        }

                        return DbConstraint.Entity("nothing");
                    })

                .AddPattern("I", "want", "$something")
                    .HowToEvaluate(c =>
                    DbConstraint.Entity("user").ExtendByAnswer("What @ wants?", c["something"]))

                .AddPattern("fact", "from", "$someone")
                    .HowToEvaluate(c =>
                    {
                        var input = Mind.Database.GetAnswers("user", "What @ said?").First();
                        var evaluation = Mind.Evaluator.Evaluate(input, Evaluator.HowToEvaluateQ, c);
                        var factConstraint = evaluation.Constraint;
                        if (factConstraint.PhraseConstraint != null && (factConstraint.SubjectConstraints.Any() || factConstraint.AnswerConstraints.Any()))
                            return evaluation.Constraint;
                        else
                            return DbConstraint.Entity("nothing");
                    })

                ;

            /*AddPolicyFact("remember fact from user");
            AddPolicyFact("if user said hello then say hi");
            AddPolicyFact("if you know which restaurant user wants then offer it");*/
        }

        private DbConstraint findUserCriterions(DbConstraint constraint)
        {
            var answers = Mind.Database.GetAnswers("user", "What @ wants?").ToArray();

            var result = new DbConstraint();
            foreach (var answer in answers)
            {
                result = result.ExtendByAnswer("What @ wants?", DbConstraint.Entity(answer));
            }

            return result;
        }

    }
}
