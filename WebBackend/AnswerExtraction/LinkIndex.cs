using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.AnswerExtraction
{
    class EntityMatch
    {
        internal readonly int Length;

        internal readonly int Position;

        internal readonly EntityInfo Entity;

        internal EntityMatch(EntityInfo entity, int position, int length)
        {
            Length = length;
            Entity = entity;
            Position = position;
        }
    }

    class EntityIndex
    {
        private readonly List<EntityMatch>[] _matches;

        private readonly string[] _words;

        internal EntityIndex(string[] words, UtteranceLinker linker)
        {
            _words = words.ToArray();
            _matches = new List<EntityMatch>[_words.Length];

            for (var i = 0; i < _matches.Length; ++i)
            {
                _matches[i] = new List<EntityMatch>();
            }

            initialize(linker);
        }

        private void initialize(UtteranceLinker linker)
        {
            for (var wordIndex = 0; wordIndex < _words.Length; ++wordIndex)
            {
                var missingWords = _words.Skip(wordIndex);
                for (var ngramLength = 1; ngramLength <= 4; ++ngramLength)
                {
                    if (wordIndex + ngramLength > _words.Length)
                        break;

                    var ngram = string.Join(" ", missingWords.Take(ngramLength));
                    foreach (var entity in linker.GetValidEntities(ngram))
                    {
                        var match = new EntityMatch(entity, wordIndex, ngramLength);
                        _matches[wordIndex].Add(match);
                    }
                }
            }
        }

        internal IEnumerable<LinkedUtterance> FindLinkedUtterancesTop(int n)
        {
            throw new NotImplementedException();
        }

        internal LinkedUtterance LinkedUtterance_Hungry()
        {
            var usedMatches = new List<EntityMatch>();
            while (true)
            {
                EntityMatch bestMatch = null;
                foreach (var match in _matches.SelectMany(m => m))
                {
                    var hasCollision = usedMatches.Any(u => testCollision(match, u));

                    if (hasCollision)
                        continue;

                    // var bestScore = bestMatch == null ? 0 : bestMatch.Entity.InBounds + bestMatch.Entity.OutBounds;
                    // var currentScore = match.Entity.InBounds + match.Entity.OutBounds;

                    var bestScore = bestMatch == null ? 0 : bestMatch.Entity.Score;
                    var currentScore = match.Entity.Score;
                    if (bestScore < currentScore)
                        bestMatch = match;

                }
                if (bestMatch == null)
                    break;

                usedMatches.Add(bestMatch);
            }

            var linkedParts = new List<LinkedUtterancePart>();
            for (var i = 0; i < _words.Length; ++i)
            {
                var match = getIndexedMatch(i, usedMatches);
                if (match == null)
                {
                    linkedParts.Add(LinkedUtterancePart.Word(_words[i]));
                }
                else
                {
                    var ngram = string.Join(" ", _words.Skip(i).Take(match.Length));
                    var entities = new List<EntityInfo>();
                    foreach (var ambigMatch in _matches[i])
                    {
                        if (ambigMatch.Length == match.Length)
                        {
                            entities.Add(ambigMatch.Entity);
                        }
                    }


                    linkedParts.Add(LinkedUtterancePart.Entity(ngram, entities.ToArray()));
                    i += match.Length - 1;
                }
            }

            return new LinkedUtterance(linkedParts);
        }

        private static bool testCollision(EntityMatch match, EntityMatch u)
        {
            var first = u.Position < match.Position ? u : match;
            var second = first == match ? u : match;

            var beginingOverlap = first.Position < second.Position && first.Position + first.Length > second.Position;
            var borderOverlap = first.Position == second.Position || first.Position + first.Length == second.Position + 1;

            return beginingOverlap || borderOverlap;
        }

        private EntityMatch getIndexedMatch(int beginingIndex, IEnumerable<EntityMatch> matches)
        {
            foreach (var match in matches)
            {
                if (match.Position == beginingIndex)
                    return match;
            }

            return null;
        }
    }
}
