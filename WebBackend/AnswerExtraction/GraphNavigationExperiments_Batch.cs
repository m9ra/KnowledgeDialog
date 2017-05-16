using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.AnswerExtraction
{
    static class GraphNavigationExperiments_Batch
    {
        public static void ListUnknownEntityWordsQDD()
        {
            var train = Configuration.GetQuestionDialogsTrain();
            var linker = Configuration.Linker;
            var aliasModel = new Models.IdfAliasDetectionModel();
            foreach (var dialog in train.Dialogs)
            {
                var answerHintTurn = dialog.AnswerTurns.LastOrDefault();
                if (answerHintTurn == null)
                    continue;

                var answerHint = answerHintTurn.InputChat;
                var linkedQuestion = linker.LinkUtterance(dialog.Question);
                var linkedAnswerHint = linker.LinkUtterance(answerHint, linkedQuestion.Entities);

                if (linkedAnswerHint == null)
                    continue;

                aliasModel.Accept(linkedAnswerHint);
            }

            foreach (var phraseCount in aliasModel.NonEntityPhrasesInverseCounts.OrderBy(p => p.Value))
            {
                Console.WriteLine("{0}: {1}", phraseCount.Key, phraseCount.Value);
            }
        }
    }
}
