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
        private readonly ILinker _linker;

        private readonly FreebaseDbProvider _db;

        internal int TotalEntityCount { get; private set; }

        private Dictionary<string, int> _positiveCounts = new Dictionary<string, int>();

        private Dictionary<string, int> _totalCounts = new Dictionary<string, int>();

        private readonly int _contextNgramSize = 3;

        private readonly string _contextAnswerPlaceholder = "$";

        internal LinkBasedExtractor(ILinker linker, FreebaseDbProvider db)
        {
            _db = db;
            _linker = linker;
        }

        internal IEnumerable<EntityInfo> ExtractAnswerEntity(QuestionDialog dialog)
        {
            var questionEntities = getQuestionEntities(dialog);


            var answerEntities = getAnswerEntities(dialog, questionEntities).ToArray();
            TotalEntityCount += answerEntities.Length;

            return answerEntities;
        }

        private EntityInfo[] getQuestionEntities(QuestionDialog dialog)
        {
            var linkedQuestion = _linker.LinkUtterance(dialog.Question);
            var questionEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();
            return questionEntities;
        }

        private IEnumerable<EntityInfo> getAnswerEntities(QuestionDialog dialog, EntityInfo[] questionEntities)
        {
            var answerPhaseEntities = getAnswerPhaseEntities(dialog, questionEntities);
            var intersectEntities = answerPhaseEntities.Intersect(questionEntities).ToArray();

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

            var linkedAnswerHint = getLinkedAnswerHint(dialog, questionEntities);
            if (linkedAnswerHint == null)
                return entityScores.Keys;

            var ngrams = linkedAnswerHint.GetNgrams(_contextNgramSize);

            answerPhaseEntities = answerPhaseEntities.Intersect(entityScores.Keys).Distinct().ToArray();

            return answerPhaseEntities.OrderByDescending(e => e.InBounds + e.OutBounds).ToArray();
            //return answerPhaseEntities.OrderByDescending(e => contextScore(e, ngrams)).ThenByDescending(e => entityScores[e]).ToArray();
            //return answerPhaseEntities.OrderByDescending(e => entityScores[e]).ToArray();
            //return answerPhaseEntities.OrderByDescending(e => questionContextScore(e, questionEntities)).ToArray();
        }

        private double questionContextScore(EntityInfo entity, IEnumerable<EntityInfo> questionEntities)
        {
            var score = 0.0;
            var entityIds = new HashSet<string>(questionEntities.Select(e => FreebaseLoader.GetId(e.Mid)));

            var entry = _db.GetEntryFromId(FreebaseLoader.GetId(entity.Mid));
            foreach (var target in entry.Targets)
            {
                if (entityIds.Contains(target.Item2))
                    score += 1;
            }

            return score;
        }

        private double contextScore(EntityInfo e, IEnumerable<string> ngrams)
        {
            var totalScore = 0.0;
            foreach (var ngram in ngrams)
            {
                if (!ngram.Contains(e.Mid))
                    continue;

                var replaced = ngram.Replace(e.Mid, _contextAnswerPlaceholder);
                int ngramScore;
                _positiveCounts.TryGetValue(replaced, out ngramScore);

                totalScore += ngramScore;
            }

            return totalScore;
        }

        private IEnumerable<EntityInfo> getAnswerPhaseEntities(QuestionDialog dialog, IEnumerable<EntityInfo> questionEntities)
        {
            var entities = new List<EntityInfo>();
            /*foreach (var answerTurn in dialog.AnswerTurns)
            {
                var linkedAnswerUtterance = _linker.LinkUtterance(answerTurn.InputChat, questionEntities);
                entities.AddRange(linkedAnswerUtterance.Parts.SelectMany(p => p.Entities));
            }*/

            var linkedAnswerUtterance = getLinkedAnswerHint(dialog, questionEntities);
            if (linkedAnswerUtterance == null)
                return Enumerable.Empty<EntityInfo>();
            entities.AddRange(linkedAnswerUtterance.Parts.SelectMany(p => p.Entities));
            return entities;
        }

        private LinkedUtterance getLinkedAnswerHint(QuestionDialog dialog, IEnumerable<EntityInfo> questionEntities)
        {
            var answerTurn = dialog.AnswerTurns.Last();
            var answerText = answerTurn.InputChat;
            var linkedAnswerUtterance = _linker.LinkUtterance(answerText, questionEntities);
            return linkedAnswerUtterance;
        }

        internal void Train(QuestionDialog dialog)
        {
            var questionEntities = getQuestionEntities(dialog);
            var answerHint = getLinkedAnswerHint(dialog, questionEntities);
            if (answerHint == null)
                return;

            foreach (var ngram in answerHint.GetNgrams(_contextNgramSize))
            {
                var replaced = ngram.Replace(dialog.AnswerMid, _contextAnswerPlaceholder);
                if (ngram.Contains(dialog.AnswerMid))
                {
                    int count;
                    _positiveCounts.TryGetValue(replaced, out count);
                    ++count;
                    _positiveCounts[replaced] = count;
                    Console.WriteLine(replaced);
                }
            }
        }

        internal void PrintInfo()
        {
            foreach (var countPair in _positiveCounts.OrderBy(p => p.Value))
            {
                Console.WriteLine(countPair.Value + ": " + countPair.Key);
            }
        }
    }
}
