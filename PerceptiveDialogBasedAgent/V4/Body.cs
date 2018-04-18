using PerceptiveDialogBasedAgent.V2;
using PerceptiveDialogBasedAgent.V3;
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

        internal readonly ConceptInstance OutputProperty;

        internal IEnumerable<Concept2> Concepts => _concepts;

        private Concept2 _currentConcept = null;

        internal string CurrentInput { get; private set; }

        internal BodyState2 LastFinishedState { get; private set; }

        internal readonly DatabaseHandler RestaurantDb;

        internal readonly PointingModelBase Model;

        internal Body()
        {
            Log.Writeln("================================================================");
            RestaurantDb = V2.RestaurantAgent.CreateRestaurantDatabase();
            Model = new HandcraftedModel(this);
            _beam = new StateBeam(this);

            this
            .Concept("yes")
                .Description("positive answer to a question")

            .Concept("no")
                .Description("negative answer to a question")

            .Concept("dont know")
                .Description("I have no idea")
                .Description("Im not sure")

            .Concept("current time")
                .Description("time on the system's clock")

            .Concept("find", _findRestaurant)
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
                .Description("an action")
                .Description("alias to say")

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

            OutputProperty = new ConceptInstance(GetConcept("output"));

            LastFinishedState = BodyState2.Empty();

            Register(RestaurantDb);
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

        internal void Register(DatabaseHandler database)
        {
            var properties = new Dictionary<string, Concept2>();
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
                    rowConcept.AddPropertyValue(propertyConcept, new ConceptInstance(_currentConcept));
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

        internal Body Concept(string conceptName, BodyAction2 action = null, bool isNative = true)
        {
            var existingConcept = GetConcept(conceptName);
            if (existingConcept == null)
            {
                _currentConcept = new Concept2(conceptName, action, isNative);
                _concepts.Add(_currentConcept);
                Model.OnConceptChange();
            }
            else
            {
                _currentConcept = existingConcept;
            }

            return this;
        }

        internal Body Description(string description)
        {
            _currentConcept.AddDescription(description);
            //Model.OnConceptChange();
            return this;
        }

        private void _print(BodyContext2 context)
        {
            if (!context.RequireParameter("What should be printed?", out var subject))
                return;

            context.SetValue(CurrentAgentInstance, OutputProperty, subject);
        }

        private void _what(BodyContext2 context)
        {
            var allProperties = context.GetProperties();
            if (!context.RequireParameter("What property?", out var property, allProperties))
                return;

            if (!context.RequireParameter(property.ToPrintable() + " of what ?", out var refferedInstance))
                return;

            var value = refferedInstance.GetPropertyValue(((ConceptInstance)property).Concept);
            if (value == null)
                return;

            context.AddScore(0.1);
            var responseInstance = Phrase.FromUtterance(property.ToPrintable() + " is " + value.First().ToPrintable());
            context.SetValue(CurrentAgentInstance, OutputProperty, responseInstance);
        }

        private void _findRestaurant(BodyContext2 context)
        {
            var database = this.RestaurantDb;
            var allCriterions = context.GetCriterions(database);
            if (!context.RequireParameter("I know many restaurants, which criteria should be used for filtering?", out var selectedCriterion, allCriterions))
                return;

            var columns = getCriterionColumns(selectedCriterion, database);
            if (columns.Count() != 1)
                throw new NotImplementedException("Disambiguate columns");

            database.ResetCriterions();
            database.SetCriterion(columns.First(), selectedCriterion.ToPrintable());
            var name = database.Read("name");
            Phrase result = Phrase.FromUtterance("I know restaurant called " + name);
            if (name == null)
                result = Phrase.FromUtterance("I don't know such a restaurant");

            //TODO refactor find to search between concepts
            var resultConcept = GetConcept("the " + name);
            context.Activate(new ConceptInstance(resultConcept));

            context.SetValue(CurrentAgentInstance, OutputProperty, result);
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
