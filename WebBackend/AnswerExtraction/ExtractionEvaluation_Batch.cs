using System;
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

        private static void extractAnswer(string utterance, ILinker linker, LinkBasedExtractor extractor, QuestionDialogDatasetReader dataset)
        {
            var utteranceWords = getCleanWords(utterance).Distinct();

            QuestionDialog dialogToAnswer = null;
            foreach (var dialog in dataset.Dialogs)
            {
                if (!dialog.HasCorrectAnswer)
                    continue;

                var questionWords = getCleanWords(dialog.Question);
                if (utteranceWords.Except(questionWords).Count() == 0)
                    dialogToAnswer = dialog;

                foreach (var turn in dialog.AnswerTurns)
                {
                    var words = getCleanWords(turn.InputChat);
                    if (utteranceWords.Except(words).Count() == 0)
                    {
                        dialogToAnswer = dialog;
                        break;
                    }
                }

                if (dialogToAnswer != null)
                    break;
            }

            Console.WriteLine(dialogToAnswer.Question);

            var answerPhrase = getAnswerPhrase(dialogToAnswer);
            var linkedQuestion = linker.LinkUtterance(dialogToAnswer.Question);
            Console.WriteLine(linkedQuestion);

            var contextEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();
            var result = linker.LinkUtterance(answerPhrase, contextEntities);
            Console.WriteLine(result);

            var answers = extractor.ExtractAnswerEntity(dialogToAnswer);
            foreach (var answer in answers)
            {
                Console.WriteLine(answer);
            }
        }


        internal static void RunLinkedAnswerExtractionExperiment()
        {
            var trainDataset = Configuration.GetQuestionDialogsTrain();
            var devDataset = Configuration.GetQuestionDialogsTrain();

            var db = Configuration.GetFreebaseDbProvider();
            db.LoadIndex();

            var linker = getTrainDataLinker(db);
            
            var linkedExtractor = new LinkBasedExtractor(linker);
            foreach (var dialog in trainDataset.Dialogs)
            {
                if (dialog.HasCorrectAnswer)
                    linkedExtractor.Train(dialog);
            }

            var desiredEntityInfoPrintingEnabled = false;


            extractAnswer("Which anime series did toei animation produce", linker, linkedExtractor, devDataset);

            var correctCount = 0;
            var totalCount = 0;
            foreach (var dialog in trainDataset.Dialogs)
            {
                if (!dialog.HasCorrectAnswer)
                    continue;

                    var linkedQuestion = linker.LinkUtterance(dialog.Question);
                    var contextEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();
                    Console.WriteLine(linkedQuestion);

                    var answerPhrase = getAnswerPhrase(dialog);
                    var linkedAnswer = linker.LinkUtterance(answerPhrase, contextEntities);
                    Console.WriteLine(linkedAnswer);

                    if (desiredEntityInfoPrintingEnabled)
                    {
                        var correctAnswer = db.GetLabel(dialog.AnswerMid);
                        var answerInBounds = db.GetInBounds(dialog.AnswerMid);
                        var answerOutBounds = db.GetOutBounds(dialog.AnswerMid);
                        Console.WriteLine("\tdesired: {0}({1})[{2}/{3}]", correctAnswer, db.GetFreebaseId(dialog.AnswerMid), answerInBounds, answerOutBounds);
                    }
                
                    var answerEntities = linkedExtractor.ExtractAnswerEntity(dialog);
                    var isCorrect = answerEntities.Select(e => e.Mid).Take(1).Contains(dialog.AnswerMid);
                    if (isCorrect)
                    {
                        Console.WriteLine("\tOK");
                        ++correctCount;
                    }
                    else
                    {
                        Console.WriteLine("\tNO");
                        foreach (var entity in answerEntities)
                        {
                            Console.WriteLine("\t\t{0}[{1}/{2}]", entity, entity.InBounds, entity.OutBounds);
                        }
                    }

                    ++totalCount;
                    Console.WriteLine("\tprecision {0:00.00}%", 100.0 * correctCount / totalCount);
                    Console.WriteLine();
            }

            Console.WriteLine(linkedExtractor.TotalEntityCount + " answer entities");
            Console.WriteLine("END");
            Console.ReadKey();
        }

        private static ILinker getTrainDataLinker(FreebaseDbProvider db)
        {
            var coreLinker = new GraphDisambiguatedLinker(db, "./verbs.lex");
            var linker = new DiskCachedLinker("../train_data.link", 1, (u) => coreLinker.LinkUtterance(u));

            return linker;
        }

        private static void linkOnAnswerHint(string utterance, GraphDisambiguatedLinker linker, QuestionDialogDatasetReader dataset)
        {
            var utteranceWords = utterance.ToLowerInvariant().Split(' ').Distinct();

            QuestionDialog dialogToLink = null;
            foreach (var dialog in dataset.Dialogs)
            {
                if (!dialog.HasCorrectAnswer)
                    continue;

                var questionWords = dialog.Question.ToLowerInvariant().Split(' ');
                if (utteranceWords.Except(questionWords).Count() == 0)
                    dialogToLink = dialog;

                foreach (var turn in dialog.AnswerTurns)
                {
                    var words = turn.InputChat.ToLowerInvariant().Split(' ').Distinct();
                    if (utteranceWords.Except(words).Count() == 0)
                    {
                        dialogToLink = dialog;
                        break;
                    }
                }

                if (dialogToLink != null)
                    break;
            }

            Console.WriteLine(dialogToLink.Question);

            var answerPhrase = getAnswerPhrase(dialogToLink);
            var linkedQuestion = linker.LinkUtterance(dialogToLink.Question);
            Console.WriteLine(linkedQuestion);

            var contextEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();
            var result = linker.LinkUtterance(answerPhrase, contextEntities);
            Console.WriteLine(result);
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

            //linkOnAnswerHint("What is karim khalili known for being", linker, devDataset);
            //linkOnAnswerHint("The film was in Portuguese and dubbed in English", linker, devDataset);
            //var result = linker.LinkUtterance("derek hagan");
            //var result = linker.LinkUtterance("The Apprentice", 5);
            //var result = linker.LinkUtterance("Scooter Libby wrote a novel called The Apprentice", 5);
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
                    var linkedQuestion = linker.LinkUtterance(dialog.Question);
                    var contextEntities = linkedQuestion.Parts.SelectMany(p => p.Entities).ToArray();
                    Console.WriteLine(linkedQuestion);

                    var answerPhrase = getAnswerPhrase(dialog);
                    var linkedUtterance = linker.LinkUtterance(answerPhrase, contextEntities);
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
        }

        private static IEnumerable<string> getCleanWords(string utterance)
        {
            return utterance.ToLowerInvariant().Replace(".", " ").Replace(",", " ").Split(' ');
        }
    }
}
