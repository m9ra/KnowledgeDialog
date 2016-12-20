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
        private readonly GraphDisambiguatedLinker _linker;

        internal int TotalEntityCount { get; private set; }

        internal LinkBasedExtractor(GraphDisambiguatedLinker linker)
        {
            _linker = linker;
        }

        internal IEnumerable<EntityInfo> ExtractAnswerEntity(QuestionDialog dialog)
        {
            var linkedQuestion = _linker.LinkUtterance(dialog.Question);
            var questionEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();

            var answerPhaseEntities = getAnswerPhaseEntities(dialog, questionEntities);
            var selectedAnswerEntities = getAnswerEntities(dialog, answerPhaseEntities, questionEntities).ToArray();

            TotalEntityCount += selectedAnswerEntities.Length;
            var answerEntities = selectedAnswerEntities.Distinct().OrderByDescending(e => e.OutBounds + e.InBounds).ToArray();

            return answerEntities;
        }

        private static IEnumerable<EntityInfo> getAnswerEntities(QuestionDialog dialog, IEnumerable<EntityInfo> answerPhaseEntities, EntityInfo[] questionEntities)
        {
            var entityScores = new Dictionary<EntityInfo, double>();
            foreach (var entity in answerPhaseEntities)
            {
                entityScores[entity] = entity.Score;
            }

            if (dialog.Question.ToLowerInvariant().Split(' ').Contains("or"))
            {
                foreach (var entity in questionEntities)
                {
                    if (!entityScores.ContainsKey(entity))
                        continue;

                    entityScores[entity] += entity.Score;
                }
            }
            else
            {
                foreach (var entity in questionEntities)
                {
                    if (!entityScores.ContainsKey(entity))
                        continue;

                    entityScores[entity] -= entity.Score;
                }
            }
            return answerPhaseEntities.OrderByDescending(e => entityScores[e]).ToArray();
        }

        private IEnumerable<EntityInfo> getAnswerPhaseEntities(QuestionDialog dialog, IEnumerable<EntityInfo> questionEntities)
        {
            var entities = new List<EntityInfo>();
            /*foreach (var answerTurn in dialog.AnswerTurns)
            {
                var linkedAnswerUtterance = _linker.LinkUtterance(answerTurn.InputChat, questionEntities);
                entities.AddRange(linkedAnswerUtterance.Parts.SelectMany(p => p.Entities));
            }*/

            var answerTurn = dialog.AnswerTurns.Last();
            var linkedAnswerUtterance = _linker.LinkUtterance(answerTurn.InputChat, questionEntities);
            entities.AddRange(linkedAnswerUtterance.Parts.SelectMany(p => p.Entities));
            return entities;
        }
    }
}
