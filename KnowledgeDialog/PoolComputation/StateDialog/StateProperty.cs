using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeDialog.PoolComputation.StateDialog
{
    /// <summary>
    /// Represents strongly typed property of <see cref="DialogState"/>.
    /// </summary>
    /// <typeparam name="Type">Type of value the property operates with.</typeparam>
    class StateProperty<Type> 
    {
        internal readonly string Name;

        internal StateProperty(string name)
        {
            Name = name;
        }

        internal Type GetValue(Dictionary<object,object> propertyToValue) 
        {
            object value;
            if (!propertyToValue.TryGetValue(this, out value))
                return default(Type);

            return (Type)value;
        }

        internal void SetValue(Dictionary<object,object> propertyToValue, Type value)
        {
            propertyToValue[this] = value;  
        }
    }
}
