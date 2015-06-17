using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    class StateProperty
    {
        internal static readonly string TrueValue = "$true";

        internal static readonly string FalseValue = "$false";

        internal readonly string PropertyName;

        internal StateProperty()
        {
        }

        internal StateProperty(string propertyName)
        {
            PropertyName = propertyName;
        }

        internal static string ToPropertyValue(bool boolValue)
        {
            return boolValue ? TrueValue : FalseValue;
        }

        public override string ToString()
        {
            if (PropertyName == null)
            {
                return base.ToString();
            }
            else
            {
                return "[StateProperty]" + PropertyName;
            }
        }
    }
}
