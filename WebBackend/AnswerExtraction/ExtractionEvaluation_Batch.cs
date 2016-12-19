﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Dataset;

using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.AnswerExtraction
{
    /// <summary>
    /// Batch for evaluation of answer extraction.
    /// </summary>
    class ExtractionEvaluation_Batch
    {
        internal static void RunLinkedAnswerExtractionExperiment()
        {
            var ENTITY_HYP_COUNT = 1;

            var trainDataset = Configuration.GetQuestionDialogsTrain();
            var devDataset = Configuration.GetQuestionDialogsDev();

            var simpleQuestions = Configuration.GetSimpleQuestionsDump();
            var db = Configuration.GetFreebaseDbProvider();
            db.LoadIndex();

            var linker = new GraphDisambiguatedLinker(db, "./verbs.lex");

            var utterancesToDisambiguate = new List<string>();
            var applicableDialogs = devDataset.Dialogs.Where(d => d.HasCorrectAnswer).ToArray();

            utterancesToDisambiguate.AddRange(applicableDialogs.Select(d => d.Question));
            utterancesToDisambiguate.AddRange(applicableDialogs.Select(d => getAnswerPhrase(d)));

            //linker.RegisterDisambiguationEntities(utterancesToDisambiguate);
            //linker.LoadDisambiguationEntities(simpleQuestions);

            var linkedExtractor = new LinkBasedExtractor(linker, ENTITY_HYP_COUNT);

            var correctCount = 0;
            var totalCount = 0;
            foreach (var dialog in devDataset.Dialogs)
            {
                if (dialog.HasCorrectAnswer)
                {
                    var answerPhrase = getAnswerPhrase(dialog);

                    var linkedQuestion = linker.LinkUtterance(dialog.Question, ENTITY_HYP_COUNT).First();
                    var linkedAnswer = linker.LinkUtterance(getAnswerPhrase(dialog), ENTITY_HYP_COUNT).First();

                    Console.WriteLine(linkedQuestion);
                    Console.WriteLine(linkedAnswer);

                    var correctAnswer = db.GetLabel(dialog.AnswerMid);
                    var answerInBounds = db.GetInBounds(dialog.AnswerMid);
                    var answerOutBounds = db.GetOutBounds(dialog.AnswerMid);
                    Console.WriteLine("\tdesired: {0}({1})[{2}/{3}]", correctAnswer, db.GetFreebaseId(dialog.AnswerMid), answerInBounds, answerOutBounds);


                    var answerEntities = linkedExtractor.ExtractAnswerEntity(dialog);
                    var isCorrect = answerEntities.Select(e => e.Mid).Contains(dialog.AnswerMid);
                    if (isCorrect)
                    {
                        Console.WriteLine("\tOK");
                        ++correctCount;
                    }
                    else
                    {
                        Console.WriteLine("\tNO");
                    }

                    ++totalCount;
                    Console.WriteLine("\tprecision {0:00.00}%", 100.0 * correctCount / totalCount);
                    Console.WriteLine();
                }
            }

            Console.WriteLine(linkedExtractor.TotalEntityCount + " answer entities");
            Console.WriteLine("END");
            Console.ReadKey();
        }

        internal static void RunLinkingExperiment()
        {
            var devDataset = Configuration.GetQuestionDialogsDev();
            var db = Configuration.GetFreebaseDbProvider();
            db.LoadIndex();

            var simpleQuestions = Configuration.GetSimpleQuestionsDump();
            var linker = new GraphDisambiguatedLinker(db, "./verbs.lex", useGraphDisambiguation: true);

            var utterancesToDisambiguate = new List<string>();
            var applicableDialogs = devDataset.Dialogs.Where(d => d.HasCorrectAnswer).ToArray();

            utterancesToDisambiguate.AddRange(applicableDialogs.Select(d => d.Question));
            utterancesToDisambiguate.AddRange(applicableDialogs.Select(d => getAnswerPhrase(d)));


            var result = linker.LinkUtterance("Scooter Libby wrote a novel called The Apprentice", 5);
            //var result = linker.LinkUtterance("It can come from a cow or a goat", 5);
            //var result = linker.LinkUtterance("Dr Who");
            //var result = linker.LinkUtterance("englis language");

            var correctCount = 0;
            var totalCount = 0;
            var totalEntityCount = 0;
            foreach (var dialog in devDataset.Dialogs)
            {
                if (dialog.HasCorrectAnswer)
                {
                    var answerPhrase = getAnswerPhrase(dialog);
                    var linkedUtterance = linker.LinkUtterance(answerPhrase, 5).First();

                    Console.WriteLine(linkedUtterance);

                    var correctAnswer = db.GetLabel(dialog.AnswerMid);
                    var answerInBounds = db.GetInBounds(dialog.AnswerMid);
                    var answerOutBounds = db.GetOutBounds(dialog.AnswerMid);
                    Console.WriteLine("\tdesired: {0}({1})[{2}/{3}]", correctAnswer, db.GetFreebaseId(dialog.AnswerMid), answerInBounds, answerOutBounds);

                    var utteranceEntities = linkedUtterance.Parts.SelectMany(p => p.Entities).ToArray();
                    totalEntityCount += utteranceEntities.Length;

                    var isCorrect = utteranceEntities.Select(e => e.Mid).Contains(dialog.AnswerMid);
                    if (isCorrect)
                    {
                        Console.WriteLine("\tOK");
                        ++correctCount;
                    }
                    else
                    {
                        Console.WriteLine("\tNO");
                        foreach (var part in linkedUtterance.Parts)
                        {
                            if (!part.Entities.Any())
                                continue;

                            Console.WriteLine("\t\t" + part + ": ");
                            foreach (var entity in part.Entities)
                            {
                                var aliases = db.GetAliases(entity.Mid).Take(3);
                                var description = db.GetDescription(entity.Mid);

                                Console.WriteLine("\t\t\t{0}({1})[{2}/{3}]  :{4}", entity.Label, db.GetFreebaseId(entity.Mid), entity.InBounds, entity.OutBounds, string.Join(" | ", aliases));
                                if (description.Length > 50)
                                    description = description.Substring(0, 50);
                                Console.WriteLine("\t\t\t\t" + description);
                            }
                            Console.WriteLine();
                        }
                    }

                    ++totalCount;
                    Console.WriteLine("\tprecision {0:00.00}%", 100.0 * correctCount / totalCount);
                    Console.WriteLine();
                }
            }
            Console.WriteLine(totalEntityCount + " answer entities");
            Console.WriteLine("END");
            Console.ReadKey();
        }

        internal static void RunAnswerExtractionEvaluation()
        {
            var trainDataset = Configuration.GetQuestionDialogsTrain();
            var devDataset = Configuration.GetQuestionDialogsDev();

            var db = Configuration.GetFreebaseDbProvider();
            db.LoadIndex();

            var linker = new GraphDisambiguatedLinker(db, "./verbs.lex", useGraphDisambiguation: true);
            foreach (var dialog in trainDataset.Dialogs)
            {
                if (dialog.HasCorrectAnswer)
                    linker.Train(getAnswerHintNgrams(dialog, linker), dialog.AnswerMid);
            }

            var nbest = 2;
            var correctCount = 0;
            var correctNCount = 0;
            var totalCount = 0;

            foreach (var dialog in devDataset.Dialogs)
            {
                if (dialog.HasCorrectAnswer)
                {
                    var hints = getAnswerHintNgrams(dialog, linker).ToArray();
                    var context = getContextNgrams(dialog).ToArray();
                    var scores = linker.Score(hints, context);
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
                        Console.WriteLine("\t " + getAnswerPhrase(dialog));
                        var correctAnswer = db.GetLabel(dialog.AnswerMid);
                        Console.WriteLine("\t desired: {0}({1})", correctAnswer, db.GetFreebaseId(dialog.AnswerMid));
                        foreach (var entity in scores.Take(5))
                        {
                            Console.WriteLine("\t {0}: {1:0.00}({2})({3})", entity.BestAliasMatch, entity.Score, db.GetFreebaseId(entity.Mid), db.GetLabel(entity.Mid));
                        }

                    }

                    ++totalCount;

                    Console.WriteLine("\tprecision {0:00.00}% ({2}best precision {1:00.00}%)", 100.0 * correctCount / totalCount, 100.0 * correctNCount / totalCount, nbest);
                }

            }

            Console.WriteLine("END");
            Console.ReadKey();

        }

        private static string getNamesRepresentation(string freebaseId, FreebaseLoader loader)
        {
            var names = loader.GetNames(freebaseId);
            return string.Join(",", names.ToArray()).Replace("\"", "");
        }

        private static string getAnswerPhrase(QuestionDialog dialog)
        {
            var answerTurn = dialog.AnswerTurns.LastOrDefault();
            if (answerTurn == null)
                return null;

            var text = answerTurn.InputChat;
            return text;
            /*  //  text = text.Split('.').First();
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

              return bestPart;*/
        }


        private static IEnumerable<string> getContextNgrams(QuestionDialog dialog)
        {
            return getNgrams(dialog.Question);
        }

        private static IEnumerable<string> getAnswerHintNgrams(QuestionDialog dialog, UtteranceLinker linker)
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
            var answerText = getAnswerPhrase(dialog);
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
