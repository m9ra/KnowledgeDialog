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
using KnowledgeDialog.Knowledge;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.EventBeam;

namespace PerceptiveDialogBasedAgent
{
    public enum OutputRecognitionAlgorithm { CeasarPalacePresence, NewBombayProperty, BombayPresenceOrModerateSearchFallback };

    public class PhraseAgentManager : CollectionManagerBase, IInformativeFeedbackProvider
    {
        internal readonly static int MinKnowledgeConfirmationCount = 3;

        private bool _hadInformativeInput = false;

        private int _inputCount = 0;

        public bool HadInformativeInput => _hadInformativeInput;

        public bool CanBeCompleted => true;

        private readonly Agent _agent = new Agent();

        private readonly HashSet<string> _exportedKnowledge = new HashSet<string>();

        private readonly OutputRecognitionAlgorithm _recognitionAlgorithm;

        private VoteContainer<object> _knowledge;

        private bool _exportKnowledge = true;

        private bool _useKnowledge = false;

        public int SuccessCode { get; private set; }

        public PhraseAgentManager(OutputRecognitionAlgorithm recognitionAlgorithm, VoteContainer<object> knowledge, bool exportKnowledge, bool useKnowledge)
        {
            SuccessCode = 1;
            _knowledge = knowledge;
            _exportKnowledge = exportKnowledge;
            _useKnowledge = useKnowledge;
            _recognitionAlgorithm = recognitionAlgorithm;

            foreach (var itemVotes in _knowledge.ItemsVotes)
            {
                if (!_useKnowledge)
                    // knowledge won't be used
                    break;

                if (itemVotes.Value.Positive >= MinKnowledgeConfirmationCount)
                    _agent.AcceptKnowledge(itemVotes.Key as EventBase);
            }
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
                    var turnStartNode = _agent.LastBestNode;
                    response = _agent.Input(utterance.OriginalSentence);
                    var turnEndNode = _agent.LastBestNode;

                    if (hasInformativeInput())
                        _hadInformativeInput = true;

                    if (_exportKnowledge)
                        voteForExportedEvents(turnStartNode, turnEndNode);
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

        private void voteForExportedEvents(BeamNode turnStart, BeamNode turnEnd)
        {
            var currentNode = turnEnd;
            while (currentNode != null)
            {
                if (currentNode == turnStart)
                    break;

                try
                {
                    if (!(currentNode.Evt is V4.Events.ExportEvent export))
                        continue;

                    var exportRepresentation = export.ExportedEvent.ToString();
                    if (_exportedKnowledge.Add(exportRepresentation))
                    {
                        //in case new information for this dialogue is given, log it
                        LogMessage(exportRepresentation);
                        _knowledge.Vote(export.ExportedEvent);
                    }
                }
                finally
                {
                    currentNode = currentNode.ParentNode;
                }
            }
        }

        private bool hasInformativeInput()
        {
            switch (_recognitionAlgorithm)
            {
                case OutputRecognitionAlgorithm.CeasarPalacePresence:
                    {
                        var lastOutput = _agent.LastOutput;
                        return lastOutput.ToLowerInvariant().Contains("caesar");
                    }

                case OutputRecognitionAlgorithm.BombayPresenceOrModerateSearchFallback:
                    {
                        if (containsBombay())
                            return true;

                        if (containsModerateSearch())
                        {
                            SuccessCode = -1;
                            return true;
                        }

                        return false;
                    }

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
                            var concept = propertySet.Target.Concept ?? propertySet.Target.Instance.Concept;
                            if (concept != null && concept.Name.ToLowerInvariant().Contains("bombay"))
                            {
                                SuccessCode = 2;
                                return true;
                            }
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

        private bool containsModerateSearch()
        {
            var currentNode = _agent.LastBestNode;

            var hadFind = false;
            var hadModerate = false;
            var hadRestaurant = false;
            while (currentNode != null && currentNode.Evt != null)
            {
                try
                {
                    var str = currentNode.Evt.ToString();
                    hadFind |= str.Contains("find");
                    hadModerate |= str.Contains("moderate");
                    hadRestaurant |= str.Contains("restaurant");

                    if (currentNode.Evt is TurnStartEvent)
                    {
                        if (hadFind && hadModerate && hadRestaurant)
                            return true;

                        //reset after turn passed
                        hadFind = false;
                        hadModerate = false;
                        hadRestaurant = false;
                    }
                }
                finally
                {
                    currentNode = currentNode.ParentNode;
                }
            }
            return false;
        }

        private bool containsBombay()
        {
            var lastOutput = _agent.LastOutput;
            return lastOutput.ToLowerInvariant().Contains("bombay");
        }
    }
}