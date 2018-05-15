using KnowledgeDialog.DataCollection;
using KnowledgeDialog.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PerceptiveDialogBasedAgent.V4;
using KnowledgeDialog.Dialog.Responses;
using System.IO;

namespace PerceptiveDialogBasedAgent
{
    public class PhraseAgentManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        private bool _hadInformativeInput = false;

        private int _inputCount = 0;

        public bool HadInformativeInput => _hadInformativeInput;

        public bool CanBeCompleted => true;

        private readonly Agent _agent = new Agent();

        public override ResponseBase Initialize()
        {
            return new SimpleResponse("Hello, how can I help you?");
        }

        public override ResponseBase Input(ParsedUtterance utterance)
        {
            //Database.DebugTrigger(849);
            string response;
            try
            {
                _inputCount += 1;
                if (_inputCount > 10)
                {
                    response = "[NOTE] Too much inputs. The task should be easier - use simple phrases. Type reset to start the dialog from beginning.";
                }
                else
                {
                    response = _agent.Input(utterance.OriginalSentence);
                    if (response.ToLowerInvariant().Contains("ceasar"))
                        _hadInformativeInput = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                File.AppendAllText("phrase_agent_manager.exceptions", DateTime.Now + "\n" + ex.ToString() + "\n\n\n\n");

                response = "[ERROR] The bot encountered an unexpected error - type reset and try to do the dialog differently.";
            }

            return new SimpleResponse(response);
        }
    }
}
