using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog.Parsing;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    struct AnswerExtractionResult
    {
        public readonly double TotalPrecision;

        public readonly double ExtractionPrecision;

        internal AnswerExtractionResult(double totalPrecision, double extractionPrecision)
        {
            TotalPrecision = totalPrecision;
            ExtractionPrecision = extractionPrecision;
        }

        public override string ToString()
        {
            return string.Format("Total: {0:0.000} | Extraction: {1:0.000}", TotalPrecision, ExtractionPrecision);
        }
    }

    static class SigdialPaperExperiments_Batch
    {
        internal static void EdgeMaximizationLinking()
        {
            var testData = Configuration.GetQuestionDialogsDev();

            foreach (var nbest in new[] { 5 })
            {
                var linker = new GraphDisambiguatedLinker(Configuration.Db, "./verbs.lex");
                linker.Nbest = nbest;

                var precision = evaluateLinker(testData, linker);
                Console.WriteLine("Precision@{1}: {0:0.000}", precision, nbest);
            }
        }

        internal static void PopularityMaximizationLinking()
        {
            var testData = Configuration.GetQuestionDialogsTest();

            foreach (var nbest in new[] { 1, 2, 5 })
            {
                var linker = new PopularityMaximizationLinker(Configuration.Db, "./verbs.lex");
                linker.Nbest = nbest;

                var precision = evaluateLinker(testData, linker);
                Console.WriteLine("Precision@{1}: {0:0.000}", precision, nbest);
            }
        }

        internal static void BasicCancelation()
        {
            var testData = Configuration.GetQuestionDialogsTest();

            var linker = Configuration.Linker;
            var linkedExtractor = new LinkBasedExtractor(linker, Configuration.Db);
            linkedExtractor.DisableEnumerationDetection = true;

            var result = evaluateExtractor(testData, linker, (d) => linkedExtractor.ExtractAnswerEntity(d));
            Console.WriteLine(result);
        }

        internal static void BasicCancelation_WithEnumDetection()
        {
            var testData = Configuration.GetQuestionDialogsTest();

            var linker = Configuration.Linker;
            var linkedExtractor = new LinkBasedExtractor(linker, Configuration.Db);
            //enumeration detection is not disabled here

            var result = evaluateExtractor(testData, linker, (d) => linkedExtractor.ExtractAnswerEntity(d));
            Console.WriteLine(result);
        }

        internal static void BasicCancelation_WithEnumAndNgrams()
        {
            var testData = Configuration.GetQuestionDialogsTest();
            var trainDataset = Configuration.GetQuestionDialogsTrain();

            var linker = Configuration.Linker;
            var linkedExtractor = new LinkBasedExtractor(linker, Configuration.Db);
            //enumeration detection is not disabled here
            linkedExtractor.UseNgramOrdering = true;

            //train the extractor
            foreach (var dialog in trainDataset.Dialogs)
            {
                if (dialog.HasCorrectAnswer)
                {
                    Console.WriteLine(dialog.Question);
                    linkedExtractor.Train(dialog);
                }
            }

            var result = evaluateExtractor(testData, linker, (d) => linkedExtractor.ExtractAnswerEntity(d));
            Console.WriteLine(result);
        }

        internal static AnswerExtractionResult evaluateExtractor(QuestionDialogDatasetReader dataset, ILinker linker, Func<QuestionDialog, IEnumerable<EntityInfo>> extractor)
        {
            var totalDialogCount = 0;
            var correctLinkCount = 0;
            var correctExtractionCount = 0;
            foreach (var dialog in dataset.Dialogs)
            {
                if (!dialog.HasCorrectAnswer)
                    continue;

                totalDialogCount += 1;

                var linkedQuestion = linker.LinkUtterance(dialog.Question);
                //Console.WriteLine(linkedQuestion);

                var contextEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();

                var answerPhrase = getAnswerPhrase(dialog);
                var linkedAnswer = linker.LinkUtterance(answerPhrase, contextEntities);

                var answerPhraseEntities = linkedAnswer == null ? new EntityInfo[0] : linkedAnswer.Entities;
                var utteranceEntities = linkedAnswer.Parts.SelectMany(p => p.Entities).ToArray();
                var isLinkingCorrect = utteranceEntities.Select(e => e.Mid).Contains(dialog.AnswerMid);

                if (!isLinkingCorrect)
                    continue;

                ++correctLinkCount;

                var denotation = extractor(dialog).FirstOrDefault();
                var isExtractionCorrect = denotation != null && denotation.Mid == dialog.AnswerMid;
                if (isExtractionCorrect)
                    ++correctExtractionCount;
                else
                {
                    Console.WriteLine(linkedQuestion);
                    Console.WriteLine(linkedAnswer);
                    var labelEntry = Configuration.Db.GetEntryFromMid(dialog.AnswerMid);
                    var denotationEntry = Configuration.Db.GetEntryFromMid(denotation.Mid);
                    Console.WriteLine("Label: {0}", labelEntry);
                    Console.WriteLine("Denotation: {0}", denotationEntry);

                    Console.WriteLine("\n\n");
                }
            }

            return new AnswerExtractionResult(1.0 * correctExtractionCount / totalDialogCount, 1.0 * correctExtractionCount / correctLinkCount);
        }

        internal static double evaluateLinker(QuestionDialogDatasetReader dataset, ILinker linker)
        {
            var totalDialogCount = 0;
            var correctLinkCount = 0;
            foreach (var dialog in dataset.Dialogs)
            {
                if (!dialog.HasCorrectAnswer)
                    continue;

                totalDialogCount += 1;

                var linkedQuestion = linker.LinkUtterance(dialog.Question);

                var contextEntities = linkedQuestion.Parts.SelectMany(p => p.Entities.Take(1)).ToArray();

                var answerPhrase = getAnswerPhrase(dialog);
                var linkedAnswer = linker.LinkUtterance(answerPhrase, contextEntities);

                var answerPhraseEntities = linkedAnswer == null ? new EntityInfo[0] : linkedAnswer.Entities;
                var isLinkingCorrect = answerPhraseEntities.Select(e => e.Mid).Contains(dialog.AnswerMid);

                if (isLinkingCorrect)
                    ++correctLinkCount;
            }

            return 1.0 * correctLinkCount / totalDialogCount;
        }


        private static string getAnswerPhrase(QuestionDialog dialog)
        {
            var answerTurn = dialog.AnswerTurns.LastOrDefault();
            if (answerTurn == null)
                return null;

            var text = answerTurn.InputChat;
            return text;
        }

    }
}
