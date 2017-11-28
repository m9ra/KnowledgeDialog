using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    public class SemanticItem
    {
        public readonly static string EntityQ = "what is it?";

        public readonly static string InputVar = "$@";

        public readonly string Question;

        public readonly string Answer;

        public readonly Constraints Constraints;

        public readonly int Id = ++_nextId;

        private static int _nextId;

        public bool IsEntity
        {
            get
            {
                return Question == EntityQ && Constraints.Input == Answer;
            }
        }

        internal string InstantiateInputWithEntityVariables()
        {
            return InstantiateWithEntityVariables(Constraints.Input);
        }

        internal string InstantiateWithEntityVariables(string phrase)
        {
            if (phrase == null)
                return null;

            var result = " " + phrase + " ";
            foreach (var variablePair in Constraints.VariableValues)
            {
                if (!variablePair.Value.IsEntity)
                    continue;

                result = result.Replace(" " + variablePair.Key + " ", " " + variablePair.Value.Answer + " ");
            }

            return result.Trim();
        }

        internal IEnumerable<string> Phrases
        {
            get
            {
                if (Question != null)
                    yield return Question;

                if (Answer != null)
                    yield return Answer;

                foreach (var phrase in Constraints.Phrases)
                    yield return phrase;
            }
        }

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

        internal string GetSubstitutionValue(string variable)
        {
            var substitutionItem = GetSubstitution(variable);

            if (!substitutionItem.IsEntity)
                throw new NotImplementedException();

            return substitutionItem?.Answer;
        }

        internal SemanticItem GetSubstitution(string variable)
        {
            if (Constraints == null)
                return null;

            if (Constraints == null)
                return null;

            var substitutionItem = Constraints.GetSubstitution(variable);
            return substitutionItem;
        }

        internal static SemanticItem From(string question, string answer, Constraints constraints)
        {
            return new SemanticItem(question, answer, constraints);
        }

        public override string ToString()
        {
            return string.Format("[{3}]{0} {1} | {2}", Question ?? "*", Answer ?? "*", Constraints.ShallowToString(), Id);
        }

        internal string ReadableRepresentation()
        {
            var question = InstantiateWithEntityVariables(Question);
            var answer = InstantiateWithEntityVariables(Answer);
            if (answer == null)
                return question;

            return question + " " + answer;
        }
    }

    public class Constraints
    {
        private readonly Dictionary<string, SemanticItem> _variableValues = new Dictionary<string, SemanticItem>();

        public readonly IEnumerable<string> Conditions = new string[0];

        internal static string Negate(string condition)
        {
            if (condition.StartsWith("~"))
            {
                return condition.Substring(1).Trim();
            }
            else
            {
                return "~ " + condition;
            }
        }

        public IEnumerable<KeyValuePair<string, SemanticItem>> VariableValues { get { return _variableValues; } }

        public string Input
        {
            get
            {
                _variableValues.TryGetValue("$@", out SemanticItem input);
                if (input == null)
                    return null;

                return input.Answer;
            }
        }

        public IEnumerable<string> Phrases
        {
            get
            {
                foreach (var variablePair in _variableValues)
                {
                    yield return variablePair.Value.Answer;
                    yield return variablePair.Value.Question;
                }
            }
        }

        public Constraints()
        {

        }

        private Constraints(Dictionary<string, SemanticItem> values, IEnumerable<string> conditions)
        {
            _variableValues = new Dictionary<string, SemanticItem>(values);
            Conditions = conditions.Distinct().ToArray();
        }

        public Constraints AddInput(string value)
        {
            return this.AddValue("$@", value);
        }

        public Constraints AddInput(SemanticItem value)
        {
            return this.AddValue("$@", value);
        }

        public Constraints AddValue(string variable, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return AddValue(variable, SemanticItem.Entity(value));
        }

        public Constraints AddValue(string variable, SemanticItem item)
        {
            item = item ?? throw new ArgumentNullException(nameof(item));

            var values = new Dictionary<string, SemanticItem>(_variableValues);
            values[variable] = item;

            return new Constraints(values, Conditions);
        }

        public SemanticItem GetSubstitution(string variable)
        {
            return _variableValues[variable];
        }

        internal Constraints AddCondition(string condition)
        {
            return new Constraints(_variableValues, Conditions.Concat(new[] { condition }));
        }

        internal string ShallowToString()
        {
            var variables = new List<string>();
            foreach (var variablePair in _variableValues.OrderBy(v => v.Key))
            {
                if (variablePair.Key == "$@")
                    continue;

                var value = string.Format("{0} {1}", variablePair.Value.Question, variablePair.Value.Answer);
                if (variablePair.Value.IsEntity)
                {
                    value = variablePair.Value.Answer;
                }

                variables.Add(string.Format("{0}: {1}", variablePair.Key, value));
            }

            var variablesStr = string.Join(", ", variables).Trim();
            if (variablesStr != "")
                variablesStr = "VARS " + variablesStr + ";";

            return string.Format("INPUT {0};{1}", Input, variablesStr);
        }

        internal static Constraints WithInput(string input)
        {
            return new Constraints().AddInput(input);
        }

        internal static Constraints WithInput(SemanticItem input)
        {
            return new Constraints().AddInput(input);
        }

        internal Constraints AddConditions(IEnumerable<string> conditions)
        {
            return new Constraints(_variableValues, Conditions.Concat(conditions));
        }

        internal Constraints MergeWith(Constraints constraints)
        {
            var result = this;
            result = result.AddConditions(constraints.Conditions);

            foreach (var variable in constraints.VariableValues)
            {
                result = result.AddValue(variable.Key, variable.Value);
            }
            return result;
        }
    }
}
