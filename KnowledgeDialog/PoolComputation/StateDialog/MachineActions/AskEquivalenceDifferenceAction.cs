using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace KnowledgeDialog.PoolComputation.StateDialog.MachineActions
{
    class AskEquivalenceDifferenceAction : MachineActionBase
    {
        /// </inheritdoc>
        protected override bool CouldApply()
        {
            return InputState.HasNonAnsweredQuestion && InputState.HasEquivalenceCandidate && InputState.HasAffirmation && !InputState.DifferenceWordQuestioned;
        }

        /// </inheritdoc>
        protected override void Apply()
        {
            //in case of confirmed equivalence we will ask about difference word
            var word = getImportantDifferenceWord(InputState.Question, InputState.EquivalenceCandidate);
            EmitResponse("So you think that the word '" + word + "' is irrelevant?");
            SetDifferenceWordQuestion(true);
        }

        private string getImportantDifferenceWord(ParsedUtterance utterance1, ParsedUtterance utterance2)
        {
            var utteranceWords1 = new HashSet<string>(utterance1.Words);
            var utteranceWords2 = new HashSet<string>(utterance2.Words);

            var intersectionSyntacticWords = utteranceWords1.Intersect(utteranceWords2).ToArray();
            var differenceWords = utteranceWords1.Union(utteranceWords2).Except(intersectionSyntacticWords).Where(w => !InputState.QA.Graph.HasEvidence(w)).ToArray();

            //find best difference word
            string longestWord = null;
            foreach (var word in differenceWords)
            {
                if (longestWord == null || longestWord.Length < word.Length)
                {
                    //TODO this can be better when considering word distribution probability
                    longestWord = word;
                }
            }

            return longestWord;
        }
    }
}