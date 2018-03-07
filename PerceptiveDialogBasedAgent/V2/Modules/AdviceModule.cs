using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2.Modules
{
    class AdviceModule : BodyModuleBase
    {
        private readonly Body _body;

        private SemanticItem _askedQuestion;

        private ModuleField<string> _simplifiedField;

        internal AdviceModule(Body body)
        {
            _body = body;

            _simplifiedField = newField<string>("SimplifiedField", _field_simplified);
        }

        /// </inheritdoc>
        protected override void initializeAbilities()
        {
            AddAbility("accept the advice")
                .Param(_simplifiedField)
                .Precondition(_precondition_acceptAdvice)
                .CallAction(_ability_acceptAdvice);

            AddAbility("ask for help")
                .CallAction(_ability_askForHelp);

            AddAnswer("question was asked", Question.IsItTrue)
                .Call(_answer_questionWasAsked);
        }

        private SemanticItem _field_simplified(ModuleContext context)
        {
            var answer = _body.InputHistory.Last();
            var simplifiedAnswer = context.GetAnswer(Question.HowToSimplify, answer);
            if (simplifiedAnswer == null)
                simplifiedAnswer = SemanticItem.Entity(answer);
            else
                simplifiedAnswer = SemanticItem.Entity(simplifiedAnswer.InstantiateWithEntityVariables(simplifiedAnswer.Answer));

            return simplifiedAnswer;
        }

        private SemanticItem _answer_questionWasAsked()
        {
            return SemanticItem.Convert(_askedQuestion != null);
        }

        private bool _precondition_acceptAdvice(ModuleContext context)
        {
            return _askedQuestion != null;
        }

        private void _ability_acceptAdvice(string answer)
        {
            var query = _askedQuestion;
            var newFact = SemanticItem.From(query.Question, answer, query.Constraints);

            Log.NewFact(newFact);
            _body.Db.Container.Add(newFact);
            _body.Print("ok");

            _askedQuestion = null;
            FireEvent("advice was received");
        }

        private void _ability_askForHelp()
        {
            var question = _databaseQuestion();
            _askedQuestion = question;

            _body.Print(_askedQuestion.ReadableRepresentation());
        }

        private SemanticItem _databaseQuestion()
        {
            var questions = _body.Db.CurrentLogRoot.GetQuestions();
            var executionQuestions = _body.ExecutionQuestions;

            if (executionQuestions.Any())
                questions = executionQuestions;

            var orderedQuestions = questions.OrderByDescending(rankQuestion).ToArray();
            var question = orderedQuestions.FirstOrDefault();
            if (question == null)
                throw new NotImplementedException("");

            return question;
        }

        private double rankQuestion(SemanticItem question)
        {
            var q = question.Question;
            var score = 0.0;
            if (q == Question.IsItTrue)
                score += 0.1;
            else if (q == Question.Entity)
                score += 0.0;
            else if (q == Question.HowToEvaluate)
                score += 0.2;
            else if (q == Question.WhatShouldAgentDoNow)
                score += 0.1;
            else if (q == Question.HowToDo)
                score += 0.1;
            else score += 1;

            score += 1.0 / (1.0 + question.Constraints.VariableValues.Count());
            var inputWords = new HashSet<string>(_body.InputHistory.Last().Split(' '));
            var commonWordCount = question.ReadableRepresentation().Split(' ').Sum(qw => _body.InputHistory.Contains(qw) ? 1 : 0);
            score += commonWordCount;
            return score;
        }
    }
}
