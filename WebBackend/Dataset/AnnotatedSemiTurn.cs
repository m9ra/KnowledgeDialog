using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Experiment;

namespace WebBackend.Dataset
{
    class AnnotatedSemiTurn
    {
        public readonly bool IsRegularTurn;

        public readonly bool IsMachineAction;

        public readonly string SemiTurnText;

        public readonly DateTime Time;

        private readonly AnnotatedActionEntry _entry;

        internal AnnotatedSemiTurn(AnnotatedActionEntry entry)
        {
            _entry = entry;

            var type = entry.Type;
            switch (type)
            {
                case "T_utterance":
                    IsRegularTurn = true;
                    SemiTurnText = entry.Text;
                    break;
                case "T_response":
                    IsRegularTurn = true;
                    SemiTurnText = entry.Text;
                    break;
            }

            Time = entry.Time;  
        }

        public override string ToString()
        {
            return SemiTurnText;
        }

        internal Dictionary<string, object> GetRepresentation()
        {
            var representation = new Dictionary<string, object>();
            representation["Time"] = _entry.Time;
            representation["Text"] = _entry.Text;


            //TODO add annotation
            //TODO add id
            //TODO add slu, machine action,...
            return representation;
        }
    }
}
