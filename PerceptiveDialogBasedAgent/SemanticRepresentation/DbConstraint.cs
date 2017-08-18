using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.SemanticRepresentation
{
    class DbConstraint
    {
        internal IEnumerable<ConstraintEntry> AnswerConstraints => _entries.Where(e => e.Subject != null);

        internal IEnumerable<ConstraintEntry> SubjectConstraints => _entries.Where(e => e.Answer != null);

        private readonly ConstraintEntry[] _entries;

        internal readonly string NameConstraint;

        internal DbConstraint(params ConstraintEntry[] entries)
        {
            _entries = entries.ToArray();
        }

        private DbConstraint(string nameConstraint)
        {
            NameConstraint = nameConstraint;
            _entries = new ConstraintEntry[0];
        }

        internal DbConstraint Join(DbConstraint dbConstraint)
        {
            throw new NotImplementedException();
        }

        internal static DbConstraint Entity(string name)
        {
            return new DbConstraint(name);
        }

        internal DbConstraint ExtendByAnswer(string question, DbConstraint answer)
        {
            var newEntry = new ConstraintEntry(null, question, answer);
            return new DbConstraint(_entries.Concat(new[] { newEntry }).ToArray());
        }

        public override string ToString()
        {
            if (NameConstraint != null)
                return "[DbConstraint]" + NameConstraint + " " + string.Join(",", _entries.Select(e => e.ToString()));

            return "[DbConstraint]" + string.Join(",", _entries.Select(e => e.ToString()));
        }
    }

    class ConstraintEntry
    {
        internal readonly DbConstraint Subject;

        internal readonly string Question;

        internal readonly DbConstraint Answer;

        internal ConstraintEntry(DbConstraint subject, string question, DbConstraint answer)
        {
            Subject = subject;
            Question = question;
            Answer = answer;
        }

        internal static ConstraintEntry AnswerWhere(string subject, string question)
        {
            return new ConstraintEntry(DbConstraint.Entity(subject), question, null);
        }

        public override string ToString()
        {
            return string.Format("({0}|{1}|{2})", Subject, Question, Answer);
        }
    }
}
