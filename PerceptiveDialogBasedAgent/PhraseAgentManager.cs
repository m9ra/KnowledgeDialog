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
    public enum OutputRecognitionAlgorithm { CeasarPalacePresence, NewBombayProperty };

    public class PhraseAgentManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        private bool _hadInformativeInput = false;

        private int _inputCount = 0;

        public bool HadInformativeInput => _hadInformativeInput;

        public bool CanBeCompleted => true;

        private readonly Agent _agent = new Agent();

        private readonly OutputRecognitionAlgorithm _recognitionAlgorithm;

        public PhraseAgentManager(OutputRecognitionAlgorithm recognitionAlgorithm)
        {
            _recognitionAlgorithm = recognitionAlgorithm;
        }

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
                    if (hasInformativeInput())
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

        private bool hasInformativeInput()
        {
            switch (_recognitionAlgorithm)
            {
                case OutputRecognitionAlgorithm.CeasarPalacePresence:
                    var lastOutput = _agent.LastOutput;
                    return lastOutput.ToLowerInvariant().Contains("caesar");

                case OutputRecognitionAlgorithm.NewBombayProperty:
                    var currentNode = _agent.LastBestNode;
                    while (currentNode != null)
                    {
                        try
                        {
                            if (!(currentNode.Evt is V4.Events.ExportEvent export))
                                continue;

                            if (!(export.ExportedEvent is V4.Events.PropertySetEvent propertySet))
                                continue;

                            var instance = propertySet.Target.Instance;
                            if (instance != null && instance.Concept.Name.ToLowerInvariant().Contains("bombay"))
                                return true;
                        }
                        finally
                        {
                            currentNode = currentNode.ParentNode;
                        }
                    }
                    return false;

                default:
                    throw new NotImplementedException("Unknown recognition algorithm " + _recognitionAlgorithm);
            }
        }
    }
}