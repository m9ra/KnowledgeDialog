using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Dialog;

namespace WebBackend.Dataset
{
    static class DataWriter_Batch
    {
        private static void writeQuestionDataset()
        {
            var provider = Program.QuestionDialogProvider;
            var sluFactory = new SLUFactory();
            provider.Refresh();

            var dialogCount = 0;
            var turnCount = 0;

            var explanationCount = 0;
            var answerCount = 0;
            var noAnswerCount = 0;
            var parahpraseCount = 0;

            var correctCount = 0;
            var incorrectCount = 0;
            var differentChoise = 0;

            var invalid = 0;
            var unwanted = 0;

            var dialogIndexes = new List<int>();
            var questions = new HashSet<string>();
            for (var i = 0; i < provider.DialogCount; ++i)
            {
                var dialog = provider.GetDialog(i);
                var annotation = dialog.Annotation;

                switch (annotation)
                {
                    case "invalid":
                        invalid += 1;
                        break;
                    case "unwanted_answer":
                        unwanted += 1;
                        break;
                }

                if (annotation == null || annotation == "invalid" || annotation == "unwanted_answer")
                    //we dont want this dialog
                    continue;

                switch (annotation)
                {
                    case "correct_answer":
                        correctCount += 1;
                        break;

                    case "different_choice":
                        differentChoise += 1;
                        incorrectCount += 1;
                        break;

                    case "no_answer":
                        noAnswerCount += 1;
                        break;

                    default:
                        incorrectCount += 1;
                        break;
                }

                questions.Add(dialog.Question);

                dialogCount += 1;
                turnCount += dialog.TurnCount;

                explanationCount += dialog.GetExplanationCount(sluFactory);
                parahpraseCount += dialog.GetParaphraseCount(sluFactory);
                answerCount += dialog.GetAnswerCount();


                dialogIndexes.Add(i);
            }

            Console.WriteLine("Dataset statistics");
            w("Dialog count", dialogCount);
            w("Turn count", turnCount);
            w("Explanation count", explanationCount);
            w("Answer count", answerCount);
            w("Noanswer count", noAnswerCount);
            w("Paraphrase count", parahpraseCount);
            w("Correct count", correctCount);
            w("Incorrect count", incorrectCount);
            w("Different choice", differentChoise);
            w("Invalid count", invalid);
            w("Unwanted count", unwanted);
            w("Question count", questions.Count);
            Console.WriteLine();


            var rnd = new Random(12345);
            Shuffle(rnd, dialogIndexes);
            var testRatio = 0.35;
            var trainRatio = 0.5;

            var testCount = (int)(dialogIndexes.Count * testRatio);
            var trainCount = (int)(dialogIndexes.Count * trainRatio);
            var devCount = dialogIndexes.Count - testRatio - trainRatio;

            var testIndexes = dialogIndexes.Take(testCount).ToArray();
            var trainIndexes = dialogIndexes.Skip(testCount).Take(trainCount).ToArray();
            var devIndexes = dialogIndexes.Skip(testCount + trainCount).ToArray();

            if (testIndexes.Intersect(trainIndexes).Any() || trainIndexes.Intersect(devIndexes).Any() || devIndexes.Intersect(testIndexes).Any())
                throw new NotSupportedException("there is an error when spliting dataset");

            writeQuestionDatasetTo("question_dialogs-test.json", testIndexes, provider);
            writeQuestionDatasetTo("question_dialogs-train.json", trainIndexes, provider);
            writeQuestionDatasetTo("question_dialogs-dev.json", devIndexes, provider);

            Console.ReadLine();
        }

        private static void w(string caption, int value)
        {
            Console.WriteLine("\t" + caption + ": " + value);
        }

        private static void writeQuestionDatasetTo(string targetFilePath, int[] dialogsToWrite, Dataset.QuestionDialogProvider provider)
        {
            var turnCount = 0;
            var writer = new QuestionDatasetWriter(targetFilePath);
            foreach (var dialogIndex in dialogsToWrite)
            {
                var dialog = provider.GetDialog(dialogIndex);
                turnCount += dialog.TurnCount;
                writer.Add(dialog);
            }

            writer.Save();

            Console.WriteLine("Statistics for file " + targetFilePath);
            w("Dialog count", dialogsToWrite.Length);
            w("Turn count", turnCount);
            Console.WriteLine();
        }


        public static void Shuffle<T>(Random rnd, IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
