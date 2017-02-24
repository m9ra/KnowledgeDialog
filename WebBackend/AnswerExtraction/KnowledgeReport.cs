using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KnowledgeDialog.Knowledge;
using KnowledgeDialog.Dialog.Parsing;

namespace WebBackend.AnswerExtraction
{
    class KnowledgeReport
    {
        public readonly IEnumerable<QuestionReport> Questions;

        public readonly string StoragePath;

        internal KnowledgeReport(ExtractionKnowledge knowledge, LinkBasedExtractor extractor)
        {
            StoragePath = knowledge.StoragePath;
            var reports = new List<QuestionReport>();
            foreach (var question in knowledge.Questions)
            {
                if (!question.AnswerHints.Any())
                    continue;

                var report = new QuestionReport(question, extractor);
                reports.Add(report);
            }

            Questions = reports.OrderByDescending(r => r.CollectedDenotations.Count());
        }
    }

    class QuestionReport
    {
        public readonly LinkedUtterance Question;

        public readonly EntityInfo DesiredEntity;

        public readonly IEnumerable<Tuple<LinkedUtterance, EntityInfo>> CollectedDenotations;

        internal QuestionReport(QuestionInfo info, LinkBasedExtractor extractor)
        {
            var linker = extractor.Linker;
            Question = linker.LinkUtterance(info.Utterance.OriginalSentence);

            var denotations = new List<Tuple<LinkedUtterance, EntityInfo>>();

            foreach (var answerHint in info.AnswerHints)
            {
                var linkedHint = linker.LinkUtterance(answerHint.OriginalSentence, Question.Entities);
                var denotation = extractor.ExtractAnswerEntity(info.Utterance.OriginalSentence, answerHint.OriginalSentence).FirstOrDefault();

                var item = Tuple.Create(linkedHint, denotation);
                denotations.Add(item);
            }

            CollectedDenotations = denotations;
        }
    }
}
