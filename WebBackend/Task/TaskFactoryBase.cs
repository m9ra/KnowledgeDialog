using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Task
{
    /// <summary>
    /// Base for task factories, that can be used by experiments for task creation.
    /// </summary>
    abstract class TaskFactoryBase
    {
        /// <summary>
        /// Creates instance of task at given index for given user.
        /// </summary>
        /// <param name="taskIndex">Index of task.</param>
        /// <param name="validationCode">Validation code that is reported after successful task completition.</param>
        /// <param name="user">User which owns the task.</param>
        /// <returns>Created task.</returns>
        abstract internal TaskInstance CreateInstance(int taskId ,int taskIndex, int validationCode);

        /// <summary>
        /// Total number of tasks provided by current factory.
        /// </summary>
        /// <returns>Total number of tasks.</returns>
        abstract internal int GetTaskCount();
    }
}
