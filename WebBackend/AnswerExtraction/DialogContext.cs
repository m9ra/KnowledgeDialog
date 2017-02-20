using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Dialog.Acts;
using KnowledgeDialog.Dialog.Parsing;

using KnowledgeDialog.DataCollection.MachineActs;

namespace WebBackend.AnswerExtraction
{
    enum CompletitionStatus { None, Useful, NotUseful };

    class DialogContext
    {

        internal readonly SLUFactory Factory;

        internal readonly IModel HandlingModel;
        
        internal readonly QuestionInfo Topic;

        internal readonly ResponseBase ContextQuestion;

        internal bool HadInformativeInput;

        internal ResponseBase LastMachineOutput { get; private set; }

        internal ResponseBase NextMachineOutput { get; private set; }

        internal CompletitionStatus CompletitionStatus = CompletitionStatus.None;

        internal bool IsComplete { get { return CompletitionStatus != CompletitionStatus.None; } }

        internal ParsedUtterance UserInput { get; private set; }

        internal DialogActBase BestUserInputAct { get { return Factory.GetBestDialogAct(UserInput); } }

        internal bool IsChitChat { get { return BestUserInputAct is ChitChatAct; } }

        internal bool IsDontKnow { get { return BestUserInputAct is DontKnowAct; } }

        internal bool HasNegation { get { return BestUserInputAct is NegateAct; } }

        internal bool HasAffirmation { get { return BestUserInputAct is AffirmAct; } }

        internal bool IsQuestionOnInput { get { return UserInput.OriginalSentence.Contains("?") || BestUserInputAct is QuestionAct; } }

        private readonly Dictionary<string, object> _storedValues = new Dictionary<string, object>();

        internal DialogContext(QuestionInfo topic, ResponseBase contextQuestion, IModel handlingModel, SLUFactory factory)
        {
            HandlingModel = handlingModel;
            Factory = factory;
            Topic = topic;
            ContextQuestion = contextQuestion;
            NextMachineOutput = contextQuestion;
        }

        internal bool GetBool(string variableName)
        {
            return GetValue<bool>(variableName);
        }

        internal void RegisterNextOutput(MachineActionBase nextOutput)
        {
            NextMachineOutput = nextOutput;
        }

        internal void RegisterInput(ParsedUtterance input)
        {
            UserInput = input;
            LastMachineOutput = NextMachineOutput;
            NextMachineOutput = null;
            HadInformativeInput = false;
        }

        internal T SetValue<T>(string name, T value)
        {
            _storedValues[name] = (object)value;
            return value;
        }

        internal T Initialize<T>(string name, T value)
        {
            if (!_storedValues.ContainsKey(name))
                SetValue(name, value);

            return GetValue<T>(name);
        }

        internal T GetValue<T>(string name)
        {
            if (!_storedValues.ContainsKey(name))
                return default(T);

            return (T)_storedValues[name];
        }
    }
}
