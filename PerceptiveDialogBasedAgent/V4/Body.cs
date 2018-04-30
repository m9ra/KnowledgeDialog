using PerceptiveDialogBasedAgent.V2;
using PerceptiveDialogBasedAgent.V3;
using PerceptiveDialogBasedAgent.V4.Brain;
using PerceptiveDialogBasedAgent.V4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Body
    {
        private readonly StateBeam _beam;

        private readonly List<Concept2> _concepts = new List<Concept2>();

        internal ConceptInstance CurrentAgentInstance { get; private set; }

        internal IEnumerable<Concept2> Concepts => _concepts;

        private Concept2 _currentConcept = null;

        internal string CurrentInput { get; private set; }

        internal BodyState2 LastFinishedState { get; private set; }

        internal readonly DatabaseHandler RestaurantDb;

        internal readonly PointingModelBase Model;

        internal readonly ConceptInstance RootConcept;

        internal Body()
        {
            Log.Writeln("================================================================");
            RestaurantDb = V2.RestaurantAgent.CreateRestaurantDatabase();


            _concepts.AddRange(new[]
            {
                Concept2.Something,Concept2.Yes, Concept2.No, Concept2.Parameter, Concept2.NativeAction, Concept2.Subject
            });

            this
            .Concept("action")
                .SetProperty(Concept2.Parameter, Concept2.Yes)

            .Concept("policy", onPropertyChange: _policy)
                .SetProperty("action", Concept2.Something) //TODO set pattern

            .Concept("dont know")
                .Description("I have no idea")
                .Description("Im not sure")

            .Concept("current time")
                .Description("time on the system's clock")

            .Concept("find", onExecution: _findRestaurant)
                .SetProperty("subject", Concept2.Something)
                .Description("looking up restaurant")
                .Description("restaurant search")
                .Description("list")
                .Description("get")
                .Description("name some restaurants")

            .Concept("restaurant")
                .Description("restaurants")
                .Description("venue to eat")
                .Description("a place where food is served")

            .Concept("address")
                .Description("location")

            .Concept("print", _print)
                .SetProperty("subject", Concept2.Something)
                .Description("print something")
                .Description("say something")

            .Concept("what", _what)
                .Description("what is something of something")

            .Concept("agent")
                .Description("the bot")

            .Concept("location")
                .Description("the venue")
                .Description("some area")
                .Description("a place")

            .Concept("want")
                .Description("need")
                .Description("can")
                .Description("must have")
                .Description("I want to")
                .Description("I would like to have")
                .Description("desire to have something")

            .Concept("cost")
                .Description("how much money is something worth of")
                .Description("related to pricerange")

            .Concept("and")
                .Description("conjunction")

            .Concept("I")
                .Description("myself")
                .Description("me")

            .Concept("You")
                .Description("your")
                .Description("yourself")

            .Concept("It")
                .Description("the thing")

            .Concept("help")
                .Description("help me")
                .Description("I need help")

            .Concept("hello")
                .Description("same as hi")
                .Description("greeting")

            .Concept("please")
                .Description("a polite request")

            .Concept("determinant")
                .Description("a")
                .Description("an")
                .Description("the")

            .Concept("output")
                .Description("output property")

            .Concept("it")
                .Description("reference to a previous object")
                .Description("its")

            .Concept("unknown")
                .Description("not known concept")
            ;

            LastFinishedState = BodyState2.Empty();
            RootConcept = new ConceptInstance(GetConcept("policy"));

            Register(RestaurantDb, GetConcept("restaurant"));

            //Model = new HandcraftedModel(this);
            Model = new MindBasedModel(this);
            _beam = new StateBeam(this);
        }

        private Body SetProperty(string propertyName, string propertyValue)
        {
            var value = GetConcept(propertyValue);
            if (value == null)
                throw new InvalidOperationException("Cant find value " + propertyValue);

            return SetProperty(propertyName, new ConceptInstance(value));
        }

        private Body SetProperty(string propertyName, Concept2 propertyValue)
        {
            return SetProperty(propertyName, new ConceptInstance(propertyValue));
        }

        private Body SetProperty(string propertyName, ConceptInstance instance)
        {
            var property = GetConcept(propertyName);
            return SetProperty(property, instance);
        }

        private Body SetProperty(Concept2 property, Concept2 conceptValue)
        {
            _currentConcept.SetPropertyValue(property, new ConceptInstance(conceptValue));
            return this;
        }

        private Body SetProperty(Concept2 property, ConceptInstance instance)
        {
            _currentConcept.SetPropertyValue(property, instance);
            return this;
        }

        internal string Input(string phrase)
        {
            var words = Phrase.AsWords(phrase);
            if (words.Length > 10)
                return "I'm sorry, the sentence is too long. Try to use simpler phrases please.";

            CurrentInput = phrase;
            CurrentAgentInstance = new ConceptInstance(GetConcept("agent"));

            Log.DialogUtterance("U: " + phrase);
            var bestState = InputTransition(_beam.BodyStates, phrase);
            var output = Model.StateReaction(bestState, out var finalState);
            Log.DialogUtterance("S: " + output);

            _beam.SetBeam(finalState);

            LastFinishedState = finalState;
            CurrentInput = null;

            return output;
        }

        internal BodyState2 InputTransition(IEnumerable<BodyState2> initialStates, string phrase)
        {
            _beam.SetBeam(initialStates);
            var words = Phrase.AsWords(phrase);
            foreach (var word in words)
            {
                _beam.ExpandBy(word);
            }

            var bestState = _beam.BestState;
            return bestState;
        }

        internal void Register(DatabaseHandler database, Concept2 rowClass = null)
        {
            var properties = new Dictionary<string, Concept2>();
            var rowClassInstance = rowClass == null ? null : new ConceptInstance(rowClass);
            foreach (var row in database.GetDump())
            {
                Concept("the " + row["name"])
                    .Description("row of a database");

                var rowConcept = _currentConcept;

                foreach (var valuePair in row)
                {
                    var value = valuePair.Value;
                    var column = valuePair.Key;

                    if (!properties.TryGetValue(column, out var propertyConcept))
                    {
                        Concept(column)
                            .Description("column of a database")
                            .Description("property")
                            ;

                        properties[column] = propertyConcept = _currentConcept;
                    }

                    Concept(value)
                        .Description("value of " + column)
                        ;

                    //TODO think about instance creation
                    rowConcept.SetPropertyValue(propertyConcept, new ConceptInstance(_currentConcept));
                    if (rowClass != null)
                        rowConcept.SetPropertyValue(Concept2.InstanceOf, rowClassInstance);
                }
            }

            foreach (var column in database.Columns)
            {
                Concept(column)
                    .Description("column of a database");

                var columnInstance = new ConceptInstance(_currentConcept);

                foreach (var value in database.GetColumnValues(column))
                {
                    var relevantConcepts = _concepts.Where(c => c.Name == value).ToArray();
                    if (relevantConcepts.Any())
                        continue;

                    Concept(value)
                        .Description("property of " + column);
                }
            }
        }

        internal Concept2 GetConcept(string conceptName)
        {
            var result = _concepts.Where(c => c.Name == conceptName).ToArray();
            if (result.Length > 1)
                throw new NotImplementedException("Concept was not found.");

            return result.FirstOrDefault();
        }

        internal Body Concept(string conceptName, MindAction onParametersComplete = null, MindAction onExecution = null, MindAction onPropertyChange = null, bool isNative = true)
        {
            var existingConcept = GetConcept(conceptName);
            if (existingConcept == null)
            {
                _currentConcept = new Concept2(conceptName, onParametersComplete, onExecution, onPropertyChange, isNative);
                _concepts.Add(_currentConcept);
                Model?.OnConceptChange();
            }
            else
            {
                _currentConcept = existingConcept;
            }

            if (onParametersComplete != null || onPropertyChange != null || onExecution != null)
                _currentConcept.SetPropertyValue(Concept2.NativeAction, new ConceptInstance(Concept2.Something));

            return this;
        }

        internal Body Description(string description)
        {
            _currentConcept.AddDescription(description);
            //Model.OnConceptChange();
            return this;
        }

        private void _policy(MindEvaluationContext context)
        {
            var action = context.GetParameter(GetConcept("action"));
            if (action == null)
                throw new NotSupportedException("In which cases this could happen?");

            context.SetProperty(Concept2.CompleteAction, action);
        }

        private void _print(MindEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        private void _what(MindEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        private void _findRestaurant(MindEvaluationContext context)
        {
            var pattern = context.GetParameter(Concept2.Subject) as ConceptInstance;
            var patternConcept = pattern?.Concept;
            if (patternConcept == null)
                throw new NotImplementedException();

            if (patternConcept != Concept2.Something)
            {
                var properties = context.GetPropertiesUsedFor(patternConcept);
                if (properties.Count() != 1)
                    throw new NotImplementedException("Split into multiple states");

                var property = properties.First();
                //we expect top concept to be an empty placeholder
                var newPattern = new ConceptInstance(Concept2.Something);
                context.SetProperty(newPattern, property, pattern);
                context.SetProperty(Concept2.Subject, newPattern);
                pattern = newPattern;
            }

            var result = new HashSet<Concept2>();
            foreach (var concept in Concepts)
            {
                var instance = new ConceptInstance(concept);
                if (context.MeetsPattern(instance, pattern))
                    result.Add(concept);
            }

            if (result.Count == 0)
            {
                context.Event(Concept2.NotFoundEvent, Concept2.Subject, pattern);
            }
            else if (result.Count == 1)
            {
                context.OutputEvent(new ConceptInstance(result.First()));
            }
            else
            {
                var ambiguousEvt = new ConceptInstance(Concept2.NeedsRefinement);
                context.SetProperty(ambiguousEvt, Concept2.Target, pattern);

                context.Event(ambiguousEvt);
            }
        }

        private IEnumerable<string> getCriterionColumns(PointableInstance criterion, DatabaseHandler database)
        {
            var result = new HashSet<string>();
            foreach (var column in database.Columns)
            {
                foreach (var value in database.GetColumnValues(column))
                {
                    if (value == criterion.ToPrintable())
                        result.Add(column);
                }
            }

            return result;
        }

        private void _nativeValue(BodyContext2 context)
        {
            //native values does not need to process context anyhow
        }
    }
}
