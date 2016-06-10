using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class LinkBasedExtractor
    {
        private readonly UtteranceLinker _linker;

        private readonly int _entityHypCount;

        internal int TotalEntityCount { get; private set; }

        internal LinkBasedExtractor(UtteranceLinker linker, int entityHypCount)
        {
            _linker = linker;
            _entityHypCount = entityHypCount;
        }

        internal IEnumerable<EntityInfo> ExtractAnswerEntity(QuestionDialog dialog)
        {
            var linkedQuestion = _linker.LinkUtterance(dialog.Question, _entityHypCount).First();

            var answerPhaseEntities = getAnswerPhaseEntities(dialog);
            var questionEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();

            var selectedAnswerEntities = getAnswerEntities(dialog, answerPhaseEntities, questionEntities).ToArray();

            TotalEntityCount += selectedAnswerEntities.Length;

            var answerEntities = selectedAnswerEntities.Distinct().OrderByDescending(e => e.OutBounds + e.InBounds).Take(1).ToArray();

            return answerEntities;
        }

        private static IEnumerable<EntityInfo> getAnswerEntities(QuestionDialog dialog, IEnumerable<EntityInfo> answerPhaseEntities, EntityInfo[] questionEntities)
        {
            IEnumerable<EntityInfo> selectedAnswerEntities;
            if (dialog.Question.ToLowerInvariant().Split(' ').Contains("or"))
            {
                selectedAnswerEntities = answerPhaseEntities.Intersect(questionEntities);
            }
            else
            {
                selectedAnswerEntities = answerPhaseEntities.Except(questionEntities);
            }
            return selectedAnswerEntities;
        }
               
        private IEnumerable<EntityInfo> getAnswerPhaseEntities(QuestionDialog dialog)
        {
            var entities = new List<EntityInfo>();
            foreach (var answerTurn in dialog.AnswerTurns)
            {
                var linkedAnswerUtterance = _linker.LinkUtterance(answerTurn.InputChat, _entityHypCount).First();
                entities.AddRange(linkedAnswerUtterance.Parts.SelectMany(p => p.Entities));
            }

            return entities;
        }
    }
}
