using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V2.Modules
{
    class ExternalDatabaseProviderModule : BodyModuleBase
    {
        private ModuleField<string> SpecifiedSlot;

        private ModuleField<string> SpecifiedValue;

        private ModuleField<int> CountValue;

        private readonly DatabaseHandler _externalDatabase;

        private readonly string _databaseName;

        internal ExternalDatabaseProviderModule(string databaseName, DatabaseHandler externalDatabase)
        {
            _databaseName = databaseName;
            _externalDatabase = externalDatabase ?? throw new ArgumentNullException();
        }

        protected override void initializeAbilities()
        {
            SpecifiedSlot = newField<string>("specifiedSlot", _specifiedSlot);
            SpecifiedValue = newField<string>("specifiedValue", _specifiedValue);
            CountValue = newField<int>("countValue", _countValue);

            this
            .AddAbility("set " + _databaseName + " specifier $specifier").CallAction(_ability_setSpecifier)
                .Param(SpecifiedSlot, "$specifier")
                .Param(SpecifiedValue, "$specifier")

            .AddAnswer("value of $slot from " + _databaseName + " database", Question.HowToEvaluate).Call(_value_slotValue)
                .Param(SpecifiedSlot, "$slot")

            .AddAnswer(_databaseName + " database has $count result", Question.IsItTrue).Call(_value_resultCountCondition)
                .Param(CountValue, "$count")
            ;

            // register all values
            foreach (var column in _externalDatabase.Columns)
            {
                Container.AddSpanElement(column);
                foreach (var value in _externalDatabase.GetColumnValues(column))
                {
                    Container.AddSpanElement(value);
                    Container.Add(SemanticItem.From(Question.WhatItSpecifies, column, Constraints.WithInput(value)));
                }
            }
        }

        private SemanticItem _specifiedValue(ModuleContext context)
        {
            var slot = context.Value(SpecifiedSlot);
            if (slot == null)
                return null;

            var values = _externalDatabase.GetColumnValues(slot);
            return context.ChooseOption(Question.HowToParaphrase, values);
        }

        private SemanticItem _specifiedSlot(ModuleContext context)
        {
            return context.ChooseOption(Question.WhatItSpecifies, _externalDatabase.Columns);
        }

        private void _ability_setSpecifier(string slot, string value)
        {
            _externalDatabase.SetCriterion(slot, value);
            if (_externalDatabase.IsUpdated)
            {
                _externalDatabase.IsUpdated = false;
                FireEvent(_databaseName + " database was updated");
            }
        }

        private SemanticItem _countValue(ModuleContext context)
        {
            var numberValue = context.Input;
            if (!int.TryParse(numberValue, out var number))
            {
                numberValue = context.GetAnswer(Question.HowToConvertItToNumber)?.Answer;
                if (numberValue == null)
                    return null;

                if (!int.TryParse(numberValue, out number))
                    return null;
            }


            return SemanticItem.Entity(number.ToString());
        }

        private SemanticItem _value_resultCountCondition(int number)
        {
            var count = _externalDatabase.ResultCount;

            return count == number ? SemanticItem.Yes : SemanticItem.No;
        }

        private SemanticItem _value_slotValue(string slot)
        {
            return SemanticItem.Entity(_externalDatabase.Read(slot));
        }
    }
}
