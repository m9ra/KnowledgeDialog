using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

using WebBackend.TaskPatterns;

namespace WebBackend
{
    static class TaskFactory
    {
        private static readonly List<TaskPatternBase> _tasks = new List<TaskPatternBase>();

        private static readonly List<Tuple<TaskPatternBase, int>> _validTasks = new List<Tuple<TaskPatternBase, int>>();

        static TaskFactory()
        {
            var g = Program.Graph;
            Add(new PresidentChildrenTask(g));
            Add(new PresidentOfStateTask(g));
            Add(new StateOfPresidentTask(g));
            Add(new WifeOfPresidentTask(g));

            GenerateTaskPairs();
        }

        private static void GenerateTaskPairs()
        {
            foreach (var task in _tasks)
            {
                for (var i = 0; i < task.SubstitutionCount; ++i)
                {
                    var substitution = task.GetSubstitution(i);
                    var answers = task.GetExpectedAnswers(i);
                    if (!answers.Any())
                        //there is no valid answer - we will skip this task
                        continue;

                    _validTasks.Add(Tuple.Create(task, i));
                }
            }
        }

        private static void Add(TaskPatternBase task)
        {
            _tasks.Add(task);
        }

        public static TaskInstance GetTask(int seed, UserTracker user, bool hasTaskLimit)
        {
            var rnd = new Random(seed);
            var availableTasks = new HashSet<Tuple<TaskPatternBase, int>>(_validTasks);

            while (availableTasks.Count > 0)
            {
                var rndIndex = rnd.Next(_validTasks.Count);
                var taskPair = _validTasks[rndIndex];
                if (!availableTasks.Remove(taskPair))
                    continue;

                var key = getKey(taskPair.Item1);
                if (!user.CompletedTasks.Contains(key))
                {
                    return createInstance(taskPair, user);
                }
            }

            if (!hasTaskLimit)
            {
                var rndIndex = rnd.Next(_validTasks.Count);
                return createInstance(_validTasks[rndIndex], user);
            }

            return null;
        }

        private static string getKey(TaskPatternBase task)
        {
            return task.GetType().ToString();
        }

        private static TaskInstance createInstance(Tuple<TaskPatternBase, int> taskPair, UserTracker user)
        {
            var task = taskPair.Item1;
            var substitutionIndex = taskPair.Item2;

            var substitution = task.GetSubstitution(substitutionIndex);
            var expectedAnswers = task.GetExpectedAnswers(substitutionIndex);

            return new TaskInstance(task.PatternFormat, new[] { substitution }, expectedAnswers, getKey(task), user);
        }
    }
}
