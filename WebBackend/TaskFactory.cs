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
        private static readonly List<TaskPatternBase> _tasks = new List<TaskPatternBase>();

        static TaskFactory()
        {
            var g = DialogWeb.Graph;
            Add(new PresidentChildrenTask(g));
            Add(new PresidentOfStateTask(g));
            Add(new StateOfPresidentTask(g));
            Add(new WifeOfPresidentTask(g));
        }

        private static void Add(TaskPatternBase task)
        {
            _tasks.Add(task);
        }

        public static TaskInstance GetTask(int seed, UserTracker user, bool hasTaskLimit)
        {
            var rnd = new Random(seed);

            foreach (var task in _tasks)
            {
                var key = getKey(task);
                if (!user.CompletedTasks.Contains(key))
                {
                    return createInstance(task, user, rnd);
                }
            }

            if (!hasTaskLimit)
            {
                var rndIndex = rnd.Next(_tasks.Count);
                var task = _tasks[rndIndex];

                return createInstance(task, user, rnd);
            }

            return null;
        }

        private static string getKey(TaskPatternBase task)
        {
            return task.GetType().ToString();
        }

        private static TaskInstance createInstance(TaskPatternBase task, UserTracker user, Random rnd)
        {
            var substitutionsCount = task.SubstitutionCount;
            var substitutionIndex = rnd.Next(substitutionsCount);

            var substitution = task.GetSubstitution(substitutionIndex);
            var expectedAnswers = task.GetExpectedAnswers(substitutionIndex);

            return new TaskInstance(task.PatternFormat, new[] { substitution }, expectedAnswers, getKey(task), user);
        }
    }
}
