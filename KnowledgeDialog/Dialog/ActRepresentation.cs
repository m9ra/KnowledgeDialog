using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.Dialog
{
    public class ActRepresentation
    {
        /// <summary>
        /// Name of represented act.
        /// </summary>
        public readonly string ActName;

        /// <summary>
        /// Values of parameters.
        /// </summary>
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public ActRepresentation(string actName)
        {
            ActName = actName;
        }

        /// <summary>
        /// Adds parameter to act representation.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        internal void AddParameter(string name, object value)
        {
            _parameters.Add(name, value);
        }

        /// <summary>
        /// Creates functional representation of dialog act.
        /// </summary>
        /// <returns>The created representation.</returns>
        public string ToFunctionalRepresentation()
        {
            var builder = new StringBuilder();
            var parameterDefinitions = new List<string>();

            foreach (var parameter in _parameters)
            {
                var value = parameter.Value;
                var name = parameter.Key;
                var valueRepresentation = value is string ? string.Format("'{0}'", value) : value;
                parameterDefinitions.Add(string.Format("{0}={1}", name, value));
            }

            builder.Append(ActName);
            builder.Append("(");
            builder.Append(string.Join(",", parameterDefinitions));
            builder.Append(")");
            return builder.ToString();
        }
    }
}
