using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Task.President
{
    class PresidentCollectionTaskFactory : PresidentTaskFactory
    {
        internal PresidentCollectionTaskFactory()
            : base(false)
        {

        }

        internal override TaskInstance CreateInstance(int taskIndex, int validationCode)
        {
            var taskInstance = base.CreateInstance(taskIndex, validationCode);

            return new CollectionTaskInstance(taskInstance.TaskFormat, taskInstance.Substitutions, taskInstance.ExpectedAnswers, taskInstance.Key, taskInstance.ValidationCode);
        }
    }
}
