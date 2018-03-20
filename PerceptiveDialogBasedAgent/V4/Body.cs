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

        private ConceptInstance _currentAgentInstance;

        private readonly ConceptInstance _outputProperty;

        internal IEnumerable<Concept2> Concepts => _concepts;

        private Concept2 _currentConcept = null;

        internal Body()
        {
            var model = new HandcraftedModel(this);
            _beam = new StateBeam(model, this);

            this
            .Concept("yes", _nativeValue)
                .Description("positive answer to a question")

            .Concept("no", _nativeValue)
                .Description("negative answer to a question")

            .Concept("current time", _nativeValue)
                .Description("time on the system's clock")

            .Concept("print", _print)
                .Description("it is an action")
                .Description("alias to say")

            .Concept("agent", _nativeValue)
                .Description("the bot")

            .Concept("output", _nativeValue)
                .Description("output property")
            ;

            _outputProperty = new ConceptInstance(GetConcept("output"));
        }

        internal void Input(string phrase)
        {
            Log.DialogUtterance("U: " + phrase);

            _currentAgentInstance = new ConceptInstance(GetConcept("agent"));

            var words = phrase.Split(' ');
            foreach (var word in words)
            {
                _beam.ExpandBy(word);
            }

            var bestState = _beam.BestState;
            var finalState = stateReaction(bestState);

            _beam.ShrinkTo(finalState);
        }

        private BodyState2 stateReaction(BodyState2 state)
        {
            var outputValue = state.GetIndexValue(_currentAgentInstance, _outputProperty);
            if (outputValue == null)
                throw new NotImplementedException("What should agent do?");

            Log.DialogUtterance("S: " + outputValue.Concept.Name);
            return state;
        }

        internal Concept2 GetConcept(string conceptName)
        {
            var result = _concepts.Where(c => c.Name == conceptName).ToArray();
            if (result.Length != 1)
                throw new NotImplementedException("Concept was not found.");

            return result.First();
        }

        internal Body Concept(string conceptName, BodyAction2 action)
        {
            _currentConcept = new Concept2(conceptName, action);
            _concepts.Add(_currentConcept);
            return this;
        }

        internal Body Description(string description)
        {
            _currentConcept.AddDescription(description);
            return this;
        }

        private void _print(BodyContext2 context)
        {
            if (!context.RequireParameter("What should be printed?", out var subject))
                return;

            context.SetValue(_currentAgentInstance, _outputProperty, subject);
        }

        private void _databaseSearch(BodyContext2 context)
        {
            if (!context.RequireParameter("Which database should I search in?", out var database, context.Databases))
                return;

            var allCriterions = context.GetCriterions(database);
            if (!context.RequireMultiParameter("Which criterions should be used for the database search?", out var selectedCriterions, allCriterions))
                return;

            throw new NotImplementedException("Add the real search as a callback to context");
        }

        private void _nativeValue(BodyContext2 context)
        {
            //native values does not need to process context anyhow
        }
    }
}
