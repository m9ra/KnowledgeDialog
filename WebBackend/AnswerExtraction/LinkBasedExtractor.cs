using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Parsing;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class LinkBasedExtractor
    {
        internal readonly ILinker Linker;

        internal readonly FreebaseDbProvider Db;

        internal bool DisableEnumerationDetection = false;

        internal bool UseNgramOrdering = false;

        private readonly Regex _entityIdParser = new Regex(@"[\[][^\]$]+[\]]", RegexOptions.Compiled);

        internal int TotalEntityCount { get; private set; }

        private Dictionary<string, int> _positiveCounts = new Dictionary<string, int>();

        private Dictionary<string, int> _falsePositiveCounts = new Dictionary<string, int>();

        private Dictionary<string, int> _totalCounts = new Dictionary<string, int>();

        private readonly int _contextNgramSize = 5;

        private readonly string _testedEntityPlaceholder = "$t";

        private readonly string _contextEntityPlaceholder = "$c";

        internal LinkBasedExtractor(ILinker linker, FreebaseDbProvider db)
        {
            Db = db;
            Linker = linker;
        }

        internal IEnumerable<EntityInfo> ExtractAnswerEntity(QuestionDialog dialog)
        {
            return ExtractAnswerEntity(dialog.Question, getAnswerHint(dialog));
        }

        internal IEnumerable<EntityInfo> ExtractAnswerEntity(string question, string answerHint)
        {
            var questionEntities = getQuestionEntities(question);
            var answerEntities = getAnswerEntities(question, answerHint, questionEntities).ToArray();

            TotalEntityCount += answerEntities.Length;

            return answerEntities;
        }

        private EntityInfo[] getQuestionEntities(string question)
        {
            var linkedQuestion = Linker.LinkUtterance(question);
            var questionEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();
            return questionEntities;
        }

        private IEnumerable<EntityInfo> getAnswerEntities(string question, string answerHint, EntityInfo[] questionEntities)
        {
            var linkedAnswerHint = getLinkedAnswerHint(answerHint, questionEntities);
            var ngrams = linkedAnswerHint.GetNgrams(_contextNgramSize);

            var answerPhaseEntities = getAnswerHintEntities(answerHint, questionEntities);

            var intersectEntities = answerPhaseEntities.Intersect(questionEntities).ToArray();

            var entityScores = new Dictionary<EntityInfo, double>();
            foreach (var entity in answerPhaseEntities)
            {
                entityScores[entity] = 1.0;
            }

            var questionScores = new Dictionary<EntityInfo, double>();
            foreach (var entity in questionEntities)
            {
                questionScores[entity] = 1.0;
            }

            if (UseNgramOrdering)
            {
                foreach (var entity in answerPhaseEntities)
                {
                    var entityNgrams = ngrams.Where(n => n.Contains(entity.Mid)).ToArray();

                    var scoreSum = 0.0;
                    foreach (var ngram in entityNgrams)
                    {
                        var processedNgram = _entityIdParser.Replace(ngram, m => "[]");

                        int positiveCount, totalCount;


                        _positiveCounts.TryGetValue(processedNgram, out positiveCount);
                        _totalCounts.TryGetValue(processedNgram, out totalCount);

                        scoreSum += 1.0 * positiveCount / (totalCount + 1);
                    }

                    entityScores[entity] = scoreSum;
                }
            }

            var linkedQuestion = Linker.LinkUtterance(question);
            var questionParts = linkedQuestion.Parts.ToArray();
            var containsEnumeration = questionParts.Any(p => p.Token == "or");
            if (containsEnumeration && !DisableEnumerationDetection)
            {
                var choiceCandidates = new List<EntityInfo>();
                for (var i = 0; i < questionParts.Length; ++i)
                {
                    var part = questionParts[i];
                    if (part.Token == "or")
                    {
                        choiceCandidates.AddRange(questionParts[i - 1].Entities);
                        choiceCandidates.AddRange(questionParts[i + 1].Entities);
                    }
                }

                foreach (var entity in choiceCandidates)
                {
                    if (!entityScores.ContainsKey(entity))
                        continue;

                    entityScores[entity] += 1;
                }
            }
            else
            {
                foreach (var entity in questionEntities)
                {
                    if (!entityScores.ContainsKey(entity))
                        continue;

                    entityScores[entity] -= questionScores[entity];
                }
            }

            if (linkedAnswerHint == null)
                return entityScores.Keys;


            answerPhaseEntities = answerPhaseEntities.Intersect(entityScores.Keys).Distinct().ToArray();

            return answerPhaseEntities.OrderByDescending(e => entityScores[e]).ThenByDescending(e => e.InBounds + e.OutBounds).ToArray();
        }

        private double questionContextScore(EntityInfo entity, IEnumerable<EntityInfo> questionEntities)
        {
            var score = 0.0;
            var entityIds = new HashSet<string>(questionEntities.Select(e => FreebaseDbProvider.GetId(e.Mid)));

            var entry = Db.GetEntryFromId(FreebaseDbProvider.GetId(entity.Mid));
            foreach (var target in entry.Targets)
            {
                if (entityIds.Contains(target.Item2))
                    score += 1;
            }

            return score;
        }

        private double contextScore(EntityInfo e, IEnumerable<string> ngrams, IEnumerable<EntityInfo> questionEntities)
        {
            var totalScore = 0.0;
            var processedNgrams = getPositiveNgrams(ngrams, questionEntities, e.Mid);
            foreach (var ngram in processedNgrams)
            {
                int positiveScore;
                _positiveCounts.TryGetValue(ngram, out positiveScore);

                int negativeScore;
                _falsePositiveCounts.TryGetValue(ngram, out negativeScore);

                totalScore += positiveScore - negativeScore;
            }

            return totalScore;
        }

        private IEnumerable<string> getPositiveNgrams(IEnumerable<string> ngrams, IEnumerable<EntityInfo> contextEntities, string testedEntityMid)
        {
            var contextIds = new HashSet<string>();
            contextIds.UnionWith(contextEntities.Select(e => e.Mid));

            var result = new List<string>();

            foreach (var ngram in ngrams)
            {
                if (!ngram.Contains(testedEntityMid))
                    //we are interested in positive ngrams only
                    continue;

                var processedNgram = ngram.Replace(testedEntityMid, _testedEntityPlaceholder);
                foreach (var contextId in contextIds)
                {
                    processedNgram = processedNgram.Replace(contextId, _contextEntityPlaceholder);
                }

                processedNgram = _entityIdParser.Replace(processedNgram, m => "[]");
                result.Add(processedNgram);
            }

            return result;
        }

        private IEnumerable<string> getNegativeNgrams(IEnumerable<string> ngrams, IEnumerable<EntityInfo> contextEntities, EntityInfo testedEntity)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<EntityInfo> getAnswerHintEntities(string answerHint, IEnumerable<EntityInfo> questionEntities)
        {
            var entities = new List<EntityInfo>();
            /*foreach (var answerTurn in dialog.AnswerTurns)
            {
                var linkedAnswerUtterance = _linker.LinkUtterance(answerTurn.InputChat, questionEntities);
                entities.AddRange(linkedAnswerUtterance.Parts.SelectMany(p => p.Entities));
            }*/

            var linkedAnswerUtterance = getLinkedAnswerHint(answerHint, questionEntities);
            if (linkedAnswerUtterance == null)
                return Enumerable.Empty<EntityInfo>();
            entities.AddRange(linkedAnswerUtterance.Parts.SelectMany(p => p.Entities));
            return entities;
        }

        private LinkedUtterance getLinkedAnswerHint(string answerHint, IEnumerable<EntityInfo> questionEntities)
        {
            var linkedAnswerUtterance = Linker.LinkUtterance(answerHint, questionEntities);
            return linkedAnswerUtterance;
        }

        private string getAnswerHint(QuestionDialog dialog)
        {
            var answerTurn = dialog.AnswerTurns.Last();
            return answerTurn.InputChat;
        }

        internal void Train(QuestionDialog dialog)
        {
            var questionEntities = getQuestionEntities(dialog.Question);
            var answerHint = getLinkedAnswerHint(getAnswerHint(dialog), questionEntities);
            if (answerHint == null)
                return;

            foreach (var ngram in answerHint.GetNgrams(_contextNgramSize))
            {
                var replacedNgram = _entityIdParser.Replace(ngram, m => "[]");
                int count;

                if (ngram.Contains(dialog.AnswerMid))
                {
                    _positiveCounts.TryGetValue(replacedNgram, out count);
                    _positiveCounts[replacedNgram] = count + 1;
                }

                _totalCounts.TryGetValue(ngram, out count);
                _totalCounts[replacedNgram] = count + 1;
            }
        }

        internal void PrintInfo()
        {
            Console.WriteLine("POSITIVE");
            foreach (var countPair in _positiveCounts.OrderBy(p => p.Value))
            {
                Console.WriteLine(countPair.Value + ": " + countPair.Key);
            }

            Console.WriteLine("FALSE POSITIVE");
            foreach (var countPair in _falsePositiveCounts.OrderBy(p => p.Value))
            {
                Console.WriteLine(countPair.Value + ": " + countPair.Key);
            }
        }
    }
}
