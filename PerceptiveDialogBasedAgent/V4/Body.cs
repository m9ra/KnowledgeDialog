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

        private readonly List<Concept> _concepts = new List<Concept>();

        internal IEnumerable<Concept> Concepts => _concepts;

        private Concept _currentConcept = null;

        internal Body()
        {
            var model = new HandcraftedGenerator(this);
            _beam = new StateBeam(model);

            this
            .Concept("yes", _nativeValue)
                .Description("positive answer to a question")

            .Concept("no", _nativeValue)
                .Description("negative answer to a question")

            .Concept("print", _print)
                .Description("it is an action")
                .Description("alias to say")
            ;
        }

        internal void Input(string phrase)
        {
            var words = phrase.Split(' ');
            foreach (var word in words)
            {
                _beam.ExpandBy(word);
            }
        }

        internal Body Concept(string conceptName, BodyAction action)
        {
            _currentConcept = new Concept(conceptName, action);
            _concepts.Add(_currentConcept);
            return this;
        }

        internal Body Description(string description)
        {
            _currentConcept.AddDescription(description);
            return this;
        }

        private void _print(BodyContext context)
        {
            if (!context.RequireParameter("What should be printed?", out var subject))
                return;

            context.SetValue("output", subject.Name);
        }

        private void _databaseSearch(BodyContext context)
        {
            if (!context.RequireParameter("Which database should I search in?", out var database, context.Databases))
                return;

            var allCriterions = context.GetCriterions(database);
            if (!context.RequireMultiParameter("Which criterions should be used for the database search?", out var selectedCriterions, allCriterions))
                return;

            throw new NotImplementedException("Add the real search as a callback to context");
        }

        private void _nativeValue(BodyContext context)
        {
            //native values does not need to process context anyhow
        }
    }
}
