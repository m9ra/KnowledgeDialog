using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Dataset
{
    class AnnotatedDialog
    {
        private readonly AnnotatedSemiTurn[] _questionTurns;

        private readonly AnnotatedSemiTurn[] _explanationTurns;

        private readonly AnnotatedSemiTurn[] _answerTurns;

        internal AnnotatedDialog(IEnumerable<AnnotatedSemiTurn> questionTurns, IEnumerable<AnnotatedSemiTurn> explanationTurns, IEnumerable<AnnotatedSemiTurn> answerTurns)
        {
            _questionTurns = questionTurns.ToArray();
            _explanationTurns = explanationTurns.ToArray();
            _answerTurns = answerTurns.ToArray();
        }

        internal string ToJson()
        {
            var questionRepresentations = getRepresentations(_questionTurns);
            
            var jsonRepresentation = new Dictionary<string, object>();
            jsonRepresentation["QuestionTurns"] = getRepresentations(_questionTurns);
            jsonRepresentation["ExplanationTurns"] = getRepresentations(_explanationTurns);
            jsonRepresentation["AnswerTurns"] = getRepresentations(_answerTurns);

            return Newtonsoft.Json.JsonConvert.SerializeObject(jsonRepresentation);
        }

        private Dictionary<string, object>[] getRepresentations(IEnumerable<AnnotatedSemiTurn> semiTurns)
        {
            var representations = new List<Dictionary<string, object>>();

            foreach (var semiTurn in semiTurns)
            {
                representations.Add(semiTurn.GetRepresentation());
            }

            return representations.ToArray();
        }
    }
}
