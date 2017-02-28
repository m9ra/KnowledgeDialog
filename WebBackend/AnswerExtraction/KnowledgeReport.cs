using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Parsing;
using KnowledgeDialog.DataCollection;

using WebBackend.Dataset;

namespace WebBackend.AnswerExtraction
{
    class KnowledgeReport
    {
        /// <summary>
        /// How many distinct questions were asked.
        /// </summary>
        public readonly int QuestionCount;

        /// <summary>
        /// How many questions with at least one hint we have.
        /// </summary>
        public int QuestionWithAnswerHintCount { get { return Questions.Count(); } }

        /// <summary>
        /// How many questions got the correct denotation.
        /// </summary>
        public int QuestionWithCorrectDenotationCount { get { return Questions.Where(q => q.HasCorrectDenotation).Count(); } }
        /// <summary>
        /// How many answer hints we collected.
        /// </summary>
        public int AnswerHintCount { get { return Questions.Select(q => q.CollectedDenotations.Count()).Sum(); } }

        public readonly IEnumerable<QuestionReport> Questions;

        public readonly string StoragePath;

        internal KnowledgeReport(ExtractionKnowledge knowledge, LinkBasedExtractor extractor, QuestionCollection questions)
        {
            StoragePath = knowledge.StoragePath;
            QuestionCount = knowledge.Questions.Count();
            var reports = new List<QuestionReport>();
            foreach (var question in knowledge.Questions)
            {
                if (!question.AnswerHints.Any())
                    continue;

                var answerId = FreebaseDbProvider.GetId(questions.GetAnswerMid(question.Utterance.OriginalSentence));
                var report = new QuestionReport(question, answerId, extractor);
                reports.Add(report);
            }

            Questions = reports.OrderByDescending(r => r.CollectedDenotations.Count());
        }
    }

    class QuestionReport
    {
        public readonly LinkedUtterance Question;

        public readonly FreebaseEntry AnswerLabel;

        public readonly IEnumerable<Tuple<LinkedUtterance, EntityInfo, bool>> CollectedDenotations;

        public readonly bool HasCorrectDenotation;

        internal QuestionReport(QuestionInfo info, string answerId, LinkBasedExtractor extractor)
        {
            var linker = extractor.Linker;
            Question = linker.LinkUtterance(info.Utterance.OriginalSentence);

            AnswerLabel = extractor.Db.GetEntryFromId(answerId);
            var denotations = new List<Tuple<LinkedUtterance, EntityInfo, bool>>();

            foreach (var answerHint in info.AnswerHints)
            {
                var linkedHint = linker.LinkUtterance(answerHint.OriginalSentence, Question.Entities);
                var denotation = extractor.ExtractAnswerEntity(info.Utterance.OriginalSentence, answerHint.OriginalSentence).FirstOrDefault();

                var item = Tuple.Create(linkedHint, denotation, answerId == FreebaseDbProvider.GetId(denotation.Mid));
                denotations.Add(item);
            }

            CollectedDenotations = denotations;

            var denotationCounts = from denotation in denotations
                                   group denotation by FreebaseDbProvider.GetId(denotation.Item2.Mid)
                                       into grouped
                                       select Tuple.Create(grouped.Key, grouped.Count());

            var maxDenotation = denotationCounts.OrderByDescending(t => t.Item2).FirstOrDefault();
            if (maxDenotation != null && AnswerLabel != null)
                HasCorrectDenotation = maxDenotation.Item1 == AnswerLabel.Id;
        }
    }
}
