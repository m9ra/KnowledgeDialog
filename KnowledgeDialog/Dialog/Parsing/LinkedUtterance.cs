using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Parsing
{
    [Serializable]
    public class LinkedUtterancePart
    {
        public readonly string Token;

        public readonly IEnumerable<EntityInfo> Entities;

        private LinkedUtterancePart(string token, EntityInfo[] entityIds)
        {
            Token = token;
            Entities = entityIds;
        }

        public static LinkedUtterancePart Word(string word)
        {
            return new LinkedUtterancePart(word, new EntityInfo[0]);
        }

        public static LinkedUtterancePart Entity(string ngram, EntityInfo[] entities)
        {
            return new LinkedUtterancePart(ngram, entities.ToArray());
        }

        public override string ToString()
        {
            if (Entities.Any())
            {
                return string.Format("[{0}]", Token);
            }
            else
            {
                return Token;
            }
        }
    }

    [Serializable]
    public class LinkedUtterance
    {
        public readonly IEnumerable<LinkedUtterancePart> Parts;

        public IEnumerable<EntityInfo> Entities { get { return Parts.SelectMany(p => p.Entities).ToArray(); } }

        public LinkedUtterance(IEnumerable<LinkedUtterancePart> parts)
        {
            Parts = parts.ToArray();
        }

        public string GetEntityBasedRepresentation()
        {
            var result = new List<string>();
            foreach (var part in Parts)
            {
                result.Add(getPartRepr(part, true));
            }

            return string.Join(" ", result);
        }

        public IEnumerable<string> GetNgrams(int n)
        {
            var result = new List<string>();
            var parts = Parts.ToArray();
            for (var i = 0; i < parts.Length; ++i)
            {
                if (i + n >= parts.Length)
                    break;

                var ngram = new StringBuilder();
                for (var j = 0; j < n; ++j)
                {
                    var index = i + j;
                    var part = parts[i];

                    var word = index >= parts.Length ? "</s>" : getPartRepr(parts[index]);
                    if (ngram.Length > 0)
                        ngram.Append(",");

                    ngram.Append(word);
                }

                result.Add(ngram.ToString());
            }
            return result;
        }

        private static string getPartRepr(LinkedUtterancePart part, bool extended = false)
        {
            if (!part.Entities.Any())
                return part.Token;

            var entity = part.Entities.First();
            var id = entity.Mid;

            if (extended)
            {
                var match = entity.BestAliasMatch;
                if (match.Contains("]"))
                    throw new NotImplementedException("escape the match");

                return "[" + id + "-" + match + "]";
            }

            return "[" + id + "]";
        }

        public override string ToString()
        {
            return string.Join(" ", Parts);
        }
    }
}
