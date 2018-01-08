using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    public delegate SemanticItem NativeEvaluator(EvaluationContext context);

    public class EvaluationContext
    {
        internal readonly SemanticItem Item;

        internal readonly EvaluationContext Parent;

        private readonly Database _db;

        public EvaluationContext(EvaluationContext parent, Database db, SemanticItem item)
        {
            Parent = parent;
            Item = item;
            _db = db;
        }

        internal string GetSubstitutionValue(string variable)
        {
            return Item.GetSubstitutionValue(variable);
        }


        internal SemanticItem GetSubstitution(string variable)
        {
            return Item.GetSubstitution(variable);
        }

        internal bool IsTrue(string variable)
        {
            var substiution = GetSubstitutionValue(variable);
            var queryItem = SemanticItem.AnswerQuery(Question.IsItTrue, Item.Constraints.AddInput(substiution));
            var rawResult = _db.SpanQuery(queryItem);

            if (!rawResult.Any())
                return false;

            return rawResult.Last().Answer == SemanticItem.Yes.Answer;
        }

        internal IEnumerable<SemanticItem> Query(string variable, string question)
        {
            var substiution = GetSubstitutionValue(variable);
            var queryItem = SemanticItem.AnswerQuery(question, new Constraints().AddInput(substiution));

            return Query(queryItem);            
        }

        internal IEnumerable<SemanticItem> Query(SemanticItem query)
        {
            var result = _db.SpanQuery(query);
            return result;
        }

        internal IEnumerable<SemanticItem> Evaluate(string variable)
        {
            var substiution = GetSubstitution(variable);
            if (!substiution.IsEntity)
                return new[] { substiution };

            var queryItem = SemanticItem.AnswerQuery(Question.HowToEvaluate, new Constraints().AddInput(substiution));
            var result = _db.SpanQuery(queryItem);
            return result;
        }

        internal SemanticItem EvaluateOne(string variable)
        {
            var many = Evaluate(variable);
            if (many.Count() > 1)
                throw new NotImplementedException();

            return many.FirstOrDefault() ?? SemanticItem.Entity(GetSubstitutionValue(variable));
        }

    }
}
