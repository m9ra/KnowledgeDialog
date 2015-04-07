using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.TaskPatterns;

namespace WebBackend
{
    static class TaskFactory
    {
        private static readonly Random _rnd = new Random(1);

        private static readonly List<TaskPatternBase> _tasks = new List<TaskPatternBase>();

        static TaskFactory()
        {
            var g = DialogWeb.Graph;
            Add(new StateOfPresidentTask(g));
        }

        private static void Add(TaskPatternBase task)
        {
            _tasks.Add(task);
        }

        public static TaskInstance GetTask(UserTracker user)
        {
            foreach (var task in _tasks)
            {
                var key = getKey(task);
                if (!user.CompletedTasks.Contains(key))
                {
                    return createInstance(task, user);
                }
            }

            return null;
        }

        private static string getKey(TaskPatternBase task)
        {
            return task.GetType().ToString();
        }

        private static TaskInstance createInstance(TaskPatternBase task, UserTracker user)
        {
            var substitutionsCount = task.SubstitutionCount;
            var substitutionIndex = _rnd.Next(substitutionsCount);

            var substitution = task.GetSubstitution(substitutionIndex);
            var expectedAnswers = task.GetExpectedAnswers(substitutionIndex);

            return new TaskInstance(task.PatternFormat, new[] { substitution }, expectedAnswers, getKey(task), user);
        }
    }
}
