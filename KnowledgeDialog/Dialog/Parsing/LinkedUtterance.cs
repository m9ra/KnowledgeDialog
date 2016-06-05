using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog.Parsing
{
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

    public class LinkedUtterance
    {
        public readonly IEnumerable<LinkedUtterancePart> Parts;

        public LinkedUtterance(IEnumerable<LinkedUtterancePart> parts)
        {
            Parts = parts.ToArray();
        }

        public override string ToString()
        {
            return string.Join(" ", Parts);
        }
    }
}
