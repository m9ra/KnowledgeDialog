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

        internal override TaskInstance CreateInstance(int taskId, int taskIndex, int validationCode)
        {
            var taskInstance = base.CreateInstance(taskId, taskIndex, validationCode);

            return new CollectionTaskInstance(taskIndex, taskInstance.TaskFormat, taskInstance.Substitutions, taskInstance.ExpectedAnswers, taskInstance.Key, taskInstance.ValidationCodeKey);
        }
    }
}
