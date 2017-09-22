using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2
{
    public class Database
    {
        private readonly List<SemanticItem> _data = new List<SemanticItem>();

        internal IEnumerable<SemanticItem> Query(SemanticItem queryItem)
        {
            if (queryItem.Question == null)
                throw new NotImplementedException();

            if (queryItem.Answer != null)
                throw new NotImplementedException();

            var result = new List<SemanticItem>();
            foreach (var item in _data)
            {
                if (item.Question != queryItem.Question)
                    continue;

                //TODO resolve constraints
                result.Add(item);
            }
            return result;
        }
    }
}
