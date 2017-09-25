﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    class SemanticItem
    {
        public readonly static string EntityQ = "What is it?";

        public readonly static string InputVar = "$@";

        public readonly string Question;

        public readonly string Answer;

        public readonly Constraints Constraints;

        private SemanticItem(string question, string answer, Constraints constraints)
        {
            Question = question;
            Answer = answer;
            Constraints = constraints;
        }

        private SemanticItem(string entity)
        {
            Question = EntityQ;
            Answer = entity;
            Constraints = new Constraints().AddValue(InputVar, this);
        }

        public static SemanticItem Entity(string entity)
        {
            return new SemanticItem(entity);
        }

        public static SemanticItem AnswerQuery(string question, Constraints constraints)
        {
            return new SemanticItem(question, null, constraints);
        }

        internal static SemanticItem Pattern(string patternValue, string question, string answer)
        {
            var constraints = new Constraints().AddInput(patternValue);
            return new SemanticItem(question, answer, constraints);
        }

        internal SemanticItem WithConstraints(Constraints constraints)
        {
            return new SemanticItem(Question, Answer, constraints);
        }
    }

    class Constraints
    {
        private readonly Dictionary<string, SemanticItem> _values = new Dictionary<string, SemanticItem>();

        private readonly List<string> _conditions = new List<string>();

        public IEnumerable<string> Conditions => _conditions;

        public string Input
        {
            get
            {
                _values.TryGetValue("$@", out SemanticItem input);
                if (input == null)
                    return null;

                return input.Answer;
            }
        }

        public Constraints()
        {

        }

        private Constraints(Dictionary<string, SemanticItem> values)
        {
            _values = new Dictionary<string, SemanticItem>(values);
        }

        public Constraints AddInput(string value)
        {
            return this.AddValue("$@", value);
        }

        public Constraints AddValue(string variable, string value)
        {
            return AddValue(variable, SemanticItem.Entity(value));
        }

        public Constraints AddValue(string variable, SemanticItem item)
        {
            var values = new Dictionary<string, SemanticItem>(_values);
            values[variable] = item;

            return new Constraints(values);
        }

        public SemanticItem GetSubstitution(string variable)
        {
            return _values[variable];
        }
    }
}
