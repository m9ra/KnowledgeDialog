using PerceptiveDialogBasedAgent.Knowledge;
using PerceptiveDialogBasedAgent.SemanticRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent
{
    public class Agent
    {
        private readonly Database _database;

        private readonly Parser _parser;

        private string _channelId;

        internal Agent(Database database)
        {
            _database = database;
            _parser = new Parser(null);
        }

        public string StartSession(string channelId)
        {
            if (_channelId != null)
                throw new NotSupportedException("Cannot start session twice");

            _channelId = channelId;

            setGoal("Expression for dialog opening.");
            return currentTopic();
        }

        public string Input(string utterance)
        {

            throw new NotImplementedException();
        }

        private void setGoal(string goal)
        {
            var parses = _parser.Parse(goal);
            throw new NotImplementedException();
        }

        private string currentTopic()
        {
            throw new NotImplementedException();
        }
    }
}
