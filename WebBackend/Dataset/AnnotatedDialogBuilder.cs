using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using WebBackend.Experiment;

namespace WebBackend.Dataset
{
    class AnnotatedBuilderContext
    {
        internal string TaskType;

        internal string SubstitutionData;
    }

    class AnnotatedDialogBuilder
    {
        private List<AnnotatedSemiTurn> _questionTurns = new List<AnnotatedSemiTurn>();

        private List<AnnotatedSemiTurn> _explanationTurns = new List<AnnotatedSemiTurn>();

        private List<AnnotatedSemiTurn> _answerTurns = new List<AnnotatedSemiTurn>();

        private readonly AnnotatedBuilderContext _context;

        private bool _isQuestionComplete;

        private bool _isExplanationComplete;

        internal bool HasValidDialog
        {
            get
            {
                return _questionTurns.Count > 1;
            }
        }

        internal bool IsDialogEnd { get; private set; }

        internal AnnotatedDialogBuilder(AnnotatedBuilderContext builder)
        {
            _context = builder;
        }

        internal void Register(AnnotatedActionEntry action)
        {
            if (IsDialogEnd)
                throw new NotSupportedException("Cannot add more actions to closed dialog");

            if (action.IsReset)
            {
                IsDialogEnd = true;
                return;
            }


            if (action.Type == "T_task")
            {
                //we have a task description action
                _context.TaskType = action.Entry.Data["task"].ToString();
                var substitutionArray = action.Entry.Data["substitutions"] as JArray;
                if (substitutionArray.Count > 1)
                    throw new NotSupportedException("We currently doesn't support multiple substitutions");

                _context.SubstitutionData = substitutionArray[0].ToString();
            }

            var turn = new AnnotatedSemiTurn(action);
            if (!turn.IsRegularTurn)
                //we are interested only in regular turns
                return;

            var actDescription = action.Act;
            if (actDescription == null)
                actDescription = "null";

            if (actDescription.Contains("RequestExplanation"))
            {
                //after explanation is requested we are no more collecting question turns
                _isQuestionComplete = true;
            }

            if (actDescription.Contains("RequestAnswer"))
            {
                _isQuestionComplete = true;
                _isExplanationComplete = true;
            }

            if (!_isQuestionComplete)
            {
                //collect turns for question                
                _questionTurns.Add(turn);
            }
            else if (!_isExplanationComplete)
            {
                //collect turns for explanation
                _explanationTurns.Add(turn);
            }
            else
            {
                _answerTurns.Add(turn);
            }
        }

        internal AnnotatedDialog Build()
        {
            return new AnnotatedDialog(_questionTurns, _explanationTurns, _answerTurns, _context.TaskType, _context.SubstitutionData);
        }

        static internal IEnumerable<AnnotatedDialog> ParseDialogs(AnnotatedLogFile log)
        {
            var actions = log.LoadActions().ToArray();
            var builders = new List<AnnotatedDialogBuilder>();

            //fill builders with dialog data
            var builderContext = new AnnotatedBuilderContext();
            AnnotatedDialogBuilder currentBuilder = null;
            foreach (var action in actions)
            {
                if (currentBuilder == null)
                {
                    currentBuilder = new AnnotatedDialogBuilder(builderContext);
                    builders.Add(currentBuilder);
                }

                currentBuilder.Register(action);

                if (currentBuilder.IsDialogEnd)
                    currentBuilder = null;
            }

            //build valid dialogs
            var dialogs = new List<AnnotatedDialog>();
            foreach (var builder in builders)
            {
                if (builder.HasValidDialog)
                    dialogs.Add(builder.Build());
            }

            return dialogs;
        }
    }
}
