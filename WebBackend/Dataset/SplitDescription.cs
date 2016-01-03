using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;

namespace WebBackend.Dataset
{
    class SplitDescription
    {
        /// <summary>
        /// Determine portion of data that has to be present in the split.
        /// </summary>
        internal readonly double SplitSizeRatio;

        /// <summary>
        /// Fullnames of tasks that are allowed for the split.
        /// </summary>
        private readonly HashSet<string> _allowedTaskTypes = new HashSet<string>();

        /// <summary>
        /// Substitutions that are allowed by this description.
        /// </summary>
        private readonly HashSet<string> _allowedSubstitutionsData = new HashSet<string>();

        /// <summary>
        /// Determine whether all substitutions are allowed for current split..
        /// </summary>
        private readonly bool _allowAllSubstitutions = true;

        /// <summary>
        /// Determine whether all task types are allowed for current split.
        /// </summary>
        private readonly bool _allowAllTaskTypes = false;

        private SplitDescription(double ratio, IEnumerable<string> allowedTasks)   
        {
            SplitSizeRatio = ratio;
            _allowedTaskTypes.UnionWith(allowedTasks);
        }

        /// <summary>
        /// Determine whether requirements set by this description are met.
        /// </summary>
        /// <param name="dialog">Dialog tested for the requirements.</param>
        /// <returns><c>true</c> whether requirements are met, <c>false</c> otherwise.</returns>
        internal bool MeetRequirements(AnnotatedDialog dialog)
        {
            if (!_allowAllTaskTypes)
                if (!_allowedTaskTypes.Contains(dialog.TaskType))
                    return false;

            if (!_allowAllSubstitutions)
                if (!_allowedSubstitutionsData.Contains(dialog.SubstitutionData))
                    return false;

            //no requirement has been violated
            return true;
        }

        /// <summary>
        /// Creates <see cref="SplitDescription"/> with given ratio.
        /// </summary>
        /// <returns>The split description.</returns>
        internal static SplitDescription Ratio(double ratio)
        {
            return new SplitDescription(ratio, new string[0]);
        }

        /// <summary>
        /// Creates <see cref="SplitDescription"/> with allowed task T.
        /// </summary>
        /// <typeparam name="T">Allowed type.</typeparam>
        /// <returns>The split description.</returns>
        internal SplitDescription Add<T>()
        {
            return new SplitDescription(SplitSizeRatio, _allowedTaskTypes.Concat(new string[] { typeof(T).FullName }));
        }
    }
}
