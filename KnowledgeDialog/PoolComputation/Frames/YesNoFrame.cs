using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KnowledgeDialog.PoolComputation.Frames
{
    class YesNoFrame : ConversationFrameBase
    {
        private readonly Action _yesHandler;

        private readonly Action _noHandler;

        private readonly string _question;

        internal YesNoFrame(ConversationContext context, string question, Action yesHandler, Action noHandler)
            : base(context)
        {
            _yesHandler = yesHandler;
            _noHandler = noHandler;

            _question = question;
        }

        protected override ModifiableResponse FrameInitialization()
        {
            return Response(_question);
        }

        protected override ModifiableResponse DefaultHandler()
        {
            var hasNo = CurrentInput.Contains("no");
            var hasYes = CurrentInput.Contains("yes");

            //TODO dont know

            if (!hasNo && !hasYes)
                return Response("I'm sorry, but I cannot understand you. Pleas answer simply 'yes' or 'no'");

            if (hasYes)
            {
                _yesHandler();
            }
            else if (hasNo)
            {
                _noHandler();
            }

            IsComplete = true;
            return null;
        }
    }
}
