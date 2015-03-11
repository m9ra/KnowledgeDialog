using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    public class SentenceParser
    {
        //   private readonly Dictionary<string, List<string>> _indexedEntities = new Dictionary<string, List<string>>();

        private readonly DoubleMetaphone _metaphone = new DoubleMetaphone();

        private StringSearch _searcher;

        private List<string> _allEntities = new List<string>();

        internal void AddEntity(string entity)
        {
            /*   var indexCode = getCode(entity);

               List<string> entities;
               if (!_indexedEntities.TryGetValue(indexCode, out entities))
                   _indexedEntities[indexCode] = entities = new List<string>();

               if (entities.Contains(entity))
                   //nothing to do
                   return;

               entities.Add(entity);*/


            _allEntities.Add(entity);
            //searcher has to be rebuilt
            _searcher = null;
        }

        internal ParsedSentence Parse(string sentence)
        {
            var validEntities = findEntities(sentence);
            return parseSentence(sentence, validEntities);
        }

        private List<StringSearchResult> findEntities(string sentence)
        {
            var searcher = getSearcher();
            var foundEntities = searcher.FindAll(sentence);


            //try to find longest entities as posible
            var validEntities = new List<StringSearchResult>();
            foreach (var foundEntity in foundEntities.OrderByDescending(e => e.Keyword.Length))
            {
                //optimistic claim
                var isValid = true;
                foreach (var validEntity in validEntities)
                {
                    //try to proove invalidity
                    var startIndex = foundEntity.Index;
                    var endIndex = startIndex + foundEntity.Keyword.Length;

                    var validStartIndex = validEntity.Index;
                    var validEndIndex = validStartIndex + validEntity.Keyword.Length;

                    var startBefore = startIndex < validStartIndex;
                    var endBefore = endIndex < validStartIndex;

                    var startAfter = startIndex > validEndIndex;
                    var endAfter = endIndex > validEndIndex;

                    isValid = isValid && ((startBefore == endBefore) && (startAfter == endAfter) && (startBefore || startAfter));
                }

                if (isValid)
                    validEntities.Add(foundEntity);
            }
            return validEntities;
        }

        private static ParsedSentence parseSentence(string sentence, List<StringSearchResult> validEntities)
        {
            var parsedWords = new List<string>();

            var currentEntityIndex = 0;
            var currentIndex = 0;
            while (currentIndex < sentence.Length)
            {
                var nextEntityStart = sentence.Length;
                var hasEntity = currentEntityIndex < validEntities.Count;
                if (hasEntity)
                {
                    nextEntityStart = validEntities[currentEntityIndex].Index;
                }

                var currentSentencePart = sentence.Substring(currentIndex, nextEntityStart - currentIndex);
                parsedWords.AddRange(currentSentencePart.Split(' '));
                if (hasEntity)
                {
                    var entity = validEntities[currentEntityIndex];
                    parsedWords.Add(entity.Keyword);
                    currentIndex = nextEntityStart + entity.Keyword.Length;
                }
            }

            return new ParsedSentence(parsedWords);
        }

        private string getCode(string entity)
        {
            _metaphone.ComputeKeys(entity);
            return _metaphone.PrimaryKey;
        }

        private StringSearch getSearcher()
        {
            if (_searcher == null)
                //new searcher has to be built
                _searcher = new StringSearch(_allEntities.ToArray());

            return _searcher;
        }
    }
}
