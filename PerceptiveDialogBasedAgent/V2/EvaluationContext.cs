using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    internal delegate SemanticItem NativeEvaluator(EvaluationContext context);

    class EvaluationContext
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

        internal IEnumerable<SemanticItem> IsTrueRaw(string variable)
        {
            var substiution = GetSubstitutionValue(variable);
            var queryItem = SemanticItem.AnswerQuery(Body.IsItTrueQ, new Constraints().AddInput(substiution));
            return _db.SpanQuery(queryItem);
        }

        internal IEnumerable<SemanticItem> Query(string variable, string question)
        {
            var substiution = GetSubstitutionValue(variable);
            var queryItem = SemanticItem.AnswerQuery(question, new Constraints().AddInput(substiution));
            var result = _db.SpanQuery(queryItem);
            return result;
        }

        internal IEnumerable<SemanticItem> Evaluate(string variable)
        {
            var substiution = GetSubstitutionValue(variable);
            var queryItem = SemanticItem.AnswerQuery(Body.HowToEvaluateQ, new Constraints().AddInput(substiution));
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
