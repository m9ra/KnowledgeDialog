using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog;
using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;

using KnowledgeDialog.Dialog.Acts;

using KnowledgeDialog.DataCollection.MachineActs;

namespace KnowledgeDialog.DataCollection
{
    public abstract class CollectionManagerBase : IInputDialogManager
    {
        /// <summary>
        /// Factory providing SLU parses of output.
        /// </summary>
        protected readonly SLUFactory Factory = new SLUFactory();

        /// <summary>
        /// Determine whether dialog is closed.
        /// </summary>
        protected bool IsDialogClosed = false;

        /// <summary>
        /// Keep track about denotations that has been asked.
        /// </summary>
        protected readonly HashSet<DenotationType> AskedDenotations = new HashSet<DenotationType>();

        /// <summary>
        /// Denotation which has been asked lastly.
        /// </summary>
        protected DenotationType LastDenotationQuestion = DenotationType.None;

        /// <summary>
        /// Words that does not give any explanatory information.
        /// </summary>
        protected readonly HashSet<string> NonInformativeWords = new HashSet<string>()
        {
            "of","for","a","to", "by","about","the","whether","what","how","who","which","when","why", "with", "without", "whatever", "whoever", "whenever", "however", "then", "than", "please", "correct", "whats", "what's", "s",

            "and", "or",

            "answer", "question", "questioner" ,"ask" ,"say", "saying" ,"asking",

            "wonder", "wondering", "know", "knowing", "want",

            "curios", "interested", "interesting", "interest", "answer", "answering", "look", "looking", "try", "trying", "find", "finding",

            "think", "thinks", "was", "were",

            "you", "it", "me", "i", "m", "we",

            "can", "could", "will", "would", "tell", "give", "name", "think", "thing"
        };


        /// <inheritdoc/>
        public abstract ResponseBase Initialize();

        /// <inheritdoc/>
        public abstract ResponseBase Input(ParsedUtterance utterance);

        protected ResponseBase Ask(DenotationType denotationType)
        {
            return Ask(denotationType, false);
        }

        protected ResponseBase AskAtLeast(DenotationType denotationType)
        {
            return Ask(denotationType, true);
        }

        protected ResponseBase AskForMissingFact()
        {
            return new BeMoreSpecificAct();
        }

        protected ResponseBase Ask(DenotationType denotationType, bool atLeastRequest)
        {
            AskedDenotations.Add(denotationType);
            LastDenotationQuestion = denotationType;
            switch (denotationType)
            {
                case DenotationType.CorrectAnswer:
                    return new RequestAnswerAct(atLeastRequest);

                case DenotationType.Explanation:
                    return new RequestExplanationAct(atLeastRequest);
            }

            throw new NotSupportedException(denotationType.ToString());
        }

        protected ResponseBase HandleChitChat(ChitChatAct act)
        {
            var domain = act.Domain;
            if (domain == ChitChatDomain.Bye)
            {
                //ending dialog is actually not simple chit chat
                IsDialogClosed = true;
                return new ByeAct();
            }

            return new ChitChatAnswerAct(domain);
        }

        protected DenotationType GetNonaskedDenotationType()
        {
            foreach (DenotationType denotationType in new[] { DenotationType.Explanation, DenotationType.CorrectAnswer })
            {
                if (!AskedDenotations.Contains(denotationType))
                    //we have denotation that could be asked
                    return denotationType;
            }

            return DenotationType.None;
        }
    }
}
