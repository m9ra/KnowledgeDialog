using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    /// <summary>
    /// Batch for evaluation of answer extraction.
    /// </summary>
    class ExtractionEvaluation_Batch
    {
        internal static void RunEvaluation()
        {
            var trainDataset = new QuestionDialogDatasetReader("question_dialogs-train.json");
            var devDataset = new QuestionDialogDatasetReader("question_dialogs-dev.json");

            var extractor = new AnswerExtraction.Extractor(@"C:\REPOSITORIES\lucene_freebase_v1_index");
            extractor.LoadIndex();
            foreach (var dialog in trainDataset.Dialogs)
            {
                if (dialog.HasCorrectAnswer)
                    extractor.Train(getAnswerHintNgrams(dialog, extractor), dialog.AnswerMid);
            }


            while (true)
            {
                var nbest = 2;
                var correctCount = 0;
                var correctNCount = 0;
                var totalCount = 0;

                foreach (var dialog in devDataset.Dialogs)
                {
                    if (dialog.HasCorrectAnswer)
                    {
                        var hints = getAnswerHintNgrams(dialog, extractor).ToArray();
                        var context = getContextNgrams(dialog).ToArray();
                        var scores = extractor.Score(hints, context);
                        var bestId = scores.Select(e => e.Mid).FirstOrDefault();
                        if (scores.Take(nbest).Select(e => e.Mid).Contains(dialog.AnswerMid))
                            ++correctNCount;

                        if (bestId == dialog.AnswerMid)
                        {
                            Console.WriteLine("OK " + bestId);
                            ++correctCount;
                        }
                        else
                        {
                            Console.WriteLine("NO " + bestId);
                            Console.WriteLine("\t " + getAnswerPhrase(dialog, extractor));
                            var correctAnswer = extractor.GetLabel(dialog.AnswerMid);
                            Console.WriteLine("\t desired: " + correctAnswer);
                            foreach (var entity in scores.Take(5))
                            {
                                Console.WriteLine("\t {0:0.00}: {1}", entity.Name, entity.Score);
                            }

                        }

                        ++totalCount;

                        Console.WriteLine("\tprecision {0:00.00}% ({2}best precision {1:00.00}%)", 100.0 * correctCount / totalCount, 100.0 * correctNCount / totalCount, nbest);
                    }
                }
            }
            return;
        }

        private static string getNamesRepresentation(string freebaseId, FreebaseLoader loader)
        {
            var names = loader.GetNames(freebaseId);
            return string.Join(",", names.ToArray()).Replace("\"", "");
        }

        private static string getAnswerPhrase(QuestionDialog dialog, AnswerExtraction.Extractor extractor)
        {
            var answerTurn = dialog.AnswerTurns.LastOrDefault();
            if (answerTurn == null)
                return null;

            var text = answerTurn.InputChat;
            return text;
            //  text = text.Split('.').First();
            //  return text;
            var parts = text.Split(new[] { " is " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0];

            return parts[1];
            string bestPart = null;
            var bestScore = double.NegativeInfinity;
            var contextNgrams = getContextNgrams(dialog).ToArray();
            var context = extractor.RawScores(contextNgrams);
            foreach (var part in parts)
            {
                //we are searching for part with lowest match to context
                var partScore = getScore(part, context, extractor);
                if (partScore > bestScore)
                {
                    bestPart = part;
                    bestScore = partScore;
                }
            }

            return bestPart;
        }

        private static double getScore(string part, Dictionary<string, EntityInfo> context, AnswerExtraction.Extractor extractor)
        {
            var ngrams = getNgrams(part);
            var ngramsArr = ngrams.ToArray();
            var scores = extractor.RawScores(ngramsArr);
            var totalScore = 0.0;
            foreach (var score in scores)
            {
                EntityInfo entity;
                if (context.TryGetValue(score.Key, out entity))
                    totalScore += entity.Score;
            }
            return totalScore;
        }

        private static IEnumerable<string> getContextNgrams(QuestionDialog dialog)
        {
            return getNgrams(dialog.Question);
        }

        private static IEnumerable<string> getAnswerHintNgrams(QuestionDialog dialog, AnswerExtraction.Extractor extractor)
        {
            var turnIndex = 0;
            var ngrams = new List<string>();
            foreach (var answerTurn in dialog.AnswerTurns)
            {
                var isOddTurn = turnIndex % 2 == 1;
                if (isOddTurn)
                    ngrams.AddRange(getNgrams(answerTurn.InputChat));

                turnIndex += 1;
            }

            //return ngrams;
            var answerText = getAnswerPhrase(dialog, extractor);
            return getNgrams(answerText);
        }

        private static IEnumerable<string> getNgrams(string text)
        {
            if (text == null)
                yield break;

            text = text.ToLowerInvariant().Replace(".", " ").Replace(",", " ").Replace("'", " ").Replace("  ", " ").Replace("  ", " ");
            var words = text.Split(' ').Select(w => w.Trim()).Where(w => w.Length > 0).ToArray();
            for (var i = 0; i < words.Length; ++i)
            {
                var j = i + 1;
                var k = i + 2;
                var l = i + 3;

                var wordI = words[i];
                var wordK = k < words.Length ? words[k] : "";

                yield return wordI;
                if (j < words.Length)
                    yield return wordI + " " + words[j];

                if (k < words.Length)
                    yield return wordI + " " + words[j] + " " + words[k];
                /*                
                                             if (l < words.Length)
                                                 yield return wordI + " " + words[j] + " " + words[k] +" "+ words[l];
                */
            }
        }
    }
}
